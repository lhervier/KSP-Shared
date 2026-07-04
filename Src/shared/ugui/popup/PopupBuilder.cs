using UnityEngine;
using UnityEngine.UI;
using TMPro;
using com.github.lhervier.ksp.shared.ugui.styles;
using com.github.lhervier.ksp.shared.ugui.sprites;
using com.github.lhervier.ksp.shared.ugui.button;

namespace com.github.lhervier.ksp.shared.ugui.popup
{
    public class PopupBuilder<T, C, O> : IUGUIBuilder<PopupController> 
        where T : MonoBehaviour
        where C : MonoBehaviour
        where O : MonoBehaviour
    {
        private static ModLogger LOGGER = new ModLogger("PopupBuilder");

        // ===============================================
        // Build parameters
        // ===============================================

        private string _popupID = null;
        public PopupBuilder<T, C, O> WithPopupID(string id)
        {
            this._popupID = id;
            return this;
        }

        private string _title = string.Empty;
        public PopupBuilder<T, C, O> WithTitle(string title)
        {
            this._title = title;
            return this;
        }

        private Sprite _icon = null;
        public PopupBuilder<T, C, O> WithIcon(Sprite icon)
        {
            this._icon = icon;
            return this;
        }

        private IUGUIBuilder<T> _titleBarBuilder;
        public PopupBuilder<T, C, O> WithTitleBarBuilder(IUGUIBuilder<T> titleBarBuilder)
        {
            this._titleBarBuilder = titleBarBuilder;
            return this;
        }

        private IUGUIBuilder<C> _contentBuilder;
        public PopupBuilder<T, C, O> WithContentBuilder(IUGUIBuilder<C> contentBuilder)
        {
            this._contentBuilder = contentBuilder;
            return this;
        }

        private IUGUIBuilder<O> _overlayBuilder;
        public PopupBuilder<T, C, O> WithOverlayBuilder(IUGUIBuilder<O> overlayBuilder)
        {
            this._overlayBuilder = overlayBuilder;
            return this;
        }

        private Vector2 _size;
        private bool _hasSize = false;
        public PopupBuilder<T, C, O> WithSize(Vector2 size)
        {
            this._size = size;
            this._hasSize = true;
            return this;
        }
        public PopupBuilder<T, C, O> WithoutSize()
        {
            this._hasSize = false;
            return this;
        }

        private GameObject _host = null;
        public PopupBuilder<T, C, O> WithHost(GameObject host)
        {
            this._host = host;
            return this;
        }

        // Whether pressing Escape closes the window. Defaults to false: KSP's native PopupDialog closes
        // on Escape, and we suppress that unless a mod explicitly opts back in.
        private bool _closeOnEscape = false;
        public PopupBuilder<T, C, O> WithCloseOnEscape(bool value)
        {
            this._closeOnEscape = value;
            return this;
        }

        // The single settings instance, shared between the controller (open state) and each spawned
        // window (position). Created at Build, read by every Spawn.
        private PopupSettings _settings;

        // =============================================
        // Builder
        // =============================================

        /// <summary>
        /// Create the persistent popup controller on the host GameObject and return it, or null on bad
        /// input. The window itself is NOT spawned here: the controller spawns it lazily on the first
        /// Show(), and re-spawns it after KSP destroys it. The caller drives everything through the
        /// returned controller (Show/Hide/RestoreState/OnOpenChanged).
        /// </summary>
        public PopupController Build()
        {
            if( this._popupID == null )
            {
                LOGGER.LogError("Unable to create a popup without a popup ID");
                return null;
            }
            if( this._host == null )
            {
                LOGGER.LogError("Unable to create a popup without a host GameObject");
                return null;
            }

            // Single settings instance shared with every spawned window (see field comment).
            this._settings = new PopupSettings(this._popupID);

            // The controller lives on the mod's host GameObject so it survives KSP destroying the window,
            // and drives it through the injected spawn function.
            return this._host
                .AddComponent<PopupController>()
                .WithSettings(this._settings)
                .WithSpawnResultProducer(this.Spawn);
        }

        /// <summary>
        /// Spawn a fresh window instance (PopupDialog + chrome + title bar + content + overlays) and return
        /// its pieces, or null if KSP failed to spawn it. Invoked lazily by the controller on each (re)open,
        /// so the sub-builders are re-run to produce a brand-new visual tree every time.
        /// </summary>
        internal PopupSpawnResult Spawn()
        {
            PopupSettings settings = this._settings;

            // Creates a ultra minimal MultiOptionDialog. We will not use it.
            float positionX;
            float positionY;
            if( settings.HasWindowPosition )
            {
                positionX = settings.WindowPosition.x;
                positionY = settings.WindowPosition.y;
            }
            else
            {
                positionX = PopupPalette.PopupDefaultPositionX;
                positionY = PopupPalette.PopupDefaultPositionY;
            }
            float width;
            float height;
            if( _hasSize )
            {
                width = _size.x;
                height = _size.y;
            }
            else
            {
                width = PopupPalette.PopupDefaultWidth;
                height = PopupPalette.PopupDefaultHeight;
            }
            var pos = NormalizedWindowPos(
                positionX, 
                positionY, 
                width,
                height
            );
            var content = new DialogGUIVerticalLayout();
            MultiOptionDialog multiOptionDialog = new MultiOptionDialog(
                this._popupID,
                string.Empty,
                string.Empty,
                HighLogic.UISkin,
                pos,
                new DialogGUIBase[]
                {
                    new DialogGUIBox(null, -1, -1, () => true, content)
                }
            );
            
            // Creates the popup dialog
            PopupDialog popupDialog = PopupDialog.SpawnPopupDialog(
                multiOptionDialog,
                true,
                HighLogic.UISkin,
                false,
                string.Empty
            );
            if( popupDialog == null || popupDialog.popupWindow == null )
            {
                return null;
            }

            // KSP's PopupDialog.Update() closes the window on Escape (Input.GetKeyUp). Disabling the
            // component suppresses its Update/FixedUpdate/LateUpdate: for this popup the DialogGUI content
            // is an empty shell (a DialogGUIBox around an empty layout), so those are no-ops anyway, while
            // the drag panel and our own uGUI tree keep working independently.
            if( !_closeOnEscape )
            {
                popupDialog.enabled = false;
            }

            // Remove KSP default title
            var title = popupDialog.popupWindow.transform.Find("Title");
            title?.gameObject.SetActive(false);

            // Keep the window hidden until it has been positioned. KSP re-applies the initial
            // spawn position on every layout pass, so the window would otherwise flicker at the
            // default position before being moved to the saved one. The controller reveals it
            // (alpha 1) from Show(), once the layout has settled and the position has been applied.
            var canvasGroup = popupDialog.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }

            // Set windows border color 
            var windowGo = popupDialog.popupWindow;
            var windowImage = windowGo.GetComponent<Image>();
            if (windowImage != null)
            {
                windowImage.sprite = SpritesPopupDialog.WindowChromeSprite;
                windowImage.type = Image.Type.Sliced;
                windowImage.color = Color.white;

                // Raycast to prevent mouse event to be sent to the game
                windowImage.raycastTarget = true;
            }

            // Set windows background color
            foreach (var image in windowGo.GetComponentsInChildren<Image>(true))
            {
                if (image == windowImage)
                {
                    continue;
                }

                image.sprite = SpritesGlobal.FillSprite;
                image.type = Image.Type.Simple;
                image.color = PopupPalette.PopupBodyColor;
            }

            // Content host: frames the region where the content lives — the window interior minus the
            // chrome border and the title bar — and escapes KSP's VerticalLayoutGroup. Added first in
            // z-order so the title bar (and any overlay/menu) draws above it. Content builders only fill
            // this host; they no longer reason about the chrome or the title bar.
            GameObject contentHostGo = this.CreateContentHost();
            contentHostGo.transform.SetParent(popupDialog.popupWindow.transform, false);

            // Build the content and stretch it to fill the host.
            MonoBehaviour contentController = this._contentBuilder.Build();
            var contentRect = contentController.GetComponent<RectTransform>();
            contentRect.SetParent(contentHostGo.transform, false);
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;

            // Add the title bar
            GameObject titleBarGo = this.CreateTitleBar(out ButtonController closeButtonController);
            titleBarGo.transform.SetParent(popupDialog.popupWindow.transform, false);

            // Window-level overlays (menus, modal internal popups), grafted last so they draw and receive
            // clicks above both the content host and the title bar. Unlike the encased content host, they
            // stretch over the WHOLE window (title bar included): the builder only assembles the overlays on
            // its root, we own its placement — ignoreLayout to escape the window's VerticalLayoutGroup, then
            // stretched to fill the window. This keeps that boilerplate out of every mod's overlay builder.
            if( this._overlayBuilder != null )
            {
                O overlayController = this._overlayBuilder.Build();
                var overlayRect = overlayController.GetComponent<RectTransform>();
                overlayRect.SetParent(popupDialog.popupWindow.transform, false);
                overlayController.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
                overlayRect.anchorMin = Vector2.zero;
                overlayRect.anchorMax = Vector2.one;
                overlayRect.offsetMin = Vector2.zero;
                overlayRect.offsetMax = Vector2.zero;
            }

            // Hand the pieces back to the controller, which owns the lifecycle. Note: unlike before, no
            // component is added onto the window — the controller lives on the mod's host GameObject.
            return new PopupSpawnResult
            {
                PopupDialog = popupDialog,
                CanvasGroup = canvasGroup,
                CloseButtonController = closeButtonController,
            };
        }

        // =================================================
        // Content host
        // =================================================

        /// <summary>
        /// Frame the region where the popup content lives: the window interior minus the chrome border
        /// on every side and the title bar at the top. The content is then stretched to fill it.
        /// </summary>
        private GameObject CreateContentHost()
        {
            var hostGo = new GameObject("Popup.ContentHost", typeof(RectTransform));

            // Escape the popupWindow's VerticalLayoutGroup so we position ourselves by anchors.
            var layoutElement = hostGo.AddComponent<LayoutElement>();
            layoutElement.ignoreLayout = true;

            // Fills the window interior minus chrome (1px on left/right/bottom) and the title bar at the top
            var rect = hostGo.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(PopupPalette.PopupBorderThickness, PopupPalette.PopupBorderThickness);
            rect.offsetMax = new Vector2(
                -PopupPalette.PopupBorderThickness,
                -(PopupPalette.PopupBorderThickness + PopupPalette.TitleBarHeight)
            );

            return hostGo;
        }

        // =================================================
        // Popup Title Bar
        // =================================================

        private GameObject CreateTitleBar(out ButtonController closeButtonController)
        {
            var titleBarGo = new GameObject("Popup.TitleBar", typeof(RectTransform));
            
            // If the parent has a layout (and that's the case), forget about me, I will position elements myself.
            var titleBarLayout = titleBarGo.AddComponent<LayoutElement>();
            titleBarLayout.ignoreLayout = true;

            // Title bar zone relative to the parent, stretched horizontaly
            // Beware not to overlap the borders
            var titleBarRect = titleBarGo.GetComponent<RectTransform>();
            titleBarRect.anchorMin = new Vector2(0f, 1f);
            titleBarRect.anchorMax = new Vector2(1f, 1f);
            titleBarRect.pivot = new Vector2(0.5f, 1f);
            titleBarRect.sizeDelta = new Vector2(-2f * PopupPalette.PopupBorderThickness, PopupPalette.TitleBarHeight);
            titleBarRect.anchoredPosition = new Vector2(0f, -PopupPalette.PopupBorderThickness);

            // Image for the backgroup of the title bar
            var headerImage = titleBarGo.AddComponent<Image>();
            headerImage.sprite = SpritesGlobal.FillSprite;
            headerImage.type = Image.Type.Simple;
            headerImage.color = PopupPalette.TitleBarBackgroundColor;
            headerImage.raycastTarget = false;

            // The main part of the title with all the elements
            var rootGo = this.CreateTitleBarRoot(out closeButtonController);
            rootGo.transform.SetParent(titleBarGo.transform, false);

            // The separator
            GameObject separatorGo = CreateSeparator();
            separatorGo.transform.SetParent(titleBarGo.transform, false);

            return titleBarGo;
        }

        public GameObject CreateTitleBarRoot(out ButtonController closeButtonController)
        {
            var rootGo = new GameObject("Popup.TitleBar.Root", typeof(RectTransform));
            
            // Full size of the parent = the title bar, minus the bottom separator
            var rootRect = rootGo.GetComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = new Vector2(0f, PopupPalette.TitleBarSeparatorHeight);
            rootRect.offsetMax = Vector2.zero;

            // Horizontal layout splitting the title bar in two cells (left + right)
            var rootLayout = rootGo.AddComponent<HorizontalLayoutGroup>();
            rootLayout.padding = new RectOffset(
                Mathf.RoundToInt(DefaultPalette.PaddingLeft),
                Mathf.RoundToInt(DefaultPalette.PaddingRight),
                Mathf.RoundToInt(DefaultPalette.PaddingTop),
                Mathf.RoundToInt(DefaultPalette.PaddingBottom)
            );
            rootLayout.spacing = 0f;
            rootLayout.childAlignment = TextAnchor.MiddleLeft;
            // Width controlled by the layout so flexibleWidth on LeftRow pushes RightRow to the right
            // Height forced so the rows fill the title bar's height for proper vertical centering
            rootLayout.childControlWidth = true;
            rootLayout.childControlHeight = true;
            rootLayout.childForceExpandWidth = false;
            rootLayout.childForceExpandHeight = true;

            var leftRow = this.CreateTitleBarLeftColumn();
            leftRow.transform.SetParent(rootGo.transform, false);

            var rightRow = this.CreateTitleBarRightColumn(out closeButtonController);
            rightRow.transform.SetParent(rootGo.transform, false);

            return rootGo;
        }

        // ===============================================
        // Popup title bar left column (icon + label)
        // ===============================================

        public GameObject CreateTitleBarLeftColumn()
        {
            var leftColumnGo = new GameObject("Popup.TitleBar.LeftColumn", typeof(RectTransform));
            
            // Greedy on width so it consumes the leftover space and pushes the right row against the right edge
            var leftColumnLayoutElement = leftColumnGo.AddComponent<LayoutElement>();
            leftColumnLayoutElement.flexibleWidth = 1f;

            // Horizontal layout containing icon + label. Children sizes are layout-controlled so the
            // column reports its TRUE preferred width (icon + text). With uncontrolled widths, the
            // preferred width would come from the children's default sizeDelta (100px for the label),
            // over-reserving space and squeezing the right column even when the title is short.
            var leftColumnLayout = leftColumnGo.AddComponent<HorizontalLayoutGroup>();
            leftColumnLayout.spacing = DefaultPalette.Spacing;
            leftColumnLayout.childAlignment = TextAnchor.MiddleLeft;
            leftColumnLayout.childControlWidth = true;
            leftColumnLayout.childControlHeight = true;
            leftColumnLayout.childForceExpandWidth = false;
            leftColumnLayout.childForceExpandHeight = false;

            if( this._icon != null )
            {
                var iconGo = CreateTitleBarIcon();
                iconGo.transform.SetParent(leftColumnLayoutElement.transform, false);
            }

            var labelGo = this.CreateTitleBarLabel();
            labelGo.transform.SetParent(leftColumnLayoutElement.transform, false);

            return leftColumnGo;
        }

        public GameObject CreateTitleBarIcon()
        {
            var iconGo = new GameObject("Popup.TitleBar.Icon", typeof(RectTransform));

            // The icon itself
            var iconImage = iconGo.AddComponent<Image>();
            iconImage.sprite = this._icon;
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false;
            if (iconImage.sprite == null)
            {
                iconGo.SetActive(false);
            }
            else
            {
                // Pin the icon to IconSize: without a LayoutElement, the parent layout would size
                // it to the Image's preferred size, which is the sprite's native resolution.
                var iconElement = iconGo.AddComponent<LayoutElement>();
                iconElement.minWidth = DefaultPalette.IconSize;
                iconElement.minHeight = DefaultPalette.IconSize;
                iconElement.preferredWidth = DefaultPalette.IconSize;
                iconElement.preferredHeight = DefaultPalette.IconSize;
            }
            return iconGo;
        }

        public GameObject CreateTitleBarLabel()
        {
            var labelGo = new GameObject("Popup.TitleBar.Label", typeof(RectTransform));
            
            var label = UGUILabels.AddLabel(labelGo);
            label.text = this._title.ToUpperInvariant();
            label.fontSize = PopupPalette.TitleBarLabelFontSize;
            label.fontStyle = FontStyles.Bold;
            label.color = PopupPalette.TitleBarLabelColor;
            label.alignment = TextAlignmentOptions.Left;

            return labelGo;
        }

        // ===============================================
        // Popup title bar right column
        // ===============================================

        public GameObject CreateTitleBarRightColumn(out ButtonController closeButtonController)
        {
            var rightRowGo = new GameObject("Popup.TitleBar.RightColumn", typeof(RectTransform));
            
            // Horizontal layout containing the right-side placeholders, sized to their text content
            var rightRowLayout = rightRowGo.AddComponent<HorizontalLayoutGroup>();
            rightRowLayout.spacing = DefaultPalette.Spacing;
            rightRowLayout.childAlignment = TextAnchor.MiddleLeft;
            rightRowLayout.childControlWidth = true;
            rightRowLayout.childControlHeight = true;
            rightRowLayout.childForceExpandWidth = false;
            rightRowLayout.childForceExpandHeight = false;

            var actionGroupLabelController = this._titleBarBuilder.Build();
            actionGroupLabelController.transform.SetParent(rightRowGo.transform, false);

            closeButtonController = new ButtonBuilder()
                .WithObjectName("Popup.TitleBar.RightColumn.CloseButton")
                .WithLabel("×")
                .WithInteractableState(true)
                .WithBackgroundColor(PopupPalette.TitleBarButtonColor)
                .WithHoverColor(PopupPalette.TitleBarButtonHoverColor)
                .Build();
            closeButtonController.transform.SetParent(rightRowGo.transform, false);
            
            return rightRowGo;
        }

        // ============================================
        // Separator between title bar and content
        // ============================================

        public GameObject CreateSeparator()
        {
            var separatorGo = new GameObject("Popup.TitleBar.Separator", typeof(RectTransform));
            
            // Stretched horizontally, positionned at the bottom of the parent
            var separatorRect = separatorGo.GetComponent<RectTransform>();
            separatorRect.anchorMin = new Vector2(0f, 0f);
            separatorRect.anchorMax = new Vector2(1f, 0f);
            separatorRect.pivot = new Vector2(0.5f, 0f);
            separatorRect.sizeDelta = new Vector2(0f, PopupPalette.TitleBarSeparatorHeight);
            separatorRect.anchoredPosition = Vector2.zero;
            
            // The separator
            var separatorImage = separatorGo.AddComponent<Image>();
            separatorImage.sprite = SpritesGlobal.FillSprite;
            separatorImage.type = Image.Type.Simple;
            separatorImage.color = PopupPalette.TitleBarSeparatorColor;
            separatorImage.raycastTarget = false;

            return separatorGo;
        }

        // ============================================
        // Helpers
        // ============================================

        /// <summary>
        /// Normalized position from screen top-left, expressed as a percentage of the screen width and height.
        /// </summary>
        private static Rect NormalizedWindowPos(float screenX, float screenYFromTop, float width, float height)
        {
            var centerX = screenX + width * 0.5f;
            var centerY = Screen.height - screenYFromTop - height * 0.5f;
            return new Rect(centerX / Screen.width, centerY / Screen.height, width, height);
        }
    }
}
