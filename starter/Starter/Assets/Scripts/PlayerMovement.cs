using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    public Rigidbody rigidBody;
    public float baseSpeed = 100f;
    public float sideSpeed = 150f;
    public float gameEndMinY = -2f;

    // Update is called once per frame
    void FixedUpdate()
    {
        if (rigidBody.position.y < gameEndMinY)
        {
            FindObjectOfType<GameManager>().EndGame();
        }
        else
        {
            rigidBody.AddForce(0, 0, baseSpeed * Time.deltaTime);
            float horizontalMovement = Input.GetAxis("Horizontal");
            rigidBody.AddForce(
                sideSpeed * horizontalMovement * Time.deltaTime, 0, 0,
                ForceMode.VelocityChange
            );
        }
    }
}
