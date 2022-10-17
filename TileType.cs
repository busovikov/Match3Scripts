using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


public class TileType : ScriptableObject
{
    public const string GizmoPath = "Assets/Gizmos/";

    public Texture2D gizmo;
    public Texture2D image;
    public Texture2D destructionImage;
    public bool dirty;

    public virtual System.Enum GetId() { return null; }
    public virtual bool SetId(System.Enum e) { return false; }

    public bool SetGizmo(Texture2D texture)
    {
        if (gizmo != texture)
        {
            gizmo = texture;
            SaveGizmo();
            return true;
        }
        return false;
    }
    public bool SetDestructionImage(Texture2D texture)
    {
        if (destructionImage != texture)
        {
            destructionImage = texture;
            return true;
        }
        return false;
    }
    public bool SetImage(Texture2D texture)
    {
        if (image != texture)
        {
            image = texture;
            return true;
        }
        return false;
    }
    private void SaveGizmo()
    {
        if (gizmo == null)
            return;
        Texture2D texture = new Texture2D(gizmo.width, gizmo.height, TextureFormat.RGBA32, false);
        texture.SetPixels(gizmo.GetPixels());
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
