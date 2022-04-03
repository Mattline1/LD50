using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace LD50.Core
{
    public delegate void ButtonDelegate(GameTime gameTime);

    public delegate void AxisDelegate(float x, GameTime gameTime);

    public delegate void Axis2DDelegate(float x, float y, GameTime gameTime);

    public enum EMouseButtons
    {
        None,
        LeftButton,
        MiddleButton,
        RightButton,
        Mouse4,
        Mouse5
    }

    public struct TActionHandle
    {
        public string action;
        public int index;

        public TActionHandle(string action, int index)
        {
            this.action = action ?? throw new ArgumentNullException(nameof(action));
            this.index = index;
        }
    }

    public struct TBinding
    {
        public Keys key;
        public Buttons button;
        public EMouseButtons mouseButton;
        public string bound;

        public TBinding(Keys key, string bound)
        {
            this.key = key;
            this.bound = bound;

            button = Buttons.A;
            mouseButton = EMouseButtons.None;
        }

        public TBinding(Buttons button, string bound)
        {
            this.button = button;
            this.bound = bound;

            key = Keys.None;
            mouseButton = EMouseButtons.None;
        }

        public TBinding(EMouseButtons mouseButton, string bound)
        {
            this.mouseButton = mouseButton;
            this.bound = bound;

            key = Keys.None;
            button = Buttons.A;
        }
    }

    public class UInput : IService
    {
        private MouseState priorMouseState;
        private KeyboardState priorKeyboardState;

        public Vector2 ControlPosition { get; private set; } = Vector2.Zero;

        // callback bindings
        private Dictionary<string, List<ButtonDelegate>> keyEvents = new Dictionary<string, List<ButtonDelegate>>();
        private List<TBinding> keyBindings = new List<TBinding>();

        // stats
        private UStatistics statistics;

        public UInput(UStatistics statistics)
        {
            this.statistics = statistics;

            priorMouseState = Mouse.GetState();
            priorKeyboardState = Keyboard.GetState();

            keyBindings.Add(new TBinding(Keys.Space, "primary"));
            keyBindings.Add(new TBinding(Keys.Enter, "secondary"));

            keyBindings.Add(new TBinding(EMouseButtons.LeftButton, "primary"));
            keyBindings.Add(new TBinding(EMouseButtons.RightButton, "secondary"));
            keyBindings.Add(new TBinding(EMouseButtons.MiddleButton, "tertiary"));

            keyBindings.Add(new TBinding(Keys.W, "up"));
            keyBindings.Add(new TBinding(Keys.A, "left"));
            keyBindings.Add(new TBinding(Keys.S, "down"));
            keyBindings.Add(new TBinding(Keys.D, "right"));

            keyBindings.Add(new TBinding(Keys.Q, "Q"));
            keyBindings.Add(new TBinding(Keys.E, "E"));


            keyBindings.Add(new TBinding(Keys.D1, "1"));
            keyBindings.Add(new TBinding(Keys.D2, "2"));
            keyBindings.Add(new TBinding(Keys.D3, "3"));
            keyBindings.Add(new TBinding(Keys.D4, "4"));
            keyBindings.Add(new TBinding(Keys.D5, "5"));
            keyBindings.Add(new TBinding(Keys.D6, "6"));
            keyBindings.Add(new TBinding(Keys.D7, "7"));
        }

        public TActionHandle BindAction(string action, ButtonDelegate callback)
        {
            if (!keyEvents.ContainsKey(action))
            {
                keyEvents[action] = new List<ButtonDelegate>();
            }
            keyEvents[action].Add(callback);
            return new TActionHandle(action, keyEvents[action].Count - 1);
        }

        public void BindAxis(string action, AxisDelegate callback)
        {
            throw new NotImplementedException();
        }

        public void BindAxis2D(string action, Axis2DDelegate callback)
        {
            throw new NotImplementedException();
        }

        public void RemoveAction(string action, ButtonDelegate callback)
        {
            keyEvents[action].Remove(callback);
        }

        public void RemoveAction(TActionHandle handle)
        {
            keyEvents[handle.action].RemoveAt(handle.index);
        }

        // interface implementation

        public int Update(GameTime gameTime)
        {
            string GetInputSuffix(bool current, bool prior)
            {
                return current ? prior ? ".Pressed" : ".OnPressed" : prior ? ".OnReleased" : ".Released";
            }

            void Invoke(string action)
            {
                if (keyEvents.ContainsKey(action))
                {
                    foreach (var evt in keyEvents[action])
                    {
                        evt.Invoke(gameTime);
                    }
                }
            }

            MouseState currentMouseState = Mouse.GetState();
            KeyboardState currentKeyboardState = Keyboard.GetState();

            //todo extend to other input methods
            ControlPosition = new Vector2(currentMouseState.X, currentMouseState.Y);

            statistics.Set("ControlPosition", ControlPosition.ToString());

            // handle key events
            foreach (var binding in keyBindings)
            {
                // keyboard bindings //
                if (binding.key != Keys.None)
                {
                    Keys key = binding.key;
                    Invoke(binding.bound + GetInputSuffix(currentKeyboardState.IsKeyDown(key), priorKeyboardState.IsKeyDown(key)));
                }

                // mouse bindings //
                else if (binding.mouseButton != EMouseButtons.None)
                {
                    switch (binding.mouseButton)
                    {
                        case EMouseButtons.LeftButton:
                            Invoke(binding.bound + GetInputSuffix(currentMouseState.LeftButton == ButtonState.Pressed, priorMouseState.LeftButton == ButtonState.Pressed));
                            break;

                        case EMouseButtons.MiddleButton:
                            Invoke(binding.bound + GetInputSuffix(currentMouseState.MiddleButton == ButtonState.Pressed, priorMouseState.MiddleButton == ButtonState.Pressed));
                            break;

                        case EMouseButtons.RightButton:
                            Invoke(binding.bound + GetInputSuffix(currentMouseState.RightButton == ButtonState.Pressed, priorMouseState.RightButton == ButtonState.Pressed));
                            break;

                        case EMouseButtons.Mouse4:
                            Invoke(binding.bound + GetInputSuffix(currentMouseState.XButton1 == ButtonState.Pressed, priorMouseState.XButton1 == ButtonState.Pressed));
                            break;

                        case EMouseButtons.Mouse5:
                            Invoke(binding.bound + GetInputSuffix(currentMouseState.XButton2 == ButtonState.Pressed, priorMouseState.XButton2 == ButtonState.Pressed));
                            break;

                        default:
                            break;
                    }
                }

                //todo handle other input peripherals here 
                else
                {

                }
            }
            priorMouseState = currentMouseState;
            priorKeyboardState = currentKeyboardState;

            return 1;
        }
    }
}
