﻿using GoogleARCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

    public Camera FirstPersonCamera;
    public GameObject SearchingPlanesUI;
    public GameObject CharacterToPlace;

    private bool isQuitting = false;
    private bool characterNeedsInit = true;
    private List<DetectedPlane> DetectedPlanes = new List<DetectedPlane>();

    void Start() {

    }

    void Update() {
        UpdateApplicationLifecycle();
        UpdatePlaneUI();
    }

    private void UpdatePlaneUI() {
        Session.GetTrackables<DetectedPlane>(DetectedPlanes);
        bool showSearchingUI = true;
        for (int i = 0; i < DetectedPlanes.Count; i++) {
            if (DetectedPlanes[i].TrackingState == TrackingState.Tracking) {
                if (characterNeedsInit) {
                    InitCharacter(DetectedPlanes[i]);
                }
                showSearchingUI = false;
                break;
            }
        }
        SearchingPlanesUI.SetActive(showSearchingUI);
    }

    private void InitCharacter(DetectedPlane plane) {
        characterNeedsInit = false;
        Pose center = plane.GetPlaneCenter();
        Vector3 centroid = center.position;
        Debug.Log("Centroid from AR API: " + centroid);
        Vector3 direction = FirstPersonCamera.transform.position - centroid;
        GameObject character = GameObject.Instantiate(CharacterToPlace, centroid, Quaternion.LookRotation(direction));
        character.GetComponent<AguController>().FirstPersonCamera = FirstPersonCamera;
        character.GetComponent<AguController>().SetCurrentPlane(plane);
        character.GetComponent<AguController>().SetGameController(this);
        characterNeedsInit = false;
    }

    private void UpdateApplicationLifecycle() {
        if (Input.GetKey(KeyCode.Escape)) {
            Application.Quit();
        }

        if (Session.Status != SessionStatus.Tracking) {
            const int lostTrackingSleepTimeout = 15;
            Screen.sleepTimeout = lostTrackingSleepTimeout;
        } else {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        if (isQuitting) {
            return;
        }

        if (Session.Status == SessionStatus.ErrorPermissionNotGranted) {
            AndroidUtils.ShowAndroidToastMessage("Camera permission is needed to run this application.");
            isQuitting = true;
            StartCoroutine(DoQuitWithDelay());
        } else if (Session.Status.IsError()) {
            AndroidUtils.ShowAndroidToastMessage("ARCore encountered a problem connecting.  Please start the app again.");
            isQuitting = true;
            StartCoroutine(DoQuitWithDelay());
        }
    }

    private IEnumerator DoQuitWithDelay(float delay = 0.5f) {
        yield return new WaitForSeconds(delay);
        Application.Quit();
    }

    public void FindSubpathTo(
        Vector3 currentPosition, Vector3 destinationPoint,
        DetectedPlane currentPlane, DetectedPlane destinationPlane,
        ref List<Vector3> subPoints
    ) {
        if (currentPlane == destinationPlane) {
            return;
        }
        bool isJumpUp = destinationPoint.y - currentPosition.y > 0;
        var currentPointsConsidered = new List<Vector3>();
        var destinationPointsConsidered = new List<Vector3>();
        currentPlane.GetBoundaryPolygon(currentPointsConsidered);
        destinationPlane.GetBoundaryPolygon(destinationPointsConsidered);
        float deltaX = destinationPoint.x - currentPosition.x;
        float deltaZ = destinationPoint.z - currentPosition.z;
        currentPointsConsidered = currentPointsConsidered.FindAll(point => {
            bool valid_x = deltaX > 0 ? point.x >= currentPosition.x : point.x <= currentPosition.x;
            return valid_x && deltaZ > 0 ? point.z >= currentPosition.z : point.z <= currentPosition.z; // valid_z
        });
        destinationPointsConsidered = destinationPointsConsidered.FindAll(point => {
            bool valid_x = -deltaX > 0 ? point.x >= destinationPoint.x : point.x <= destinationPoint.x;
            return valid_x && -deltaZ > 0 ? point.z >= destinationPoint.z : point.z <= destinationPoint.z; ;
        });
        // we have only valid points at this moment ( considering direction to move)
        // now optimize if there is any overlapping of planes
        if (isJumpUp) {
            List<Vector3> pointsInCurrentPlaneExtents = new List<Vector3>();
            destinationPointsConsidered.ForEach(point => {
                if (currentPlane.IsPoseInExtents(new Pose(point, Quaternion.identity))) {
                    pointsInCurrentPlaneExtents.Add(point);
                }
            });
            if (pointsInCurrentPlaneExtents.Count > 0) {
                destinationPointsConsidered = pointsInCurrentPlaneExtents;
                currentPointsConsidered = generateMidPoints(ref destinationPointsConsidered, currentPosition, currentPosition.y, 1 / 3);
            }
        } else {
            List<Vector3> pointsInDestinationPlaneExtents = new List<Vector3>();
            currentPointsConsidered.ForEach(point => {
                if (destinationPlane.IsPoseInExtents(new Pose(point, Quaternion.identity))) {
                    pointsInDestinationPlaneExtents.Add(point);
                }
            });
            if (pointsInDestinationPlaneExtents.Count > 0) {
                currentPointsConsidered = pointsInDestinationPlaneExtents;
                destinationPointsConsidered = generateMidPoints(ref currentPointsConsidered, destinationPoint, destinationPoint.y, 1 / 3);
            }
        }
        // now find closest pair
        float minDistance = float.MaxValue;
        var minPoint1 = Vector3.zero;
        var minPoint2 = Vector3.zero;
        float distPoints, distOrigin, distDestination, distanceTotal;
        foreach (Vector3 p1 in currentPointsConsidered) {
            foreach (Vector3 p2 in destinationPointsConsidered) {
                distPoints = Mathf.Pow(p1.x - p2.x, 2) + Mathf.Pow(p1.z - p2.z, 2);
                distOrigin = Mathf.Pow(currentPosition.x - p1.x, 2) + Mathf.Pow(currentPosition.z - p1.z, 2);
                distDestination = Mathf.Pow(p2.x - destinationPoint.x, 2) + Mathf.Pow(p2.z - destinationPoint.z, 2);
                distanceTotal = distPoints + distOrigin + distDestination;
                if (distanceTotal < minDistance) {
                    minDistance = distanceTotal;
                    minPoint1 = p1;
                    minPoint2 = p2;
                }
            }
        }
        subPoints.Clear();
        subPoints.Add(minPoint1);
        subPoints.Add(minPoint2);
    }

    /// <summary>
    /// Generate mid-points between points and singlePoint with yDefined
    /// </summary>
    private List<Vector3> generateMidPoints(ref List<Vector3> points, Vector3 singlePoint, float yDefined, float ratio) {
        var midPoints = new List<Vector3>();
        points.ForEach(point => {
            var midPoint = point + (singlePoint - point) * ratio;
            midPoint.y = yDefined;
            midPoints.Add(midPoint);
        });
        return midPoints;
    }
}
