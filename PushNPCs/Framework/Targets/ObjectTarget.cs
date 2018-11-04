using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SlyryD.Stardew.Common;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using Object = StardewValley.Object;

namespace SlyryD.Stardew.PushNPCs.Framework.Targets
{
    /// <summary>Positional metadata about a world object.</summary>
    internal class ObjectTarget : GenericTarget
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Simplifies access to private game code.</summary>
        private readonly IReflectionHelper Reflection;

        /// <summary>The item sprite.</summary>
        private readonly SpriteInfo CustomSprite;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="obj">The underlying in-game object.</param>
        /// <param name="tilePosition">The object's tile position in the current location (if applicable).</param>
        /// <param name="position">The object's position in the current location (if applicable).</param>
        /// <param name="reflection">Simplifies access to private game code.</param>
        public ObjectTarget(GameHelper gameHelper, Object obj, Vector2? tilePosition, Vector2? position, IReflectionHelper reflection)
            : base(gameHelper, TargetType.Object, obj, tilePosition, position)
        {
            this.Reflection = reflection;
            this.CustomSprite = gameHelper.GetSprite(obj, onlyCustom: true); // only get sprite if it's custom; else we'll use contextual logic (e.g. for fence direction)
        }

        /// <summary>Get the area occupied by the target (absolute).</summary>
        public override Rectangle GetOccupiedArea()
        {
            // get object info
            Object obj = (Object)this.Value;
            return obj.getBoundingBox(this.GetTile());
        }

        /// <summary>Get a rectangle that roughly bounds the visible sprite (absolute).</summary>
        public override Rectangle GetSpriteArea()
        {
            // get object info
            Object obj = (Object)this.Value;
            Rectangle boundingBox = obj.getBoundingBox(this.GetTile());

            // get sprite area
            if (this.CustomSprite != null)
            {
                Rectangle spriteArea = this.GetSpriteArea(boundingBox, this.CustomSprite.SourceRectangle);
                return new Rectangle(
                    x: spriteArea.X,
                    y: spriteArea.Y - (spriteArea.Height / 2), // custom sprite areas are offset from game logic
                    width: spriteArea.Width,
                    height: spriteArea.Height
                );
            }
            if (obj is Furniture furniture)
            {
                return this.GetSpriteArea(boundingBox, furniture.sourceRect.Value);
            }

            if (obj.bigCraftable.Value)
            {
                return this.GetSpriteArea(boundingBox, Object.getSourceRectForBigCraftable(obj.ParentSheetIndex));
            }

            if (obj is Fence fence)
            {
                return this.GetSpriteArea(boundingBox, this.GetSourceRectangle(fence, Game1.currentLocation));
            }
            else
            {
                return this.GetSpriteArea(boundingBox, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, obj.ParentSheetIndex, Object.spriteSheetTileSize, Object.spriteSheetTileSize));
            }
        }

        /// <summary>Get the source rectangle for a fence texture.</summary>
        /// <param name="fence">The fence object.</param>
        /// <param name="location">The location containing the fence target.</param>
        /// <remarks>Reverse-engineered from <see cref="Fence.draw(SpriteBatch,int,int,float)"/>.</remarks>
        private Rectangle GetSourceRectangle(Fence fence, GameLocation location)
        {
            int spriteID = 1;
            if (fence.health.Value > 1.0)
            {
                int index = 0;
                Vector2 tile = fence.TileLocation;

                // connected to right fence
                tile.X += 1;
                if (location.objects.ContainsKey(tile) && location.objects[tile] is Fence && ((Fence)location.objects[tile]).countsForDrawing(fence.whichType.Value))
                {
                    index += 100;
                }

                // connected to left fence
                tile.X -= 2;
                if (location.objects.ContainsKey(tile) && location.objects[tile] is Fence && ((Fence)location.objects[tile]).countsForDrawing(fence.whichType.Value))
                {
                    index += 10;
                }

                // connected to top fence
                tile.X += 1;
                tile.Y += 1;
                if (location.objects.ContainsKey(tile) && location.objects[tile] is Fence && ((Fence)location.objects[tile]).countsForDrawing(fence.whichType.Value))
                {
                    index += 500;
                }

                // connected to bottom fence
                tile.Y -= 2;
                if (location.objects.ContainsKey(tile) && location.objects[tile] is Fence && ((Fence)location.objects[tile]).countsForDrawing(fence.whichType.Value))
                {
                    index += 1000;
                }

                if (fence.isGate.Value)
                {
                    if (index == 110)
                    {
                        return new Rectangle(fence.gatePosition.Value == Fence.gateOpenedPosition ? 24 : 0, 128, 24, 32);
                    }

                    if (index == 1500)
                    {
                        return new Rectangle(fence.gatePosition.Value == Fence.gateClosedPosition ? 16 : 0, 160, 16, 16);
                    }

                    spriteID = Fence.sourceRectForSoloGate;
                }
                else
                {
                    spriteID = Fence.fenceDrawGuide[index];
                }
            }

            Texture2D texture = this.Reflection.GetField<Lazy<Texture2D>>(fence, "fenceTexture").GetValue().Value;
            return new Rectangle(spriteID * Fence.fencePieceWidth % texture.Bounds.Width, spriteID * Fence.fencePieceWidth / texture.Bounds.Width * Fence.fencePieceHeight, Fence.fencePieceWidth, Fence.fencePieceHeight);
        }
    }
}
