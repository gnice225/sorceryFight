using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace sorceryFight
{
    /// <summary>
    /// Handles the cinematic cutscene when a player activates Domain Expansion.
    /// Freezes all entities, moves camera to caster, and returns after expansion.
    /// </summary>
    public class DomainExpansionCutscene : ModSystem
    {
        public static bool IsInCutscene { get; private set; } = false;
        public static int CasterWhoAmI { get; private set; } = -1;
        public static Vector2 CasterPosition { get; private set; } = Vector2.Zero;
        public static float DomainRadius { get; private set; } = 4200f; // Default radius
        private static int cutsceneDuration = 0;
        private static int cutsceneTimer = 0;

        // Store original positions for NPCs and Players
        private static Dictionary<int, Vector2> npcOriginalPositions = new Dictionary<int, Vector2>();
        private static Dictionary<int, Vector2> npcOriginalVelocities = new Dictionary<int, Vector2>();
        private static Dictionary<int, Vector2> playerOriginalPositions = new Dictionary<int, Vector2>();

        /// <summary>
        /// Starts the Domain Expansion cutscene
        /// </summary>
        /// <param name="casterWhoAmI">The whoAmI of the player casting the domain</param>
        /// <param name="duration">Duration in ticks (60 = 1 second)</param>
        /// <param name="domainRadius">Radius of the domain (only players in this radius see cutscene)</param>
        public static void StartCutscene(int casterWhoAmI, int duration = 240, float domainRadius = 4200f)
        {
            if (Main.dedServ) return; // Don't run on server

            Player caster = Main.player[casterWhoAmI];
            
            // Only start cutscene if local player is within domain radius
            float distanceToLocalPlayer = Vector2.Distance(Main.LocalPlayer.Center, caster.Center);
            if (distanceToLocalPlayer > domainRadius)
            {
                return; // Local player is too far, don't show cutscene
            }

            IsInCutscene = true;
            CasterWhoAmI = casterWhoAmI;
            CasterPosition = caster.Center;
            DomainRadius = domainRadius;
            cutsceneDuration = duration;
            cutsceneTimer = 0;

            // Store NPC and Player states
            npcOriginalPositions.Clear();
            npcOriginalVelocities.Clear();
            playerOriginalPositions.Clear();

            foreach (NPC npc in Main.ActiveNPCs)
            {
                npcOriginalPositions[npc.whoAmI] = npc.position;
                npcOriginalVelocities[npc.whoAmI] = npc.velocity;
            }

            foreach (Player player in Main.ActivePlayers)
            {
                playerOriginalPositions[player.whoAmI] = player.position;
            }
            
            // Move camera to caster with smooth interpolation
            CameraController.SetCameraPosition(CasterPosition, duration, 0.1f);
            
            // Zoom in for dramatic effect (1.5x zoom)
            CameraController.SetCameraZoom(new Vector2(1.5f, 1.5f), duration, 0.1f);

            // Schedule end of cutscene
            TaskScheduler.Instance.AddDelayedTask(() =>
            {
                EndCutscene();
            }, duration);
        }

        /// <summary>
        /// Ends the cutscene and restores normal gameplay
        /// </summary>
        public static void EndCutscene()
        {
            IsInCutscene = false;
            CasterWhoAmI = -1;
            CasterPosition = Vector2.Zero;
            cutsceneTimer = 0;

            // Clear stored data
            npcOriginalPositions.Clear();
            npcOriginalVelocities.Clear();
            playerOriginalPositions.Clear();

            // Camera returns automatically via CameraController
        }

        public override void PostUpdateEverything()
        {
            if (!IsInCutscene) return;

            cutsceneTimer++;

            // Freeze all NPCs within domain radius
            foreach (NPC npc in Main.ActiveNPCs)
            {
                float distanceToCaster = Vector2.Distance(npc.Center, CasterPosition);
                if (distanceToCaster <= DomainRadius)
                {
                    if (!npc.boss) // Don't freeze bosses completely, just slow them
                    {
                        npc.velocity = Vector2.Zero;
                        
                        // Keep NPCs in their original position
                        if (npcOriginalPositions.ContainsKey(npc.whoAmI))
                        {
                            npc.position = npcOriginalPositions[npc.whoAmI];
                        }
                    }
                    else
                    {
                        // Slow down bosses to 10% speed
                        npc.velocity *= 0.1f;
                    }
                }
            }

            // Freeze ALL players within domain radius (INCLUDING the caster!)
            foreach (Player player in Main.ActivePlayers)
            {
                float distanceToCaster = Vector2.Distance(player.Center, CasterPosition);
                if (distanceToCaster <= DomainRadius)
                {
                    // Completely freeze the player
                    player.velocity = Vector2.Zero;
                    player.immuneTime = 2; // Make immune to prevent damage
                    
                    // Keep player in their original position
                    if (playerOriginalPositions.ContainsKey(player.whoAmI))
                    {
                        player.position = playerOriginalPositions[player.whoAmI];
                    }
                }
            }
        }
    }

    /// <summary>
    /// Prevents damage during cutscene
    /// </summary>
    public class DomainCutscenePlayer : ModPlayer
    {
        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            if (DomainExpansionCutscene.IsInCutscene)
            {
                // No damage during cutscene
                modifiers.FinalDamage *= 0f;
            }
        }
    }

    /// <summary>
    /// Prevents NPC damage during cutscene
    /// </summary>
    public class DomainCutsceneNPC : GlobalNPC
    {
        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (DomainExpansionCutscene.IsInCutscene)
            {
                modifiers.FinalDamage *= 0f;
            }
        }

        public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
        {
            if (DomainExpansionCutscene.IsInCutscene)
            {
                modifiers.FinalDamage *= 0f;
            }
        }

        public override void ModifyHitPlayer(NPC npc, Player target, ref Player.HurtModifiers modifiers)
        {
            if (DomainExpansionCutscene.IsInCutscene)
            {
                modifiers.FinalDamage *= 0f;
            }
        }
    }
}
