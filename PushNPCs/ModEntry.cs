using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using SlyryD.Stardew.Common;
using SlyryD.Stardew.PushNPCs.Components;
using SlyryD.Stardew.PushNPCs.Framework;
using SlyryD.Stardew.PushNPCs.Framework.Constants;
using SlyryD.Stardew.PushNPCs.Framework.Targets;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;

namespace SlyryD.Stardew.PushNPCs
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        /*********
        ** Properties
        *********/
        /// <summary>The mod configuration.</summary>
        private ModConfig Config;

        /****
        ** State
        ****/
        /// <summary>Provides utility methods for interacting with the game code.</summary>
        private GameHelper GameHelper;

        /// <summary>Finds and analyses lookup targets in the world.</summary>
        private TargetFactory TargetFactory;

        /// <summary>Draws debug information to the screen.</summary>
        private DebugInterface DebugInterface;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            // load config
            this.Config = this.Helper.ReadConfig<ModConfig>();

            // hook up events
            GameEvents.FirstUpdateTick += this.GameEvents_FirstUpdateTick;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method invoked on the first update tick, once all mods are initialised.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void GameEvents_FirstUpdateTick(object sender, EventArgs e)
        {
            // initialise functionality
            this.GameHelper = new GameHelper();
            this.TargetFactory = new TargetFactory(this.Helper.Reflection, this.GameHelper);
            this.DebugInterface = new DebugInterface(this.GameHelper, this.TargetFactory, this.Config, this.Monitor);

            // hook up events
            //TimeEvents.AfterDayStarted += this.TimeEvents_AfterDayStarted;
            InputEvents.ButtonPressed += this.InputEvents_ButtonPressed;
            GraphicsEvents.OnPostRenderHudEvent += this.GraphicsEvents_OnPostRenderHudEvent;
            GraphicsEvents.OnPostRenderEvent += this.GraphicsEvents_OnPostRenderEvent;
        }

        /// <summary>The method invoked when the player presses a controller, keyboard, or mouse button.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void InputEvents_ButtonPressed(object sender, EventArgsInput e)
        {
            // ignore if player hasn't loaded a save yet
            if (!Context.IsWorldReady)
            {
                return;
            }

            // perform bound action
            ModConfig.ModConfigControls controls = this.Config.Controls;

            if (controls.Push.Contains(e.Button))
            {
                this.Push();
            }
            else if (controls.ToggleDebug.Contains(e.Button) && Context.IsPlayerFree)
            {
                this.DebugInterface.Enabled = !this.DebugInterface.Enabled;
            }
        }

        /// <summary>The method invoked when the interface is rendering.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void GraphicsEvents_OnPostRenderHudEvent(object sender, EventArgs e)
        {
            // render debug interface
            if (this.DebugInterface.Enabled)
            {
                this.DebugInterface.Draw();
            }
        }

        /// <summary>The method invoked when the world is rendering.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void GraphicsEvents_OnPostRenderEvent(object sender, EventArgs e)
        {
            if (this.DebugInterface.Enabled)
            {
                Rectangle facingRectangle = this.TargetFactory.GetFacingRectangle(Game1.player);
                CommonHelper.DrawLine(Game1.spriteBatch, facingRectangle.Left, facingRectangle.Top, new Vector2(facingRectangle.Width, facingRectangle.Height), Color.DarkRed);
            }
        }


        /****
        ** Helpers
        ****/
        /// <summary>Push the target in front of the player.</summary>
        private void Push()
        {
            ITarget target = this.TargetFactory.GetTarget(Game1.currentLocation);
            if (target != null)
            {
                switch (target.Type)
                {
                    case TargetType.Horse:
                    case TargetType.Junimo:
                    case TargetType.Monster:
                    case TargetType.Pet:
                    case TargetType.Villager:
                        NPC npc = target.GetValue<NPC>();
                        this.Monitor.Log("NPC: " + npc.Name);
                        npc.Position += this.GetPushVector(target, npc.Name);
                        break;
                    case TargetType.FarmAnimal:
                        FarmAnimal animal = target.GetValue<FarmAnimal>();
                        this.Monitor.Log("Animal: " + animal.Name);
                        animal.Position += this.GetPushVector(target, animal.Name);
                        break;
                    case TargetType.Farmer:
                        Farmer farmer = target.GetValue<Farmer>();
                        this.Monitor.Log("Farmer: " + farmer.Name);
                        farmer.Position += this.GetPushVector(target, farmer.Name);
                        break;
                    default:
                        StardewValley.Object obj = target.GetValue<StardewValley.Object>();
                        this.Monitor.Log("Unpushable object: " + obj.Name);
                        break;
                }
            }
        }

        /// <summary>
        /// Get push vector for a target that does not put the target in an invalid location
        /// </summary>
        /// <param name="target">Target to push</param>
        /// <param name="name">Name of the target</param>
        /// <returns>Push vector</returns>
        private Vector2 GetPushVector(ITarget target, string name)
        {
            Vector2 direction = this.TileOffsetFromFacingDirection(Game1.player.FacingDirection);
            for (int magnitude = Game1.tileSize; magnitude > 0; magnitude--)
            {
                Vector2 pushVector = magnitude * direction;
                Rectangle destinationArea = target.GetOccupiedArea();
                destinationArea.Offset((int)pushVector.X, (int)pushVector.Y);

                IEnumerable<Vector2> tiles = this.TilesFromAbsoluteArea(destinationArea);
                if (tiles.All(tile => this.IsPassable(Game1.currentLocation, tile, destinationArea) && !this.IsOccupied(Game1.currentLocation, tile, destinationArea, name)))
                {
                    return pushVector;
                }
            }

            return new Vector2(0, 0);
        }

        /// <summary>
        /// Get tiles intersecting the area rectangle (absolute)
        /// </summary>
        /// <param name="absoluteArea">The rectangle to check for intersecting tiles</param>
        /// <returns>Tiles intersecting the area</returns>
        private IEnumerable<Vector2> TilesFromAbsoluteArea(Rectangle absoluteArea)
        {
            int left = absoluteArea.Left / Constant.TileSize;
            int top = absoluteArea.Top / Constant.TileSize;
            int right = (int)Math.Ceiling(absoluteArea.Right / (double)Constant.TileSize);
            int bottom = (int)Math.Ceiling(absoluteArea.Bottom / (double)Constant.TileSize);
            for (int x = left; x < right; ++x)
            {
                for (int y = top; y < bottom; ++y)
                {
                    yield return new Vector2(x, y);
                }
            }
        }

        /// <summary>Get whether a tile is passable.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="tile">The tile to check.</param>
        /// <param name="tilePixels">The tile area in pixels.</param>
        /// <remarks>Derived from <see cref="Farmer.MovePosition"/>, <see cref="GameLocation.isCollidingPosition(Rectangle,xTile.Dimensions.Rectangle,bool)"/>, <see cref="GameLocation.isTilePassable(Location,xTile.Dimensions.Rectangle)"/>, and <see cref="Fence"/>.</remarks>
        private bool IsPassable(GameLocation location, Vector2 tile, Rectangle tilePixels)
        {
            // check layer properties
            if (location.isTilePassable(new xTile.Dimensions.Location((int)tile.X, (int)tile.Y), Game1.viewport))
            {
                return true;
            }

            // allow bridges
            if (location.doesTileHaveProperty((int)tile.X, (int)tile.Y, "Passable", "Buildings") != null)
            {
                xTile.Tiles.Tile backTile = location.map.GetLayer("Back").PickTile(new xTile.Dimensions.Location(tilePixels.X, tilePixels.Y), Game1.viewport.Size);
                if (backTile == null || !backTile.TileIndexProperties.TryGetValue("Passable", out xTile.ObjectModel.PropertyValue value) || value != "F")
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Get whether a tile is blocked due to something it contains.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="tile">The tile to check.</param>
        /// <param name="tilePixels">The tile area in pixels.</param>
        /// <param name="name">The name of the thing we are pushing, so we do not consider it as already occupying the destination.</param>
        /// <remarks>Derived from <see cref="GameLocation.isCollidingPosition(Rectangle,xTile.Dimensions.Rectangle,bool)"/> and <see cref="Farm.isCollidingPosition(Rectangle,xTile.Dimensions.Rectangle,bool,int,bool,Character,bool,bool,bool)"/>.</remarks>
        private bool IsOccupied(GameLocation location, Vector2 tile, Rectangle tilePixels, string name)
        {
            // show open gate as passable
            if (location.objects.TryGetValue(tile, out StardewValley.Object obj) && obj is Fence fence && fence.isGate.Value && fence.gatePosition.Value == Fence.gateOpenedPosition)
            {
                return false;
            }

            // check for objects, characters, or terrain features
            if (location.isTileOccupiedIgnoreFloors(tile, name))
            {
                return true;
            }

            // buildings
            if (location is BuildableGameLocation buildableLocation)
            {
                foreach (Building building in buildableLocation.buildings)
                {
                    Rectangle buildingArea = new Rectangle(building.tileX.Value, building.tileY.Value, building.tilesWide.Value, building.tilesHigh.Value);
                    if (buildingArea.Contains((int)tile.X, (int)tile.Y))
                    {
                        return true;
                    }
                }
            }

            // large terrain features
            if (location.largeTerrainFeatures.Any(p => p.getBoundingBox().Intersects(tilePixels)))
            {
                return true;
            }

            // resource clumps
            if (location is Farm farm)
            {
                if (farm.resourceClumps.Any(p => p.getBoundingBox(p.tile.Value).Intersects(tilePixels)))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Get the tile offset of the adjacent tile the player is facing.
        /// </summary>
        /// <param name="facingDirection">The direction the player is facing.</param>
        /// <returns>Tile offset</returns>
        private Vector2 TileOffsetFromFacingDirection(int facingDirection)
        {
            switch ((FacingDirection)facingDirection)
            {
                case FacingDirection.Up:
                    return Character.AdjacentTilesOffsets[2];
                case FacingDirection.Right:
                    return Character.AdjacentTilesOffsets[0];
                case FacingDirection.Down:
                    return Character.AdjacentTilesOffsets[3];
                case FacingDirection.Left:
                    return Character.AdjacentTilesOffsets[1];
                default:
                    throw new Exception("Invalid facing direction");
            }
        }
    }
}
