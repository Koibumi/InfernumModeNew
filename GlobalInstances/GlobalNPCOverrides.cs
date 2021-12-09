﻿using CalamityMod;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Dyes;
using CalamityMod.NPCs;
using CalamityMod.NPCs.Bumblebirb;
using CalamityMod.NPCs.Calamitas;
using CalamityMod.NPCs.DesertScourge;
using CalamityMod.NPCs.DevourerofGods;
using CalamityMod.NPCs.Leviathan;
using CalamityMod.NPCs.Ravager;
using CalamityMod.Projectiles.Ranged;
using CalamityMod.Projectiles.DraedonsArsenal;
using CalamityMod.Projectiles.Rogue;
using CalamityMod.World;
using InfernumMode.Buffs;
using InfernumMode.BehaviorOverrides.BossAIs.Cultist;
using InfernumMode.BehaviorOverrides.BossAIs.EoW;
using InfernumMode.BehaviorOverrides.BossAIs.SlimeGod;
using InfernumMode.BehaviorOverrides.BossAIs.WallOfFlesh;
using InfernumMode.OverridingSystem;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

using SlimeGodCore = CalamityMod.NPCs.SlimeGod.SlimeGodCore;
using CrimulanSGBig = CalamityMod.NPCs.SlimeGod.SlimeGodRun;
using EbonianSGBig = CalamityMod.NPCs.SlimeGod.SlimeGod;
using CryogenNPC = CalamityMod.NPCs.Cryogen.Cryogen;
using PolterghastNPC = CalamityMod.NPCs.Polterghast.Polterghast;
using OldDukeNPC = CalamityMod.NPCs.OldDuke.OldDuke;
using YharonNPC = CalamityMod.NPCs.Yharon.Yharon;

using CalamityMod.NPCs.AquaticScourge;
using CalamityMod.NPCs.Signus;
using CalamityMod.NPCs.AstrumAureus;
using CalamityMod.Projectiles.Melee;
using CalamityMod.Projectiles.Magic;
using CalamityMod.Projectiles.Summon;
using CalamityMod.NPCs.GreatSandShark;
using CalamityMod.Projectiles.Typeless;
using CalamityMod.Projectiles.Hybrid;
using CalamityMod.NPCs.SupremeCalamitas;
using InfernumMode.BehaviorOverrides.BossAIs.Draedon;
using CalamityMod.Events;
using CalamityMod.UI;
using System.Linq;
using InfernumMode.Items;
using CalamityMod.NPCs.Perforator;
using CalamityMod.NPCs.Crabulon;
using CalamityMod.Buffs.DamageOverTime;
using InfernumMode.BehaviorOverrides.BossAIs.BoC;
using CalamityMod.Buffs.StatDebuffs;

namespace InfernumMode.GlobalInstances
{
    public partial class GlobalNPCOverrides : GlobalNPC
    {
        #region Instance and Variables
        public override bool InstancePerEntity => true;

        public static bool MLSealTeleport = false;
        public const int TotalExtraAISlots = 100;

        // I'll be fucking damned if this isn't enough
        public float[] ExtraAI = new float[TotalExtraAISlots];
        public Vector2 angleTarget = default;
        public Rectangle arenaRectangle = default;
        public bool canTelegraph = false;
        public PrimitiveTrailCopy OptionalPrimitiveDrawer;

        public static int Cryogen = -1;
        public static int AstrumAureus = -1;

        #endregion

        #region Reset Effects
        public override void ResetEffects(NPC npc)
        {
            void ResetSavedIndex(ref int type, int type1, int type2 = -1)
            {
                if (type >= 0)
                {
                    if (!Main.npc[type].active)
                    {
                        type = -1;
                    }
                    else if (type2 == -1)
                    {
                        if (Main.npc[type].type != type1)
                            type = -1;
                    }
                    else
                    {
                        if (Main.npc[type].type != type1 && Main.npc[type].type != type2)
                            type = -1;
                    }
                }
            }

            ResetSavedIndex(ref Cryogen, ModContent.NPCType<CryogenNPC>());
            ResetSavedIndex(ref AstrumAureus, ModContent.NPCType<AstrumAureus>());
        }
        #endregion Reset Effects

        #region Overrides

        #region Get Alpha
        public override Color? GetAlpha(NPC npc, Color drawColor)
        {
            if (npc.type == NPCID.MoonLordHand ||
                npc.type == NPCID.MoonLordHead ||
                npc.type == NPCID.MoonLordCore)
            {
                if (PoDWorld.InfernumMode)
                    return new Color(7, 81, 81);
            }
            if (npc.type == ModContent.NPCType<CalamitasRun3>() && PoDWorld.InfernumMode)
            {
                bool brotherAlive = false;
                if (CalamityGlobalNPC.cataclysm != -1)
                {
                    if (Main.npc[CalamityGlobalNPC.cataclysm].active)
                    {
                        brotherAlive = true;
                    }
                }
                if (CalamityGlobalNPC.catastrophe != -1)
                {
                    if (Main.npc[CalamityGlobalNPC.catastrophe].active)
                    {
                        brotherAlive = true;
                    }
                }
                if (PoDWorld.InfernumMode && brotherAlive)
                    return new Color(100, 0, 0, 127);
            }
            return base.GetAlpha(npc, drawColor);
        }
        #endregion

        public override void BossHeadSlot(NPC npc, ref int index)
        {
            if (!PoDWorld.InfernumMode)
                return;

            if (npc.type == ModContent.NPCType<CryogenNPC>())
                index = ModContent.GetModBossHeadSlot("InfernumMode/BehaviorOverrides/BossAIs/Cryogen/CryogenMapIcon");
        }

        public override void SetDefaults(NPC npc)
        {
            angleTarget = default;
            for (int i = 0; i < ExtraAI.Length; i++)
                ExtraAI[i] = 0f;

            OptionalPrimitiveDrawer = null;

            if (InfernumMode.CanUseCustomAIs)
            {
                if (OverridingListManager.InfernumSetDefaultsOverrideList.ContainsKey(npc.type))
                    OverridingListManager.InfernumSetDefaultsOverrideList[npc.type].DynamicInvoke(npc);
            }
        }
        public override bool PreAI(NPC npc)
        {
            if (InfernumMode.CanUseCustomAIs)
            {
                // Correct an enemy's life depending on its cached true life value.
                if (InfernumNPCHPValues.HPValues.ContainsKey(npc.type) && InfernumNPCHPValues.HPValues[npc.type] >= 0 && InfernumNPCHPValues.HPValues[npc.type] != npc.lifeMax)
                {
                    npc.life = npc.lifeMax = InfernumNPCHPValues.HPValues[npc.type];

                    if (BossHealthBarManager.Bars.Any(b => b.NPCIndex == npc.whoAmI))
                        BossHealthBarManager.Bars.First(b => b.NPCIndex == npc.whoAmI).InitialMaxLife = npc.lifeMax;

                    npc.netUpdate = true;
                }

                // Make perf worms immune to debuffs.
                if (CalamityGlobalNPC.PerforatorIDs.Contains(npc.type))
                {
                    for (int k = 0; k < npc.buffImmune.Length; k++)
                        npc.buffImmune[k] = true;
                }

                if (OverridingListManager.InfernumNPCPreAIOverrideList.ContainsKey(npc.type))
                {
                    // Use timed DR if enabled.
                    if (npc.Calamity().KillTime > 0 && npc.Calamity().AITimer < npc.Calamity().KillTime)
                        npc.Calamity().AITimer++;

                    // Decrement each immune timer if it's greater than 0.
                    for (int i = 0; i < CalamityGlobalNPC.maxPlayerImmunities; i++)
                    {
                        if (npc.Calamity().dashImmunityTime[i] > 0)
                            npc.Calamity().dashImmunityTime[i]--;
                    }

                    return (bool)OverridingListManager.InfernumNPCPreAIOverrideList[npc.type].DynamicInvoke(npc);
                }
            }
            return base.PreAI(npc);
        }

        public override bool PreNPCLoot(NPC npc)
        {
            if (!InfernumMode.CanUseCustomAIs)
                return base.PreNPCLoot(npc);

            if (npc.type == NPCID.EaterofWorldsHead)
            {
                if (npc.realLife != -1 && Main.npc[npc.realLife].Infernum().ExtraAI[9] == 0f)
                {
                    Main.npc[npc.realLife].NPCLoot();
                    Main.npc[npc.realLife].Infernum().ExtraAI[9] = 1f;
                    return false;
                }

                if (npc.ai[2] >= 2f)
                {
                    npc.boss = true;
                    if (BossRushEvent.BossRushActive)
                        typeof(BossRushEvent).GetMethod("OnBossKill", Utilities.UniversalBindingFlags).Invoke(null, new object[] { npc, mod });
                }

                else if (npc.realLife == -1 && npc.Infernum().ExtraAI[10] == 0f)
                {
                    npc.Infernum().ExtraAI[10] = 1f;
                    EoWHeadBehaviorOverride.HandleSplit(npc, ref npc.ai[2]);
                }

                return npc.ai[2] >= 2f;
            }

            // Clear lightning.
            if (npc.type == NPCID.BrainofCthulhu)
            {
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    if (Main.projectile[i].type == ProjectileID.CultistBossLightningOrbArc || Main.projectile[i].type == ModContent.ProjectileType<PsionicOrb>())
                        Main.projectile[i].active = false;
                }
            }

            if (npc.type == NPCID.WallofFleshEye)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    for (int i = 0; i < Main.rand.Next(11, 15 + 1); i++)
                        Utilities.NewProjectileBetter(npc.Center, Main.rand.NextVector2CircularEdge(8f, 8f), ModContent.ProjectileType<CursedSoul>(), 55, 0f);
                    if (Main.npc.IndexInRange(Main.wof))
                        Main.npc[Main.wof].StrikeNPC(1550, 0f, 0);
                }

                return false;
            }

            if (npc.type == ModContent.NPCType<OldDukeNPC>())
                CalamityMod.CalamityMod.StopRain();

            return base.PreNPCLoot(npc);
        }

        public override void NPCLoot(NPC npc)
        {
            if (!InfernumMode.CanUseCustomAIs)
                return;
            
            if (npc.type == NPCID.WallofFlesh)
            {
                for (int i = 0; i < Main.rand.Next(18, 29 + 1); i++)
                {
                    int soul = Utilities.NewProjectileBetter(npc.Center, Main.rand.NextVector2CircularEdge(8f, 8f), ModContent.ProjectileType<CursedSoul>(), 55, 0f);
                    Main.projectile[soul].localAI[1] = Main.rand.NextBool().ToDirectionInt();
                }
            }

            if (npc.type == InfernumMode.CalamityMod.NPCType("DevourerofGodsHead"))
            {
                // Skip the sentinel phase entirely
                CalamityWorld.DoGSecondStageCountdown = 600;

                if (Main.netMode == NetmodeID.Server)
                {
                    var netMessage = InfernumMode.CalamityMod.GetPacket();
                    netMessage.Write((byte)14);
                    netMessage.Write(CalamityWorld.DoGSecondStageCountdown);
                    netMessage.Send();
                }
            }

            // Drop challenge drops.
            if (!CalamityWorld.malice)
            {
                if (ChallengeDropHandling.ChallengeDropTable.TryGetValue(npc.type, out int[] loot))
                {
                    if (npc.type == NPCID.Spazmatism)
                    {
                        if (!NPC.AnyNPCs(NPCID.Retinazer))
                        {
                            for (int i = 0; i < loot.Length; i++)
                                DropHelper.DropItem(npc, loot[i]);
                        }
                        return;
                    }
                    if (npc.type == NPCID.Retinazer)
                    {
                        if (!NPC.AnyNPCs(NPCID.Spazmatism))
                        {
                            for (int i = 0; i < loot.Length; i++)
                                DropHelper.DropItem(npc, loot[i]);
                        }
                        return;
                    }
                    if (npc.type == ModContent.NPCType<Siren>())
                    {
                        if (!NPC.AnyNPCs(ModContent.NPCType<Leviathan>()))
                        {
                            for (int i = 0; i < loot.Length; i++)
                                DropHelper.DropItem(npc, loot[i]);
                        }
                        return;
                    }
                    if (npc.type == ModContent.NPCType<Leviathan>())
                    {
                        if (!NPC.AnyNPCs(ModContent.NPCType<Siren>()))
                        {
                            for (int i = 0; i < loot.Length; i++)
                                DropHelper.DropItem(npc, loot[i]);
                        }
                        return;
                    }

                    for (int i = 0; i < loot.Length; i++)
                        DropHelper.DropItem(npc, loot[i]);
                }
            }
        }

        public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot)
        {
            if (npc.type == ModContent.NPCType<DevourerofGodsBody>())
            {
                cooldownSlot = 0;
                return npc.alpha == 0;
            }
            return base.CanHitPlayer(npc, target, ref cooldownSlot);
        }

        public override bool StrikeNPC(NPC npc, ref double damage, int defense, ref float knockback, int hitDirection, ref bool crit)
        {
            if (!InfernumMode.CanUseCustomAIs)
                return base.StrikeNPC(npc, ref damage, defense, ref knockback, hitDirection, ref crit);

            if (npc.type == InfernumMode.CalamityMod.NPCType("Yharon"))
            {
                if (npc.life - (int)Math.Ceiling(damage) <= 0)
                {
                    npc.NPCLoot();
                }
            }

            if (npc.type == NPCID.MoonLordHand || npc.type == NPCID.MoonLordHead)
            {
                if (damage > 1500)
                    damage = 1500;
                return false;
            }

            if (npc.type == NPCID.TheDestroyerBody)
                CalamityGlobalNPC.DestroyerIDs.Remove(NPCID.TheDestroyerBody);

            double realDamage = crit ? damage * 2 : damage;
            int life = npc.realLife > 0 ? Main.npc[npc.realLife].life : npc.life;
            if ((npc.type == ModContent.NPCType<DevourerofGodsHead>() || npc.type == ModContent.NPCType<DevourerofGodsBody>() || npc.type == ModContent.NPCType<DevourerofGodsTail>()) &&
                 life - realDamage <= npc.lifeMax * 0.6 && npc.Infernum().ExtraAI[33] == 0f)
            {
                damage = 0;
                npc.dontTakeDamage = true;
                npc.Infernum().ExtraAI[10] = 1f;
                return false;
            }

            if ((npc.type == ModContent.NPCType<DevourerofGodsHead>() || npc.type == ModContent.NPCType<DevourerofGodsBody>() || npc.type == ModContent.NPCType<DevourerofGodsTail>()) &&
                 life - realDamage <= 1000 && npc.Infernum().ExtraAI[33] == 1f)
            {
                damage = 0;
                npc.dontTakeDamage = true;
                if (npc.Infernum().ExtraAI[32] == 0f)
                {
                    Main.PlaySound(InfernumMode.CalamityMod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/DevourerSpawn"), npc.Center);
                    npc.Infernum().ExtraAI[32] = 1f;
                }
                return false;
            }

            return base.StrikeNPC(npc, ref damage, defense, ref knockback, hitDirection, ref crit);
        }

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            if (!InfernumMode.CanUseCustomAIs)
                return;

            bool isDesertScourge = npc.type == ModContent.NPCType<DesertScourgeHead>() || npc.type == ModContent.NPCType<DesertScourgeBody>();
            isDesertScourge |= npc.type == ModContent.NPCType<DesertScourgeTail>() || npc.type == ModContent.NPCType<DesertNuisanceHead>();
            isDesertScourge |= npc.type == ModContent.NPCType<DesertNuisanceBody>() || npc.type == ModContent.NPCType<DesertNuisanceTail>();
            bool isSplitEoW = npc.type == NPCID.EaterofWorldsBody && npc.realLife >= 0 && Main.npc[npc.realLife].ai[2] >= 1f;

            if (isDesertScourge && (projectile.type == ProjectileID.JestersArrow || projectile.type == ProjectileID.UnholyArrow || projectile.type == ProjectileID.WaterBolt))
                damage = (int)(damage * 0.45f);

            if (isDesertScourge && (projectile.penetrate == -1 || projectile.penetrate > 1))
                damage = (int)(damage * 0.425f);

            if (projectile.type == ProjectileID.Flare || projectile.type == ProjectileID.BlueFlare)
                damage = (int)(damage * 0.8f);

            if (isDesertScourge && (projectile.type == ProjectileID.Flare || projectile.type == ProjectileID.BlueFlare))
                damage = (int)(damage * 0.75f);

            if (npc.type == NPCID.KingSlime && (projectile.penetrate == -1 || projectile.penetrate > 1))
                damage = (int)(damage * 0.67f);

            if (isSplitEoW && (projectile.penetrate == -1 || projectile.penetrate > 1))
                damage = (int)(damage * 0.45f);

            if (projectile.type == ModContent.ProjectileType<AshenStalagmiteProj>() && projectile.Calamity().stealthStrike)
                damage = (int)(damage * 0.65f);

            if (npc.type == ModContent.NPCType<CrabulonIdle>() && projectile.type == ModContent.ProjectileType<SeafoamBubble>())
                damage = (int)(damage * 0.785f);

            if (npc.type == NPCID.EaterofWorldsBody && projectile.type == ModContent.ProjectileType<ScourgeoftheDesertProj>() && projectile.Calamity().stealthStrike)
                damage = (int)(damage * 0.75f);

            if ((npc.type == ModContent.NPCType<PerforatorBodyMedium>() ||
                npc.type == ModContent.NPCType<PerforatorBodyLarge>()) && (projectile.penetrate >= 2 || projectile.penetrate == -1))
            {
                damage = (int)(damage * 0.4f);
            }

            if ((npc.type == ModContent.NPCType<PerforatorBodySmall>() ||
                npc.type == ModContent.NPCType<PerforatorBodyLarge>()) && (projectile.type == ModContent.ProjectileType<InfernalKrisCinder>()))
            {
                damage = (int)(damage * 0.3f);
            }

            bool isInkCloud = projectile.type == ModContent.ProjectileType<InkCloud>() || projectile.type == ModContent.ProjectileType<InkCloud2>() || projectile.type == ModContent.ProjectileType<InkCloud3>();
            if (isInkCloud && (npc.type == ModContent.NPCType<SlimeSpawnCrimson3>() || npc.type == ModContent.NPCType<SlimeSpawnCorrupt2>()))
                damage = (int)(damage * 0.6f);
            if ((npc.type == ModContent.NPCType<SlimeSpawnCrimson3>() || npc.type == ModContent.NPCType<SlimeSpawnCorrupt2>()) &&
                (projectile.penetrate >= 2 || projectile.penetrate == -1))
            {
                damage = (int)(damage * 0.5f);
            }

            if (npc.type == NPCID.WallofFleshEye && (projectile.penetrate == -1 || projectile.penetrate > 1))
                damage = (int)(damage * 0.785);

            if (npc.type == NPCID.WallofFleshEye && projectile.type == ModContent.ProjectileType<TrackingDiskLaser>())
                damage = (int)(damage * 0.625);

            if (projectile.type == ModContent.ProjectileType<SporeBomb>() || projectile.type == ModContent.ProjectileType<LeafArrow>() || projectile.type == ModContent.ProjectileType<IcicleArrowProj>())
                damage = (int)(damage * 0.55);

            if (projectile.type == ModContent.ProjectileType<Corrocloud1>() ||
                projectile.type == ModContent.ProjectileType<Corrocloud2>() ||
                projectile.type == ModContent.ProjectileType<Corrocloud3>())
            {
                damage = (int)(damage * 0.65);
            }

            if ((projectile.type == ModContent.ProjectileType<Corrocloud1>() ||
                projectile.type == ModContent.ProjectileType<Corrocloud2>() ||
                projectile.type == ModContent.ProjectileType<Corrocloud3>()) && npc.type == ModContent.NPCType<Leviathan>())
            {
                damage = (int)(damage * 0.55);
            }

            if ((projectile.type == ModContent.ProjectileType<Corrocloud1>() ||
                projectile.type == ModContent.ProjectileType<Corrocloud2>() ||
                projectile.type == ModContent.ProjectileType<Corrocloud3>()) && npc.type == ModContent.NPCType<AstrumAureus>())
            {
                damage = (int)(damage * 0.425);
            }

            bool isPhantasmDragon = npc.type == NPCID.CultistDragonBody1 || npc.type == NPCID.CultistDragonBody2 || npc.type == NPCID.CultistDragonBody3 || npc.type == NPCID.CultistDragonBody4 || npc.type == NPCID.CultistDragonTail;
            if (isPhantasmDragon && (projectile.penetrate == -1 || projectile.penetrate > 1))
                damage = (int)(damage * 0.24);

            if (npc.type == ModContent.NPCType<DevourerofGodsHead>() || npc.type == ModContent.NPCType<DevourerofGodsTail>() &&
                (projectile.type == ModContent.ProjectileType<PhantasmalRuinGhost>() || projectile.type == ModContent.ProjectileType<PhantasmalRuinProj>()) || projectile.type == ProjectileID.LostSoulFriendly)
            {
                damage = (int)(damage * 0.6);
            }

            if (npc.type == ModContent.NPCType<DevourerofGodsBody>() && (projectile.type == ProjectileID.MoonlordBullet || projectile.type == ProjectileID.MoonlordArrow || projectile.type == ProjectileID.MoonlordArrowTrail))
                damage = (int)(damage * 0.45);

            if (npc.type == InfernumMode.CalamityMod.NPCType("Providence") && projectile.minion && projectile.type == ModContent.ProjectileType<HolyFireBulletProj>())
                damage = (int)(damage * 0.6);
            
            if (npc.type == ModContent.NPCType<SupremeCalamitas>() && projectile.type == ModContent.ProjectileType<InfernadoFriendly>())
                damage = (int)(damage * 0.55);
        }

        public override void BossHeadRotation(NPC npc, ref float rotation)
        {
            if (!InfernumMode.CanUseCustomAIs)
                return;

            if (npc.type == ModContent.NPCType<DevourerofGodsHead>() ||
                npc.type == ModContent.NPCType<DevourerofGodsTail>() ||
                npc.type == ModContent.NPCType<DevourerofGodsBody>())
            {
                if (npc.Opacity < 0.1f)
                    rotation = float.NaN;
            }

            if (npc.type == ModContent.NPCType<Siren>())
            {
                if (npc.Opacity < 0.1f)
                    rotation = float.NaN;
            }

            if (npc.type == ModContent.NPCType<Signus>())
                rotation = float.NaN;

            // Prevent Yharon from showing himself amongst his illusions in Subphase 10.
            if (npc.type == ModContent.NPCType<YharonNPC>())
            {
                if (npc.life / (float)npc.lifeMax <= 0.05f && npc.Infernum().ExtraAI[2] == 1f)
                    rotation = float.NaN;
            }
        }

        public override bool? DrawHealthBar(NPC npc, byte hbPosition, ref float scale, ref Vector2 position)
        {
            if (!InfernumMode.CanUseCustomAIs)
                return base.DrawHealthBar(npc, hbPosition, ref scale, ref position);

            return base.DrawHealthBar(npc, hbPosition, ref scale, ref position);
        }

        public override bool CheckDead(NPC npc)
        {
            if (!InfernumMode.CanUseCustomAIs)
                return base.CheckDead(npc);

            if (npc.type == ModContent.NPCType<DevourerofGodsHead>())
            {
                npc.life = 1;
                npc.dontTakeDamage = true;
                if (npc.Infernum().ExtraAI[20] == 0f)
                {
                    Main.PlaySound(InfernumMode.CalamityMod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/DevourerSpawn"), npc.Center);
                    npc.Infernum().ExtraAI[20] = 1f;
                }
                npc.active = true;
                npc.netUpdate = true;
                return false;
            }

            if (npc.type == ModContent.NPCType<PolterghastNPC>())
            {
                if (npc.Infernum().ExtraAI[6] > 0f)
                    return true;

                npc.Infernum().ExtraAI[6] = 1f;
                npc.life = 1;
                npc.netUpdate = true;
                npc.dontTakeDamage = true;

                return false;
            }

            if (npc.type == ModContent.NPCType<Bumblefuck2>())
            {
                if (npc.ai[0] != 3f && npc.ai[3] > 0f)
                {
                    npc.life = npc.lifeMax;
                    npc.dontTakeDamage = true;
                    npc.ai[0] = 3f;
                    npc.ai[1] = 0f;
                    npc.ai[2] = 0f;
                    npc.netUpdate = true;
                }
                return false;
            }

            if ((npc.type == NPCID.Spazmatism || npc.type == NPCID.Retinazer))
            {
                bool otherTwinHasCreatedShield = false;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    if (!Main.npc[i].active)
                        continue;
                    if (Main.npc[i].type != NPCID.Retinazer && Main.npc[i].type != NPCID.Spazmatism)
                        continue;
                    if (Main.npc[i].type == npc.type)
                        continue;

                    if (Main.npc[i].Infernum().ExtraAI[3] == 1f)
                    {
                        otherTwinHasCreatedShield = true;
                        break;
                    }
                }

                if (npc.Infernum().ExtraAI[3] == 0f && !otherTwinHasCreatedShield)
                {
                    npc.life = 1;
                    npc.active = true;
                    npc.netUpdate = true;
                    npc.dontTakeDamage = true;
                    return false;
                }
            }

            if (npc.type == ModContent.NPCType<RavagerClawLeft>() || npc.type == ModContent.NPCType<RavagerClawRight>())
            {
                npc.Infernum().ExtraAI[0] = 1f;
                npc.netUpdate = true;
                npc.active = true;
                npc.dontTakeDamage = true;
                npc.life = npc.lifeMax;

                // Synchronize the other claw, if it too has been released.
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC checkNPC = Main.npc[i];
                    bool correctNPC = checkNPC.type == ModContent.NPCType<RavagerClawLeft>() || checkNPC.type == ModContent.NPCType<RavagerClawRight>();
                    if (!correctNPC || !checkNPC.active || checkNPC.Infernum().ExtraAI[0] != 1f)
                        continue;

                    checkNPC.ai[0] = 2f;
                    checkNPC.ai[1] = 0f;
                    checkNPC.netUpdate = true;
                }

                return false;
            }

            if (npc.type == NPCID.CultistBoss)
            {
                CultistBehaviorOverride.ClearAwayEntities();
                npc.Infernum().ExtraAI[6] = 1f;
                npc.netUpdate = true;
                npc.active = true;
                npc.dontTakeDamage = true;
                npc.life = 1;

                Main.PlaySound(SoundID.NPCDeath59, npc.Center);

                return false;
            }

            if (npc.type == ModContent.NPCType<SupremeCalamitas>())
            {
                npc.netUpdate = true;
                npc.active = true;
                npc.dontTakeDamage = true;
                npc.life = 1;

                return false;
            }

            if (ExoMechManagement.FindFinalMech() == npc)
                ExoMechManagement.MakeDraedonSayThings(4);
            else if (ExoMechManagement.TotalMechs - 1 == 1)
                ExoMechManagement.MakeDraedonSayThings(5);

            return base.CheckDead(npc);
        }

        public override bool CheckActive(NPC npc)
        {
            if (!InfernumMode.CanUseCustomAIs)
                return base.CheckActive(npc);

            if (npc.type == NPCID.KingSlime)
                return false;
            if (npc.type == NPCID.SkeletronHand)
                return false;
            if (npc.type == ModContent.NPCType<GreatSandShark>())
                return false;
            if (npc.type == NPCID.AncientCultistSquidhead)
                return false;
            if (npc.type == NPCID.MoonLordFreeEye)
                return false;

            return base.CheckActive(npc);
        }

        public override void OnHitPlayer(NPC npc, Player target, int damage, bool crit)
        {
            if (!PoDWorld.InfernumMode)
                return;

            if (npc.type == ModContent.NPCType<CrabulonIdle>())
            {
                target.AddBuff(BuffID.Poisoned, 180);
                target.AddBuff(ModContent.BuffType<ArmorCrunch>(), 180);
            }
            if (npc.type == ModContent.NPCType<CrimulanSGBig>() || npc.type == ModContent.NPCType<SlimeSpawnCrimson3>())
                target.AddBuff(ModContent.BuffType<BurningBlood>(), 240);
            if (npc.type == ModContent.NPCType<EbonianSGBig>() || npc.type == ModContent.NPCType<SlimeSpawnCorrupt2>())
                target.AddBuff(ModContent.BuffType<Shadowflame>(), 240);

            if (npc.type == ModContent.NPCType<SlimeGodCore>())
            {
                target.AddBuff(ModContent.BuffType<BurningBlood>(), 120);
                target.AddBuff(ModContent.BuffType<Shadowflame>(), 120);
                target.AddBuff(BuffID.Slimed, 240);
                target.AddBuff(BuffID.Slow, 240);
            }
            if (npc.type == NPCID.Retinazer && !NPC.AnyNPCs(NPCID.Spazmatism))
                target.AddBuff(ModContent.BuffType<RedSurge>(), 180);
            if (npc.type == NPCID.Spazmatism && !NPC.AnyNPCs(NPCID.Retinazer))
                target.AddBuff(ModContent.BuffType<ShadowflameInferno>(), 180);
        }

        #endregion
    }
}