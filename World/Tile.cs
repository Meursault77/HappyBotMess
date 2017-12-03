using Barely.Util;
using LD40.Actors;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD40.World
{
    public class Tile
    {

        int x;
        int y;
        public Point position { get { return new Point(x, y); } }
        public TileType tileType;
        public Sprite groundSprite;
        private Blockable blockable = null;
        public Actor actorOnTop;

        public Tile(int x, int y, TileType tileType, Blockable blockable)
        {
            this.tileType = tileType;
            this.blockable = blockable;
            this.x = x;
            this.y = y;
        }

        public bool IsWalkable()
        {
            return tileType == TileType.Normal && actorOnTop == null && blockable == null;
        }

        public bool HasCover()
        {
            return blockable != null;
        }

        public void Render(SpriteBatch spriteBatch)
        {
            Point pos = new Point(x, y) * Map.tileSize;
            groundSprite?.Render(spriteBatch, new Rectangle(pos, Map.tileSize));
            if (blockable != null)
                blockable.sprite.Render(spriteBatch, pos);
        }
    }


    public enum TileType
    {
        Empty,
        Normal
    }
}
