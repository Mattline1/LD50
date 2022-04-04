using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace LD50.Core
{
    public class AMissiles : ADefence
    {
        private readonly Texture2D lineTexture = null;
        private readonly UView3D view3D;
        private readonly bool bAreEvil;

        public static List<AMissiles> internalStaticMissileList = new List<AMissiles>();

        public AMissiles(ContentManager content, AFX fx, AThreatField threatfield, UAudio audio, UView3D view3D, double order, double cost, double delay, bool bAreEvil)
            : base(content, fx, threatfield, audio, order, cost, delay)
        {
            defaultAnimation = "Target";
            defaultSpriteSize = 0.7f;

            lineTexture = content.Load<Texture2D>("Line");
            this.view3D = view3D;
            this.bAreEvil = bAreEvil;

            internalStaticMissileList.Add(this);
        }

        public override void AddDefence(Vector3 destination, GameTime gameTime, Vector3 origin)
        {
            base.AddDefence(destination, gameTime, origin);
        }

        public float GetCurrentDistanceToTarget(int i, GameTime gameTime)
        {
            return (GetCurrentPosition(i, gameTime) - destinations[i]).Length();
        }

        public Vector3 GetCurrentPosition(int i, GameTime gameTime)
        {
            float path = (origins[i] - destinations[i]).Length();
            float diff = (float)(gameTime.TotalGameTime.TotalSeconds - timestamps[i]);

            path    = MathF.Max(path, 0.0000001f); // remove possibility of division by 0
            float a = Math.Clamp(diff / path, 0.0f, 1.0f);
            return Vector3.Lerp(origins[i], destinations[i], a);
        }

        public void DrawLine(SpriteBatch spriteBatch, Vector3 origin, Vector3 destination, Color color)
        {
            Vector2 origin2d = view3D.WorldToScreen(origin, out _);
            Vector2 destin2d = view3D.WorldToScreen(destination, out _);

            float dist      = Vector2.Distance(origin2d, destin2d);
            float angle     = (float)Math.Atan2(destin2d.Y - origin2d.Y, destin2d.X - origin2d.X);
            Vector2 centre  = new Vector2(0f, 0.5f);
            Vector2 scale   = new Vector2(dist, 1.0f);

            spriteBatch.Draw(lineTexture, origin2d, null, color, angle, centre, scale, SpriteEffects.None, 0);
        }

        public override bool TriggerDefence(int i, int radius, GameTime gameTime)
        {
            if (bAreEvil)
            {
                FIntVector2 gridcoords = GetGridCoords(destinations[i]);
                threatfield.SetSource(gridcoords.x, gridcoords.y, true);

                RemoveDefence(i);

                //fx.AddFX("creep", transforms.positions[i]);

                return true;
            }
            else
            {
                return base.TriggerDefence(i, radius, gameTime);
            }
        }

        public override int Draw2D(UView3D view3D, SpriteBatch spriteBatch, GameTime gameTime)
        {
            for (int i = 0; i < transforms.Count; i++)
            {
                DrawLine(spriteBatch, origins[i], GetCurrentPosition(i, gameTime), bAreEvil ? Color.Red : Color.WhiteSmoke);
            }

            return sprites.Draw2D(view3D, spriteBatch, gameTime);
        }

        public static void CheckCollisions(AMissiles A, AMissiles B, float radius, AFX fx, GameTime gameTime)
        {
            radius *= radius;

            int iA = 0;
            while (iA < A.destinations.Count)
            {
                Vector3 posA = A.GetCurrentPosition(iA, gameTime);

                int iB = 0;
                while (iB < B.destinations.Count)
                {
                    Vector3 posB = B.GetCurrentPosition(iB, gameTime);

                    if (Vector3.DistanceSquared(posA, posB) < radius)
                    {
                        fx.AddFX("explosion", posA);
                        fx.AddFX("explosion", posB);

                        A.RemoveDefence(iA);
                        B.RemoveDefence(iB);
                        iA--;
                        iB--;
                    }
                    iB++;
                }
                iA++;
            }
        }

        public override int Update(GameTime gameTime)
        {
            int i = 0;

            if (internalStaticMissileList.Count > 1)
            {
                CheckCollisions(internalStaticMissileList[0], internalStaticMissileList[1], 1, fx, gameTime);
            }

            while (i < destinations.Count)
            {
                float dist = GetCurrentDistanceToTarget(i, gameTime);
                if (dist <= 0.1f && TriggerDefence(i, 0, gameTime))
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
