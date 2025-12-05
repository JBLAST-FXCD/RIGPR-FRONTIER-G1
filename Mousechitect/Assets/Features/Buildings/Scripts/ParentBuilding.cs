using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class parent_building : MonoBehaviour
{
    [SerializeField] private GameObject[] building_prefabs;
    [SerializeField] private int[]        capacitys;
    [SerializeField] private int          tier;

    private GameObject building_prefab;
    private List<MouseTemp> Mouse;
    private int capacity;

    // Start is called before the first frame update
    void Start()
    {
        ConstructTier(tier);
        building_prefab.transform.localPosition = this.transform.localPosition;
        Instantiate(building_prefab);
    }

    // Pick and the diffrenmt mesh and stats for the teirs
    private void ConstructTier(int tier) 
    {
        tier--;
        building_prefab = building_prefabs[tier];
        capacity = capacitys[tier];
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Hit");
        if (other != null && other.tag == "MouseTemp" && Mouse.Count <= capacity)
        {
            other.transform.gameObject.SetActive(false);
            //Mouse.Add(other);
        }
    }

    private void MouseLeave(MouseTemp m1)
    {
        Vector3 new_loc = m1.transform.localPosition;

        switch (this.transform.eulerAngles.y)
        {
            case 0:
                new_loc.z += 1;
                break;
            case 90:
                new_loc.x += 1;
                break;
            case 180:
                new_loc.z -= 1;
                break;
            case 270:
                new_loc.x -= 1;
                break;
        }

        m1.transform.localPosition = new_loc;
        m1.transform.gameObject.SetActive(true);
    }
}