using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommercialBuilding : ParentBuilding
{
    //Total amout of cheese types
    protected int cheese_types;

    protected float[] cheese_popularity;

    protected int mini;

    CommercialBuilding() 
    {
        cheese_types = 7;
        cheese_popularity = new float[cheese_types];
        mini = 5;
    }

    // Start is called before the first frame update
    void Start()
    {
        float temp = 0;
        PopularityAlgorithm();
        for (int i = 0; i < cheese_popularity.Length; i++)
            Debug.Log(cheese_popularity[i]);
        for (int j = 0; j < cheese_popularity.Length; j++)
            temp += cheese_popularity[j];
        Debug.Log(temp);
    }

    protected void PopularityAlgorithm()
    {
        float remaining_percent = 100.0f;
        int remaining_cheese = 0;
        int index = 0;

        for (int i = 0; i <= cheese_popularity.Length - 1; i++)
        {
            if (remaining_percent > mini * remaining_cheese)
            {
                cheese_popularity[i] = UnityEngine.Random.Range(mini, (int)remaining_percent);

                remaining_percent -= cheese_popularity[i];
                remaining_cheese = cheese_popularity.Length - 1 - i;
                index++;
            }
            else
                break;
        }

        //difrentce between percent needed and remaining
        float difrentce = (remaining_cheese * mini) - remaining_percent;
        //angle equals difrence divided by cheese not remaining
        float theta = difrentce / (cheese_types - remaining_cheese);

        //remove percent off cheese not remaining
        for (int j = 0; j < index; j++)
        {
            if (cheese_popularity[j] != 0)
            {
                cheese_popularity[j] -= theta;
                remaining_percent += theta;
            }
        }

        theta = remaining_percent / remaining_cheese;

        //remaing cheese equal 5 percent
        for (int k = index; k <= cheese_popularity.Length - 1; k++)
        {
            cheese_popularity[k] += theta;
        }
    }
}
