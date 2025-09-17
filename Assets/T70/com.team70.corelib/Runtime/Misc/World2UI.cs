﻿using UnityEngine;

public class World2UI
{
    public static int ScreenWidth;
    public static int ScreenHeight;
    
    public static Vector2 ChangeWorld2UI(Camera camera, Vector3 position, RectTransform ui)
    {
        var screenPos = camera.WorldToScreenPoint(position);
        var result = Screen2UI(ui, screenPos);
        
        return result;
    }
    
    public static Vector2 Screen2UI(RectTransform ui, Vector2 screenPos)
    {
        var screenW = ScreenWidth;
        var screenH = ScreenHeight;

        var max = ui.anchorMax;
        var min = ui.anchorMin;

        var sx = min.x + (max.x - min.x) / 2;
        var sy = min.y + (max.y - min.y) / 2;

        return new Vector2(screenPos.x - screenW * sx, screenPos.y - screenH * sy);
    }
}