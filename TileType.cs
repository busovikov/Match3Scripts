using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


public class TileType : ScriptableObject
{
    public const string GizmoPath = "Assets/Gizmos/";

    public Texture2D image;
    public bool dirty;

    public virtual System.Enum GetId() { return null; }
    public virtual bool SetId(System.Enum e) { return false; }

    public bool SetImage(Texture2D texture)
    {
        if (image != texture)
        {
            image = texture;
            SaveGizmo();
            return true;
        }
        return false;
    }
    private void SaveGizmo()
    {
        if (image == null)
            return;
        Texture2D texture = new Texture2D(image.width, image.height, TextureFormat.RGBA32, false);
        texture.SetPixels(image.GetPixels());
        texture.Apply();
        
        byte[] bytes = texture.EncodeToPNG();
        
        if (!Directory.Exists(GizmoPath))
        {
            Directory.CreateDirectory(GizmoPath);
        }
        File.WriteAllBytes(GizmoPath + name + ".png", bytes);
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }
}
