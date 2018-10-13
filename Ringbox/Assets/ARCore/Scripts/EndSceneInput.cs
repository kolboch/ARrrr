using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndSceneInput : MonoBehaviour {

	public void HandleYesInput() {
        StartCoroutine(QuitWithDelay());
    }

    private IEnumerator QuitWithDelay() {
        yield return new WaitForSeconds(3);
        Application.Quit();
    }
}
