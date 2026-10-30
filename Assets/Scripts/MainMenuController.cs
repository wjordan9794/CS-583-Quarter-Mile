using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public string raceSceneName = "RaceScene";

    public void StartRace()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            raceSceneName
        );
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}