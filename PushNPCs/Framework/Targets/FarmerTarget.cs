using Microsoft.Xna.Framework;
using SFarmer = StardewValley.Farmer;

namespace SlyryD.Stardew.PushNPCs.Framework.Targets
{
    /// <summary>Positional metadata about a farmer (i.e. player).</summary>
    internal class FarmerTarget : GenericTarget
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="farmer">The underlying in-game object.</param>
        public FarmerTarget(GameHelper gameHelper, SFarmer farmer)
            : base(gameHelper, TargetType.Farmer, farmer, farmer.getTileLocation(), farmer.Position) { }

        /// <summary>Get the area occupied by the target (absolute).</summary>
        public override Rectangle GetOccupiedArea()
        {
            SFarmer farmer = (SFarmer)this.Value;
            return farmer.GetBoundingBox();
        }

        /// <summary>Get a rectangle that roughly bounds the visible sprite (absolute).</summary>
        public override Rectangle GetSpriteArea()
        {
            SFarmer farmer = (SFarmer)this.Value;
            return this.GetSpriteArea(farmer.GetBoundingBox(), farmer.FarmerSprite.SourceRect);
        }
    }
}
