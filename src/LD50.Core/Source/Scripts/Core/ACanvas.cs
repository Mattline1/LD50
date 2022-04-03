using LD50.XMLSchema;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace LD50.Core
{
    public delegate void WidgetDelegate(GameTime gameTime);

    public class ACanvas : IScript
    {
        private SpriteFont bodyFont;

        //HACK for game jam, there should be a better way to edit widgets than public
        public FCanvas widgets;

        private Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();

        // callback bindings
        private Dictionary<string, List<WidgetDelegate>> bindings = new Dictionary<string, List<WidgetDelegate>>();

        private string suffix   = ".Released";
        private bool bIsHovered = false;

        private GraphicsDevice graphics;
        private AInput inputScript;

        public ACanvas(ContentManager Content, GraphicsDevice graphicsDevice, AInput input, FCanvas widgetStruct)
        {
            graphics = graphicsDevice;
            bodyFont = Content.Load<SpriteFont>("RoentgenNbp");

            SwitchCanvas(Content, widgetStruct);

            if (input != null)
            {
                inputScript = input;
                input.BindAction("primary.Released", EFocus.GameUI, (GameTime gt) => { suffix = ".Released"; });
                input.BindAction("primary.Pressed", EFocus.GameUI, (GameTime gt) => { suffix = ".Pressed"; });
                input.BindAction("primary.OnPressed", EFocus.GameUI, (GameTime gt) => { suffix = ".OnClick"; });
                input.BindAction("primary.OnReleased", EFocus.GameUI, (GameTime gt) => { suffix = ".OnEndClick"; });
            }
        }

        public void SwitchCanvas(ContentManager Content, FCanvas widgetStruct)
        {
            widgets = widgetStruct;

            foreach (string texture in widgets.textures)
            {
                if (texture != "" && !textures.ContainsKey(texture))
                {
                    textures.Add(texture, Content.Load<Texture2D>(texture));
                }
            }
            //todo in a correct implementation, this should clear textures and bindings
        }

        public void BindAction(string action, WidgetDelegate callback)
        {
            if (!bindings.ContainsKey(action))
            {
                bindings[action] = new List<WidgetDelegate>();
            }

            bindings[action].Add(callback);
        }

        public int Draw(UView3D view3D, GameTime gameTime)
        {
            return 1;
        }

        public int Draw2D(UView3D view3D, SpriteBatch spriteBatch, GameTime gameTime)
        {
            int currentParent = 0;

            Rectangle view = spriteBatch.GraphicsDevice.Viewport.Bounds;
            Rectangle root = widgets.root;

            switch (widgets.anchor)
            {
                case EWidgetAnchor.N:
                    root.X += (view.Width / 2) - (root.Width / 2);
                    break;
                case EWidgetAnchor.NE:
                    root.X += view.Width - root.Width;
                    break;
                case EWidgetAnchor.E:
                    root.X += view.Width - root.Width;
                    root.Y += (view.Height / 2) - (root.Height / 2);
                    break;
                case EWidgetAnchor.SE:
                    root.X += view.Width - root.Width;
                    root.Y += view.Height - root.Height;
                    break;
                case EWidgetAnchor.S:
                    root.X += (view.Width / 2) - (root.Width / 2);
                    root.Y += view.Height - root.Height;
                    break;
                case EWidgetAnchor.SW:
                    root.Y += view.Height - root.Height;
                    break;
                case EWidgetAnchor.W:
                    root.Y += view.Height / 2;
                    break;
                case EWidgetAnchor.NW:
                    break;
                case EWidgetAnchor.C:
                    root.X += (view.Width / 2) - (root.Width / 2);
                    root.Y += (view.Height / 2) - (root.Height / 2);
                    break;
                default:
                    break;
            }

            for (int i = 0; i < widgets.parents.Length; i++)
            {
                int parentIndex = widgets.parents[i];
                Rectangle rect  = widgets.rectangles[i];
                rect.Offset(root.Location);

                // set scissor
                if (parentIndex != currentParent)
                {
                    if (parentIndex == -1)
                    {
                        spriteBatch.GraphicsDevice.ScissorRectangle = root;
                    }
                    else
                    {
                        Rectangle parentRect = widgets.rectangles[parentIndex];
                        parentRect.Offset(root.Location);
                        spriteBatch.GraphicsDevice.ScissorRectangle = parentRect;
                    }
                }

                // draw by type
                switch (widgets.widgetTypes[i])
                {
                    case EWidgetType.text:
                        spriteBatch.DrawString(
                            bodyFont,
                            widgets.texts[i],
                            rect.Location.ToVector2(),
                            widgets.colors[i]
                        );
                        break;

                    case EWidgetType.panel:
                    case EWidgetType.button:
                    case EWidgetType.label:
                        spriteBatch.Draw(
                            textures[widgets.textures[i]],
                            rect,
                            widgets.sources[i],
                            widgets.colors[i]
                        );
                        break;
                    default:
                        break;
                }
            }

            spriteBatch.GraphicsDevice.ScissorRectangle = view;

            return 1;
        }

        public int Update(GameTime gameTime)
        {
            if (inputScript == null) { return 1; }

            int currentParent = 0;
            Rectangle view = graphics.Viewport.Bounds;
            Rectangle root = widgets.root;
            Rectangle scissor = view;
            bool hovered = false;

            switch (widgets.anchor)
            {
                case EWidgetAnchor.N:
                    root.X += (view.Width / 2) - (root.Width / 2);
                    break;
                case EWidgetAnchor.NE:
                    root.X += view.Width - root.Width;
                    break;
                case EWidgetAnchor.E:
                    root.X += view.Width - root.Width;
                    root.Y += (view.Height / 2) - (root.Height / 2);
                    break;
                case EWidgetAnchor.SE:
                    root.X += view.Width - root.Width;
                    root.Y += view.Height - root.Height;
                    break;
                case EWidgetAnchor.S:
                    root.X += (view.Width / 2) - (root.Width / 2);
                    root.Y += view.Height - root.Height;
                    break;
                case EWidgetAnchor.SW:
                    root.Y += view.Height - root.Height;
                    break;
                case EWidgetAnchor.W:
                    root.Y += view.Height / 2;
                    break;
                case EWidgetAnchor.NW:
                    break;
                case EWidgetAnchor.C:
                    root.X += (view.Width / 2) - (root.Width / 2);
                    root.Y += (view.Height / 2) - (root.Height / 2);
                    break;
                default:
                    break;
            }

            for (int i = 0; i < widgets.parents.Length; i++)
            {
                // get parent index
                int parent = widgets.parents[i];

                // calculate roots
                Rectangle rect = widgets.rectangles[i];
                rect.Offset(root.Location);

                // update scissor
                if (parent != currentParent)
                {
                    if (parent == -1) // no parent?
                    {
                        scissor = view; // use full screen
                    }
                    else
                    {
                        Rectangle parentRect = widgets.rectangles[parent];
                        parentRect.Offset(root.Location);
                        scissor = parentRect;
                    }
                }

                // control point in scissor?
                if (!scissor.Contains(inputScript.ControlPosition)) { continue; }
                // control point in widget?
                if (!rect.Contains(inputScript.ControlPosition)) { continue; }

                hovered = true;

                switch (widgets.widgetTypes[i])
                {
                    case EWidgetType.text:
                    case EWidgetType.panel:
                        break;
                    case EWidgetType.button:
                        string key = widgets.IDs[i] + suffix;
                        if (bindings.ContainsKey(key))
                        {
                            foreach (var binding in bindings[key])
                            {
                                binding.Invoke(gameTime);
                            }
                        }
                        break;
                    case EWidgetType.label:
                    default:
                        break;
                }
            }

            if (hovered != bIsHovered)
            {
                if (hovered)
                {
                    AInput.PushFocus(EFocus.GameUI);
                }
                else
                {
                    AInput.PopFocus(EFocus.GameUI);
                }
                bIsHovered = hovered;
            }

            return 1;
        }
    }
}
