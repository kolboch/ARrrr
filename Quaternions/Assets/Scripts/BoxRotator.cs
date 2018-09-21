using System.Collections;
using UnityEngine;

public class BoxRotator : MonoBehaviour {

    public Transform Cube;
    public Transform CyllinderRed;
    public Transform CyllinderBlue;
    public Vector3 Origin = new Vector3(0, 1.5f, 0);
    public Vector3 OriginRotation = new Vector3(0, 0, 0);
    private bool rotateAroundOn = false;
    private bool rotateOn = false;
    private bool rotateTowardsOn = false;
    private bool lerpOn = false;
    private bool slerpOn = false;

    public void ResetPosition() {
        Cube.position = Origin;
        Cube.rotation = Quaternion.Euler(OriginRotation);
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
        Cube.rotation = Quaternion.FromToRotation(Vector3.up, Cube.up);
    }

    public void RotateToggle() {
        if (rotateOn) {
            rotateOn = false;
        } else {
            rotateOn = true;
            StartCoroutine(RotateCoroutine());
        }
    }

    public void RotateTowardsToggle() {
        if (rotateTowardsOn) {
            rotateTowardsOn = false;
        } else {
            rotateTowardsOn = true;
            StartCoroutine(RotateTowardsCoroutine());
        }
    }

    public void LerpToggle() {
        if (lerpOn) {
            lerpOn = false;
        } else {
            lerpOn = true;
            StartCoroutine(LerpCoroutine());
        }
    }

    public void SlerpToggle() {
        if (slerpOn) {
            slerpOn = false;
        } else {
            slerpOn = true;
            StartCoroutine(SlerpCoroutine());
        }
    }

    private IEnumerator RotateAround() {
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
        Vector3 relativePosition = CyllinderRed.position - Cube.position;
        relativePosition.y = 0;
        Cube.rotation = Quaternion.LookRotation(relativePosition, Vector3.up);
        yield return new WaitForSeconds(3);
        Cube.rotation = Quaternion.LookRotation(relativePosition, Vector3.left);
        yield return new WaitForSeconds(3);
        Cube.rotation = Quaternion.LookRotation(relativePosition, Vector3.right);
    }

    private IEnumerator RotateCoroutine() {
        // by default rotation is around local axes, same like Space.self
        // good example of Space.World with look rotation
        while (true) {
            Cube.Rotate(Vector3.up * Time.deltaTime * 45, Space.World);
            if (rotateOn) {
                yield return null;
            } else {
                yield break;
            }
        }
    }

    private IEnumerator RotateTowardsCoroutine() {
        while (true) {
            if (rotateTowardsOn) {
                Cube.rotation = Quaternion.RotateTowards(Cube.rotation, Quaternion.Euler(Vector3.up), 15 * Time.deltaTime);
                yield return null;
            } else {
                yield break;
            }
        }
    }

    private IEnumerator LerpCoroutine() {
        float speed = 0.5f;
        while (true) {
            if (lerpOn) {
                Cube.rotation = Quaternion.Lerp(Cube.rotation, CyllinderBlue.rotation, speed * Time.deltaTime);
                yield return null;
            } else {
                yield break;
            }
        }
    }

    // Slerp has additionaly to Lerp Ease in ease out behaviour
    private IEnumerator SlerpCoroutine() {
        var speed = 0.5f;
        while (true) {
            if (slerpOn) {
                Cube.rotation = Quaternion.Slerp(Cube.rotation, CyllinderBlue.rotation, speed * Time.deltaTime);
                yield return null;
            } else {
                yield break;
            }
        }
    }
}
