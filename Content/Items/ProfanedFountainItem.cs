using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using CalamityMod.Items.Placeables.FurnitureProfaned;
using InfernumMode.Content.Tiles;

namespace InfernumMode.Content.Items
{
    public class ProfanedFountainItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = 1;
            DisplayName.SetDefault("Profaned Lava Fountain");
        }

        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 42;
            Item.maxStack = 999;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.value = Item.buyPrice(0, 4, 0, 0);
            Item.rare = ItemRarityID.White;
            Item.createTile = ModContent.TileType<ProfanedFountainTile>();
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<ProfanedRock>(), 20);
            recipe.AddIngredient(ItemID.LavaBucket);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
}