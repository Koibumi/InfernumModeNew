﻿using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace InfernumMode.Common.Graphics
{
    public class RenderTargetManager : ModSystem
    {
        internal static List<ManagedRenderTarget> ManagedTargets = new();

        internal static void ResetTargetSizes(On.Terraria.Main.orig_SetDisplayMode orig, int width, int height, bool fullscreen)
        {
            foreach (ManagedRenderTarget target in ManagedTargets)
            {
                // Don't attempt to recreate targets that are in the middle of being disposed, is null, or shouldn't be recreated.
                if (target.IsDisposed || target is null)
                    continue;

                ScreenSaturationBlurSystem.DrawActionQueue.Enqueue(() =>
                {
                    target.Recreate(width, height);
                });
            }

            orig(width, height, fullscreen);
        }

        internal static void DisposeOfTargets()
        {
            if (ManagedTargets is null)
                return;

            foreach (ManagedRenderTarget target in ManagedTargets)
                target?.Dispose();
            ManagedTargets.Clear();
        }

        public static RenderTarget2D CreateScreenSizedTarget(int screenWidth, int screenHeight) =>
            new(Main.instance.GraphicsDevice, screenWidth, screenHeight, true, SurfaceFormat.Color, DepthFormat.Depth24, 8, RenderTargetUsage.DiscardContents);

        public override void OnModLoad()
        {
            DisposeOfTargets();
            ManagedTargets = new();
            On.Terraria.Main.SetDisplayMode += ResetTargetSizes;
        }

        public override void OnModUnload() => DisposeOfTargets();
    }
}