using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNode
{
    protected Vector2Int postion;
    protected float speed;
    protected int cost;
    protected int total_cost;
    protected bool searched;
    protected PathNode previous_node;


    public Vector2Int Postion { get { return postion; } set { postion = value; } }
    public int Cost { get { return cost; } set { cost = value; } }
    public int Total_cost { get { return total_cost; } set { total_cost = value; } }
    public bool Searched { get { return searched; } set { searched = value; } }
    public PathNode Previous_node { get { return previous_node; } set { previous_node = value; } }

    public PathNode(Vector2Int postion, GridManager grid_manager)
    {
        this.postion = postion;
        this.speed = grid_manager.GetCellMoveSpeed(postion);

        this.cost = (int)(1 / speed * 100.0f);
        this.total_cost = int.MaxValue;
        this.searched = false;
        this.previous_node = null;
    }
}