using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LD50.Core
{
    interface IScript
    {
        public int Update(GameTime gameTime);
        public int Draw2D(UView3D view3D, SpriteBatch spriteBatch, GameTime gameTime);
    }
}
