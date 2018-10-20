using Newtonsoft.Json;
using SlyryD.Stardew.Common;
using StardewModdingAPI;

namespace SlyryD.Stardew.PushNPCs.Framework
{
    /// <summary>The parsed mod configuration.</summary>
    internal class ModConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The control bindings.</summary>
        public ModConfigControls Controls { get; set; } = new ModConfigControls();


        /*********
        ** Nested models
        *********/
        /// <summary>A set of control bindings.</summary>
        internal class ModConfigControls
        {
            /// <summary>The control which toggles the lookup UI for something under the cursor.</summary>
            [JsonConverter(typeof(StringEnumArrayConverter))]
            public SButton[] Push { get; set; } = { SButton.P };

            /// <summary>Toggle the display of debug information.</summary>
            [JsonConverter(typeof(StringEnumArrayConverter))]
            public SButton[] ToggleDebug { get; set; } = new SButton[0];
        }
    }
}
