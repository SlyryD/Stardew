using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using SlyryD.Stardew.Common;
using SlyryD.Stardew.PushNPCs.Framework.Constants;
using SlyryD.Stardew.PushNPCs.Framework.Targets;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Monsters;

namespace SlyryD.Stardew.PushNPCs.Framework
{
    /// <summary>Finds and analyses lookup targets in the world.</summary>
    internal class TargetFactory
    {
        /*********
        ** Properties
        *********/
        /// <summary>Simplifies access to private game code.</summary>
        private readonly IReflectionHelper Reflection;

        /// <summary>Provides translations stored in the mod folder.</summary>
        private readonly ITranslationHelper Translations;

        /// <summary>Provides utility methods for interacting with the game code.</summary>
        private readonly GameHelper GameHelper;


        /*********
        ** Public methods
        *********/
        /****
        ** Constructors
        ****/
        /// <summary>Construct an instance.</summary>
        /// <param name="translations">Provides translations stored in the mod folder.</param>
        /// <param name="reflection">Simplifies access to private game code.</param>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        public TargetFactory(ITranslationHelper translations, IReflectionHelper reflection, GameHelper gameHelper)
        {
            this.Translations = translations;
            this.Reflection = reflection;
            this.GameHelper = gameHelper;
        }

        /****
        ** Targets
        ****/
        /// <summary>Get all potential targets in the current location.</summary>
        /// <param name="location">The current location.</param>
        public IEnumerable<ITarget> GetTargetsInLocation(GameLocation location)
        {
            // NPCs
            foreach (NPC npc in location.characters)
            {
                TargetType type = TargetType.Unknown;
                if (npc is Child || npc.isVillager())
                    type = TargetType.Villager;
                else if (npc is Horse)
                    type = TargetType.Horse;
                else if (npc is Junimo)
                    type = TargetType.Junimo;
                else if (npc is Pet)
                    type = TargetType.Pet;
                else if (npc is Monster)
                    type = TargetType.Monster;

                yield return new CharacterTarget(this.GameHelper, type, npc, this.Reflection);
            }

            // animals
            foreach (FarmAnimal animal in (location as Farm)?.animals.Values ?? (location as AnimalHouse)?.animals.Values ?? Enumerable.Empty<FarmAnimal>())
            {
                yield return new FarmAnimalTarget(this.GameHelper, animal);
            }

            // players
            foreach (Farmer farmer in location.farmers)
            {
                if (farmer == Game1.player)
                {
                    continue;
                }

                yield return new FarmerTarget(this.GameHelper, farmer);
            }
        }

        /// <summary>Get the target on the specified tile.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="tile">The tile to search.</param>
        public ITarget GetTarget(GameLocation location)
        {
            Rectangle facingRectangle = this.GetFacingRectangle(Game1.player);
            return (
                from target in this.GetTargetsInLocation(location)
                where
                    target.Type != TargetType.Unknown
                    && facingRectangle.Intersects(target.GetSpriteArea())
                select target
            ).FirstOrDefault();
        }

        /*********
        ** Private methods
        *********/
        /// <summary>Get a rectangle in front of the player.</summary>
        /// <param name="player">The player to check.</param>
        public Rectangle GetFacingRectangle(Farmer player)
        {
            Vector2 position = player.Position - new Vector2(Game1.viewport.X, Game1.viewport.Y);
            FacingDirection direction = (FacingDirection)player.FacingDirection;
            switch (direction)
            {
                case FacingDirection.Up:
                    // { { X: 0 Y: -64 Width: 64 Height: 64} }
                    return new Rectangle((int)position.X, (int)position.Y - Constant.TileSize, Constant.TileSize, Constant.TileSize);
                case FacingDirection.Right:
                    // { { X: 48 Y: -32 Width: 64 Height: 64} }
                    return new Rectangle((int)position.X + 3 * Constant.QuarterTileSize, (int)position.Y - Constant.HalfTileSize, Constant.TileSize, Constant.TileSize);
                case FacingDirection.Down:
                    // { { X: 0 Y: -32 Width: 64 Height: 64} }
                    return new Rectangle((int)position.X, (int)position.Y - Constant.HalfTileSize, Constant.TileSize, Constant.TileSize);
                case FacingDirection.Left:
                    // { { X: -48 Y: -32 Width: 64 Height: 64} }
                    return new Rectangle((int)position.X - 3 * Constant.QuarterTileSize, (int)position.Y - Constant.HalfTileSize, Constant.TileSize, Constant.TileSize);
                default:
                    throw new NotSupportedException($"Unknown facing direction {direction}");
            }
        }
    }
}
