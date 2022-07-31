﻿using InfernumMode.Items.Relics;
using Terraria.ModLoader;

namespace InfernumMode.Tiles.Relics
{
    public class GreatSandSharkRelicTile : BaseInfernumBossRelic
    {
        public override int DropItemID => ModContent.ItemType<GreatSandSharkRelic>();

        public override string RelicTextureName => "InfernumMode/Tiles/Relics/GreatSandSharkRelicTile";
    }
}