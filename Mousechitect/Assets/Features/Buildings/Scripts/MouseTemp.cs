using System;
using UnityEngine;

public class MouseTemp : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        GameObject building = GameObject.FindGameObjectWithTag("BuildingTest");
        Vector3 building_loc = building.transform.localPosition;
        Vector3 cube_loc = this.transform.localPosition;

        Vector3 Diffrence = building_loc - cube_loc;
        Vector3 New_loc = Diffrence.normalized * Time.deltaTime * 2 + cube_loc;
        this.transform.position = New_loc;
    }
}
