using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using sorceryFight.Content.Buffs.Vessel;
using sorceryFight.SFPlayer;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace sorceryFight.Content.Buffs.Shrine
{
    public class DomainAmplificationBuff : PassiveTechnique
    {
        public override LocalizedText DisplayName => SFUtils.GetLocalization("Mods.sorceryFight.Buffs.DomainAmplificationBuff.DisplayName");
        public override LocalizedText Description => SFUtils.GetLocalization("Mods.sorceryFight.Buffs.DomainAmplificationBuff.Description");
        public override string LockedDescription => SFUtils.GetLocalizationValue("Mods.sorceryFight.Buffs.DomainAmplificationBuff.LockedDescription");
        public override string Stats
        {
            get
            {
                return $"{SFUtils.GetLocalizationValue("Mods.sorceryFight.Buffs.Stats.BaseCEConsumption")} 10 {SFUtils.GetLocalizationValue("Mods.sorceryFight.UI.CursedEnergyBar.CEPerSecond")}\n"
                        + $"50% {SFUtils.GetLocalizationValue("Mods.sorceryFight.Buffs.Stats.DamageReduction")}.\n";
            }
        }
        public override bool isActive { get; set; } = false;
        public override float CostPerSecond { get; set; } = 10f;

        public Dictionary<int, int> auraIndices;

        public override void Apply(Player player)
        {
            player.AddBuff(ModContent.BuffType<DomainAmplificationBuff>(), 2);
            SorceryFightPlayer sfPlayer = player.GetModPlayer<SorceryFightPlayer>();

            if (player.HasBuff<HollowWickerBasketBuff>())
            {
                sfPlayer.innateTechnique.PassiveTechniques[1].isActive = false;
            }

            sfPlayer.domainAmp = true;


            if (auraIndices == null)
                auraIndices = new Dictionary<int, int>();

            if (Main.myPlayer == player.whoAmI && !auraIndices.ContainsKey(player.whoAmI))
            {
                Vector2 playerPos = player.MountedCenter;
                var entitySource = player.GetSource_FromThis();

                auraIndices[player.whoAmI] = Projectile.NewProjectile(entitySource, playerPos, Vector2.Zero, ModContent.ProjectileType<DomainAmplificationProjectile>(), 0, 0, player.whoAmI);
            }

            sfPlayer.disableCurseTechniques = true;
        }

        public override void Remove(Player player)
        {
            SorceryFightPlayer sfPlayer = player.GetModPlayer<SorceryFightPlayer>();
            sfPlayer.domainAmp = false;

            if (auraIndices == null)
                auraIndices = new Dictionary<int, int>();

            if (auraIndices.ContainsKey(player.whoAmI))
            {
                Main.projectile[auraIndices[player.whoAmI]].Kill();
                auraIndices.Remove(player.whoAmI);
            }
        }

        public override bool Unlocked(SorceryFightPlayer sf)
        {
            return sf.HasDefeatedBoss(NPCID.WallofFlesh) || sf.Player.HasBuff(ModContent.BuffType<KingOfCursesBuff>());
        }

        public override void Update(Player player, ref int buffIndex)
        {
            SorceryFightPlayer sfPlayer = player.GetModPlayer<SorceryFightPlayer>();

            float minimumDistance = 25f;
            float accumulativeDamage = 0f;

            foreach (Projectile proj in Main.ActiveProjectiles)
            {
                if (!proj.hostile) continue;

                float distance = Vector2.DistanceSquared(proj.Center, player.Center);
                if (distance <= minimumDistance * minimumDistance)
                {
                    accumulativeDamage += proj.damage;
                }
            }

            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.friendly || npc.type == NPCID.TargetDummy) continue;

                float distance = Vector2.DistanceSquared(npc.Center, player.Center);
                if (distance <= minimumDistance * minimumDistance)
                {
                    accumulativeDamage += npc.damage;
                }
            }

            if (accumulativeDamage > 0f)
            {
                sfPlayer.disableRegenFromBuffs = true;
            }

            float multiplier = 1;
            if (CalamityMod.CalPlayer.CalamityPlayer.areThereAnyDamnBosses)
            {
                multiplier = 1.5f;
            }

            CostPerSecond = 10f;
            CostPerSecond += accumulativeDamage * multiplier;
            
            base.Update(player, ref buffIndex);
        }
    }
}
