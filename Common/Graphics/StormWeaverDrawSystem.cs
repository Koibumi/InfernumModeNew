using CalamityMod.NPCs.StormWeaver;
using InfernumMode.Assets.Effects;
using InfernumMode.Assets.ExtraTextures;
using InfernumMode.Content.BehaviorOverrides.BossAIs.StormWeaver;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace InfernumMode.Common.Graphics
{
    public class StormWeaverDrawSystem : ModSystem
    {
        public static ManagedRenderTarget WeaverDrawTarget
        {
            get;
            private set;
        }

        public override void OnModLoad()
        {
            Main.OnPreDraw += PrepareTarget;
            WeaverDrawTarget = new(true, RenderTargetManager.CreateScreenSizedTarget);
        }

        internal static void PrepareTarget(GameTime obj)
        {
            int weaverHeadID = ModContent.NPCType<StormWeaverHead>();
            if (Main.gameMenu || WeaverDrawTarget.Target.IsDisposed || !NPC.AnyNPCs(weaverHeadID) || !InfernumMode.CanUseCustomAIs)
                return;

            Main.instance.GraphicsDevice.SetRenderTarget(WeaverDrawTarget.Target);
            Main.instance.GraphicsDevice.Clear(Color.Transparent);

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.Default, Main.Rasterizer, null, Matrix.Identity);

            // Draw all weaver segments.
            int weaverBodyID = ModContent.NPCType<StormWeaverBody>();
            int weaverTailID = ModContent.NPCType<StormWeaverTail>();
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC n = Main.npc[i];
                if (!n.active)
                    continue;

                if (n.type != weaverHeadID && n.type != weaverBodyID && n.type != weaverTailID)
                    continue;

                Texture2D texture = ModContent.Request<Texture2D>("CalamityMod/NPCs/StormWeaver/StormWeaverBodyNaked").Value;
                if (n.type == weaverHeadID)
                    texture = ModContent.Request<Texture2D>("CalamityMod/NPCs/StormWeaver/StormWeaverHeadNaked").Value;
                if (n.type == weaverTailID)
                    texture = ModContent.Request<Texture2D>("CalamityMod/NPCs/StormWeaver/StormWeaverTailNaked").Value;

                Vector2 drawPosition = n.Center - Main.screenPosition;
                Main.spriteBatch.Draw(texture, drawPosition, null, n.GetAlpha(Color.White), n.rotation, texture.Size() * 0.5f, n.scale, 0, 0f);
            }

            Main.spriteBatch.End();
        }

        public static void DrawTarget()
        {
            int weaverIndex = NPC.FindFirstNPC(ModContent.NPCType<StormWeaverHead>());
            if (weaverIndex == -1 || !InfernumMode.CanUseCustomAIs)
                return;

            float electricityFormInterpolant = Main.npc[weaverIndex].ai[3];
            Color drawColor = Color.Lerp(Color.White, Color.Cyan with { A = 100 }, electricityFormInterpolant);
            if (electricityFormInterpolant > 0f)
            {
                for (int i = 0; i < 8; i++)
                {
                    Vector2 drawOffset = (MathHelper.TwoPi * i / 8f).ToRotationVector2() * electricityFormInterpolant * 8f;
                    Main.spriteBatch.Draw(WeaverDrawTarget.Target, WeaverDrawTarget.Target.Size() * 0.5f + drawOffset, null, Color.Lerp(drawColor, Color.Wheat, 0.5f) with { A = 0 } * 0.7f, 0f, WeaverDrawTarget.Target.Size() * 0.5f, 1f, 0, 0f);
                }
            }
            Main.spriteBatch.Draw(WeaverDrawTarget.Target, Vector2.Zero, drawColor);

            // Draw the fog if necessary.
            float fogInterpolant = Main.npc[weaverIndex].Infernum().ExtraAI[StormWeaverHeadBehaviorOverride.FogInterpolantIndex];
            if (fogInterpolant > 0f)
            {
                Texture2D pixel = InfernumTextureRegistry.Pixel.Value;
                Vector2 scale = new Vector2(Main.screenWidth, Main.screenHeight) / pixel.Size() * 1.1f;
                Vector2 drawPosition = new Vector2(Main.screenWidth, Main.screenHeight) * 0.5f;

                Main.spriteBatch.EnterShaderRegion();

                InfernumEffectsRegistry.FogShaderShader.Apply();
                InfernumEffectsRegistry.FogShaderShader.SetShaderTexture(InfernumTextureRegistry.SmokyNoise);
                Main.spriteBatch.Draw(pixel, drawPosition, null, Color.SlateGray * fogInterpolant * 0.5f, 0f, pixel.Size() * 0.5f, scale, 0, 0f);
                Main.spriteBatch.ExitShaderRegion();
            }
        }
    }
}