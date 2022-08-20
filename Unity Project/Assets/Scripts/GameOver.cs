using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    [SerializeField] Text finalScore, endText;
    [SerializeField] PlayerController pc;

    // Disables all of the UI and the script because the game has just started
    private void Start()
    {
        finalScore.gameObject.SetActive(false);
        endText.gameObject.SetActive(false);
        enabled = false;
    }

    // When necessary, script turns on to show text and enable its update method
    public void ShowText()
    {
        enabled = true;
        finalScore.text = "FINAL SCORE: " + pc.score;
        finalScore.gameObject.SetActive(true);
        endText.gameObject.SetActive(true);
    }

    // Based on a key press, game is unpaused and either the game or the menu has its scene loaded
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            SceneManager.LoadScene(1);
            Time.timeScale = 1;
        }

        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene(0);
            Time.timeScale = 1;
        }
    }
}