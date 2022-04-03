using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace LD50.Core
{
    public class UStatistics : IService
    {
        public Dictionary<string, string> stats = new Dictionary<string, string>();

        public string this[string stat]
        {
            get { return stats[stat]; }
        }

        public void Set(string stat, string value)
        {
            stats[stat] = value;
        }

        public int Update(GameTime gameTime)
        {
            return 1;
        }
    }
}
