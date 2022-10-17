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

            var oldName = tm.currentLevel.name;
            tm.currentLevel.name = GUILayout.TextField(tm.currentLevel.name);
            var oldWidth = tm.currentLevel.width;
            tm.currentLevel.width = (byte)math.clamp(math.round(EditorGUILayout.FloatField("Width", tm.currentLevel.width)), LevelEditorHelper.widthMin, LevelEditorHelper.widthMax);
            var oldHeight = tm.currentLevel.height;
            tm.currentLevel.height = (byte)math.clamp(math.round(EditorGUILayout.FloatField("Height", tm.currentLevel.height)), LevelEditorHelper.heightMin, LevelEditorHelper.heightMax);

            var sizeChanged = tm.currentLevel.tiles != null && tm.currentLevel.tiles.Length != tm.currentLevel.width * tm.currentLevel.height;
            if (oldName != tm.currentLevel.name || sizeChanged)
                LevelEditorHelper.levelsDirty = true;

            if (sizeChanged)
            {
                RebuidGrid(oldWidth, oldHeight);
            }
        }
    }

    private void RebuidGrid(int oldWidth, int oldHeight)
    {
        LevelEditorHelper.RebuildGrid(oldWidth, oldHeight, (target as TileMap).currentLevel);
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
        this.LoadLevels();
        this.LoadTileSets();
        (target as TileMap).currentLevel = LevelEditorHelper.GetSelectedLevel();
        (target as TileMap).gizmos = LevelEditorHelper.gizmos;
    }

    private void LoadTileSets()
    {
        LevelEditorHelper.LoadTileSets();
    }

    private void LoadLevels()
    {
        LevelEditorHelper.LoadLevels();
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
        /*TileMap tileMap = target as TileMap;
        var position = tileMap.transform.position - Vector3.one * 0.5f;
        if (tileMap.currentLevel != null)
        {
            if (!tileMap.currentLevel.Valid())
                return;
            
            Rect rect = new Rect(position.x, position.y, tileMap.currentLevel.width, tileMap.currentLevel.height);
            Handles.DrawSolidRectangleWithOutline(rect, new Color(0.5f, 0.5f, 0.5f, .7843137f), Color.white);

            for (byte x = 0; x < tileMap.currentLevel.width; ++x)
                for (byte y = 0; y < tileMap.currentLevel.height; ++y)
                {
                    int index = x + tileMap.currentLevel.width * y;
                    Vector3 pos = new Vector3(x, y, x*y) + tileMap.transform.position - Vector3.one * 0.5f;
                    var type = tileMap.currentLevel.tiles[index];
                    //Color tileColor = new Color(type, type, type, .7843137f);
                    if (type != TileMap.BasicTileType.None)
                    {
                        Rect tileRect = new Rect(pos, Vector2.one);
                        //Handles.DrawSolidRectangleWithOutline(tileRect, tileColor, Color.white);
                        //Handles.Label(pos, Resize(LevelEditorHelper.GetTileByType(type).image, 40, 40));// DrawTexture3DSDF(LevelEditorHelper.GetTileByType(type).image);
                        Gizmos.DrawGUITexture(new Rect(10, 10, 20, 20), LevelEditorHelper.GetTileByType(type).image);
                    }
                }
        }
        /*Event e = Event.current;

        new Color(0.1176471f, 0.1019608f, .1960784f, .7843137f)

        controlPressed = e.modifiers == EventModifiers.Control;

        Vector3 mousePosition = e.mousePosition;
        Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
        mousePosition = ray.origin;

        down = (e.type == EventType.MouseDrag || e.type == EventType.MouseDown);

        for (byte x = 0; x < tileMap.levelGrid.width; ++x)
            for (byte y = 0; y < tileMap.levelGrid.height; ++y)
            {
                Vector3 pos = new Vector3(x, y) + tileMap.transform.position - Vector3.one * 0.5f;
                Rect rect = new Rect(pos, Vector2.one);
                bool contain = ToolManager.IsActiveTool(this) && rect.Contains(mousePosition);
                if (contain && down)
                {
                    tileMap.levelGrid.tiles[x + tileMap.levelGrid.width * y] = !controlPressed ? 1 : -1;
                }
                Color tileColor = tileMap.levelGrid.tiles[x + tileMap.levelGrid.width * y] == -1 ? Color.gray : new Color(0.1176471f, 0.1019608f, .1960784f, .7843137f);
                Color color = contain ? Color.green : tileColor;

                //Handles.RectangleHandleCap(,pos,Quaternion.identity,1f,);
                //Handles.DrawSolidRectangleWithOutline(rect, color, Color.white);
            }
        */
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
        LoadTiles();
    }


    public override void OnToolGUI(EditorWindow window)
    {
        if (!(window is SceneView sceneView))
            return;
        
        if (!ToolManager.IsActiveTool(this))
            return;

        TileMap tileMap = target as TileMap;
        Vector3 position = tileMap.transform.position - Vector3.one * 0.5f;

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
                    Debug.Log("MouseDrag over " + controlID);
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

    private void CheckTile(Vector3 mousePosition, TileMap tileMap, bool erase = false)
    {
        if (tileMap.currentLevel == null)
            return;

        Vector2 pos = mousePosition - tileMap.transform.position;
        int x = (int)math.round(pos.x);
        int y = (int)math.round(pos.y);

        var tile = tileMap.currentTile;
        if (tile != null)
        {
            if (erase)
            {
                tileMap.currentLevel.SetZero(x, y);
            }
            else
            {
                tileMap.currentLevel.Set(x, y, tile);
            }
        }

        AssetDatabase.SaveAssetIfDirty(tileMap.currentLevel);
    }
    private double PointRectDist(Vector3 p, Rect rect)
    {
        var cx = math.max(math.min(p.x, rect.x + rect.width), rect.x);
        var cy = math.max(math.min(p.y, rect.y + rect.height), rect.y);
        return math.sqrt((p.x - cx) * (p.x - cx) + (p.y - cy) * (p.y - cy));
    }
}
