using System.Collections;
using UnityEngine;
using TMPro;

public class RaceCountdown : MonoBehaviour
{
    public PrometeoCarController playerCar;
    public NPCPrometeoCarController npcCar;
    public Rigidbody playerRigidbody;
    public TMP_Text countdownText;

    void Start()
    {
        if (playerRigidbody == null)
            playerRigidbody = playerCar.GetComponent<Rigidbody>();

        playerCar.enabled = false;
        playerRigidbody.isKinematic = true;

        StartCoroutine(Countdown());
    }

    IEnumerator Countdown()
    {
        countdownText.text = "3";
        yield return new WaitForSeconds(1f);

        countdownText.text = "2";
        yield return new WaitForSeconds(1f);

        countdownText.text = "1";
        yield return new WaitForSeconds(1f);

        countdownText.text = "GO!";

        playerRigidbody.isKinematic = false;
        playerCar.enabled = true;
        npcCar.StartDriving();

        yield return new WaitForSeconds(1f);
        countdownText.text = "";
    }
}