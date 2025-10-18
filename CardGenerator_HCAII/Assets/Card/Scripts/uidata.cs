using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;


public class uidata : MonoBehaviour {
	private List<float> scores = new List<float>();
	private List<float> maxPunchSpeeds = new List<float>();
	private List<float> avgPunchSpeeds = new List<float>();
	private List<float> hitRates = new List<float>();
	private List<float> minReactionTimes = new List<float>();
	private List<float> avgReactionTimes = new List<float>();
	private List<float> totalPunchNums = new List<float>();

	public float[] cornerDistances = new float[6];

	public Text scoreText, agilityText, staminaText, skillText, luckText, focusText, strengthText, scorepercentText, hitpunch, hitrateText;

	public float focusI, luckI, staminaI, agilityI, skillI, strengthI;


	public GameObject strengthpic, agilitypic, focuspic, staminapic, luckpic, skillpic;
	public GameObject strengthcardbadge, agilitycardbadge, focuscardbadge, staminacardbadge, luckcardbadge, skillcardbadge;
	public Sprite[] strength;
	public Sprite[] agility;
	public Sprite[] focus;
	public Sprite[] stamina;
	public Sprite[] luck;
	public Sprite[] skill;

	private float[] strengthThresholds = { 101, 119, 135, 144 };
	//private float[] agilityThresholds = { 2.158f, 2.562f, 3.071f, 3.469f };
	//private float[] focusThresholds = { 0.370f, 0.394f, 0.469f, 0.6f };
	//private float[] luckThresholds = { 0.192f, 0.236f, 0.278f, 0.333f };
	private float[] agilityThresholds = { 2.158f, 2.562f, 3.071f, 3.469f };
	private float[] focusThresholds = { 0.6f, 0.469f, 0.394f, 0.370f };
	private float[] staminaThresholds = { 1.567f, 1.884f, 2.173f, 2.382f };
	private float[] luckThresholds = { 0.333f, 0.278f, 0.236f, 0.192f };
    private float[] skillThresholds = { 84.9f, 93.4f, 97.3f, 98.4f };

	private string[] strengthPrompts = { "", "Belt", "Bowtie", "Sunglasses", "Cloak" };
	private string[] agilityPrompts = { "Turtle", "Skate board", "Car", "Spaceship", "Wings" };
	private string[] focusPrompts = { "Snowland", "Volcano", "Desert", "Sky", "Universe" };
	private string[] staminaPrompts = { "White", "Yellow", "Purple", "Black", "Rainbow" };
	private string[] luckPrompts = { "", "Star", "Torch", "Clover", "Firework" };
	private string[] skillPrompts = { "", "Cap", "Cowboy hat", "Helmet", "Crown" };

	private string textToType;
	public float typingSpeed;

	private float scoreNum;
	private float hits;
	private float totalhits;
	private float hitrate;
	public float timeToCount;
	[SerializeField] private GameObject ServerCamera;
	[SerializeField] private Camera shootingCamera;
	[SerializeField] private Animator leftboardAnimator;
	[SerializeField] private Animator rightboardAnimator;
	[SerializeField] private List<Animator> radarAnimators = new List<Animator>();

	[SerializeField] private LineRenderer currentRadarLineRenderer;
	[SerializeField] private LineRenderer previousRadarLineRenderer;
	[SerializeField] private MeshFilter currentRadarMeshFilter;

	[SerializeField] private GameObject blackground;

	[SerializeField] private TypewriterEffect nameTypewriterEffect;

	[SerializeField] private TitleGPTConnector titleGPTConnector;
	[SerializeField] private StableDiffusionText2Image SDText2Image;
	[SerializeField] private StableDiffusionImage2Image SDImage2Image;


	[SerializeField] private Canvas avarCardCanvas;

	[SerializeField] private Image progressBar;
	[SerializeField] private Text WaitingText;
	[SerializeField] private Animator resultAnimator;
	[SerializeField] private Animator cardAnimator;

	[SerializeField] private Text cardTitleText;
	[SerializeField] private Text cardPromptsText;
	[SerializeField] private Text cardScoreText;
	[SerializeField] private Text cardTimeText;
	[SerializeField] private Text cardTrialText;
	[SerializeField] private RawImage qrRawImage;

	private string currentTimeStamp;
	private string showingTimeStamp;

	private AvarCardSummary previousCardSummary;

	[SerializeField] private GameObject restartText;
	[SerializeField] private GameObject printText;
	private bool isAbleToRestart = false;

	[SerializeField] private Text PrescanText;
	[SerializeField] private Text ScanningText;
	[SerializeField] private Text ScannedText;

	[SerializeField] private RawImage webcamDisplay;
	private WebCamTexture webCamTexture;

	public UnityEvent OnAvarCardCompleted;

	private string previousCardPath;


	public enum ScanStateType { Prescan, Scanning, Scanned, Off}
	private ScanStateType scanState = ScanStateType.Prescan;
	public ScanStateType ScanState {
		get { return scanState; }
		set { scanState = value;
			PrescanText.gameObject.SetActive(false);
			ScanningText.gameObject.SetActive(false);
			ScannedText.gameObject.SetActive(false);
			webcamDisplay.gameObject.SetActive(false);
			webCamTexture.Stop();
			switch (scanState) {
				case ScanStateType.Prescan:
				PrescanText.gameObject.SetActive(true);
				previousCardSummary = null;
				break;
				case ScanStateType.Scanning:
				ScanningText.gameObject.SetActive(true);
				webcamDisplay.gameObject.SetActive(true);
				webcamDisplay.texture = webCamTexture;
				webCamTexture.Play();
				StartCoroutine(ScanQRCode());

				break;
				case ScanStateType.Scanned:
				ScannedText.gameObject.SetActive(true);
				break;
			}
		}
	}

	private void Awake() {
		webCamTexture = new WebCamTexture(1920, 1080);
	}

    void Start() {
		//Debug
		Reset();
        PreloadDatas();
        //ScanState = ScanStateType.Off;
        ScanState = ScanStateType.Prescan;
        //ShowResult();
    }

    public void Reset() {
		progressBar.gameObject.SetActive(false);
		blackground.gameObject.SetActive(false);
		restartText.gameObject.SetActive(false);
		printText.gameObject.SetActive(false);
		ScanState = ScanStateType.Off;
		//previousCardSummary = null;
		leftboardAnimator.Play("Hide");
		rightboardAnimator.Play("Hide");
		for (int i = 0; i < radarAnimators.Count; i++) {
			radarAnimators[i].Play("Hide");
		}
	}

	public void Restart() {
		Reset();
		ServerCamera.SetActive(false);
		avarCardCanvas.gameObject.SetActive(false);
		OnAvarCardCompleted?.Invoke();
		ScanState = ScanStateType.Prescan;
	}

	public void StopScan() {
		ScanState = ScanStateType.Off;
	}

	public void PreloadDatas() {
		//read data
		//LoadData("Assets/ui/mpdata");
		string streamingAssetsPath = Application.streamingAssetsPath;
		string mpdataPath = streamingAssetsPath + "/AvarCard/mpdata/";
		LoadData(mpdataPath);
		SortAllLists();
	}

	public void ShowResult() {
		Reset();
		ServerCamera.SetActive(true);
		avarCardCanvas.gameObject.SetActive(true);
		string streamingAssetsPath = Application.streamingAssetsPath;
		

		//string folderPath = "Assets/ui/mpdata";
		string statisticPath = Application.streamingAssetsPath;
		string latestFileName = GetLatestFileName(statisticPath);
		string latestFilePath = statisticPath + "/" + latestFileName;
		if (latestFilePath == null) {
			Debug.LogError("No JSON files found in the specified directory.");
			return;
		}

		currentTimeStamp = latestFileName;


		Debug.Log($"Reading data from: {Path.GetFileName(latestFilePath)}");
		PlayerData latestPlayerData = LoadSpecificPlayerData(latestFilePath);

		PlayerData LoadSpecificPlayerData(string filePath) {
			string dataAsJson = File.ReadAllText(filePath);
			PlayerData playerData = JsonUtility.FromJson<PlayerData>(dataAsJson);
			
			return playerData;
		}

		//GM.UserData LoadUserData(string filePath) {
		//	string dataAsJson = File.ReadAllText(filePath);
		//	GM.UserData userData = JsonUtility.FromJson<GM.UserData>(dataAsJson);
		//	return userData;
		//}

		//GM.UserData ud = LoadUserData(latestFilePath);
		string showingName = string.Format("{0}/{1} {2}:{3}", DateTime.Now.Month.ToString("00"), DateTime.Now.Day.ToString("00"), DateTime.Now.Hour.ToString("00"), DateTime.Now.Minute.ToString("00"));
		nameTypewriterEffect.textToType = showingName;
		nameTypewriterEffect.StartTypeText();

        string timestamp = string.Format("{0}{1}_{2}{3}", DateTime.Now.Month.ToString("00"), DateTime.Now.Day.ToString("00"), 
			DateTime.Now.Hour.ToString("00"), DateTime.Now.Minute.ToString("00"));
		showingTimeStamp = timestamp;


        float latestPlayerScore = latestPlayerData.summary.score;
		scoreNum = Mathf.Round(latestPlayerScore);
		Debug.Log("score:" + latestPlayerScore);
		Debug.Log($"Latest player's score percentile: {CalculatePercentile(latestPlayerScore, scores)}");


		float latestPlayerMaxPunchSpeed = latestPlayerData.summary.maxPunchSpeed;
		agilityI = latestPlayerMaxPunchSpeed;
		agilityText.text = $"{Mathf.Round(CalculatePercentile(latestPlayerMaxPunchSpeed, maxPunchSpeeds))}%";
		Debug.Log("agilty:" + latestPlayerMaxPunchSpeed);
		cornerDistances[0] = CalculatePercentile(latestPlayerMaxPunchSpeed, maxPunchSpeeds);
		Debug.Log($"Latest player's Max Punch Speed percentile: {cornerDistances[0]}");


		float latestPlayerAvgPunchSpeed = latestPlayerData.summary.avgPunchSpeed;
		staminaI = latestPlayerAvgPunchSpeed;
		staminaText.text = $"{Mathf.Round(CalculatePercentile(latestPlayerAvgPunchSpeed, avgPunchSpeeds))}%";
		Debug.Log("stamina:" + latestPlayerAvgPunchSpeed);
		cornerDistances[1] = CalculatePercentile(latestPlayerAvgPunchSpeed, avgPunchSpeeds);
		Debug.Log($"Latest player's Avg Punch Speed percentile: {cornerDistances[1]}");


		float latestPlayerHitRate = latestPlayerData.summary.hitRate;
		skillI = latestPlayerHitRate;
		skillText.text = $"{Mathf.Round(CalculatePercentile(latestPlayerHitRate, hitRates))}%";
		Debug.Log("skill:" + latestPlayerHitRate);
		cornerDistances[2] = CalculatePercentile(latestPlayerHitRate, hitRates);
		Debug.Log($"Latest player's HitRate percentile: {cornerDistances[2]}");


		float latestPlayerMinReactionTime = latestPlayerData.summary.minReactionTime;
		luckI = latestPlayerMinReactionTime;
		luckText.text = $"{Mathf.Round(CalculatePercentile(latestPlayerMinReactionTime, minReactionTimes))}%";
		Debug.Log("luck range: " + minReactionTimes[0]+ ", " + minReactionTimes[minReactionTimes.Count - 1]);
		Debug.Log("luck:" + latestPlayerMinReactionTime);
		Debug.Log("luck percentile: " + CalculatePercentile(latestPlayerMinReactionTime, minReactionTimes));
		cornerDistances[3] = CalculatePercentile(latestPlayerMinReactionTime, minReactionTimes);
		Debug.Log($"Latest player's Min Reaction Time percentile: {cornerDistances[3]}");
		Debug.Log(latestPlayerMinReactionTime);

		float latestPlayerAvgReactionTime = latestPlayerData.summary.avgReactionTime;
		focusI = latestPlayerAvgReactionTime;
		focusText.text = $"{Mathf.Round(CalculatePercentile(latestPlayerAvgReactionTime, avgReactionTimes))}%";
		Debug.Log("focus:" + latestPlayerAvgReactionTime);
		cornerDistances[4] = CalculatePercentile(latestPlayerAvgReactionTime, avgReactionTimes);
		Debug.Log($"Latest player's Avg Reaction Time percentile: {cornerDistances[4]}");


		float latestPlayerTotalPunchNum = latestPlayerData.summary.totalPunchNum;
		strengthI = latestPlayerTotalPunchNum;
		strengthText.text = $"{Mathf.Round(CalculatePercentile(latestPlayerTotalPunchNum, totalPunchNums))}%";
		Debug.Log("strength:" + latestPlayerTotalPunchNum);
		cornerDistances[5] = CalculatePercentile(latestPlayerTotalPunchNum, totalPunchNums);
		Debug.Log($"Latest player's total Punch Num percentile: {cornerDistances[5]}");

		float totalHits = MathF.Round(latestPlayerTotalPunchNum / (latestPlayerHitRate / 100));
		if (latestPlayerHitRate == 0f) {
			totalHits = 0f;
        }
		Debug.Log($"Total Hits: {totalHits}");

		hits = Mathf.Round(latestPlayerTotalPunchNum);
		totalhits = totalHits;
		hitrate = MathF.Round(latestPlayerHitRate);

		textToType = $"You're ahead of {Mathf.Round(CalculatePercentile(latestPlayerScore, scores))}% of players.";

		Mesh hexagonMesh = CreateHexagonMesh();
		GetComponent<MeshFilter>().mesh = hexagonMesh;
		//DrawHexagon();
		List<float> percentileRanking = new List<float>();
		percentileRanking.Add(CalculatePercentile(latestPlayerTotalPunchNum, totalPunchNums) * 0.01f);
		percentileRanking.Add(CalculatePercentile(latestPlayerMaxPunchSpeed, maxPunchSpeeds) * 0.01f);
		percentileRanking.Add(CalculatePercentile(latestPlayerAvgReactionTime, avgReactionTimes) * 0.01f);
		percentileRanking.Add(CalculatePercentile(latestPlayerAvgPunchSpeed, avgPunchSpeeds) * 0.01f);
		percentileRanking.Add(CalculatePercentile(latestPlayerMinReactionTime, minReactionTimes) * 0.01f);
		percentileRanking.Add(CalculatePercentile(latestPlayerHitRate, hitRates) * 0.01f);

		//for (int i = 0; i < percentileRanking.Count; i++) {
		//	Debug.Log("hex " + i + " : " + percentileRanking[i]);
  //      }
		//PlotStats(currentRadarLineRenderer, percentileRanking);
		//CreateHexagonMesh(currentRadarMeshFilter, 200f, Color.yellow, new List<float> { 0.8f, 0.7f, 0.9f, 0.6f, 0.5f, 0.4f });
		CreateHexagonMesh(currentRadarMeshFilter, 200f, Color.yellow, percentileRanking);
		if (previousCardSummary != null) {
			previousRadarLineRenderer.gameObject.SetActive(true);
			//PlayerData pd = GetPlayerDataByTimeStamp(streamingAssetsPath, previousCardSummary.timestamp);
			string previousPath = statisticPath + "/" + previousCardSummary.timestamp;
			Debug.Log(previousRadarLineRenderer);
			PlayerData pd = JsonUtility.FromJson<PlayerData>(File.ReadAllText(previousPath));
			if (pd != null) {
				List<float> prevPercentileRanking = new List<float>();
				prevPercentileRanking.Add(CalculatePercentile(pd.summary.totalPunchNum, totalPunchNums) * 0.01f);
				prevPercentileRanking.Add(CalculatePercentile(pd.summary.maxPunchSpeed, maxPunchSpeeds) * 0.01f);
				prevPercentileRanking.Add(CalculatePercentile(pd.summary.avgReactionTime, avgReactionTimes) * 0.01f);
				prevPercentileRanking.Add(CalculatePercentile(pd.summary.avgPunchSpeed, avgPunchSpeeds) * 0.01f);
				prevPercentileRanking.Add(CalculatePercentile(pd.summary.minReactionTime, minReactionTimes) * 0.01f);
				prevPercentileRanking.Add(CalculatePercentile(pd.summary.hitRate, hitRates) * 0.01f);
				previousRadarLineRenderer.gameObject.SetActive(true);
				PlotStats(previousRadarLineRenderer, prevPercentileRanking);
            }
        } else {
			previousRadarLineRenderer.gameObject.SetActive(false);
        }

        StartCoroutine(CountToScore());
		StartCoroutine(CountPunch());
		StartCoroutine(CountToHitRate());
		StartCoroutine(TypeText());

		//badges
		//focuspic.GetComponent<Image>().sprite = GetSpriteByThreshold(focusI, focus, focusThresholds);//avgRT
		//luckpic.GetComponent<Image>().sprite = GetSpriteByThreshold(luckI, luck, luckThresholds);//minRT
		//staminapic.GetComponent<Image>().sprite = GetSpriteByThreshold(staminaI, stamina, staminaThresholds);//avgPS
		//agilitypic.GetComponent<Image>().sprite = GetSpriteByThreshold(agilityI, agility, agilityThresholds);//maxPS
		//skillpic.GetComponent<Image>().sprite = GetSpriteByThreshold(skillI, skill, skillThresholds);//hitRate
		//strengthpic.GetComponent<Image>().sprite = GetSpriteByThreshold(strengthI, strength, strengthThresholds);//TPN

		//focuscardbadge.GetComponent<Image>().sprite = GetSpriteByThreshold(focusI, focus, focusThresholds);//avgRT
		//luckcardbadge.GetComponent<Image>().sprite = GetSpriteByThreshold(luckI, luck, luckThresholds);//minRT
		//staminacardbadge.GetComponent<Image>().sprite = GetSpriteByThreshold(staminaI, stamina, staminaThresholds);//avgPS
		//agilitycardbadge.GetComponent<Image>().sprite = GetSpriteByThreshold(agilityI, agility, agilityThresholds);//maxPS
		//skillcardbadge.GetComponent<Image>().sprite = GetSpriteByThreshold(skillI, skill, skillThresholds);//hitRate
		//strengthcardbadge.GetComponent<Image>().sprite = GetSpriteByThreshold(strengthI, strength, strengthThresholds);//TPN

		strengthpic.GetComponent<Image>().sprite = GetBadges(CalculatePercentile(latestPlayerTotalPunchNum, totalPunchNums), strength);
		strengthcardbadge.GetComponent<Image>().sprite = GetBadges(CalculatePercentile(latestPlayerTotalPunchNum, totalPunchNums), strength);
		agilitypic.GetComponent<Image>().sprite = GetBadges(CalculatePercentile(latestPlayerMaxPunchSpeed, maxPunchSpeeds), agility);
		agilitycardbadge.GetComponent<Image>().sprite = GetBadges(CalculatePercentile(latestPlayerMaxPunchSpeed, maxPunchSpeeds), agility);
		focuspic.GetComponent<Image>().sprite = GetBadges(CalculatePercentile(latestPlayerAvgReactionTime, avgReactionTimes), focus);
		focuscardbadge.GetComponent<Image>().sprite = GetBadges(CalculatePercentile(latestPlayerAvgReactionTime, avgReactionTimes), focus);
		skillpic.GetComponent<Image>().sprite = GetBadges(CalculatePercentile(latestPlayerHitRate, hitRates), skill);
		skillcardbadge.GetComponent<Image>().sprite = GetBadges(CalculatePercentile(latestPlayerHitRate, hitRates), skill);
		luckpic.GetComponent<Image>().sprite = GetBadges(CalculatePercentile(latestPlayerMinReactionTime, minReactionTimes), luck);
		luckcardbadge.GetComponent<Image>().sprite = GetBadges(CalculatePercentile(latestPlayerMinReactionTime, minReactionTimes), luck);
		staminapic.GetComponent<Image>().sprite = GetBadges(CalculatePercentile(latestPlayerAvgPunchSpeed, avgPunchSpeeds), stamina);
		staminacardbadge.GetComponent<Image>().sprite = GetBadges(CalculatePercentile(latestPlayerAvgPunchSpeed, avgPunchSpeeds), stamina);

		//QR Code
		AvarCardSummary cardSummary = new AvarCardSummary();
		cardSummary.timestamp = currentTimeStamp;
		//cardSummary.score = latestPlayerScore;
		//cardSummary.totalPunchNum = latestPlayerTotalPunchNum;
		//cardSummary.maxPunchSpeed = latestPlayerMaxPunchSpeed;
		//cardSummary.avgPunchSpeed = latestPlayerAvgPunchSpeed;
		//cardSummary.avgReactionTime = latestPlayerAvgReactionTime;
		//cardSummary.hitRate = latestPlayerHitRate;
		//cardSummary.minReactionTime = latestPlayerMinReactionTime;
		//Debug.Log(latestPlayerAvgPunchSpeed);
		//Debug.Log((float)System.Math.Round((double)latestPlayerAvgPunchSpeed, 4));
		//cardSummary.stats.Add(latestPlayerScore.ToString());
		//cardSummary.stats.Add(latestPlayerTotalPunchNum.ToString());
		//cardSummary.stats.Add(latestPlayerMaxPunchSpeed.ToString("F4"));
		//cardSummary.stats.Add(latestPlayerAvgPunchSpeed.ToString("F4"));
		//cardSummary.stats.Add(latestPlayerAvgReactionTime.ToString("F4"));
		//cardSummary.stats.Add(latestPlayerHitRate.ToString("F4"));
		//cardSummary.stats.Add(latestPlayerMinReactionTime.ToString("F4"));
		if (previousCardSummary == null) {
			//cardSummary.stats.Add(1.ToString());
			cardSummary.trial = 1;
		} else {
			//cardSummary.stats.Add((previousCardSummary.stats[7] + 1).ToString());
			cardSummary.trial = previousCardSummary.trial + 1;
		}

		var barcodeWriter = new BarcodeWriter {
			Format = BarcodeFormat.QR_CODE,
			Options = new QrCodeEncodingOptions {
				Width = 256,
				Height = 256,
				Margin = 1, // Minimum margin
			}
		};
		//Debug.Log(JsonUtility.ToJson(cardSummary));
		var qrCodeColor = barcodeWriter.Write(JsonUtility.ToJson(cardSummary));
		Texture2D qrTex = new Texture2D(256, 256);
		qrTex.SetPixels32(qrCodeColor);
		qrTex.Apply();


		//Texture2D qrTex = GetQRCode(JsonUtility.ToJson(cardSummary));
		//Texture2D croppedTexture = Crop(qrTex, 30, 30, 30, 30);
		Texture2D finalText = ChangeBlackToPink(qrTex);

		qrRawImage.texture = finalText;

		//prompts
		List<string> prompts = new List<string>();
		string strengthPrompt = GetPrompts(CalculatePercentile(latestPlayerTotalPunchNum, totalPunchNums), strengthPrompts);
		prompts.Add(strengthPrompt);
		string agilityPrompt = GetPrompts(CalculatePercentile(latestPlayerMaxPunchSpeed, maxPunchSpeeds), agilityPrompts);
		prompts.Add(agilityPrompt);
		string focusPrompt = GetPrompts(CalculatePercentile(latestPlayerAvgReactionTime, avgReactionTimes), focusPrompts);
		prompts.Add(focusPrompt);
		string skillPrompt = GetPrompts(CalculatePercentile(latestPlayerHitRate, hitRates), skillPrompts);
		prompts.Add(skillPrompt);
		string staminaPrompt = GetPrompts(CalculatePercentile(latestPlayerAvgPunchSpeed, avgPunchSpeeds), staminaPrompts);
		prompts.Add(staminaPrompt);
		string luckPrompt = GetPrompts(CalculatePercentile(latestPlayerMinReactionTime, minReactionTimes), luckPrompts);
		prompts.Add(luckPrompt);

		string allPrompts = "";
		for (int i = 0; i < prompts.Count; i++) {
			if (string.IsNullOrEmpty(prompts[i]) == true) {
				continue;
			}
			if (string.IsNullOrEmpty(allPrompts) == false) {
				allPrompts += ", ";
			}
			allPrompts += prompts[i];
		}
		titleGPTConnector.RequestTitle(allPrompts);
		//Debug.Log(allPrompts);

		string imagePrompts = "<lora:fallguys:1.2>, cuteness render, Leather feel render, Sutures render, ";
		imagePrompts += string.Format("A {0} color 3D character, pill-shaped, no human, fall guys like, small mouth, small eyebrows, fat, wearing big boxing gloves, ", staminaPrompt);
		imagePrompts += string.Format("in {0} background, ", focusPrompt);

		if (string.IsNullOrEmpty(skillPrompt) != true) {
			imagePrompts += string.Format("Wearing {0}, ", skillPrompt);
		}
		if (string.Equals(agilityPrompt, "Wings") == true) {
			imagePrompts += string.Format("{0} on the back, ", agilityPrompt);
		} else {
			imagePrompts += string.Format("Riding on a {0}, ", agilityPrompt);
		}
		if (string.IsNullOrEmpty(strengthPrompt) != true) {
			imagePrompts += string.Format("Wearing {0}, ", strengthPrompt);
		}
		if (string.IsNullOrEmpty(luckPrompt) != true) {
			imagePrompts += string.Format("{0} decoration, ", luckPrompt);
		}
		string imageNegativePrompts = "animal, mammal, (human, man, human face:1.4), (hairs, nose, muscle), bad-hands-5, (multiple characters), words, error";

		//Debug.Log(imagePrompts);

		if (previousCardSummary == null) {
			SDText2Image.SavingImageName = string.Format("Image_{0}", currentTimeStamp);
			SDText2Image.prompt = imagePrompts;
			SDText2Image.negativePrompt = imageNegativePrompts;
			SDText2Image.Generate();
		} else {
			string saveLocation = streamingAssetsPath + "/AvarCard/SDImages/";
			string filename = saveLocation + string.Format("Image_{0}", previousCardSummary.timestamp) + ".png";
			Texture2D previousTex = LoadTexture(filename);
			if (previousTex != null) {
				SDImage2Image.SavingImageName = string.Format("Image_{0}", currentTimeStamp);
				SDImage2Image.inputTexture = previousTex;
				SDImage2Image.prompt = imagePrompts;
				SDImage2Image.negativePrompt = imageNegativePrompts;
				SDImage2Image.Generate();
			} else {
				SDText2Image.SavingImageName = string.Format("Image_{0}", currentTimeStamp);
				SDText2Image.prompt = imagePrompts;
				SDText2Image.negativePrompt = imageNegativePrompts;
				SDText2Image.Generate();
			}
		}


		cardPromptsText.text = allPrompts;
		cardScoreText.text = Mathf.Round(latestPlayerScore).ToString();
		cardTimeText.text = showingName;

		cardTrialText.text = "Trial " + cardSummary.trial.ToString();

		leftboardAnimator.Play("Show");
		rightboardAnimator.Play("Show");
		for (int i = 0; i < radarAnimators.Count; i++) {
			radarAnimators[i].Play("Show");
		}
		blackground.gameObject.SetActive(true);

		StartCoroutine(StartMonitorAIGenProgress());
	}

	private IEnumerator StartMonitorAIGenProgress() {
		RectTransform rt = progressBar.transform as RectTransform;
		WaitingText.gameObject.SetActive(false);
		progressBar.gameObject.SetActive(true);
		rt.sizeDelta = new Vector2(0f, 10f);
		if (previousCardSummary == null) {
			while (SDText2Image.Progress < 1f) {
				rt.sizeDelta = new Vector2(1920f * SDText2Image.Progress, rt.sizeDelta.y);
				yield return null;
			}
		} else {
			while (SDImage2Image.Progress < 1f) {
				rt.sizeDelta = new Vector2(1920f * SDImage2Image.Progress, rt.sizeDelta.y);
				yield return null;
			}
		}
		rt.sizeDelta = new Vector2(1920f, rt.sizeDelta.y);

		WaitingText.gameObject.SetActive(true);
		bool isWaitingInput = true;
		while (isWaitingInput) {
			if (Input.GetKeyDown(KeyCode.Space)) {
				isWaitingInput = false;
            }
			yield return null;
        }
		WaitingText.gameObject.SetActive(false);

		cardTitleText.text = titleGPTConnector.TitleResult;

		resultAnimator.SetTrigger("Hide");
		cardAnimator.gameObject.SetActive(true);
		cardAnimator.SetTrigger("Show");

		yield return new WaitForSeconds(5f);
		SaveCard();
		yield return new WaitForSeconds(3f);

		restartText.gameObject.SetActive(true);
		printText.gameObject.SetActive(true);
		isAbleToRestart = true;
	}

	public void SaveCard() {
		string streamingAssetsPath = Application.streamingAssetsPath;
		string saveLocation = streamingAssetsPath + "/AvarCard/Card/";
		if (!Directory.Exists(saveLocation)) {
			Directory.CreateDirectory(saveLocation);
		}
		string savePath = saveLocation + string.Format("Card_{0}.png", currentTimeStamp);
		int width = 600;
		int height = 950;

		RenderTexture outputMap = new RenderTexture(width, height, 32);
		outputMap.name = "Whatever";
		outputMap.enableRandomWrite = true;
		outputMap.Create();
		RenderTexture.active = outputMap;

		// Set the target texture for the UI Camera
		shootingCamera.gameObject.SetActive(true);
		shootingCamera.cullingMask = LayerMask.GetMask("AvarCard");
		shootingCamera.targetTexture = outputMap;
		//mainCamera.
		// Render the camera's view
		shootingCamera.Render();

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
		RenderTexture.active = null;
		shootingCamera.targetTexture = null;
		avarCardCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
		//shootingCamera.cullingMask = -1; //everything
		shootingCamera.cullingMask = LayerMask.GetMask("AvarCard", "Environment", "UI");

		Debug.Log("Card saved to " + savePath);
		previousCardPath = savePath;
	}

	Mesh CreateHexagonMesh() {
		Mesh mesh = new Mesh();

		Vector3[] vertices = new Vector3[7];
		int[] triangles = new int[18];
		Vector2[] uv = new Vector2[7];
		for (int i = 0; i < 6; i++) {
			float angleRad = Mathf.PI / 3 * i;
			vertices[i] = new Vector3(Mathf.Cos(angleRad) * cornerDistances[i], 0f, Mathf.Sin(angleRad) * cornerDistances[i]);
			uv[i] = new Vector2(vertices[i].x, vertices[i].z);
		}

		vertices[6] = Vector3.zero;
		uv[6] = new Vector2(0.5f, 0.5f);

		for (int i = 0; i < 6; i++) {
			triangles[i * 3] = 6;
			triangles[i * 3 + 1] = i;
			triangles[i * 3 + 2] = (i + 1) % 6;
		}

		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.uv = uv;

		mesh.RecalculateNormals();

		return mesh;
	}

	private void DrawHexagon() {
		// Draw the hexagon shape
		LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
		lineRenderer.positionCount = 7;
		lineRenderer.loop = true;
		lineRenderer.widthMultiplier = 2f;
		lineRenderer.useWorldSpace = false;
		lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
		lineRenderer.startColor = Color.black;
		lineRenderer.endColor = Color.black;

		float graphSize = 1f;

		float angle = 60f;
		for (int i = 0; i < 6; i++) {
			float x = graphSize * Mathf.Cos(Mathf.Deg2Rad * angle * i);
			float y = graphSize * Mathf.Sin(Mathf.Deg2Rad * angle * i);
			lineRenderer.SetPosition(i, new Vector3(x, y, 0));
		}
		lineRenderer.SetPosition(6, lineRenderer.GetPosition(0));
	}

	private void PlotStats(LineRenderer lineRenderer, List<float> stats) {
		// Plot the stats
		lineRenderer.positionCount = 7;
		lineRenderer.loop = false;
		lineRenderer.widthMultiplier = 0.01f;
		//lineRenderer.useWorldSpace = false;
		//lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
		//lineRenderer.startColor = Color.green;
		//lineRenderer.endColor = Color.green;

		float graphSize = 200f;

		float angle = 60f;
		for (int i = 0; i < 6; i++) {
			//Debug.Log(stats[i]);
			float y = stats[i] * graphSize * Mathf.Cos(Mathf.Deg2Rad * angle * i);
			float x = stats[i] * graphSize * Mathf.Sin(Mathf.Deg2Rad * angle * i);
			lineRenderer.SetPosition(i, new Vector3(x, y, 0));
		}
		lineRenderer.SetPosition(6, lineRenderer.GetPosition(0));
	}

	private void CreateHexagonMesh(MeshFilter meshFilter, float size, Color color, List<float> stats = null) {
		Mesh mesh = new Mesh();

		Vector3[] vertices = new Vector3[7];
		int[] triangles = new int[18]; // 6 triangles * 3 vertices each
		Color[] colors = new Color[7];

		float angle = 60f;
		vertices[0] = Vector3.zero; // Center vertex
		colors[0] = color;

		for (int i = 0; i < 6; i++) {
			float y = (stats == null ? 1 : stats[i]) * size * Mathf.Cos(Mathf.Deg2Rad * (angle * i));
			float x = (stats == null ? 1 : stats[i]) * size * Mathf.Sin(Mathf.Deg2Rad * (angle * i));
			vertices[i + 1] = new Vector3(x, y, 0);
			colors[i + 1] = color;
		}

		for (int i = 0; i < 6; i++) {
			triangles[i * 3] = 0; // Center vertex
			triangles[i * 3 + 1] = i + 1;
			triangles[i * 3 + 2] = i + 2 > 6 ? 1 : i + 2;
		}

		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.colors = colors;
		mesh.RecalculateNormals();

		meshFilter.mesh = mesh;
	}

	IEnumerator CountToScore() {
		float currentNumber = 0;
		float timePassed = 0;
		float rateOfChange;

		while (currentNumber < scoreNum) {
			timePassed += Time.deltaTime;
			float progress = timePassed / timeToCount;

			rateOfChange = Mathf.Pow(progress, 2);
			currentNumber = Mathf.Lerp(0, scoreNum, rateOfChange);

			scoreText.text = ((int)currentNumber).ToString();
			yield return null;
		}

		scoreText.text = scoreNum.ToString();
	}
	IEnumerator CountPunch() {
		int numerator = 0;
		int denominator = 0;
		float timePerIncrement = timeToCount / (hits + totalhits);

		while (numerator < hits || denominator < totalhits) {
			if (numerator < hits) {
				numerator++;
				Mathf.RoundToInt(numerator);
			}

			if (denominator < totalhits) {
				denominator++;
				Mathf.RoundToInt(denominator);
			}

			hitpunch.text = $"{numerator}/{denominator}";
			yield return new WaitForSeconds(timePerIncrement);
		}
	}
	IEnumerator CountToHitRate() {
		float currentNumber = 0;
		float timePassed = 0;
		float rateOfChange;

		while (currentNumber < hitrate) {
			timePassed += Time.deltaTime;
			float progress = timePassed / timeToCount;

			rateOfChange = Mathf.Pow(progress, 2);
			currentNumber = Mathf.Lerp(0, hitrate, rateOfChange);

			hitrateText.text = ((int)currentNumber).ToString() + "%";
			yield return null;
		}

		hitrateText.text = hitrate.ToString() + "%";
	}
	IEnumerator TypeText() {
		foreach (char letter in textToType.ToCharArray()) {
			scorepercentText.text += letter;
			yield return new WaitForSeconds(typingSpeed);
		}
	}

	void PrintPreviousCard() {
        //PrintDocument printDoc = new PrintDocument();
        //printDoc.PrinterSettings = new PrinterSettings();
        //printDoc.DefaultPageSettings.Landscape = false;
        //printDoc.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);

        //PaperSize paperSize = new PaperSize("CreditCard", 213, 339);
        //printDoc.DefaultPageSettings.PaperSize = paperSize;

        //printDoc.PrintPage += (sender, e) => {
        //	// Load the PNG image to print
        //	using (System.Drawing.Image img = System.Drawing.Image.FromFile(previousCardPath)) {
        //		// Print the image
        //		e.Graphics.DrawImage(img, e.PageBounds);
        //	}
        //};
        //printDoc.Print();


        System.Diagnostics.Process printProcess = new System.Diagnostics.Process();
        printProcess.StartInfo.FileName = previousCardPath;
		printProcess.StartInfo.Verb = "print";
        printProcess.StartInfo.CreateNoWindow = false;
        printProcess.Start();
        Debug.Log("Printing PNG...");
	}

	void Update() {
		//OnEnable?
		//	focuspic.GetComponent<Image>().sprite = GetSpriteByThreshold(focusI, focus, focusThresholds);//avgRT
		//	luckpic.GetComponent<Image>().sprite = GetSpriteByThreshold(luckI, luck, luckThresholds);//minRT
		//	staminapic.GetComponent<Image>().sprite = GetSpriteByThreshold(staminaI, stamina, staminaThresholds);//avgPS
		//	agilitypic.GetComponent<Image>().sprite = GetSpriteByThreshold(agilityI, agility, agilityThresholds);//maxPS
		//	skillpic.GetComponent<Image>().sprite = GetSpriteByThreshold(skillI, skill, skillThresholds);//hitRate
		//	strengthpic.GetComponent<Image>().sprite = GetSpriteByThreshold(strengthI, strength, strengthThresholds);//TPN
		if (isAbleToRestart) {
			if (Input.GetKeyDown(KeyCode.Space)) {
				Restart();
			}
			if (Input.GetKeyDown(KeyCode.P)) {
				PrintPreviousCard();
            }
		}
		switch (ScanState) {
			case ScanStateType.Prescan:
			if (Input.GetKeyDown(KeyCode.Return)) {
				ScanState = ScanStateType.Scanning;
			}
			break;
			case ScanStateType.Scanning:
			if (Input.GetKeyDown(KeyCode.Return)) {
				ScanState = ScanStateType.Prescan;
			}
			break;
			case ScanStateType.Scanned:
			if (Input.GetKeyDown(KeyCode.Backspace)) {
				ScanState = ScanStateType.Prescan;
			}
			break;
        }

		//debug
		if (Input.GetKeyDown(KeyCode.Tab)) {
			ShowResult();
		}

	}

	Sprite GetSpriteByThreshold(float value, Sprite[] sprites, float[] thresholds) {
		for (int i = 0; i < thresholds.Length; i++) {
			if (value < thresholds[i]) {
				return sprites[i];
			}
		}
		return sprites[sprites.Length - 1];
	}

	string GetLatestFileName(string folderPath) {
		var directoryInfo = new DirectoryInfo(folderPath);
		FileInfo[] files = directoryInfo.GetFiles("user_*.json");

		DateTime latestDate = DateTime.MinValue;
		FileInfo latestFile = null;

		foreach (var file in files) {
			Debug.Log(file.Name);
			Match match = Regex.Match(file.Name, @"user__(\d{4}_\d{1,2}_\d{1,2}_\d{1,2}_\d{1,2}_\d{1,2})\d+\.json");
			if (match.Success) {
				if (DateTime.TryParseExact(match.Groups[1].Value, "yyyy_M_d_H_m_s", null, System.Globalization.DateTimeStyles.None, out DateTime fileDate)) {
					if (fileDate > latestDate) {
						latestDate = fileDate;
						latestFile = file;
					}
				}
			}
		}
		//Debug.Log(latestFile.FullName);
		//			string path = filePath;
		//	string fileName = Path.GetFileName(filePath);
		//	if (fileName.StartsWith("._")) {
		//		fileName = fileName.Substring(2);
		//		path = folderPath + fileName;
		//	}

		//Debug.Log(latestFile.Name);

		//return latestFile != null ? latestFile.FullName : null;
		return latestFile != null ? latestFile.Name : null;
	}

	PlayerData GetPlayerDataByTimeStamp(string folderPath, string timestamp) {
		string[] filePaths = Directory.GetFiles(folderPath);
		string dataAsJson = "";
		foreach (var filePath in filePaths) {
			string path = filePath;
			string fileName = Path.GetFileName(filePath);
			if (fileName.StartsWith("._")) {
				fileName = fileName.Substring(2);
				path = folderPath + fileName;
			}
			//string.
			//dataAsJson = File.ReadAllText(path);
			break;

		}
		
		PlayerData playerData = JsonUtility.FromJson<PlayerData>(dataAsJson);
		return playerData;
	}

	void LoadData(string folderPath) {
		string[] filePaths = Directory.GetFiles(folderPath, "*.json");
		foreach (var filePath in filePaths) {
			string path = filePath;
			string fileName = Path.GetFileName(filePath);
			if (fileName.StartsWith("._")) {
				fileName = fileName.Substring(2);
				path = folderPath + fileName;
			}

			string dataAsJson = File.ReadAllText(path);
			PlayerData playerData = JsonUtility.FromJson<PlayerData>(dataAsJson);
			scores.Add(playerData.summary.score);
			maxPunchSpeeds.Add(playerData.summary.maxPunchSpeed);
			avgPunchSpeeds.Add(playerData.summary.avgPunchSpeed);
			hitRates.Add(playerData.summary.hitRate);
			minReactionTimes.Add(playerData.summary.minReactionTime);
			avgReactionTimes.Add(playerData.summary.avgReactionTime);
			totalPunchNums.Add(playerData.summary.totalPunchNum);
		}
	}

	void SortAllLists() {
		scores.Sort();
		maxPunchSpeeds.Sort();
		avgPunchSpeeds.Sort();
		hitRates.Sort();
		minReactionTimes.Sort();
		avgReactionTimes.Sort();
		totalPunchNums.Sort();
	}

	float CalculatePercentile(float value, List<float> sortedList, bool largerThan = true) {
		int index = -1;
		if (largerThan == true) {
			index = sortedList.FindIndex(s => s >= value);
        } else {
			index = sortedList.FindIndex(s => s <= value);
		}
		if (index == -1) return 0.0f;
		float percentile = (float)index / sortedList.Count * 100;
		return percentile;
	}

	string GetPrompts(float percentile, string[] promptList) {
		List<float> percentileTable = new List<float>() { 50f, 75f, 90f, 95f };
		int index = percentileTable.FindIndex(s => s >= percentile);
		if (index == -1) {
			index = 4;
		}
		string prompt = promptList[index];
		return prompt;
	}

	Sprite GetBadges(float percentile, Sprite[] sprites) {
		List<float> percentileTable = new List<float>() { 50f, 75f, 90f, 95f };
		int index = percentileTable.FindIndex(s => s >= percentile);
		if (index == -1) {
			index = 4;
		}
		Sprite badge = sprites[index];
		return badge;
	}

	private Texture2D GetQRCode(string msg) {
		BarcodeWriter writer = new BarcodeWriter();
		writer.Format = BarcodeFormat.QR_CODE;
		writer.Options = new QrCodeEncodingOptions {
			Height = 256,
			Width = 256
		};

		Color32[] pixels = writer.Write(msg);
		Texture2D texture = new Texture2D(writer.Options.Width, writer.Options.Height);
		texture.SetPixels32(pixels);
		texture.Apply();

		return texture;
	}

	Texture2D Crop(Texture2D original, int top, int left, int right, int bottom) {
		int width = original.width - left - right;
		int height = original.height - top - bottom;

		if (width <= 0 || height <= 0) {
			Debug.LogError("Invalid crop dimensions.");
			return original;
		}

		Texture2D croppedTexture = new Texture2D(width, height);

		Color[] pixels = original.GetPixels(left, bottom, width, height);
		croppedTexture.SetPixels(pixels);
		croppedTexture.Apply();

		return croppedTexture;
	}

	Texture2D ChangeBlackToPink(Texture2D original) {
		// Create a new Texture2D with the same dimensions as the original
		Texture2D newTexture = new Texture2D(original.width, original.height, original.format, false);
		Color pink = new Color(1f, 0.48f, 0.67f, 1f);
		// Iterate over all the pixels of the texture
		for (int y = 0; y < original.height; y++) {
			for (int x = 0; x < original.width; x++) {
				Color pixelColor = original.GetPixel(x, y);

				// Change black pixels to pink
				if (pixelColor == Color.black) {
					//pixelColor = pink;
					pixelColor = new Color(0.2f, 0.2f, 0.2f);
				}

				// Set the new color to the new texture
				newTexture.SetPixel(x, y, pixelColor);
			}
		}

		// Apply the changes to the new texture
		newTexture.Apply();

		return newTexture;
	}

	Texture2D LoadTexture(string path) {
		if (File.Exists(path)) {
			byte[] fileData = File.ReadAllBytes(path);
			Texture2D texture = new Texture2D(512, 512);
			if (texture.LoadImage(fileData)) // Load the image data into the texture
			{
				return texture;
			}
		}
		return null;
	}

	IEnumerator ScanQRCode() {
		// Create a barcode reader instance
		QRCodeReader qrCodeReader = new QRCodeReader();
		IBarcodeReader barcodeReader = new BarcodeReader();

		while (webCamTexture.isPlaying) {
			// Wait for a frame to be captured
			yield return new WaitForEndOfFrame();

			// Get the frame pixels
			Color32[] pixels = webCamTexture.GetPixels32();
			int width = webCamTexture.width;
			int height = webCamTexture.height;

			// Create a binary bitmap from the pixel data
			var luminanceSource = new Color32LuminanceSource(pixels, width, height);
			var binarizer = new HybridBinarizer(luminanceSource);
			var binaryBitmap = new BinaryBitmap(binarizer);

			// Decode the QR code
			//var result = qrCodeReader.decode(binaryBitmap);
			var result = barcodeReader.Decode(pixels, width, height);


			if (result != null) {
				Debug.Log("QR Code: " + result.Text);
				AvarCardSummary acs = JsonUtility.FromJson<AvarCardSummary>(result.Text);

				// Stop scanning after a successful decode
				if (acs != null) {
					ScannedText.text = "Card Scanned: " + acs.timestamp;
					previousCardSummary = acs;
					ScanState = ScanStateType.Scanned;
				yield break;
                }
			}
		}
	}

	[Serializable]
	public class PlayerData {
		public Summary summary;
	}

	[Serializable]
	public class Summary {
		public float score;
		public float maxPunchSpeed;
		public float avgPunchSpeed;
		public float hitRate;
		public float minReactionTime;
		public float avgReactionTime;
		public float totalPunchNum;
	}

	[Serializable]
	public class AvarCardSummary {
		public string timestamp;
        //public List<string> stats = new List<string>();
        //public float score;
        //public float maxPunchSpeed;
        //public float avgPunchSpeed;
        //public float hitRate;
        //public float minReactionTime;
        //public float avgReactionTime;
        //public float totalPunchNum;
        public int trial;
    }
}