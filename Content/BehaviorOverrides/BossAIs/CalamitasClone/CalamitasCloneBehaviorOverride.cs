using CalamityMod;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.NPCs;
using CalamityMod.NPCs.CalClone;
using CalamityMod.Particles;
using CalamityMod.Particles.Metaballs;
using CalamityMod.UI.CalamitasEnchants;
using InfernumMode.Assets.ExtraTextures;
using InfernumMode.Assets.Sounds;
using InfernumMode.Common.Graphics;
using InfernumMode.Common.Graphics.Particles;
using InfernumMode.Core.GlobalInstances.Systems;
using InfernumMode.Core.OverridingSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using CalamitasCloneBoss = CalamityMod.NPCs.CalClone.CalamitasClone;
using SCalBoss = CalamityMod.NPCs.SupremeCalamitas.SupremeCalamitas;

namespace InfernumMode.Content.BehaviorOverrides.BossAIs.CalamitasClone
{
    public class CalamitasCloneBehaviorOverride : NPCBehaviorOverride
    {
        public override int NPCOverrideType => ModContent.NPCType<CalamitasCloneBoss>();

        #region Enumerations
        public enum CloneAttackType
        {
            SpawnAnimation,
            WandFireballs,
            SoulSeekerResurrection,
            ShadowTeleports,
            DarkOverheadFireball,
            ConvergingBookEnergy // Nerd emoji.
        }
        #endregion

        #region AI

        public const float Phase2LifeRatio = 0.667f;

        public const float Phase3LifeRatio = 0.25f;

        public const int ArmRotationIndex = 5;

        public override float[] PhaseLifeRatioThresholds => new float[]
        {
            Phase2LifeRatio,
            Phase3LifeRatio
        };

        public override bool PreAI(NPC npc)
        {
            // FUCK YOU FUCK YOU FUCK YOU FUCK YOU FUCK YOU FUCK YOU FUCK
            if (npc.scale != 1f)
            {
                npc.width = 52;
                npc.height = 52;
                npc.scale = 1f;
            }

            // Do targeting.
            npc.TargetClosestIfTargetIsInvalid();
            Player target = Main.player[npc.target];

            // Set the whoAmI variable globally.
            CalamityGlobalNPC.calamitas = npc.whoAmI;

            // Handle despawn behaviors.
            if (!target.active || target.dead || !npc.WithinRange(target.Center, 7200f))
            {
                npc.velocity = Vector2.Lerp(npc.velocity, Vector2.UnitY * -28f, 0.08f);
                if (!npc.WithinRange(target.Center, 1450f))
                {
                    npc.life = 0;
                    npc.active = false;
                    npc.netUpdate = true;
                }
                return false;
            }

            int brotherCount = NPC.CountNPCS(ModContent.NPCType<Cataclysm>()) + NPC.CountNPCS(ModContent.NPCType<Catastrophe>());
            float lifeRatio = npc.life / (float)npc.lifeMax;
            ref float attackType = ref npc.ai[0];
            ref float attackTimer = ref npc.ai[1];
            ref float backgroundEffectIntensity = ref npc.localAI[1];
            ref float blackFormInterpolant = ref npc.localAI[2];
            ref float eyeGleamInterpolant = ref npc.localAI[3];
            ref float armRotation = ref npc.Infernum().ExtraAI[ArmRotationIndex];

            // Use a custom hitsound.
            npc.HitSound = SoundID.NPCHit49 with { Pitch = -0.56f };

            // Reset things every frame.
            npc.damage = npc.defDamage;
            npc.dontTakeDamage = false;
            npc.noGravity = true;
            npc.noTileCollide = true;
            npc.gfxOffY = 10f;
            
            switch ((CloneAttackType)(int)attackType)
            {
                case CloneAttackType.SpawnAnimation:
                    DoBehavior_SpawnAnimation(npc, target, ref attackTimer, ref backgroundEffectIntensity, ref blackFormInterpolant, ref eyeGleamInterpolant, ref armRotation);
                    break;
                case CloneAttackType.WandFireballs:
                    DoBehavior_WandFireballs(npc, target, ref attackTimer, ref armRotation);
                    break;
                case CloneAttackType.SoulSeekerResurrection:
                    DoBehavior_SoulSeekerResurrection(npc, target, ref attackTimer, ref armRotation);
                    break;
                case CloneAttackType.ShadowTeleports:
                    DoBehavior_ShadowTeleports(npc, target, ref attackTimer, ref armRotation, ref blackFormInterpolant);
                    break;
                case CloneAttackType.DarkOverheadFireball:
                    DoBehavior_DarkOverheadFireball(npc, target, ref attackTimer, ref armRotation);
                    break;
            }

            // Disable the base Calamity screen shader and background.
            if (Main.netMode != NetmodeID.Server)
                Filters.Scene["CalamityMod:CalamitasRun3"].Deactivate();

            attackTimer++;
            return false;
        }

        public static void DoBehavior_SpawnAnimation(NPC npc, Player target, ref float attackTimer, ref float backgroundEffectIntensity, ref float blackFormInterpolant, ref float eyeGleamInterpolant, ref float armRotation)
        {
            int blackFadeoutTime = 30;
            int blackFadeinTime = 6;
            int maximumDarknessTime = 50;
            int eyeGleamTime = 44;

            // Calculate the black fade intensity. This is used to give an illusion that CalClone emerged from the shadows.
            InfernumMode.BlackFade = Utils.GetLerpValue(0f, blackFadeoutTime, attackTimer, true) * Utils.GetLerpValue(blackFadeoutTime + blackFadeinTime + maximumDarknessTime, blackFadeoutTime + maximumDarknessTime, attackTimer, true);

            // Respond the gravity and natural tile collision for the duration of the attack.
            npc.noGravity = false;
            npc.noTileCollide = false;

            // Don't exist yet if the fade effects are ongoing.
            if (attackTimer < blackFadeoutTime)
            {
                blackFormInterpolant = 1f;
                npc.Opacity = 0f;
                npc.dontTakeDamage = true;
                npc.ShowNameOnHover = false;
                npc.Center = target.Center + Vector2.UnitX * target.direction * 450f;
                while (Collision.SolidCollision(npc.TopLeft, npc.width, npc.height))
                    npc.position.Y -= 2f;
                npc.Calamity().ShouldCloseHPBar = true;
                npc.Calamity().ProvidesProximityRage = false;

                armRotation = 0f;
            }

            // Appear once they're done.
            else if (InfernumMode.BlackFade < 1f)
            {
                blackFormInterpolant = MathHelper.Clamp(blackFormInterpolant - 0.018f, 0f, 1f);
                npc.Opacity = MathHelper.Clamp(npc.Opacity + 0.06f, 0f, 1f);
                npc.ShowNameOnHover = true;

                // Do an eye gleam effect.
                float gleamAnimationCompletion = Utils.GetLerpValue(blackFadeoutTime + blackFadeinTime + maximumDarknessTime, blackFadeoutTime + blackFadeinTime + maximumDarknessTime + eyeGleamTime, attackTimer, true);
                eyeGleamInterpolant = CalamityUtils.Convert01To010(gleamAnimationCompletion);
                if (attackTimer == blackFadeoutTime + blackFadeinTime + maximumDarknessTime)
                {
                    bool feelingLikeABigShot = Main.rand.NextBool(100) || Utilities.IsAprilFirst();
                    SoundEngine.PlaySound(feelingLikeABigShot ? InfernumSoundRegistry.GolemSpamtonSound : HeavenlyGale.LightningStrikeSound, target.Center);
                }

                // Look at the target.
                npc.spriteDirection = (target.Center.X < npc.Center.X).ToDirectionInt();

                backgroundEffectIntensity = MathHelper.Clamp(backgroundEffectIntensity + 0.011f, 0f, 1f);

                // Fly into the air and transition to the first attack after the background is fully dark.
                if (backgroundEffectIntensity >= 1f)
                {
                    SoundEngine.PlaySound(InfernumSoundRegistry.VassalJumpSound with { Pitch = -0.4f, Volume = 1.6f }, target.Center);
                    SoundEngine.PlaySound(SCalBoss.SpawnSound with { Pitch = -0.12f, Volume = 0.7f }, target.Center);

                    npc.velocity.Y -= 23f;
                    Collision.HitTiles(npc.TopLeft, Vector2.UnitY * -12f, npc.width, npc.height + 100);
                    SelectNextAttack(npc);
                }
            }

            // Perform animation effects.
            npc.frameCounter += 0.2f;
        }

        public static void DoBehavior_WandFireballs(NPC npc, Player target, ref float attackTimer, ref float armRotation)
        {
            int wandChargeUpTime = 45;
            int wandAimDelay = 32;
            int wandAimTime = 30;
            int wandWaveTime = 45;
            int wandCycleTime = wandAimTime + wandWaveTime;
            int totalWandCycles = 2;
            int flameReleaseRate = 3;
            int wandAttackCycle = (int)(attackTimer - wandChargeUpTime - wandAimDelay) % wandCycleTime;

            int wandReelBackTime = 40;

            float fireShootSpeed = 16.75f;
            Vector2 armStart = npc.Center + new Vector2(npc.spriteDirection * 9.6f, -2f);
            Vector2 wandEnd = armStart + (armRotation + MathHelper.Pi - MathHelper.PiOver2).ToRotationVector2() * npc.scale * 45f;
            wandEnd += (armRotation + MathHelper.Pi).ToRotationVector2() * npc.scale * npc.spriteDirection * -8f;
            ref float wandGlowInterpolant = ref npc.Infernum().ExtraAI[0];
            ref float throwingWand = ref npc.Infernum().ExtraAI[1];
            ref float wandWasThrown = ref npc.Infernum().ExtraAI[2];

            // Aim the wand at the sky.
            if (attackTimer < wandChargeUpTime && throwingWand == 0f)
            {
                armRotation = armRotation.AngleLerp(MathHelper.Pi, 0.06f).AngleTowards(MathHelper.Pi, 0.016f);
                npc.velocity *= 0.93f;
            }

            // Release lightning at the wand.
            if (attackTimer == wandChargeUpTime && throwingWand == 0f)
            {
                SoundEngine.PlaySound(HeavenlyGale.LightningStrikeSound, npc.Center);

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    for (int i = 0; i < 7; i++)
                    {
                        Vector2 lightningSpawnPosition = npc.Center - Vector2.UnitY.RotatedByRandom(0.51f) * Main.rand.NextFloat(900f, 1000f);
                        Vector2 lightningVelocity = (wandEnd - lightningSpawnPosition).SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(6.4f, 6.7f);

                        ProjectileSpawnManagementSystem.PrepareProjectileForSpawning(lightning =>
                        {
                            lightning.ModProjectile<BrimstoneLightning>().Destination = wandEnd;
                        });
                        Utilities.NewProjectileBetter(lightningSpawnPosition, lightningVelocity, ModContent.ProjectileType<BrimstoneLightning>(), 0, 0f, -1, lightningVelocity.ToRotation(), Main.rand.Next(100));
                    }

                    npc.velocity = Vector2.Zero;
                    npc.netUpdate = true;
                }
            }

            // Aim the wand at the target and hover near them.
            if (attackTimer >= wandChargeUpTime + wandAimDelay && throwingWand == 0f)
            {
                float idealRotation = npc.AngleTo(target.Center) - MathHelper.PiOver2;

                // Wave the wand and release flame projectiles.
                if (wandAttackCycle >= wandAimTime)
                {
                    // Shoot fire.
                    if (wandAttackCycle % flameReleaseRate == 0f)
                    {
                        SoundEngine.PlaySound(SoundID.Item73, wandEnd);

                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            if (npc.velocity.Length() < 10f)
                                npc.velocity -= npc.SafeDirectionTo(target.Center) * 1.3f;
                            Vector2 fireShootVelocity = npc.SafeDirectionTo(wandEnd) * fireShootSpeed;
                            Utilities.NewProjectileBetter(wandEnd, fireShootVelocity, ModContent.ProjectileType<DarkMagicFlame>(), 155, 0f);
                        }

                        // Do funny screen effects.
                        Main.LocalPlayer.Infernum_Camera().CurrentScreenShakePower = 4f;
                    }

                    float aimCompletion = Utils.GetLerpValue(0f, wandWaveTime, wandAttackCycle - wandAimTime, true);
                    float aimAngularOffset = MathF.Sin(3f * MathHelper.Pi * aimCompletion) * 1.09f;
                    idealRotation += aimAngularOffset;
                }
                else
                    npc.Center = Vector2.Lerp(npc.Center, target.Center, 0.024f);

                armRotation = armRotation.AngleLerp(idealRotation, 0.08f).AngleTowards(idealRotation, 0.017f);

                // Fly near the target.
                Vector2 idealVelocity = npc.SafeDirectionTo(target.Center + Vector2.UnitX * (target.Center.X < npc.Center.X).ToDirectionInt() * 400f) * 16f;
                npc.SimpleFlyMovement(idealVelocity, 0.22f);

                // Emit cinders at the end of the wand.
                Dust cinder = Dust.NewDustPerfect(wandEnd, Main.rand.NextBool() ? 169 : 60, -Vector2.UnitY.RotatedByRandom(0.56f) * Main.rand.NextFloat(2f));
                cinder.scale *= 1.4f;
                cinder.color = Color.Lerp(Color.White, Color.Orange, Main.rand.NextFloat());
                cinder.noLight = true;
                cinder.noLightEmittence = true;
                cinder.noGravity = true;
            }

            // After the cycles have completed, move the arm back in anticipation before throwing it.
            if (throwingWand == 1f)
            {
                npc.velocity *= 0.94f;

                // Move the arm back.
                if (attackTimer >= wandReelBackTime)
                {
                    float idealRotation = MathHelper.Pi - 2.44f * npc.spriteDirection;
                    if (MathHelper.Distance(MathHelper.WrapAngle(idealRotation), MathHelper.WrapAngle(armRotation)) > 0.2f)
                        armRotation -= npc.spriteDirection * 0.18f;
                }
                else
                {
                    float idealRotation = (-MathHelper.PiOver2 - 0.72f) * npc.spriteDirection;
                    armRotation = armRotation.AngleLerp(idealRotation, 0.075f).AngleTowards(idealRotation, 0.019f);
                }

                // Throw the wand.
                if (attackTimer == wandReelBackTime)
                {
                    SoundEngine.PlaySound(SoundID.Item117 with { Pitch = 0.3f }, npc.Center);

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Utilities.NewProjectileBetter(armStart, (target.Center - armStart).SafeNormalize(Vector2.UnitY) * 18f, ModContent.ProjectileType<CharredWand>(), 0, 0f);
                        wandWasThrown = 1f;
                        npc.netUpdate = true;
                    }
                }

                if (attackTimer >= wandReelBackTime + 132f)
                    SelectNextAttack(npc);
            }

            // Look at the target.
            npc.spriteDirection = (target.Center.X < npc.Center.X).ToDirectionInt();

            // Perform animation effects.
            npc.frameCounter += 0.2f;

            if (attackTimer >= wandChargeUpTime + wandAimDelay + wandCycleTime * totalWandCycles && throwingWand == 0f)
            {
                throwingWand = 1f;
                attackTimer = 0f;
                npc.netUpdate = true;
            }
        }

        public static void DoBehavior_SoulSeekerResurrection(NPC npc, Player target, ref float attackTimer, ref float armRotation)
        {
            int redirectTime = 45;
            int seekerSummonTime = 30;
            int seekerShootTime = 240;
            int laserTelegraphTime = 120;
            int laserShootTime = EntropyBeam.Lifetime;
            Vector2 armStart = npc.Center + new Vector2(npc.spriteDirection * 9.6f, -2f);
            Vector2 staffEnd = armStart + (armRotation + MathHelper.Pi - MathHelper.PiOver2).ToRotationVector2() * npc.scale * 66f;
            ref float totalSummonedSoulSeekers = ref npc.Infernum().ExtraAI[0];
            ref float telegraphInterpolant = ref npc.Infernum().ExtraAI[1];
            ref float beamDirection = ref npc.Infernum().ExtraAI[2];

            // Hover to the side of the target.
            if (attackTimer <= redirectTime)
            {
                Vector2 hoverDestination = target.Center + Vector2.UnitX * (target.Center.X < npc.Center.X).ToDirectionInt() * 400f;
                npc.Center = Vector2.Lerp(npc.Center, hoverDestination, 0.03f).MoveTowards(hoverDestination, 2.4f);
                npc.SimpleFlyMovement(npc.SafeDirectionTo(hoverDestination) * 18f, 0.32f);

                if (Collision.SolidCollision(npc.TopLeft, npc.width, npc.height + 800))
                    npc.position.Y -= 10f;
            }

            // Aim Entropy's Vigil downwards and use it to raise soul seekers from the dead.
            else if (attackTimer <= redirectTime + seekerSummonTime)
            {
                npc.velocity *= 0.9f;

                float idealRotation = Utils.Remap(attackTimer - redirectTime, 0f, seekerSummonTime, -0.54f, 0.54f);
                armRotation = armRotation.AngleLerp(idealRotation, 0.2f).AngleTowards(idealRotation, 0.03f);
                if (attackTimer % 5f == 4f)
                {
                    SoundEngine.PlaySound(SoundID.Item74, npc.Center);
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                        Utilities.NewProjectileBetter(staffEnd, npc.SafeDirectionTo(staffEnd), ModContent.ProjectileType<SoulSeekerResurrectionBeam>(), 0, 0f);
                }
            }

            // Hover near the target.
            else
            {
                if (attackTimer <= redirectTime + seekerSummonTime + seekerShootTime)
                {
                    Vector2 hoverDestination = target.Center + Vector2.UnitX * (target.Center.X < npc.Center.X).ToDirectionInt() * 300f;
                    hoverDestination.Y -= 60f;
                    npc.SimpleFlyMovement(npc.SafeDirectionTo(hoverDestination) * 18f, 0.32f);

                    float idealRotation = npc.AngleTo(target.Center) - MathHelper.PiOver2;
                    armRotation = armRotation.AngleTowards(idealRotation, 0.05f);
                }

                // Make all seekers go away.
                if (attackTimer == redirectTime + seekerSummonTime + seekerShootTime)
                {
                    SoundEngine.PlaySound(CalamitasEnchantUI.EXSound with { Pitch = -0.5f }, target.Center);

                    // Teleport above the player and make all seekers leave.
                    npc.Center = target.Center - Vector2.UnitY * 350f;
                    npc.velocity = Vector2.Zero;
                    armRotation = MathHelper.Pi;

                    armStart = npc.Center + new Vector2(npc.spriteDirection * 9.6f, -2f);
                    staffEnd = armStart + (armRotation + MathHelper.Pi - MathHelper.PiOver2).ToRotationVector2() * npc.scale * 66f;
                    for (int i = 0; i < 35; i++)
                    {
                        Color fireColor = Main.rand.NextBool() ? Color.Yellow : Color.Red;
                        CloudParticle fireCloud = new(staffEnd, (MathHelper.TwoPi * i / 35f).ToRotationVector2() * 20f, fireColor, Color.DarkGray, 50, Main.rand.NextFloat(2.6f, 3.4f));
                        GeneralParticleHandler.SpawnParticle(fireCloud);
                    }

                    ScreenEffectSystem.SetBlurEffect(staffEnd, 1.6f, 45);
                    target.Infernum_Camera().CurrentScreenShakePower = 10f;

                    Utilities.DeleteAllProjectiles(false, ModContent.ProjectileType<DarkMagicFlame>());

                    int seekerID = ModContent.NPCType<SoulSeeker>();
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC n = Main.npc[i];

                        if (n.active && n.type == seekerID)
                        {
                            n.Infernum().ExtraAI[0] = 1f;
                            n.netUpdate = true;
                        }
                    }
                }

                // Aim the staff at the target in anticipation of the laser.
                if (attackTimer <= redirectTime + seekerSummonTime + seekerShootTime + laserTelegraphTime)
                {
                    // Play a charge telegraph sound.
                    if (attackTimer == redirectTime + seekerSummonTime + seekerShootTime)
                        SoundEngine.PlaySound(InfernumSoundRegistry.EntropyRayChargeSound, target.Center);

                    float telegraphCompletion = Utils.GetLerpValue(0f, laserTelegraphTime, attackTimer - redirectTime - seekerSummonTime - seekerShootTime, true);
                    telegraphInterpolant = Utils.GetLerpValue(0f, 0.67f, telegraphCompletion, true) * Utils.GetLerpValue(1f, 0.84f, telegraphCompletion, true);

                    float idealRotation = npc.AngleTo(target.Center) - MathHelper.PiOver2;
                    armRotation = armRotation.AngleLerp(idealRotation, 0.04f).AngleTowards(idealRotation, 0.01f);
                }

                // Fire the laser.
                if (attackTimer == redirectTime + seekerSummonTime + seekerShootTime + laserTelegraphTime)
                {
                    ScreenEffectSystem.SetBlurEffect(staffEnd, 1.6f, 45);
                    target.Infernum_Camera().CurrentScreenShakePower = 10f;
                    SoundEngine.PlaySound(InfernumSoundRegistry.EntropyRayFireSound, target.Center);

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Utilities.NewProjectileBetter(npc.Center, armRotation.ToRotationVector2(), ModContent.ProjectileType<EntropyBeam>(), 240, 0f);
                        beamDirection = (MathHelper.WrapAngle(npc.AngleTo(target.Center) - armRotation - MathHelper.PiOver2) > 0f).ToDirectionInt();
                        npc.netUpdate = true;
                    }
                }

                // Spin the laser after it appears.
                if (attackTimer >= redirectTime + seekerSummonTime + seekerShootTime + laserTelegraphTime)
                    armRotation += beamDirection * 0.023f;

                if (attackTimer >= redirectTime + seekerSummonTime + seekerShootTime + laserTelegraphTime + laserShootTime)
                    SelectNextAttack(npc);
            }

            // Look at the target.
            if (attackTimer < redirectTime + seekerSummonTime + seekerShootTime + laserTelegraphTime)
                npc.spriteDirection = (target.Center.X < npc.Center.X).ToDirectionInt();

            // Perform animation effects.
            npc.frameCounter += 0.2f;

            if (attackTimer >= 800f)
            {
                npc.Center = target.Center - Vector2.UnitY * 300f;
                attackTimer = 0f;
            }
        }

        public static void DoBehavior_ShadowTeleports(NPC npc, Player target, ref float attackTimer, ref float armRotation, ref float blackFormInterpolant)
        {
            int jitterTime = 45;
            int disappearTime = 20;
            int sitTime = 14;
            int fadeOutTime = 25;
            int teleportCount = 6;
            int wrappedAttackTimer = (int)(attackTimer - jitterTime) % (disappearTime + sitTime + fadeOutTime);
            ref float teleportOffsetAngle = ref npc.Infernum().ExtraAI[0];
            ref float teleportCounter = ref npc.Infernum().ExtraAI[1];

            armRotation = 0f;

            // Jitter in place and become transluscent.
            if (attackTimer <= jitterTime)
            {
                float jitterInterpolant = Utils.GetLerpValue(0f, jitterTime, attackTimer, true);
                npc.Center += Main.rand.NextVector2Circular(3f, 3f) * jitterInterpolant;
                npc.Opacity = MathHelper.Lerp(1f, 0.5f, jitterInterpolant);
            }

            if (attackTimer >= jitterTime)
            {
                // Dissipate into shadow particles.
                if (wrappedAttackTimer == 0f)
                {
                    if (attackTimer == jitterTime)
                        teleportOffsetAngle = MathHelper.TwoPi * Main.rand.Next(4) / 4f;
                    else
                    {
                        teleportOffsetAngle += MathHelper.TwoPi / teleportCount;
                        teleportCounter++;
                    }

                    npc.velocity = Vector2.Zero;
                    npc.Opacity = 0f;
                    npc.dontTakeDamage = true;
                    npc.netUpdate = true;
                    if (teleportCounter >= teleportCount - 1f)
                    {
                        npc.Opacity = 1f;
                        SelectNextAttack(npc);
                        return;
                    }

                    SoundEngine.PlaySound(InfernumSoundRegistry.SizzleSound, target.Center);

                    var cloneTexture = ModContent.Request<Texture2D>("InfernumMode/Content/BehaviorOverrides/BossAIs/CalamitasClone/CalamitasCloneSingleFrame", AssetRequestMode.ImmediateLoad).Value;
                    cloneTexture.CreateMetaballsFromTexture(ref FusableParticleManager.GetParticleSetByType<ShadowDemonParticleSet>().Particles, npc.Center, npc.rotation, npc.scale, 28f, 10);

                    if (Main.netMode != NetmodeID.Server)
                    {
                        for (int i = 0; i < 12; i++)
                            Utilities.NewProjectileBetter(npc.Center, Main.rand.NextVector2Unit() * Main.rand.NextFloat(9f, 14f), ModContent.ProjectileType<ShadowBlob>(), 0, 0f);
                    }
                }

                // Hover above the target.
                if (wrappedAttackTimer <= disappearTime)
                {
                    npc.Center = target.Center + teleportOffsetAngle.ToRotationVector2() * 396f;
                    npc.spriteDirection = (target.Center.X < npc.Center.X).ToDirectionInt();

                    // Create shadow particles shortly before fully appearing.
                    if (wrappedAttackTimer >= disappearTime - 15f)
                    {
                        npc.Opacity = MathHelper.Clamp(npc.Opacity + 0.07f, 0f, 1f);
                        npc.dontTakeDamage = true;
                        for (int i = 0; i < (1f - npc.Opacity) * 24f; i++)
                        {
                            Color shadowMistColor = Color.Lerp(Color.MediumPurple, Color.DarkBlue, Main.rand.NextFloat(0.66f));
                            var mist = new MediumMistParticle(npc.Center + npc.Opacity * Main.rand.NextVector2Circular(90f, 90f), Main.rand.NextVector2Circular(5f, 5f), shadowMistColor, Color.DarkGray, Main.rand.NextFloat(0.55f, 0.7f), 172f, Main.rand.NextFloatDirection() * 0.012f);
                            GeneralParticleHandler.SpawnParticle(mist);
                        }
                    }
                    else
                        npc.Opacity = 0f;
                }

                if (wrappedAttackTimer == disappearTime)
                {
                    SoundEngine.PlaySound(InfernumSoundRegistry.CalCloneTeleportSound, target.Center);

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        for (int i = 0; i < 7; i++)
                        {
                            float shootOffsetAngle = MathHelper.Lerp(-0.63f, 0.63f, i / 6f);
                            Vector2 shootVelocity = npc.SafeDirectionTo(target.Center).RotatedBy(shootOffsetAngle) * 12f;
                            Utilities.NewProjectileBetter(npc.Center + shootVelocity, shootVelocity, ModContent.ProjectileType<DarkMagicFlame>(), 155, 0f);
                        }
                    }
                }
            }

            blackFormInterpolant = MathF.Pow(1f - npc.Opacity, 0.2f) * 3f;
        }

        public static void DoBehavior_DarkOverheadFireball(NPC npc, Player target, ref float attackTimer, ref float armRotation)
        {
            int redirectTime = 45;
            int fireOrbReleaseDelay = 15;
            int boltReleaseDelay = 60;
            int boltCircleReleaseRate = 25;
            int boltShootCycleTime = 90;
            int wrappedAttackTimer = (int)(attackTimer - redirectTime - fireOrbReleaseDelay - boltReleaseDelay) % boltShootCycleTime;
            int fireShootTime = 240;
            var fireOrbs = Utilities.AllProjectilesByID(ModContent.ProjectileType<LargeDarkFireOrb>());
            bool readyToBlowUpFireOrb = attackTimer >= redirectTime + fireOrbReleaseDelay + boltReleaseDelay + fireShootTime;
            bool canShootFire = attackTimer >= redirectTime + fireOrbReleaseDelay + boltReleaseDelay && !readyToBlowUpFireOrb;
            Vector2 armStart = npc.Center + new Vector2(npc.spriteDirection * 9.6f, -2f);
            Vector2 armEnd = armStart + (armRotation + MathHelper.PiOver2).ToRotationVector2() * npc.scale * 8f;
            ref float fireShootCounter = ref npc.Infernum().ExtraAI[0];
            ref float isSlammingFireballDown = ref npc.Infernum().ExtraAI[1];
            ref float fireballHasExploded = ref npc.Infernum().ExtraAI[2];

            if (fireballHasExploded == 0f)
            {
                // Hover above the target at first.
                if (attackTimer < redirectTime)
                {
                    Vector2 hoverDestination = target.Center - Vector2.UnitY * 200f;
                    Vector2 idealVelocity = (hoverDestination - npc.Center) * 0.06f;
                    npc.velocity = Vector2.Lerp(npc.velocity, idealVelocity, 0.12f);

                    if (MathHelper.Distance(target.Center.X, npc.Center.X) >= 50f)
                        npc.spriteDirection = (target.Center.X < npc.Center.X).ToDirectionInt();
                }

                // Afterwards have CalClone raise her arm up towards the fire orb and slow down.
                else
                {
                    if (npc.velocity.Length() > 0.001f)
                        npc.velocity = npc.velocity.ClampMagnitude(0f, 9f) * 0.8f;
                    armRotation = armRotation.AngleLerp(MathHelper.Pi, 0.05f).AngleTowards(MathHelper.Pi, 0.015f);

                    if (!readyToBlowUpFireOrb)
                    {
                        Dust magic = Dust.NewDustPerfect(armEnd + Main.rand.NextVector2Circular(3f, 3f), 267);
                        magic.color = CalamityUtils.MulticolorLerp(Main.rand.NextFloat(), Color.MediumPurple, Color.Red, Color.Orange, Color.Red);
                        magic.noGravity = true;
                        magic.velocity = -Vector2.UnitY.RotatedByRandom(0.22f) * Main.rand.NextFloat(0.4f, 18f);
                        magic.scale = Main.rand.NextFloat(1f, 1.3f);
                    }
                }

                // Prepare the fire orb.
                if (attackTimer == redirectTime + fireOrbReleaseDelay)
                {
                    SoundEngine.PlaySound(SoundID.Item163 with { Pitch = 0.08f }, npc.Center);

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Vector2 fireOrbSpawnPosition = npc.Top - Vector2.UnitY * (LargeDarkFireOrb.MaxFireOrbRadius - 75f);
                        Utilities.NewProjectileBetter(fireOrbSpawnPosition, Vector2.Zero, ModContent.ProjectileType<LargeDarkFireOrb>(), 0, 0f);
                    }
                }

                // Release fire from the orb.
                if (canShootFire && wrappedAttackTimer <= boltShootCycleTime - 30f && wrappedAttackTimer % boltCircleReleaseRate == boltCircleReleaseRate - 1f)
                {
                    SoundEngine.PlaySound(InfernumSoundRegistry.SizzleSound, target.Center);

                    if (Main.netMode != NetmodeID.MultiplayerClient && fireOrbs.Any())
                    {
                        int fireShootCount = (int)Utils.Remap(npc.Distance(target.Center), 800f, 3000f, 24f, 48f);
                        if (fireShootCount % 2 != 0)
                            fireShootCount++;

                        float fireShootSpeed = Utils.Remap(npc.Distance(target.Center), 600f, 3000f, 8.5f, 70f);
                        Vector2 fireOrbCenter = fireOrbs.First().Center;
                        for (int i = 0; i < fireShootCount; i++)
                        {
                            Vector2 fireShootVelocity = npc.SafeDirectionTo(target.Center).RotatedBy(MathHelper.TwoPi * (i + (fireShootCounter % 2f == 0f ? 0.5f : 0f)) / fireShootCount) * fireShootSpeed;
                            Utilities.NewProjectileBetter(fireOrbCenter + fireShootVelocity * 5f, fireShootVelocity, ModContent.ProjectileType<DarkMagicFlame>(), 155, 0f);
                        }

                        fireShootCounter++;
                        npc.netUpdate = true;
                    }
                }

                // Blow up the fire orb.
                if (readyToBlowUpFireOrb && fireOrbs.Any())
                {
                    Projectile fireOrb = fireOrbs.First();
                    float idealHoverDestinationY = npc.Center.Y - LargeDarkFireOrb.MaxFireOrbRadius - 120f;
                    if (fireOrb.Center.Y >= idealHoverDestinationY + 10f && fireOrb.velocity.Length() < 2f && fireOrb.timeLeft >= 40)
                        fireOrb.Center = new Vector2(fireOrb.Center.X, MathHelper.Lerp(fireOrb.Center.Y, idealHoverDestinationY, 0.12f));

                    // Make the orb slam down.
                    else if (isSlammingFireballDown == 0f)
                    {
                        fireOrb.velocity = Vector2.UnitY * 8f;
                        fireOrb.netUpdate = true;
                        isSlammingFireballDown = 1f;
                        npc.netUpdate = true;
                    }
                }
            }

            // Create meteors from above.
            else
            {
                if (attackTimer == 1f)
                {
                    SoundEngine.PlaySound(InfernumSoundRegistry.ProvidenceLavaEruptionSmallSound, target.Center);
                    ScreenEffectSystem.SetFlashEffect(target.Center - Vector2.UnitY * 500f, 4f, 35);
                    target.Infernum_Camera().CurrentScreenShakePower = 10f;

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        for (float dx = -1300f; dx < 1300f; dx += 150f)
                        {
                            Vector2 meteorSpawnPosition = target.Center + new Vector2(dx - 60f, Math.Abs(dx) * -0.35f - 600f);
                            Vector2 meteorShootVelocity = Vector2.UnitY * 15f;
                            Utilities.NewProjectileBetter(meteorSpawnPosition, meteorShootVelocity, ModContent.ProjectileType<BrimstoneMeteor>(), 160, 0f);
                        }
                    }
                }

                if (attackTimer >= 90f)
                    SelectNextAttack(npc);
            }
        }

        public static void SelectNextAttack(NPC npc)
        {
            float lifeRatio = npc.life / (float)npc.lifeMax;
            CloneAttackType currentAttack = (CloneAttackType)npc.ai[0];
            CloneAttackType nextAttack = CloneAttackType.DarkOverheadFireball;
            if (currentAttack == CloneAttackType.SpawnAnimation)
                nextAttack = CloneAttackType.SoulSeekerResurrection;
            if (currentAttack == CloneAttackType.DarkOverheadFireball)
                nextAttack = CloneAttackType.WandFireballs;

            for (int i = 0; i < 5; i++)
                npc.Infernum().ExtraAI[i] = 0f;

            npc.ai[0] = (int)nextAttack;
            npc.ai[1] = 0f;
            npc.netUpdate = true;
        }
        #endregion AI

        #region Frames and Drawcode

        public override void FindFrame(NPC npc, int frameHeight)
        {
            // Redefine the frame size to be in line with the SCal sheet.
            npc.frame.Width = 52;
            npc.frame.Height = 52;

            npc.frameCounter += npc.localAI[0];
            int frameOffset = (int)npc.frameCounter % 6;
            int frame = frameOffset;

            npc.frame.X = npc.frame.Width * (frame / 21);
            npc.frame.Y = npc.frame.Height * (frame % 21);
        }

        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Color lightColor)
        {
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (npc.spriteDirection == 1)
                spriteEffects = SpriteEffects.FlipHorizontally;

            int afterimageCount = 8;
            Texture2D texture = ModContent.Request<Texture2D>("InfernumMode/Content/BehaviorOverrides/BossAIs/CalamitasClone/CalamitasClone").Value;
            Texture2D armTexture = ModContent.Request<Texture2D>("InfernumMode/Content/BehaviorOverrides/BossAIs/CalamitasClone/CalamitasCloneArm").Value;
            Vector2 origin = npc.frame.Size() * 0.5f;

            // Incorporate the black shadow form effects.
            lightColor = Color.Lerp(lightColor, Color.Black, MathF.Pow(npc.localAI[2], 0.33f));
            float shadowBackglowOffset = 25f * MathF.Pow(npc.localAI[2], 2.8f) * npc.scale;
            float eyeGleamInterpolant = npc.localAI[3];

            // Draw afterimages.
            if (CalamityConfig.Instance.Afterimages)
            {
                for (int i = 1; i < afterimageCount; i += 2)
                {
                    Color afterimageColor = lightColor * ((afterimageCount - i) / 15f) * npc.Opacity;
                    Vector2 afterimageDrawPosition = Vector2.Lerp(npc.oldPos[i] + npc.Size * 0.5f, npc.Center, 0.55f);
                    afterimageDrawPosition += -Main.screenPosition + Vector2.UnitY * npc.gfxOffY;
                    Main.spriteBatch.Draw(texture, afterimageDrawPosition, npc.frame, afterimageColor, npc.rotation, origin, npc.scale, spriteEffects, 0f);
                }
            }

            // Draw a shadow backglow.
            Vector2 drawPosition = npc.Center - Main.screenPosition + Vector2.UnitY * npc.gfxOffY;
            Vector2 armDrawPosition = drawPosition + new Vector2(npc.spriteDirection * 9.6f, -2f);
            if (shadowBackglowOffset > 0f)
            {
                for (int i = 0; i < 10; i++)
                {
                    Vector2 drawOffset = (MathHelper.TwoPi * i / 10f).ToRotationVector2() * shadowBackglowOffset;
                    Main.spriteBatch.Draw(texture, drawPosition + drawOffset, npc.frame, lightColor with { A = 0 } * (1f - npc.localAI[2]), npc.rotation, origin, npc.scale, spriteEffects, 0f);
                }
            }

            Color shadowColor = CalamityUtils.ColorSwap(Color.Purple, Color.Blue, 10f);
            lightColor = Color.Lerp(lightColor, shadowColor, 0.7f);
            lightColor = Color.Lerp(lightColor, Color.Black, 0.32f);
            lightColor.A = 232;

            // Draw the body and arm.
            float armRotation = npc.Infernum().ExtraAI[ArmRotationIndex];

            // Draw a backglow.
            for (int i = 0; i < 5; i++)
            {
                Vector2 drawOffset = (MathHelper.TwoPi * i / 5f).ToRotationVector2() * 4f;
                Color backglowColor = Color.Purple with { A = 0 };
                Main.spriteBatch.Draw(texture, drawPosition + drawOffset, npc.frame, backglowColor * npc.Opacity * 0.45f, npc.rotation, origin, npc.scale, spriteEffects, 0f);
            }

            // Draw the body and arms.
            Main.spriteBatch.Draw(texture, drawPosition, npc.frame, lightColor * npc.Opacity, npc.rotation, origin, npc.scale, spriteEffects, 0f);
            Main.spriteBatch.Draw(armTexture, armDrawPosition, null, lightColor * npc.Opacity, armRotation, armTexture.Size() * new Vector2(0.4f, 0.1f), npc.scale, spriteEffects, 0f);

            // Draw the wand if it's being used.
            if (npc.ai[0] == (int)CloneAttackType.WandFireballs && npc.Infernum().ExtraAI[2] == 0f)
            {
                float wandBrightness = npc.Infernum().ExtraAI[0];
                float wandRotation = armRotation + MathHelper.Pi - MathHelper.PiOver4;
                Texture2D wandTexture = ModContent.Request<Texture2D>("InfernumMode/Content/BehaviorOverrides/BossAIs/CalamitasClone/CharredWand").Value;
                Vector2 wandDrawPosition = armDrawPosition + (armRotation + MathHelper.Pi - MathHelper.PiOver2).ToRotationVector2() * npc.scale * 12f;

                if (wandBrightness > 0f)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        Color wandMagicBackglowColor = Color.HotPink with { A = 0 } * wandBrightness * npc.Opacity * 0.6f;
                        Vector2 drawOffset = (MathHelper.TwoPi * i / 6f).ToRotationVector2() * wandBrightness * 3f;
                        Main.spriteBatch.Draw(wandTexture, wandDrawPosition + drawOffset, null, wandMagicBackglowColor, wandRotation, wandTexture.Size() * Vector2.UnitY, npc.scale * 0.7f, 0, 0f);
                    }
                }
                Main.spriteBatch.Draw(wandTexture, wandDrawPosition, null, Color.LightGray * npc.Opacity, wandRotation, wandTexture.Size() * Vector2.UnitY, npc.scale * 0.7f, 0, 0f);
            }

            // Draw the staff if it's being used.
            if (npc.ai[0] == (int)CloneAttackType.SoulSeekerResurrection)
            {
                float staffRotation = armRotation + MathHelper.Pi - MathHelper.PiOver4;
                float telegraphInterpolant = npc.Infernum().ExtraAI[1];
                Texture2D staffTexture = ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Summon/EntropysVigil").Value;
                Vector2 staffDrawPosition = armDrawPosition + (armRotation + MathHelper.Pi - MathHelper.PiOver2).ToRotationVector2() * npc.scale * 12f;
                Vector2 staffEnd = armDrawPosition + (armRotation + MathHelper.Pi - MathHelper.PiOver2).ToRotationVector2() * npc.scale * 48f;

                BloomLineDrawInfo lineInfo = new()
                {
                    LineRotation = -armRotation - MathHelper.PiOver2,
                    WidthFactor = 0.004f + MathF.Pow(telegraphInterpolant, 4f) * (MathF.Sin(Main.GlobalTimeWrappedHourly * 3f) * 0.001f + 0.001f),
                    BloomIntensity = MathHelper.Lerp(0.3f, 0.4f, telegraphInterpolant),
                    Scale = Vector2.One * telegraphInterpolant * MathHelper.Clamp(npc.Distance(Main.player[npc.target].Center) * 3f, 10f, 3600f),
                    MainColor = Color.Lerp(Color.HotPink, Color.Red, telegraphInterpolant * 0.9f + 0.1f),
                    DarkerColor = Color.Orange,
                    Opacity = MathF.Sqrt(telegraphInterpolant),
                    BloomOpacity = 0.5f,
                    LightStrength = 5f
                };
                Utilities.DrawBloomLineTelegraph(staffEnd, lineInfo);

                Main.spriteBatch.Draw(staffTexture, staffDrawPosition, null, Color.White * npc.Opacity, staffRotation, staffTexture.Size() * Vector2.UnitY, npc.scale * 0.85f, 0, 0f);
            }

            // Draw the eye gleam.
            if (eyeGleamInterpolant > 0f)
            {
                float eyePulse = Main.GlobalTimeWrappedHourly * 0.84f % 1f;
                Texture2D eyeGleam = InfernumTextureRegistry.Gleam.Value;
                Vector2 eyePosition = npc.Center + new Vector2(npc.spriteDirection * -4f, -6f);
                Vector2 horizontalGleamScaleSmall = new Vector2(eyeGleamInterpolant * 3f, 1f) * 0.55f;
                Vector2 verticalGleamScaleSmall = new Vector2(1f, eyeGleamInterpolant * 2f) * 0.55f;
                Vector2 horizontalGleamScaleBig = horizontalGleamScaleSmall * (1f + eyePulse * 2f);
                Vector2 verticalGleamScaleBig = verticalGleamScaleSmall * (1f + eyePulse * 2f);
                Color eyeGleamColorSmall = Color.Violet * eyeGleamInterpolant;
                eyeGleamColorSmall.A = 0;
                Color eyeGleamColorBig = eyeGleamColorSmall * (1f - eyePulse);

                // Draw a pulsating red eye.
                Main.spriteBatch.Draw(eyeGleam, eyePosition - Main.screenPosition, null, eyeGleamColorSmall, 0f, eyeGleam.Size() * 0.5f, horizontalGleamScaleSmall, 0, 0f);
                Main.spriteBatch.Draw(eyeGleam, eyePosition - Main.screenPosition, null, eyeGleamColorSmall, 0f, eyeGleam.Size() * 0.5f, verticalGleamScaleSmall, 0, 0f);
                Main.spriteBatch.Draw(eyeGleam, eyePosition - Main.screenPosition, null, eyeGleamColorBig, 0f, eyeGleam.Size() * 0.5f, horizontalGleamScaleBig, 0, 0f);
                Main.spriteBatch.Draw(eyeGleam, eyePosition - Main.screenPosition, null, eyeGleamColorBig, 0f, eyeGleam.Size() * 0.5f, verticalGleamScaleBig, 0, 0f);
            }

            return false;
        }
        #endregion Frames and Drawcode
    }
}
