using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace LD50.Core
{
    public class AFactory : ADefence
    {
        public static double Resource = 0;

        public AFactory(ContentManager content, AThreatField threatfield, UAudio audio, int delay) : base(content, threatfield, audio, delay)
        {
            defaultAnimation = "Factory";
            defaultSpriteSize = 1.0f;
        }

        public override bool TriggerDefence(int i, int radius, GameTime gameTime)
        {
            FIntVector2 gridcoords = GetGridCoords(destinations[i]);

            if (!threatfield.GetIsResource(gridcoords) || threatfield.GetMagnitude(gridcoords) > 100)
            {
                return base.TriggerDefence(i, 2, gameTime);
            }

            Resource += gameTime.ElapsedGameTime.TotalSeconds;
            return false;
        }

    }
}
