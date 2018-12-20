using System.Collections;
using GoogleARCore;
using UnityEngine;

public class CarrotController : MonoBehaviour
{
    public float updateYInterval = 5f;
    private DetectedPlane Plane;

    private void Start()
    {
        StartCoroutine(UpdateYPositionCoroutine());
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(Tags.TAG_CHARACTER))
        {
            other.gameObject.GetComponent<AguController>().OnCarrotCollected();
            ReleaseResources();
            Destroy(gameObject);
        }
    }

    public void SetPlane(DetectedPlane plane)
    {
        Plane = plane;
    }

    public void ReleaseResources()
    {
        StopAllCoroutines();
        Plane = null;
    }

    private IEnumerator UpdateYPositionCoroutine()
    {
        while (true)
        {
            if (Plane != null && Plane.TrackingState == TrackingState.Tracking)
            {
                var updatePosition = transform.position;
                updatePosition.y = Plane.CenterPose.position.y;
                transform.position = updatePosition;
            }

            yield return new WaitForSeconds(updateYInterval);
        }
    }
}