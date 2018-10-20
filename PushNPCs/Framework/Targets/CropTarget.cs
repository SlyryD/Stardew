using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace SlyryD.Stardew.PushNPCs.Framework.Targets
{
    /// <summary>Positional metadata about a crop.</summary>
    internal class CropTarget : GenericTarget
    {
        /*********
        ** Properties
        *********/
        /// <summary>The underlying crop.</summary>
        private readonly Crop Crop;

        /// <summary>Simplifies access to private game code.</summary>
        private readonly IReflectionHelper Reflection;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="obj">The underlying in-game object.</param>
        /// <param name="tilePosition">The object's tile position in the current location (if applicable).</param>
        /// <param name="reflectionHelper">Simplifies access to private game code.</param>
        public CropTarget(GameHelper gameHelper, TerrainFeature obj, Vector2? tilePosition, IReflectionHelper reflectionHelper)
            : base(gameHelper, TargetType.Crop, obj, tilePosition)
        {
            this.Crop = ((HoeDirt)obj).crop;
            this.Reflection = reflectionHelper;
        }

        /// <summary>Get a rectangle which roughly bounds the visible sprite relative the viewport.</summary>
        public override Rectangle GetSpriteArea()
        {
            HoeDirt tile = (HoeDirt)this.Value;
            return this.GetSpriteArea(tile.getBoundingBox(this.GetTile()), this.GetSourceRectangle(this.Crop));
        }

        /// <summary>Get whether the visible sprite intersects the specified coordinate. This can be an expensive test.</summary>
        /// <param name="tile">The tile to search.</param>
        /// <param name="position">The viewport-relative coordinates to search.</param>
        /// <param name="spriteArea">The approximate sprite area calculated by <see cref="GenericTarget.GetSpriteArea"/>.</param>
        /// <remarks>Derived from <see cref="StardewValley.Crop.draw"/>.</remarks>
        public override bool SpriteIntersectsPixel(Vector2 tile, Vector2 position, Rectangle spriteArea)
        {
            Crop crop = this.Crop;
            SpriteEffects spriteEffects = crop.flip.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            // base crop
            if (this.SpriteIntersectsPixel(tile, position, spriteArea, Game1.cropSpriteSheet, this.GetSourceRectangle(crop), spriteEffects))
                return true;

            // crop in last phase (may have fruit, be identical to base crop, or be blank)
            if (!crop.tintColor.Equals(Color.White) && crop.currentPhase.Value == crop.phaseDays.Count - 1 && !crop.dead.Value)
            {
                var sourceRectangle = new Rectangle(
                    x: (crop.fullyGrown.Value ? (crop.dayOfCurrentPhase.Value <= 0 ? 6 : 7) : crop.currentPhase.Value + 1 + 1) * 16 + (crop.rowInSpriteSheet.Value % 2 != 0 ? 128 : 0),
                    y: crop.rowInSpriteSheet.Value / 2 * 16 * 2,
                    width: 16,
                    height: 32
                );
                return this.SpriteIntersectsPixel(tile, position, spriteArea, Game1.cropSpriteSheet, sourceRectangle, spriteEffects);
            }

            return false;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the crop's source rectangle in the sprite sheet.</summary>
        /// <param name="crop">The crop.</param>
        private Rectangle GetSourceRectangle(Crop crop)
        {
            return this.Reflection.GetMethod(crop, "getSourceRect").Invoke<Rectangle>(crop.rowInSpriteSheet.Value);
        }
    }
}
