using System;
using System.Collections.Generic;
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

        // Normalized table indexed by (int)VesselType. Holds the stock Sprite references as-is (the
        // UI atlas they belong to is never unloaded, so the references stay valid for the whole run).
        private static Sprite[] _icons;
        private static bool _attempted;

        // MapNode does NOT index its iconSprites[] by (int)VesselType: it remaps each type to a
        // position in a larger map-icon atlas (MapNode.GetIcon's VesselType switch). We must apply
        // that remap when reading from MapNode, otherwise e.g. Station (enum 9) would pick the wrong
        // sprite. VesselIconSprite and KSCVesselMarker, by contrast, index directly by (int)VesselType.
        private static readonly Dictionary<VesselType, int> MAP_NODE_INDEX = new Dictionary<VesselType, int>
        {
            { VesselType.Debris, 7 },
            { VesselType.SpaceObject, 21 },
            { VesselType.Probe, 18 },
            { VesselType.Rover, 19 },
            { VesselType.Lander, 14 },
            { VesselType.Ship, 20 },
            { VesselType.Station, 0 },
            { VesselType.Base, 5 },
            { VesselType.EVA, 13 },
            { VesselType.Flag, 11 },
            { VesselType.Plane, 23 },
            { VesselType.Relay, 24 },
            { VesselType.DeployedScienceController, 28 },
            { VesselType.DeployedGroundPart, 29 },
        };

        // MapNode's '_ => 22' default branch (types absent from the table above).
        private const int MAP_NODE_DEFAULT_INDEX = 22;

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
        // null and the callers fall back to text. Each source is normalized into a table indexed
        // by (int)VesselType. Sources are tried cleanest first (VesselIconSprite has an array
        // dedicated 1:1 to the enum, and KSCVesselMarker indexes the same way); MapNode is the
        // scene-bound fallback most likely to be in memory during flight, but it needs its remap.
        private static void EnsureLoaded()
        {
            if (_attempted)
            {
                return;
            }
            _attempted = true;

            Sprite[] raw = ReadSpriteArray<VesselIconSprite>("vesselTypeIcons");
            if (raw == null)
            {
                raw = ReadSpriteArray<KSCVesselMarker>("vesselIcons");
            }
            if (raw != null)
            {
                // Both arrays above are indexed directly by (int)VesselType.
                _icons = Reindex(raw, DirectIndex);
            }
            else
            {
                raw = ReadSpriteArray<MapNode>("iconSprites");
                if (raw != null)
                {
                    _icons = Reindex(raw, MapNodeIndex);
                }
            }

            if (_icons == null)
            {
                LOGGER.LogWarning("No source prefab found for stock vessel-type icons; callers fall back to text.");
            }
            else
            {
                LOGGER.LogDebug("Loaded stock vessel-type icons.");
            }
        }

        // The raw-array index of a vessel type for a source indexed 1:1 by the enum.
        private static int DirectIndex(VesselType type)
        {
            return (int)type;
        }

        // The raw-array index of a vessel type within MapNode.iconSprites (its remap table).
        private static int MapNodeIndex(VesselType type)
        {
            int idx;
            return MAP_NODE_INDEX.TryGetValue(type, out idx) ? idx : MAP_NODE_DEFAULT_INDEX;
        }

        // Builds a table indexed by (int)VesselType, pulling each entry from the source array
        // through the given index mapping. Entries whose mapped index is out of the source's
        // range are left null (Get falls back to Unknown).
        private static Sprite[] Reindex(Sprite[] raw, Func<VesselType, int> indexOf)
        {
            int max = 0;
            foreach (VesselType type in Enum.GetValues(typeof(VesselType)))
            {
                if ((int)type > max)
                {
                    max = (int)type;
                }
            }

            var table = new Sprite[max + 1];
            foreach (VesselType type in Enum.GetValues(typeof(VesselType)))
            {
                int src = indexOf(type);
                if (src >= 0 && src < raw.Length)
                {
                    table[(int)type] = raw[src];
                }
            }
            return table;
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
