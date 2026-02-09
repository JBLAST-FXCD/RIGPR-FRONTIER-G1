using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Iain Benner 21/01/2026

/// <summary>
/// Stores data for pathfinding and speed from the grid manager.
/// Base node is ISaveable and is used as pathfinding return because speed can be checked against current grid to check for grid changes when loading route.
/// </summary>

[System.Serializable]
public struct BaseNode
{
    public Vector2Int postion;
    public float speed;

    public BaseNode(Vector2Int postion, float speed)
    {
        this.postion = postion;
        this.speed = speed;
    }
}

public class PathNode
{
    protected BaseNode node;
    protected int cost;
    protected int total_cost;
    protected bool searched;
    protected PathNode previous_node;

    public Vector2Int Postion { get { return node.postion; } set { node.postion = value; } }
    public float Speed { get { return node.speed; } set { node.speed = value; } }
    public int Cost { get { return cost; } set { cost = value; } }
    public int Total_cost { get { return total_cost; } set { total_cost = value; } }
    public bool Searched { get { return searched; } set { searched = value; } }
    public PathNode Previous_node { get { return previous_node; } set { previous_node = value; } }

    public PathNode(Vector2Int postion, GridManager grid_manager)
    {
        float cell_speed = grid_manager.GetCellMoveSpeed(postion);

        this.node.postion = postion;
        this.searched = cell_speed > 0 ? false : true;
        this.node.speed = cell_speed;
        this.cost = (int)(1 / node.speed * 100.0f);
        this.total_cost = int.MaxValue;
        this.previous_node = null;
    } 
}