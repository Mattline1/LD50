using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD50.Core
{

    public struct FIntVector2
    {
        public int x;
        public int y;

        public FIntVector2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public bool IsZero() { return x == 0 && y == 0; }

        public Vector3 AsVector3() { return new Vector3(x, 0, y); }
    }

    public class AFieldValues
    {
        public List<int> x              = new List<int>();
        public List<int> y              = new List<int>();
        public List<float> magnitude    = new List<float>();
        public List<bool> bIsSource     = new List<bool>();
        public List<bool> bIsResource   = new List<bool>();

        public void Add(Random rnd, bool _bIsSource = false, bool _bIsResource = false)
        {
            x.Add(rnd.Next(-1, 2));
            y.Add(rnd.Next(-1, 2));
            magnitude.Add(_bIsSource ? 255 : 0);
            bIsSource.Add(_bIsSource);
            bIsResource.Add(_bIsResource);
        }
    }

    public class AThreatField : IScript
    {
        private AFieldValues fieldA = new AFieldValues();
        private AFieldValues fieldB = new AFieldValues();
        private bool         bUsingFieldA = true;

        private ATransform2D    transforms;
        private ASprites        sprites;
        private Texture2D       textureAtlas;

        public int width;
        public int height;
        private readonly AFX fx;
        private readonly UAudio audio;
        private SoundEffectInstance creepnoise;

        private Random rnd = new Random();

        private float creepVirality  = 100;
        private float creepDiffusion = 200;

        protected float missileTempo = 20.0f;
        protected double lastMissileTimeStamp = -20.0f;
        protected AMissiles missiles;


        public AThreatField(int width, int height, ContentManager content, AFX fx, UAudio audio, UView3D view3D)
        {
            this.width  = width;
            this.height = height;
            this.fx = fx;
            this.audio  = audio;
            transforms  = new ATransform2D();
            sprites     = new ASprites(content, transforms);

            textureAtlas = content.Load<Texture2D>("GreyGrid");
            sprites.AddSpriteAnimation("Square", 16, 16, textureAtlas, new Rectangle(0, 0, 32, 32), true);

            creepnoise = audio.Play("creep");
            creepnoise.IsLooped = true;
            creepnoise.Volume = 0.0f;

            missiles = new AMissiles(content, fx, this, audio, view3D, 0.0, 0.0, 0.0, true);
            missiles.defaultColor = Color.Red;

            for (int h = 0; h < height; h++)
            {
                for (int w = 0; w < width; w++)
                {
                    transforms.Add(0, new Vector3(w, 0.0f, h));
                    int i = sprites.AddSprite();
                    sprites.Play(i, "Square", rnd.Next(0, 16), true);

                    bool source = (MathF.Abs(width / 2 - w) > 20 || MathF.Abs(height / 2 - h) > 20) && rnd.Next(0, 100) == 5;
                    bool resource = rnd.Next(0, 100) == 5;
                    fieldA.Add(rnd, source, resource);
                    fieldB.Add(rnd, source, resource);
                }
            }
        }

        internal void Stop()
        {
            creepnoise.Stop();
        }

        internal void DoEndGame()
        {
            creepVirality *= 10;
        }

        public int Draw(UView3D view3D, GameTime gameTime)
        {
            return 1;
        }

        public int Draw2D(UView3D view3D, SpriteBatch spriteBatch, GameTime gameTime)
        {
            sprites.Draw2D(view3D, spriteBatch, gameTime);
            missiles.Draw2D(view3D, spriteBatch, gameTime);
            return 1;
        }

        public int Get1DIndex(int x, int y)
        {
            return (y * width) + x;
        }

        public int Get1DIndexOffset(int i, int x, int y, int range)
        {
            int trueY = Math.Clamp((i / width) + y, 0, height - 1);
            int trueX = (i % width) + x;
            int trueI = Get1DIndex(trueX, trueY);
            return Math.Clamp(trueI, 0, range - 1);
        }

        public FIntVector2 Get2DIndex(int i)
        {
            int y = Math.Clamp(i / width, 0, height - 1);
            int x = i % width;
            return new FIntVector2(x, y);
        }

        public bool GetIsResource(FIntVector2 coords)
        {
            return GetIsResource(coords.x, coords.y);
        }

        public bool GetIsResource(int x, int y)
        {
            int i = Get1DIndex(x, y);
            if (IsValidIndex(i))
            {
                if (bUsingFieldA)
                {
                    return fieldA.bIsResource[i];
                }
                else
                {
                    return fieldB.bIsResource[i];
                }
            }
            return false;
        }

        public float GetMagnitude(FIntVector2 coords)
        {
            return GetMagnitude(coords.x, coords.y);
        }

        public float GetMagnitude(int x, int y)
        {
            int i = Get1DIndex(x, y);
            if (IsValidIndex(i))
            {
                if (bUsingFieldA)
                {
                    return fieldA.magnitude[i];
                }
                else
                {
                    return fieldB.magnitude[i];
                }
            }
            return 0;
        }

        public FIntVector2 GetRandomDirection()
        {
            int r = rnd.Next(0, 8);
            int rx = (r % 3) - 1;
            int ry = (r / 3) - 1;
            return new FIntVector2(rx, ry);
        }

        public FIntVector2 GetRandomDirection(int xRange, int yRange)
        {
            int rx = rnd.Next(Math.Min(xRange, -1), Math.Max(xRange + 1, 1));
            int ry = rnd.Next(Math.Min(yRange, -1), Math.Max(yRange + 1, 1));
            rx = Math.Clamp(rx, -1, 1);
            ry = Math.Clamp(ry, -1, 1);
            return new FIntVector2(rx, ry);
        }

        public bool IsValidIndex(int i)
        {
            return (i >= 0 && i < fieldA.x.Count);
        }

        public void SetGlobalScale(float globalScale)
        {
            sprites.globalScale = globalScale;
        }

        public void SetMagnitude(int x, int y, float magnitude)
        {
            int i = Get1DIndex(x, y);
            if (IsValidIndex(i) && !fieldA.bIsSource[i])
            {
                fieldA.magnitude[i] = magnitude;
                fieldB.magnitude[i] = magnitude;
            }
        }

        public void SetMagnitudeInRadius(int x, int y, int radius, float magnitude)
        {
            for (int xi = -radius; xi <= radius; xi++)
            {
                int tx = x + xi;
                if (tx < 0 || tx >= width) { continue; }

                for (int yi = -radius; yi <= radius; yi++)
                {
                    int ty = y + yi;
                    if (ty < 0 || ty >= height) { continue; }

                    SetMagnitude(tx, y + yi, magnitude);
                }
            }
        }

        public void SetResource(int x, int y, bool isResource)
        {
            int i = Get1DIndex(x, y);
            if (IsValidIndex(i))
            {
                fieldA.bIsResource[i] = isResource;
                fieldB.bIsResource[i] = isResource;
            }
        }
        public void SetSource(int x, int y, bool isSource)
        {
            int i = Get1DIndex(x, y);
            if (IsValidIndex(i))
            {
                SetMagnitude(x, y, isSource ? 255.0f : 0.0f);
                fieldA.bIsSource[i] = isSource;
                fieldB.bIsSource[i] = isSource;
            }
        }

        public void Step(GameTime gametime)
        {
            AFieldValues readField = bUsingFieldA ? fieldA : fieldB;
            AFieldValues writeField = bUsingFieldA ? fieldB : fieldA;

            int infested = 0;

            for (int i = 0; i < fieldA.x.Count; i++)
            {
                float m = readField.magnitude[i];

                if (m > creepDiffusion)
                {
                    int x = readField.x[i];
                    int y = readField.y[i];

                    FIntVector2 randomDirection = GetRandomDirection(x, y);
                    if (randomDirection.IsZero())
                    {
                        randomDirection = GetRandomDirection();
                    }

                    int cti = Get1DIndexOffset(i, randomDirection.x, randomDirection.y, readField.x.Count);

                    writeField.x[cti]           = Math.Clamp(readField.x[cti] + x, -1, 1);
                    writeField.y[cti]           = Math.Clamp(readField.y[cti] + y, -1, 1);
                    writeField.magnitude[cti]   = Math.Min(
                        readField.magnitude[cti] + (creepVirality * (float)gametime.ElapsedGameTime.TotalSeconds),
                        255.0f
                        );
                    writeField.bIsResource[i]   = false;

                    infested++;
                 }

                sprites.SetColor(i, Color.Lerp(Color.White, Color.Red, m / 255.0f));

                if (readField.bIsSource[i] 
                    && gametime.TotalGameTime.TotalSeconds - lastMissileTimeStamp > missileTempo &&
                    rnd.Next(0, 100) == 50)
                {
                    int target  = -1;
                    int iter    = 0;
                    while (!IsValidIndex(target) && iter < 100)
                    {
                        target = rnd.Next(0, width * height);
                        if (readField.magnitude[target] > 50)
                        {
                            target = -1;
                        }
                        iter++;
                    }

                    if (IsValidIndex(target))
                    {
                        missiles.AddDefence(Get2DIndex(target).AsVector3(), gametime, Get2DIndex(i).AsVector3());
                        lastMissileTimeStamp = gametime.TotalGameTime.TotalSeconds;
                    }
                }

                if (readField.bIsResource[i])
                {
                    sprites.SetColor(i, Color.MediumPurple);
                }
            }

            float factor = (float)infested / (float)(width * height);
            creepnoise.Volume = factor * factor * factor;
            bUsingFieldA = !bUsingFieldA;
        }

        public int Update(GameTime gameTime)
        {
            Step(gameTime);
            missiles.Update(gameTime);
            return 1;
        }
    }
}
