using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SlyryD.Stardew.Common;
using SlyryD.Stardew.PushNPCs.Framework;
using SlyryD.Stardew.PushNPCs.Framework.Constants;
using SlyryD.Stardew.PushNPCs.Framework.Targets;
using StardewModdingAPI;
using StardewValley;

namespace SlyryD.Stardew.PushNPCs.Components
{
    /// <summary>Draws debug information to the screen.</summary>
    internal class DebugInterface
    {
        /*********
        ** Properties
        *********/
        /// <summary>Provides utility methods for interacting with the game code.</summary>
        private readonly GameHelper GameHelper;

        /// <summary>Finds and analyses lookup targets in the world.</summary>
        private readonly TargetFactory TargetFactory;

        /// <summary>Encapsulates monitoring and logging.</summary>
        private readonly IMonitor Monitor;

        /// <summary>The warning text to display when debug mode is enabled.</summary>
        private readonly string WarningText;


        /*********
        ** Accessors
        *********/
        /// <summary>Whether the debug interface is enabled.</summary>
        public bool Enabled { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="targetFactory">Finds and analyses lookup targets in the world.</param>
        /// <param name="config">The mod configuration.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        public DebugInterface(GameHelper gameHelper, TargetFactory targetFactory, ModConfig config, IMonitor monitor)
        {
            // save fields
            this.GameHelper = gameHelper;
            this.TargetFactory = targetFactory;
            this.Monitor = monitor;

            // generate warning text
            this.WarningText = $"Debug info enabled; press {string.Join(" or ", config.Controls.ToggleDebug)} to disable.";
        }

        /// <summary>Draw debug metadata to the screen.</summary>
        public void Draw()
        {
            if (!this.Enabled)
                return;

            this.Monitor.InterceptErrors("drawing debug info", () =>
            {
                // get location info
                GameLocation currentLocation = Game1.currentLocation;
                Vector2 cursorTile = Game1.currentCursorTile;
                Vector2 cursorPosition = this.GameHelper.GetScreenCoordinatesFromCursor();

                // show 'debug enabled' warning + cursor position
                this.GameHelper.ShowInfoMessage($"{this.WarningText} Cursor tile ({cursorTile.X}, {cursorTile.Y}), position ({cursorPosition.X}, {cursorPosition.Y}).");
            }, this.OnDrawError);
        }


        /*********
        ** Public methods
        *********/
        /// <summary>The method invoked when an unhandled exception is intercepted.</summary>
        /// <param name="ex">The intercepted exception.</param>
        private void OnDrawError(Exception ex)
        {
            this.Monitor.InterceptErrors("handling an error in the debug code", () =>
            {
                this.Enabled = false;
            });
        }
    }
}
