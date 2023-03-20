using System;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using InfernumMode.Assets.Effects;
using InfernumMode.Assets.ExtraTextures;
using Terraria.Graphics;
using CalamityMod.NPCs;
using CalamityMod;
using Terraria.Audio;
using Terraria.ID;

namespace InfernumMode.Common.Graphics
{
    public class CeaselessDimensionDrawSystem : ModSystem
    {
        public static Action<Main, bool> DrawBlack
        {
            get;
            private set;
        }

        public static Action<Main> DrawWalls
        {
            get;
            private set;
        }

        public static bool ShouldDrawWithDimensionEffects => BackgroundChangeInterpolant > 0f;

        public static float BackgroundChangeInterpolant
        {
            get;
            set;
        }

        public static float ZoomChangeInterpolant
        {
            get;
            set;
        }

        public override void OnModLoad()
        {
            On.Terraria.Main.DoDraw_WallsAndBlacks += DistortBackground;

            // Wrap the draw black and draw walls functions in delegates so that they can be tossed around and called at will.
            var drawBlack = typeof(Main).GetMethod("DrawBlack", Utilities.UniversalBindingFlags);
            DrawBlack = Delegate.CreateDelegate(typeof(Action<Main, bool>), drawBlack) as Action<Main, bool>;

            var drawWalls = typeof(Main).GetMethod("DrawWalls", Utilities.UniversalBindingFlags);
            DrawWalls = Delegate.CreateDelegate(typeof(Action<Main>), drawWalls) as Action<Main>;
        }

        private void DistortBackground(On.Terraria.Main.orig_DoDraw_WallsAndBlacks orig, Main self)
        {
            if (!ShouldDrawWithDimensionEffects)
            {
                orig(self);
                return;
            }

            // For context, drawToScreen exists as an optimization for Terraria so that they only need to calculate drawing information for things like
            // tiles every five frames or so, with the remaining frames just drawing the contents of a render target instead.
            if (Main.drawToScreen)
            {
                // Draw the black thing to their render target.
                DrawBlack.Invoke(Main.instance, false);

                // Draw the walls to their render target.
                Main.tileBatch.Begin();
                DrawWalls.Invoke(Main.instance);
                Main.tileBatch.End();
            }
            else
            {
                // Apply shader effects if necessary.
                // For the sake of not exposing weird background pieces, this is also draws a dimensional rift thing in the background at the absolute back.
                if (ShouldDrawWithDimensionEffects)
                {
                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

                    DrawDimensionalWarpBackground();

                    var backgroundEffect = InfernumEffectsRegistry.BackgroundDistortionShader;
                    backgroundEffect.UseImage1("Images/Misc/Perlin");
                    backgroundEffect.Shader.Parameters["distortionIntensity"].SetValue(BackgroundChangeInterpolant);
                    backgroundEffect.Apply();
                }
                else
                    Main.spriteBatch.Draw(Main.instance.blackTarget, Main.sceneTilePos - Main.screenPosition, Color.White);

                // Draw the walls.
                TimeLogger.DetailedDrawTime(13);
                Main.spriteBatch.Draw(Main.instance.wallTarget, Main.sceneWallPos - Main.screenPosition, Color.White);
                if (ShouldDrawWithDimensionEffects)
                {
                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
                }

                TimeLogger.DetailedDrawTime(14);
            }

            Overlays.Scene.Draw(Main.spriteBatch, RenderLayers.Walls);
        }

        private static void DrawDimensionalWarpBackground()
        {
            Texture2D pixel = InfernumTextureRegistry.Pixel.Value;
            Vector2 screenArea = new(Main.instance.GraphicsDevice.Viewport.Width, Main.instance.GraphicsDevice.Viewport.Height);
            Vector2 textureArea = screenArea / pixel.Size();
            var backgroundShader = InfernumEffectsRegistry.SignusBackgroundShader;
            backgroundShader.Shader.Parameters["vortexSwirlSpeed"].SetValue(-2.33f);
            backgroundShader.Shader.Parameters["vortexSwirlDetail"].SetValue(67f);
            backgroundShader.Shader.Parameters["vortexEdgeFadeFactor"].SetValue(14f);
            backgroundShader.Shader.Parameters["luminanceThreshold"].SetValue(0.8f);
            backgroundShader.SetShaderTexture(InfernumTextureRegistry.HoneycombNoise);
            backgroundShader.UseColor(Color.Lerp(Color.BlueViolet, Color.Black, 0.32f));
            backgroundShader.UseSecondaryColor(Color.White);
            backgroundShader.UseShaderSpecificData(new Vector4(screenArea.Y, screenArea.X, 0f, 0f));
            backgroundShader.Apply();
            Main.spriteBatch.Draw(pixel, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, textureArea, 0, 0f);
        }

        public override void PostUpdateEverything()
        {
            if (!InfernumMode.CanUseCustomAIs || CalamityGlobalNPC.voidBoss == -1 || Main.npc[CalamityGlobalNPC.voidBoss].ai[0] == 0f)
            {
                BackgroundChangeInterpolant = MathHelper.Clamp(BackgroundChangeInterpolant - 0.3f, 0f, 1f);
                ZoomChangeInterpolant = MathHelper.Clamp(ZoomChangeInterpolant - 0.2f, 0f, 1f);
                return;
            }

            ZoomChangeInterpolant = MathHelper.Clamp(ZoomChangeInterpolant + 0.03f, 0f, 1f);
            BackgroundChangeInterpolant += 0.005f;
            if (BackgroundChangeInterpolant >= 1f)
                BackgroundChangeInterpolant = 0f;
            if (MathHelper.Distance(BackgroundChangeInterpolant, 0.3f) < 0.0001f)
                SoundEngine.PlaySound(SoundID.Item163 with { Pitch = -0.32f });
        }

        public override void ModifyTransformMatrix(ref SpriteViewMatrix Transform)
        {
            if (ZoomChangeInterpolant <= 0f)
                return;

            Vector2 idealZoom = new Vector2(Main.screenWidth, Main.screenHeight) / new Vector2(2150f, 1210f);
            Main.GameViewMatrix.Zoom = Vector2.SmoothStep(Main.GameViewMatrix.Zoom, idealZoom, ZoomChangeInterpolant);
        }

        public override void ModifyLightingBrightness(ref float scale)
        {
            if (BackgroundChangeInterpolant > 0f)
                scale += MathF.Pow(BackgroundChangeInterpolant, 0.2f) * 0.06f;
        }
    }
}