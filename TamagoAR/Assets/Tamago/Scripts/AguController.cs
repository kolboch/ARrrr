using GoogleARCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AguController : MonoBehaviour {

    public Camera FirstPersonCamera;

    public float rotateSpeed = 1f;
    public float walkSpeed = 1f;
    public int yUpdateInterval = 5;
    public float jumpHeightOffset = 0.1f;

    private float jumpAnimTime;
    private float rainDanceTime;
    private Animator animator;

    private IEnumerator Walking;
    private IEnumerator Rotating;
    private IEnumerator Jumping;
    private IEnumerator SequenceMoving;
    private IEnumerator RainDancing;
    private List<Vector3> PathSubPoints = new List<Vector3>();

    private DetectedPlane CurrentPlane;
    private GameController GameController;

    private bool updateYActive = true;
    private bool performsJump = false;
    private bool performsRainDance = false;

    void Start() {
        animator = GetComponent<Animator>();
        StartCoroutine(UpdateYRelativeToCurrentPlane());
        jumpAnimTime = animator.runtimeAnimatorController.animationClips.First(anim => anim.name == AguAnim.JUMP_CLIP).length;
        rainDanceTime = animator.runtimeAnimatorController.animationClips.First(anim => anim.name == AguAnim.RAIN_DANCE_CLIP).length;
    }

    // Update is called once per frame
    void Update() {
        HandleInput();
    }

    private void OnDestroy() {
        StopAllCoroutines();
    }

    public void OnStarCollected() {
        GameController.CollectStar();
    }
    
    public void OnCarrotCollected()
    {
        GameController.CollectCarrot();
    }

    public void SetCurrentPlane(DetectedPlane plane) {
        CurrentPlane = plane;
    }

    public void SetGameController(GameController controller) {
        GameController = controller;
    }

    private void HandleInput() {
        if (performsRainDance)
        {
            return;
        }
        Touch touch;
        if (Input.touchCount >= 1 && (touch = Input.GetTouch(0)).phase == TouchPhase.Began) {
            if (IsClickOnUI(touch.position.x, touch.position.y))
            {
                return;
            }
            // check if click on character
            var ray = FirstPersonCamera.ScreenPointToRay(touch.position);
            RaycastHit physicsHit;
            if (Physics.Raycast(ray, out physicsHit))
            {
                Debug.DrawRay(FirstPersonCamera.ScreenToWorldPoint(touch.position), Vector3.up);
                if (physicsHit.collider.CompareTag(Tags.TAG_CHARACTER))
                {
                    Debug.Log("Character touch!!");
                    HandleCharacterTouch();
                    return;
                }
            }
            
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
                    StartMovement(plane, hit.Pose.position);
                } else {
                    Debug.Log("Hit at back of detected plane");
                }
            }
        }
    }

    private void HandleCharacterTouch()
    {
        if (GameController.HasCarrotsThatNeedRain())
        {
            if (performsJump)
            {
                return;
            }
            else
            {
                PreRotateAndWalk();
                StartCoroutine(RainDancing = RainDanceCoroutine());
            }
        }
    }

    private bool IsClickOnUI(float touchX, float touchY)
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current) {position = new Vector2(touchX, touchY)};
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 0;
    }

    private void StartMovement(DetectedPlane destinationPlane, Vector3 destinationPoint) {
        if (performsJump) {
            return;
        } else {
            PreRotateAndWalk();
            StartCoroutine(SequenceMoving = RotateAndWalkToPosition(destinationPlane, destinationPoint));
        }
    }

    private IEnumerator RotateAndWalkToPosition(DetectedPlane destinationPlane, Vector3 destinationPoint) {
        if (destinationPlane != null && !destinationPlane.Equals(CurrentPlane)) {
            Debug.Log("Walking to different plane!");
            GameController.FindSubpathTo(transform.position, destinationPoint, CurrentPlane, destinationPlane, ref PathSubPoints);
            if (PathSubPoints.Count > 0) {
                Debug.Log("Subpoints count: " + PathSubPoints.Count);
                for (int i = 0; i < PathSubPoints.Count; i += 2) {
                    yield return StartCoroutine(Rotating = RotateCoroutine(GetLookRotation(PathSubPoints[i])));
                    yield return StartCoroutine(Walking = WalkCoroutine(GetPointIgnoredRelativeY(PathSubPoints[i])));
                    yield return StartCoroutine(Rotating = RotateCoroutine(GetLookRotation(PathSubPoints[i + 1])));
                    yield return StartCoroutine(Jumping = JumpCoroutine(PathSubPoints[i + 1], destinationPlane));
                }
                yield return StartCoroutine(Rotating = RotateCoroutine(GetLookRotation(destinationPoint)));
                yield return StartCoroutine(Walking = WalkCoroutine(GetPointIgnoredRelativeY(destinationPoint)));
            }
        } else {
            yield return StartCoroutine(Rotating = RotateCoroutine(GetLookRotation(destinationPoint)));
            yield return StartCoroutine(Walking = WalkCoroutine(GetPointIgnoredRelativeY(destinationPoint)));
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
            Rotating = null;
        }
        if (Walking != null) {
            StopCoroutine(Walking);
            Walking = null;
        }
        if (Jumping != null) {
            StopCoroutine(Jumping);
            Jumping = null;
        }
        if (SequenceMoving != null) {
            StopCoroutine(SequenceMoving);
            SequenceMoving = null;
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
            if (CurrentPlane == null || CurrentPlane.TrackingState != TrackingState.Tracking) {
                GameController.RepositionCharacterAfterPlaneLost(gameObject);
            } else if (updateYActive && !performsJump) {
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

        if (destinationPlane != null)
        {
            CurrentPlane = destinationPlane;    
        }
        
        animator.SetBool(AguAnim.IS_JUMPING, false);
        performsJump = false;
    }

    private IEnumerator RainDanceCoroutine()
    {
        performsRainDance = true;
        animator.SetTrigger(AguAnim.RAIN_DANCE_TRIGGER);
        yield return new WaitForSeconds(rainDanceTime);
        performsRainDance = false;
        GameController.OnRainDancePerformed();
    }
}
