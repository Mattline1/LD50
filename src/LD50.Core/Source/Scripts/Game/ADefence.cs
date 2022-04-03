using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace LD50.Core
{
    public class ADefence : IScript
    {
        protected readonly AThreatField threatfield;

        protected ATransform2D    transforms;
        protected ASprites        sprites;
        protected Texture2D       textureAtlas;

        protected List<Vector3> destinations  = new List<Vector3>();
        protected List<double>   timestamps   = new List<double>();

        protected double delayTime = 0.0;
        protected string defaultAnimation = "Mine";

        public ADefence(ContentManager content, AThreatField threatfield, double delayTime)
        {
            this.delayTime = delayTime;
            this.threatfield = threatfield;
            transforms  = new ATransform2D();
            sprites     = new ASprites(content, transforms);

            textureAtlas = content.Load<Texture2D>("DefencesAtlas");
            sprites.AddSpriteAnimation("Mine", 16, 16, textureAtlas, new Rectangle(0, 0, 32, 32), true);
        }

        public void AddDefence(Vector3 destination, GameTime gameTime)
        {
            if (!destinations.Contains(destination))
            {
                destinations.Add(destination);
                timestamps.Add(gameTime.TotalGameTime.TotalSeconds);
                
                transforms.Add(0, GetGridCoords(destination).AsVector3());
                sprites.AddSprite(defaultAnimation, 1.0f);
            }
        }

        public virtual bool TriggerDefence(int i, int radius)
        {
            FIntVector2 gridcoords = GetGridCoords(destinations[i]);
            RemoveDefence(i);
            threatfield.SetMagnitudeInRadius(gridcoords.x, gridcoords.y, radius, 0);
            return true;
        }

        public FIntVector2 GetGridCoords(Vector3 destination)
        {
            return new FIntVector2(
                (int)MathF.Round(destination.X),
                (int)MathF.Round(destination.Z)
                );
        }

        public void RemoveDefence(int i)
        {
            destinations.RemoveAt(i);
            timestamps.RemoveAt(i);
            transforms.Remove(i);
            sprites.RemoveSprite(i);
        }

        public void SetGlobalScale(float globalScale)
        {
            sprites.globalScale = globalScale;
        }

        // interface
        public virtual int Draw(UView3D view3D, GameTime gameTime)
        {
            return 1;
        }

        public virtual int Draw2D(UView3D view3D, SpriteBatch spriteBatch, GameTime gameTime)
        {
            return sprites.Draw2D(view3D, spriteBatch, gameTime);
        }

        public virtual int Update(GameTime gameTime)
        {
            int i = 0;

            while (i < destinations.Count)
            {
                double diff = gameTime.TotalGameTime.TotalSeconds - timestamps[i];

                if (diff >= delayTime && TriggerDefence(i, 2))
                {
                    continue;
                }
                else
                {
                    i++;
                }
            }

            return 1;
        }
    }
}
