using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Monsters;

namespace SlyryD.Stardew.PushNPCs.Framework.Targets
{
    /// <summary>Positional metadata about an NPC.</summary>
    internal class CharacterTarget : GenericTarget
    {
        /*********
        ** Properties
        *********/
        /// <summary>Simplifies access to private game code.</summary>
        private readonly IReflectionHelper Reflection;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="type">The target type.</param>
        /// <param name="obj">The underlying in-game object.</param>
        /// <param name="reflectionHelper">Simplifies access to private game code.</param>
        public CharacterTarget(GameHelper gameHelper, TargetType type, NPC obj, IReflectionHelper reflectionHelper)
            : base(gameHelper, type, obj, obj.getTileLocation(), obj.Position)
        {
            this.Reflection = reflectionHelper;
        }

        /// <summary>Get the area occupied by the target (absolute).</summary>
        public override Rectangle GetOccupiedArea()
        {
            NPC npc = (NPC)this.Value;
            return npc.GetBoundingBox(); // the 'occupied' area at the NPC's feet
        }

        /// <summary>Get a rectangle that roughly bounds the visible sprite (absolute).</summary>
        public override Rectangle GetSpriteArea()
        {
            NPC npc = (NPC)this.Value;
            AnimatedSprite sprite = npc.Sprite;
            Rectangle boundingBox = npc.GetBoundingBox(); // the 'occupied' area at the NPC's feet

            // calculate y origin
            int yOrigin;
            if (npc is DustSpirit)
            {
                yOrigin = boundingBox.Bottom;
            }
            else if (npc is Bat)
            {
                yOrigin = boundingBox.Center.Y;
            }
            else if (npc is Bug)
            {
                yOrigin = boundingBox.Top - sprite.SpriteHeight * Game1.pixelZoom + (int)(System.Math.Sin(Game1.currentGameTime.TotalGameTime.Milliseconds / 1000.0 * (2.0 * System.Math.PI)) * 10.0);
            }
            else if (npc is SquidKid squidKid)
            {
                int yOffset = this.Reflection.GetField<int>(squidKid, "yOffset").GetValue();
                yOrigin = boundingBox.Bottom - sprite.SpriteHeight * Game1.pixelZoom + yOffset;
            }
            else
            {
                yOrigin = boundingBox.Top;
            }

            // get bounding box
            int height = sprite.SpriteHeight * Game1.pixelZoom;
            int width = sprite.SpriteWidth * Game1.pixelZoom;
            int x = boundingBox.Center.X - (width / 2);
            int y = yOrigin + boundingBox.Height - height + npc.yJumpOffset * 2;

            return new Rectangle(x, y, width, height);
        }
    }
}
