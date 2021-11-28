using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Config
{
    private static readonly string Ver = "Ver.1.0.";
    static StringBuilder builder = new StringBuilder();

    static string Name(string name)
    {
        string str = builder.Append(Ver).Append(name).ToString();
        builder.Clear();
        return str;
    }
    static public void LoadInt(string name, out int val, int def)
    {
        string str = Name(name);
        if (PlayerPrefs.HasKey(str))
        {
            val = PlayerPrefs.GetInt(str);
        }
        else
        {
            val = def;
        }
    }

    static public void LoadBool(string name, out bool val, bool def)
    {
        string str = Name(name);
        if (PlayerPrefs.HasKey(str))
        {
            val = Convert.ToBoolean(PlayerPrefs.GetInt(str));
        }
        else
        {
            val = def;
        }
    }

    static public void LoadFloat(string name, out float val, float def)
    {
        string str = Name(name);
        if (PlayerPrefs.HasKey(str))
        {
            val = PlayerPrefs.GetFloat(str);
        }
        else
        {
            val = def;
        }
    }

    static public void SaveInt(string name, int val)
    {
        PlayerPrefs.SetInt(Name(name), val);
    }

    static public void SaveBool(string name, bool val)
    {
        PlayerPrefs.SetInt(Name(name), Convert.ToInt32(val));
    }

    static public void SaveFloat(string name, float val)
    {
        PlayerPrefs.SetFloat(Name(name), val);
    }
}
