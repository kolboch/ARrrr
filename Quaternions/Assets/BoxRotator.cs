using System.Collections;
using UnityEngine;

public class BoxRotator : MonoBehaviour {

    public Transform Cube;
    public Transform Cyllinder;
    public Vector3 Origin = new Vector3(0, 1.5f, 0);
    public Vector3 OriginRotation = new Vector3(0, 0, 0);
    private bool rotateAroundOn = false;

    private void Update() {
        
    }

    public void ResetPosition() {
        Cube.transform.position = Origin;
        Cube.transform.rotation = Quaternion.Euler(OriginRotation);
    }

    public void RotateAroundToggle() {
        if (rotateAroundOn) {
            rotateAroundOn = false;
        } else {
            rotateAroundOn = true;
            StartCoroutine(RotateAround());
        }
    }

    public void LookRotation() {
        StartCoroutine(LookRotationCoroutine());
    }

    public void FromToRotatation() {
        Cube.transform.rotation = Quaternion.FromToRotation(Vector3.up, Cube.transform.up);
    }

    IEnumerator RotateAround() {
        Vector3 RotationPoint = Origin + new Vector3(1, 0, 1);
        while (true) {
            Cube.RotateAround(RotationPoint, Vector3.up, 180 * Time.deltaTime);
            if (rotateAroundOn) {
                yield return null;
            } else {
                yield break;
            }
        }
    }

    private IEnumerator LookRotationCoroutine() {
        // forward z axis will point into provided direction
        Vector3 relativePosition = Cyllinder.transform.position - Cube.transform.position;
        relativePosition.y = 0;
        Cube.transform.rotation = Quaternion.LookRotation(relativePosition, Vector3.up);
        yield return new WaitForSeconds(3);
        Cube.transform.rotation = Quaternion.LookRotation(relativePosition, Vector3.left);
        yield return new WaitForSeconds(3);
        Cube.transform.rotation = Quaternion.LookRotation(relativePosition, Vector3.right);
    }
}
