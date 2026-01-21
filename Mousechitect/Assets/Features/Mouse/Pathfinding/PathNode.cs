using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Iain Benner 21/01/2026

/// <summary>
/// Stores data for pathfinding and speed from the grid manager.
/// </summary>
public class PathNode
{
    protected Vector2Int postion;
    protected float speed;
    protected int cost;
    protected int total_cost;
    protected bool searched;
    protected PathNode previous_node;

    public Vector2Int Postion { get { return postion; } set { postion = value; } }
    public float Speed { get { return speed; } }
    public int Cost { get { return cost; } set { cost = value; } }
    public int Total_cost { get { return total_cost; } set { total_cost = value; } }
    public bool Searched { get { return searched; } set { searched = value; } }
    public PathNode Previous_node { get { return previous_node; } set { previous_node = value; } }

    public PathNode(Vector2Int postion, GridManager grid_manager)
    {
        this.postion = postion;

        if (grid_manager.GetCellMoveSpeed(postion) > 0)
        {
            this.speed = grid_manager.GetCellMoveSpeed(postion);
            this.searched = false;
        }
        else
        {
            this.searched = true;
        }

        this.cost = (int)(1 / speed * 100.0f);
        this.total_cost = int.MaxValue;
        this.previous_node = null;
    } 
}