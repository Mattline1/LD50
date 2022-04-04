using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace LD50.Core
{
    public class AFX : IScript
    {
        private readonly UAudio audio;
        private ATransform2D transforms;
        private ASprites sprites;
        private Texture2D textureAtlas;

        public AFX(ContentManager content, UAudio audio)
        {
            transforms = new ATransform2D();
            sprites    = new ASprites(content, transforms);

            textureAtlas = content.Load<Texture2D>("DefencesAtlas");
            sprites.AddSpriteAnimation("explosion", 16, 16, textureAtlas, new Rectangle(0, 352, 32, 32), false, 16);
            this.audio = audio;
        }

        public void AddFX(string FX, Vector3 destination, float scale = 1.0f)
        {
            transforms.Add(0, destination);
            sprites.AddSprite(FX, scale);
            audio.PlaySingle(FX);
        }

        public int Draw2D(UView3D view3D, SpriteBatch spriteBatch, GameTime gameTime)
        {
            return sprites.Draw2D(view3D, spriteBatch, gameTime);
        }

        public int Update(GameTime gameTime)
        {
            sprites.ClearFinished((i) => { transforms.Remove(i); return 1;});
            return 1;
        }
    }
}
