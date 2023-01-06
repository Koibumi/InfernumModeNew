using InfernumMode.Common.BaseEntities;
using Microsoft.Xna.Framework;
using Terraria;

namespace InfernumMode.Content.Projectiles
{
    public class ProfanedTempleCinder : BaseCinderProjectile
    {
        public override void SetStaticDefaults() => DisplayName.SetDefault("Profaned Cinder");

        public override Color? GetAlpha(Color lightColor) => (Main.dayTime ? Color.White : Color.Cyan) * Projectile.Opacity;
    }
}