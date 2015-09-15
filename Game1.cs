using Game1.Game;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;

namespace Game1
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont spriteFont;
        Level currentLevel;
        Rectangle screenRect;
        KeyboardState keyboardState;
        TimeSpan resumeTimeSpan = TimeSpan.FromSeconds(5);

        private FrameCounter frameCounter;

        GameState gameState;
        bool canToggle;

        Texture2D pauseImage;
        Texture2D[]  resumeImage;
        Texture2D gameOverImage;

        int curLevelIndex = 0;
        enum GameState
        {
            GS_RUNNING = 0,
            GS_PAUSED = 1,
            GS_RESUMING = 2,
            GS_GAME_OVER
       
        }
       

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);

            graphics.IsFullScreen = false;
            graphics.PreferredBackBufferHeight = 720;
            graphics.PreferredBackBufferWidth = 1280;

            Content.RootDirectory = "Content";

            frameCounter = new FrameCounter();
            
            gameState = GameState.GS_RESUMING;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            //player1 = new Player();

            //player2 = new Player();

            

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            //spriteFont = new SpriteFont(;

            screenRect = new Rectangle(0, 0, GraphicsDevice.Viewport.TitleSafeArea.Width, GraphicsDevice.Viewport.TitleSafeArea.Height);

            pauseImage = Content.Load<Texture2D>("Graphics\\pause");
            gameOverImage = Content.Load<Texture2D>("Graphics\\game_over");
            resumeImage = new Texture2D[6];
            for(int i = 0; i<=5; i++)
            {
                resumeImage[i] = Content.Load<Texture2D>("Graphics\\Resume-"+i);
            }

            canToggle = true;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {


            // Load the level.

            //level1 = new Level(Content, spriteBatch, curLevelIndex, screenRect, );


            // TODO: use this.Content to load your game content here

            LoadNextLevel();



            

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        public void LoadNextLevel ()
        {
            if (currentLevel != null)
                currentLevel.Dispose();

            ++curLevelIndex;

            currentLevel = new Level(Content, spriteBatch, 1, screenRect);

            string levelPath = string.Format("Content/Level/{0}.txt", curLevelIndex);

            if (File.Exists(levelPath))
            {
                using (Stream fileStream = TitleContainer.OpenStream(levelPath))
                {

                    currentLevel.LoadContent(fileStream);
                }

            }
            else
                gameState = GameState.GS_GAME_OVER;
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (gameState == GameState.GS_RESUMING)
            { 

                if ( Math.Floor(resumeTimeSpan.TotalSeconds)==0)
                    {
                        gameState = GameState.GS_RUNNING;
                        resumeTimeSpan = TimeSpan.FromSeconds(5);
                   
                    }
                else 
                    {
                        resumeTimeSpan -= gameTime.ElapsedGameTime;
                    }
            }

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            keyboardState = Keyboard.GetState();

            if (canToggle && keyboardState.IsKeyDown(Keys.P))
            {
                gameState = (gameState == GameState.GS_RUNNING) ? GameState.GS_PAUSED: GameState.GS_RESUMING;
                canToggle = false;
            }

            if (keyboardState.IsKeyUp(Keys.P))
            {
                canToggle = true;

            }

            if (keyboardState.IsKeyDown(Keys.R))
            {
                gameState = GameState.GS_RESUMING;
                currentLevel.Reset();
            }

            if (gameState == GameState.GS_RUNNING)
                currentLevel.Update(gameTime, keyboardState);

            if (currentLevel.reachedExit)
                LoadNextLevel();

            base.Update(gameTime);
        }



        private void drawPauseLayer ()
        {
            spriteBatch.Draw(pauseImage, new Rectangle(GraphicsDevice.Viewport.TitleSafeArea.X + 590, GraphicsDevice.Viewport.TitleSafeArea.Y + 260, 200,100), Color.White);
        }

        private void drawGameOver ()
        {
            spriteBatch.Draw(pauseImage, new Rectangle(GraphicsDevice.Viewport.TitleSafeArea.X + 590, GraphicsDevice.Viewport.TitleSafeArea.Y + 260, 200, 100), Color.White);
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            /*var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            frameCounter.Update(deltaTime);

            var fps = string.Format("FPS: {0}", frameCounter.AverageFramesPerSecond);

            spriteBatch.DrawString(_spriteFont, fps, new Vector2(1, 1), Color.Black);*/

            GraphicsDevice.Clear(Color.White);

            // TODO: Add your drawing code here

            spriteBatch.Begin();
            if (gameState != GameState.GS_GAME_OVER)
                currentLevel.Draw();
            else
            {
                
                drawGameOver();
            }

            if (gameState == GameState.GS_PAUSED)
                drawPauseLayer();
                

            if(gameState == GameState.GS_RESUMING)
            {
               spriteBatch.Draw(resumeImage[resumeTimeSpan.Seconds], new Rectangle(GraphicsDevice.Viewport.TitleSafeArea.X + 590, GraphicsDevice.Viewport.TitleSafeArea.Y + 260, 200, 100), Color.White);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
