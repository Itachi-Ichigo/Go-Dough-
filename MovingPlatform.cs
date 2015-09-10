using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Game1
{
    class MovingPlatform
    {

        Rectangle boundingRectangle;
        int size;

        Vector2 velocity = new Vector2(0,200.0f);
        public Vector2 position;
        public Vector2 prevPosition;

        int direction;
        Texture2D texture;

        
        public MovingPlatform (Texture2D texture, int x, int y)
        {
            boundingRectangle = new Rectangle(x*40, y*40, 40, (18- y)*40);

            position = new Vector2(boundingRectangle.X, boundingRectangle.Y);

            size = 1;
            direction = 1;

            this.texture = texture;
        }

        public void Update(GameTime gameTime)
        {
            prevPosition = position;

            position.X += (float)(direction * velocity.X * gameTime.ElapsedGameTime.TotalSeconds);
            position.Y += (float)(direction * velocity.Y * gameTime.ElapsedGameTime.TotalSeconds);

            if (position.Y > boundingRectangle.Y + (size * 40))
                direction *= -1;
            else if (position.Y <= boundingRectangle.Y)
                direction *= -1;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture,position,Color.White);
        }

        public bool Check(int x, int y)
        {
            if (boundingRectangle.Intersects(new Rectangle(x * 40, y * 40, 40, 40)))
            {
                ++size;

                return true;
            }

            return false;
        }

        public void Reset()
        {
            position = new Vector2(boundingRectangle.X, boundingRectangle.Y);
            prevPosition = new Vector2(boundingRectangle.X, boundingRectangle.Y);
            direction = 1;
        }
    }
}
