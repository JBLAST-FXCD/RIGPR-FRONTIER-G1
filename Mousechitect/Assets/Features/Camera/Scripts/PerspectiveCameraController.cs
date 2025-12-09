using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Jess @ 02/12/2025
// <summary>
// Camera controller for perspective view that orbits while holding RMB, pans with WASD or by moving cursor to screen edge, and zooms with mouse wheel.
// </summary>

public class PerspectiveCameraController : MonoBehaviour
{
    public Transform target;

    [Header("Camera Settings")]
    [SerializeField] private float orbit_speed = 0.0f;
    [SerializeField] private float wasd_pan_speed = 0.0f;
    [SerializeField] private float edge_pan_speed = 0.0f;
    [SerializeField] private float follow_smoothing = 0.0f;
    [SerializeField] private float edge_pan_size = 0.0f;
    [SerializeField] private LayerMask ground_layer;

    [Header("Camera Zoom Settings")]
    [SerializeField] private float zoom_speed = 0.0f;


    private float zoom_distance = 0.0f, zoom_min_distance = 0.0f, zoom_max_distance = 0.0f;
    private float yaw = 0.0f, pitch = 0.0f, float_value_zero;

    private Vector3 target_position_current, target_position_desired;
    private Camera camera_component;

    private const float FADE_START_DISTANCE = 20.0f;
    private const float FADE_END_DISTANCE = 10.0f;
    private const float ROTATION_MIN_DEGREES = -25.0f;
    private const float ROTATION_MAX_DEGREES = 45.0f;
    private const float ZOOM_MIN_PERCENT = 0.3f;
    private const float ZOOM_MAX_PERCENT = 1.30f;

    private const int MOUSE_RIGHT_BUTTON = 1;

    public void SetTarget(Transform camera_target)
    {
        target = camera_target;
    }

    public Transform GetTarget()
    {
               return target;
    }

    private void Start()
    {
        // sets default cam parameters on start
        camera_component = GetComponent<Camera>();

        target_position_current = target.position;
        target_position_desired = target.position;
        pitch = float_value_zero;
        yaw = float_value_zero;
    }

    private void LateUpdate()
    {
        HandleInput();
        UpdateTargetPosition();
        UpdateCameraTransform();
        ApplyFadeCulling();
    }

    private void HandleInput()
    {
        float mouse_scroll = Input.GetAxis("Mouse ScrollWheel");

        if (mouse_scroll != float_value_zero)
        {
            zoom_distance -= mouse_scroll * zoom_speed;
        }

        if (Input.GetMouseButton(MOUSE_RIGHT_BUTTON))
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            float mx = Input.GetAxis("Mouse X");
            float my = Input.GetAxis("Mouse Y");

            yaw += mx * orbit_speed;
            pitch -= my * orbit_speed;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        // clamp to prevent flipping
        pitch = Mathf.Clamp(pitch, ROTATION_MIN_DEGREES, ROTATION_MAX_DEGREES);
        
        // WASD panning
        float horizontal_input = Input.GetAxis("Horizontal");
        float vertical_input = Input.GetAxis("Vertical");

        if (horizontal_input != float_value_zero || vertical_input != float_value_zero)
        {
            Vector3 forward_wasd_pan = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            Vector3 right_wasd_pan = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;

            target_position_desired += (right_wasd_pan * horizontal_input + forward_wasd_pan * vertical_input) * wasd_pan_speed;
        }

        // edge panning, moves camera when mouse is near screen edge
        Vector3 mouse_position = Input.mousePosition;

        Vector3 forward_edge_pan = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        Vector3 right_edge_pan = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;

        if (mouse_position.x <= edge_pan_size)
        {
            target_position_desired -= right_edge_pan * edge_pan_speed;
        }

        else if (mouse_position.x >= Screen.width - edge_pan_size)
        {
            target_position_desired += right_edge_pan * edge_pan_speed;
        }

        if (mouse_position.y <= edge_pan_size)
        {
            target_position_desired -= forward_edge_pan * edge_pan_speed;
        }

        else if (mouse_position.y >= Screen.height - edge_pan_size)
        {
            target_position_desired += forward_edge_pan * edge_pan_speed;
        }
    }

    private void UpdateTargetPosition()
    {
        // smooth movement between current and desired target position
        target_position_current = Vector3.Lerp(target_position_current, target_position_desired, follow_smoothing);
    }

    private void UpdateCameraTransform()
    {
        // Adjust zoom limits based on ground raycast
        float ground_distance = GetGroundDistance();
        zoom_min_distance = ground_distance * ZOOM_MIN_PERCENT;
        zoom_max_distance = ground_distance * ZOOM_MAX_PERCENT;

        zoom_distance = Mathf.Clamp(zoom_distance, zoom_min_distance, zoom_max_distance);

        // Calculate camera position and rotation
        Quaternion rotation = Quaternion.Euler(pitch, yaw, float_value_zero);
        Vector3 camera_offset = rotation * new Vector3(float_value_zero, float_value_zero, -zoom_distance);
        Vector3 desired_camera_position = target_position_current + camera_offset;

        transform.position = desired_camera_position;
        transform.rotation = Quaternion.LookRotation(target_position_current - transform.position, Vector3.up);
    }


    // Raycasts to find distance to ground below the camera
    private float GetGroundDistance()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        RaycastHit hit;

        float max_cast_distance = 1000.0f;

        if (Physics.Raycast(ray, out hit, max_cast_distance, ground_layer))
        {
                       return hit.distance;
        }
        return 100.0f;
    }

    private void ApplyFadeCulling()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, FADE_START_DISTANCE);

        foreach (var hit in hits)
        {
            ObjectFading fadeable = hit.GetComponent<ObjectFading>();
            // skips if no fade component is found
            if (fadeable == null)
            {
                continue;
            }

            float distance_to_camera = Vector3.Distance(transform.position, fadeable.transform.position);

            // constrains alpha values to 0-1 range based on distance to camera, parses into the ObjectFading component
            if (distance_to_camera <= FADE_END_DISTANCE)
            {
                fadeable.SetObjectFade(0.0f);
            }

            else if (distance_to_camera >= FADE_START_DISTANCE)
            {
                fadeable.SetObjectFade(1.0f);
            }
            else
            {
                float alpha = (distance_to_camera - FADE_END_DISTANCE) / (FADE_START_DISTANCE - FADE_END_DISTANCE);
                fadeable.SetObjectFade(alpha);
            }
        }
    }
}
