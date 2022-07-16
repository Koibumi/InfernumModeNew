using CalamityMod.NPCs;
using InfernumMode.Sounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.Skies
{
    public class DoGSkyScene : ModSceneEffect
    {
        public override bool IsSceneEffectActive(Player player) => false;

        // FUCK YOU FUCK YOU
        public override SceneEffectPriority Priority => (SceneEffectPriority)10000;

        public override float GetWeight(Player player) => 0.9f;

        public override void SpecialVisuals(Player player, bool isActive)
        {
            if (isActive)
                SkyManager.Instance.Deactivate("CalamityMod:DevourerofGodsHead");
            player.ManageSpecialBiomeVisuals("InfernumMode:DoG", isActive);
        }
    }

    public class DoGSkyInfernum : CustomSky
    {
        public class Lightning
        {
            public int Lifetime;
            public float Depth;
            public Vector2 Position;
            public Color LightningColor;
        }

        public float BackgroundIntensity;
        public float LightningIntensity;
        public List<Lightning> LightningBolts = new();
        public static bool CanSkyBeActive
        {
            get
            {
                if (!InfernumMode.CanUseCustomAIs)
                    return false;

                return CalamityGlobalNPC.DoGHead != -1;
            }
        }

        public static void CreateLightningBolt(Color color, int count = 1, bool playSound = false)
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            for (int i = 0; i < count; i++)
            {
                Lightning lightning = new()
                {
                    Lifetime = 30,
                    Depth = Main.rand.NextFloat(1.5f, 10f),
                    Position = new Vector2(Main.LocalPlayer.Center.X + Main.rand.NextFloatDirection() * 5000f, Main.rand.NextFloat(4850f)),
                    LightningColor = color
                };
                (SkyManager.Instance["InfernumMode:DoG"] as DoGSkyInfernum).LightningBolts.Add(lightning);
            }

            // Make the sky flash if enough lightning bolts are created.
            if (count >= 10)
            {
                (SkyManager.Instance["InfernumMode:DoG"] as DoGSkyInfernum).LightningIntensity = 1f;
                playSound = true;
            }

            if (playSound && !Main.gamePaused)
                SoundEngine.PlaySound(InfernumSoundRegistry.CalThunderStrikeSound with { Volume = 0.5f }, Main.LocalPlayer.Center);
        }

        public override void Update(GameTime gameTime)
        {
            if (!CanSkyBeActive)
            {
                LightningIntensity = 0f;
                BackgroundIntensity = MathHelper.Clamp(BackgroundIntensity - 0.08f, 0f, 1f);
                LightningBolts.Clear();
                Deactivate();
                return;
            }

            LightningIntensity = MathHelper.Clamp(LightningIntensity * 0.95f - 0.025f, 0f, 1f);
            BackgroundIntensity = MathHelper.Clamp(BackgroundIntensity + 0.01f, 0f, 1f);

            for (int i = 0; i < LightningBolts.Count; i++)
            {
                LightningBolts[i].Lifetime--;
            }

            Opacity = BackgroundIntensity;
        }

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            if (!CanSkyBeActive)
                return;

            if (maxDepth >= float.MaxValue)
            {
                // Draw lightning in the background based on Main.magicPixel.
                // It is a long, white vertical strip that exists for some reason.
                // This lightning effect is achieved by expanding this to fit the entire background and then drawing it as a distinct element.
                Vector2 scale = new(Main.screenWidth * 1.1f / TextureAssets.MagicPixel.Value.Width, Main.screenHeight * 1.1f / TextureAssets.MagicPixel.Value.Height);
                Vector2 screenArea = new Vector2(Main.screenWidth, Main.screenHeight) * 0.5f;
                Color drawColor = Color.White * MathHelper.Lerp(0f, 0.24f, LightningIntensity) * BackgroundIntensity;

                // Draw a grey background as base.
                Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, screenArea, null, OnTileColor(Color.Transparent), 0f, TextureAssets.MagicPixel.Value.Size() * 0.5f, scale, SpriteEffects.None, 0f);

                for (int i = 0; i < 2; i++)
                    Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, screenArea, null, drawColor, 0f, TextureAssets.MagicPixel.Value.Size() * 0.5f, scale, SpriteEffects.None, 0f);
            }

            Texture2D flashTexture = ModContent.Request<Texture2D>("Terraria/Images/Misc/VortexSky/Flash").Value;
            Texture2D boltTexture = ModContent.Request<Texture2D>("Terraria/Images/Misc/VortexSky/Bolt").Value;

            // Draw lightning bolts.
            float spaceFade = Math.Min(1f, (Main.screenPosition.Y - 300f) / 300f);
            Vector2 screenCenter = Main.screenPosition + new Vector2(Main.screenWidth * 0.5f, Main.screenHeight * 0.5f);
            Rectangle rectangle = new(-1000, -1000, 4000, 4000);

            LightningBolts.RemoveAll(l => l.Lifetime <= 0);

            for (int i = 0; i < LightningBolts.Count; i++)
            {
                Vector2 boltScale = new(1f / LightningBolts[i].Depth, 0.9f / LightningBolts[i].Depth);
                Vector2 position = (LightningBolts[i].Position - screenCenter) * boltScale + screenCenter - Main.screenPosition;
                if (rectangle.Contains((int)position.X, (int)position.Y))
                {
                    Texture2D texture = boltTexture;
                    int life = LightningBolts[i].Lifetime;
                    if (life > 24 && life % 2 == 0)
                        texture = flashTexture;

                    float opacity = life * spaceFade / 20f;
                    Main.spriteBatch.Draw(texture, position, null, LightningBolts[i].LightningColor * opacity, 0f, Vector2.Zero, boltScale.X * 5f, SpriteEffects.None, 0f);
                }
            }
        }

        public override float GetCloudAlpha() => 0f;

        public override void Reset() { }

        public override void Activate(Vector2 position, params object[] args) { }

        public override void Deactivate(params object[] args) { }

        public override bool IsActive() => CanSkyBeActive && !Main.gameMenu;
    }
}
