using GoogleARCore;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BoxOnClickAnimator : MonoBehaviour {

    private Animator animator;
    private bool nextSceneLaunched = false;

    private void Start() {
        animator = GetComponent<Animator>();
    }

    void Update() {
        Touch touch;
        if ((Input.touchCount > 0) && (touch = Input.GetTouch(0)).phase == TouchPhase.Began) {
            Ray cameraRay = Camera.current.ScreenPointToRay(touch.position);
            RaycastHit hit;
            if (Physics.Raycast(cameraRay.origin, cameraRay.direction, out hit)) {
                if(hit.transform == this.transform) {
                    Debug.Log("GOT HIT!!");
                    animator.Play("Open");
                    if(!nextSceneLaunched) {
                        StartCoroutine(PlayNextSceneAfterDelay());
                    }
                }
            }
        }
    }

    private IEnumerator PlayNextSceneAfterDelay() {
        yield return new WaitForSeconds(10);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}

