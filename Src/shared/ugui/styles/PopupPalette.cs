using UnityEngine;

namespace com.github.lhervier.ksp.shared.ugui.styles
{
    public static class PopupPalette
    {
        public const float PopupDefaultPositionX = 428f;
        public const float PopupDefaultPositionY = 20f;
        public const float PopupDefaultWidth = 350f;
        public const float PopupDefaultHeight = 400f;

        public const float PopupBorderThickness = 1f;
        
        // Colors
        public static readonly Color PopupBodyColor = new Color(20f / 255f, 20f / 255f, 20f / 255f);
        public static readonly Color PopupBorderColor = new Color(85f / 255f, 85f / 255f, 85f / 255f);
        
        // ===============================================================
        // Main content
        // ===============================================================

        public const float PopupScrollbarWidth = 8f;
        public const float PopupPlaceholderHeight = 800f;
        public static readonly Color PopupScrollbarColor = new Color(136f / 255f, 136f / 255f, 136f / 255f);

        // ===============================================================
        // Title bar
        // ===============================================================
        public const float TitleBarHeight = 28f;
        public const float TitleBarSeparatorHeight = 1f;
        public const float TitleBarActionGroupBorderThickness = 1f;
        public static readonly Color TitleBarLabelColor = new Color(232f / 255f, 232f / 255f, 232f / 255f);
        
        // Colors
        public static readonly Color TitleBarBackgroundColor = new Color(46f / 255f, 46f / 255f, 46f / 255f);
        public static readonly Color TitleBarSeparatorColor = new Color(68f / 255f, 68f / 255f, 68f / 255f);
        public static readonly Color TitleBarButtonColor = new Color(56f / 255f, 56f / 255f, 56f / 255f);
        public static readonly Color TitleBarButtonHoverColor = new Color(72f / 255f, 72f / 255f, 72f / 255f);
    }
}
