using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoint
{
    public bool isExplored;
    public Waypoint exploredFrom;

    private Vector2Int gridPos;

    public Vector2Int GridPos
    {
        get { return gridPos; }
        set { gridPos = value; }
    }

    public Waypoint(bool b, Waypoint w, Vector2Int v)
    {
        isExplored = b;
        exploredFrom = w;
        gridPos = v;
    }
}
