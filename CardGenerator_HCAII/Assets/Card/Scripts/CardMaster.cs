using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Android;
using UnityEditorInternal;

namespace Card
{
    public class CardMaster : MonoBehaviour
    {
        public static CardMaster Instance;
        [SerializeField] private DataAnalyzer dataAnalyzer;
        [SerializeField] private CardScanner cardScanner;
        [SerializeField] private CardCreator cardCreator;
        [SerializeField] private GPTConnector gptConnector;
        [SerializeField] private SDConnector sdConnector;
        [SerializeField] private OSCServer oscServer;

        private string previousCardPath;

        [SerializeField] private GameObject serverCamera;
        [SerializeField] private Camera saveCardCamera;
        //Card Canvas
        [SerializeField] private GameObject blackGround;
        [SerializeField] private GameObject generatingText;
        [SerializeField] private GameObject generationCompleteText;
        [SerializeField] private Canvas cardCanvas;
        [SerializeField] private RectTransform progressBar;
        [SerializeField] private GameObject restartText;
        [SerializeField] private GameObject printText;

        private string AssetsPath = Application.dataPath;
        [SerializeField] private string mpdataPath = "/Card/mpdata/";
        public string MPDataPath
        {
            get
            {
                return AssetsPath + mpdataPath;
            }
        }
        [SerializeField] private string preloadedJsonPath = "/Card/PreloadedJson.json";
        public string PreloadedJsonPath
        {
            get
            {
                return AssetsPath + preloadedJsonPath;
            }
        }
        private string receivedDataPath = Application.streamingAssetsPath;
        public string ReceivedDataPath
        {
            get { return receivedDataPath; }
        }
        [SerializeField] private string sdImagePath = "/Card/SDImages/";
        public string SDImagePath
        {
            get
            {
                return AssetsPath + sdImagePath;
            }
        }
        [SerializeField] private string autoSavePath = "/Card/CardAutoSave/";
        [SerializeField] private string manualSavePath = "/Card/CardManualSave/";
        [SerializeField] private string experimentSavePath = "/Card/CardExpSave/";
        [SerializeField] private string fakeCardPath = "/Card/FakeCard/";
        private string currentDataName;

        private CardSystemStateType systemState = CardSystemStateType.Off;

        private float currentStrengthPR = 0f;
        private float currentSpeedPR = 0f;
        private float currentSkillPR = 0f;

        public CardSystemStateType SystemState
        {
            get { return systemState; }
            set
            {
                switch (systemState)
                {
                    case CardSystemStateType.Off:
                        cardScanner.ScanState = ScanStateType.Off;
                        break;
                    case CardSystemStateType.Generating:
                        blackGround.gameObject.SetActive(false);
                        generatingText.gameObject.SetActive(false);
                        progressBar.gameObject.SetActive(false);
                        sdConnector.OnGenerationProgressUpdated -= UpdateProgressBar;
                        gptConnector.OnGenerationCompleted -= SetGetneratedContent;
                        break;
                    case CardSystemStateType.GenerationComplete:
                        blackGround.gameObject.SetActive(false);
                        generationCompleteText.gameObject.SetActive(false);
                        break;
                    case CardSystemStateType.Showing:
                        blackGround.gameObject.SetActive(false);
                        break;
                    case CardSystemStateType.ShowComplete:
                        blackGround.gameObject.SetActive(false);
                        cardCanvas.gameObject.SetActive(false);
                        break;
                }
                systemState = value;
                switch (systemState)
                {
                    case CardSystemStateType.Prestart:
                        cardScanner.ScanState = ScanStateType.Prescan;
                        Reset();
                        break;
                    case CardSystemStateType.Generating:
                        blackGround.gameObject.SetActive(true);
                        generatingText.gameObject.SetActive(true);
                        RectTransform rt = progressBar.transform as RectTransform;
                        rt.sizeDelta = new Vector2(0f, 10f);
                        progressBar.gameObject.SetActive(true);
                        sdConnector.OnGenerationProgressUpdated += UpdateProgressBar;
                        gptConnector.OnGenerationCompleted += SetGetneratedContent;
                        break;
                    case CardSystemStateType.GenerationComplete:
                        blackGround.gameObject.SetActive(true);
                        generationCompleteText.gameObject.SetActive(true);
                        serverCamera.SetActive(true);
                        break;
                    case CardSystemStateType.Showing:
                        blackGround.gameObject.SetActive(true);
                        cardCanvas.gameObject.SetActive(true);
                        StartCoroutine(ShowCardProcedure());
                        break;
                    case CardSystemStateType.ShowComplete:
                        blackGround.gameObject.SetActive(true);
                        cardCanvas.gameObject.SetActive(true);
                        restartText.gameObject.SetActive(true);
                        printText.gameObject.SetActive(true);
                        break;
                    case CardSystemStateType.Off:
                        blackGround.gameObject.SetActive(false);
                        progressBar.gameObject.SetActive(false);
                        generatingText.gameObject.SetActive(false);
                        generationCompleteText.gameObject.SetActive(false);
                        restartText.gameObject.SetActive(false);
                        printText.gameObject.SetActive(false);
                        cardCanvas.gameObject.SetActive(false);
                        serverCamera.gameObject.SetActive(false);
                        break;
                }
            }
        }

        private void Awake()
        {
            if (CardMaster.Instance == null)
            {
                CardMaster.Instance = this;
            }
            else
            {
                GameObject.Destroy(this);
            }

            Reset();
            cardScanner.ScanState = ScanStateType.Prescan;
        }

        private void Update()
        {
            switch (SystemState)
            {
                case CardSystemStateType.Generating:
                    if (sdConnector.IsGenerating == false && gptConnector.IsGenerating == false)
                    {
                        SystemState = CardSystemStateType.GenerationComplete;
                    }
                    break;
                case CardSystemStateType.GenerationComplete:
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        SystemState = CardSystemStateType.Showing;
                        string saveFileName = string.Format("Card_{0}.png", currentDataName);
                        string savePath = AssetsPath + autoSavePath;
                        SaveCard(savePath, saveFileName);
                    }
                    break;
                case CardSystemStateType.ShowComplete:
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        Restart();
                    }
                    break;
            }

            //data Analyzer
            if (Input.GetKeyDown(KeyCode.R))
            {
                dataAnalyzer.LoadAndSaveData();
            }

            //OSC Connector
            if (Input.GetKeyDown(KeyCode.Z))
            {
                oscServer.PingAllIP();
            }
            if (Input.GetKeyDown(KeyCode.C))
            {
                oscServer.RequestLatestData();
            }
            if (Input.GetKeyDown(KeyCode.M))
            {
                oscServer.RequestTodayDatas();
            }

            //Debug
            if (Input.GetKeyDown(KeyCode.F))
            {
                GenerateFakeCards();
            }
            if (Input.GetKeyDown(KeyCode.T))
            {
                RegenerateTexts();
            }
            //Generate
            if (Input.GetKeyDown(KeyCode.G))
            {
                ShowResult();
            }

            //Save Card
            if (Input.GetKeyDown(KeyCode.S))
            {
                string saveFileName = string.Format("Card_{0}.png", currentDataName);
                string savePath = AssetsPath + manualSavePath;
                SaveCard(savePath, saveFileName);
                //Debug.Log(dataAnalyzer.GetPercentileRanking(4f, dataAnalyzer.MinReactionTimes, false));
                //Debug.Log(dataAnalyzer.GetPercentileRanking(0.3f, dataAnalyzer.MinReactionTimes, false));
            }

            if (Input.GetKeyDown(KeyCode.D))
            {
                string saveFileName = string.Format("Card_Exp_{0}.png", GetFileNameByTimeStamp());
                string savePath = AssetsPath + experimentSavePath;
                SaveCard(savePath, saveFileName);
            }
            //Print        
            if (Input.GetKeyDown(KeyCode.P))
            {
                PrintPreviousCard();
            }
        }
        public void Reset()
        {
            SystemState = CardSystemStateType.Off;
            cardScanner.ScanState = ScanStateType.Off;
        }

        private void SetGetneratedContent()
        {
            cardCreator.AvatarName = gptConnector.NameResult;
            cardCreator.AvatarStory = gptConnector.StoryResult;
        }

        public void ShowResult()
        {
            Reset();

            string latestFileName = CardUtilities.GetLatestFileName(ReceivedDataPath);
            string latestFilePath = ReceivedDataPath + "/" + latestFileName;
            if (latestFilePath == null)
            {
                Debug.LogError("No JSON files found in the specified directory.");
                return;
            }
            Debug.Log(latestFilePath);
            PlayerData latestPlayerData = CardUtilities.LoadSpecificPlayerData(latestFilePath);

            currentDataName = latestFileName;

            currentStrengthPR = dataAnalyzer.GetPercentileRanking(latestPlayerData.summary.totalPunchNum, dataAnalyzer.TotalPunchNums);
            currentSpeedPR = dataAnalyzer.GetPercentileRanking(latestPlayerData.summary.minReactionTime, dataAnalyzer.MinReactionTimes, false);
            currentSkillPR = dataAnalyzer.GetPercentileRanking(latestPlayerData.summary.hitRate, dataAnalyzer.HitRates);

            float playerScore = latestPlayerData.summary.score;
            cardCreator.Score = Mathf.RoundToInt(latestPlayerData.summary.score);
            cardCreator.TimeStamp = GetCurrentTimeStamp();

            cardCreator.StrengthData = Mathf.RoundToInt(latestPlayerData.summary.totalPunchNum);
            cardCreator.SpeedData = latestPlayerData.summary.minReactionTime;
            cardCreator.SkillData = latestPlayerData.summary.hitRate;

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
                currentDataName
                );
            SystemState = CardSystemStateType.Generating;
            // Invoke(() => { Debug.Break(); }, 1f);
            //SystemState = CardSystemStateType.GenerationComplete;
        }

        private void BreakForAIGen()
        {

        }

        private void UpdateProgressBar(float value)
        {
            RectTransform rt = progressBar.transform as RectTransform;
            rt.sizeDelta = new Vector2(1920f * sdConnector.Progress, 10f);
        }

        private IEnumerator ShowCardProcedure()
        {
            //show card animation
            yield return new WaitForSeconds(3f);
            SystemState = CardSystemStateType.ShowComplete;
        }

        public void SaveCard(string filePath, string fileName)
        {
            //string saveLocation = streamingAssetsPath + "/AvarCard/Card/";
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
            string savePath = filePath + fileName;
            //string.Format("Card_{0}.png", currentDataName);
            int width = 646;
            int height = 999;

            RenderTexture outputMap = new RenderTexture(width, height, 32, RenderTextureFormat.ARGB32);
            outputMap.name = "Whatever";
            outputMap.enableRandomWrite = true;
            outputMap.Create();
            RenderTexture.active = outputMap;

            // Set the target texture for the UI Camera
            saveCardCamera.gameObject.SetActive(true);
            //saveCardCamera.cullingMask = LayerMask.GetMask("AvarCard");
            //saveCardCamera.clearFlags = CameraClearFlags.SolidColor;
            //saveCardCamera.backgroundColor = Color.clear;
            saveCardCamera.depth = 100;
            saveCardCamera.targetTexture = outputMap;
            cardCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            cardCanvas.worldCamera = saveCardCamera;
            //mainCamera.
            serverCamera.SetActive(false);
            // Render the camera's view
            saveCardCamera.Render();

            // Create a new Texture2D with the same dimensions as the RenderTexture
            Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false);

            // Read the RenderTexture contents into the Texture2D
            texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            texture.Apply();

            // Encode the texture to PNG
            byte[] bytes = texture.EncodeToPNG();

            // Save the PNG file
            File.WriteAllBytes(savePath, bytes);

            // Clean up
            outputMap.Release();
            RenderTexture.active = null;
            saveCardCamera.targetTexture = null;
            cardCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            //shootingCamera.cullingMask = -1; //everything
            //saveCardCamera.cullingMask = LayerMask.GetMask("AvarCard");

            Debug.Log("Card saved to " + savePath);
            previousCardPath = savePath;
            saveCardCamera.gameObject.SetActive(false);
            serverCamera.SetActive(true);
        }

        private void PrintPreviousCard()
        {
            if (string.IsNullOrEmpty(previousCardPath))
            {
                //no previous card
                return;
            }
            System.Diagnostics.Process printProcess = new System.Diagnostics.Process();
            printProcess.StartInfo.FileName = previousCardPath;
            printProcess.StartInfo.Verb = "print";
            printProcess.StartInfo.CreateNoWindow = false;
            printProcess.Start();
            Debug.Log("Printing PNG...");
        }
        public void Restart()
        {
            SystemState = CardSystemStateType.Prestart;
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

        //Fake Card Testing
        private void GenerateFakeCards()
        {
            StartCoroutine(AutoGenerateFakeCards());
        }

        IEnumerator AutoGenerateFakeCards()
        {
            int fakeAmount = 1;
            for (int i = 0; i < fakeAmount; i++)
            {
                GenerateFakeCard();
                while (SystemState == CardSystemStateType.Generating)
                {
                    yield return null;
                }
                if (SystemState == CardSystemStateType.GenerationComplete)
                {
                    SystemState = CardSystemStateType.Showing;
                    string saveFileName = string.Format("Card_Fake_{0}.png", currentDataName);
                    string savePath = AssetsPath + fakeCardPath;
                    SaveCard(savePath, saveFileName);
                }
                while (SystemState == CardSystemStateType.Showing)
                {

                    yield return null;
                }
                if (SystemState == CardSystemStateType.ShowComplete)
                {
                    // SystemState = CardSystemStateType.Off;
                }
                yield return new WaitForSeconds(5f);
            }
        }

        public void GenerateFakeCard()
        {
            Reset();
            serverCamera.SetActive(true);

            float GetRandomDataWithin(List<float> stats)
            {
                int index = UnityEngine.Random.Range(0, stats.Count);
                return stats[index];
            }

            float fakeScore = GetRandomDataWithin(dataAnalyzer.Scores);
            float fakeTotalPunchNumber = GetRandomDataWithin(dataAnalyzer.TotalPunchNums);
            float fakeMinReactionTime = GetRandomDataWithin(dataAnalyzer.MinReactionTimes);
            float fakeHitRate = GetRandomDataWithin(dataAnalyzer.HitRates);
            Debug.Log(fakeTotalPunchNumber + ", " + fakeMinReactionTime + ", " + fakeHitRate);

            currentDataName = GetFileNameByTimeStamp();

            currentStrengthPR = dataAnalyzer.GetPercentileRanking(fakeTotalPunchNumber, dataAnalyzer.TotalPunchNums);
            //float strengthPercentileRanking = 99.9999f;
            currentSpeedPR = dataAnalyzer.GetPercentileRanking(fakeMinReactionTime, dataAnalyzer.MinReactionTimes, false);
            currentSkillPR = dataAnalyzer.GetPercentileRanking(fakeHitRate, dataAnalyzer.HitRates);

            float playerScore = fakeScore;
            cardCreator.Score = Mathf.RoundToInt(fakeScore);
            cardCreator.TimeStamp = GetCurrentTimeStamp();

            cardCreator.StrengthData = Mathf.RoundToInt(fakeTotalPunchNumber);
            cardCreator.SpeedData = fakeMinReactionTime;
            cardCreator.SkillData = fakeHitRate;

            cardCreator.StrengthRank = currentStrengthPR;
            cardCreator.SpeedRank = currentSpeedPR;
            cardCreator.SkillRank = currentSkillPR;

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
                currentDataName
                );
            SystemState = CardSystemStateType.Generating;

            //SystemState = CardSystemStateType.GenerationComplete;
        }

        //Regenerate Text
        private void RegenerateTexts()
        {
            gptConnector.RequestAvatarContent(
                PromptsManager.GetPrompt(currentStrengthPR,
                PromptsManager.StrengthTextPrompts),
                PromptsManager.GetPrompt(currentSpeedPR,
                PromptsManager.SpeedTextPrompts),
                PromptsManager.GetPrompt(currentSkillPR,
                PromptsManager.SkillTextPrompts),
                currentStrengthPR.ToString(),
                currentSpeedPR.ToString(),
                currentSkillPR.ToString());
            gptConnector.OnGenerationCompleted += SetGetneratedContent;
            gptConnector.OnGenerationCompleted += () =>
            {
                gptConnector.OnGenerationCompleted = null;
            };
        }
    }
}