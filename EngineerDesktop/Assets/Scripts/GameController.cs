using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

    public Transform Character;
    public Animator animator;
    public Camera MainCamera;
    public float WalkSpeed = 0.5f;
    public float RotateSpeed = 5f;
    private Vector3 DestinationPoint;
    private IEnumerator Walking;
    private IEnumerator Rotating;

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
                RotateAndWalkToPosition();
            }
        }
    }

    private void RotateAndWalkToPosition() {
        if(Rotating != null) {
            StopCoroutine(Rotating);
        }
        StopWalkingCoroutine();
        animator.SetBool("IsWalking", false);
        Walking = WalkingCoroutine();
        Rotating = RotateCoroutine(Walking);
        StartCoroutine(Rotating);
    }

    private void StartWalkingToPosition() {
        StopWalkingCoroutine();
        Walking = WalkingCoroutine();
        StartCoroutine(Walking);
    }

    private void StopWalkingCoroutine() {
        if(Walking != null) {
            StopCoroutine(Walking);
        }
    }

    private IEnumerator WalkingCoroutine() {
        animator.SetBool("IsWalking", true);
        var destinationIgnoredY = new Vector3(DestinationPoint.x, Character.position.y, DestinationPoint.z);
        while (Character.position != destinationIgnoredY) {
            Character.position = Vector3.MoveTowards(Character.position, destinationIgnoredY, WalkSpeed * Time.deltaTime);
            yield return null;
        }
        animator.SetBool("IsWalking", false);
    }

    private IEnumerator RotateCoroutine(IEnumerator SuccessorCoroutine = null) {
        Vector3 destinationIgnoredY = new Vector3(DestinationPoint.x, Character.position.y, DestinationPoint.z);
        Vector3 direction = destinationIgnoredY - Character.position;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        Debug.Log("Here I am");
        while (Character.rotation != lookRotation) {
            Debug.Log("Inside loop");
            Debug.Log("Character: " + Character.rotation + " Destination: " + lookRotation);
            Character.rotation = Quaternion.RotateTowards(Character.rotation, lookRotation, RotateSpeed);
            yield return null;
        }
        if (SuccessorCoroutine != null) {
            yield return StartCoroutine(SuccessorCoroutine);
        }
    }
}
