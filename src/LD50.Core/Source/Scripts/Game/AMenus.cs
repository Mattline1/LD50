using LD50.XMLSchema;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace LD50.Core
{
    public class AMenus : IScript
    {
        private readonly ACanvas canvas;
        private readonly ContentManager content;
        private readonly GraphicsDevice graphics;
        private readonly AInput input;
        private readonly UAudio audio;

        public AMenus(ContentManager content, GraphicsDevice graphics, AInput input, UAudio audio)
        {
            this.content = content;
            this.graphics = graphics;
            this.input = input;
            this.audio = audio;

            canvas = new ACanvas(content, graphics, input, content.Load<FCanvas>("TopUI"));
            //canvas.BindAction("button1.OnClick", (gt) => OnChangeType(0, gt));
            //canvas.BindAction("button2.OnClick", (gt) => OnOrderType(0, gt));
            //canvas.BindAction("button3.OnClick", (gt) => OnChangeType(1, gt));
        }

        public int Draw(UView3D view3D, GameTime gameTime)
        {
            return 1;
        }

        public int Draw2D(UView3D view3D, SpriteBatch spriteBatch, GameTime gameTime)
        {
            return canvas.Draw2D(view3D, spriteBatch, gameTime);
        }

        public int Update(GameTime gameTime)
        {
            canvas.widgets.texts[1] = string.Format("Resources > {0}", (int)AFactory.Resource);

            return canvas.Update(gameTime);
        }
    }
}
