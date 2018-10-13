using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

    public Transform Character;
    public Animator animator;
    public Camera MainCamera;
    public float speed = 0.5f;
    private Vector3 DestinationPoint;
    private IEnumerator Walking;

    // Use this for initialization
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        HandleInput();
    }

    private void HandleInput() {
        if (Input.GetMouseButton(0)) {
            RaycastHit hit;
            Ray ray = MainCamera.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out hit);
            if (hit.collider.tag == "Environment") {
                DestinationPoint = hit.point;
                StartWalkingToPosition();
            }
        }
    }

    private void StartWalkingToPosition() {
        if(Walking != null) {
            StopCoroutine(Walking);
        }
        Walking = WalkingCoroutine();
        StartCoroutine(Walking);
    }

    private IEnumerator WalkingCoroutine() {
        animator.SetBool("IsWalking", true);
        Debug.Log("Is walking anim bool: " + animator.GetBool("IsWalking"));
        var destinationIgnoredY = new Vector3(DestinationPoint.x, Character.position.y, DestinationPoint.z);
        while (Character.position != destinationIgnoredY) {
            Character.position = Vector3.MoveTowards(Character.position, destinationIgnoredY, speed * Time.deltaTime);
            yield return null;
        }
        animator.SetBool("IsWalking", false);
        Debug.Log("Is walking anim bool: " + animator.GetBool("IsWalking"));
    }
}
