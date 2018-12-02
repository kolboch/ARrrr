using UnityEngine;

public class CarrotController : MonoBehaviour {

	void OnTriggerEnter(Collider other) {
		if (other.gameObject.CompareTag(Tags.TAG_CHARACTER)) {
			other.gameObject.GetComponent<AguController>().OnCarrotCollected();
			Destroy(gameObject);
		}
	}
}
