using GoogleARCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class AguController : MonoBehaviour {

    public Camera FirstPersonCamera;
    public float rotateSpeed = 1f;
    public float walkSpeed = 1f;
    public int yUpdateInterval = 5;
    private IEnumerator Rotating;
    private IEnumerator Walking;
    private Animator animator;
    private Vector3 DestinationPoint;
    private DetectedPlane currentPlane;
    private DetectedPlane destinationPlane;
    private bool updateYActive = true;

    void Start() {
        animator = GetComponent<Animator>();
        StartCoroutine(UpdateYRelativeToCurrentPlane());
    }

    // Update is called once per frame
    void Update() {
        HandleInput();
    }
    
    public void SetCurrentPlane(DetectedPlane plane) {
        currentPlane = plane;
    }

    private void HandleInput() {
        Touch touch;
        if (Input.touchCount >= 1 && (touch = Input.GetTouch(0)).phase == TouchPhase.Began) {
            //casting against objects tracked by arcore - frame raycast ( not for custom ones)
            TrackableHit hit;
            TrackableHitFlags filter = TrackableHitFlags.PlaneWithinPolygon;
            if (Frame.Raycast(touch.position.x, touch.position.y, filter, out hit)) {
                DetectedPlane plane;
                if(hit.Trackable is DetectedPlane) {
                    plane = (DetectedPlane) hit.Trackable;
                } else {
                    return;
                }
                if (!plane.IsVerticalPlane() && Vector3.Dot(FirstPersonCamera.transform.position - hit.Pose.position, hit.Pose.rotation * Vector3.up) > 0) {
                    DestinationPoint = hit.Pose.position;
                    destinationPlane = plane;
                    RotateAndWalkToPosition();
                } else {
                    Debug.Log("Hit at back of detected plane");
                }
            }
        }
    }

    private void RotateAndWalkToPosition() {
        if(destinationPlane != currentPlane) {
            Debug.Log("Walking to different plane!");
        }
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
        while (transform.rotation != lookRotation) {
            Debug.Log("Character: " + transform.rotation + " Destination: " + lookRotation);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, rotateSpeed);
            yield return null;
        }
        if (SuccessorCoroutine != null) {
            yield return StartCoroutine(SuccessorCoroutine);
        }
    }

    private IEnumerator UpdateYRelativeToCurrentPlane() {
        while(true) {
            if(updateYActive && currentPlane != null) {
                float updatedY = currentPlane.GetPlaneCenter().position.y;
                Vector3 updatedPosition = transform.position;
                updatedPosition.y = updatedY;
                transform.position = updatedPosition;
            }
            yield return new WaitForSeconds(yUpdateInterval);
        }
    }
}
