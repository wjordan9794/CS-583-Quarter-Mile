using System.Collections;
using UnityEngine;
using TMPro;

public class RaceCountdown : MonoBehaviour
{
    [Header("Cars")]
    public PrometeoCarController playerCar;
    public NPCPrometeoCarController npcCar;
    public Rigidbody playerRigidbody;

    [Header("Countdown UI")]
    public TMP_Text countdownText;
    public GameObject countdownBackground;

    [Header("Countdown Audio")]
    public AudioSource countdownAudioSource;
    public AudioClip countdownStartSound;

    private EngineAudioBySpeed playerEngineAudio;
    private EngineAudioBySpeed npcEngineAudio;

    void Start()
    {
        if (playerCar != null)
        {
            playerCar.enabled = false;

            if (playerRigidbody == null)
            {
                playerRigidbody =
                    playerCar.GetComponent<Rigidbody>();
            }

            playerEngineAudio =
                playerCar.GetComponentInChildren<
                    EngineAudioBySpeed
                >();

            if (playerEngineAudio != null)
                playerEngineAudio.StopEngine();
        }

        if (npcCar != null)
        {
            npcEngineAudio =
                npcCar.GetComponentInChildren<
                    EngineAudioBySpeed
                >();

            if (npcEngineAudio != null)
                npcEngineAudio.StopEngine();
        }

        if (playerRigidbody != null)
            playerRigidbody.isKinematic = true;

        if (countdownAudioSource != null)
        {
            countdownAudioSource.playOnAwake = false;
            countdownAudioSource.loop = false;
        }

        if (countdownBackground != null)
            countdownBackground.SetActive(true);

        if (countdownText != null)
            countdownText.text = "HOLD W TO START";

        StartCoroutine(WaitForPlayerToHoldW());
    }

    IEnumerator WaitForPlayerToHoldW()
    {
        while (!Input.GetKey(KeyCode.W))
            yield return null;

        yield return StartCoroutine(Countdown());
    }

    IEnumerator Countdown()
    {
        // Plays once when the countdown starts.
        if (countdownAudioSource != null &&
            countdownStartSound != null)
        {
            countdownAudioSource.PlayOneShot(
                countdownStartSound
            );
        }

        yield return new WaitForSeconds(0.5f);

        if (countdownText != null)
            countdownText.text = "3";

        yield return new WaitForSeconds(1f);

        if (countdownText != null)
            countdownText.text = "2";

        yield return new WaitForSeconds(1f);

        if (countdownText != null)
            countdownText.text = "1";

        yield return new WaitForSeconds(1f);

        if (countdownText != null)
            countdownText.text = "GO!";

        if (playerRigidbody != null)
            playerRigidbody.isKinematic = false;

        if (playerCar != null)
            playerCar.enabled = true;

        if (playerEngineAudio != null)
            playerEngineAudio.StartEngine();

        if (npcEngineAudio != null)
            npcEngineAudio.StartEngine();

        if (npcCar != null)
            npcCar.StartDriving();

        yield return new WaitForSeconds(1f);

        if (countdownText != null)
            countdownText.text = "";

        if (countdownBackground != null)
            countdownBackground.SetActive(false);
    }
}