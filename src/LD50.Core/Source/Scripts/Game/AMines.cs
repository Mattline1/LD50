using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System.Linq;

namespace LD50.Core
{
    public class AMines : ADefence
    {
        public AMines(ContentManager content, AThreatField threatfield) : base(content, threatfield, 2) {}

        public override bool TriggerDefence(int i, int radius)
        {
            FIntVector2 gridcoords = GetGridCoords(destinations[i]);

            if (threatfield.GetMagnitude(gridcoords) > 150)
            {
                return base.TriggerDefence(i, 2);
            }
            return false;
        }
    }
}
