﻿using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.NPCs.SunkenSea;
using CalamityMod.Projectiles.Enemy;
using InfernumMode.OverridingSystem;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using GiantClamNPC = CalamityMod.NPCs.SunkenSea.GiantClam;

namespace InfernumMode.FuckYouModeAIs.GiantClam
{
    public class GiantClamBehaviorOverride : NPCBehaviorOverride
    {
        public enum GiantClamAttackState
        {
            PearlSwirl = 0,
            PearlRain = 1,
            TeleportSlam = 2,
        }

        public const int HitsRequiredToAnger = 5;

        public override int NPCOverrideType => ModContent.NPCType<GiantClamNPC>();

        public override NPCOverrideContext ContentToOverride => NPCOverrideContext.NPCAI | NPCOverrideContext.NPCFindFrame;

        public override bool PreAI(NPC npc)
        {
            ref float attackState = ref npc.Infernum().ExtraAI[0];
            ref float attackTimer = ref npc.Infernum().ExtraAI[1];
            ref float hidingInShell = ref npc.Infernum().ExtraAI[2];
            ref float hitCount = ref npc.ai[0];
            bool hardmode = Main.hardMode;
           

            if (hitCount < HitsRequiredToAnger)
            {
                if (npc.justHit)
                    hitCount++;

                npc.chaseable = false;
                npc.defense = 9999;
                return false;
            }
            else if (hitCount == HitsRequiredToAnger)
            {
                hitCount++;
                npc.defense = 15;
                npc.damage = 150;
                if (Main.hardMode)
                {
                    npc.defense = 35;
                    npc.damage = 250;
                }

                if (hidingInShell == 1f)
                    npc.defense *= 100;

                npc.defDamage = npc.damage;
                npc.defDefense = npc.defense;

                npc.chaseable = true;

                npc.netUpdate = true;
            }

            npc.TargetClosest(true);
            Player target = Main.player[npc.target];

            switch ((GiantClamAttackState)(int)attackState)
            {
                case GiantClamAttackState.PearlSwirl:

                    if (attackTimer > 180)
                        hidingInShell = 1f;

                    if (attackTimer == 0 || attackTimer == 180)
                    {
                        int projectileType = ModContent.ProjectileType<PearlSwirl>();
                        Utilities.NewProjectileBetter(npc.Center, new Vector2(10f, 0f), projectileType, 150, 0f);
                        Utilities.NewProjectileBetter(npc.Center, new Vector2(0f, 10f), projectileType, 150, 0f);
                        Utilities.NewProjectileBetter(npc.Center, new Vector2(-10f, 0f), projectileType, 150, 0f);
                        Utilities.NewProjectileBetter(npc.Center, new Vector2(0f, -10f), projectileType, 150, 0f);
                        if (hardmode)
                        {
                            int one = Utilities.NewProjectileBetter(npc.Center, new Vector2(10f, 0f), projectileType, 150, 0f);
                            int two = Utilities.NewProjectileBetter(npc.Center, new Vector2(0f, 10f), projectileType, 150, 0f);
                            int three = Utilities.NewProjectileBetter(npc.Center, new Vector2(-10f, 0f), projectileType, 150, 0f);
                            int four = Utilities.NewProjectileBetter(npc.Center, new Vector2(0f, -10f), projectileType, 150, 0f);
                            int[] reverses = { one, two, three, four };
                            foreach (int proj in reverses)
                            {
                                if (Main.projectile.IndexInRange(proj))
                                    Main.projectile[proj].ai[0] = 1f;
                            }
                        }
                    }

                    attackTimer++;
                    if (attackTimer >= 240)
                        GoToNextAttack(npc);
                    break;

                case GiantClamAttackState.PearlRain:
                    if (attackTimer == 0f)
                    {
                        Main.PlaySound(SoundID.Item67, npc.position);
                        for (float offset = -750f; offset < 750f; offset += 150f)
                        {
                            Vector2 spawnPosition = target.Center + new Vector2(offset, -750f);
                            Vector2 pearlShootVelocity = Vector2.UnitY * 8f;
                            Utilities.NewProjectileBetter(spawnPosition, pearlShootVelocity, ModContent.ProjectileType<PearlRain>(), npc.damage, 0f, Main.myPlayer, 0f, 0f);
                        }
                        for (float offset = -675f; offset < 825f; offset += 150f)
                        {
                            Vector2 spawnPosition = target.Center + new Vector2(offset, 750f);
                            Vector2 pearlShootVelocity = Vector2.UnitY * -8f;
                            Utilities.NewProjectileBetter(spawnPosition, pearlShootVelocity, ModContent.ProjectileType<PearlRain>(), npc.damage, 0f, Main.myPlayer, 0f, 0f);
                        }
                    }
                    if (hardmode)
                    {
                        if (attackTimer == 90f)
                        {
                            for (float offset = -750f; offset < 750f; offset += 200f)
                            {
                                Vector2 spawnPosition = target.Center + new Vector2(-950f, offset);
                                Vector2 pearlShootVelocity = Vector2.UnitX * 8f;
                                Utilities.NewProjectileBetter(spawnPosition, pearlShootVelocity, ModContent.ProjectileType<PearlRain>(), npc.damage, 0f, Main.myPlayer, 0f, 0f);
                            }
                            for (float offset = -675f; offset < 825f; offset += 200f)
                            {
                                Vector2 spawnPosition = target.Center + new Vector2(950f, offset);
                                Vector2 pearlShootVelocity = Vector2.UnitX * -8f;
                                Utilities.NewProjectileBetter(spawnPosition, pearlShootVelocity, ModContent.ProjectileType<PearlRain>(), npc.damage, 0f, Main.myPlayer, 0f, 0f);
                            }
                        }
                    }
                    if (attackTimer >= 180)
                        hidingInShell = 1f;
                    attackTimer++;
                    if (attackTimer >= 210)
                        GoToNextAttack(npc);
                    break;

                case GiantClamAttackState.TeleportSlam:
                    ref float attackSubstate = ref npc.Infernum().ExtraAI[3];
                    ref float slamCount = ref npc.Infernum().ExtraAI[4];
                    if (attackTimer == 1f)
                        attackSubstate = 1f;

                    if (attackSubstate == 1f)
                    {
                        npc.alpha += 20;

                        npc.noGravity = true;
                        npc.noTileCollide = true;

                        if (npc.alpha >= 255)
                        {
                            npc.alpha = 255;
                            npc.position.X = target.position.X - 60f;
                            npc.position.Y = target.position.Y - 400f;

                            attackSubstate = 2f;
                        }
                    }
                    else if (attackSubstate == 2f)
                    {
                        if (slamCount < 1)
                            npc.alpha -= 6;
                        else
                            npc.alpha -= 16;

                        if (npc.alpha <= 0)
                        {
                            npc.alpha = 0;
                            attackSubstate = 3f;
                        }
                    }
                    else if (attackSubstate == 3f)
                    {
                        if (npc.Center.Y > target.position.Y + 100 || npc.noTileCollide == false)
                        {
                            npc.noTileCollide = false;

                            if (npc.velocity.Y == 0f)
                            {
                                Main.PlaySound(InfernumMode.CalamityMod.GetLegacySoundSlot(SoundType.Item, "Sounds/Item/ClamImpact"), (int)npc.position.X, (int)npc.position.Y);
                                slamCount++;
                                
                                if (slamCount < (hardmode ? 6f : 3f))
                                {
                                    attackTimer = 0f;
                                    attackSubstate = 1f;
                                }
                                else
                                    attackSubstate = 4f;
                            }
                            else
                                npc.velocity.Y += 1f;
                        }
                        else
                            npc.velocity.Y += 1f;
                    }

                    attackTimer++;

                    if (attackSubstate == 4f)
                    {
                        attackSubstate = 0f;
                        slamCount = 0f;
                        GoToNextAttack(npc);
                    }

                    break;
            }

            if (hidingInShell == 0)
                Lighting.AddLight(npc.Center, 0f, npc.Opacity * 2.5f, npc.Opacity * 2.5f);

            return false;
            
        }

        public static void GoToNextAttack(NPC npc)
        {
            GiantClamAttackState CurrentAttack = (GiantClamAttackState)(int)npc.Infernum().ExtraAI[0];
            GiantClamAttackState NextAttack = CurrentAttack;

            while (NextAttack == CurrentAttack)
                NextAttack = (GiantClamAttackState)new Random().Next(0, Enum.GetNames(typeof(GiantClamAttackState)).Length);

            npc.Infernum().ExtraAI[0] = (float)NextAttack;
            npc.Infernum().ExtraAI[1] = 0f;

            switch (NextAttack)
            {
                case GiantClamAttackState.PearlSwirl:
                case GiantClamAttackState.PearlRain:
                case GiantClamAttackState.TeleportSlam:
                    npc.Infernum().ExtraAI[2] = 0f;
                    break;
            }
        }

        public override void FindFrame(NPC npc, int frameHeight)
        {
            ref float hitCount = ref npc.ai[0];
            ref float hidingInShell = ref npc.Infernum().ExtraAI[2];
            ref float attackState = ref npc.Infernum().ExtraAI[0];
            ref float attackTimer = ref npc.Infernum().ExtraAI[1];
            ref float attackSubstate = ref npc.Infernum().ExtraAI[3];

            npc.frame.Y = (int)Math.Floor(npc.frameCounter / 5) * frameHeight;
            npc.frameCounter++;
            if (npc.frameCounter > 24)
                npc.frameCounter = 0;

            if (hitCount < HitsRequiredToAnger || hidingInShell == 1f)
                npc.frame.Y = frameHeight * 11;

            if (attackState == (float)GiantClamAttackState.PearlSwirl)
            {
               if (attackTimer > 180)
                    npc.frame.Y = ((int)MathHelper.Clamp(attackTimer - 180f, 0f, 6f) + 5) * frameHeight;
            }

            else if (attackState == (float)GiantClamAttackState.PearlRain)
            {
                if (attackTimer > 180)
                    npc.frame.Y = ((int)MathHelper.Clamp(attackTimer - 180f, 0f, 6f) + 5) * frameHeight;
            }

            else if (attackState == (float)GiantClamAttackState.TeleportSlam)
            {
                if (attackSubstate == 1)
                    npc.frame.Y = ((int)MathHelper.Clamp(attackTimer, 0f, 6f) + 5) * frameHeight;
            }
        }
    }
}
