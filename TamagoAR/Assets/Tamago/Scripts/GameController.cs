using GoogleARCore;
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
        List<Vector3> boundaryPoints = new List<Vector3>();
        plane.GetBoundaryPolygon(boundaryPoints);
        Vector3 centroid = VectorUtils.FindCentroid(boundaryPoints);
        Vector3 direction = FirstPersonCamera.transform.position - centroid;
        GameObject character = GameObject.Instantiate(CharacterToPlace, centroid, Quaternion.LookRotation(direction));
        character.GetComponent<AguController>().FirstPersonCamera = FirstPersonCamera;
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


}
