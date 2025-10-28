using CalamityMod;
using Microsoft.Xna.Framework;
using sorceryFight.SFPlayer;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace sorceryFight.Content.Buffs.Limitless
{
    public class MaximumAmplifiedAuraBuff : PassiveTechnique
    {
        public float SpeedMultiplier { get; set; } = 100f;
        public float DamageMultiplier { get; set; } = 50f;

        public override LocalizedText DisplayName { get; } = SFUtils.GetLocalization("Mods.sorceryFight.Buffs.MaximumAmplifiedAuraBuff.DisplayName");
        public override string Stats
        {
            get
            {
                return $"{SFUtils.GetLocalizationValue("Mods.sorceryFight.Buffs.Stats.CEConsumption")} {CostPerSecond} {SFUtils.GetLocalizationValue("Mods.sorceryFight.UI.CursedEnergyBar.CEPerSecond")}\n"
                        + $"+{SpeedMultiplier}% {SFUtils.GetLocalizationValue("Mods.sorceryFight.Buffs.Stats.SpeedBoost")}.\n"
                        + $"+{DamageMultiplier}% {SFUtils.GetLocalizationValue("Mods.sorceryFight.Buffs.Stats.DamageBoost")}.\n"
                        + $"{SFUtils.GetLocalizationValue("Mods.sorceryFight.Buffs.Stats.CannotUseCursedTechniques")},\n"
                        + $"{SFUtils.GetLocalizationValue("Mods.sorceryFight.Buffs.Stats.UnlessUniqueBody")}.\n";
            }
        }
        public override LocalizedText Description => SFUtils.GetLocalization("Mods.sorceryFight.Buffs.MaximumAmplifiedAuraBuff.Description");
        public override string LockedDescription => SFUtils.GetLocalizationValue("Mods.sorceryFight.Buffs.MaximumAmplifiedAuraBuff.LockedDescription");
        public override bool isActive { get; set; } = false;
        public override float CostPerSecond { get; set; } = 10f;
        public override bool Unlocked(SorceryFightPlayer sf)
        {
            return sf.HasDefeatedBoss(NPCID.Golem);
        }
        protected Dictionary<int, int> auraIndices;
        public override void Apply(Player player)
        {
            player.AddBuff(ModContent.BuffType<MaximumAmplifiedAuraBuff>(), 2);

            if (player.HasBuff<AmplifiedAuraBuff>())
            {
                player.GetModPlayer<SorceryFightPlayer>().innateTechnique.PassiveTechniques[1].isActive = false;
            }

            if (player.HasBuff<FallingBlossomEmotionBuff>())
            {
                player.GetModPlayer<SorceryFightPlayer>().innateTechnique.PassiveTechniques[3].isActive = false;
            }

            if (Main.myPlayer == player.whoAmI && !auraIndices.ContainsKey(player.whoAmI))
            {
                Vector2 playerPos = player.MountedCenter;
                var entitySource = player.GetSource_FromThis();

                auraIndices[player.whoAmI] = Projectile.NewProjectile(entitySource, playerPos, Vector2.Zero, ModContent.ProjectileType<MaximumAmplifiedAuraProjectile>(), 0, 0, player.whoAmI);
            }

            player.GetModPlayer<SorceryFightPlayer>().disableCurseTechniques = true;
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

            CostPerSecond = 50f; // Base

            SorceryFightPlayer sf = player.GetModPlayer<SorceryFightPlayer>();
            float newCPS = sf.maxCursedEnergy / 50 + CostPerSecond;

            if (newCPS > CostPerSecond)
                CostPerSecond = newCPS;

        }

        public override void Update(Player player, ref int buffIndex)
        {
            base.Update(player, ref buffIndex);

            CostPerSecond = 50f; // Base

            SorceryFightPlayer sf = player.GetModPlayer<SorceryFightPlayer>();
            float newCPS = sf.maxCursedEnergy / 50 + CostPerSecond;

            if (newCPS > CostPerSecond)
                CostPerSecond = newCPS;

            player.moveSpeed *= (SpeedMultiplier / 100) + 1;
            player.GetDamage(DamageClass.Melee) *= (DamageMultiplier / 100) + 1;
            player.GetDamage(DamageClass.Ranged) *= (DamageMultiplier / 100) + 1;
            player.GetDamage(DamageClass.Magic) *= (DamageMultiplier / 100) + 1;
            player.GetDamage(DamageClass.Summon) *= (DamageMultiplier / 100) + 1;
            player.GetDamage(RogueDamageClass.Throwing) *= (DamageMultiplier / 100) + 1;
            player.GetDamage(CursedTechniqueDamageClass.Instance) *= (DamageMultiplier / 100) + 1;
        }
    }
}