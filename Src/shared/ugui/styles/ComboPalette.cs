using UnityEngine;
using static com.github.lhervier.ksp.shared.ugui.styles.Utils;

namespace com.github.lhervier.ksp.shared.ugui.styles
{
    public class ComboPalette
    {
        public const float Spacing = 6f;
        public const float Height = 22f;
        public const float PaddingH = 8f;
        public const float DropdownMaxHeight = 170f;

        public const int LabelFontSize = 11;
        public const int ComboFontSize = 12;
        public const int CaretFontSize = 10;
        
        public static readonly Color LabelColor = Rgb(153, 153, 153);
        public static readonly Color BackgroundColor = Rgb(42, 42, 42);
        public static readonly Color BorderColor = Rgb(85, 85, 85);
        public static readonly Color HoverColor = Rgb(51, 51, 51);
        public static readonly Color ComboTextColor = Rgb(232, 232, 232);
        public static readonly Color CaretColor = Rgb(136, 136, 136);
        public static readonly Color ComboBgColor = Rgb(30, 30, 30);
        public static readonly Color ItemColor = Rgb(221, 221, 221);
        public static readonly Color ItemHoverColor = Rgb(42, 42, 42);
        public static readonly Color ItemSelectedColor = Rgb(141, 190, 69);
        public static readonly Color ItemSelectedBgColor = Rgba(141, 190, 69, 0.08f);
    }
}