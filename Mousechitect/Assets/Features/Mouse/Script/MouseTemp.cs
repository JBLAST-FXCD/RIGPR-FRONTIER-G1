using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]

public class MouseTemp : MonoBehaviour
{
    protected string mouse_id;

    //Varibles for paths and LERP.
    protected List<BaseNode> path;
    protected bool moving; 

    //Grind manager is for checking calulated speed vs current speed.
    protected static GridManager grid_manager;

    protected ParentBuilding home;

    public string Mouse_id { get { return mouse_id; } }

    public Vector2Int Position { get { return new Vector2Int((int)this.transform.position.x, (int)this.transform.position.z); } }
    public bool Moving { get { return moving; } set { moving = value; } }
    public List<BaseNode> Path { get { return path; } set { path = value; } }
    public static GridManager Grid_manager { set { grid_manager = value; } }
    public ParentBuilding Home { get { return home; } set { home = value; } }
    public bool Collider { set { GetComponent<CapsuleCollider>().enabled = value; } }

    // updated by Anthony - 10/2/2026
    [Header("Per-mouse Morale")]
    [SerializeField, Range(0f, 1f)] private float mouse_morale = 0.5f;

    // How much the mouse morale can change per morale tick
    [SerializeField] private float morale_step_per_tick = 0.05f;

    [Header("Cheese Preferences")]
    [SerializeField] private CheeseTypes favourite_cheese;
    [SerializeField] private CheeseTypes least_favourite_cheese;

    [SerializeField] private float favourite_bonus = 0.10f;
    [SerializeField] private float least_penalty = 0.10f;

    private bool preferences_initialised = false;

    // Public read-only access for averaging in MoraleSystem
    public float MouseMorale { get { return mouse_morale; } }
    public CheeseTypes FavouriteCheese { get { return favourite_cheese; } }
    public CheeseTypes LeastFavouriteCheese { get { return least_favourite_cheese; } }

    public MouseTemp() 
    {
        moving = false;
    }

    protected int SetRotation(Vector3 current_loc, Vector3 new_loc)
    {
        if (current_loc.x > new_loc.x)
            return 90;
        else if (current_loc.x < new_loc.x)
            return 270;
        else if (current_loc.z < new_loc.z)
            return 180;
        else
            return 0;
    }

    public IEnumerator FollowPath(Action<bool> callback)
    {
        for (int i = 0; i < path.Count; i++)
        {
            Vector3 current_loc = this.transform.position;
            Vector3 new_loc = new Vector3(path[i].postion.x, -2.16f, path[i].postion.y);
            float speed = new PathNode(path[i].postion, grid_manager).Speed;

            this.transform.eulerAngles = new Vector3(0, SetRotation(current_loc, new_loc), 0);

            yield return new WaitForEndOfFrame();

            if (path[i].speed == speed)
            {
                if (speed == 0)
                    speed = 1;

                float time_elapsed = 0;

                while (this.transform.position != new_loc)
                {
                    time_elapsed += Time.deltaTime * speed;
                    this.transform.position = Vector3.Lerp(current_loc, new_loc, time_elapsed);
                    yield return new WaitForFixedUpdate();
                }
            }
            else
                break;
        }

        path = null;
        Collider = true;
        yield return new WaitForFixedUpdate();

        if (home == null)
            callback(false);
        else
            callback(true);
    }

    // Anthony - 10/2/2026 (Per-mouse morale)

    // Assigns random cheese preferences. Called once after spawn (or first morale tick).
    public void InitialisePreferencesIfNeeded()
    {
        if (preferences_initialised) return;

        CheeseTypes[] values = (CheeseTypes[])System.Enum.GetValues(typeof(CheeseTypes));

        favourite_cheese = values[UnityEngine.Random.Range(0, values.Length)];

        do
        {
            least_favourite_cheese = values[UnityEngine.Random.Range(0, values.Length)];
        }
        while (least_favourite_cheese == favourite_cheese);

        preferences_initialised = true;
    }

    // Updates this mouse's morale toward a target based on city baseline + cheese preference delta.
    public void UpdatePerMouseMorale(float city_baseline)
    {
        InitialisePreferencesIfNeeded();

        float delta = 0.0f;

        ResourceManager rm = ResourceManager.instance;
        if (rm != null)
        {
            if (rm.IsCheeseTypeActive(favourite_cheese)) delta += favourite_bonus;
            if (rm.IsCheeseTypeActive(least_favourite_cheese)) delta -= least_penalty;
        }

        float target = Mathf.Clamp01(city_baseline + delta);

        // Tick-based smoothing: moves a small amount each MoraleSystem update
        mouse_morale = Mathf.MoveTowards(mouse_morale, target, morale_step_per_tick);
    }
}
