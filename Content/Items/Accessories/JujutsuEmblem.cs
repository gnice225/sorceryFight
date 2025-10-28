using sorceryFight.Rarities;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using sorceryFight.SFPlayer;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace sorceryFight.Content.Items.Accessories
{
    public class JujutsuEmblem : ModItem
    {
        private const float cursedTechniqueDamageIncrease = 0.15f;
        public override LocalizedText DisplayName => SFUtils.GetLocalization("Mods.sorceryFight.Accessories.JujutsuEmblem.DisplayName");
        public override LocalizedText Tooltip => SFUtils.GetLocalization("Mods.sorceryFight.Accessories.JujutsuEmblem.Tooltip").WithFormatArgs((int)(cursedTechniqueDamageIncrease * 100));

        public override void SetDefaults()
        {
            Item.accessory = true;
            Item.maxStack = 1;
            Item.rare = ModContent.RarityType<SorceryFightAccessory>();
            Item.width = 30;
            Item.height = 30;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            base.UpdateAccessory(player, hideVisual);

            player.GetDamage(CursedTechniqueDamageClass.Instance) *= 1f + cursedTechniqueDamageIncrease;
        }
    }
}
