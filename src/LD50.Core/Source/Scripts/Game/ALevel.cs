using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace LD50.Core
{
    public class ALevel : IScript
    {
        private readonly Game1 game;
        private List<EventHandler<EventArgs>> eventHandlers = new List<EventHandler<EventArgs>>();

        protected AFX fx                        = null;
        protected AThreatField threatField      = null;
        protected ADefenceController defences   = null;

        public EventHandler<EventArgs> OnFailState { get; internal set; }
        private bool bHasFailed = false;

        public ALevel(Game1 game, UAudio audio, UInput input, UView3D view3D, UStatistics statistics, int gridSize = 60)
        {
            //scripts
            fx          = new AFX(game.Content, audio);
            threatField = new AThreatField(gridSize, gridSize, game.Content, fx, audio, view3D);
            defences    = new ADefenceController(game.Content, game.GraphicsDevice, fx, threatField, input, audio, view3D, statistics);

            // ensure correct sprite scaling
            threatField.SetGlobalScale(0.5f);
            defences.SetGlobalScale(0.5f);

            EventHandler<EventArgs> newEventhandler = new EventHandler<EventArgs>((Object sender, EventArgs e) =>
            {
                float scaleFactor = (float)game.Window.ClientBounds.Height / 900.0f;
                threatField.SetGlobalScale(scaleFactor * 0.5f);
                defences.SetGlobalScale(scaleFactor * 0.5f);
            }
            );

            game.Window.ClientSizeChanged += newEventhandler;
            eventHandlers.Add(newEventhandler);
            this.game = game;
        }

        public void ForceFailState(GameTime gameTime)
        {
            defences.ForceFailState(gameTime);
        }

        public int Draw2D(UView3D view3D, SpriteBatch spriteBatch, GameTime gameTime)
        {
            threatField.Draw2D(view3D, spriteBatch, gameTime);
            defences.Draw2D(view3D, spriteBatch, gameTime);
            fx.Draw2D(view3D, spriteBatch, gameTime);
            return 1;
        }

        public int Update(GameTime gameTime)
        {
            if (defences.PollFailState() && !bHasFailed)
            {
                AFactory.Resource = 0;
                threatField.DoEndGame();
                OnFailState.Invoke(this, new EventArgs());
                bHasFailed = true;
            }

            threatField.Update(gameTime);
            defences.Update(gameTime);
            fx.Update(gameTime);

            return 1;
        }

        internal void Stop()
        {
            foreach (var eventhandler in eventHandlers)
            {
                game.Window.ClientSizeChanged -= eventhandler;
            }
            threatField.Stop();

            AMissiles.internalStaticMissileList.Clear();
        }
    }
}
