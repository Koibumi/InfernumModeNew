using CalamityMod;
using CalamityMod.NPCs.Leviathan;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.Items
{
    public class LeviathanDetector : ModItem
    {
        public int frameCounter = 0;
        
        public int frame = 0;
        
        public override void SetStaticDefaults()
        {
            SacrificeTotal = 1;
            DisplayName.SetDefault("Leviathan Detector");
            Tooltip.SetDefault("Summons Anahita\n" +
                "Does not need to be used at the ocean\n" +
                "Not consumable");
        }

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.rare = ItemRarityID.Lime;
            Item.useAnimation = 45;
            Item.useTime = 45;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.consumable = false;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.Wire, 100);
            recipe.AddRecipeGroup("AnyCopperBar", 10);
            recipe.AddIngredient(ItemID.Glass, 15);
            recipe.AddIngredient(ItemID.SoulofLight, 4);
            recipe.AddIngredient(ItemID.SoulofNight, 4);
            recipe.AddTile(TileID.DemonAltar);
            recipe.Register();
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frameI, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Texture2D texture = ModContent.Request<Texture2D>("InfernumMode/Items/LeviathanDetector_Animated").Value;
            Rectangle f = Item.GetCurrentFrame(ref frame, ref frameCounter, 8, 5);
            Main.spriteBatch.Draw(texture, position, f, Color.White, 0f, f.Size() * new Vector2(0.16f, 0.25f), scale, SpriteEffects.None, 0);
            return false;
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = ModContent.Request<Texture2D>("InfernumMode/Items/LeviathanDetector_Animated").Value;
            Main.spriteBatch.Draw(texture, Item.position - Main.screenPosition, Item.GetCurrentFrame(ref frame, ref frameCounter, 8, 5), lightColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0);
            return false;
        }

        public override bool CanUseItem(Player player) => !NPC.AnyNPCs(ModContent.NPCType<Anahita>()) && !NPC.AnyNPCs(ModContent.NPCType<Leviathan>()) && (player.Center.X < 9000f || player.Center.X > Main.maxTilesX * 16f - 9000f);

        public override bool? UseItem(Player player)/* tModPorter Suggestion: Return null instead of false */
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Vector2 spawnPosition = player.Center - Vector2.UnitY * 350f;
                NPC.SpawnBoss((int)spawnPosition.X, (int)spawnPosition.Y, ModContent.NPCType<Anahita>(), player.whoAmI);
            }
            return true;
        }
    }
}