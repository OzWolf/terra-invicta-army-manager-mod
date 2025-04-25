using System;
using ModKit;
using PavonisInteractive.TerraInvicta;
using UnityEngine;

namespace ArmyManager
{
    // ReSharper disable InconsistentNaming
    public static class UIElements
    {
        private static readonly GUIStyle HeaderStyle = new GUIStyle(UIStyles.AlignCenter)
        {
            fontSize = 16.point()
        };

        public static void ValueAdjuster(ref int value, Action<int> onChange, int min, int max,
            params GUILayoutOption[] options)
        {
            var v = value;
            using (UI.HorizontalScope(options))
            {
                if (v > min)
                    UI.ActionButton(" < ".bold(), () => OnValueChange(-1), UIStyles.SimpleButton);
                else
                    UI.ActionButton(" < ".grey(), () => { }, UIStyles.SimpleButton);

                UI.Label(v.ToString().orange().bold(), UIStyles.SimpleButton, UI.MinWidth(30), UI.MaxWidth(30));

                if (v < max)
                    UI.ActionButton(" > ".bold(), () => OnValueChange(1), UIStyles.SimpleButton);
                else
                    UI.ActionButton(" > ".grey(), () => { }, UIStyles.SimpleButton);
            }

            return;

            void OnValueChange(int delta)
            {
                onChange(v + delta);
            }
        }

        public static void Header(string title, float? width = null)
        {
            UI.Space(20);
            UI.Div();
            UI.Space(10);
            UI.Label(title.ToUpper().orange().bold(), HeaderStyle, UI.MinWidth(width ?? UI.ummWidth));
            UI.Space(10);
            UI.Div();
            UI.Space(20);
        }

        public static void Flag(TINationState nation)
        {
            var style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = new GUIStyleState
                {
                    background = nation.flag.texture
                },
                fixedWidth = 55,
                fixedHeight = 55 * (nation.flag.textureRect.height / nation.flag.textureRect.width)
            };

            UI.Label(" ", style);
        }
    }
}
