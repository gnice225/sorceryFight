using System;
using sorceryFight.SFPlayer;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace sorceryFight.Content.Buffs.PlayerAttributes
{
    public class SukunasVesselBuff : ModBuff
    {
        public override LocalizedText DisplayName => SFUtils.GetLocalization("Mods.sorceryFight.Buffs.PlayerAttributes.SukunasVessel.DisplayName");
        public override LocalizedText Description => SFUtils.GetLocalization("Mods.sorceryFight.Buffs.PlayerAttributes.SukunasVessel.Description");
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.buffNoSave[Type] = false;
            Main.persistentBuff[Type] = true;
        }

        public override bool RightClick(int buffIndex)
        {
            return false;
        }
    }
}
