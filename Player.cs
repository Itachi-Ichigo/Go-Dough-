using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using Game1.Game;
using System.Collections.Generic;

namespace Game1
{
    enum playerForm
    {
        NORMAL,
        SQUARE,
        BALL
    }


    class Player
    {
        
        public SpriteEffects flip = SpriteEffects.None;

        public Texture2D playerTexture;
        public Texture2D normalTexture;
        public Texture2D squareTexture;
        public Texture2D ballTexture;


        public Vector2 position;
        public Vector2 velocity;

        public Rectangle rectangle;

        public bool active;

        private playerForm playerForm;

        public int health;

        private Level level;
        public int morphCount = 0;

        private const float moveAcceleration = 13000.0f;
        private const float ballMoveAcceleration = 25000.0f;
        private const float maxMoveSpeed = 1750.0f;
        private const float groundDragFactor = 0.50f;
        private const float airDragFactor = 0.60f;

        private const float maxJumpTime = 0.35f;
        private const float jumpLaunchVelocity = -1500.0f;
        private const float gravityAcceleration = 3400.0f;
        private const float maxFallSpeed = 550.0f;
        private const float jumpControlPower = 0.15f;

        private const float inclineCalcFactorDown = 1.1f;
        private const float inclineCalcFactorUp = 0.8f;

        private const float maxGroundRollingTime = 10.0f;
        private const float rollControlPower = 1.0f;

        private SoundEffect jumpSound;

        public bool isOnMovingPlatform = false;
        public bool wasOnMovingPlatform = false;

        public bool reachedExit = false;

        public bool IsOnGround
        {
            get { return isOnGround; }
        }
        bool isOnGround = false;

        bool wasOnGround = false;

        int isOnIncline = 0;

        private float movement;

        private bool isJumping;
        private bool wasJumping;
        private float jumpTime;

        private float rollingTime;

        private float prevBottom;
        private float prevTop;
        private float prevLeft;
        private float prevRight;

        private int direction;

        private float groundVelocity;

        private int frameWidth = 480;
        private int frameHeight = 600;

        public int Width
        {
            get { return playerTexture.Width; }
        }

        public int Height
        {
            get { return playerTexture.Height; }
        }

        /////////////////////////////////////////////////////////////////////////
        ///
        public Dictionary<string, Animation> animationSet;
        private const int FRAMETIME = 100;
        public string action;
        bool flipped;
        private Vector2 scale;
        //
        /////////////////////////////////////////////////////////////////////////
        public void Initialize(Texture2D normalTexture,Texture2D squareTexture,Texture2D ballTexture, Vector2 pPosition, Rectangle winRect, Level pLevel)
        {
            playerTexture = normalTexture;

            this.normalTexture = normalTexture;
            this.squareTexture = squareTexture;
            this.ballTexture = ballTexture;

            playerForm = playerForm.NORMAL;

            rectangle = winRect;
                 
            position = pPosition;

            velocity = Vector2.Zero;

            active = true;

            health = 100;

            level = pLevel;

            direction = 1;

            jumpSound = level.content.Load<SoundEffect>("Sound\\PlayerJump");
        
            action = "char";

            scale = new Vector2(0.08333f,0.06667f);

            loadAnimationSet();
            //
            ///////////////
        }

        public void Update(KeyboardState keyboardState, GameTime gameTime)
        {

            playerForm prevForm = playerForm;

            if (keyboardState.IsKeyDown(Keys.Z) && playerForm != playerForm.NORMAL)
            {
                playerForm = playerForm.NORMAL;
                playerTexture = normalTexture;
                morphCount++;
            }
            else if (keyboardState.IsKeyDown(Keys.X) && playerForm != playerForm.SQUARE)
            {
                playerForm = playerForm.SQUARE;
                playerTexture = squareTexture;
                morphCount++;
            }
            else if (keyboardState.IsKeyDown(Keys.C) && playerForm != playerForm.BALL)
            {
                playerForm = playerForm.BALL;
                playerTexture = ballTexture;
                morphCount++;
            }
            else if (playerForm == playerForm.NORMAL && keyboardState.IsKeyDown(Keys.Right))
            {
                movement = 1.0f;
                direction = 1;
            }
            else if (playerForm == playerForm.NORMAL && keyboardState.IsKeyDown(Keys.Left))
            {
                movement = -1.0f;
                direction = -1;
            }
            
            isJumping = (!wasJumping &&  (keyboardState.IsKeyDown(Keys.Up) || keyboardState.IsKeyDown(Keys.Space)));

            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 prevPos = position;

            if (playerForm == playerForm.NORMAL)
            {
                velocity = DoWalk(velocity,gameTime);

                velocity.Y = DoJump(velocity.Y, gameTime);

            } else if (playerForm == playerForm.BALL)
            {

                velocity = DoRoll(velocity, gameTime, prevForm);
            }
            else if (playerForm == playerForm.SQUARE)
            {
                velocity.Y = MathHelper.Clamp(velocity.Y + gravityAcceleration * elapsed, -maxFallSpeed, maxFallSpeed);
            }

            if (IsOnGround)
                velocity.X *= groundDragFactor;
            else
                velocity.X *= airDragFactor;

            velocity.X = MathHelper.Clamp(velocity.X, -maxMoveSpeed, maxMoveSpeed);

            
            position += velocity * elapsed;
            position = new Vector2((float)Math.Round(position.X), (float)Math.Round(position.Y));

            HandleCollisions();

            if (position.X == prevPos.X)
                velocity.X = 0;

            if (position.Y == prevPos.Y)
            {
                wasJumping = false;
                velocity.Y = 0;
            }

            if (animationSet[action].Active == false)
            {
                if (action == "c2b")
                {
                    action = "ball";
                }
                if (action == "b2c")
                {
                    action = "char";
                }

                if (action == "c2s")
                {
                    action = "square";
                }
                if (action == "s2c")
                {
                    action = "char";
                }

                if (action == "b2s")
                {
                    action = "square";
                }
                if (action == "s2b")
                {
                    action = "ball";
                }
            }

            if (playerForm == playerForm.NORMAL)
            {
                if (action == "ball")
                {
                    action = "b2c";
                    activate();
                } else if (action == "square")
                {
                    action = "s2c";
                    activate();
                } else if (action == "b2c")
                {
                    action = "b2c";
                } else if (action == "s2c")
                {
                    action = "s2c";
                } else if (movement > 0.5f || movement < -0.5f)
                {
                    action = "walk";
                } else if (isJumping)
                {
                    action = "jump";
                } else
                {
                    action = "char";
                }
            }

            if (playerForm == playerForm.BALL)
            {
                if (action == "char")
                {
                    action = "c2b";
                    activate();
                }
                if (action == "square")
                {
                    action = "s2b";
                    activate();
                }
            }

            if (playerForm == playerForm.SQUARE)
            {
                if (action == "char")
                {
                    action = "c2s";
                    activate();
                }
                if (action == "ball")
                {
                    action = "b2s";
                    activate();
                }
            }

            flipped = direction == 1 ? false : true;

            animationSet[action].Position = position;

            animationSet[action].Update(gameTime);

            movement = 0.0f;
            isJumping = false;
        }

        public void activate()
        {
            animationSet[action].Active = true;
        }

        public float DoJump (float velocityY, GameTime gameTime)
        {

            
            if (isJumping && isOnGround)
            {
                SoundEffectInstance jump = jumpSound.CreateInstance();

                jump.Volume = 1;
                jump.Play();

                

                velocityY = jumpLaunchVelocity * (1.0f - (float)Math.Pow(jumpTime / maxJumpTime, jumpControlPower));

                wasJumping = true;
            }
            else if (wasJumping)
            {
                jumpTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (0.0f < jumpTime && jumpTime <= maxJumpTime)
                    velocityY = jumpLaunchVelocity * (1.0f - (float)Math.Pow(jumpTime / maxJumpTime, jumpControlPower));
            }
            else
            {
                jumpTime = 0.0f;
                wasJumping = false;
            }
            
                
            return velocityY;
        }

        public Vector2 DoWalk (Vector2 velocity, GameTime gameTime)
        {

            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            velocity.X += movement * moveAcceleration * elapsed;
            velocity.Y = MathHelper.Clamp(velocity.Y + gravityAcceleration * elapsed, -maxFallSpeed, maxFallSpeed);

            if (isOnIncline != 0)
            {
                if (isOnIncline == direction)
                    velocity.X *= inclineCalcFactorDown;
                else
                    velocity.X *= inclineCalcFactorUp;
            }

            return velocity;
        }

        public Vector2 DoRoll(Vector2 velocity, GameTime gameTime, playerForm prevForm)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (prevForm != playerForm)
                rollingTime = 0.0f;

            if (isOnIncline != 0 && velocity.X == 0)
            {
                velocity.X += isOnIncline * ballMoveAcceleration * elapsed;
                direction = isOnIncline;
            }
            else if (isOnIncline != 0 && velocity.X != 0)
            {
                if (prevForm != playerForm)
                {
                    velocity.X += isOnIncline * ballMoveAcceleration * elapsed;
                    direction = isOnIncline;
                }
                else
                {
                    if (rollingTime == 0.0)
                        groundVelocity = velocity.X * 2.0f;

                    rollingTime += elapsed;

                    if (0.0f < rollingTime && rollingTime <= maxGroundRollingTime)
                        velocity.X = groundVelocity * (1.0f - (rollingTime / maxGroundRollingTime));

                    
                }
            }
            else if (isOnGround && velocity.X != 0)
            {
                //if (prevForm == playerForm) 
                //    velocity.X += direction * ballMoveAcceleration * elapsed;

                if (rollingTime == 0.0)
                    groundVelocity = velocity.X * 1.5f;

                rollingTime += elapsed;

                if (0.0f < rollingTime && rollingTime <= maxGroundRollingTime)
                    velocity.X = groundVelocity * (1.0f - (rollingTime / maxGroundRollingTime));
            }


            if (isOnIncline != 0)
            {
                if (isOnIncline == direction)
                    velocity.X *= inclineCalcFactorDown;
                else
                    velocity.X *= inclineCalcFactorUp;
            }

            velocity.Y = MathHelper.Clamp(velocity.Y + gravityAcceleration * elapsed, -maxFallSpeed, maxFallSpeed);

            return velocity;
        }

        public Rectangle BoundingRectangle
        {
            get
            {
                Rectangle boundingRect;

                boundingRect.X = (int)position.X + 12;
                boundingRect.Y = (int)position.Y ;
                boundingRect.Width = 16;
                boundingRect.Height = 40;

                return boundingRect;
            }
        }

        public void HandleCollisions ()
        {
            Rectangle boundingRect = BoundingRectangle;

            

            int leftTile = (int)Math.Floor((float)boundingRect.Left/40);
            int rightTile = (int)Math.Ceiling((float)boundingRect.Right/40) - 1;
            int topTile = (int)Math.Floor((float)boundingRect.Top/40);
            int bottomTile = (int)Math.Ceiling((float)boundingRect.Bottom/40) - 1;

            wasOnGround = isOnGround;
            isOnGround = false;

            wasOnMovingPlatform = isOnMovingPlatform;
            isOnMovingPlatform = false;

            isOnIncline = 0;

            for (int y = topTile; y<=bottomTile; ++y)
            {
                for (int x = leftTile; x<=rightTile; ++x)
                {
                    TileType collision = level.GetCollision(x, y);

                    if (collision != TileType.PASSABLE) {

                        Rectangle tileBounds = new Rectangle(x*40,y*40,40,40);
                        Vector2 depth = GetIntersectionDepth(boundingRect,tileBounds);

                        if (depth != Vector2.Zero)
                        {
                            float absDepthX = Math.Abs(depth.X);
                            float absDepthY = Math.Abs(depth.Y);

                            if (collision == TileType.PLATFORM_FLAT || collision == TileType.SOLID)
                            {
                                if (prevBottom <= tileBounds.Top)
                                {
                                    isOnGround = true;

                                    position = new Vector2(position.X, position.Y + depth.Y);
                                } else if (prevRight <= tileBounds.Left)
                                {
                                    position = new Vector2(position.X + depth.X, position.Y);
                                } else if (prevTop >= tileBounds.Bottom)
                                {
                                    velocity.Y = 0;

                                    position = new Vector2(position.X, position.Y + depth.Y);
                                } else if (prevLeft >= tileBounds.Right)
                                {
                                    position = new Vector2(position.X + depth.X, position.Y);
                                }

                                boundingRect = BoundingRectangle;

                            }
                            else if (collision == TileType.PLATFORM_INCLINE_RIGHT_TOP)
                            {
                                if (prevBottom <= tileBounds.Top + (boundingRect.Left - tileBounds.Left))
                                {
                                    if (absDepthY >= (boundingRect.Left - tileBounds.Left))
                                    {
                                        isOnGround = true;
                                        isOnIncline = 1;

                                        position = new Vector2(position.X, tileBounds.Top + (boundingRect.Left - tileBounds.Left) - 40);
                                    }
                                }
                                else if (prevRight <= tileBounds.Left)
                                {
                                    position = new Vector2(position.X + depth.X, position.Y);
                                }
                                else if (prevTop >= tileBounds.Bottom)
                                {
                                    velocity.Y = 0;
                                    position = new Vector2(position.X, position.Y + depth.Y);
                                }
                                else if (prevLeft >= tileBounds.Right)
                                {
                                    position = new Vector2(position.X, tileBounds.Y + (boundingRect.Left - tileBounds.Left) - 40);

                                    isOnGround = true;
                                    isOnIncline = 1;
                                }
                                else if (wasOnGround)
                                {
                                    position = new Vector2(position.X, tileBounds.Y + (boundingRect.Left - tileBounds.Left) - 40);

                                    isOnGround = true;
                                    isOnIncline = 1;
                                }

                                boundingRect = BoundingRectangle;
                            }
                            else if (collision == TileType.PLATFORM_INCLINE_LEFT_TOP)
                            {
                                if (prevBottom <= tileBounds.Top + (tileBounds.Right - boundingRect.Right))
                                {
                                    if (absDepthY >= (tileBounds.Right - boundingRect.Right))
                                    {
                                        isOnGround = true;
                                        isOnIncline = -1;

                                        position = new Vector2(position.X, tileBounds.Top + (tileBounds.Right - boundingRect.Right) - 40);
                                    }
                                }
                                else if (prevRight <= tileBounds.Left)
                                {
                                    position = new Vector2(position.X, tileBounds.Top + (tileBounds.Right - boundingRect.Right) - 40);

                                    isOnGround = true;
                                    isOnIncline = -1;
                                }
                                else if (prevTop >= tileBounds.Bottom)
                                {
                                    velocity.Y = 0;
                                    position = new Vector2(position.X, position.Y + depth.Y);
                                }
                                else if (prevLeft >= tileBounds.Right)
                                {
                                    position = new Vector2(position.X + depth.X, position.Y);

                                    
                                }
                                else if (wasOnGround)
                                {
                                    position = new Vector2(position.X, tileBounds.Y + (tileBounds.Right - boundingRect.Right) - 40);

                                    isOnGround = true;
                                    isOnIncline = -1;
                                }

                                

                                boundingRect = BoundingRectangle;
                            } else if (collision == TileType.VERTICAL_MOVING_PLATFORM)
                            {
                                MovingPlatform platform = level.GetMovingPlatform(x, y);

                                Vector2 platformPos = platform.position;
                                Vector2 platformPrevPos = platform.prevPosition;

                                if (prevBottom <= platformPrevPos.Y && (position.Y + 40) >= platformPos.Y )
                                {
                                    isOnGround = true;
                                    position.Y = platformPos.Y - 40;
                                    wasJumping = false;
                                } else if (wasOnMovingPlatform)
                                {
                                    position.Y = platformPos.Y - 40;
                                    isOnGround = true;
                                }

                                
                                


                                boundingRect = BoundingRectangle;
                            } else if (collision >= TileType.OBSTACLE_RIGHT && collision <= TileType.OBSTACLE_DOWN)
                            {
                                bool collided = true; 

                                if (collision == TileType.OBSTACLE_RIGHT)
                                {

                                }
                                else if (collision == TileType.OBSTACLE_LEFT)
                                {

                                }
                                else if (collision == TileType.OBSTACLE_UP)
                                {

                                }
                                else if (collision == TileType.OBSTACLE_DOWN)
                                {

                                }

                                if (collided)
                                    level.Reset();

                            } else if (collision == TileType.LEVEL_END)
                            {
                                reachedExit = true;
                            }
                        }
                    }


                }
            }

            prevBottom = boundingRect.Bottom;
            prevLeft = boundingRect.Left;
            prevRight = boundingRect.Right;
            prevTop = boundingRect.Top;

        }

        public void Reset (Vector2 position)
        {
            this.position = position;

            isOnGround = false;
            wasJumping = false;
            isJumping = false;
            reachedExit = false;
            isOnMovingPlatform = false;
            wasOnMovingPlatform = false;

            playerForm = playerForm.NORMAL;
            playerTexture = normalTexture;

            jumpTime = 0.0f;
            rollingTime = 0.0f;

            prevBottom = 999 ;
            prevTop = -1;
            prevLeft = -1;
            prevRight = 999;

            morphCount = 0;
    }

        public void Draw(SpriteBatch spriteBatch)
        {

            Rectangle playerRect = new Rectangle((int)position.X,(int)position.Y, 40,40);
            
            if (direction == -1)
                flip = SpriteEffects.FlipHorizontally;
            else
                flip = SpriteEffects.None;
            
         
            //spriteBatch.Draw(playerTexture, playerRect, null, Color.White, 0.0f, Vector2.Zero, flip, 0f);


            /*spriteBatch.Draw(   PlayerTexture,                      // PLayer Texture
                                Position,                           // Position of the texture
                                null,                               // Source Rectangle ??
                                Color.White,                    // Color applied onto the image
                                0f,                                 // Rotation
                                Vector2.One,                        // Origin -- origin of the texture about which operations such as rotation are carried out.
                                0f,                                 // scale
                                SpriteEffects.None,                 // flipping the image vertically/horizontally
                                0f);                                // Depth to determine which comes on top

            */

            /////////////////////
            //

            //Rectangle playerRect = new Rectangle((int)position.X,(int)position.Y, 40,40);

            ////Vector2 Origin = new Vector2(20, 40);

            //if (direction == -1)
            //    flip = SpriteEffects.FlipHorizontally;
            //else
            //    flip = SpriteEffects.None;


            //spriteBatch.Draw(playerTexture, playerRect, null, Color.White, 0.0f, Vector2.Zero, flip, 0f);
            //
            /////////////////////

            ///////////////////
            //
            animationSet[action].flipped = flipped;
            animationSet[action].Draw(spriteBatch);
            animationSet[action].flipped = false;
            //Console.WriteLine("action: "+action);
            //
            ////////////////////
         
        }

        public static Vector2 GetIntersectionDepth(Rectangle rectA, Rectangle rectB)
        {
            float halfWidthA = rectA.Width / 2.0f;
            float halfHeightA = rectA.Height / 2.0f;
            float halfWidthB = rectB.Width / 2.0f;
            float halfHeightB = rectB.Height / 2.0f;

            Vector2 centerA = new Vector2(rectA.Left + halfWidthA, rectA.Top + halfHeightA);
            Vector2 centerB = new Vector2(rectB.Left + halfWidthB, rectB.Top + halfHeightB);

            float distanceX = centerA.X - centerB.X;
            float distanceY = centerA.Y - centerB.Y;
            float minDistanceX = halfWidthA + halfWidthB;
            float minDistanceY = halfHeightA + halfHeightB;

            if (Math.Abs(distanceX) >= minDistanceX || Math.Abs(distanceY) >= minDistanceY)
                return Vector2.Zero;

            float depthX = distanceX > 0 ? minDistanceX - distanceX : -minDistanceX - distanceX;
            float depthY = distanceY > 0 ? minDistanceY - distanceY : -minDistanceY - distanceY;
            return new Vector2(depthX, depthY);
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////
        //
        void loadAnimationSet()
        {

            animationSet = new Dictionary<string, Animation>();

            Animation animation = new Animation();
            Texture2D playerTexture = level.content.Load<Texture2D>("Graphics\\c");
            animation.Initialize(playerTexture, Vector2.Zero, frameWidth, frameHeight, 4, FRAMETIME, Color.White, scale, true, -1, false);
            animationSet.Add("walk", animation);

            animation = new Animation();
            animation.Initialize(playerTexture, Vector2.Zero, frameWidth, frameHeight, 1, FRAMETIME, Color.White, scale, true, 0, false);
            animationSet.Add("char", animation);

            animation = new Animation();
            animation.Initialize(playerTexture, Vector2.Zero, frameWidth, frameHeight, 1, FRAMETIME, Color.White, scale, true, 2, false);
            animationSet.Add("jump", animation);

            animation = new Animation();
            playerTexture = level.content.Load<Texture2D>("Graphics\\c-ball");
            animation.Initialize(playerTexture, Vector2.Zero, frameWidth, frameHeight, 5, FRAMETIME, Color.White, scale, false, -1, false);
            animationSet.Add("c2b", animation);

            animation = new Animation();
            animation.Initialize(playerTexture, Vector2.Zero, frameWidth, frameHeight, 5, FRAMETIME, Color.White, scale, false, -1, true);
            animationSet.Add("b2c", animation);

            animation = new Animation();
            animation.Initialize(playerTexture, Vector2.Zero, frameWidth, frameHeight, 1, FRAMETIME, Color.White, scale, true, 4, false);
            animationSet.Add("ball", animation);

            animation = new Animation();
            playerTexture = level.content.Load<Texture2D>("Graphics\\c-s");
            animation.Initialize(playerTexture, Vector2.Zero, frameWidth, frameHeight, 5, FRAMETIME, Color.White, scale, false, -1, false);
            animationSet.Add("c2s", animation);

            animation = new Animation();
            animation.Initialize(playerTexture, Vector2.Zero, frameWidth, frameHeight, 5, FRAMETIME, Color.White, scale, false, -1, true);
            animationSet.Add("s2c", animation);

            animation = new Animation();
            animation.Initialize(playerTexture, Vector2.Zero, frameWidth, frameHeight, 1, FRAMETIME, Color.White, scale, true, 4, false);
            animationSet.Add("square", animation);

            animation = new Animation();
            playerTexture = level.content.Load<Texture2D>("Graphics\\b-s");
            animation.Initialize(playerTexture, Vector2.Zero, frameWidth, frameHeight, 1, FRAMETIME, Color.White, scale, false, -1, false);
            animationSet.Add("b2s", animation);

            animation = new Animation();
            animation.Initialize(playerTexture, Vector2.Zero, frameWidth, frameHeight, 1, FRAMETIME, Color.White, scale, false, -1, true);
            animationSet.Add("s2b", animation);
        }
        //
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}
