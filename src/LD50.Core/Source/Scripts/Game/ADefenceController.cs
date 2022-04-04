using LD50.XMLSchema;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD50.Core
{
    class ADefenceController : IScript
    {
        private readonly ACanvas canvas;
        private readonly AThreatField threatfield;
        private readonly AInput input;
        private readonly UAudio audio;
        private readonly UView3D view3D;
        private readonly UStatistics statistics;

        private int activeDefence = 0;

        private List<ADefence> defences         = new List<ADefence>();
        private List<int> stockpiles            = new List<int>();
        private List<int> orders                = new List<int>();
        private List<double> orderTimeStamps    = new List<double>();

        public ADefenceController(ContentManager content, GraphicsDevice graphics, AThreatField threatfield, UInput input, UAudio audio, UView3D view3D, UStatistics statistics)
        {
            this.threatfield = threatfield;
            this.input = new AInput(input);
            this.audio = audio;
            this.view3D = view3D;
            this.statistics = statistics;

            canvas = new ACanvas(content, graphics, this.input, content.Load<FCanvas>("GUI"));

            AMines  mines       = new AMines(content, threatfield, audio);
            AMortar mortars     = new AMortar(content, threatfield, audio, 2, 3);
            AMortar nukes       = new AMortar(content, threatfield, audio, 6, 20);
            AFactory factories  = new AFactory(content, threatfield, audio, 1);

            AddDefenceType(factories, 1);
            AddDefenceType(mines, 10);
            AddDefenceType(mortars, 5);
            AddDefenceType(nukes, 0);

            this.input.BindAction("primary.OnPressed", OnPressed);
            this.input.BindAction("1.OnPressed", (gt) => OnChangeAndOrderType(0, gt));
            this.input.BindAction("2.OnPressed", (gt) => OnChangeAndOrderType(1, gt));
            this.input.BindAction("3.OnPressed", (gt) => OnChangeAndOrderType(2, gt));
            this.input.BindAction("4.OnPressed", (gt) => OnChangeAndOrderType(3, gt));

            canvas.BindAction("button1.OnClick", (gt) => OnChangeType(0, gt));
            canvas.BindAction("button2.OnClick", (gt) => OnOrderType(0, gt));

            canvas.BindAction("button3.OnClick", (gt) => OnChangeType(1, gt));
            canvas.BindAction("button4.OnClick", (gt) => OnOrderType(1, gt));

            canvas.BindAction("button5.OnClick", (gt) => OnChangeType(2, gt));
            canvas.BindAction("button6.OnClick", (gt) => OnOrderType(2, gt));

            canvas.BindAction("button7.OnClick", (gt) => OnChangeType(3, gt));
            canvas.BindAction("button8.OnClick", (gt) => OnOrderType(3, gt));


            int width = threatfield.width;
            int height = threatfield.height;
            threatfield.SetResource(width / 2, height / 2, true);
            defences[0].AddDefence(new Vector3(width / 2, 0.0f, height / 2), new GameTime());
        }

        private void AddDefence(Vector3 destination, GameTime gameTime)
        {
            if (stockpiles[activeDefence] > 0)
            {
                defences[activeDefence].AddDefence(destination, gameTime);
                stockpiles[activeDefence]--;
            }
        }
        private void AddDefenceType(ADefence defence, int stockpiled)
        {
            defences.Add(defence);
            stockpiles.Add(stockpiled);
            orders.Add(0);
            orderTimeStamps.Add(0);
        }

        private FIntVector2 GetGridCoords(Vector3 destination)
        {
            return new FIntVector2(
                (int)MathF.Round(destination.X),
                (int)MathF.Round(destination.Z)
                );
        }

        private void OnPressed(GameTime gameTime)
        {
            AddDefence(view3D.ProjectScreenToGroundPlane(input.ControlPosition), gameTime);
        }

        private void OnChangeType(int index, GameTime gameTime)
        {
            audio.PlaySingle("chirp");
            activeDefence = index;
        }

        private void OnChangeAndOrderType(int index, GameTime gameTime)
        {
            if (activeDefence == index)
            {
                OnOrderType(activeDefence, gameTime);
            }
            OnChangeType(index, gameTime);
        }

        private void OnOrderType(int index, GameTime gameTime)
        {
            if (AFactory.Resource > defences[index].Cost)
            {
                audio.PlaySingle("ordered");
                AFactory.Resource -= defences[index].Cost;

                if (orders[index] == 0)
                {
                    orderTimeStamps[index] = gameTime.TotalGameTime.TotalSeconds;
                }
                orders[index]++;
            }
            else
            {
                audio.PlaySingle("chirp");
            }
        }

        public void SetGlobalScale(float globalScale)
        {
            foreach (var defence in defences)
            {
                defence.SetGlobalScale(globalScale);
            }
        }

        // interface
        public int Draw(UView3D view3D, GameTime gameTime)
        {
            foreach (var defence in defences)
            {
                defence.Draw(view3D, gameTime);
            }
            return 1;
        }

        public int Draw2D(UView3D view3D, SpriteBatch spriteBatch, GameTime gameTime)
        {
            foreach (var defence in defences)
            {
                defence.Draw2D(view3D, spriteBatch, gameTime);
            }

            canvas.Draw2D(view3D, spriteBatch, gameTime);

            return 1;
        }

        public int Update(GameTime gameTime)
        {
            foreach (var defence in defences)
            {
                defence.Update(gameTime);
            }

            //HACK for game jam, there should be a better way to edit widgets

            for (int i = 0; i < orders.Count; i++)
            {
                if (orders[i] > 0)
                {
                    double diff = gameTime.TotalGameTime.TotalSeconds - orderTimeStamps[i];
                    double countdown = defences[i].OrderTime - diff;

                    if (countdown <= 0)
                    {
                        orders[i]--;
                        stockpiles[i]++;
                        orderTimeStamps[i] = gameTime.TotalGameTime.TotalSeconds;
                        audio.PlaySingle("gain");
                    }

                    canvas.widgets.texts[8 + i] = string.Format(">{0}\n>{1}\n{2}", stockpiles[i], orders[i], countdown.ToString("0.00"));
                }
                else
                {
                    canvas.widgets.texts[8 + i] = string.Format(">{0}\n\n>{1}", stockpiles[i], orders[i]);
                }
            }

            canvas.Update(gameTime);

            statistics.Set("Resource", ((int)AFactory.Resource).ToString());

            return 1;
        }
    }
}
