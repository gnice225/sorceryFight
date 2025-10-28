using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using sorceryFight.SFPlayer;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace sorceryFight.Content.Buffs.Limitless
{

    public class InfinityBuff : PassiveTechnique
    {
        private Dictionary<int, Vector2> velocityData = new Dictionary<int, Vector2>();
        public override LocalizedText DisplayName => SFUtils.GetLocalization("Mods.sorceryFight.Buffs.Infinity.DisplayName");
        public override string Stats
        {
            get
            {
                return $"{SFUtils.GetLocalizationValue("Mods.sorceryFight.Buffs.Stats.BaseCEConsumption")} 1 {SFUtils.GetLocalizationValue("Mods.sorceryFight.UI.CursedEnergyBar.CEPerSecond")}\n"
                        + $"{SFUtils.GetLocalizationValue("Mods.sorceryFight.Buffs.Stats.InfinityBlockedObject")}\n"
                        + $"{SFUtils.GetLocalizationValue("Mods.sorceryFight.Buffs.Stats.InfinityBossFight")}\n";
            }
        }
        public override LocalizedText Description => SFUtils.GetLocalization("Mods.sorceryFight.Buffs.Infinity.Description");
        public override string LockedDescription => SFUtils.GetLocalizationValue("Mods.sorceryFight.Buffs.Infinity.LockedDescription");

        public override bool isActive { get; set; } = false;
        public override float CostPerSecond { get; set; } = 1f;
        public bool waiting = false;
        public override bool Unlocked(SorceryFightPlayer sf)
        {
            return sf.HasDefeatedBoss(NPCID.EyeofCthulhu);
        }

        public override void Apply(Player player)
        {
            player.AddBuff(ModContent.BuffType<InfinityBuff>(), 2);

            player.GetModPlayer<SorceryFightPlayer>().infinity = true;
        }

        public override void Remove(Player player)
        {
            SorceryFightPlayer sf = player.GetModPlayer<SorceryFightPlayer>();
            sf.infinity = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            SorceryFightPlayer sf = player.GetModPlayer<SorceryFightPlayer>();
            float infinityDistance = 50f;
            CostPerSecond = 1f;

            sf.disableRegenFromBuffs = false;

            float accumulativeDamage = 0f;
            int npcInInfinity = 0;
            foreach (Projectile proj in Main.ActiveProjectiles)
            {

                if (proj.hostile)
                {
                    float distance = Vector2.Distance(proj.Center, player.Center);
                    if (distance <= infinityDistance)
                    {
                        accumulativeDamage += proj.damage;
                        npcInInfinity++;

                        proj.velocity *= 0.5f;
                        Vector2 vector = player.Center.DirectionTo(proj.Center);
                        proj.velocity = vector * (3f + player.velocity.Length()) * ((infinityDistance - distance) / 75);
                    }
                }
            }

            foreach (NPC npc in Main.npc)
            {

                if (!npc.friendly && npc.type != NPCID.TargetDummy && npc.active)
                {
                    float distance = Vector2.Distance(npc.Center, player.Center);
                    if (distance <= infinityDistance)
                    {
                        accumulativeDamage += npc.damage;

                        if (!velocityData.ContainsKey(npc.whoAmI))
                        {
                            velocityData[npc.whoAmI] = npc.velocity;
                        }

                        npc.velocity *= 0.5f;
                        Vector2 vector = player.Center.DirectionTo(npc.Center);
                        npc.velocity = vector * (3f + player.velocity.Length()) * ((infinityDistance - distance) / 50);
                        
                        // Apply contact damage to NPC
                        if (Main.myPlayer == player.whoAmI && distance < 30f)
                        {
                            int contactDamage = (int)(npc.damage * 0.5f);
                            if (contactDamage > 0)
                            {
                                player.ApplyDamageToNPC(npc, contactDamage, 0f, 0, false);
                            }
                        }
                    }

                    else if (velocityData.ContainsKey(npc.whoAmI))
                    {
                        npc.velocity = velocityData[npc.whoAmI];
                        velocityData.Remove(npc.whoAmI);
                    }

                }
            }

            if (accumulativeDamage > 0f && !waiting)
            {

                TaskScheduler.Instance.AddContinuousTask(() =>
                {
                    sf.disableRegenFromBuffs = true;
                },
                300);

                TaskScheduler.Instance.AddDelayedTask(() =>
                {
                    waiting = false;
                },
                301);

                waiting = true;
            }

            int multiplier = 1;
            if (CalamityMod.CalPlayer.CalamityPlayer.areThereAnyDamnBosses)
            {
                multiplier = 3;
            }

            CostPerSecond += accumulativeDamage *= multiplier;

            base.Update(player, ref buffIndex);
        }
    }
}
