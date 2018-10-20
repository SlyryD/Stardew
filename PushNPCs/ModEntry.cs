using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using SlyryD.Stardew.Common;
using SlyryD.Stardew.PushNPCs.Framework;
using SlyryD.Stardew.PushNPCs.Framework.Constants;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

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
            this.TargetFactory = new TargetFactory(this.Metadata, this.Helper.Translation, this.Helper.Reflection, this.GameHelper);
            this.DebugInterface = new DebugInterface(this.GameHelper, this.TargetFactory, this.Config, this.Monitor);

            // hook up events
            //TimeEvents.AfterDayStarted += this.TimeEvents_AfterDayStarted;
            InputEvents.ButtonPressed += this.InputEvents_ButtonPressed;
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
                    this.Push();
        }

        /****
        ** Helpers
        ****/
        /// <summary>Push the NPC in front of the player.</summary>
        private void Push()
        {
            this.Monitor.Log("Push");
        }
    }
}
