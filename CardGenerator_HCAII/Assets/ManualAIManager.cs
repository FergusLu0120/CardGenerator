using System;
using System.Collections;
using System.Collections.Generic;
using Card;
using extOSC;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ManualAIManager : MonoBehaviour
{
    [SerializeField] private OSCReceiver receiver;

    [SerializeField] private CardGeneratorConnector cardGeneratorConnector;
    [SerializeField] private CardCreator cardCreator;
        [SerializeField] private GPTConnector gptConnector;
    [SerializeField] private SDConnector sdConnector;


    [SerializeField] private TextMeshProUGUI strengthValueText;
    [SerializeField] private TextMeshProUGUI speedValueText;
    [SerializeField] private TextMeshProUGUI skillValueText;
    private float strengthValue = 0f;
    public float StrengthValue
    {
        get { return strengthValue; }
        set
        {
            strengthValue = Mathf.RoundToInt(value);
            strengthValueText.text = $"{strengthValue}%";
        }
    }
    private float speedValue = 0f;
    public float SpeedValue
    {
        get { return speedValue; }
        set
        {
            speedValue = Mathf.RoundToInt(value);
            speedValueText.text = $"{speedValue}%";
        }
    }
    private float skillValue = 0f;
    public float SkillValue
    {
        get { return skillValue; }
        set
        {
            skillValue = Mathf.RoundToInt(value);
            skillValueText.text = $"{skillValue}%";
        }
    }

    private string timestamp;
    private ManualAIData mad;

    private void Start()
    {
        receiver.Bind("/ManualAIData", ReceiveManualAIData);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            GenerateCard();
        }
    }

    private void ReceiveManualAIData(OSCMessage msg)
    {
        // DateTime timestamp = DateTime.ParseExact(msg.Values[0].StringValue, "MM/dd/yyyy hh:mm:ss tt", null);

        // Debug.Log(msg.Values[0].StringValue);
        // Debug.Log(timestamp);
        // string timeStamp = string.Format("{0}/{1}/{2} {3}:{4}",
        //         timestamp.Year.ToString("0000"),
        //         timestamp.Month.ToString("00"),
        //         timestamp.Day.ToString("00"),
        //         timestamp.Hour.ToString("00"),
        //         timestamp.Minute.ToString("00")
        //         );
        timestamp = msg.Values[0].StringValue;
        Debug.Log("ManualAIData Received at " + timestamp);
        string json = msg.Values[1].StringValue;
        mad = JsonUtility.FromJson<ManualAIData>(json);
        Debug.Log($"ManualAIData Value: {mad.StrengthRanking}, {mad.SpeedRanking}, {mad.SkillRanking}");
    }

    private void GenerateCard()
    {
        float currentStrengthPR = mad.StrengthRanking;
        float currentSpeedPR = mad.SpeedRanking;
        float currentSkillPR = mad.SkillRanking;

        cardCreator.Score = -1;
        cardCreator.TimeStamp = GetCurrentTimeStamp();

        cardCreator.StrengthData = -1;
        cardCreator.SpeedData = -1f;
        cardCreator.SkillData = -1f;

        cardCreator.StrengthRank = Mathf.RoundToInt(currentStrengthPR);
        cardCreator.SpeedRank = Mathf.RoundToInt(currentSpeedPR);
        cardCreator.SkillRank = Mathf.RoundToInt(currentSkillPR);

        //AI generation
        gptConnector.RequestAvatarContent(
            PromptsManager.GetPrompt(currentStrengthPR, PromptsManager.StrengthTextPrompts),
            PromptsManager.GetPrompt(currentSpeedPR, PromptsManager.SpeedTextPrompts),
            PromptsManager.GetPrompt(currentSkillPR, PromptsManager.SkillTextPrompts),
            currentStrengthPR.ToString(),
            currentSpeedPR.ToString(),
            currentSkillPR.ToString());

        sdConnector.RequestAvatarImage(
            PromptsManager.GetPrompt(currentStrengthPR,
            PromptsManager.StrengthImagePrompts) + ", " +
            PromptsManager.GetPrompt(currentSpeedPR,
            PromptsManager.SpeedImagePrompts) + ", " +
            PromptsManager.GetPrompt(currentSkillPR,
            PromptsManager.SkillImagePrompts),
            GetFileNameByTimeStamp()
            );
        CardMaster.Instance.SystemState = CardSystemStateType.Generating;
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
        private string GetFileNameByTimeStamp()
        {
            string timeStamp = string.Format("{0}{1}{2}_{3}{4}{5}",
                DateTime.Now.Year.ToString("0000"),
                DateTime.Now.Month.ToString("00"),
                DateTime.Now.Day.ToString("00"),
                DateTime.Now.Hour.ToString("00"),
                DateTime.Now.Minute.ToString("00"),
                DateTime.Now.Second.ToString("00")
                );
            return timeStamp;
        }

    // public void SendDataToServer()
    // {
    //     ManualAIData data = new ManualAIData();
    //     data.StrengthRanking = StrengthValue;
    //     data.SpeedRanking = SpeedValue;
    //     data.SkillRanking = SkillValue;
    //     string fileJson = JsonUtility.ToJson(data);
    //     cardGeneratorConnector.Transmitter.RemoteHost = cardGeneratorConnector.ServerIP;
    //     cardGeneratorConnector.Transmitter.Connect();
    //     OSCMessage msg = new OSCMessage("/ManualAIData");
    //     msg.AddValue(OSCValue.String(DateTime.Now.ToString()));
    //     msg.AddValue(OSCValue.String(fileJson));
    //     cardGeneratorConnector.Transmitter.Send(msg);
    // }
    public class ManualAIData
    {
        public float StrengthRanking;
        public float SpeedRanking;
        public float SkillRanking;

    }
}
