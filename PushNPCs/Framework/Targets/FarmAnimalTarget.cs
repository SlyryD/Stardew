using Microsoft.Xna.Framework;
using StardewValley;

namespace SlyryD.Stardew.PushNPCs.Framework.Targets
{
    /// <summary>Positional metadata about a farm animal.</summary>
    internal class FarmAnimalTarget : GenericTarget
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="obj">The underlying in-game object.</param>
        public FarmAnimalTarget(GameHelper gameHelper, FarmAnimal obj)
            : base(gameHelper, TargetType.FarmAnimal, obj, obj.getTileLocation(), obj.Position) { }

        /// <summary>Get the area occupied by the target (absolute).</summary>
        public override Rectangle GetOccupiedArea()
        {
            FarmAnimal animal = (FarmAnimal)this.Value;
            return animal.GetBoundingBox();
        }

        /// <summary>Get a rectangle that roughly bounds the visible sprite (absolute).</summary>
        public override Rectangle GetSpriteArea()
        {
            FarmAnimal animal = (FarmAnimal)this.Value;
            return this.GetSpriteArea(animal.GetBoundingBox(), animal.Sprite.SourceRect);
        }
    }
}
