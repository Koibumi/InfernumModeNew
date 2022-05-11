using CalamityMod.Dusts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Providence
{
    public class HolyCinder : ModProjectile
    {
        public ref float Time => ref Projectile.ai[0];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Holy Cinder");
            Main.projFrames[Projectile.type] = 4;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 3;
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 10000;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 16;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 360;
        }

        public override void AI()
        {
            if (Projectile.velocity.Length() < 25f && Time >= 25f)
                Projectile.velocity *= 1.035f;

            Projectile.frameCounter++;
            Projectile.frame = Projectile.frameCounter / 5 % Main.projFrames[Projectile.type];
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
            Projectile.tileCollide = Time > 30f;
            Time++;
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 10; i++)
            {
                Dust holyFire = Dust.NewDustPerfect(Projectile.Center, (int)CalamityDusts.ProfanedFire);
                holyFire.velocity = Main.rand.NextVector2Circular(14f, 14f);
                holyFire.scale = 1.7f;
                holyFire.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float telegraphInterpolant = Utils.GetLerpValue(0f, 45f, Time, true);
            if (telegraphInterpolant >= 1f)
                telegraphInterpolant = 0f;

            Main.spriteBatch.DrawLineBetter(Projectile.Center, Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitY) * 6000f, Color.Yellow * telegraphInterpolant, telegraphInterpolant * 3f);
            lightColor = Color.Lerp(lightColor, Color.White, 0.4f);
            lightColor.A = 128;
            Utilities.DrawAfterimagesCentered(Projectile, lightColor, ProjectileID.Sets.TrailingMode[Projectile.type]);
            return false;
        }

        public override bool? CanDamage() => Projectile.alpha < 20 ? null : false;
    }
}