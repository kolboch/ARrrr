using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InputValidator : MonoBehaviour {

    private string password = "2410";
    public InputField field;

    void Start() {
        field.onEndEdit.AddListener(delegate { CheckIfPasswordIsValid(field.text); });
    }

    public void CheckIfPasswordIsValid(string input) {
        if(input.Equals(password)) {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}
