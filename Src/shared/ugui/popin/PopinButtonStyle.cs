namespace com.github.lhervier.ksp.shared.ugui.popin
{
    /// <summary>
    /// Visual style of a popin footer button. Each value maps to a color triple (background / hover / text)
    /// declared in <see cref="com.github.lhervier.ksp.shared.ugui.styles.PopinPalette"/>.
    /// </summary>
    public enum PopinButtonStyle
    {
        /// <summary>Neutral button (e.g. Cancel).</summary>
        Normal,

        /// <summary>Positive/confirming action (green, e.g. Save).</summary>
        Confirm,

        /// <summary>Destructive action (red, e.g. Remove).</summary>
        Alert
    }
}
