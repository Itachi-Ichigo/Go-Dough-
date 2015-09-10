using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Game1
{
    class Animation
    {
        // The image representing the collection of images used for animation

        Texture2D spriteStrip;

        // The scale used to display the sprite strip

        Vector2 scale;

        // The time since we last updated the frame

        int elapsedTime;

        // The time we display a frame until the next one

        int frameTime;

        // The number of frames that the animation contains

        int frameCount;

        // The index of the current frame we are displaying

        int currentFrame;

        // The color of the frame we will be displaying

        Color color;

        // The area of the image strip we want to display

        Rectangle sourceRect = new Rectangle();

        // The area where we want to display the image strip in the game

        Rectangle destinationRect = new Rectangle();

        // Width of a given frame

        public int FrameWidth;

        // Height of a given frame

        public int FrameHeight;

        // The state of the Animation

        public bool Active;

        // Determines if the animation will keep playing or deactivate after one run

        public bool Looping;

        public int fixedFrame;

        // Width of a given frame

        public Vector2 Position;

        // Reverse
        public bool reversed;

        public bool flipped;

        public void Initialize(Texture2D texture, Vector2 position, int frameWidth, int frameHeight, int frameCount, int frametime, Color color, Vector2 scale, bool looping, int _fixedFrame, bool rev)

        {

            // Keep a local copy of the values passed in

            this.color = color;

            this.FrameWidth = frameWidth;

            this.FrameHeight = frameHeight;

            this.frameCount = frameCount;

            this.frameTime = frametime;

            this.scale = scale;



            Looping = looping;

            Position = position;

            spriteStrip = texture;

            fixedFrame = _fixedFrame;

            // Set the time to zero

            elapsedTime = 0;

            reversed = rev;

            if (reversed == false)
            {
                currentFrame = 0;
            } else
            {
                currentFrame = frameCount - 1;
            }


            // Set the Animation to active by default

            Active = true;
            
        }

        public void Update(GameTime gameTime)
        {
            
            elapsedTime += (int)gameTime.ElapsedGameTime.TotalMilliseconds;

            // If the elapsed time is larger than the frame time

            // we need to switch frames

            if (elapsedTime > frameTime)
            {

                // Move to the next frame

                if (reversed == false)
                {
                    currentFrame++;
                }
                else {
                    currentFrame--;
                }




                // If the currentFrame is equal to frameCount reset currentFrame to zero

                if (currentFrame >= frameCount || currentFrame <= -1)
                {
                    if (reversed == false)
                    {
                        currentFrame = 0;
                    }
                    else
                    {
                        currentFrame = frameCount - 1;
                    }

                    // If we are not looping deactivate the animation

                    if (Looping == false)
                        Active = false;

                }

                // Reset the elapsed time to zero

                elapsedTime -= frameTime;

            }

            // Grab the correct frame in the image strip by multiplying the currentFrame index by the Frame width
            
            if (fixedFrame >= 0)
            {
                sourceRect = new Rectangle(fixedFrame * FrameWidth, 0, FrameWidth, FrameHeight);
            }
            else
            {
                sourceRect = new Rectangle(currentFrame * FrameWidth, 0, FrameWidth, FrameHeight);
            }

            // Grab the correct frame in the image strip by multiplying the currentFrame index by the frame width

            destinationRect = new Rectangle((int)Position.X/* - (int)(FrameWidth * scale) / 2*/,
            (int)Position.Y ,//- (int)(FrameHeight * scale) ,
            (int)(FrameWidth * scale.X),
            (int)(FrameHeight * scale.Y));


            if (Active == false)
            {
                if (reversed)
                    sourceRect = new Rectangle(0, 0, FrameWidth, FrameHeight);
                else
                    sourceRect = new Rectangle((frameCount - 1) * FrameWidth, 0, FrameWidth, FrameHeight);
            }

            Console.WriteLine(currentFrame);
        }

        // Draw the Animation Strip

        public void Draw(SpriteBatch spriteBatch)
        {
            SpriteEffects flip;
            if (flipped)
            {
                flip = SpriteEffects.FlipHorizontally;
            }
            else
            {
                flip = SpriteEffects.None;
            }

            //Console.WriteLine("Dest X: " + destinationRect.X);

            spriteBatch.Draw(spriteStrip, destinationRect, sourceRect, color, 0f, Vector2.Zero, flip, 0f);
        }
    }
}
