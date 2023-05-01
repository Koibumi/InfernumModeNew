using CalamityMod;
using CalamityMod.NPCs.AquaticScourge;
using CalamityMod.NPCs.AstrumAureus;
using CalamityMod.NPCs.AstrumDeus;
using CalamityMod.NPCs.DevourerofGods;
using CalamityMod.NPCs.ExoMechs.Ares;
using CalamityMod.NPCs.ExoMechs.Thanatos;
using CalamityMod.NPCs.Perforator;
using CalamityMod.NPCs.ProfanedGuardians;
using CalamityMod.NPCs.Ravager;
using CalamityMod.NPCs.SupremeCalamitas;
using CalamityMod.Projectiles.DraedonsArsenal;
using CalamityMod.Projectiles.Magic;
using CalamityMod.Projectiles.Melee;
using CalamityMod.Projectiles.Ranged;
using CalamityMod.Projectiles.Rogue;
using InfernumMode.Content.BehaviorOverrides.BossAIs.Draedon.Ares;
using InfernumMode.Content.BehaviorOverrides.BossAIs.ProfanedGuardians;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace InfernumMode.Core.Balancing
{
    public static class BalancingChangesManager
    {
        internal static List<IBalancingRule[]> UniversalBalancingChanges = null;
        internal static List<NPCBalancingChange> NPCSpecificBalancingChanges = null;

        public const float AdrenalineChargeTimeFactor = 1.6f;

        public const int DashDelay = 15;

        internal static void Load()
        {
            int corrosiveSpineCloud1 = ProjectileType<Corrocloud1>();
            int corrosiveSpineCloud2 = ProjectileType<Corrocloud2>();
            int corrosiveSpineCloud3 = ProjectileType<Corrocloud3>();

            UniversalBalancingChanges = new List<IBalancingRule[]>()
            {
                Do(new ProjectileResistBalancingRule(0.8f, ProjectileID.Flare, ProjectileID.BlueFlare)),
                Do(new StealthStrikeBalancingRule(0.65f, ProjectileType<AshenStalagmiteProj>())),
                Do(new ProjectileResistBalancingRule(0.55f, ProjectileType<SporeBomb>(), ProjectileType<LeafArrow>(), ProjectileType<IcicleArrowProj>())),
                Do(new ProjectileResistBalancingRule(0.25f, corrosiveSpineCloud1, corrosiveSpineCloud2, corrosiveSpineCloud3)),
            };

            var eowIsSplitRequirement = new NPCSpecificRequirementBalancingRule(n => n.type == NPCID.EaterofWorldsBody && n.realLife >= 0 && Main.npc[n.realLife].ai[2] >= 1f);

            int inkCloud1 = ProjectileType<InkCloud>();
            int inkCloud2 = ProjectileType<InkCloud2>();
            int inkCloud3 = ProjectileType<InkCloud3>();

            float aresPierceResistFactor = 0.925f;
            float sepulcherPierceResistFactor = 0.375f;

            NPCSpecificBalancingChanges = new List<NPCBalancingChange>()
            {
                // King Slime.
                new NPCBalancingChange(NPCID.KingSlime, new PierceResistBalancingRule(0.67f)),

                // Eater of Worlds.
                new NPCBalancingChange(NPCID.EaterofWorldsBody, Do(eowIsSplitRequirement, new PierceResistBalancingRule(0.45f))),
                new NPCBalancingChange(NPCID.EaterofWorldsBody, Do(new PierceResistBalancingRule(0.4f))),

                // Perforators.
                new NPCBalancingChange(NPCType<PerforatorBodySmall>(), Do(new PierceResistBalancingRule(0.4f), new ProjectileResistBalancingRule(0.3f, ProjectileType<InfernalKrisCinder>()))),
                new NPCBalancingChange(NPCType<PerforatorBodyMedium>(), Do(new PierceResistBalancingRule(0.4f))),
                new NPCBalancingChange(NPCType<PerforatorBodyLarge>(), Do(new PierceResistBalancingRule(0.4f), new ProjectileResistBalancingRule(0.3f, ProjectileType<InfernalKrisCinder>()))),

                // Wall of Flesh.
                new NPCBalancingChange(NPCID.WallofFleshEye, Do(new PierceResistBalancingRule(0.785f))),
                new NPCBalancingChange(NPCID.WallofFleshEye, Do(new PierceResistBalancingRule(0.785f))),

                // Queen Slime.
                new NPCBalancingChange(NPCID.QueenSlimeBoss, Do(new ProjectileResistBalancingRule(0.4f, ProjectileType<SeasSearingSpout>()))),

                // Aquatic Scourge.
                new NPCBalancingChange(NPCType<AquaticScourgeBody>(), Do(new PierceResistBalancingRule(0.45f))),
                new NPCBalancingChange(NPCType<AquaticScourgeBodyAlt>(), Do(new PierceResistBalancingRule(0.45f))),

                // Astrum Aureus.
                new NPCBalancingChange(NPCType<AstrumAureus>(), Do(new PierceResistBalancingRule(0.67f))),

                // Ravager.
                new NPCBalancingChange(NPCType<RavagerLegLeft>(), Do(new PierceResistBalancingRule(0.4f))),
                new NPCBalancingChange(NPCType<RavagerLegRight>(), Do(new PierceResistBalancingRule(0.4f))),
                new NPCBalancingChange(NPCType<RavagerHead>(), Do(new PierceResistBalancingRule(0.4f))),

                // Cultist.
                new NPCBalancingChange(NPCID.CultistDragonBody1, Do(new PierceResistBalancingRule(0.1f))),
                new NPCBalancingChange(NPCID.CultistDragonBody2, Do(new PierceResistBalancingRule(0.1f))),
                new NPCBalancingChange(NPCID.CultistDragonBody3, Do(new PierceResistBalancingRule(0.1f))),
                new NPCBalancingChange(NPCID.CultistDragonBody4, Do(new PierceResistBalancingRule(0.1f))),

                // Astrum Deus.
                new NPCBalancingChange(NPCType<AstrumDeusBody>(), Do(new PierceResistBalancingRule(0.35f))),
                new NPCBalancingChange(NPCType<AstrumDeusBody>(), Do(new ProjectileResistBalancingRule(0.00000001f, ProjectileType<TenebreusTidesWaterProjectile>(), ProjectileType<TenebreusTidesWaterSpear>()))),

                // Guardians. Summoner completely melted them at ~50% faster killtimes which is not ok.
                new NPCBalancingChange(NPCType<ProfanedGuardianCommander>(), Do(new ClassResistBalancingRule(0.654424808f, ClassType.Summon))),
                new NPCBalancingChange(NPCType<ProfanedGuardianDefender>(), Do(new ClassResistBalancingRule(0.654424808f, ClassType.Summon))),
                new NPCBalancingChange(NPCType<ProfanedGuardianHealer>(), Do(new ClassResistBalancingRule(0.654424808f, ClassType.Summon))),
                new NPCBalancingChange(NPCType<HealerShieldCrystal>(), Do(new ClassResistBalancingRule(0.654424808f, ClassType.Summon))),

                // The Devourer of Gods.
                new NPCBalancingChange(NPCType<DevourerofGodsBody>(), Do(new ProjectileResistBalancingRule(0.5f, ProjectileType<TerrorBeam>(), ProjectileType<TerrorBlast>()))),
                new NPCBalancingChange(NPCType<DevourerofGodsHead>(), Do(new ProjectileResistBalancingRule(0.5f, ProjectileType<TerrorBeam>(), ProjectileType<TerrorBlast>()))),

                // Exo Mechs.
                new NPCBalancingChange(NPCType<AresBody>(), Do(new PierceResistBalancingRule(aresPierceResistFactor))),
                new NPCBalancingChange(NPCType<AresLaserCannon>(), Do(new PierceResistBalancingRule(aresPierceResistFactor))),
                new NPCBalancingChange(NPCType<AresPlasmaFlamethrower>(), Do(new PierceResistBalancingRule(aresPierceResistFactor))),
                new NPCBalancingChange(NPCType<AresTeslaCannon>(), Do(new PierceResistBalancingRule(aresPierceResistFactor))),
                new NPCBalancingChange(NPCType<AresGaussNuke>(), Do(new PierceResistBalancingRule(aresPierceResistFactor))),
                new NPCBalancingChange(NPCType<AresPulseCannon>(), Do(new PierceResistBalancingRule(aresPierceResistFactor))),
                new NPCBalancingChange(NPCType<ThanatosBody1>(), Do(new ProjectileResistBalancingRule(0.2f, ProjectileType<WavePounderBoom>()))),
                new NPCBalancingChange(NPCType<ThanatosBody2>(), Do(new ProjectileResistBalancingRule(0.2f, ProjectileType<WavePounderBoom>()))),
                new NPCBalancingChange(NPCType<ThanatosBody1>(), Do(new ProjectileResistBalancingRule(0.45f, ProjectileType<DragonRageStaff>()))),
                new NPCBalancingChange(NPCType<ThanatosBody2>(), Do(new ProjectileResistBalancingRule(0.45f, ProjectileType<DragonRageStaff>()))),
                new NPCBalancingChange(NPCType<ThanatosBody1>(), Do(new ProjectileResistBalancingRule(0.4f, ProjectileType<DragonRageFireball>()))),
                new NPCBalancingChange(NPCType<ThanatosBody2>(), Do(new ProjectileResistBalancingRule(0.4f, ProjectileType<DragonRageFireball>()))),

                // Supreme Calamitas.
                new NPCBalancingChange(NPCType<SepulcherBody>(), Do(new PierceResistBalancingRule(sepulcherPierceResistFactor))),
                new NPCBalancingChange(NPCType<SepulcherBodyEnergyBall>(), Do(new PierceResistBalancingRule(sepulcherPierceResistFactor))),
                new NPCBalancingChange(NPCType<SoulSeekerSupreme>(), Do(new TrueMeleeBalancingRule(0.45f))),
                new NPCBalancingChange(NPCType<SupremeCalamitas>(), Do(new ProjectileResistBalancingRule(0.55f, ProjectileType<InfernadoFriendly>()))),
            };
        }

        internal static void Unload()
        {
            UniversalBalancingChanges = null;
            NPCSpecificBalancingChanges = null;
        }

        public static void ApplyFromProjectile(NPC npc, ref int damage, Projectile proj)
        {
            NPCHitContext hitContext = NPCHitContext.ConstructFromProjectile(proj);

            // Apply universal balancing rules.
            foreach (IBalancingRule[] balancingRules in UniversalBalancingChanges)
            {
                foreach (IBalancingRule balancingRule in balancingRules)
                {
                    if (balancingRule.AppliesTo(npc, hitContext))
                        balancingRule.ApplyBalancingChange(npc, ref damage);
                }
            }

            // As well as rules specific to NPCs.
            foreach (NPCBalancingChange balanceChange in NPCSpecificBalancingChanges)
            {
                if (npc.type != balanceChange.NPCType)
                    continue;

                foreach (IBalancingRule balancingRule in balanceChange.BalancingRules)
                {
                    if (balancingRule.AppliesTo(npc, hitContext))
                        balancingRule.ApplyBalancingChange(npc, ref damage);
                }
            }
        }

        // This function simply concatenates a bunch of balancing rules into an array.
        // It looks a lot nicer than constantly typing "new IBalancingRule[]".
        internal static IBalancingRule[] Do(params IBalancingRule[] r) => r;
    }
}