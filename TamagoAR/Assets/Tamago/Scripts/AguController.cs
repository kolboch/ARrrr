using GoogleARCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class AguController : MonoBehaviour {

    public Camera FirstPersonCamera;

    public float rotateSpeed = 1f;
    public float walkSpeed = 1f;
    public int yUpdateInterval = 5;
    public float jumpHeightOffset = 0.1f;

    private float jumpAnimTime;
    private Animator animator;
    private Vector3 DestinationPoint;

    private IEnumerator Walking;
    private IEnumerator Rotating;
    private IEnumerator Jumping;
    private List<Vector3> PathSubPoints = new List<Vector3>();

    private DetectedPlane CurrentPlane;
    private DetectedPlane DestinationPlane;
    private GameController GameController;

    private bool updateYActive = true;
    private bool performsJump = false;

    void Start() {
        animator = GetComponent<Animator>();
        StartCoroutine(UpdateYRelativeToCurrentPlane());
        jumpAnimTime = animator.runtimeAnimatorController.animationClips.First(anim => anim.name == AguAnim.JUMP_CLIP).length;
    }

    // Update is called once per frame
    void Update() {
        HandleInput();
    }

    public void OnStarCollected() {
        GameController.CollectStar();
    }

    public void SetCurrentPlane(DetectedPlane plane) {
        CurrentPlane = plane;
    }

    public void SetGameController(GameController controller) {
        GameController = controller;
    }

    private void HandleInput() {
        Touch touch;
        if (Input.touchCount >= 1 && (touch = Input.GetTouch(0)).phase == TouchPhase.Began) {
            //casting against objects tracked by arcore - frame raycast ( not for custom ones)
            TrackableHit hit;
            TrackableHitFlags filter = TrackableHitFlags.PlaneWithinPolygon;
            if (Frame.Raycast(touch.position.x, touch.position.y, filter, out hit)) {
                DetectedPlane plane;
                if (hit.Trackable is DetectedPlane) {
                    plane = (DetectedPlane)hit.Trackable;
                } else {
                    return;
                }
                if (!plane.IsVerticalPlane() && Vector3.Dot(FirstPersonCamera.transform.position - hit.Pose.position, hit.Pose.rotation * Vector3.up) > 0) {
                    DestinationPoint = hit.Pose.position;
                    DestinationPlane = plane;
                    StartMovement();
                } else {
                    Debug.Log("Hit at back of detected plane");
                }
            }
        }
    }

    private void StartMovement() {
        if (performsJump) {
            return;
        }
        StartCoroutine(RotateAndWalkToPosition());
    }

    private IEnumerator RotateAndWalkToPosition() {
        if (!DestinationPlane.Equals(CurrentPlane)) {
            Debug.Log("Walking to different plane!");
            GameController.FindSubpathTo(transform.position, DestinationPoint, CurrentPlane, DestinationPlane, ref PathSubPoints);
            if (PathSubPoints.Count > 0) {
                PreRotateAndWalk();
                Debug.Log("Subpoints count: " + PathSubPoints.Count);
                for (int i = 0; i < PathSubPoints.Count; i += 2) {
                    yield return StartCoroutine(Rotating = RotateCoroutine(GetLookRotation(PathSubPoints[i])));
                    yield return StartCoroutine(Walking = WalkCoroutine(GetPointIgnoredRelativeY(PathSubPoints[i])));
                    yield return StartCoroutine(Rotating = RotateCoroutine(GetLookRotation(PathSubPoints[i + 1])));
                    yield return StartCoroutine(Jumping = JumpCoroutine(PathSubPoints[i + 1], DestinationPlane));
                }
                yield return StartCoroutine(Rotating = RotateCoroutine(GetLookRotation(DestinationPoint)));
                yield return StartCoroutine(Walking = WalkCoroutine(GetPointIgnoredRelativeY(DestinationPoint)));
            }
        } else {
            PreRotateAndWalk();
            yield return StartCoroutine(Rotating = RotateCoroutine(GetLookRotation(DestinationPoint)));
            yield return StartCoroutine(Walking = WalkCoroutine(GetPointIgnoredRelativeY(DestinationPoint)));
        }
    }

    private void PreRotateAndWalk() {
        StopMovementCoroutines();
        StopMovementAnimations();
    }

    private Quaternion GetLookRotation(Vector3 destination) {
        var destinationYIgnored = GetPointIgnoredRelativeY(destination);
        Vector3 direction = destinationYIgnored - transform.position;
        return Quaternion.LookRotation(direction);
    }

    private void StopMovementCoroutines() {
        if (Rotating != null) {
            StopCoroutine(Rotating);
        }
        if (Walking != null) {
            StopCoroutine(Walking);
        }
    }

    private void StopMovementAnimations() {
        Assert.IsNotNull(animator);
        animator.SetBool(AguAnim.IS_WALKING, false);
    }


    private Vector3 GetPointIgnoredRelativeY(Vector3 point) {
        var ignoredY = point;
        ignoredY.y = transform.position.y;
        return ignoredY;
    }

    private IEnumerator WalkCoroutine(Vector3 destination) {
        Debug.Log("Walking invoked.");
        animator.SetBool(AguAnim.IS_WALKING, true);
        while (transform.position != destination) {
            transform.position = Vector3.MoveTowards(transform.position, destination, walkSpeed * Time.deltaTime);
            yield return null;
        }
        animator.SetBool(AguAnim.IS_WALKING, false);
    }

    private IEnumerator RotateCoroutine(Quaternion lookRotation) {
        while (transform.rotation != lookRotation) {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, rotateSpeed);
            yield return null;
        }
    }

    private IEnumerator UpdateYRelativeToCurrentPlane() {
        while (true) {
            if (updateYActive && CurrentPlane != null && !performsJump) {
                float updatedY = CurrentPlane.CenterPose.position.y;
                Vector3 updatedPosition = transform.position;
                updatedPosition.y = updatedY;
                transform.position = updatedPosition;
            }
            yield return new WaitForSeconds(yUpdateInterval);
        }
    }

    private IEnumerator JumpCoroutine(Vector3 destination, DetectedPlane destinationPlane) {
        performsJump = true;
        animator.SetBool(AguAnim.IS_JUMPING, true);
        var isJumpUp = destination.y > transform.position.y;
        var yMidPoint = (isJumpUp ? destination.y : transform.position.y) + jumpHeightOffset;
        Vector3 midPoint = new Vector3((transform.position.x + destination.x) / 2, yMidPoint, (transform.position.z + destination.z) / 2);
        var maxDelta = Vector3.Distance(transform.position, destination) / jumpAnimTime;
        //loop mid point
        while (transform.position != midPoint) {
            transform.position = Vector3.MoveTowards(transform.position, midPoint, maxDelta * Time.deltaTime);
            yield return null;
        }
        //loop destination
        while (transform.position != destination) {
            transform.position = Vector3.MoveTowards(transform.position, destination, maxDelta * Time.deltaTime);
            yield return null;
        }
        CurrentPlane = destinationPlane;
        animator.SetBool(AguAnim.IS_JUMPING, false);
        performsJump = false;
    }
}
