using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD50.Core
{
    public class AMortar : ADefence
    {
        private readonly int radius;

        public AMortar(ContentManager content, AThreatField threatfield, int radius, int delay) : base(content, threatfield, delay)
        {
            this.radius = radius;
        }

        public override bool TriggerDefence(int i, int radius)
        {
            return base.TriggerDefence(i, radius);
        }
    }
}
