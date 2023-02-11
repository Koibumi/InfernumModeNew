﻿using CalamityMod;
using InfernumMode.Assets.Effects;
using InfernumMode.Assets.ExtraTextures;
using InfernumMode.Common.Graphics;
using InfernumMode.Content.Projectiles.Wayfinder;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;

namespace InfernumMode.Content.BehaviorOverrides.BossAIs.ProfanedGuardians
{
    public class LavaEruptionPillar : ModProjectile, IPixelPrimitiveDrawer
    {
        public PrimitiveTrailCopy LavaDrawer { get; private set; } = null;

        public PrimitiveTrailCopy TelegraphDrawer { get; private set; } = null;

        public static int Lifetime => (int)(180 + TelegraphLength);

        public override string Texture => InfernumTextureRegistry.InvisPath;

        public static float MaxLength => 4000f;

        public static float TelegraphLength => 30;

        public ref float Timer => ref Projectile.ai[0];

        public ref float CurrentLength => ref Projectile.ai[1];

        public ref float StretchOffset => ref Projectile.localAI[0];

        public float Width => Projectile.width * 40f;

        public float VariableWidth => Width * Utilities.EaseInOutCubic(CurrentLength / MaxLength);

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Fire Wall");
        }

        public override void SetDefaults()
        {
            Projectile.width = 4;
            Projectile.height = 4;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Lifetime;
            CooldownSlot = 1;
        }

        public override void AI()
        {
            if (StretchOffset == 0)
                StretchOffset = Main.rand.NextFloat(-0.1f, 0.1f);
            if (Timer >= TelegraphLength - 20)
                CurrentLength = MaxLength * MathF.Sin((Timer - TelegraphLength - 10) / (Lifetime - TelegraphLength - 10) * MathF.PI);
            Timer++;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 start = Projectile.Center;
            Vector2 end = start - Vector2.UnitY * CurrentLength;
            float _ = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, VariableWidth, ref _);
        }

        public float WidthFunction(float completionRatio) => /*Timer < Lifetime / 2f ? Width :*/ VariableWidth;

        public static Color ColorFunction(float completionRatio)
        {
            float interpolant = (1f + MathF.Sin(Main.GlobalTimeWrappedHourly * 2f)) / 2f;
            float colorInterpolant = MathHelper.Lerp(0.25f, 0.35f, interpolant);
            return Color.Lerp(Color.OrangeRed, Color.Gold, colorInterpolant);
        }

        public float TelegraphWidthFunction(float completionRatio) => Width * 0.75f;

        public static Color TelegraphColorFunction(float completionRatio)
        {
            Color orange = Color.Lerp(Color.OrangeRed, WayfinderSymbol.Colors[2], 0.5f);
            return Color.Lerp(orange, WayfinderSymbol.Colors[0], completionRatio);
        }

        public void DrawPixelPrimitives(SpriteBatch spriteBatch)
        {
            if (Timer < TelegraphLength + 20)
            {
                TelegraphDrawer ??= new PrimitiveTrailCopy(TelegraphWidthFunction, TelegraphColorFunction, null, true, InfernumEffectsRegistry.SideStreakVertexShader);

                InfernumEffectsRegistry.SideStreakVertexShader.SetShaderTexture(InfernumTextureRegistry.CultistRayMap);
                float opacityScalar = MathF.Sin(CalamityUtils.SineInOutEasing(Timer / (TelegraphLength + 20), 0) * MathF.PI);
                InfernumEffectsRegistry.SideStreakVertexShader.UseOpacity(0.5f * opacityScalar);

                Vector2 startT = Projectile.Center;
                Vector2 endT = startT - Vector2.UnitY * MaxLength * 1.2f;
                Vector2[] drawPositionsT = new Vector2[8];
                for (int i = 0; i < drawPositionsT.Length; i++)
                    drawPositionsT[i] = Vector2.Lerp(startT, endT, (float)i / drawPositionsT.Length);

                TelegraphDrawer.DrawPixelated(drawPositionsT, -Main.screenPosition, 20);

                Texture2D warningSymbol = InfernumTextureRegistry.VolcanoWarning.Value;
                Vector2 drawPosition = (startT - Vector2.UnitY * MaxLength * 0.575f) - Main.screenPosition;
                Color drawColor = Color.Lerp(TelegraphColorFunction(0.5f), Color.Orange, 0.5f) * Projectile.Opacity;
                drawColor.A = 0;
                Vector2 origin = warningSymbol.Size() * 0.5f;

                spriteBatch.Draw(warningSymbol, drawPosition, null, drawColor * opacityScalar, 0f, origin, 0.8f, SpriteEffects.None, 0f);
            }
            if (Timer > TelegraphLength)
            {
                LavaDrawer ??= new PrimitiveTrailCopy(WidthFunction, ColorFunction, null, true, InfernumEffectsRegistry.GuardiansLaserVertexShader);

                InfernumEffectsRegistry.GuardiansLaserVertexShader.SetShaderTexture(InfernumTextureRegistry.StreakThinGlow);
                InfernumEffectsRegistry.GuardiansLaserVertexShader.SetShaderTexture2(InfernumTextureRegistry.CultistRayMap);
                InfernumEffectsRegistry.GuardiansLaserVertexShader.UseColor(Color.LightGoldenrodYellow);
                InfernumEffectsRegistry.GuardiansLaserVertexShader.Shader.Parameters["flipY"].SetValue(false);
                float lengthScalar = CurrentLength / MaxLength;
                InfernumEffectsRegistry.GuardiansLaserVertexShader.Shader.Parameters["stretchAmount"].SetValue(2f + StretchOffset * lengthScalar);
                InfernumEffectsRegistry.GuardiansLaserVertexShader.Shader.Parameters["pillarVarient"].SetValue(true);

                Vector2 start = Projectile.Center;
                Vector2 end = start - Vector2.UnitY * CurrentLength;
                Vector2[] drawPositions = new Vector2[8];
                for (int i = 0; i < drawPositions.Length; i++)
                    drawPositions[i] = Vector2.Lerp(start, end, (float)i / drawPositions.Length);

                LavaDrawer.DrawPixelated(drawPositions, -Main.screenPosition, 30);
            }
        }
    }
}
