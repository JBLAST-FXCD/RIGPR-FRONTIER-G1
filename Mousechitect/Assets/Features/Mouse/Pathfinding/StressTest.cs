using System;
using UnityEngine;

public class StressTest : MonoBehaviour
{
    [SerializeField] protected MouseTemp mouse;

    ParentBuilding[] buildings;
    MouseTemp[] mouses;

    int occupants;

    StressTest()
    {
        occupants = 32;
    }

    protected void FirstWave()
    {
        buildings = FindObjectsOfType(typeof(ParentBuilding)) as ParentBuilding[];
        mouses = new MouseTemp[buildings.Length * occupants];

        int j = 0;
        for (int i = 0; i < mouses.Length; i++)
        {
            MouseTemp new_mouse = Instantiate(mouse, new Vector3(0.0f, 0.5f, 0.0f), mouse.transform.rotation);

            new_mouse.GetVectors(buildings[j]);

            mouses[i] = new_mouse;

            j++;
            if (j == buildings.Length)
                j = 0;
        }

        Invoke(nameof(SecondWave), 30);
    }

    protected void SecondWave(MouseTemp mouse, ParentBuilding building)
    {
        building.MouseLeave(mouse);
    }

    protected void SecondWave()
    {
        int j = 0;
        UnityEngine.Random.InitState((int)DateTime.Now.Ticks);
        for (int i = 0; i < mouses.Length; i++)
        {
            int rand = UnityEngine.Random.Range(0, buildings.Length);
            if (buildings[j].CheckOccupants(mouses[i]))
            {
                buildings[j].MouseLeave(mouses[i]);

                mouses[i].GetVectors(buildings[rand]);
            }

            j++;
            if (j == buildings.Length)
                j = 0;
        }

        Invoke(nameof(SecondWave), 30);
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Q))
        //{
        //    FirstWave();
        //}
    }
}
