using UnityEngine;
using static com.github.lhervier.ksp.shared.ugui.styles.Utils;

namespace com.github.lhervier.ksp.shared.ugui.styles
{
    public static class SelectionRingPalette
    {
        public const int RingThickness = 1;

        // Deliberately neutral rather than accented: the accent means "linked to the current vessel",
        // a state the ring has to be able to overlay without being taken for it.
        public static readonly Color RingColor = Rgba(232, 232, 232, 0.5f);
    }
}
