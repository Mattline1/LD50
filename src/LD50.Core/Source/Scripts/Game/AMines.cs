using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace LD50.Core
{
    public class AMines : ADefence
    {
        public AMines(ContentManager content, AFX fx, AThreatField threatfield, UAudio audio, double order, double cost, double delay)
            : base(content, fx, threatfield, audio, order, cost, delay) {}

        public override bool TriggerDefence(int i, int radius, GameTime gameTime)
        {
            FIntVector2 gridcoords = GetGridCoords(destinations[i]);

            if (threatfield.GetMagnitude(gridcoords) > 150)
            {
                return base.TriggerDefence(i, 2, gameTime);
            }
            return false;
        }
    }
}
