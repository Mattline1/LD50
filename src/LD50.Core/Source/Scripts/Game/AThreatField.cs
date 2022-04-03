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
        public List<int> magnitude      = new List<int>();
        public List<bool> bIsSource     = new List<bool>();

        public void Add(Random rnd, bool _bIsSource = false)
        {
            x.Add(rnd.Next(-1, 2));
            y.Add(rnd.Next(-1, 2));
            magnitude.Add(_bIsSource ? 255 : 0);
            bIsSource.Add(_bIsSource);
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

        private int width;
        private int height;
        private Random rnd = new Random();

        public AThreatField(int width, int height, ContentManager content)
        {
            this.width  = width;
            this.height = height;
            transforms  = new ATransform2D();
            sprites     = new ASprites(content, transforms);

            textureAtlas = content.Load<Texture2D>("GreyGrid");
            sprites.AddSpriteAnimation("Square", 16, 16, textureAtlas, new Rectangle(0, 0, 32, 32), true);

            for (int h = 0; h < height; h++)
            {
                for (int w = 0; w < width; w++)
                {
                    transforms.Add(0, new Vector3(w, 0.0f, h));
                    int i = sprites.AddSprite();
                    sprites.Play(i, "Square", rnd.Next(0, 16), true);

                    bool source = rnd.Next(0, 30) == 5;
                    fieldA.Add(rnd, source);
                    fieldB.Add(rnd, source);
                }
            }
        }

        public int Draw(UView3D view3D, GameTime gameTime)
        {
            return 1;
        }

        public int Draw2D(UView3D view3D, SpriteBatch spriteBatch, GameTime gameTime)
        {
            sprites.Draw2D(view3D, spriteBatch, gameTime);
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

        public int GetMagnitude(FIntVector2 coords)
        {
            return GetMagnitude(coords.x, coords.y);
        }

        public int GetMagnitude(int x, int y)
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

        public void SetMagnitude(int x, int y, int magnitude)
        {
            int i = Get1DIndex(x, y);
            if (IsValidIndex(i))
            {
                fieldA.magnitude[i] = magnitude;
                fieldB.magnitude[i] = magnitude;
            }
        }

        public void SetMagnitudeInRadius(int x, int y, int radius, int magnitude)
        {
            for (int xi = -radius; xi <= radius; xi++)
            {
                for (int yi = -radius; yi <= radius; yi++)
                {
                    int i = Get1DIndex(x + xi, y + yi);

                    if (IsValidIndex(i))
                    {
                        fieldA.magnitude[i] = magnitude;
                        fieldB.magnitude[i] = magnitude;
                    }
                }
            }
        }

        public void Step()
        {
            AFieldValues readField = bUsingFieldA ? fieldA : fieldB;
            AFieldValues writeField = bUsingFieldA ? fieldB : fieldA;

            for (int i = 0; i < fieldA.x.Count; i++)
            {
                int m = readField.magnitude[i];

                if (m > 200)
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
                    writeField.magnitude[cti]   = Math.Min(readField.magnitude[cti] + 5, 255);
                }

                sprites.SetColor(i, Color.Lerp(Color.White, Color.Red, m / 255.0f));
            }

            bUsingFieldA = !bUsingFieldA;
        }

        public int Update(GameTime gameTime)
        {
            Step();
            return 1;
        }
    }
}
