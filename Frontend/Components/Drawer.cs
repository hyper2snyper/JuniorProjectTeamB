﻿using System.Drawing;
using System.Windows.Media.Imaging;
using Drawing = System.Drawing;
using JuniorProject.Backend;
using System.Windows;
using System.Drawing.Drawing2D;
using JuniorProject.Backend.Helpers;
using Controls = System.Windows.Controls;
using Colors = System.Drawing;
using System.Windows.Controls;
using System.Windows.Media;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Specialized;
using JuniorProject.Backend.Agents;
using System.Xml.Linq;
using JuniorProject.Backend.WorldData;
using System.Collections.Generic;

namespace JuniorProject.Frontend.Components
{
    public class SpriteInfo
    {
        public int x1, y1, width, height;
    }

    public class Drawer
    {
        public int tileSize;
        Vector2Int mapPixelSize;

        Boolean firstPass = true;

        Bitmap worldBitmap;
        Canvas Canvas;

        string spriteSheetPath = $"{Properties.Resources.ProjectDir}\\Frontend\\GUI\\Sprites\\SpriteSheet.png";
        string jsonPath = $"{Properties.Resources.ProjectDir}\\Frontend\\GUI\\Sprites\\sprites.json";
        public Dictionary<string, SpriteInfo> sprites { get; set; }
        Bitmap spriteSheet;

        private TileMap tileMap;

        public Queue<Drawable> drawables;
        private Dictionary<(int, int), List<int>> drawableGridLocations;

        Dictionary<string, Bitmap> bitmapCache = new Dictionary<string, Bitmap>();
        Dictionary<(int, int), Bitmap> tileMapBitmapCache = new Dictionary<(int, int), Bitmap>();
        Dictionary<string, ImageSource> preloadedSprites = new Dictionary<string, ImageSource>();

        Dictionary<string, CachedDrawable> cachedUnits = new Dictionary<string, CachedDrawable>();
        Dictionary<(int, int), CachedDrawable> cachedTiles = new Dictionary<(int, int), CachedDrawable>();
        Dictionary<(int, int), CachedDrawable> cachedBuildings = new Dictionary<(int, int), CachedDrawable>();

        int total = 0;

        Drawable map;
        Drawable grid;

        World world;

        public Drawer(ref Canvas mapCanvas)
        {
            Canvas = mapCanvas;
            drawables = new Queue<Drawable>();
            drawableGridLocations = new Dictionary<(int, int), List<int>>();
        }

        public void Initialize()
        {
            preloadAllSprites();
            tileSize = ClientCommunicator.GetData<int>("tileSize");
            mapPixelSize = ClientCommunicator.GetData<Vector2Int>("mapPixelSize");
            worldBitmap = ClientCommunicator.GetData<Drawing.Bitmap>("WorldImage");
            world = ClientCommunicator.GetData<World>("World");



            tileMap = ClientCommunicator.GetData<TileMap>("TileMap");
            tileMap.TilesChanged += OnTilesChange;

            world.RedrawAction += OnTilesChange;

            Debug.Print(String.Format("{0:N}", tileSize));
            if (tileSize != default(int) && mapPixelSize.X != default(int) && mapPixelSize.Y != default(int) && worldBitmap != default(Bitmap))
            {
                Debug.Print("Successfully loaded map values onto frontend. . .");
            }
            // Initialize Map and Gridlines so they are not constantly redrawn
            Controls.Image mapImage = new Controls.Image
            {
                Width = worldBitmap.Width,
                Height = worldBitmap.Height,
                Source = TransferToWriteableBitmap(worldBitmap),
            };
            map = new Drawable(mapImage, true, "WorldMap", 0);

            Controls.Image gridImage = new Controls.Image
            {
                Width = worldBitmap.Width,
                Height = worldBitmap.Height,
                Source = TransferToWriteableBitmap(GetGridlines())
            };
            grid = new Drawable(gridImage, true, "Grid", 1);
        }

        private void preloadAllSprites()
        {
            int amountOfImagesAdded = 0;
            string jsonData = File.ReadAllText(jsonPath);
            sprites = JsonConvert.DeserializeObject<Dictionary<string, SpriteInfo>>(jsonData);
            spriteSheet = new Bitmap(spriteSheetPath);

            string imageSource = "SpriteSheet";

            foreach (var sprite in sprites) {
                Rectangle section = new Rectangle(sprite.Value.x1, sprite.Value.y1, sprite.Value.width, sprite.Value.height);
                Bitmap bitmap = spriteSheet.Clone(section, spriteSheet.PixelFormat);
                ImageSource source = TransferToWriteableBitmap(bitmap);

                if (!preloadedSprites.TryAdd(sprite.Key, source))
                {
                    Debug.Print($"!!!ERROR: COULD NOT ADD SPRITE TO 'preloadedSprites' {sprite.Key}");
                }
                else {
                    amountOfImagesAdded += 1;
                }
            }
            Debug.Print($"Total Images Preloaded: {amountOfImagesAdded}");
        }

        ImageSource getPreloadedSprite(string sprite)
        {
            return preloadedSprites[sprite];
        }

        private void OnTilesChange()
        {
            Application.Current?.Dispatcher.Invoke(Draw);
        }

        public void checkMouseClick(int x, int y)
        {
            Vector2Int p = ConvertPixelsToGridPosition(x, y);

            List<Controls.Image> images = new List<Controls.Image>();
            List<string> titles = new List<string>();
            List<string> information = new List<string>();

            TileMap.Tile tile = tileMap.getTile(p);
            Bitmap tileBitmap = extractTileFromMap(p.X * tileSize, p.Y * tileSize, 32, 32);
            ImageSource imageSource = TransferToWriteableBitmap(tileBitmap);

            Controls.Image tileImage = new Controls.Image
            {
                Width = tileBitmap.Width,
                Height = tileBitmap.Height,
                Source = imageSource
            };

            images.Add(tileImage);
            titles.Add("Tile");
            information.Add(getTileInformation(ref tile));

            foreach (Mob m in tile.Occupants) {
                Controls.Image mobImage = new Controls.Image
                {
                    Width = getPreloadedSprite(m.GetSprite()).Width,
                    Height = getPreloadedSprite(m.GetSprite()).Height,
                    Source = getPreloadedSprite(m.GetSprite())
                };

                images.Add(mobImage);
                titles.Add(m.GetSprite());
                information.Add($"Health -> 100\nPosition -> [{tile.pos.X}, {tile.pos.Y}]"); // this is cap for now but it's ok
            }

            string color = tile.Owner != null ? tile.Owner.color : null;

            AccordionInfoModal im = new AccordionInfoModal($"Tile - [{p.X}, {p.Y}]", color, images, titles, information);
            im.Show();
        }

        public string getTileInformation(ref TileMap.Tile t)
        {
            return $"Movement Cost -> {t.movementCost}\nElevation Average -> {t.elevationAvg}\nImpassible? -> {t.impassible.ToString()}\nTeam -> {t.Owner?.name}";
        }

        public Bitmap extractFromSprite(string name)
        {
            if (bitmapCache.ContainsKey(name))
            {
                return bitmapCache[name];
            }
            if (sprites.TryGetValue(name, out SpriteInfo targetSprite))
            {
                Rectangle section = new Rectangle(targetSprite.x1, targetSprite.y1, targetSprite.width, targetSprite.height);
                Bitmap bitmap = spriteSheet.Clone(section, spriteSheet.PixelFormat);
                bitmapCache.Add(name, bitmap); 
                return bitmap;
            }
            else
            {
                Debug.Print(String.Format("ERROR! Cannot find sprite {0:S}", name));
            }
            return null;
        }

        public Bitmap extractTileFromMap(int x1, int y1, int width, int height)
        {
            if(tileMapBitmapCache.ContainsKey((x1,y1)))
            {
                return tileMapBitmapCache[(x1,y1)];
            }
            Rectangle section = new Rectangle(x1, y1, width, height);
            return worldBitmap.Clone(section, spriteSheet.PixelFormat);
        }

        public void ClearCanvas()
        {
            drawables.Clear();
            drawableGridLocations.Clear();
        }

        public void PopulateCanvas()
        {
            // DRAW NEW STUFF
            foreach (Drawable d in drawables)
            {
                if (d != null && d.shouldDraw)
                {
                    if (!d.isMapOrGridlines)
                    {
                        Canvas.SetLeft(d.image, d.pixelPosition.X);
                        Canvas.SetTop(d.image, d.pixelPosition.Y);
                    }
                    Panel.SetZIndex(d.image, d.layer);
                    Canvas.Children.Add(d.image);
                }
            }

            foreach (var d in cachedUnits)
            {
                if (d.Value.shouldMove) {
                    Canvas.SetLeft(d.Value.image, d.Value.pixelPosition.X);
                    Canvas.SetTop(d.Value.image, d.Value.pixelPosition.Y);
                    d.Value.shouldMove = false;
                }
            }
        }

        public void SetGridlines()
        {
            grid.shouldDraw = !grid.shouldDraw;
            Draw();
        }

        public void Draw()
        {
            List<GenericDrawable> genericDrawables = new List<GenericDrawable>();
            world.PopulateDrawablesList(ref genericDrawables);

            ClearCanvas();

            if (firstPass) {
                drawables.Enqueue(map);
                drawables.Enqueue(grid);
                firstPass = false;
            }

            for (int i = 0; i < 2; i++) {
                foreach (GenericDrawable d in genericDrawables)
                {
                    if (d.sprite == null || d.sprite == "") continue;
                    if (d.layer == i) {
                        AddToDrawables(d.sprite, d.gridPosition, d.uniqueIdentifier, d.sprite);
                    }
                }
            }

            checkCachedDrawingsToDelete();

            //DebugImages();
            PopulateCanvas();
        }

        public void DebugImages()
        {
            /*  vvvv FOR DEBUGGING/TESTING IMAGE SOURCES BELOW vvvv   */
            //AddImageToCanvas("TestSpriteRed_1", $"{Properties.Resources.ProjectDir}\\Frontend\\Images\\Sprites\\IndividualImages\\TestSpriteRed.png", 3, 2);
            //AddImageToCanvas("TestSpriteYellow_1", $"{Properties.Resources.ProjectDir}\\Frontend\\Images\\Sprites\\IndividualImages\\TestSpriteYellow.png", 10, 15);

            //AddImageToCanvas("YellowKingdom", $"{Properties.Resources.ProjectDir}\\Frontend\\Images\\Sprites\\IndividualImages\\YellowCastle.png", 3, 3);
            //AddImageToCanvas("YellowSoldier", $"{Properties.Resources.ProjectDir}\\Frontend\\Images\\Sprites\\IndividualImages\\YellowSoldier.png", 3, 4);
            //AddImageToCanvas("YellowArcher", $"{Properties.Resources.ProjectDir}\\Frontend\\Images\\Sprites\\IndividualImages\\YellowArcher.png", 3, 5);
            //AddImageToCanvas("YellowVillage", $"{Properties.Resources.ProjectDir}\\Frontend\\Images\\Sprites\\IndividualImages\\YellowHouse.png", 3, 6);

            //AddImageToCanvas("GreenKingdom", $"{Properties.Resources.ProjectDir}\\Frontend\\Images\\Sprites\\IndividualImages\\GreenCastle.png", 4, 3);
            //AddImageToCanvas("GreenSoldier", $"{Properties.Resources.ProjectDir}\\Frontend\\Images\\Sprites\\IndividualImages\\GreenSoldier.png", 4, 4);
            //AddImageToCanvas("GreenArcher", $"{Properties.Resources.ProjectDir}\\Frontend\\Images\\Sprites\\IndividualImages\\GreenArcher.png", 4, 5);
            //AddImageToCanvas("GreenVillage", $"{Properties.Resources.ProjectDir}\\Frontend\\Images\\Sprites\\IndividualImages\\GreenHouse.png", 4, 6);

            //AddImageToCanvas("RedKingdom", $"{Properties.Resources.ProjectDir}\\Frontend\\Images\\Sprites\\IndividualImages\\RedCastle.png", 5, 3);
            //AddImageToCanvas("RedSoldier", $"{Properties.Resources.ProjectDir}\\Frontend\\Images\\Sprites\\IndividualImages\\RedSoldier.png", 5, 4);
            //AddImageToCanvas("RedArcher", $"{Properties.Resources.ProjectDir}\\Frontend\\Images\\Sprites\\IndividualImages\\RedArcher.png", 5, 5);
            //AddImageToCanvas("RedVillage", $"{Properties.Resources.ProjectDir}\\Frontend\\Images\\Sprites\\IndividualImages\\RedHouse.png", 5, 6);

            //AddImageToCanvas("WheatFarm", $"{Properties.Resources.ProjectDir}\\Frontend\\Images\\Sprites\\IndividualImages\\WheatFarm.png", 6, 3);
            //AddImageToCanvas("Mine", $"{Properties.Resources.ProjectDir}\\Frontend\\Images\\Sprites\\IndividualImages\\Mine.png", 6, 4);

            //AddImageToCanvas("Bread", $"{Properties.Resources.ProjectDir}\\Frontend\\Images\\Sprites\\IndividualImages\\FoodResource.png", 7, 3);
            //AddImageToCanvas("Gold", $"{Properties.Resources.ProjectDir}\\Frontend\\Images\\Sprites\\IndividualImages\\GoldResource.png", 7, 4);
            //AddImageToCanvas("Wood", $"{Properties.Resources.ProjectDir}\\Frontend\\Images\\Sprites\\IndividualImages\\WoodResource.png", 7, 5);
            //AddImageToCanvas("Stone", $"{Properties.Resources.ProjectDir}\\Frontend\\Images\\Sprites\\IndividualImages\\StoneResource.png", 7, 6);
            //AddImageToCanvas("Iron", $"{Properties.Resources.ProjectDir}\\Frontend\\Images\\Sprites\\IndividualImages\\IronResource.png", 7, 7);
            //AddImageToCanvas("SwordUnit", $"{Properties.Resources.ProjectDir}\\Frontend\\Images\\Sprites\\IndividualImages\\SoldierUnitResource.png", 7, 8);
            //AddImageToCanvas("ArcherUnit", $"{Properties.Resources.ProjectDir}\\Frontend\\Images\\Sprites\\IndividualImages\\ArcherUnitResource.png", 7, 9);

            /*  vvvv FOR DEBUGGING/TESTING IMAGE SPRITES BELOW vvvv   */
            //AddBitmapToCanvas("YellowCastle", extractFromSprite("YellowCastle"), 3, 3);
            //AddBitmapToCanvas("YellowSoldier", extractFromSprite("YellowSoldier"), 3, 4);
            //AddBitmapToCanvas("YellowArcher", extractFromSprite("YellowArcher"), 3, 5);
            //AddBitmapToCanvas("YellowHouse", extractFromSprite("YellowHouse"), 3, 6);

            //AddBitmapToCanvas("RedCastle", extractFromSprite("RedCastle"), 4, 3);
            //AddBitmapToCanvas("RedSoldier", extractFromSprite("RedSoldier"), 4, 4);
            //AddBitmapToCanvas("RedArcher", extractFromSprite("RedArcher"), 4, 5);
            //AddBitmapToCanvas("RedHouse", extractFromSprite("RedHouse"), 4, 6);

            //AddBitmapToCanvas("GreenCastle", extractFromSprite("GreenCastle"), 5, 3);
            //AddBitmapToCanvas("GreenSoldier", extractFromSprite("GreenSoldier"), 5, 4);
            //AddBitmapToCanvas("GreenArcher", extractFromSprite("GreenArcher"), 5, 5);
            //AddBitmapToCanvas("GreenHouse", extractFromSprite("GreenHouse"), 5, 6);

            //AddBitmapToCanvas("FoodResource", extractFromSprite("FoodResource"), 6, 3);
            //AddBitmapToCanvas("Gold", extractFromSprite("GoldResource"), 6, 4);
            //AddBitmapToCanvas("Wood", extractFromSprite("WoodResource"), 6, 5);
            //AddBitmapToCanvas("Stone", extractFromSprite("StoneResource"), 6, 6);
            //AddBitmapToCanvas("Iron", extractFromSprite("IronResource"), 6, 7);
            //AddBitmapToCanvas("Soldier", extractFromSprite("SoldierResource"), 6, 8);
            //AddBitmapToCanvas("Archer", extractFromSprite("ArcherResource"), 6, 9);

            //AddBitmapToCanvas("WheatFarm", extractFromSprite("WheatFarm"), 7, 3);
            //AddBitmapToCanvas("Mine", extractFromSprite("Mine"), 7, 4);

            //AddBitmapToCanvas("YellowDock", extractFromSprite("YellowDock"), 8, 3);
            //AddBitmapToCanvas("YellowShip", extractFromSprite("YellowShip"), 8, 4);

            //AddBitmapToCanvas("GreenDock", extractFromSprite("GreenDock"), 9, 3);
            //AddBitmapToCanvas("GreenShip", extractFromSprite("GreenShip"), 9, 4);

            //AddBitmapToCanvas("RedDock", extractFromSprite("RedDock"), 10, 3);
            //AddBitmapToCanvas("RedShip", extractFromSprite("RedShip"), 10, 4);
        }

        public void checkCachedDrawingsToDelete() {
            foreach (var u in cachedUnits)
            {
                if (u.Value.shouldDelete) {
                    Canvas.Children.Remove(u.Value.image);
                }
                u.Value.shouldDelete = true;
            }
            foreach (var u in cachedTiles)
            {
                if (u.Value.shouldDelete)
                {
                    Canvas.Children.Remove(u.Value.image);
                }
                u.Value.shouldDelete = true;
            }
            foreach (var u in cachedBuildings)
            {
                if (u.Value.shouldDelete)
                {
                    Canvas.Children.Remove(u.Value.image);
                }
                u.Value.shouldDelete = true;
            }
        }

        public void AddToDrawables(string name, Vector2Int gridPos, string uniqueIdentifier, string sprite)
        {
            string imageSource = "SpriteSheet";
            Vector2Int pixelPosition = ConvertGridPositionToPixels(gridPos.X, gridPos.Y);

            // UNITS
            if (uniqueIdentifier != null)
            {
                if (!cachedUnits.ContainsKey(uniqueIdentifier) || (cachedUnits[uniqueIdentifier].sprite != sprite))
                {
                    if (cachedUnits.ContainsKey(uniqueIdentifier)) { 
                        Canvas.Children.Remove(cachedUnits[uniqueIdentifier].image);
                    }

                    Controls.Image newUnitImage = new Controls.Image
                    {
                        Source = getPreloadedSprite(name),
                        Width = getPreloadedSprite(name).Width,
                        Height = getPreloadedSprite(name).Height,
                    };
                    drawables.Enqueue(new Drawable(newUnitImage, true, name, imageSource, pixelPosition, gridPos, 4));
                    cachedUnits[uniqueIdentifier] = new CachedDrawable(newUnitImage, pixelPosition, gridPos);
                    return;
                }

                if (cachedUnits[uniqueIdentifier].gridPosition.X != gridPos.X || cachedUnits[uniqueIdentifier].gridPosition.Y != gridPos.Y)
                {
                    cachedUnits[uniqueIdentifier] = new CachedDrawable(cachedUnits[uniqueIdentifier].image, pixelPosition, gridPos);
                    cachedUnits[uniqueIdentifier].shouldMove = true;
                    cachedUnits[uniqueIdentifier].shouldDelete = false;
                    return;
                }

                cachedUnits[uniqueIdentifier].shouldDelete = false;
                return;
            }
            //Dictionary<(int, int), CachedDrawable> cachedTiles = new Dictionary<(int, int), CachedDrawable>();
            // TILES
            if (name.Contains("Tile"))
            {
                if (!cachedTiles.ContainsKey((gridPos.X, gridPos.Y)) || cachedTiles[(gridPos.X, gridPos.Y)].team != name)
                {
                    if (cachedTiles.ContainsKey((gridPos.X, gridPos.Y)))
                    {
                        Canvas.Children.Remove(cachedTiles[(gridPos.X, gridPos.Y)].image);
                    }

                    Controls.Image newTile = new Controls.Image
                    {
                        Source = getPreloadedSprite(name),
                        Width = getPreloadedSprite(name).Width,
                        Height = getPreloadedSprite(name).Height,
                    };
                    cachedTiles[(gridPos.X, gridPos.Y)] = new CachedDrawable(newTile, pixelPosition, gridPos, name);
                    drawables.Enqueue(new Drawable(newTile, true, "Tile", "SpriteSheet", pixelPosition, gridPos, 2));
                    Debug.Print($"ADDED TILE: {name} [{gridPos.X}, {gridPos.Y}]");
                }
                cachedTiles[(gridPos.X, gridPos.Y)].shouldDelete = false;
                return;
            }

            // BUILDINGS
            if (!cachedBuildings.ContainsKey((gridPos.X, gridPos.Y)) || cachedBuildings[(gridPos.X, gridPos.Y)].team != name)
            {
                if (cachedBuildings.ContainsKey((gridPos.X, gridPos.Y)))
                {
                    Canvas.Children.Remove(cachedBuildings[(gridPos.X, gridPos.Y)].image);
                }

                Controls.Image newBuilding = new Controls.Image
                {
                    Source = getPreloadedSprite(name),
                    Width = getPreloadedSprite(name).Width,
                    Height = getPreloadedSprite(name).Height,
                };
                cachedBuildings[(gridPos.X, gridPos.Y)] = new CachedDrawable(newBuilding, pixelPosition, gridPos, name);
                drawables.Enqueue(new Drawable(newBuilding, true, name, "SpriteSheet", pixelPosition, gridPos, 3));
                return;
            }
            else
            {
                cachedBuildings[(gridPos.X, gridPos.Y)].shouldDelete = false;
            }
        }

        public Vector2Int ConvertGridPositionToPixels(int x, int y)
        {
            // based on current Sprite being 20x20 sprites
            int xPixelWidth = x * tileSize;
            int yPixelWidth = y * tileSize;
            return new Vector2Int(xPixelWidth, yPixelWidth);
        }

        public Vector2Int ConvertPixelsToGridPosition(int x, int y)
        {
            int xGrid = x / tileSize;
            int yGrid = y / tileSize;
            return new Vector2Int(xGrid, yGrid);
        }
        public WriteableBitmap TransferToWriteableBitmap(Bitmap worldBitmap)
        {
            WriteableBitmap map = new WriteableBitmap(worldBitmap.Width, worldBitmap.Height, 96, 96, PixelFormats.Bgra32, null);
            byte[] pixels = new byte[4 * (worldBitmap.Width * worldBitmap.Height)]; //pixel color buffer. each color is four bytes.
            for (int y = 0; y < worldBitmap.Width; y++)
            {
                for (int x = 0; x < worldBitmap.Height; x++)
                {
                    Drawing.Color c = worldBitmap.GetPixel(x, y);
                    int pos = ((y * worldBitmap.Width) + x) * 4;
                    pixels[pos] = c.B;
                    pixels[pos + 1] = c.G;
                    pixels[pos + 2] = c.R;
                    pixels[pos + 3] = c.A;
                }
            }
            map.WritePixels(new Int32Rect(0, 0, worldBitmap.Width, worldBitmap.Height), pixels, (worldBitmap.Width * 4), 0); //Update the bitmap
            return map;
        }

        public Bitmap GetGridlines()
        {
            Bitmap gridBitmap = new Bitmap(mapPixelSize.X, mapPixelSize.Y);

            using (Graphics g = Graphics.FromImage(gridBitmap))
            {
                g.Clear(Colors.Color.Transparent);
                g.SmoothingMode = SmoothingMode.None;
                g.PixelOffsetMode = PixelOffsetMode.None;
                using (Colors.Pen gridPen = new Colors.Pen(Colors.Color.Black, 2))
                {
                    gridPen.Alignment = PenAlignment.Inset;
                    for (int y = tileSize; y < gridBitmap.Width; y += tileSize)
                    {
                        g.DrawLine(gridPen, 0, y, mapPixelSize.X, y);
                    }

                    for (int x = tileSize; x < gridBitmap.Height; x += tileSize)
                    {
                        g.DrawLine(gridPen, x, 0, x, mapPixelSize.Y);
                    }
                }
            }

            return gridBitmap;
        }
    }
}