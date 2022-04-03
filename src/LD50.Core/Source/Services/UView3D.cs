using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD50.Core
{
    public class UView3D
    {
        public Matrix ViewMatrix;
        public Matrix Projection;
        public Matrix ProjectionInverse;
        public BoundingFrustum Frustum;

        public Vector3 position;
        public Vector3 rotation;
        public Vector2 window;
        public Viewport viewport;

        protected bool _isOrtho;

        public UView3D(Viewport viewport, bool isOrtho = false)
        {
            position = Vector3.Up;
            rotation = new Vector3(0.0f, MathHelper.ToRadians(-45), 0.0f);
            window   = new Vector2(4.0f, 2.4f);

            this.viewport = viewport;
            _isOrtho = isOrtho;

            BuildMatrices();
        }

        public UView3D(Viewport viewport, Vector3 position, Vector3 rotation, Vector2 window, bool isOrtho = false)
        {
            this.position = position;
            this.rotation = rotation;
            this.window   = window;
            this.viewport = viewport;
            _isOrtho = isOrtho;

            BuildMatrices();
        }

        public void Translate(Vector3 translation)
        {
            position += translation;
            BuildViewMatrix();
        }

        public void Rotate(Vector3 translation)
        {
            rotation += translation;
            BuildViewMatrix();
        }

        public void BuildFrustum()
        {
            Frustum = new BoundingFrustum(ViewMatrix * Projection);
        }

        public void BuildProjectionMatrix()
        {
            if (_isOrtho)
            {
                Projection = Matrix.CreateOrthographic(window.X, window.Y, 0.5f, 1.5f);
            }
            else
            {
                Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), viewport.AspectRatio, 0.1f, 100f);
            }
            ProjectionInverse = Matrix.Invert(Projection);
        }

        public void BuildViewMatrix()
        {
            Quaternion quat = Quaternion.CreateFromYawPitchRoll(rotation.X, rotation.Y, rotation.Z);
            Vector3 forward = Vector3.Transform(Vector3.Forward, quat);
            Vector3 up      = Vector3.Transform(Vector3.Up, quat);
            ViewMatrix      = Matrix.CreateWorld(position, forward, up);
            ViewMatrix      = Matrix.Invert(ViewMatrix);
        }

        public void BuildMatrices()
        {
            BuildProjectionMatrix();
            BuildViewMatrix();
            BuildFrustum();
        }

        public Vector3 ProjectScreenToGroundPlane(Vector2 screen)
        {
            Vector3 rayEnd = ScreenToWorld(screen);
            Vector3 rayDir = rayEnd - position;
            rayDir.Normalize();

            Ray ray = new Ray(position, rayDir);
            float? distance = ray.Intersects(new Plane(Vector3.Zero, Vector3.Up));

            if (distance.HasValue)
            {
                return position + (rayDir * distance.Value);
            }
            else
            {
                return Vector3.Zero;
            }
        }

        public Vector3 ScreenToWorld(Vector2 screen)
        {
            Vector3 comp = new Vector3(screen.X, screen.Y, 0.0f);
            return viewport.Unproject(comp, Projection, ViewMatrix, Matrix.Identity);
        }

        public Vector2 WorldToScreen(Vector3 world, out bool InFrustum)
        {
            Vector3 comp = viewport.Project(world, Projection, ViewMatrix, Matrix.Identity);
            InFrustum = comp.Z >= 0 && comp.Z <= 1.0f; // in frustum?
            return new Vector2(comp.X, comp.Y);
        }
    }
}
