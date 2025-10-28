using CalamityMod;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using sorceryFight.SFPlayer;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ID;

namespace sorceryFight.Content.Buffs.Limitless
{
    public class FallingBlossomEmotionBuff : PassiveTechnique
    {
        public override LocalizedText DisplayName => SFUtils.GetLocalization("Mods.sorceryFight.Buffs.FallingBlossomEmotionBuff.DisplayName");
        public override string Stats
        {
            get
            {
                return $"{SFUtils.GetLocalizationValue("Mods.sorceryFight.Buffs.Stats.CEConsumption")} {CostPerSecond} {SFUtils.GetLocalizationValue("Mods.sorceryFight.UI.CursedEnergyBar.CEPerSecond")}\n"
                        + $"+20 {SFUtils.GetLocalizationValue("Mods.sorceryFight.Buffs.Stats.DefenseBoost")} ({SFUtils.GetLocalizationValue("Mods.sorceryFight.Buffs.Stats.WhenStandingStill")}).\n";
            }
        }
        public override LocalizedText Description => SFUtils.GetLocalization("Mods.sorceryFight.Buffs.FallingBlossomEmotionBuff.Description");

        public override string LockedDescription => SFUtils.GetLocalizationValue("Mods.sorceryFight.Buffs.FallingBlossomEmotionBuff.LockedDescription");
        public override bool isActive { get; set; } = false;
        public override float CostPerSecond { get; set; } = 85;

        public override bool Unlocked(SorceryFightPlayer sf)
        {
            return sf.HasDefeatedBoss(NPCID.CultistBoss);
        }

        protected Dictionary<int, int> auraIndices;
        public override void Apply(Player player)
        {
            player.AddBuff(ModContent.BuffType<FallingBlossomEmotionBuff>(), 2);


            if (player.HasBuff<AmplifiedAuraBuff>())
            {
                player.GetModPlayer<SorceryFightPlayer>().innateTechnique.PassiveTechniques[1].isActive = false;

            }
            if (player.HasBuff<MaximumAmplifiedAuraBuff>())
            {
                player.GetModPlayer<SorceryFightPlayer>().innateTechnique.PassiveTechniques[2].isActive = false;
            }

            if (auraIndices == null)
                auraIndices = new Dictionary<int, int>();

            if (Main.myPlayer == player.whoAmI && !auraIndices.ContainsKey(player.whoAmI))
            {
                Vector2 playerPos = player.MountedCenter;
                var entitySource = player.GetSource_FromThis();

                auraIndices[player.whoAmI] = Projectile.NewProjectile(entitySource, playerPos, Vector2.Zero, ModContent.ProjectileType<FallingBlossomEmotionProjectile>(), 0, 0, player.whoAmI);
            }

            player.GetModPlayer<SorceryFightPlayer>().disableCurseTechniques = true;
            player.GetModPlayer<SorceryFightPlayer>().fallingBlossomEmotion = true;
        }

        public override void Remove(Player player)
        {
            if (auraIndices == null)
                auraIndices = new Dictionary<int, int>();

            if (auraIndices.ContainsKey(player.whoAmI))
            {
                Main.projectile[auraIndices[player.whoAmI]].Kill();
                auraIndices.Remove(player.whoAmI);
            }
            SorceryFightPlayer sf = player.GetModPlayer<SorceryFightPlayer>();

            sf.fallingBlossomEmotion = false;
            player.noKnockback = true;

        }

        public override void Update(Player player, ref int buffIndex)
        {
            base.Update(player, ref buffIndex);

            if (player.velocity == Vector2.Zero)
            { 
                player.statDefense += 20;
                player.noKnockback = true;
            }
        }
    }
}