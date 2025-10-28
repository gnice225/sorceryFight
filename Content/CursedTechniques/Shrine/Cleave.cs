using System;
using CalamityMod.Projectiles.Magic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using sorceryFight.Content.Buffs.Vessel;
using sorceryFight.Content.Items.Accessories;
using sorceryFight.SFPlayer;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace sorceryFight.Content.CursedTechniques.Shrine
{
    public class Cleave : CursedTechnique
    {
        public static readonly int FRAME_COUNT = 8;
        public static readonly int TICKS_PER_FRAME = 1;
        public static Texture2D texture;
        public override LocalizedText DisplayName => SFUtils.GetLocalization("Mods.sorceryFight.CursedTechniques.Cleave.DisplayName");
        public override string Description => SFUtils.GetLocalizationValue("Mods.sorceryFight.CursedTechniques.Cleave.Description");
        public override string LockedDescription => SFUtils.GetLocalizationValue("Mods.sorceryFight.CursedTechniques.Cleave.LockedDescription");
        public override float Cost => 75f;
        public override Color textColor => new Color(120, 21, 8);
        public override bool DisplayNameInGame => false;
        public override int Damage => 50;
        public override int MasteryDamageMultiplier => 25;
        public override float Speed => 0f;
        public override float LifeTime => 16f;
        float baseDamagePercent = 0.05f;
        public override int GetProjectileType()
        {
            return ModContent.ProjectileType<Cleave>();
        }
        public override bool Unlocked(SorceryFightPlayer sf)
        {
            return sf.HasDefeatedBoss(NPCID.SkeletronHead) || sf.Player.HasBuff(ModContent.BuffType<KingOfCursesBuff>());
        }

        public override string GetStats(SorceryFightPlayer sf)
        {
            float cost = CalculateTrueCost(sf);
            float percent = cost / sf.maxCursedEnergy;
            return $"{SFUtils.GetLocalizationValue("Mods.sorceryFight.UI.CursedTechnique.Damage")} {Math.Round(CalculateTrueDamage(sf), 2)} + {Math.Round(baseDamagePercent * 100, 2)}% of target's health\n"
                + $"{SFUtils.GetLocalizationValue("Mods.sorceryFight.UI.CursedTechnique.Cost")} {Math.Round(CalculateTrueCost(sf), 2)} {SFUtils.GetLocalizationValue("Mods.sorceryFight.UI.CursedTechnique.CE")}\n";
        }

        public override float CalculateTrueDamage(SorceryFightPlayer sf)
        {
            return base.CalculateTrueDamage(sf) + (0.01f * (sf.sukunasFingerConsumed / 20f));
        }

        public override float CalculateTrueCost(SorceryFightPlayer sf)
        {
            float finalCost = Cost - (Cost * (sf.bossesDefeated.Count / 100f));
            finalCost += sf.maxCursedEnergy * 0.05f;
            finalCost *= 1 - sf.ctCostReduction;
            finalCost *= 1 - (0.01f * sf.sukunasFingerConsumed);
            return finalCost;
        }

        public override int UseTechnique(SorceryFightPlayer sf)
        {
            Player player = sf.Player;

            if (player.whoAmI == Main.myPlayer)
            {
                Vector2 playerPos = player.MountedCenter;
                Vector2 mousePos = Main.MouseWorld;
                Vector2 dir = (mousePos - playerPos).SafeNormalize(Vector2.Zero) * Speed;
                var entitySource = player.GetSource_FromThis();
                sf.cursedEnergy -= CalculateTrueCost(sf);

                return Projectile.NewProjectile(entitySource, player.Center, dir, GetProjectileType(), (int)CalculateTrueDamage(sf), 0, player.whoAmI);
            }
            return -1;
        }

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = FRAME_COUNT;

            if (Main.dedServ) return;
            texture = ModContent.Request<Texture2D>("sorceryFight/Content/CursedTechniques/Shrine/Cleave", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.width = 188;
            Projectile.height = 188;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            Projectile.ai[0]++;

            if (Projectile.ai[0] >= LifeTime)
            {
                Projectile.Kill();
            }

            if (Projectile.frameCounter++ >= TICKS_PER_FRAME)
            {
                Projectile.frameCounter = 0;

                if (Projectile.frame++ >= FRAME_COUNT - 1)
                {
                    Projectile.frame = 0;
                }
            }

            /**
            Thank you MurasamaSlash.cs source code i'm too lazy to figure ts out -ehann
            */

            Player player = Main.player[Projectile.owner];
            Vector2 playerRotatedPoint = player.RotatedRelativePoint(player.MountedCenter, true);
            float velocityAngle = Projectile.velocity.ToRotation();
            float offset = 60f * Projectile.scale;

            Projectile.velocity = (Main.MouseWorld - playerRotatedPoint).SafeNormalize(Vector2.UnitX * player.direction);
            Projectile.direction = (Math.Cos(velocityAngle) > 0).ToDirectionInt();
            Projectile.rotation = velocityAngle + (Projectile.direction == -1).ToInt() * MathHelper.Pi;
            Projectile.Center = playerRotatedPoint + velocityAngle.ToRotationVector2() * offset;
            player.ChangeDir(Projectile.direction);

            if (Projectile.ai[0] == 1)
            {
                SoundEngine.PlaySound(SorceryFightSounds.CleaveSwing with { Volume = 3f }, player.Center);
            }

        }

        public override bool PreDraw(ref Color lightColor)
        {
            int frameHeight = texture.Height / FRAME_COUNT;
            int frameY = Projectile.frame * frameHeight;

            Rectangle sourceRectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
            Vector2 projOrigin = sourceRectangle.Size() * 0.5f;
            SpriteEffects spriteEffects = Projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + new Vector2(0, -32).RotatedBy(Projectile.rotation), sourceRectangle, Color.White, Projectile.rotation, projOrigin, 0.75f, spriteEffects, 0f);
            return false;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.FinalDamage.Flat += target.life * baseDamagePercent;
        }
    }
}
