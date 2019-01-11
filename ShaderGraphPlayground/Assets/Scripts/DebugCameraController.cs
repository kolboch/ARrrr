using UnityEngine;
using UnityEngine.Experimental.UIElements;

public class DebugCameraController : MonoBehaviour
{
    private bool isActive = false;
    public float transformModifier = 0.1f;
    public float rotationSpeed = 1f;
    
    // Update is called once per frame
    void Update()
    {
        if (isActive)
        {
            Vector3 positionChange = new Vector3();
            if (Input.GetKey(KeyCode.A))
            {
                positionChange.x = -transformModifier;
            }

            if (Input.GetKey(KeyCode.D))
            {
                positionChange.x = transformModifier;
            }

            if (Input.GetKey(KeyCode.W))
            {
                positionChange.z = transformModifier;
            }

            if (Input.GetKey(KeyCode.S))
            {
                positionChange.z = -transformModifier;
            }

            if (Input.GetMouseButton(1)) // 0- left click, 1- right click, 2- middle click
            {
                transform.Rotate(new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0) * Time.deltaTime * rotationSpeed, Space.Self);    
            }
            
            transform.Translate(positionChange);
        }
        else if (Input.GetKey(KeyCode.C) && Input.GetKey(KeyCode.M))
        {
            isActive = !isActive; // toggle camera
        }
        
        Vector3 forward = transform.TransformDirection(Vector3.forward) * 10;
        Vector3 right = transform.TransformDirection(Vector3.right) * 10;
        Vector3 up = transform.TransformDirection(Vector3.up) * 10;
        Debug.DrawRay(transform.position, forward, Color.blue);
        Debug.DrawRay(transform.position, right, Color.red);
        Debug.DrawRay(transform.position, up, Color.green);

        //TODO make sure to include deltaTime everywhere
    }
}