using System.Collections;
using UnityEngine;

public class BoxRotator : MonoBehaviour {

    public Transform Cube;
    public Vector3 Origin = new Vector3(0, 0.5f, 0);
    private bool rotateAroundOn = false;

    public void ResetPosition() {
        Cube.transform.position = Origin;
    }

    public void RotateAroundToggle() {
        if (rotateAroundOn) {
            rotateAroundOn = false;
        } else {
            rotateAroundOn = true;
            StartCoroutine(RotateAround());
        }
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
}
