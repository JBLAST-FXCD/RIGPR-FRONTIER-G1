using System;
using UnityEngine;

public class MouseTemp : MonoBehaviour
{
    protected string mouse_id;

    public string GetMouseID()
    {
        return mouse_id;
    }

    // Start is called before the first frame update
    void Start()
    {
        mouse_id = System.Guid.NewGuid().ToString("N");
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
