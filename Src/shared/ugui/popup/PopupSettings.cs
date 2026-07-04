using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using UnityEngine;
using com.github.lhervier.ksp.shared;

namespace com.github.lhervier.ksp.shared.ugui.popup {

    /// <summary>
    /// Réglages d'une popup (par installation, indépendants de la sauvegarde). Persistés dans
    /// PluginData/[popupID].cfg à côté de la DLL — le dossier PluginData est ignoré par GameDatabase,
    /// donc KSP ne tente pas d'interpréter ce fichier comme un config de pièce.
    /// </summary>
    public class PopupSettings {
        private static readonly ModLogger LOGGER = new ModLogger("PopupSettings");

        private const string ROOT_NODE = "POPUP_SETTINGS";
        
        private string _popupID;

        /// <summary>Memorized window position (localPosition), if it has already been captured.</summary>
        public bool HasWindowPosition { get; private set; }
        public Vector2 WindowPosition { get; private set; }

        public void SetWindowPosition(Vector2 position) {
            WindowPosition = position;
            HasWindowPosition = true;
        }

        /// <summary>Memorized open/closed state of the window, if it has already been captured.</summary>
        public bool HasWindowVisible { get; private set; }
        public bool WindowVisible { get; private set; }

        public void SetWindowVisible(bool visible) {
            WindowVisible = visible;
            HasWindowVisible = true;
        }

        public PopupSettings(string popupID) {
            try {
                _popupID = popupID;
                string path = GetSettingsPath();
                if (!File.Exists(path)) {
                    return;
                }
                ConfigNode root = ConfigNode.Load(path);
                ConfigNode node = root?.GetNode(ROOT_NODE);
                if (node == null) {
                    return;
                }
                if (node.HasValue("windowX") && node.HasValue("windowY")
                    && float.TryParse(node.GetValue("windowX"), NumberStyles.Float, CultureInfo.InvariantCulture, out float x)
                    && float.TryParse(node.GetValue("windowY"), NumberStyles.Float, CultureInfo.InvariantCulture, out float y)) {
                    WindowPosition = new Vector2(x, y);
                    HasWindowPosition = true;
                }
                if (node.HasValue("windowVisible")
                    && bool.TryParse(node.GetValue("windowVisible"), out bool windowVisible)) {
                    WindowVisible = windowVisible;
                    HasWindowVisible = true;
                }
            } catch (Exception e) {
                LOGGER.LogError($"Error loading settings: {e.Message}");
            }
        }

        public void Save() {
            try {
                var root = new ConfigNode();
                ConfigNode node = root.AddNode(ROOT_NODE);
                if (HasWindowPosition) {
                    node.AddValue("windowX", WindowPosition.x.ToString(CultureInfo.InvariantCulture));
                    node.AddValue("windowY", WindowPosition.y.ToString(CultureInfo.InvariantCulture));
                }
                if (HasWindowVisible) {
                    node.AddValue("windowVisible", WindowVisible.ToString());
                }
                
                string path = GetSettingsPath();
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                root.Save(path);
            } catch (Exception e) {
                LOGGER.LogError($"Error saving settings: {e.Message}");
            }
        }

        private string GetSettingsPath() {
            if( string.IsNullOrEmpty(_popupID) )
            {
                return null;
            }
            string dir = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "PluginData"
            );
            return Path.Combine(dir, _popupID + ".cfg");
        }
    }
}
