using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace LD50.Core
{
    public class AStaticSprites : IScript
    {
        //private SpriteBatch spriteBatch = null;
        private ATransform2D    transforms;
        private List<Texture2D> sprites = new List<Texture2D>();
        private List<bool>      bShouldDraw = new List<bool>();

        ContentManager content; // todo remove

        public AStaticSprites(ContentManager content, ATransform2D transforms)
        {
            this.transforms  = transforms;
            this.content     = content;
        }

        public void Add(string texture)
        {
            sprites.Add(content.Load<Texture2D>(texture));
            bShouldDraw.Add(true);
        }

        public void Add(Texture2D texture)
        {
            sprites.Add(texture);
            bShouldDraw.Add(true);
        }

        public int Draw(UView3D view3D, GameTime gameTime)
        {
            return 1;
        }

        public int Draw2D(UView3D view3D, SpriteBatch spriteBatch, GameTime gameTime)
        {
            for (int i = 0; i < sprites.Count; i++)
            {
                bool inFrustum;
                Vector2 screenpos = view3D.WorldToScreen(transforms.positions[i], out inFrustum);

                if (!inFrustum || !bShouldDraw[i]) { continue; }

                spriteBatch.Draw(
                    sprites[i],
                    screenpos,
                    null, //spriteTexture.Bounds, 
                    Color.White,
                    transforms.angles[i],
                    sprites[i].Bounds.Center.ToVector2(),
                    0.3f,
                    SpriteEffects.None,
                    0
                );
            }

            return 1;
        }

        public void SetShouldDraw(int i, bool val)
        {
            bShouldDraw[i] = val;
        }

        public int Update(GameTime gameTime)
        {
            return 1;
        }
    }
}
