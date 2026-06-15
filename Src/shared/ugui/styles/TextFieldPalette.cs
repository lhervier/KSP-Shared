using UnityEngine;
using static com.github.lhervier.ksp.shared.ugui.styles.Utils;

namespace com.github.lhervier.ksp.shared.ugui.styles
{
    /// <summary>
    /// Couleurs et métriques par défaut du champ de saisie partagé (TextFieldBuilder), calquées sur
    /// le champ de recherche de la maquette (fond #0d0d0d, bordure #2a2a2a, texte #e8e8e8).
    /// Un builder peut surcharger ponctuellement ; sinon tout vient d'ici.
    /// </summary>
    public static class TextFieldPalette
    {
        // Hauteur d'un champ mono-ligne (un multi-ligne fixe sa propre hauteur via le builder).
        public const float FieldHeight = 22f;
        public const int FontSize = 12;

        // Marge interne entre la bordure et le texte (horizontale ; verticale en multi-ligne).
        public const float PaddingH = 7f;
        public const float PaddingV = 8f;

        public const int BorderThickness = 1;

        public static readonly Color BgColor = Rgb(13, 13, 13);          // #0d0d0d
        public static readonly Color BorderColor = Rgb(42, 42, 42);      // #2a2a2a
        public static readonly Color TextColor = Rgb(232, 232, 232);     // #e8e8e8
        public static readonly Color PlaceholderColor = Rgb(85, 85, 85); // #555
    }
}
