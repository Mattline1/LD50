using LD50.XMLSchema;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace LD50.Core
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        // Scripts
        private List<IScript> scripts = new List<IScript>();

        // Services
        private UAudio audio = null;
        private UInput input = null;
        private USettings settings = null;
        private UStatistics statistics = null;
        private UView3D view3D = null;

        // State
        private RasterizerState rasterizerState;
        private RasterizerState rasterizerUIState;

        // Profiling
        private Stopwatch updateStopwatch = null;
        private Stopwatch drawStopwatch = null;
        private SpriteFont font;
        private bool drawStats = true;

        private int grid = 50;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += new EventHandler<EventArgs>(OnResize);
        }
        public void OnResize(Object sender, EventArgs e)
        {
            RecalculateViewMatrix(
                Window.ClientBounds.Width,
                Window.ClientBounds.Height);
        }

        protected void RecalculateViewMatrix(int preferredWidth, int preferredHeight)
        {
            graphics.PreferredBackBufferWidth  = preferredWidth;
            graphics.PreferredBackBufferHeight = preferredHeight;
            graphics.ApplyChanges();

            view3D.viewport.Width  = preferredWidth;
            view3D.viewport.Height = preferredHeight;

            view3D.position = new Vector3(grid / 2 - 0.5f, grid * 1.4f , grid / 2 - 0.5f);
            view3D.rotation = new Vector3(0.0f, MathHelper.ToRadians(-90), 0.0f);
            view3D.window   = new Vector2(4.0f, 2.4f);
            view3D.BuildMatrices();
        }

        protected override void Initialize()
        {
            base.Initialize();

            updateStopwatch     = new Stopwatch();
            drawStopwatch       = new Stopwatch();
            spriteBatch         = new SpriteBatch(GraphicsDevice);
            rasterizerState     = new RasterizerState() { ScissorTestEnable = false };
            rasterizerUIState   = new RasterizerState() { ScissorTestEnable = true };

            // services
            audio               = new UAudio(Content);
            statistics          = new UStatistics();
            input               = new UInput(statistics);
            settings            = new USettings();
            view3D              = new UView3D(GraphicsDevice.Viewport);

            //music 
            audio.Play("music").IsLooped = true;

            //scripts
            AInput inputScript = new AInput(input);
            AMenus menus = new AMenus(Content, GraphicsDevice, inputScript, audio);

            scripts.Add(inputScript);

            InitializeLevel(inputScript);

            scripts.Add(menus);

            RecalculateViewMatrix(900, 900);
        }

        protected void InitializeLevel(AInput inputScript)
        {
            //scripts
            AThreatField threatField = new AThreatField(grid, grid, Content, audio);
            ADefenceController defences = new ADefenceController(Content, GraphicsDevice, threatField, inputScript, audio, view3D, statistics);

            scripts.Add(threatField);
            scripts.Add(defences);

            // ensure correct sprite scaling
            threatField.SetGlobalScale(0.5f);
            defences.SetGlobalScale(0.5f);

            Window.ClientSizeChanged += new EventHandler<EventArgs>((Object sender, EventArgs e) =>
            {
                float scaleFactor = (float)Window.ClientBounds.Height / 900.0f;
                threatField.SetGlobalScale(scaleFactor * 0.5f);
                defences.SetGlobalScale(scaleFactor * 0.5f);
            }
            );
        }


        protected override void LoadContent()
        {
            font = Content.Load<SpriteFont>("RoentgenNbp");
        }

        protected override void Update(GameTime gameTime)
        {
            updateStopwatch.Restart();

            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            foreach (IScript script in scripts)
            {
                script.Update(gameTime);
            }

            updateStopwatch.Stop();
        }

        protected override void Draw(GameTime gameTime)
        {
            drawStopwatch.Restart();
            GraphicsDevice.Clear(new Color(45, 45, 45, 1));

            foreach (IScript script in scripts)
            {
                script.Draw(view3D, gameTime);
            }

            spriteBatch.Begin();
            foreach (IScript script in scripts)
            {
                script.Draw2D(view3D, spriteBatch, gameTime);
            }

            //HACK present important info
            //spriteBatch.DrawString(font, "Resources: " + (1.0 / gameTime.ElapsedGameTime.TotalSeconds).ToString("0"), new Vector2(10, 10), Color.BlanchedAlmond);

            spriteBatch.End();
            drawStopwatch.Stop();

            // stats
            DrawStats(gameTime);

            base.Draw(gameTime);
        }

        private void DrawStats(GameTime gameTime)
        {
            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, rasterizerUIState);

            if (drawStats)
            {
                spriteBatch.DrawString(font, "FPS:   " + (1.0 / gameTime.ElapsedGameTime.TotalSeconds).ToString("0"), new Vector2(10, 10), Color.Black);
                spriteBatch.DrawString(font, "update:" + updateStopwatch.Elapsed.TotalMilliseconds.ToString(), new Vector2(10, 25), Color.Black);
                spriteBatch.DrawString(font, "draw:  " + drawStopwatch.Elapsed.TotalMilliseconds.ToString(), new Vector2(10, 40), Color.Black);

                int y = 55;
                foreach (var stat in statistics.stats)
                {
                    spriteBatch.DrawString(font, stat.Key + ":" + stat.Value, new Vector2(10, y), Color.Black);
                    y += 15;
                }
            }
            spriteBatch.End();
        }
    }
}
