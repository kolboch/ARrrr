using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollision : MonoBehaviour {

    public MonoBehaviour playerMovement;

    private void OnCollisionEnter(Collision collision)
    { 
        if(collision.collider.tag.Equals("BlueBox"))
        {
            Debug.Log("Colided with blue box");
            playerMovement.enabled = false;
            FindObjectOfType<GameManager>().EndGame();
        }    
        else
        {
            Debug.Log("Colision detected");
        }
    }
}
