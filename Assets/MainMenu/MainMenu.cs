using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;
using WhatsInTheBox;

public class MainMenu : MonoBehaviour
{
    [SerializeField] bool buttonTrue;
    [SerializeField] GameEvent exitManagement;
    public Animator transition;

    public float transitionTime = 1f;

    public void PlayGame() {
        if (buttonTrue == true) {
            StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex + 1));
        }
    }

    public void doExitGame() {
            Application.Quit();
    }

    void Awake() {
        if (buttonTrue != true) {
            StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex + 1));
        }
    }

    IEnumerator LoadLevel(int levelIndex) {
        if (buttonTrue != true) {
            yield return new WaitForSeconds(transitionTime);
        }
        transition.SetTrigger("Start");
        if (buttonTrue == true) {
            yield return new WaitForSeconds(transitionTime);
            exitManagement.Invoke();
            //Destroy(gameObject);
        }
        SceneManager.LoadScene(levelIndex);
    }
}
