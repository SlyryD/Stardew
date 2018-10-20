using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using SlyryD.Stardew.Common;
using SlyryD.Stardew.PushNPCs.Framework;
using SlyryD.Stardew.PushNPCs.Components;
using SlyryD.Stardew.PushNPCs.Framework.Constants;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using SlyryD.Stardew.PushNPCs.Framework.Targets;

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
            this.TargetFactory = new TargetFactory(this.Helper.Translation, this.Helper.Reflection, this.GameHelper);
            this.DebugInterface = new DebugInterface(this.GameHelper, this.TargetFactory, this.Config, this.Monitor);

            // hook up events
            //TimeEvents.AfterDayStarted += this.TimeEvents_AfterDayStarted;
            InputEvents.ButtonPressed += this.InputEvents_ButtonPressed;
            GraphicsEvents.OnPostRenderHudEvent += this.GraphicsEvents_OnPostRenderHudEvent;
        }

        /// <summary>The method invoked when the player presses a controller, keyboard, or mouse button.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void InputEvents_ButtonPressed(object sender, EventArgsInput e)
        {
            // ignore if player hasn't loaded a save yet
            if (!Context.IsWorldReady)
                return;

            // perform bound action
            var controls = this.Config.Controls;

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
                this.DebugInterface.Draw();
        }

        /****
        ** Helpers
        ****/
        /// <summary>Push the NPC in front of the player.</summary>
        private void Push()
        {
            // TODO: Search positions in front of player instead of using tiles
            ITarget target = this.TargetFactory.GetTargetFromTile(Game1.currentLocation, Game1.player.getTileLocation())
                ?? this.TargetFactory.GetTargetFromTile(Game1.currentLocation, this.GetTileInFront());
            if (target != null) // TODO: Figure out when this is null
            {
                switch (target.Type)
                {
                    case TargetType.Horse:
                    case TargetType.Junimo:
                    case TargetType.Monster:
                    case TargetType.Pet:
                    case TargetType.Villager:
                        var npc = target.GetValue<NPC>();
                        this.Monitor.Log("NPC: " + npc.Name);
                        npc.setTileLocation(2 * npc.getTileLocation() - Game1.player.getTileLocation());
                        break;
                    case TargetType.FarmAnimal:
                        var animal = target.GetValue<FarmAnimal>();
                        this.Monitor.Log("Animal: " + animal.Name);
                        animal.setTileLocation(2 * animal.getTileLocation() - Game1.player.getTileLocation());
                        break;
                    case TargetType.Farmer:
                        var farmer = target.GetValue<Farmer>();
                        this.Monitor.Log("Farmer: " + farmer.Name);
                        farmer.setTileLocation(2 * farmer.getTileLocation() - Game1.player.getTileLocation());
                        break;
                    default:
                        var obj = target.GetValue<StardewValley.Object>();
                        this.Monitor.Log("Unpushable object: " + obj.Name);
                        break;
                }
            }
        }

        private Vector2 FacingDirectionToTileOffset(int facingDirection)
        {
            switch (facingDirection)
            {
                case 0:
                    return Character.AdjacentTilesOffsets[2];
                case 1:
                    return Character.AdjacentTilesOffsets[0];
                case 2:
                    return Character.AdjacentTilesOffsets[3];
                case 3:
                    return Character.AdjacentTilesOffsets[1];
                default:
                    throw new Exception("Invalid facing direction");
            }
        }

        private Vector2 GetTileInFront()
        {
            Farmer player = Game1.player;
            return player.getTileLocation() + this.FacingDirectionToTileOffset(player.FacingDirection);
            //this.Monitor.Log("Position: " + string.Join(",", player.Position));
            //this.Monitor.Log("Position in front: " + string.Join(",", player.Position + Game1.tileSize * this.FacingDirectionToTileOffset(player.FacingDirection)));
        }
    }
}
