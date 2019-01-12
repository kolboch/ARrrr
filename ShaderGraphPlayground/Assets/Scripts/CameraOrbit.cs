using System;
using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    public float mouseSensivity = 4f;
    public float scrollSensivity = 2f;
    public float orbitDampening = 10f;
    public float scrollDampening = 6f;
    public bool cameraDisabled = false;

    [Tooltip("Minimum angle between the camera and pivot object horizon")]
    public float clampHorizon = 10f;
    
    [Tooltip("Minimum distance between the camera and the pivot point [m]")]
    public float cameraClampMin = 2f;
    [Tooltip("Maximum distance between the camera and the pivot point [m]")]
    public float cameraClampMax = 150f;

    private Transform CameraTransform;
    private Transform ParentTransform;
    private Vector3 LocalRotation;
    private float cameraDistance = 10f;
    private float ZERO_TOLERANCE = 0.001f;
    private const String MOUSE_X = "Mouse X";
    private const String MOUSE_Y = "Mouse Y";
    private const String MOUSE_SCROLLWHEEL = "Mouse ScrollWheel";
    private const float SCROLL_MAGIC_NUMBER = 0.3f;
    private float clampCeiling = 90f;
    
    void Start()
    {
        CameraTransform = transform; // script must be attached to the camera
        ParentTransform = transform.parent; // the camera is nested
    }

    // camera is handled in late update to be sure that everything else was rendered before
    void LateUpdate()
    {
        // toggle camera
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            cameraDisabled = !cameraDisabled;
        }

        if (!cameraDisabled)
        {
            // mouse rotation
            if (Math.Abs(Input.GetAxis(MOUSE_X)) > ZERO_TOLERANCE || Math.Abs(Input.GetAxis(MOUSE_Y)) > ZERO_TOLERANCE)
            {
                LocalRotation.y += Input.GetAxis(MOUSE_X) * mouseSensivity;
                LocalRotation.x -= Input.GetAxis(MOUSE_Y) * mouseSensivity;

                // clamp the rotation at the horizon & do not flip over at the top
                LocalRotation.x = Mathf.Clamp(LocalRotation.x, clampHorizon, clampCeiling);
            }

            // handling scroll wheel input
            if (Math.Abs(Input.GetAxis(MOUSE_SCROLLWHEEL)) > ZERO_TOLERANCE)
            {
                float scrollAmount = Input.GetAxis(MOUSE_SCROLLWHEEL) * scrollSensivity;
                // the greater distance the faster scroll
                scrollAmount *= cameraDistance * SCROLL_MAGIC_NUMBER;
                cameraDistance += -scrollAmount;
                // clamping camera
                cameraDistance = Mathf.Clamp(cameraDistance, cameraClampMin, cameraClampMax);
            }
        }

        // camera rig transformations
        Quaternion quaternion = Quaternion.Euler(LocalRotation);
        ParentTransform.rotation =
            Quaternion.Lerp(ParentTransform.rotation, quaternion, Time.deltaTime * orbitDampening);
        if (Math.Abs(CameraTransform.localPosition.z + cameraDistance) > ZERO_TOLERANCE)
        {
            CameraTransform.localPosition = new Vector3(0f, 0f,
                Mathf.Lerp(CameraTransform.localPosition.z, -cameraDistance, Time.deltaTime * scrollDampening));
        }
    }
}