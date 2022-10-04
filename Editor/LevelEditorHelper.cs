using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Editor
{
    public static class LevelEditorHelper
    {
        #region LevelMahagment
        public const string LevelsPath = "Assets/Scripts/ScriptableObjects/Levels";
        public static int selectedLevel;
        public static List<LevelGrid> levels;
        public static List<LevelGrid> levelsToRemove;
        public static bool levelsDirty = false;
        public static Vector2 levelScrollPosition;
        public static float widthMin = 6;
        public static float widthMax = 255;
        public static float heightMin = 6;
        public static float heightMax = 255;
        #endregion

        #region TileTypeManagment
        public const string TilesPath = "Assets/Scripts/ScriptableObjects/Tiles";
        public static bool tilesDirty = false;
        public static int selectedTile;
        public static List<TileType> tiles;
        public static List<TileType> tilesToRemove;
        public static Rect tileWindowRect;

        public static Dictionary<TileMap.AliveType, TileType> tileCache;
        #endregion


        public static string GetUniqueName(IEnumerable<ScriptableObject> container, string name)
        {
            int n = 1;
            StringBuilder sb = new StringBuilder(name, 50);
            while (container.Select(t => t.name).Contains(sb.ToString()))
            {
                sb.Clear();
                sb.AppendFormat("{0} ({1})", name, n++);
            }
            return sb.ToString();
        }

        private static string GetUniqueName(string name)
        {
            int n = 1;
            StringBuilder sb = new StringBuilder(name, 50);
            while (tiles.Select(t => t.name).Contains(sb.ToString()))
            {
                sb.Clear();
                sb.AppendFormat("{0} ({1})", name, n++);
            }
            return sb.ToString();
        }
        internal static TileType GetSelectedTile()
        {
            if (selectedTile >= tiles.Count())
            {
                LoadTiles();
            }
            return tiles[selectedTile];
        }

        internal static TileType GetTileByType(TileMap.AliveType index)
        {
            return tileCache[index];
        }

        internal static void AddNewTile()
        {
            TileType newTile = ScriptableObject.CreateInstance<TileType>();
            newTile.name = GetUniqueName("New Tile");
            newTile.image = new Texture2D(40, 40);

            selectedTile = tiles.Count;
            tiles.Add(newTile);
            tilesDirty = true;
        }

        internal static LevelGrid GetSelectedLevel()
        {
            if (levels.Count > 0)
                return levels[selectedLevel];
            return null;
        }

        internal static void SaveTileAssets()
        {
            foreach (var asset in tilesToRemove)
            {
                var p = AssetDatabase.GetAssetPath(asset);
                AssetDatabase.DeleteAsset(p);
            }

            StringBuilder sb = new StringBuilder(TilesPath, 50);
            foreach (var asset in tiles)
            {
                var p = AssetDatabase.GetAssetPath(asset);
                if (p == "")
                {
                    sb.Clear();
                    sb.AppendFormat("{0}/{1}.asset", TilesPath, asset.name).ToString();

                    AssetDatabase.CreateAsset(asset, sb.ToString());
                }

                var path = AssetDatabase.GetAssetPath(asset);
                if (Path.GetFileNameWithoutExtension(path) != asset.name)
                {
                    string err = AssetDatabase.RenameAsset(path, asset.name);
                    if (err != string.Empty)
                    {
                        Debug.Log(err);
                    }
                }

                AssetDatabase.SaveAssetIfDirty(asset);
            }
            //AssetDatabase.SaveAssets();
            tilesDirty = false;
        }

        internal static void DeleteTileTypeInctance()
        {
            if (tiles.Count > 0)
            {
                var p = AssetDatabase.GetAssetPath(tiles[selectedTile]);
                if (p != "")
                {
                    tilesToRemove.Add(tiles[selectedTile]);
                }
                tiles.RemoveAt(selectedTile);
            }
            if (selectedTile >= tiles.Count)
            {
                selectedTile--;
            }
            tilesDirty = true;
        }

        internal static void LoadLevels()
        {
            Debug.Log("loading levels...");
            levelsToRemove = new List<LevelGrid>();
            string[] elements = Directory.GetFiles(LevelsPath, "*.asset");
            levels = new List<LevelGrid>();

            for (int i = 0; i < elements.Length; i++)
            {
                LevelGrid assets = AssetDatabase.LoadAssetAtPath<LevelGrid>(elements[i]);
                assets.name = Path.GetFileNameWithoutExtension(elements[i]);

                levels.Add(assets);
            }
        }

        internal static void AddLevelGridInctance(LevelGrid currentLevel)
        {
            LevelGrid newLevel = ScriptableObject.CreateInstance<LevelGrid>();
            newLevel.Init(GetUniqueName(levels, "New Level"));

            selectedLevel = levels.Count;
            currentLevel = newLevel;
            levels.Add(newLevel);
            levelsDirty = true;
        }

        internal static void SaveLevelGridAssets()
        {
            foreach (var asset in levelsToRemove)
            {
                var p = AssetDatabase.GetAssetPath(asset);
                AssetDatabase.DeleteAsset(p);
            }

            StringBuilder sb = new StringBuilder(LevelsPath, 50);
            foreach (var asset in levels)
            {
                var p = AssetDatabase.GetAssetPath(asset);
                if (p == "")
                {
                    sb.Clear();
                    sb.AppendFormat("{0}/{1}.asset", LevelsPath, asset.name);
                    AssetDatabase.CreateAsset(asset, sb.ToString());
                }
            }

            var level = levels[selectedLevel];
            var path = AssetDatabase.GetAssetPath(level);

            if (Path.GetFileNameWithoutExtension(path) != level.name)
            {
                string err = AssetDatabase.RenameAsset(path, level.name);
                if (err != string.Empty)
                {
                    Debug.Log(err);
                }
            }

            AssetDatabase.SaveAssetIfDirty(level);

            //AssetDatabase.SaveAssets();
            levelsDirty = false;
        }

        internal static void DeleteLevelGridInctance()
        {
            if (levels.Count > 0)
            {
                var p = AssetDatabase.GetAssetPath(levels[selectedLevel]);
                if (p != "")
                {
                    levelsToRemove.Add(levels[selectedLevel]);
                }
                levels.RemoveAt(selectedLevel);
            }
            if (selectedLevel >= levels.Count)
            {
                selectedLevel--;
            }
            levelsDirty = true;
        }

        internal static void LoadTiles()
        {
            tilesToRemove = new List<TileType>();
            Debug.Log("loading tiles...");
            string[] elements = Directory.GetFiles(TilesPath, "*.asset");
            tiles = new List<TileType>();
            tileCache = new Dictionary<TileMap.AliveType, TileType>();

            for (int i = 0; i < elements.Length; i++)
            {
                TileType asset = AssetDatabase.LoadAssetAtPath<TileType>(elements[i]);
                asset.name = Path.GetFileNameWithoutExtension(elements[i]);
                tiles.Add(asset);
                tileCache[asset.id] = asset;
            }
        }

        internal static void RebuildGrid(int oldWidth, int oldHeight, LevelGrid currentLevel)
        {
            if (currentLevel.tiles == null)
            {
                currentLevel.tiles = new TileMap.AliveType[currentLevel.width * currentLevel.height];
                for (int i = 0; i < currentLevel.width * currentLevel.height; i++)
                {
                    currentLevel.tiles[i] = TileMap.AliveType.None;
                }
                levelsDirty = true;
                return;
            }

            var newOne = new TileMap.AliveType[currentLevel.width * currentLevel.height];
            for (byte x = 0; x < currentLevel.width; ++x)
                for (byte y = 0; y < currentLevel.height; ++y)
                {
                    int index = x + oldWidth * y;
                    bool old = x < oldWidth && y < oldHeight && index < currentLevel.tiles.Length;
                    newOne[x + currentLevel.width * y] = old ? currentLevel.tiles[index] : TileMap.AliveType.None;
                }
            currentLevel.tiles = newOne;
            levelsDirty = true;
        }
    }
}