using TMPro;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class RaceProgression : MonoBehaviour
{
    public static RaceProgression Instance;

    [Header("Player")]
    public PrometeoCarController playerCar;
    public ManualGearController playerGears;
    public EngineAudioBySpeed playerEngineAudio;

    [Header("NPC")]
    public NPCPrometeoCarController npcCar;
    public EngineAudioBySpeed npcEngineAudio;

    [Header("Progression Per Completed Race")]
    [Range(0f, 0.2f)]
    public float accelerationIncreasePerLevel = 0.03f;

    [Range(0f, 0.2f)]
    public float topSpeedIncreasePerLevel = 0.02f;

    [Header("Random Variation Per Race")]
    [Range(0f, 0.1f)]
    public float randomVariation = 0.02f;

    [Header("Optional Stats UI")]
    public TMP_Text winsText;
    public TMP_Text lossesText;
    public TMP_Text levelText;

    private const string WinsKey = "DragRace_Wins";
    private const string LossesKey = "DragRace_Losses";
    private const string LevelKey = "DragRace_Level";

    public static int Wins =>
        PlayerPrefs.GetInt(WinsKey, 0);

    public static int Losses =>
        PlayerPrefs.GetInt(LossesKey, 0);

    public static int Level =>
        PlayerPrefs.GetInt(LevelKey, 0);

    void Awake()
    {
        Instance = this;

        if (playerGears == null &&
            playerCar != null)
        {
            playerGears =
                playerCar.GetComponent<
                    ManualGearController
                >();
        }

        if (playerEngineAudio == null &&
            playerCar != null)
        {
            playerEngineAudio =
                playerCar.GetComponentInChildren<
                    EngineAudioBySpeed
                >();
        }

        if (npcEngineAudio == null &&
            npcCar != null)
        {
            npcEngineAudio =
                npcCar.GetComponentInChildren<
                    EngineAudioBySpeed
                >();
        }

        ApplyProgression();
    }

    void Start()
    {
        UpdateStatsUI();
    }

    void ApplyProgression()
    {
        float levelMultiplier =
            1f +
            Level * accelerationIncreasePerLevel;

        float topSpeedMultiplier =
            1f +
            Level * topSpeedIncreasePerLevel;

        float playerRandomMultiplier =
            1f +
            Random.Range(
                -randomVariation,
                randomVariation
            );

        float npcRandomMultiplier =
            1f +
            Random.Range(
                -randomVariation,
                randomVariation
            );

        if (playerGears != null)
        {
            ScaleArray(
                playerGears.gearAcceleration,
                levelMultiplier *
                playerRandomMultiplier
            );

            ScaleArray(
                playerGears.gearTopSpeeds,
                topSpeedMultiplier *
                playerRandomMultiplier
            );
        }

        if (playerEngineAudio != null &&
            playerGears != null)
        {
            playerEngineAudio.gearTopSpeeds =
                (float[])playerGears.gearTopSpeeds.Clone();
        }

        if (npcCar != null)
        {
            npcCar.motorTorque *=
                levelMultiplier *
                npcRandomMultiplier;

            ScaleArray(
                npcCar.gearTopSpeeds,
                topSpeedMultiplier *
                npcRandomMultiplier
            );

            npcCar.maxSpeedKmh =
                npcCar.gearTopSpeeds[
                    npcCar.gearTopSpeeds.Length - 1
                ];
        }

        if (npcEngineAudio != null &&
            npcCar != null)
        {
            npcEngineAudio.gearTopSpeeds =
                (float[])npcCar.gearTopSpeeds.Clone();
        }

        if (playerCar != null &&
            playerGears == null)
        {
            playerCar.maxSpeed =
                Mathf.RoundToInt(
                    playerCar.maxSpeed *
                    topSpeedMultiplier
                );

            playerCar.accelerationMultiplier =
                Mathf.RoundToInt(
                    playerCar.accelerationMultiplier *
                    levelMultiplier
                );
        }
    }

    void ScaleArray(
        float[] values,
        float multiplier
    )
    {
        if (values == null)
            return;

        for (int i = 1; i < values.Length; i++)
        {
            values[i] *= multiplier;
        }
    }

    public static void RecordWin()
    {
        PlayerPrefs.SetInt(
            WinsKey,
            Wins + 1
        );

        CompleteRace();
    }

    public static void RecordLoss()
    {
        PlayerPrefs.SetInt(
            LossesKey,
            Losses + 1
        );

        CompleteRace();
    }

    static void CompleteRace()
    {
        PlayerPrefs.SetInt(
            LevelKey,
            Level + 1
        );

        PlayerPrefs.Save();
    }

    void UpdateStatsUI()
    {
        if (winsText != null)
            winsText.text = "WINS: " + Wins;

        if (lossesText != null)
            lossesText.text = "LOSSES: " + Losses;

        if (levelText != null)
            levelText.text = "LEVEL: " + Level;
    }

    public void ResetProgression()
    {
        PlayerPrefs.DeleteKey(WinsKey);
        PlayerPrefs.DeleteKey(LossesKey);
        PlayerPrefs.DeleteKey(LevelKey);
        PlayerPrefs.Save();

        Debug.Log("Race progression reset.");
    }
}