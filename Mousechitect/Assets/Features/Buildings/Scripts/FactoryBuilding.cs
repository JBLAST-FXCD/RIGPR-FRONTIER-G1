using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactoryBuilding : ParentBuilding
{
    //first element is for rarity and second element is for cheese type.
    [SerializeField] protected CheeseTemp[,] cheesetypes;

    protected int selectedcheese;
    protected CheeseTemp cheesetype;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    //for player to select cheese
    protected void SelectCheese(int input) 
    {
        cheesetype = cheesetypes[tier - 1, input];
    }
}
