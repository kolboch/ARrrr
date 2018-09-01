using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

    bool gameEnded = false;
    bool finishedLevel = false;
    public float endDelaySeconds = 2f;
    public GameObject completeLevelUI;    

    public void EndGame()
    {
        if(!gameEnded && !finishedLevel)
        {
            gameEnded = true;
            Debug.Log("End game called in GameManager!");
            Invoke("ResetGameState", endDelaySeconds);
        }
    }


    public void FinishLevel()
    {
        finishedLevel = true;
        completeLevelUI.SetActive(true);
    }

    private void ResetGameState()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
	
}
