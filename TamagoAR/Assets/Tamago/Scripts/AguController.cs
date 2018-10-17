using GoogleARCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class AguController : MonoBehaviour {

    public Camera FirstPersonCamera;
    public float rotateSpeed = 1f;
    public float walkSpeed = 1f;
    private IEnumerator Rotating;
    private IEnumerator Walking;
    private Animator animator;
    private Vector3 DestinationPoint;

    void Start() {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update() {
        HandleInput();
    }

    private void HandleInput() {
        Touch touch;
        if (Input.touchCount >= 1 && (touch = Input.GetTouch(0)).phase == TouchPhase.Began) {
            //casting against objects tracked by arcore ( not for custom ones)
            TrackableHit hit;
            TrackableHitFlags filter = TrackableHitFlags.PlaneWithinPolygon;
            if (Frame.Raycast(touch.position.x, touch.position.y, filter, out hit)) {
                if (hit.Trackable is DetectedPlane && Vector3.Dot(FirstPersonCamera.transform.position - hit.Pose.position, hit.Pose.rotation * Vector3.up) > 0) {
                    DestinationPoint = hit.Pose.position;
                    RotateAndWalkToPosition();
                } else {
                    Debug.Log("Hit at back of detected plane");
                }
            }
        }
    }

    private void RotateAndWalkToPosition() {
        if (Rotating != null) {
            StopCoroutine(Rotating);
        }
        StopWalkingCoroutine();
        Assert.IsNotNull(animator);
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
        if (Walking != null) {
            StopCoroutine(Walking);
        }
    }

    private IEnumerator WalkingCoroutine() {
        Debug.Log("Walking invoked.");
        animator.SetBool("IsWalking", true);
        var destinationIgnoredY = new Vector3(DestinationPoint.x, transform.position.y, DestinationPoint.z);
        while (transform.position != destinationIgnoredY) {
            transform.position = Vector3.MoveTowards(transform.position, destinationIgnoredY, walkSpeed * Time.deltaTime);
            yield return null;
        }
        animator.SetBool("IsWalking", false);
    }

    private IEnumerator RotateCoroutine(IEnumerator SuccessorCoroutine = null) {
        Debug.Log("RotateCoroutine invoked.");
        Vector3 destinationIgnoredY = new Vector3(DestinationPoint.x, transform.position.y, DestinationPoint.z);
        Vector3 direction = destinationIgnoredY - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        Debug.Log("Here I am");
        while (transform.rotation != lookRotation) {
            Debug.Log("Inside loop");
            Debug.Log("Character: " + transform.rotation + " Destination: " + lookRotation);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, rotateSpeed);
            yield return null;
        }
        if (SuccessorCoroutine != null) {
            yield return StartCoroutine(SuccessorCoroutine);
        }
    }
}
