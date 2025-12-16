using UnityEngine;

public enum ObjectColor { GREEN, RED }
public static class Operations
{
    public static ObjectColor SetObjectColor(ObjectColor color, Renderer rend)
    {
        Color rgbColor = (int) color switch {
            0 => new Color(0,1f,0f),
            1 => new Color(1f, 0,0),
            _ => new Color(0,1f,0f)
        };
        rend.material.color = rgbColor;
        return color;
    }
    
}
