using UnityEngine;

public class StarController : MonoBehaviour {

    void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag(Tags.TAG_CHARACTER)) {
            other.gameObject.GetComponent<AguController>().OnStarCollected();
            Destroy(gameObject);
        }
    }

}
