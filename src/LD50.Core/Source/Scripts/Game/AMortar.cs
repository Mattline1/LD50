using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace LD50.Core
{
    public class AMortar : ADefence
    {
        private readonly int radius;

        public AMortar(ContentManager content, AFX fx, AThreatField threatfield, UAudio audio, double order, double cost, double delay, int radius)
            : base(content, fx, threatfield, audio, order, cost, delay)
        {
            this.radius = radius;
            defaultAnimation = "Mortar";
            defaultSpriteSize = 2.0f;
        }

        public override bool TriggerDefence(int i, int radius, GameTime gameTime)
        {
            return base.TriggerDefence(i, this.radius, gameTime);
        }
    }
}
