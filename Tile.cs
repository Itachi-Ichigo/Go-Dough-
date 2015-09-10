using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Game1.Game
{
    enum TileType
    {
        PASSABLE =0,
        PLATFORM_FLAT = 1,
        SOLID = 2,
        PLATFORM_INCLINE_RIGHT_TOP = 3,
        PLATFORM_INCLINE_RIGHT_BOTTOM = 4,
        PLATFORM_INCLINE_LEFT_TOP = 5,
        PLATFORM_INCLINE_LEFT_BOTTOM = 6,
        OBSTACLE_RIGHT = 7,
        OBSTACLE_LEFT = 8,
        OBSTACLE_UP = 9,
        OBSTACLE_DOWN = 10,
        VERTICAL_MOVING_PLATFORM = 118,
        LEVEL_END = 122
        
    }

    class Tile
    {

        public TileType tileType;
        public Texture2D tileTexture;
        public MovingPlatform movingPlatform;

        public const int width = 40;
        public const int height = 40;

        public Tile (Texture2D pTexture, TileType pTileType, MovingPlatform movingPlatform = null)
        {
            tileType = pTileType;

            tileTexture = pTexture;

            this.movingPlatform = movingPlatform;
        }


    }
}
