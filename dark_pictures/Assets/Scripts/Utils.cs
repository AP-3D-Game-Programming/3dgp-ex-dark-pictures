using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ObjectColor { GREEN, RED }
public static class Utils
{
    public static ObjectColor SetObjectColor(ObjectColor color, Renderer rend)
    {
        Color rgbColor = (int)color switch
        {
            0 => new Color(0, 1f, 0f),
            1 => new Color(1f, 0, 0),
            _ => new Color(0, 1f, 0f)
        };
        rend.material.color = rgbColor;
        rend.material.SetColor("_EmissionColor", rgbColor);
        return color;
    }
    public static void Print<TKey, TValue>(Dictionary<TKey, TValue> dict)
    {
        foreach (var (key, value) in dict)
        {
            Debug.Log($"{key}: {value}");
        }
    }
    public static void PrintTags(List<GameObject> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            Debug.Log($"{i}: {list[i].tag}");
        }
    }
}
