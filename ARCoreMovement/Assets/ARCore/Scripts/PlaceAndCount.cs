using GoogleARCore;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public interface IInputHandler {
    void HandleInput();
}

public class PlaceAndCount : MonoBehaviour, IInputHandler {

    public Camera FirstPersonCamera;
    public GameObject PrefabModel;
    private List<GameObject> TrackedObjects = new List<GameObject>();
    public Text CountUI;
    private float k_ModelRotation = 180.0f;

    public void HandleInput() {
        Touch touch;
        if (Input.touchCount < 1 || Input.GetTouch(0).phase != TouchPhase.Began) {
            return;
        }
        touch = Input.GetTouch(0);

        // Raycast against the location the player touched to search for planes.
        TrackableHit hit;
        TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon |
            TrackableHitFlags.FeaturePointWithSurfaceNormal;

        if (Frame.Raycast(touch.position.x, touch.position.y, raycastFilter, out hit)) {
            // Use hit pose and camera pose to check if hittest is from the
            // back of the plane, if it is, no need to create the anchor.
            if ((hit.Trackable is DetectedPlane) &&
                Vector3.Dot(FirstPersonCamera.transform.position - hit.Pose.position,
                    hit.Pose.rotation * Vector3.up) < 0) {
                Debug.Log("Hit at back of the current DetectedPlane");
            } else {
                // Choose the model for the Trackable that got hit.
                GameObject prefab;
                if (hit.Trackable is FeaturePoint) {
                    prefab = PrefabModel; //point prefab
                } else {
                    prefab = PrefabModel; // plane prefab
                }
                Debug.Log("Creating prefab in a second !!!");
                var prefabToPlace = Instantiate(prefab, hit.Pose.position, hit.Pose.rotation);

                // move above plane
                // float prefabOffsetY = prefabToPlace.GetComponent<Renderer>().bounds.size.y / 2;
                // Debug.Log("Move above plane offset: " + prefabOffsetY);
                // prefabToPlace.transform.position = prefabToPlace.transform.position + new Vector3(0, prefabOffsetY, 0);

                // Compensate for the hitPose rotation facing away from the raycast (i.e. camera).
                prefabToPlace.transform.Rotate(0, k_ModelRotation, 0, Space.Self);

                // Create an anchor to allow ARCore to track the hitpoint as understanding of the physical
                // world evolves.
                var anchor = hit.Trackable.CreateAnchor(hit.Pose);

                // Make model a child of the anchor.
                prefabToPlace.transform.parent = anchor.transform;

                //add object to list of created ones and update UI
                Debug.Log("Add to list!");
                TrackedObjects.Add(prefabToPlace);
                CountUI.text = "#: " + TrackedObjects.Count;
                Debug.Log("Count: " + TrackedObjects.Count);
            }
        }
    }
}
