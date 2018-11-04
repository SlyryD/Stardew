using System;
using Microsoft.Xna.Framework;
using StardewValley;

namespace SlyryD.Stardew.PushNPCs.Framework.Targets
{
    /// <summary>Positional metadata about an object in the world.</summary>
    internal abstract class GenericTarget : ITarget
    {
        /*********
        ** Properties
        *********/
        /// <summary>Provides utility methods for interacting with the game code.</summary>
        private readonly GameHelper GameHelper;


        /*********
        ** Accessors
        *********/
        /// <summary>The target type.</summary>
        public TargetType Type { get; set; }

        /// <summary>The underlying in-game object.</summary>
        public object Value { get; set; }

        /// <summary>The object's tile position in the current location (if applicable).</summary>
        public Vector2? Tile { get; set; }

        /// <summary>The object's position in the current location (if applicable).</summary>
        public Vector2? Position { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Get the target's tile position, or throw an exception if it doesn't have one.</summary>
        /// <exception cref="InvalidOperationException">The target doesn't have a tile position.</exception>
        public Vector2 GetTile()
        {
            if (this.Tile == null)
            {
                throw new InvalidOperationException($"This {this.Type} target doesn't have a tile position.");
            }

            return this.Tile.Value;
        }

        /// <summary>Get whether the object is at the specified map tile position.</summary>
        /// <param name="position">The map tile position.</param>
        public bool IsAtTile(Vector2 position)
        {
            return this.Tile != null && this.Tile == position;
        }

        /// <summary>Get the target's position, or throw an exception if it doesn't have one.</summary>
        /// <exception cref="InvalidOperationException">The target doesn't have a position.</exception>
        public Vector2 GetPosition()
        {
            if (this.Position == null)
            {
                throw new InvalidOperationException($"This {this.Type} target doesn't have a position.");
            }

            return this.Position.Value;
        }

        /// <summary>Get whether the object is at the specified map position.</summary>
        /// <param name="position">The map position.</param>
        public bool IsAtPosition(Vector2 position)
        {
            return this.Position != null && this.Position == position;
        }

        /// <summary>Get a strongly-typed instance.</summary>
        /// <typeparam name="T">The expected value type.</typeparam>
        public T GetValue<T>()
        {
            return (T)this.Value;
        }

        /// <summary>Get the area occupied by the target (absolute).</summary>
        public virtual Rectangle GetOccupiedArea()
        {
            Rectangle occupiedArea = this.GameHelper.GetScreenCoordinatesFromTile(this.GetTile());
            occupiedArea.Offset(Game1.viewport.X, Game1.viewport.Y);
            return occupiedArea;
        }

        /// <summary>Get a rectangle that roughly bounds the visible sprite (absolute).</summary>
        public virtual Rectangle GetSpriteArea()
        {
            Rectangle spriteArea = this.GameHelper.GetScreenCoordinatesFromTile(this.GetTile());
            spriteArea.Offset(Game1.viewport.X, Game1.viewport.Y);
            return spriteArea;
        }


        /*********
        ** Protected methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="type">The target type.</param>
        /// <param name="obj">The underlying in-game object.</param>
        /// <param name="tilePosition">The object's tile position in the current location (if applicable).</param>
        protected GenericTarget(GameHelper gameHelper, TargetType type, object obj, Vector2? tilePosition = null, Vector2? position = null)
        {
            this.GameHelper = gameHelper;
            this.Type = type;
            this.Value = obj;
            this.Tile = tilePosition;
            this.Position = position;
        }

        /// <summary>Get a rectangle that roughly bounds the visible sprite (absolute).</summary>
        /// <param name="boundingBox">The occupied 'floor space' at the bottom of the sprite in the world.</param>
        /// <param name="sourceRectangle">The sprite's source rectangle in the sprite sheet.</param>
        protected Rectangle GetSpriteArea(Rectangle boundingBox, Rectangle sourceRectangle)
        {
            int height = sourceRectangle.Height * Game1.pixelZoom;
            int width = sourceRectangle.Width * Game1.pixelZoom;
            int x = boundingBox.Center.X - (width / 2);
            int y = boundingBox.Y + boundingBox.Height - height;
            return new Rectangle(x, y, width, height);
        }
    }
}
