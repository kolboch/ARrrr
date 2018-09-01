using UnityEngine;

public class FinishTrigger : MonoBehaviour {

    public GameManager manager;

    private void OnTriggerEnter(Collider other)
    {
        manager.FinishLevel();
    }
}
