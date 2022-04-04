using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Text;

namespace LD50.XMLSchema
{
    public enum EWidgetAnchor
    {
        N,
        NE,
        E,
        SE,
        S,
        SW,
        W,
        NW,
        C
    }

    public enum EWidgetType
    {
        panel,
        button,
        label,
        text
    }

    public class FCanvas : ISerialisedData
    {
        public string           ID;
        public Rectangle        root;
        public EWidgetAnchor    anchor;
        public int[]            parents;
        public string[]         IDs;
        public string[]         textures;
        public string[]         texts;
        public Rectangle[]      rectangles;
        public Rectangle[]      sources;
        public Color[]          colors;
        public Color[]          highlightcolors;
        public EWidgetType[]    widgetTypes;

        public string Type => "LD50.ACanvas";
    }
}
