using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Card;
using TMPro;
using System;
using UnityEngine.UI;

public class StatisticInterfceManager : MonoBehaviour
{
    [SerializeField] private DataAnalyzer dataAnalyzer;
    [SerializeField] private GameObject StatisticInterfaceCanvas;

    [SerializeField] private TextMeshProUGUI scoreText;
    public int Score
    {
        set { scoreText.text = value.ToString(); }
    }
    [SerializeField] private Text timeStampText;
    public string TimeStamp
    {
        set
        {
            timeStampText.text = value;
        }
    }
    [SerializeField] private TextMeshProUGUI strengthDataText;
    public int StrengthData
    {
        set { strengthDataText.text = value.ToString(); }
    }
    [SerializeField] private TextMeshProUGUI strengthRankText;
    public float StrengthRank
    {
        set
        {
            if (Mathf.Round(value) >= 100f)
            {
                strengthRankText.text = "100<size=15px>%</size>";
            }
            else
            {
                strengthRankText.text = string.Format("{0}<size=15px>%</size>", value.ToString("F1"));
            }
        }
    }
    [SerializeField] private TextMeshProUGUI speedDataText;
    public float SpeedData
    {
        set { speedDataText.text = string.Format("{0}s", value.ToString()); }
    }
    [SerializeField] private TextMeshProUGUI speedRankText;
    public float SpeedRank
    {
        set
        {
            if (Mathf.Round(value) >= 100f)
            {
                speedRankText.text = "100<size=15px>%</size>";
            }
            else
            {
                speedRankText.text = string.Format("{0}<size=15px>%</size>", value.ToString("F1"));
            }
        }
    }
    [SerializeField] private TextMeshProUGUI skillDataText;
    public float SkillData
    {
        set { skillDataText.text = string.Format("{0}%", value.ToString("F2")); }
    }
    [SerializeField] private TextMeshProUGUI skillRankText;
    public float SkillRank
    {
        set
        {
            if (Mathf.Round(value) >= 100f)
            {
                skillRankText.text = "100<size=15px>%</size>";
            }
            else
            {
                skillRankText.text = string.Format("{0}<size=15px>%</size>", value.ToString("F1"));
            }
        }
    }

    private void Update()
    {
        //Toggle Statistic Interface
        if (Input.GetKeyDown(KeyCode.K))
        {
            ToggleStatisticInterface();
        }
    }

    // Statistic Interface
    private void ToggleStatisticInterface()
    {
        StatisticInterfaceCanvas.SetActive(!StatisticInterfaceCanvas.activeSelf);
        if (StatisticInterfaceCanvas.activeSelf == false) return;

        string latestFileName = CardUtilities.GetLatestFileName(CardMaster.Instance.ReceivedDataPath);
        string latestFilePath = CardMaster.Instance.ReceivedDataPath + "/" + latestFileName;
        if (latestFilePath == null)
        {
            Debug.LogError("No JSON files found in the specified directory.");
            return;
        }
        Debug.Log(latestFilePath);
        PlayerData latestPlayerData = CardUtilities.LoadSpecificPlayerData(latestFilePath);

        float currentStrengthPR = dataAnalyzer.GetPercentileRanking(latestPlayerData.summary.totalPunchNum, dataAnalyzer.TotalPunchNums);
        float currentSpeedPR = dataAnalyzer.GetPercentileRanking(latestPlayerData.summary.minReactionTime, dataAnalyzer.MinReactionTimes, false);
        float currentSkillPR = dataAnalyzer.GetPercentileRanking(latestPlayerData.summary.hitRate, dataAnalyzer.HitRates);

        StrengthData = Mathf.RoundToInt(latestPlayerData.summary.totalPunchNum);
        SpeedData = latestPlayerData.summary.minReactionTime;
        SkillData = latestPlayerData.summary.hitRate;

        StrengthRank = Mathf.RoundToInt(currentStrengthPR);
        SpeedRank = Mathf.RoundToInt(currentSpeedPR);
        SkillRank = Mathf.RoundToInt(currentSkillPR);

        float playerScore = latestPlayerData.summary.score;
        Score = Mathf.RoundToInt(latestPlayerData.summary.score);
        TimeStamp = GetCurrentTimeStamp();
    }
    private string GetCurrentTimeStamp()
    {
        string timeStamp = string.Format("{0}/{1}/{2} {3}:{4}",
            DateTime.Now.Year.ToString("0000"),
            DateTime.Now.Month.ToString("00"),
            DateTime.Now.Day.ToString("00"),
            DateTime.Now.Hour.ToString("00"),
            DateTime.Now.Minute.ToString("00")
            );
        return timeStamp;
    }
}
