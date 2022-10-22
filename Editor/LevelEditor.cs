using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.ShortcutManagement;
using Unity.Mathematics;
using System.IO;
using System.Linq;
using UnityEditor.Overlays;
using UnityEngine.UIElements;
using Assets.Scripts.Editor;
using System;

[CustomEditor(typeof(TileMap)), CanEditMultipleObjects]
public class LevelEditor : Editor
{

    public override void OnInspectorGUI()
    {
        GUILayout.Label("Tile Options");
        DrawTileEditor();
        GUILayout.Space(5f);
        GUILayout.Label("Level Options");
        DrawLevelEditor();
        GUILayout.Space(5f);
        DrawLevelsPanel();
        GUILayout.Space(5f);
        DrawTileSetPanel();
    }


    private void DrawTileEditor()
    {
        TileType currentTile = (target as TileMap).currentTile;

        if (currentTile != null)
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                var oldName = currentTile.name;
                currentTile.name = GUILayout.TextField(currentTile.name);

                var id = currentTile.GetId();
                bool IdChanged = currentTile.SetId(EditorGUILayout.EnumPopup("Main Type:", id != null ? id : TileMap.BasicTileType.None ));

                var gizmoChanged = currentTile.SetGizmo((Texture2D)EditorGUILayout.ObjectField("Gizmo (Editor)", currentTile.gizmo, typeof(Texture2D), false));

                var imageChanged = currentTile.SetImage((Texture2D)EditorGUILayout.ObjectField("Image", currentTile.image, typeof(Texture2D), false));
                LevelEditorHelper.gizmos[currentTile.GetId()] = currentTile;
                
                var destructionImageChanged = currentTile.SetDestructionImage((Texture2D)EditorGUILayout.ObjectField("Destruction Image", currentTile.destructionImage, typeof(Texture2D), false));

                if (oldName != currentTile.name || imageChanged || IdChanged || destructionImageChanged || gizmoChanged)
                    LevelEditorHelper.SetTileDirty(currentTile.GetType());
            }
        }
    }

    private void DrawLevelEditor()
    {
        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
        {
            TileMap tm = target as TileMap;
            
            if (tm.currentLevel == null)
                return;

            bool nameChanged = tm.currentLevel.SetName(GUILayout.TextField(tm.currentLevel.name));
            bool widthChanged = tm.currentLevel.SetWidth((byte)EditorGUILayout.IntField("Width", tm.currentLevel.width));
            bool heightChanged = tm.currentLevel.SetHeight((byte)EditorGUILayout.IntField("Height", tm.currentLevel.height));


            if (nameChanged || widthChanged || heightChanged)
            {
                tm.UpdateSize();
                LevelEditorHelper.levelsDirty = true;
            }
        }
    }

    private void DrawLevelsPanel()
    {
        var tm = target as TileMap;
        using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
        {
            if (GUILayout.Button("Make Default"))
            {
                tm.levelGrid = tm.currentLevel;
            }

            if (GUILayout.Button("Reset"))
            {
                LevelEditorHelper.ResetGrid(tm.currentLevel);
            }
        }

        using (new GUILayout.HorizontalScope())
        {
            if (LevelEditorHelper.levels.Count > 0 && GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20)))
            {
                DeleteLevelGridInctance();
            }
            if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20)))
            {
                AddLevelGridInctance();
            }
            if (LevelEditorHelper.levelsDirty && GUILayout.Button("Save", GUILayout.Width(50), GUILayout.Height(20)))
            {
                SaveLevelGridAssets();
            }
        }

        var levelList = LevelEditorHelper.levels.Select(l => 
        {
            if (tm.levelGrid != null && tm.levelGrid.name == l.name)
                return "" + l.name + " = default";

            return l.name; 
        
        }).ToArray();
        LevelEditorHelper.levelScrollPosition = GUILayout.BeginScrollView(LevelEditorHelper.levelScrollPosition, GUILayout.MinHeight(100));
        int level = GUILayout.SelectionGrid(LevelEditorHelper.selectedLevel, levelList, 1, EditorStyles.toolbarButton, GUILayout.Height(100));
        
        if (LevelEditorHelper.levels.Count > 0 && (level != LevelEditorHelper.selectedLevel || tm.currentLevel == null))
        {
            LevelEditorHelper.selectedLevel = level;
            tm.currentLevel = LevelEditorHelper.GetSelectedLevel();
            tm.UpdateSize();
        }
        GUILayout.EndScrollView();
    }

    private void DrawTileSetPanel()
    {
        var tileset = LevelEditorHelper.GetSelectedTileSet();
        LevelEditorHelper.setsDirty |= tileset.SetName(GUILayout.TextField(tileset.name));

        if (GUILayout.Button("Save All Tiles to Set"))
        {
            LevelEditorHelper.SaveAllTilesToSet();
        }

        using (new GUILayout.HorizontalScope())
        {
            if (LevelEditorHelper.tilesSets.Count > 0 && GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20)))
            {
                LevelEditorHelper.DeleteTileSetInctance();
            }
            if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20)))
            {
                LevelEditorHelper.AddTileSetInctance();
            }
            if (LevelEditorHelper.setsDirty && GUILayout.Button("Save", GUILayout.Width(50), GUILayout.Height(20)))
            {
                LevelEditorHelper.SaveTileSetAssets();
            }
        }

        var setList = LevelEditorHelper.tilesSets.Select(s =>
        {
            int count = s.tiles == null ? 0 : s.tiles.Length;
            return "" + s.name + " (" + count.ToString() + ")";
        }).ToArray();

        LevelEditorHelper.tileSetScrollPosition = GUILayout.BeginScrollView(LevelEditorHelper.tileSetScrollPosition, GUILayout.MinHeight(100));
        int set = GUILayout.SelectionGrid(LevelEditorHelper.selectedTileSet, setList, 1, EditorStyles.toolbarButton, GUILayout.Height(100));

        if (LevelEditorHelper.tilesSets.Count > 0 && set != LevelEditorHelper.selectedTileSet)
        {
            LevelEditorHelper.selectedTileSet = set;
        }
        GUILayout.EndScrollView();
    }

    private void OnEnable()
    {
        LevelEditorHelper.Init(target as TileMap);
    }

    private void AddLevelGridInctance()
    {
        LevelEditorHelper.AddLevelGridInctance((target as TileMap).currentLevel);
    }

    private void SaveLevelGridAssets()
    {
        LevelEditorHelper.SaveLevelGridAssets();
    }
    private void DeleteLevelGridInctance()
    {
        LevelEditorHelper.DeleteLevelGridInctance();
    }
}
[EditorTool("Level Editor Tool", typeof(TileMap))]
public class LevelEditorTool : EditorTool, IDrawSelectedHandles
{
    Vector3 fieldPosition;
    int tileType;

    Texture2D Resize(Texture2D source, int newWidth, int newHeight)
    {
        source.filterMode = FilterMode.Point;
        RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
        rt.filterMode = FilterMode.Point;
        RenderTexture.active = rt;
        Graphics.Blit(source, rt);
        Texture2D nTex = new Texture2D(newWidth, newHeight);
        nTex.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
        nTex.Apply();
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);
        return nTex;
    }

    public void OnDrawHandles()
    {
       
    }

    // The second "context" argument accepts an EditorWindow type.
    [Shortcut("Activate TileMap Tool", typeof(SceneView), KeyCode.G)]
    static void PlatformToolShortcut()
    {
        if (Selection.GetFiltered<TileMap>(SelectionMode.TopLevel).Length > 0)
            ToolManager.SetActiveTool<LevelEditorTool>();
        else
            Debug.Log("No platforms selected!");
    }

    // Called when the active tool is set to this tool instance. Global tools are persisted by the ToolManager,
    // so usually you would use OnEnable and OnDisable to manage native resources, and OnActivated/OnWillBeDeactivated
    // to set up state. See also `EditorTools.{ activeToolChanged, activeToolChanged }` events.
    public override void OnActivated()
    {
        //if (Event.current.type == EventType.Layout)
        //    HandleUtility.AddDefaultControl(GUIUtility.GetControlID(GetHashCode(), FocusType.Passive));
    }

    // Called before the active tool is changed, or destroyed. The exception to this rule is if you have manually
    // destroyed this tool (ex, calling `Destroy(this)` will skip the OnWillBeDeactivated invocation).
    public override void OnWillBeDeactivated()
    {
        //if (Event.current.type == EventType.Layout)
        //    HandleUtility.AddDefaultControl(GUIUtility.GetControlID(GetHashCode(), FocusType.Keyboard));
    }

    private void OnEnable()
    {
        LevelEditorHelper.Init(target as TileMap);
    }

    private void OnDisable()
    {
        LevelEditorHelper.Reset();
    }

    public override void OnToolGUI(EditorWindow window)
    {
        if (!(window is SceneView sceneView))
            return;
        
        if (!ToolManager.IsActiveTool(this))
            return;

        TileMap tileMap = target as TileMap;
        Vector2 position = (Vector2)tileMap.transform.position - tileMap.offset - Vector2.one * .5f;

        Handles.BeginGUI();
        GUILayout.FlexibleSpace();
        using (new GUILayout.HorizontalScope())
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                DrawTilePanel();
            }
            GUILayout.FlexibleSpace();
        }
        Handles.EndGUI();



        int controlID = GUIUtility.GetControlID(FocusType.Passive);
        //Debug.Log("Control id for: " + i + " = " + controlID);


        Vector3 mousePosition = Event.current.mousePosition;
        Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
        mousePosition = ray.origin;
            
        //Debug.Log(new Rect(position.x, position.y, tileMap.currentLevel.width, tileMap.currentLevel.height));
        //Debug.Log(mousePosition);

        bool controlPressed = Event.current.modifiers == EventModifiers.Control;

        switch (Event.current.GetTypeForControl(controlID))
        {
            case EventType.Layout:
                if (tileMap.currentLevel == null)
                    break;
                Rect rect = new Rect(position.x, position.y, tileMap.currentLevel.width, tileMap.currentLevel.height);
                var distance = (float)PointRectDist(mousePosition, rect);
                if (rect.Contains(mousePosition))
                {
                    HandleUtility.AddControl(controlID, distance);
                }
                break;

            case EventType.MouseUp:
                if (GUIUtility.hotControl == controlID)
                {
                    GUIUtility.hotControl = 0;
                    Event.current.Use();
                }
                break;
            case EventType.MouseDown:

                if (HandleUtility.nearestControl == controlID)
                {
                    GUIUtility.hotControl = controlID;
                    CheckTile(mousePosition, tileMap, controlPressed);
                    GUI.changed = true;
                    Event.current.Use();
                }
                break;
            case EventType.MouseDrag:

                if (GUIUtility.hotControl == controlID)
                {
                    CheckTile(mousePosition, tileMap, controlPressed);
                    GUI.changed = true;
                    Event.current.Use();
                }
                break;
        }
    }

    
    private void DrawTilePanel()
    {
        foreach (var type in LevelEditorHelper.tiles.Keys)
        {
            using (new GUILayout.VerticalScope())
            {
                GUILayout.Label(type.ToString());
                using (new GUILayout.HorizontalScope())
                {
                    bool changed = LevelEditorHelper.SetSelectedTile(type, GUILayout.SelectionGrid(LevelEditorHelper.selectedTile[type], LevelEditorHelper.tiles[type].Select(l => l.gizmo).ToArray(), 5, EditorStyles.toolbarButton, GUILayout.Width(200)));
                    if (changed)
                    {
                        (target as TileMap).currentTile = LevelEditorHelper.GetSelectedTile(type);
                    }
                }
                using (new GUILayout.HorizontalScope())
                {
                    if (LevelEditorHelper.tiles.Count > 0 && GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20)))
                    {
                        DeleteTileTypeInctance(type);
                    }
                    if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20)))
                    {
                        AddTileTypeInctance(type);

                        (target as TileMap).currentTile = LevelEditorHelper.GetSelectedTile(type);
                    }
                    if (LevelEditorHelper.IsTileDirty(type) && GUILayout.Button("Save", GUILayout.Width(50), GUILayout.Height(20)))
                    {
                        SaveTileAssets(type);
                    }
                }
                //GUILayout.FlexibleSpace();
            }
        }
    }

    private void AddTileTypeInctance(System.Type t)
    {
        LevelEditorHelper.AddNewTile(t);
    }

    private void SaveTileAssets(System.Type t)
    {
        LevelEditorHelper.SaveTileAssets(t);
    }
    private void DeleteTileTypeInctance(System.Type t)
    {
        LevelEditorHelper.DeleteTileTypeInctance(t);
    }



    private void LoadTiles()
    {
        LevelEditorHelper.LoadTiles();
    }

    private void OnLevelPathChanged(object sender, FileSystemEventArgs e)
    {
    }

    private void OnTilePathChanged(object sender, FileSystemEventArgs e)
    {
        LoadTiles();
    }

    private void CheckTile(Vector2 mousePosition, TileMap tileMap, bool erase = false)
    {
        if (tileMap.currentLevel == null)
            return;

        Vector2 pos = mousePosition - (Vector2)tileMap.transform.position + tileMap.offset + Vector2.one * .5f;
        int x = (int)(pos.x);
        int y = (int)(pos.y);
        var tile = tileMap.currentTile;
        if (tile != null)
        {
            if (erase)
            {
                tileMap.currentLevel.SetZero(x, y, tile);
            }
            else
            {
                tileMap.currentLevel.Set(x, y, tile);
            }
        }

        LevelEditorHelper.levelsDirty = true;
    }
    private double PointRectDist(Vector3 p, Rect rect)
    {
        var cx = math.max(math.min(p.x, rect.x + rect.width), rect.x);
        var cy = math.max(math.min(p.y, rect.y + rect.height), rect.y);
        return math.sqrt((p.x - cx) * (p.x - cx) + (p.y - cy) * (p.y - cy));
    }
}
