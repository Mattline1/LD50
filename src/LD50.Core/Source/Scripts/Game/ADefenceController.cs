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

        private int activeDefence = 0;
        private List<ADefence> defences = new List<ADefence>();

        public ADefenceController(ContentManager content, AThreatField threatfield, AInput input, UView3D view3D)
        {
            this.threatfield = threatfield;
            this.input = input;
            this.view3D = view3D;

            AMines  mines   = new AMines(content, threatfield);
            AMortar mortars = new AMortar(content, threatfield, 3, 5);
            AMortar nukes   = new AMortar(content, threatfield, 10, 30);

            defences.Add(mines);
            defences.Add(mortars);
            defences.Add(nukes);

            input.BindAction("primary.OnPressed", OnPressed);
        }

        private void AddDefence(Vector3 destination, GameTime gameTime)
        {
            defences[activeDefence].AddDefence(destination, gameTime);
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
            return 1;
        }
    }
}
