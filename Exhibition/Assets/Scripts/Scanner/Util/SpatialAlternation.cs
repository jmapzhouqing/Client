using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpatialAlternation
{
    public static Vector2 Vector2Rotate(Vector2 center,Vector2 vertice, float rotation){
        vertice = vertice - center;
        Vector2 transform = new Vector2(vertice.x * Mathf.Cos(rotation) + vertice.y * Mathf.Sin(rotation), vertice.y * Mathf.Cos(rotation) - vertice.x * Mathf.Sin(rotation));
        return (center + transform);
    }
}
