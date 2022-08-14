using System;
using System.IO;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TileMap))]
public class LevelEditor : Editor
{
    LevelGrid levelGrid;

    private void OnEnable()
    {
        TileMap tileMap = target as TileMap;

        if (tileMap.levelGrid == null)
            return;

        if (tileMap.levelGrid.tiles == null || tileMap.levelGrid.width * tileMap.levelGrid.height != tileMap.levelGrid.tiles.Length)
        {
            tileMap.levelGrid.tiles = new int[tileMap.levelGrid.width * tileMap.levelGrid.height];
            for (Byte x = 0; x < tileMap.levelGrid.width; ++x)
                for (Byte y = 0; y < tileMap.levelGrid.height; ++y)
                    tileMap.levelGrid.tiles[x + tileMap.levelGrid.width * y] = -1;
        }
       
        
    }

    
    private void OnSceneGUI()
    {
        Handles.BeginGUI();
        {
            GUIStyle style = new GUIStyle("box");
            GUILayout.BeginArea(new Rect(10, 10, 200, 40), style);
            {


            }
            GUILayout.EndArea();
        }
        Handles.EndGUI();

        Event e = Event.current;

        if (e.type == EventType.Layout)
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(GetHashCode(), FocusType.Passive));

        bool control = e.modifiers == EventModifiers.Control;


        TileMap tileMap = target as TileMap;


        if (tileMap.levelGrid == null)
            return;


        Vector3 mousePosition = e.mousePosition;
        Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
        mousePosition = ray.origin;

        bool down = (e.type == EventType.MouseDrag || e.type == EventType.MouseDown);
        bool downRight = (e.type == EventType.MouseDrag || e.type == EventType.MouseDown) && e.button == 1;

        for (Byte x = 0; x < tileMap.levelGrid.width; ++x)
            for (Byte y = 0; y < tileMap.levelGrid.height; ++y)
            {
                Vector3 pos = new Vector3(x, y) + tileMap.transform.position - Vector3.one * 0.5f;
                Rect rect = new Rect(pos, Vector2.one);
                bool contain = rect.Contains(mousePosition);
                if (contain && down)
                {

                    tileMap.levelGrid.tiles[x + tileMap.levelGrid.width * y] = !control ? 1 : -1;
                    //e.Use();
                }
                Color tileColor = tileMap.levelGrid.tiles[x + tileMap.levelGrid.width * y] == -1 ? Color.gray : new Color(0.1176471f, 0.1019608f, .1960784f, .7843137f);
                Color color = contain ? Color.green : tileColor;
              
                Handles.DrawSolidRectangleWithOutline(rect, color, Color.white);
            }
    }

    
}
