using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace LD50.Core
{
    public class ATransform2D
    {
        public readonly List<float> angles      = new List<float>(); //in radians
        public readonly List<float> scales      = new List<float>(); //in radians
        public readonly List<Vector3> positions = new List<Vector3>();
        public int Count => positions.Count;

        public Matrix this[int i]
        {
            get { return Matrix.CreateScale(scales[i]) * Matrix.CreateRotationZ(angles[i]) * Matrix.CreateTranslation(positions[i]); }
        }

        public void Add(float angle, Vector3 position, float scale = 1.0f)
        {
            angles.Add(angle);
            positions.Add(position);
            scales.Add(scale);
        }

        public void Remove(int i)
        {
            angles.RemoveAt(i);
            positions.RemoveAt(i);
            scales.RemoveAt(i);
        }
    }
}