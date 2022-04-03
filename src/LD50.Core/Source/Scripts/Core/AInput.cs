using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD50.Core
{
    public enum EFocus
    {
        Console = 0,
        UI = 1,
        GameUI = 2,
        Game = 4
    }

    class AInput : IScript
    {
        private UInput handler = null;

        public Vector2 ControlPosition => handler.ControlPosition;

        private bool Active { get; set; }

        private EFocus Focus { get; set; } //maybe remove and do focus on a per callback basis

        public AInput(UInput handler, EFocus focus = EFocus.Game)
        {
            this.handler = handler;
            Focus = focus;
            Active = true;

            if (focus == EFocus.Game)
            {
                PushFocus(focus);
            }
        }

        private bool CheckValidToTrigger(EFocus focus)
        {
            return Active && focusStack.Count > 0 && focus == focusStack.Peek();
        }

        public void BindAction(string action, ButtonDelegate callback)
        {
            handler.BindAction(action, (GameTime gt) =>
            {
                if (CheckValidToTrigger(Focus)) { callback.Invoke(gt); }
            });
        }

        public void BindAction(string action, EFocus focus, ButtonDelegate callback)
        {
            handler.BindAction(action, (GameTime gt) =>
            {
                if (CheckValidToTrigger(focus)) { callback.Invoke(gt); }
            });
        }

        public void BindAxis(string action, AxisDelegate callback)
        {
            handler.BindAxis(action, (float x, GameTime gt) =>
            {
                if (CheckValidToTrigger(Focus)) { callback.Invoke(x, gt); }
            });
        }

        public void BindAxis2D(string action, Axis2DDelegate callback)
        {
            handler.BindAxis2D(action, (float x, float y, GameTime gt) =>
            {
                if (CheckValidToTrigger(Focus)) { callback.Invoke(x, y, gt); }
            });
        }

        // static interface
        private static Stack<EFocus> focusStack = new Stack<EFocus>();

        public static void PushFocus(EFocus focus)
        {
            if (focusStack.Count == 0 || focusStack.Peek() >= focus) // >= because more than one script may request focus
            {
                focusStack.Push(focus);
            }
        }
        public static void PopFocus(EFocus focus)
        {
            if (focusStack.Count != 0 && focusStack.Peek() == focus)
            {
                focusStack.Pop();
            }
        }

        public int Update(GameTime gameTime)
        {
            return handler.Update(gameTime);
        }

        public int Draw(UView3D view3D, GameTime gameTime)
        {
            return 1;
        }

        public int Draw2D(UView3D view3D, SpriteBatch spriteBatch, GameTime gameTime)
        {
            return 1;
        }
    }
}
