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

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            InputEvents.ButtonPressed += this.InputEvents_ButtonPressed;
        }


        /*********
        ** Private methods
        *********/
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

                if (controls.ToggleLookup.Contains(e.Button))
                    this.ToggleLookup(LookupMode.Cursor);
                else if (controls.ToggleLookupInFrontOfPlayer.Contains(e.Button))
                    this.ToggleLookup(LookupMode.FacingPlayer);
        }

        /****
        ** Helpers
        ****/
        /// <summary>Show the lookup UI for the current target.</summary>
        /// <param name="lookupMode">The lookup target mode.</param>
        private void ToggleLookup(LookupMode lookupMode)
        {
        }
    }
}
