using System;
using System.Collections.Generic;
using System.Text;
using ModKit;
using UnityEngine;

namespace ArmyManager
{
    // ReSharper disable InconsistentNaming
    public static class UIStyles
    {
        public static GUIStyle AlignCenter = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 12.point()
        };

        public static GUIStyle AlignRight = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleRight,
            fontSize = 12.point()
        };

        public static GUIStyle AlignLeft = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleLeft,
            fontSize = 12.point()
        };

        public static readonly GUIStyle NationSelectButton = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14.point(),
            alignment = TextAnchor.MiddleLeft
        };

        public static readonly GUIStyle RegionSelectButton = new GUIStyle(GUI.skin.label)
        {
            fontSize = 12.point(),
            alignment = TextAnchor.MiddleLeft
        };

        public static readonly GUIStyle SimpleButton = new GUIStyle(UI.textBoxStyle)
        {
            fontSize = 12.point()
        };

        public static GUIStyle Hint = new GUIStyle(GUI.skin.label)
        {
            fontSize = 11.point(),
            fontStyle = FontStyle.Italic,
            alignment = TextAnchor.MiddleLeft
        };

        public static GUIStyle MovingHint = new GUIStyle(Hint)
        {
            alignment = TextAnchor.MiddleCenter
        };

        public static GUIStyle Header = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14.point(),
            alignment = TextAnchor.MiddleLeft
        };

        public static GUIStyle Label = FontSize(GUI.skin.label, 12);

        public static GUIStyle FontSize(GUIStyle baseStyle, int size) => new GUIStyle(baseStyle)
        {
            fontSize = size.point()
        };
    }
}
