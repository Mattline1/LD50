using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace LD50.Core
{    
    public struct AnimationData
    {
        public int texID;
        public int frames;
        public int width;
        public bool loops;
        public Rectangle source;
        public float frameRate;

        public AnimationData(int texID, int frames, float frameRate, int width, bool loops, Rectangle source)
        {
            this.texID = texID;
            this.frames = frames;
            this.width = width;
            this.loops = loops;
            this.source = source;
            this.frameRate = frameRate;
        }
    }

    public class ASprites : IScript
    {
        private ATransform2D transforms;

        // internal data
        private List<Texture2D> textures = new List<Texture2D>();
        private List<bool> bShouldDraw = new List<bool>();
        private Dictionary<string, AnimationData> animations = new Dictionary<string, AnimationData>();



        // info on current per-sprite animation
        private List<string> name       = new List<string>();
        private List<int>  texID        = new List<int>();
        private List<int>  startFrame   = new List<int>();
        private List<int>  endFrame     = new List<int>();
        private List<int>  width        = new List<int>();
        private List<bool> loop         = new List<bool>();
        private List<float> elapsedTime = new List<float>();
        private List<float> frameRate   = new List<float>();
        private List<Rectangle> sources = new List<Rectangle>();
        private List<Color> colors      = new List<Color>();
        private List<float> scales      = new List<float>();

        public float FrameRate => 16.0f;

        // flags
        public bool bViewAlignedSprites = false;

        //is hack
        public float globalScale = 1.0f;

        public ASprites(ContentManager content, ATransform2D transforms)
        {
            this.transforms = transforms;
        }

        public int AddSprite(string defaultAnimation = "", float scale = 1.0f)
        {
            // get index
            int index = texID.Count;

            // add empty entity data to lists
            name.Add("");
            texID.Add(-1);
            startFrame.Add(0);
            endFrame.Add(0);
            width.Add(0);
            loop.Add(false);
            elapsedTime.Add(0);
            frameRate.Add(FrameRate);
            sources.Add(new Rectangle());
            colors.Add(Color.White);
            scales.Add(scale);
            bShouldDraw.Add(true);

            // play animation on entity if animation is provided
            if (defaultAnimation != "")
            {
                Play(index, defaultAnimation, 0 , true);
            }

            return index;
        }

        public void AddSpriteAnimation(string name, int frames, int width, Texture2D texture, Rectangle source, bool shouldLoop = false, float frameRate = -1.0f)
        {
            int tex = textures.FindIndex(obj => obj == texture);
            if (tex == -1)
            {
                tex = textures.Count;
                textures.Add(texture);
            }

            animations[name] = new AnimationData(tex, frames, frameRate == -1 ? FrameRate : frameRate, width, shouldLoop, source);
        }
        
        public AnimationData GetAnimationData(int i)
        {
            return animations[name[i]];
        }

        public float GetElapsedTime(int i)
        {
            return elapsedTime[i];
        }

        public int GetEndFrame(int i)
        {
            return endFrame[i];
        }

        public bool GetLooping(int i)
        {
            return loop[i];
        }

        public string GetPlaying(int i)
        {
            return name[i];
        }

        public bool GetIsPlaying(int i)
        {
            int frame = startFrame[i] + (int)(elapsedTime[i] * FrameRate);
            return frame <= endFrame[i];
        }

        public int GetStartFrame(int i)
        {
            return startFrame[i];
        }

        public void Play(int i, string name, int startFrame = 0, bool forceLoop = false, float exactStart = 0.0f, bool clampToFrameCount = true)
        {
            AnimationData animdata = animations[name];

            this.name[i]        = name;
            texID[i]            = animdata.texID;
            this.startFrame[i]  = startFrame;
            endFrame[i]         = clampToFrameCount ? (animdata.frames - 1) : startFrame + (animdata.frames - 1);// 0 == first
            width[i]            = animdata.width;
            loop[i]             = forceLoop || animdata.loops;
            elapsedTime[i]      = exactStart;
            frameRate[i]        = animdata.frameRate;
            sources[i]          = animdata.source;
        }

        public void SetColor(int i, Color color)
        {
            colors[i] = color;
        }

        public void SetShouldDraw(int i, bool val)
        {
            bShouldDraw[i] = val;
        }

        public void RemoveSprite(int i)
        {
            name.RemoveAt(i);
            texID.RemoveAt(i);
            startFrame.RemoveAt(i);
            endFrame.RemoveAt(i);
            width.RemoveAt(i);
            loop.RemoveAt(i);
            elapsedTime.RemoveAt(i);
            frameRate.RemoveAt(i);
            sources.RemoveAt(i);
            colors.RemoveAt(i);
            scales.RemoveAt(i);
            bShouldDraw.RemoveAt(i);
        }

        public int Draw2D(UView3D view, SpriteBatch spriteBatch, GameTime gameTime)
        {
            for (int i = 0; i < transforms.Count; i++)
            {
                if (texID[i] == -1 || !bShouldDraw[i]) { continue; }

                Texture2D tex = textures[texID[i]];
                int frame = startFrame[i] + (int)(elapsedTime[i] * frameRate[i]);

                // step frame?
                if (frame > endFrame[i])
                {
                    if (loop[i])
                    {
                        frame = startFrame[i];
                        elapsedTime[i] = (float)gameTime.ElapsedGameTime.TotalSeconds;
                    }
                    else
                    {
                        frame = endFrame[i]; // rest on last frame 
                        // also trigger callback - maybe not here, but flag it needs to happen here
                    }
                }
                
                elapsedTime[i] = elapsedTime[i] + (float)gameTime.ElapsedGameTime.TotalSeconds;

                int x = frame % width[i];
                int y = frame / width[i];
                Rectangle source = sources[i];
                Rectangle centre = new Rectangle(0, 0, source.Width, source.Height);
                Rectangle offset = source;
                offset.Offset(source.Width * x, source.Height * y);

                bool inFrustum;
                Vector2 pos = view.WorldToScreen(transforms.positions[i], out inFrustum);
                if (!inFrustum) { continue; }

                spriteBatch.Draw(
                            tex,
                            pos,
                            offset, //spriteTexture.Bounds, 
                            colors[i],
                            bViewAlignedSprites ? 0 : transforms.angles[i],
                            centre.Center.ToVector2(),
                            globalScale * scales[i],
                            SpriteEffects.None,
                            0);
            }

            return 1;
        }

        public int Update(GameTime gameTime)
        {
            return 1;
        }

        public int Draw(UView3D view3D, GameTime gameTime)
        {
            return 1;
        }
    }
}
