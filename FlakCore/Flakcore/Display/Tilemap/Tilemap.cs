using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Flakcore.Display;
using Flakcore;
using Flakcore.Utils;
using Flakcore.Physics;

namespace Display.Tilemap
{
    public class Tilemap : Node
    {
        public static int tileWidth { get; private set; }
        public static int tileHeight { get; private set; }

        private HashSet<string> CollisionGroups;

        private List<Layer> Layers;
        private List<Tileset> Tilesets;

        public Tilemap()
        {
            this.Layers = new List<Layer>();
            this.Tilesets = new List<Tileset>();
            this.CollisionGroups = new HashSet<string>();

            CollisionSolver.Tilemaps.Add(this);
        }

        public void loadMap(string path, int tileWidth, int tileHeight)
        {
            Tilemap.tileWidth = tileWidth;
            Tilemap.tileHeight = tileHeight;

            XDocument doc = XDocument.Load(path);

            Width = Convert.ToInt32(doc.Element("map").Attribute("width").Value);
            Height = Convert.ToInt32(doc.Element("map").Attribute("width").Value);

            // load all tilesets
            foreach (XElement element in doc.Descendants("tileset"))
            {
                string assetName = element.Element("image").Attribute("source").Value;
                assetName = Path.GetFileNameWithoutExtension(assetName);

                // load all collisionGroups from this tileset ('collisionGroups' from different tiles)
                string[] tileCollisionGroups = new string[100];


                foreach (XElement tile in element.Descendants("tile"))
                {
                    if (tile.Descendants("property").First().Attribute("name").Value == "collisionGroups")
                    {
                        int index = (int)tile.Attribute("id");
                        string groupName = tile.Descendants("property").First().Attribute("value").Value;

                        tileCollisionGroups[index] = groupName;
                        this.CollisionGroups.Add(groupName);
                    }
                }

                Tileset tileset = new Tileset(Convert.ToInt32(element.Attribute("firstgid").Value), element.Attribute("name").Value, Convert.ToInt32(element.Element("image").Attribute("width").Value), Convert.ToInt32(element.Element("image").Attribute("height").Value), GameManager.content.Load<Texture2D>(assetName), tileCollisionGroups);

                Tilesets.Add(tileset);
            }

            // load all layers
            foreach (XElement element in doc.Descendants("layer"))
            {
                Layer layer = new Layer(element.Attribute("name").Value, Convert.ToInt32(element.Attribute("width").Value), Convert.ToInt32(element.Attribute("height").Value), this);

                int x = 0;
                int y = 0;

                foreach (XElement tile in element.Descendants("tile"))
                {
                    if (Convert.ToInt32(tile.Attribute("gid").Value) == 0)
                    {
                        x++;
                        if (x >= layer.Width)
                        {
                            y++;
                            x = 0;
                        }
                        continue;
                    }

                    layer.addTile(Convert.ToInt32(tile.Attribute("gid").Value), x, y, getCorrectTileset(Convert.ToInt32(tile.Attribute("gid").Value)));
                    x++;

                    // check if y needs to be incremented
                    if (x >= layer.Width)
                    {
                        y++;
                        x = 0;
                    }
                }


                Layers.Add(layer);
            }
        }

        private Tileset getCorrectTileset(int gid)
        {
            Tileset best = null;

            foreach (Tileset tileset in Tilesets)
            {
                if (best == null)
                    best = tileset;
                else
                {
                    if (gid >= tileset.firstGid && tileset.firstGid > best.firstGid)
                        best = tileset;
                }
            }

            return best;
        }

        public override void Draw(SpriteBatch spriteBatch, Matrix parentTransform)
        {
            // loop through all layers to draw them
            foreach (Layer layer in Layers)
            {
                layer.Draw(spriteBatch, Matrix.Identity);
            }
        }

        public override List<Node> getAllChildren(List<Node> nodes)
        {
            foreach (Layer layer in Layers)
            {
                layer.getAllChildren(nodes);
            }

            return nodes;
        }

        internal bool HasTileCollisionGroup(string groupName)
        {
            return this.CollisionGroups.Contains(groupName);
        }

        internal void GetCollidedTiles(Node node, List<Node> collidedNodes)
        {
            foreach (Layer layer in this.Layers)
            {
                layer.GetCollidedTiles(node, collidedNodes);
            }
        }
    }
}
