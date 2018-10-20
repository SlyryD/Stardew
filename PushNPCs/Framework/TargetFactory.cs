using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using SlyryD.Stardew.PushNPCs.Framework.Constants;
using SlyryD.Stardew.PushNPCs.Framework.Targets;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.TerrainFeatures;
using SObject = StardewValley.Object;

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
        /// <summary>Get all potential lookup targets in the current location.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="originTile">The tile from which to search for targets.</param>
        /// <param name="includeMapTile">Whether to allow matching the map tile itself.</param>
        public IEnumerable<ITarget> GetNearbyTargets(GameLocation location, Vector2 originTile)
        {
            // NPCs
            foreach (NPC npc in location.characters)
            {
                if (!this.GameHelper.CouldSpriteOccludeTile(npc.getTileLocation(), originTile))
                    continue;

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

                yield return new CharacterTarget(this.GameHelper, type, npc, npc.getTileLocation(), this.Reflection);
            }

            // animals
            foreach (FarmAnimal animal in (location as Farm)?.animals.Values ?? (location as AnimalHouse)?.animals.Values ?? Enumerable.Empty<FarmAnimal>())
            {
                if (!this.GameHelper.CouldSpriteOccludeTile(animal.getTileLocation(), originTile))
                    continue;

                yield return new FarmAnimalTarget(this.GameHelper, animal, animal.getTileLocation());
            }

            // players
            foreach (Farmer farmer in location.farmers)
            {
                if (!this.GameHelper.CouldSpriteOccludeTile(farmer.getTileLocation(), originTile))
                    continue;

                yield return new FarmerTarget(this.GameHelper, farmer);
            }
        }

        /// <summary>Get the target on the specified tile.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="tile">The tile to search.</param>
        /// <param name="includeMapTile">Whether to allow matching the map tile itself.</param>
        public ITarget GetTargetFromTile(GameLocation location, Vector2 tile)
        {
            return (
                from target in this.GetNearbyTargets(location, tile)
                where
                    target.Type != TargetType.Unknown
                    && target.Value != Game1.player
                    && target.IsAtTile(tile)
                select target
            ).FirstOrDefault();
        }

        /// <summary>Get the target at the specified coordinate.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="tile">The tile to search.</param>
        /// <param name="position">The viewport-relative pixel coordinate to search.</param>
        /// <param name="includeMapTile">Whether to allow matching the map tile itself.</param>
        public ITarget GetTargetFromScreenCoordinate(GameLocation location, Vector2 tile, Vector2 position)
        {
            // get target sprites which might overlap cursor position (first approximation)
            Rectangle tileArea = this.GameHelper.GetScreenCoordinatesFromTile(tile);
            var candidates = (
                from target in this.GetNearbyTargets(location, tile)
                let spriteArea = target.GetSpriteArea()
                let isAtTile = target.IsAtTile(tile)
                where
                    target.Type != TargetType.Unknown
                    && (isAtTile || spriteArea.Intersects(tileArea))
                orderby
                    target.Type != TargetType.Tile ? 0 : 1, // Tiles are always under anything else.
                    spriteArea.Y descending,                // A higher Y value is closer to the foreground, and will occlude any sprites behind it.
                    spriteArea.X ascending                  // If two sprites at the same Y coordinate overlap, assume the left sprite occludes the right.

                select new { target, spriteArea, isAtTile }
            ).ToArray();

            // choose best match
            return
                candidates.FirstOrDefault(p => p.target.SpriteIntersectsPixel(tile, position, p.spriteArea))?.target // sprite pixel under cursor
                ?? candidates.FirstOrDefault(p => p.isAtTile)?.target; // tile under cursor
        }

        /*********
        ** Private methods
        *********/
        /// <summary>Get the tile the player is facing.</summary>
        /// <param name="player">The player to check.</param>
        private Vector2 GetFacingTile(Farmer player)
        {
            Vector2 tile = player.getTileLocation();
            FacingDirection direction = (FacingDirection)player.FacingDirection;
            switch (direction)
            {
                case FacingDirection.Up:
                    return tile + new Vector2(0, -1);
                case FacingDirection.Right:
                    return tile + new Vector2(1, 0);
                case FacingDirection.Down:
                    return tile + new Vector2(0, 1);
                case FacingDirection.Left:
                    return tile + new Vector2(-1, 0);
                default:
                    throw new NotSupportedException($"Unknown facing direction {direction}");
            }
        }
    }
}
