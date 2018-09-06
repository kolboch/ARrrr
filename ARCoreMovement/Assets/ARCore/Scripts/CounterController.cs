using GoogleARCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CounterController : MonoBehaviour {

    public GameObject SearchingForPlaneUI;
    public GameObject CountUI;
    public PlaceAndCount InputHandler;
    private List<DetectedPlane> AllDetectedPlanes;
    private bool _IsQuitting = false;

    void Start() {
        AllDetectedPlanes = new List<DetectedPlane>();
    }

    // Update is called once per frame
    void Update() {
        _UpdateApplicationLifeCycle();

        Session.GetTrackables<DetectedPlane>(AllDetectedPlanes);
        bool showSearchingUI = !AllDetectedPlanes.Exists(plane => plane.TrackingState == TrackingState.Tracking);
        SearchingForPlaneUI.SetActive(showSearchingUI);

        InputHandler.HandleInput();
    }

    private void _UpdateApplicationLifeCycle() {
        // exit app on back pressed
        if (Input.GetKey(KeyCode.Escape)) {
            Application.Quit();
        }
        // Only allow the screen to sleep when not tracking.
        if (Session.Status != SessionStatus.Tracking) {
            const int lostTrackingSleepTimeout = 15;
            Screen.sleepTimeout = lostTrackingSleepTimeout;
        } else {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }
        if (_IsQuitting) { return; }

        // Quit if ARCore was unable to connect and give Unity some time for the toast to appear.
        if (Session.Status == SessionStatus.ErrorPermissionNotGranted) {
            AndroidUtils.ShowAndroidToastMessage("Camera permission is needed to run this application");
            _IsQuitting = true;
            StartCoroutine(_DoQuitWithDelay(0.5f));
        } else if (Session.Status.IsError()) {
            AndroidUtils.ShowAndroidToastMessage("ARCore connecting problem. Please start the app again.");
            _IsQuitting = true;
            StartCoroutine(_DoQuitWithDelay(0.5f));
        }
    }

    private IEnumerator _DoQuitWithDelay(float delay) {
        yield return new WaitForSeconds(delay);
    }

    // exits the application
    private void _DoQuit() {
        Application.Quit();
    }
}

