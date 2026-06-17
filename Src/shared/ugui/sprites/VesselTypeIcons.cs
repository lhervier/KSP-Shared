using System;
using System.Reflection;
using UnityEngine;
using KSP.UI.Screens;
using KSP.UI.Screens.Mapview;

namespace com.github.lhervier.ksp.shared.ugui.sprites
{
    /// <summary>
    /// Stock vessel-type icon sprites (the ones KSP uses on the map and at the tracking
    /// station), recovered by reflection from the game's own UI prefabs. Falls back to no
    /// sprite when no source prefab can be found, letting the caller use a text label instead.
    /// </summary>
    internal static class VesselTypeIcons
    {
        private static readonly ModLogger LOGGER = new ModLogger("VesselTypeIcons");

        // Index 2 is VesselType.Unknown; the stock code uses it as the out-of-range fallback.
        private const int UNKNOWN_INDEX = 2;

        // Indexed by (int)VesselType. Holds the stock Sprite references as-is (the UI atlas
        // they belong to is never unloaded, so the references stay valid for the whole run).
        private static Sprite[] _icons;
        private static bool _attempted;

        /// <summary>The stock icon sprite for the given vessel type, or null when no source
        /// prefab could be found (caller should fall back to a text label).</summary>
        public static Sprite Get(VesselType type)
        {
            EnsureLoaded();
            if (_icons == null)
            {
                return null;
            }

            // Replicate the game's own rule (VesselIconSprite/KSCVesselMarker): an index past
            // the end of the table resolves to Unknown rather than throwing.
            int idx = (int)type;
            if (idx < 0 || idx > _icons.Length - 1)
            {
                idx = UNKNOWN_INDEX;
            }
            return idx >= 0 && idx < _icons.Length ? _icons[idx] : null;
        }

        /// <summary>The stock icon sprite for the vessel type given by its enum name (as
        /// VesselBookmark stores it in string form). Returns false when the name is not a known
        /// VesselType or no sprite is available.</summary>
        public static bool TryGet(string vesselTypeName, out Sprite sprite)
        {
            sprite = null;
            if (string.IsNullOrEmpty(vesselTypeName))
            {
                return false;
            }

            VesselType type;
            try
            {
                type = (VesselType)Enum.Parse(typeof(VesselType), vesselTypeName, true);
            }
            catch (ArgumentException)
            {
                return false;
            }

            sprite = Get(type);
            return sprite != null;
        }

        /// <summary>Whether the icon table has been populated (a source prefab was found).</summary>
        public static bool Available
        {
            get
            {
                EnsureLoaded();
                return _icons != null;
            }
        }

        // Lazy one-shot population, tried only once: if no source is loaded yet we keep _icons
        // null and the callers fall back to text. Sources are tried cleanest first
        // (VesselIconSprite has a table dedicated 1:1 to the enum), then the more scene-bound
        // ones that are more likely to be in memory during flight.
        private static void EnsureLoaded()
        {
            if (_attempted)
            {
                return;
            }
            _attempted = true;

            _icons = ReadSpriteArray<VesselIconSprite>("vesselTypeIcons")
                  ?? ReadSpriteArray<MapNode>("iconSprites")
                  ?? ReadSpriteArray<KSCVesselMarker>("vesselIcons");

            if (_icons == null)
            {
                LOGGER.LogWarning("No source prefab found for stock vessel-type icons; callers fall back to text.");
            }
            else
            {
                LOGGER.LogDebug("Loaded " + _icons.Length + " stock vessel-type icons.");
            }
        }

        // Returns the named Sprite[] field of the first loaded instance of T whose array is
        // non-null and non-empty, or null. FindObjectsOfTypeAll also returns prefabs that are
        // loaded in memory but not instantiated in any scene, which is how we reach these UI
        // prefabs from a flight scene without opening the map.
        private static Sprite[] ReadSpriteArray<T>(string fieldName) where T : Component
        {
            FieldInfo field = typeof(T).GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (field == null)
            {
                LOGGER.LogWarning("Field '" + fieldName + "' not found on " + typeof(T).Name + " (KSP version mismatch?).");
                return null;
            }

            T[] instances = Resources.FindObjectsOfTypeAll<T>();
            foreach (T instance in instances)
            {
                var sprites = field.GetValue(instance) as Sprite[];
                if (sprites != null && sprites.Length > 0)
                {
                    return sprites;
                }
            }
            return null;
        }
    }
}
