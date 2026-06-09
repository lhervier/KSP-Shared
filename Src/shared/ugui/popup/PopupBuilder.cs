using UnityEngine;
using UnityEngine.UI;
using com.github.lhervier.ksp.shared.ugui.styles;
using com.github.lhervier.ksp.shared.ugui.sprites;
using com.github.lhervier.ksp.shared.ugui.button;

namespace com.github.lhervier.ksp.shared.ugui.popup
{
    public class PopupBuilder<T, C> : IUGUIBuilder<PopupController> 
        where T : PopupTitleBarController
        where C : PopupContentController
    {
        private string _popupID = "Popup";
        private IUGUIBuilder<T> _titleBarBuilder;
        private IUGUIBuilder<C> _contentBuilder;
        private ButtonBuilder _buttonBuilder = new ButtonBuilder();

        private bool _hasPosition = false;
        private Vector2 _initialPosition;

        private bool _hasSize = false;
        private Vector2 _size;

        private Sprite _icon = null;
        private string _label = string.Empty;
        
        // ===============================================
        // Build parameters
        // ===============================================

        public PopupBuilder<T, C> PopupID(string id)
        {
            this._popupID = id;
            return this;
        }

        public PopupBuilder<T, C> Title(string title)
        {
            this._label = title;
            return this;
        }

        public PopupBuilder<T, C> Icon(Sprite icon)
        {
            this._icon = icon;
            return this;
        }

        public PopupBuilder<T, C> TitleBarBuilder(IUGUIBuilder<T> titleBarBuilder)
        {
            this._titleBarBuilder = titleBarBuilder;
            return this;
        }

        public PopupBuilder<T, C> ContentBuilder(IUGUIBuilder<C> contentBuilder)
        {
            this._contentBuilder = contentBuilder;
            return this;
        }

        public PopupBuilder<T, C> Position(Vector2 position)
        {
            this._initialPosition = position;
            this._hasPosition = true;
            return this;
        }

        public PopupBuilder<T, C> DeletePosition()
        {
            this._hasPosition = false;
            return this;
        }

        public PopupBuilder<T, C> Size(Vector2 size)
        {
            this._size = size;
            this._hasSize = true;
            return this;
        }

        public PopupBuilder<T, C> DeleteSize()
        {
            this._hasSize = false;
            return this;
        }

        // =============================================
        // Builder
        // =============================================

        /// <summary>
        /// Spawn the cheat-sheet popup window and return its controller, or null if KSP failed to spawn
        /// it. The caller drives the window through the returned controller.
        /// </summary>
        public PopupController Build()
        {
            // Creates a ultra minimal MultiOptionDialog. We will not use it.
            float positionX;
            float positionY;
            if( _hasPosition )
            {
                positionX = _initialPosition.x;
                positionY = _initialPosition.y;
            }
            else
            {
                positionX = PopupPalette.WindowInitialPositionX;
                positionY = PopupPalette.WindowInitialPositionY;
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
                width = PopupPalette.WindowWidth;
                height = PopupPalette.WindowHeight;
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
            
            PopupController controller = popupDialog.popupWindow.AddComponent<PopupController>();
            controller.BindPopupDialog(popupDialog);
            if (_hasPosition)
            {
                controller.InitializePosition(_initialPosition);
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
                controller.BindCanvasGroup(canvasGroup);
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
                image.color = PopupPalette.WindowBodyColor;
            }

            // Add the body (scrollable content). First in z-order so the overlay/menu draw above it.
            MonoBehaviour bodyController = this._contentBuilder.Build();
            bodyController.transform.SetParent(popupDialog.popupWindow.transform, false);

            // Add the title bar
            GameObject titleBarGo = this.CreateTitleBar(controller);
            titleBarGo.transform.SetParent(popupDialog.popupWindow.transform, false);

            return controller;
        }

        // =================================================
        // Popup Title Bar
        // =================================================

        private GameObject CreateTitleBar(PopupController controller)
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
            titleBarRect.sizeDelta = new Vector2(-2f * PopupPalette.WindowBorderThickness, PopupPalette.TitleBarHeight);
            titleBarRect.anchoredPosition = new Vector2(0f, -PopupPalette.WindowBorderThickness);

            // Image for the backgroup of the title bar
            var headerImage = titleBarGo.AddComponent<Image>();
            headerImage.sprite = SpritesGlobal.FillSprite;
            headerImage.type = Image.Type.Simple;
            headerImage.color = PopupPalette.TitleBarBackgroundColor;
            headerImage.raycastTarget = false;

            // The main part of the title with all the elements
            var rootGo = this.CreateTitleBarRoot(controller);
            rootGo.transform.SetParent(titleBarGo.transform, false);

            // The separator
            GameObject separatorGo = CreateSeparator();
            separatorGo.transform.SetParent(titleBarGo.transform, false);

            return titleBarGo;
        }

        public GameObject CreateTitleBarRoot(PopupController controller)
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

            var rightRow = this.CreateTitleBarRightColumn(controller);
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

            // Horizontal layout containing icon + label
            var leftColumnLayout = leftColumnGo.AddComponent<HorizontalLayoutGroup>();
            leftColumnLayout.spacing = DefaultPalette.Spacing;
            leftColumnLayout.childAlignment = TextAnchor.MiddleLeft;
            leftColumnLayout.childControlWidth = false;
            leftColumnLayout.childControlHeight = false;
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
                var iconRect = iconGo.GetComponent<RectTransform>();
                iconRect.sizeDelta = new Vector2(
                    DefaultPalette.IconSize,
                    DefaultPalette.IconSize
                );
            }
            return iconGo;
        }

        public GameObject CreateTitleBarLabel()
        {
            var labelGo = new GameObject("Popup.TitleBar.Label", typeof(RectTransform));
            
            var label = labelGo.AddComponent<Text>();
            label.text = this._label.ToUpperInvariant();
            label.font = HighLogic.UISkin.font;
            label.fontSize = 12;
            label.fontStyle = FontStyle.Bold;
            label.color = PopupPalette.TitleBarLabelColor;
            label.alignment = TextAnchor.MiddleLeft;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.raycastTarget = false;

            return labelGo;
        }

        // ===============================================
        // Popup title bar right column
        // ===============================================

        public GameObject CreateTitleBarRightColumn(PopupController controller)
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

            var closeButtonController = _buttonBuilder
                .ObjectName("Popup.TitleBar.RightColumn.CloseButton")
                .Label("×")
                .Interactable(true)
                .BackgroundColor(PopupPalette.TitleBarButtonColor)
                .HoverColor(PopupPalette.TitleBarButtonHoverColor)
                .Build();
            closeButtonController.transform.SetParent(rightRowGo.transform, false);
            controller.BindCloseButton(closeButtonController);

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
