using Microsoft.Xna.Framework;

namespace SlyryD.Stardew.PushNPCs.Framework.Targets
{
    /// <summary>Positional metadata about an object in the world.</summary>
    internal interface ITarget
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The target type.</summary>
        TargetType Type { get; set; }

        /// <summary>The underlying in-game object.</summary>
        object Value { get; set; }

        /// <summary>The object's tile position in the current location (if applicable).</summary>
        Vector2? Tile { get; set; }

        /// <summary>The object's position in the current location (if applicable).</summary>
        Vector2? Position { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Get the target's tile position, or throw an exception if it doesn't have one.</summary>
        /// <exception cref="System.InvalidOperationException">The target doesn't have a tile position.</exception>
        Vector2 GetTile();

        /// <summary>Get whether the object is at the specified map tile position.</summary>
        /// <param name="position">The map tile position.</param>
        bool IsAtTile(Vector2 position);

        /// <summary>Get the target's position, or throw an exception if it doesn't have one.</summary>
        /// <exception cref="System.InvalidOperationException">The target doesn't have a position.</exception>
        Vector2 GetPosition();

        /// <summary>Get whether the object is at the specified map position.</summary>
        /// <param name="position">The map position.</param>
        bool IsAtPosition(Vector2 position);

        /// <summary>Get a strongly-typed value.</summary>
        /// <typeparam name="T">The expected value type.</typeparam>
        T GetValue<T>();

        /// <summary>Get the area occupied by the target (absolute).</summary>
        Rectangle GetOccupiedArea();

        /// <summary>Get a rectangle that roughly bounds the visible sprite (absolute).</summary>
        Rectangle GetSpriteArea();
    }
}
