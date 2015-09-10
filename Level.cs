using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using System.IO;
using Microsoft.Xna.Framework.Input;


namespace Game1.Game
{
    class Level : IDisposable
    {

        Texture2D[] layers;
        Tile[,] tiles;
        //ContentManager Content;
        SpriteBatch spriteBatch;
        Rectangle screenRect;
        public ContentManager content;
        Vector2 startPos;
        TimeSpan timeSpan = TimeSpan.FromSeconds(120);
        int shapeChangeCount = 0;
        List<MovingPlatform> movingPlatforms;
        SpriteFont spriteFont;
        int height;
        int width;

        const int playerLayer = 1;
        Player player;

        public bool reachedExit = false;

        public Level (ContentManager pContent, SpriteBatch i_spriteBatch, int levelIndex, Rectangle i_screenRect)
        {
            // Load tiles for the level
            //CreateTiles();

            content = pContent;

            //startPos = new Vector2(10,360);

            layers = new Texture2D[4];

            spriteBatch = i_spriteBatch;
            screenRect = i_screenRect;
            spriteFont = content.Load<SpriteFont>("Font\\Font");
            for (int i = 0; i < playerLayer; ++i)
            {
                layers[i] = content.Load<Texture2D>("Graphics\\background-0" + (i+1));
            }

            
            //layers[0] = content.Load<Texture2D>("Graphics\\background-01");

        }

        public void CreateTiles (Stream fileStream)
        {
            int width;
            List<string> lines = new List<string>();
            using (StreamReader reader = new StreamReader(fileStream))
            {
                string line = reader.ReadLine();
                width = line.Length;
                while (line != null)
                {
                    lines.Add(line);
                    if (line.Length != width)
                        throw new Exception(String.Format("The length of line {0} is different from all preceeding lines.", lines.Count));
                    line = reader.ReadLine();
                }
            }

            // Allocate the tile grid.
            tiles = new Tile[width, lines.Count];
            height = lines.Count;
            this.width = width;

            for (int y = 0; y < lines.Count; ++y)
            {
                for (int x = 0; x < width; ++x)
                {
                    // to load each tile.
                    int tempType = (int)Char.GetNumericValue(lines[y][x]);

                    int tileType = (tempType == -1) ? (int)lines[y][x]: tempType ;
                    tiles[x, y] = LoadTile(tileType, x, y);
                }
            }

            
        }

        public Tile LoadTile (int pTileType,int x, int y)
        {
            if (pTileType == (int)TileType.PASSABLE)
            {
                return new Tile(null, (TileType)pTileType);
            }
            else if (pTileType == 'a')
            {
                SetPlayerPos(x, y);
                return new Tile(null, TileType.PASSABLE);
            }
            else if (pTileType == (int)TileType.PLATFORM_FLAT)
            {
                return new Tile(content.Load<Texture2D>("Graphics\\1"), (TileType)pTileType);
            }
            else if (pTileType == (int)TileType.SOLID)
            {
                return new Tile(content.Load<Texture2D>("Graphics\\2"), (TileType)pTileType);
            }
            else if (pTileType == (int)TileType.PLATFORM_INCLINE_RIGHT_TOP)
            {
                return new Tile(content.Load<Texture2D>("Graphics\\3"), (TileType)pTileType);
            }
            else if (pTileType == (int)TileType.PLATFORM_INCLINE_RIGHT_BOTTOM)
            {
                return new Tile(content.Load<Texture2D>("Graphics\\4"), (TileType)pTileType);
            }
            else if (pTileType == (int)TileType.PLATFORM_INCLINE_LEFT_TOP)
            {
                return new Tile(content.Load<Texture2D>("Graphics\\5"), (TileType)pTileType);
            }
            else if (pTileType == (int)TileType.PLATFORM_INCLINE_LEFT_BOTTOM)
            {
                return new Tile(content.Load<Texture2D>("Graphics\\6"), (TileType)pTileType);
            } 
            else if (pTileType == (int)TileType.OBSTACLE_RIGHT)
            {
                return new Tile(content.Load<Texture2D>("Graphics\\7"), (TileType)pTileType);
            }
            else if (pTileType == (int)TileType.OBSTACLE_LEFT)
            {
                return new Tile(content.Load<Texture2D>("Graphics\\8"), (TileType)pTileType);
            }
            else if (pTileType == (int)TileType.OBSTACLE_UP)
            {
                return new Tile(content.Load<Texture2D>("Graphics\\9"), (TileType)pTileType);
            }
            else if (pTileType == (int)TileType.OBSTACLE_DOWN)
            {
                return new Tile(content.Load<Texture2D>("Graphics\\10"), (TileType)pTileType);
            } else if (pTileType == 'v')
            {
                return new Tile(null, (TileType)pTileType, CreateMovingPlatform(content.Load<Texture2D>("Graphics\\BlockA1"), x, y));
            } else if (pTileType == 'z')
            {
                return new Tile(content.Load<Texture2D>("Graphics\\exit"), (TileType)pTileType);
            }

            return new Tile(null, (TileType)pTileType);
        }

        private MovingPlatform CreateMovingPlatform(Texture2D texture,int x, int y)
        {

            MovingPlatform newPlatform = null;


            if (movingPlatforms == null)
            {
                movingPlatforms = new List<MovingPlatform> ();
            }

            if (movingPlatforms.Count == 0)
            {
                newPlatform = new MovingPlatform(texture, x, y);

                movingPlatforms.Add(newPlatform);
            } else
            {
                bool check = false;
                int i;
                for (i =0; i<movingPlatforms.Count; ++i)
                {
                    if (check = movingPlatforms[i].Check(x, y))
                        break;
                }

                if (!check)
                {
                    newPlatform = new MovingPlatform(texture, x, y);

                    movingPlatforms.Add(newPlatform);
                }
                else
                    newPlatform = movingPlatforms[i];
            }

            return newPlatform;
                
        }

        private void SetPlayerPos(int x, int y)
        {
            startPos.X = (x * 40);
            startPos.Y = (y * 40);
        }

        public void LoadContent(Stream fileStream)
        {
            CreateTiles(fileStream);

            player = new Player();
            
            player.Initialize(content.Load<Texture2D>("Graphics\\dough-01"), content.Load<Texture2D>("Graphics\\dough-02"), content.Load<Texture2D>("Graphics\\dough-03"), startPos, screenRect, this);

        }

        public void Update (GameTime gameTime, KeyboardState keyboardState)
        {
            if (Math.Floor(timeSpan.TotalSeconds) != 0)
            {
                timeSpan -= gameTime.ElapsedGameTime;
            }
            
            if (movingPlatforms != null)
            {
                for (int i = 0; i < movingPlatforms.Count; ++i)
                {
                    movingPlatforms[i].Update(gameTime);
                }
            }

            player.Update(keyboardState, gameTime);

            if (player.reachedExit)
                reachedExit = true;
        }

        public MovingPlatform GetMovingPlatform (int x,int y)
        {
            return tiles[x, y].movingPlatform;
        } 

        public void Draw()
        {

            int minutes = timeSpan.Minutes;
            int seconds = timeSpan.Seconds;
            String time = "Time ";
            if (minutes == 0)
            {
                time += seconds;
            }
            else
            {
                time += minutes + ":" + seconds;
            }

       


            for (int i = 0; i < playerLayer; ++i)
            {
                spriteBatch.Draw(layers[0],                        // PLayer Texture
                                 screenRect,                       // Position of the texture
                                 null,                             // Source Rectangle ??
                                 Color.White,                      // Color applied onto the image
                                 0.0f,
                                 Vector2.Zero,
                                 SpriteEffects.FlipHorizontally,
                                 1.0f
                                 );

            }

            DrawTiles();


            if (movingPlatforms != null)
            {
                for (int i = 0; i < movingPlatforms.Count; ++i)
                {
                    movingPlatforms[i].Draw(spriteBatch);
                }
            }

            player.Draw(spriteBatch);
            spriteBatch.DrawString(spriteFont, time, new Vector2(900, 0), Color.Black);
            spriteBatch.DrawString(spriteFont, "Morphed: "+player.morphCount, new Vector2(500, 0), Color.Black);

        }

        public void DrawTiles ()
        {
            for (int y=0;y < 18; ++y)
            {
                for (int x = 0; x< 32; ++x)
                {
                    Texture2D tileTex = tiles[x, y].tileTexture;
                    if (tileTex != null) {

                        Rectangle pos = new Rectangle(x * 40, y * 40,40,40);
                        spriteBatch.Draw(tileTex, pos, Color.White);
                        spriteBatch.Draw(tileTex,                      // PLayer Texture
                                 pos,                     // Destination Rectangle
                                 null,                           // Source Rectangle ??
                                 Color.White);
                    }
                }
            }
        }

        public void Dispose()
        {
            content.Unload();
        }

        public void Reset ()
        {
            if (movingPlatforms != null)
            {
                for (int i = 0; i < movingPlatforms.Count; ++i)
                {
                    movingPlatforms[i].Reset();
                }
            }

            player.Reset(startPos);
            timeSpan = TimeSpan.FromSeconds(120);
            reachedExit = false;
        }

        public TileType GetCollision(int x, int y)
        {
            // Prevent escaping past the level ends.
            if (x < 0 || x >= width)
                return TileType.SOLID;
            // Allow jumping past the level top and falling through the bottom.
            if (y < 0 || y >= height)
                return TileType.SOLID;

            return tiles[x, y].tileType;
        }

    }
}
