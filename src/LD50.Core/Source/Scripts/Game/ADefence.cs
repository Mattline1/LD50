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
        protected float defaultSpriteSize = 1.0f;

        public double Cost = 1.0f;
        public double OrderTime = 3.0f;

        public ADefence(ContentManager content, AThreatField threatfield, double delayTime)
        {
            this.delayTime = delayTime;
            this.threatfield = threatfield;
            transforms  = new ATransform2D();
            sprites     = new ASprites(content, transforms);

            textureAtlas = content.Load<Texture2D>("DefencesAtlas");
            sprites.AddSpriteAnimation("Mine", 16, 16, textureAtlas, new Rectangle(0, 0, 32, 32), true);
            sprites.AddSpriteAnimation("Mortar", 32, 8, textureAtlas, new Rectangle(0, 32, 64, 64), true, 10);
        }

        public void AddDefence(Vector3 destination, GameTime gameTime)
        {
            if (!destinations.Contains(destination))
            {
                destinations.Add(destination);
                timestamps.Add(gameTime.TotalGameTime.TotalSeconds);
                
                transforms.Add(0, GetGridCoords(destination).AsVector3());
                sprites.AddSprite(defaultAnimation, defaultSpriteSize);
            }
        }

        public virtual bool TriggerDefence(int i, int radius, GameTime gameTime)
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

                if (diff >= delayTime && TriggerDefence(i, 2, gameTime))
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
