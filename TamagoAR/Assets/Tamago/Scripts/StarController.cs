using System.Collections;
using UnityEngine;

public class StarController : MonoBehaviour {

    public float minDelay = 10f;
    public float maxDelay = 30f;

    private void Start() {
        StartCoroutine(DestructAfterDelay());
    }

    void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag(Tags.TAG_CHARACTER)) {
            other.gameObject.GetComponent<AguController>().OnStarCollected();
            Destroy(gameObject);
        }
    }

    private IEnumerator DestructAfterDelay() {
        yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));
        Destroy(gameObject);
    }

}
