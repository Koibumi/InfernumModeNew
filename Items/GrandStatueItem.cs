using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using InfernumMode.Tiles;
using CalamityMod.Items.Materials;

namespace InfernumMode.Items
{
    public class GrandStatueItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = 1;
            DisplayName.SetDefault("Grand Statue");
        }

        public override void SetDefaults()
        {
            Item.width = 64;
            Item.height = 64;
            Item.maxStack = 999;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.value = 0;
            Item.createTile = ModContent.TileType<GrandStatue>();
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<GrandScale>());
            recipe.AddIngredient(ItemID.StoneBlock, 25);
            recipe.AddTile(TileID.Furnaces);
            recipe.Register();
        }
    }
}