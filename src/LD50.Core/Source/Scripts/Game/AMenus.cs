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
        private readonly List<ACanvas> canvases = new List<ACanvas>();

        private readonly ContentManager content;
        private readonly GraphicsDevice graphics;
        private readonly AInput input;
        private readonly UAudio audio;
        private readonly Game1 game;

        protected int activecanvas = 0;
        protected int nextCanvas = 0;

        public AMenus(ContentManager content, GraphicsDevice graphics, AInput input, UAudio audio, Game1 game)
        {
            this.content = content;
            this.graphics = graphics;
            this.input = input;
            this.audio = audio;
            this.game = game;

            canvases.Add(new ACanvas(content, graphics, input, content.Load<FCanvas>("SplashUI")));
            canvases.Add(new ACanvas(content, graphics, input, content.Load<FCanvas>("MenuUI")));
            canvases.Add(new ACanvas(content, graphics, input, content.Load<FCanvas>("TopUI")));
            canvases.Add(new ACanvas(content, graphics, input, content.Load<FCanvas>("FailUI")));
            canvases.Add(new ACanvas(content, graphics, input, content.Load<FCanvas>("PauseUI")));

            // splash
            canvases[0].BindAction("button1.OnClick", (gt) => ChangeCanvas(1));

            // menu
            canvases[1].BindAction("button1.OnClick", (gt) => {
                game.RestartLevel();
                ChangeCanvas(2);
            });
            canvases[1].BindAction("button2.OnClick", (gt) => game.Exit());

            // fail state
            canvases[3].BindAction("button1.OnClick", (gt) => {
                game.RestartLevel();
                ChangeCanvas(2);
            });
            canvases[3].BindAction("button2.OnClick", (gt) => game.Exit());

            // pause
            canvases[4].BindAction("button1.OnClick", (gt) => ChangeCanvas(2));
            canvases[4].BindAction("button2.OnClick", (gt) => game.Exit());
        }

        public int Draw(UView3D view3D, GameTime gameTime)
        {
            return 1;
        }

        public int Draw2D(UView3D view3D, SpriteBatch spriteBatch, GameTime gameTime)
        {
            return canvases[activecanvas].Draw2D(view3D, spriteBatch, gameTime);
        }

        public void TryPause()
        {
            if (activecanvas == 2)
            {
                ChangeCanvas(4);
            }
        }

        public void ChangeCanvas(int activecanvas)
        {
            nextCanvas = activecanvas;
        }

        public int Update(GameTime gameTime)
        {
            if (nextCanvas != activecanvas)
            {
                canvases[this.activecanvas].Disable();
                activecanvas = nextCanvas;
                return 1;
            }
            else
            {
                if (activecanvas == 2)
                {
                    canvases[2].widgets.texts[1] = string.Format("Resources > {0}", (int)AFactory.Resource);
                }

                return canvases[activecanvas].Update(gameTime);
            }
        }
    }
}
