using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;

namespace sorceryFight
{
    /// <summary>
    /// Handles screen darkening effect for Malevolent Shrine domain expansion
    /// </summary>
    public class MalevolentShrineScreenEffect : ModSystem
    {
        public static bool IsActive { get; private set; }
        private static float darkenIntensity = 0f;
        private static bool isFadingIn = false;
        private static bool isFadingOut = false;
        private const float FadeSpeed = 0.015f; // Speed of fade effect (slower for smoother transition)

        public override void Load()
        {
            On_Main.DrawBackgroundBlackFill += DrawDarkenOverlay;
        }

        public override void Unload()
        {
            On_Main.DrawBackgroundBlackFill -= DrawDarkenOverlay;
        }

        /// <summary>
        /// Start darkening effect after voice line finishes (~3.9 seconds + small delay)
        /// </summary>
        public static void StartDarkening()
        {
            IsActive = true;
            isFadingIn = true;
            isFadingOut = false;
        }

        /// <summary>
        /// Stop darkening effect and fade back to normal
        /// </summary>
        public static void StopDarkening()
        {
            isFadingIn = false;
            isFadingOut = true;
        }

        public override void PostUpdateEverything()
        {
            // Fade in (darken screen)
            if (isFadingIn && darkenIntensity < 1f)
            {
                darkenIntensity += FadeSpeed;
                if (darkenIntensity >= 1f)
                {
                    darkenIntensity = 1f;
                    isFadingIn = false;
                }
            }

            // Fade out (brighten screen back to normal)
            if (isFadingOut && darkenIntensity > 0f)
            {
                darkenIntensity -= FadeSpeed;
                if (darkenIntensity <= 0f)
                {
                    darkenIntensity = 0f;
                    isFadingOut = false;
                    IsActive = false;
                }
            }
        }
        
        // Draw darkening overlay using DomainExpansionBase texture
        private void DrawDarkenOverlay(On_Main.orig_DrawBackgroundBlackFill orig, Main self)
        {
            // Draw original first
            orig(self);
            
            // Then draw our overlay on top (SpriteBatch is already active at this point)
            if (IsActive && darkenIntensity > 0f && !Main.gameMenu)
            {
                SpriteBatch spriteBatch = Main.spriteBatch;
                
                // Load the DomainExpansionBase texture
                Texture2D overlayTexture = ModContent.Request<Texture2D>("sorceryFight/Content/DomainExpansions/DomainExpansionBase").Value;
                
                // Calculate scale to cover entire screen
                float scaleX = (float)Main.screenWidth / overlayTexture.Width;
                float scaleY = (float)Main.screenHeight / overlayTexture.Height;
                float scale = Math.Max(scaleX, scaleY) * 1.2f; // 1.2x to ensure full coverage
                
                Vector2 position = new Vector2(Main.screenWidth / 2f, Main.screenHeight / 2f);
                Vector2 origin = new Vector2(overlayTexture.Width / 2f, overlayTexture.Height / 2f);
                
                Color overlayColor = Color.White * darkenIntensity; // 100% opacity - полное затемнение
                
                // Don't call Begin/End, SpriteBatch is already active
                // Just draw directly
                spriteBatch.Draw(
                    overlayTexture,
                    position,
                    null,
                    overlayColor,
                    0f,
                    origin,
                    scale,
                    SpriteEffects.None,
                    0f
                );
            }
        }

        /// <summary>
        /// Reset effect state (useful for cleanup)
        /// </summary>
        public static void Reset()
        {
            IsActive = false;
            darkenIntensity = 0f;
            isFadingIn = false;
            isFadingOut = false;
        }
    }
}
