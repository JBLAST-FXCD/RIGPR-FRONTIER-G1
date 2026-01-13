using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNode
{
    protected Vector2Int postion;
    protected int cost;

    public Vector2Int Postion { get { return postion; } }
    public int Cost { get { return cost; } }

    public PathNode(Vector2Int postion, GridManager grid_manager)
    {
        this.postion = postion;

        this.cost    = (int)(1 / grid_manager.GetCellMoveSpeed(postion) * 100.0f);
    }
}