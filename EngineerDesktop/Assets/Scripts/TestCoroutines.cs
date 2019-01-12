using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCoroutines : MonoBehaviour {

    private IEnumerator FirstCoroutine;
    private IEnumerator SecondCoroutine;
    private IEnumerator ThirdCoroutine;
    private int counter = 0;
    
    private void Update() {
        if (Input.GetKeyDown(KeyCode.S)) {
            StartCoroutine("ParentCoroutine", ++counter);
        }
    }

    private IEnumerator ParentCoroutine(int counter) {
        Debug.Log("Starting parent" + counter);
        StopCoroutinesExecution();
        Debug.Log("Start first");
        yield return StartCoroutine(FirstCoroutine = FirstChild("First time " + counter));
        Debug.Log("Start second");
        yield return StartCoroutine(SecondCoroutine = SecondChild());
        Debug.Log("Start first 2");
        yield return StartCoroutine(FirstCoroutine = FirstChild("Second time " + counter));
        Debug.Log("Start third");
        yield return StartCoroutine(ThirdCoroutine = ThirdChild());
        Debug.Log("Start first 3");
        yield return StartCoroutine(FirstCoroutine = FirstChild("Third time " + counter));
    }

    private void StopCoroutinesExecution() {
        if (FirstCoroutine != null) {
            StopCoroutine(FirstCoroutine);
        }
        if (SecondCoroutine != null) {
            StopCoroutine(SecondCoroutine);
        }
        if (ThirdCoroutine != null) {
            StopCoroutine(ThirdCoroutine);
        }
    }

    private IEnumerator FirstChild(string startedCount = "") {
        if (SecondCoroutine == null) {
            Debug.Log("Second coroutine is null at this point.");
        }
        if (ThirdCoroutine == null) {
            Debug.Log("Third coroutine is null at this point.");
        }
        yield return new WaitForSeconds(1);
        Debug.Log("End first child." + startedCount);
    }

    private IEnumerator SecondChild() {
        yield return new WaitForSeconds(1);
        Debug.Log("End second child.");
    }

    private IEnumerator ThirdChild() {
        yield return new WaitForSeconds(1);
        Debug.Log("End third child.");
    }
}
