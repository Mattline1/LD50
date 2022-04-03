using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;

namespace LD50.Core
{
    public class UAudio : IService
    {
        private Dictionary<string, SoundEffect> soundTable = new Dictionary<string, SoundEffect>();
        private ContentManager _contentManager;

        public UAudio(ContentManager contentManager)
        {
            _contentManager = contentManager;
        }

        public void Cache(string sound)
        {
            SoundEffect soundEffect = _contentManager.Load<SoundEffect>(sound);
            soundTable.TryAdd(sound, soundEffect);
        }

        private SoundEffect Get(string sound)
        {
            if (soundTable.ContainsKey(sound) == false)
            {
                Cache(sound);
            }

            if (soundTable.ContainsKey(sound))
            {
                return soundTable[sound];
            }

            return null;
        }

        public void PlaySingle(string sound)
        {
            Get(sound).Play();
        }

        public SoundEffectInstance Play(string sound)
        {
            SoundEffectInstance instance = Get(sound).CreateInstance();
            instance.Play();
            return instance;
        }

        public int Update(GameTime gameTime)
        {
            return 1;
        }
    }
}
