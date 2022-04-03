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
        private readonly AThreatField threatfield;
        private readonly AInput input;
        private readonly UView3D view3D;
        private readonly UStatistics statistics;

        private int activeDefence = 0;
        private List<ADefence> defences         = new List<ADefence>();
        private List<int> stockpiles            = new List<int>();
        private List<int> orders                = new List<int>();
        private List<double> orderTimeStamps    = new List<double>();

        public ADefenceController(ContentManager content, AThreatField threatfield, AInput input, UView3D view3D, UStatistics statistics)
        {
            this.threatfield = threatfield;
            this.input = input;
            this.view3D = view3D;
            this.statistics = statistics;

            AMines  mines       = new AMines(content, threatfield);
            AMortar mortars     = new AMortar(content, threatfield, 3, 3);
            AMortar nukes       = new AMortar(content, threatfield, 10, 10);
            AFactory factories  = new AFactory(content, threatfield, 1);

            AddDefenceType(factories, 1);
            AddDefenceType(mines, 10);
            AddDefenceType(mortars, 5);
            AddDefenceType(nukes, 0);

            input.BindAction("primary.OnPressed", OnPressed);
            input.BindAction("1.OnPressed", (gt) => OnChangeType(0, gt));
            input.BindAction("2.OnPressed", (gt) => OnChangeType(1, gt));
            input.BindAction("3.OnPressed", (gt) => OnChangeType(2, gt));
            input.BindAction("4.OnPressed", (gt) => OnChangeType(3, gt));
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
            if (activeDefence == index)
            {
                if (AFactory.Resource > defences[activeDefence].Cost)
                {
                    AFactory.Resource -= defences[activeDefence].Cost;
                    orders[activeDefence]++;
                    orderTimeStamps[activeDefence] = gameTime.TotalGameTime.TotalSeconds;
                }
            }
            activeDefence = index;
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
            return 1;
        }

        public int Update(GameTime gameTime)
        {
            foreach (var defence in defences)
            {
                defence.Update(gameTime);
            }

            for (int i = 0; i < orders.Count; i++)
            {
                if (orders[i] > 0)
                {
                    if (gameTime.TotalGameTime.TotalSeconds - orderTimeStamps[i] > defences[i].OrderTime)
                    {
                        orders[i]--;
                        stockpiles[i]++;
                        orderTimeStamps[i] = gameTime.TotalGameTime.TotalSeconds;
                    }
                }
            }

            statistics.Set("Resource", ((int)AFactory.Resource).ToString());
            return 1;
        }
    }
}
