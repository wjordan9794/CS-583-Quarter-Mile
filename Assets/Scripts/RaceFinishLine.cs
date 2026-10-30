using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class RaceFinishLine : MonoBehaviour
{
    [Header("Cars")]
    public PrometeoCarController playerCar;
    public NPCPrometeoCarController npcCar;

    [Header("Camera")]
    public CameraFollowerMine cameraFollowScript;

    [Header("UI")]
    public GameObject racingHUD;
    public GameObject resultsPanel;
    public TMP_Text resultText;
    public TMP_Text resultTimeText;

    [Header("Finish Sounds")]
    public AudioSource resultAudioSource;
    public AudioClip winSound;
    public AudioClip loseSound;

    [Header("Race Settings")]
    public float carsContinueTime = 2f;
    public float engineFadeDuration = 1.5f;

    private bool playerFinished;
    private bool npcFinished;
    private bool resultsShown;

    private float raceStartTime;
    private float playerFinishTime;
    private float npcFinishTime;

    void Start()
    {
        raceStartTime = Time.time;

        if (resultsPanel != null)
            resultsPanel.SetActive(false);

        if (resultAudioSource == null)
            resultAudioSource =
                GetComponent<AudioSource>();

        if (resultAudioSource != null)
        {
            resultAudioSource.playOnAwake = false;
            resultAudioSource.loop = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        PrometeoCarController player =
            other.GetComponentInParent<
                PrometeoCarController
            >();

        NPCPrometeoCarController npc =
            other.GetComponentInParent<
                NPCPrometeoCarController
            >();

        if (player != null &&
            player == playerCar &&
            !playerFinished)
        {
            PlayerCrossedFinish();
        }

        if (npc != null &&
            npc == npcCar &&
            !npcFinished)
        {
            npcFinished = true;
            npcFinishTime =
                Time.time - raceStartTime;
        }
    }

    void PlayerCrossedFinish()
    {
        playerFinished = true;

        playerFinishTime =
            Time.time - raceStartTime;

        if (cameraFollowScript != null)
            cameraFollowScript.enabled = false;

        if (racingHUD != null)
            racingHUD.SetActive(false);

        FadeOutCarAudio();
        ShowResults();

        StartCoroutine(StopCarsAfterDelay());
    }

    void ShowResults()
    {
        if (resultsShown)
            return;

        resultsShown = true;

        bool playerWon =
            !npcFinished ||
            playerFinishTime < npcFinishTime;

        if (playerWon)
            RaceProgression.RecordWin();
        else
            RaceProgression.RecordLoss();

        if (resultsPanel != null)
            resultsPanel.SetActive(true);

        if (resultText != null)
        {
            resultText.text =
                playerWon
                    ? "YOU WIN!"
                    : "YOU LOSE!";
        }

        if (resultTimeText != null)
        {
            resultTimeText.text =
                "TIME: " +
                playerFinishTime.ToString("F2") +
                " seconds\n" +
                "WINS: " +
                RaceProgression.Wins +
                "   LOSSES: " +
                RaceProgression.Losses +
                "\nLEVEL: " +
                RaceProgression.Level;
        }

        PlayFinishSound(playerWon);
    }

    void PlayFinishSound(bool playerWon)
    {
        if (resultAudioSource == null)
            return;

        AudioClip sound =
            playerWon
                ? winSound
                : loseSound;

        if (sound != null)
            resultAudioSource.PlayOneShot(sound);
    }

    void FadeOutCarAudio()
    {
        if (playerCar != null)
        {
            EngineAudioBySpeed audio =
                playerCar.GetComponentInChildren<
                    EngineAudioBySpeed
                >();

            if (audio != null)
                audio.FadeOutEngine(
                    engineFadeDuration
                );
        }

        if (npcCar != null)
        {
            EngineAudioBySpeed audio =
                npcCar.GetComponentInChildren<
                    EngineAudioBySpeed
                >();

            if (audio != null)
                audio.FadeOutEngine(
                    engineFadeDuration
                );
        }
    }

    IEnumerator StopCarsAfterDelay()
    {
        yield return new WaitForSeconds(
            carsContinueTime
        );

        if (playerCar != null)
        {
            playerCar.enabled = false;

            Rigidbody body =
                playerCar.GetComponent<Rigidbody>();

            if (body != null)
                body.linearVelocity = Vector3.zero;
        }

        if (npcCar != null)
            npcCar.StopDriving();
    }

    public void RestartRace()
    {
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }
}