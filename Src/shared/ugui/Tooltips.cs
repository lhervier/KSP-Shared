using KSP.UI.TooltipTypes;
using UnityEngine;
using UnityEngine.UI;
using com.github.lhervier.ksp.shared.ugui.sprites;
using com.github.lhervier.ksp.shared.ugui.styles;

namespace com.github.lhervier.ksp.shared.ugui
{
    /// <summary>
    /// Attache des tooltips natifs KSP (TooltipController_Text) à un GameObject. KSP gère le survol,
    /// le positionnement et l'affichage sur SON canvas de tooltip dédié — donc le tooltip déborde
    /// naturellement hors de la fenêtre, n'est jamais clippé.
    ///
    /// Pour le style : on ne touche PAS au prefab stock. On en fait un clone restylé une seule fois
    /// (fond sombre + bordure verte, texte clair) avec un nom unique, et on donne ce clone à nos
    /// controllers — les tooltips du jeu gardent leur apparence.
    /// Pré-requis : le GameObject doit avoir un Graphic raycastTarget (le cas des boutons/icônes).
    /// </summary>
    internal static class Tooltips
    {
        private static Tooltip_Text _styledPrefab;
        private static bool _resolved;

        private static Tooltip_Text StyledPrefab
        {
            get
            {
                if (_resolved) return _styledPrefab;
                _resolved = true;

                Tooltip_Text stock = AssetBase.GetPrefab<Tooltip_Text>("Tooltip_Text");
                if (stock == null) return null;

                // Clone restylé, conservé entre les scènes, jamais affiché tel quel.
                Tooltip_Text clone = Object.Instantiate(stock);
                clone.gameObject.name = "VesselBookmark_Tooltip";
                clone.gameObject.SetActive(false);
                Object.DontDestroyOnLoad(clone.gameObject);
                Restyle(clone.gameObject);

                _styledPrefab = clone;
                return _styledPrefab;
            }
        }

        public static void Attach(GameObject go, string text)
        {
            if (go == null || string.IsNullOrEmpty(text)) return;
            Tooltip_Text prefab = StyledPrefab;
            if (prefab == null) return;

            var controller = go.AddComponent<TooltipController_Text>();
            controller.prefab = prefab;
            controller.RequireInteractable = false;   // affiche même sur un élément non-interactable / grisé
            controller.textString = text;
        }

        // Applique le thème sombre/vert au clone : fond chromé + texte clair.
        private static void Restyle(GameObject root)
        {
            // Fond(s) : sprite chromé (fond sombre + bordure verte), teinté blanc pour montrer le sprite.
            foreach (Image img in root.GetComponentsInChildren<Image>(true))
            {
                img.sprite = SpritesGlobal.Border(TooltipPalette.BackgroundColor, TooltipPalette.AccentBorderColor, 1);
                img.type = Image.Type.Sliced;
                img.color = Color.white;
            }

            // Texte : le label TMP EST un UnityEngine.UI.Graphic → on règle sa couleur sans référencer TMP.
            foreach (Graphic g in root.GetComponentsInChildren<Graphic>(true))
            {
                if (g is Image) continue;
                g.color = TooltipPalette.TextColor;
            }
        }
    }
}
