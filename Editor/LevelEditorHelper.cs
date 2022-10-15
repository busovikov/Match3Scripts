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
        public static Dictionary<System.Type, int> selectedTile;
        public static Dictionary<System.Type,List<TileType>> tiles;
        public static List<TileType> tilesToRemove;
        public static Rect tileWindowRect;

        public static Dictionary<System.Enum, TileType> gizmos;
        #endregion

        public static bool SetSelectedTile(System.Type t, int value)
        {
            if (selectedTile[t] != value)
            {
                foreach(var index in selectedTile.Keys.ToList())
                {
                    selectedTile[index] = t == index ? value : -1;
                }
                return true;
            }
            return false;
        }
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
            //while (tiles.Select(t => t.name).Contains(sb.ToString()))
            {
                sb.Clear();
                sb.AppendFormat("{0} ({1})", name, n++);
            }
            return sb.ToString();
        }
        internal static TileType GetSelectedTile(System.Type t)
        {
            if (selectedTile == null || !selectedTile.ContainsKey(t) || tiles == null || !tiles.ContainsKey(t) || selectedTile[t] >= tiles[t].Count())
            {
                LoadTiles();
            }
            return tiles[t][selectedTile[t]];
        }

        internal static void AddNewTile(System.Type t)
        {
            if (!tiles.ContainsKey(t) || !selectedTile.ContainsKey(t))
            {
                return;
            }
            TileType newTile = ScriptableObject.CreateInstance(t.Name) as TileType;
            newTile.name = GetUniqueName("New Tile");
            newTile.image = new Texture2D(40, 40);

            selectedTile[t] = tiles[t].Count;
            tiles[t].Add(newTile);
            tilesDirty = true;
        }

        internal static LevelGrid GetSelectedLevel()
        {
            if (levels.Count > 0)
                return levels[selectedLevel];
            return null;
        }

        internal static void SaveTileAssets(System.Type t)
        {
            
            foreach (var asset in tilesToRemove)
            {
                var p = AssetDatabase.GetAssetPath(asset);
                AssetDatabase.DeleteAsset(p);
            }

            if (!tiles.ContainsKey(t))
            {
                return;
            }

            StringBuilder sb = new StringBuilder(TilesPath, 50);
            foreach (var asset in tiles[t])
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

        internal static void ResetGrid(LevelGrid currentLevel)
        {
            currentLevel.tiles = new LevelGrid.Tile[currentLevel.width * currentLevel.height];
            LevelGrid.Tile val = new LevelGrid.Tile();
            val.SetMain (TileMap.BasicTileType.Random);
            val.SetBlocked (TileMap.BlockedTileType.Unblocked);
            val.SetBackground (TileMap.BackgroundTileType.NoBackground);

            for (int i = 0; i < currentLevel.width * currentLevel.height; i++)
            {
                currentLevel.tiles[i] = val;
            }
            levelsDirty = true;
        }

        internal static void DeleteTileTypeInctance(System.Type t)
        {
            if (!tiles.ContainsKey(t) || !selectedTile.ContainsKey(t))
            {
                return;
            }
            if (tiles[t].Count > 0)
            {
                var p = AssetDatabase.GetAssetPath(tiles[t][selectedTile[t]]);
                if (p != "")
                {
                    tilesToRemove.Add(tiles[t][selectedTile[t]]);
                }
                tiles[t].RemoveAt(selectedTile[t]);
            }
            if (selectedTile[t] >= tiles[t].Count)
            {
                selectedTile[t]--;
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
            tiles = new Dictionary<System.Type, List<TileType>>();
            selectedTile = new Dictionary<System.Type, int>();
            gizmos = new Dictionary<System.Enum, TileType>();

            for (int i = 0; i < elements.Length; i++)
            {
                TileType asset = AssetDatabase.LoadAssetAtPath<TileType>(elements[i]);
                if (asset == null)
                {
                    continue;
                }
                if (!tiles.ContainsKey(asset.GetType()))
                {
                    Debug.Log("Add type " + asset.GetType());
                    tiles[asset.GetType()] = new List<TileType>();
                    selectedTile[asset.GetType()] = -1;
                }
                Debug.Log("Load: " + asset.ToString());
                asset.name = Path.GetFileNameWithoutExtension(elements[i]);
                tiles[asset.GetType()].Add(asset);
                var id = asset.GetId();
                if (id != null)
                {
                    gizmos[id] = asset;
                }
            }
        }

        internal static void RebuildGrid(int oldWidth, int oldHeight, LevelGrid currentLevel)
        {
            if (currentLevel.tiles == null)
            {
                currentLevel.tiles = new LevelGrid.Tile[currentLevel.width * currentLevel.height];
                for (int i = 0; i < currentLevel.width * currentLevel.height; i++)
                {
                    currentLevel.tiles[i].SetMain (TileMap.BasicTileType.Random);
                }
                levelsDirty = true;
                return;
            }

            var newOne = new LevelGrid.Tile[currentLevel.width * currentLevel.height];
            for (byte x = 0; x < currentLevel.width; ++x)
                for (byte y = 0; y < currentLevel.height; ++y)
                {
                    int index = x + oldWidth * y;
                    int current = x + currentLevel.width * y;
                    if (x < oldWidth && y < oldHeight && index < currentLevel.tiles.Length)
                    {
                        newOne[current] = currentLevel.tiles[index];
                    }
                    else
                    {
                        newOne[current].SetMain (TileMap.BasicTileType.Random);
                    }
                }
            currentLevel.tiles = newOne;
            levelsDirty = true;
        }
    }
}