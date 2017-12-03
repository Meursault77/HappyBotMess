using Barely.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LD40.World
{
    public class Tileset
    {
        public static Dictionary<string, Tileset> tilesets;

        public static void LoadTilesets(XmlDocument tilesetDef, Texture2D atlas)
        {
            tilesets = new Dictionary<string, Tileset>();
            foreach (XmlNode n in tilesetDef.SelectNodes("tilesets/tileset"))
            {
                string name = n.Attributes["name"].Value;
                Tileset t = new Tileset(n, atlas);
                tilesets.Add(name, t);
            }
        }

        private Sprite[] groundTiles;

        public Tileset(XmlNode tilesetDef, Texture2D atlas)
        {
            Point tileSize = Map.tileSize;

            groundTiles = new Sprite[16];
            foreach(XmlNode n in tilesetDef.SelectNodes("ground/t"))
            {
                int index   = int.Parse(n.Attributes["auto"].Value);
                int x       = int.Parse(n.Attributes["x"].Value);
                int y       = int.Parse(n.Attributes["y"].Value);
                groundTiles[index] = new Sprite(atlas, new Rectangle(new Point(x,y) * tileSize, tileSize));
            }

        }

        public Sprite GetGroundTile(int autoTile)
        {
            Debug.Assert(autoTile >= 0 && autoTile < 16);
            return groundTiles[autoTile];
        }

    }
}
