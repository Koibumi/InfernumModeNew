using CalamityMod;
using CalamityMod.NPCs.ExoMechs.Artemis;
using CalamityMod.Projectiles.BaseProjectiles;
using CalamityMod.Skies;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Draedon.ArtemisAndApollo
{
    public class ArtemisSpinLaser : BaseLaserbeamProjectile
    {
        public int OwnerIndex
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        public const int LaserLifetime = 90;
        public override float MaxScale => 1f;
        public override float MaxLaserLength => 3600f;
        public override float Lifetime => LaserLifetime;
        public override Color LaserOverlayColor => new(250, 180, 100, 100);
        public override Color LightCastColor => Color.White;
        public override Texture2D LaserBeginTexture => Utilities.ProjTexture(Projectile.type);
        public override Texture2D LaserMiddleTexture => ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Lasers/AresLaserBeamMiddle").Value;
        public override Texture2D LaserEndTexture => ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Lasers/AresLaserBeamEnd").Value;
        public override string Texture => "CalamityMod/Projectiles/Boss/AresLaserBeamStart";

        // Dude
        // Dude
        // Dude
        // You are going to Ohio
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Ohio Beam");
            Main.projFrames[Projectile.type] = 5;
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.hostile = true;
            Projectile.alpha = 255;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 600;
            Projectile.Calamity().canBreakPlayerDefense = true;
            CooldownSlot = 1;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Projectile.localAI[0]);
            writer.Write(Projectile.localAI[1]);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Projectile.localAI[0] = reader.ReadSingle();
            Projectile.localAI[1] = reader.ReadSingle();
        }

        public override void AttachToSomething()
        {
            if (Main.npc[OwnerIndex].active && Main.npc[OwnerIndex].type == ModContent.NPCType<Artemis>())
            {
                Vector2 fireFrom = Main.npc[OwnerIndex].Center + Vector2.UnitY * Main.npc[OwnerIndex].gfxOffY;
                fireFrom += Projectile.velocity.SafeNormalize(Vector2.UnitY) * 78f;
                Projectile.Center = fireFrom;
            }

            // Die of the owner is invalid in some way.
            else
            {
                Projectile.Kill();
                return;
            }

            bool notUsingReleventAttack = Main.npc[OwnerIndex].ai[0] != (int)ApolloBehaviorOverride.TwinsAttackType.SpecialAttack_LaserRayScarletBursts;
            if (Main.npc[OwnerIndex].Opacity <= 0f || notUsingReleventAttack)
            {
                Projectile.Kill();
                return;
            }

            // Periodically create lightning bolts in the sky.
            int lightningBoltCreateRate = ExoMechManagement.CurrentTwinsPhase >= 6 ? 3 : 6;
            if (Main.netMode != NetmodeID.Server && Time % lightningBoltCreateRate == lightningBoltCreateRate - 1f)
                ExoMechsSky.CreateLightningBolt(6);

            Time = Main.npc[OwnerIndex].ai[1];
        }

        public override float DetermineLaserLength()
        {
            float[] sampledLengths = new float[10];
            Collision.LaserScan(Projectile.Center, Projectile.velocity, Projectile.width * Projectile.scale, MaxLaserLength, sampledLengths);

            float newLaserLength = sampledLengths.Average();

            // Fire laser through walls at max length if target is behind tiles.
            if (!Collision.CanHitLine(Main.npc[OwnerIndex].Center, 1, 1, Main.player[Main.npc[OwnerIndex].target].Center, 1, 1))
                newLaserLength = MaxLaserLength;

            return newLaserLength;
        }

        public override void UpdateLaserMotion()
        {
            Projectile.rotation = Main.npc[OwnerIndex].rotation;
            Projectile.velocity = (Projectile.rotation - MathHelper.PiOver2).ToRotationVector2();
        }

        public override void PostAI()
        {
            // Determine frames.
            Projectile.frameCounter++;
            if (Projectile.frameCounter % 5f == 0f)
                Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Projectile.type];
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // This should never happen, but just in case.
            if (Projectile.velocity == Vector2.Zero)
                return false;

            Color beamColor = LaserOverlayColor;
            Rectangle startFrameArea = LaserBeginTexture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
            Rectangle middleFrameArea = LaserMiddleTexture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
            Rectangle endFrameArea = LaserEndTexture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);

            // Start texture drawing.
            Main.spriteBatch.Draw(LaserBeginTexture,
                             Projectile.Center - Main.screenPosition,
                             startFrameArea,
                             beamColor,
                             Projectile.rotation,
                             LaserBeginTexture.Size() / 2f,
                             Projectile.scale,
                             SpriteEffects.FlipVertically,
                             0f);

            // Prepare things for body drawing.
            float laserBodyLength = LaserLength + middleFrameArea.Height;
            Vector2 centerOnLaser = Projectile.Center + Projectile.velocity * Projectile.scale * 5f;

            // Body drawing.
            if (laserBodyLength > 0f && middleFrameArea.Height >= 1f)
            {
                float laserOffset = middleFrameArea.Height * Projectile.scale;
                float incrementalBodyLength = 0f;
                while (incrementalBodyLength + 1f < laserBodyLength)
                {
                    Main.spriteBatch.Draw(LaserMiddleTexture,
                                     centerOnLaser - Main.screenPosition,
                                     middleFrameArea,
                                     beamColor,
                                     Projectile.rotation,
                                     LaserMiddleTexture.Size() * 0.5f,
                                     Projectile.scale,
                                     SpriteEffects.None,
                                     0f);
                    incrementalBodyLength += laserOffset;
                    centerOnLaser += Projectile.velocity * laserOffset;
                    middleFrameArea.Y += LaserMiddleTexture.Height / Main.projFrames[Projectile.type];
                    if (middleFrameArea.Y + middleFrameArea.Height > LaserMiddleTexture.Height)
                        middleFrameArea.Y = 0;
                }
            }

            Vector2 laserEndCenter = centerOnLaser - Main.screenPosition;
            Main.spriteBatch.Draw(LaserEndTexture,
                             laserEndCenter,
                             endFrameArea,
                             beamColor,
                             Projectile.rotation,
                             LaserEndTexture.Size() * 0.5f,
                             Projectile.scale,
                             SpriteEffects.FlipVertically,
                             0f);
            return false;
        }

        public override bool CanHitPlayer(Player target) => Projectile.scale >= 0.5f;

        public override void ModifyHitPlayer(Player target, ref int damage, ref bool crit)
        {
            target.Calamity().lastProjectileHit = Projectile;
        }
    }
}
