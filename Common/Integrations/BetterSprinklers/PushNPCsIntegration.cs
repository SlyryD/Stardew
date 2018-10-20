using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace SlyryD.Stardew.Common.Integrations.PushNPCs
{
    /// <summary>Handles the logic for integrating with the Push NPCs mod.</summary>
    internal class PushNPCsIntegration : BaseIntegration
    {
        /*********
        ** Properties
        *********/
        /// <summary>The mod's public API.</summary>
        private readonly IPushNPCsApi ModApi;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="modRegistry">An API for fetching metadata about loaded mods.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        public PushNPCsIntegration(IModRegistry modRegistry, IMonitor monitor)
            : base("Push NPCs", "SlyryD.PushNPCs", "1.0.0", modRegistry, monitor)
        {
            if (!this.IsLoaded)
                return;

            // get mod API
            this.ModApi = this.GetValidatedApi<IPushNPCsApi>();
            this.IsLoaded = this.ModApi != null;
        }
    }
}
