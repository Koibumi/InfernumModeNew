﻿using CalamityMod;
using CalamityMod.Projectiles.BaseProjectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Prime
{
    public class LaserRayIdle : BaseLaserbeamProjectile
    {
        public float InitialDirection = -100f;
        public int OwnerIndex => (int)Projectile.ai[1];
        public override float Lifetime => 260;
        public override Color LaserOverlayColor => Color.White;
        public override Color LightCastColor => Color.White;
        public override Texture2D LaserBeginTexture => ModContent.Request<Texture2D>("InfernumMode/ExtraTextures/PrimeBeamBegin").Value;
        public override Texture2D LaserMiddleTexture => ModContent.Request<Texture2D>("InfernumMode/ExtraTextures/PrimeBeamMid").Value;
        public override Texture2D LaserEndTexture => ModContent.Request<Texture2D>("InfernumMode/ExtraTextures/PrimeBeamEnd").Value;
        public override string Texture => "InfernumMode/ExtraTextures/PrimeBeamBegin";
        public override float MaxLaserLength => 3100f;
        public override float MaxScale => 1f;
        public override void SetStaticDefaults() => DisplayName.SetDefault("Deathray");

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 14;
            Projectile.hostile = true;
            Projectile.alpha = 255;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = (int)Lifetime;
            Projectile.Calamity().canBreakPlayerDefense = true;
            CooldownSlot = 1;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Projectile.localAI[0]);
            writer.Write(Projectile.localAI[1]);
            writer.Write(InitialDirection);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Projectile.localAI[0] = reader.ReadSingle();
            Projectile.localAI[1] = reader.ReadSingle();
            InitialDirection = reader.ReadSingle();
        }
        public override void AttachToSomething()
        {
            if (InitialDirection == -100f)
                InitialDirection = Projectile.velocity.ToRotation();

            if (!Main.npc.IndexInRange(OwnerIndex))
            {
                Projectile.Kill();
                return;
            }

            Projectile.velocity = (InitialDirection + Main.npc[OwnerIndex].Infernum().ExtraAI[3]).ToRotationVector2();
            Projectile.Center = Main.npc[OwnerIndex].Center - Vector2.UnitY * 16f + Projectile.velocity * 2f;
        }

        public override void ModifyHitPlayer(Player target, ref int damage, ref bool crit) => target.Calamity().lastProjectileHit = Projectile;

        public override void OnHitPlayer(Player target, int damage, bool crit) => target.AddBuff(BuffID.OnFire, 240);
    }
}
