using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ResearchTreeTemp : MonoBehaviour
{
    protected CheeseTypes type;
    protected int cheese_amount;
    protected int scrap_cost;

    protected float prodution_time;

    protected bool is_researched;

    public CheeseTypes Type { get { return type; } }
    public int Cheese_amount { get { return cheese_amount; } }
    public int Scrap_cost { get { return scrap_cost; } }
    public float Production_time {  get { return prodution_time; } }
    public bool Is_researched { get { return is_researched; } }

    public void IsResearched()
    {
        is_researched = true;
    }

    public ResearchTreeTemp()
    {
        type = CheeseTypes.AmericanCheese;
        cheese_amount = 0;
        scrap_cost = 0;
        prodution_time = 5f;
        is_researched = false;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
