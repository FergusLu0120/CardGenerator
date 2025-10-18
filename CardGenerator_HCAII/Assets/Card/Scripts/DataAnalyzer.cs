using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Card {
	public class DataAnalyzer : MonoBehaviour {
		public List<float> Scores = new List<float>();
		public List<float> MaxPunchSpeeds = new List<float>();
		public List<float> AvgPunchSpeeds = new List<float>();
		public List<float> HitRates = new List<float>();
		public List<float> MinReactionTimes = new List<float>();
		public List<float> AvgReactionTimes = new List<float>();
		public List<float> TotalPunchNums = new List<float>();

		private void Start() {
			PreloadDataRecords();
		}

		private void Update () {
		if (Input.GetKeyDown(KeyCode.O)) {
				StartCoroutine(FilterData());
			}
		}

		private IEnumerator FilterData() {
			string folderPath = CardMaster.Instance.MPDataPath;
			string targetFolder  = Application.dataPath + "/Card/FilteredData";
			string[] allFilePaths = Directory.GetFiles(folderPath, "*.json");
			for (int i = 0; i < allFilePaths.Length; i++) {
				Debug.Log($"Filtering data No. {(i+1)} / {allFilePaths.Length}");
				string path = allFilePaths[i];
				string fileName = Path.GetFileName(path);
				//if (fileName.StartsWith("._")) {
				//	fileName = fileName.Substring(2);
				//	path = folderPath + fileName;
				//}
				string dataAsJson = File.ReadAllText(path);
				PlayerData playerData = JsonUtility.FromJson<PlayerData>(dataAsJson);
				if (playerData.summary.score == 0) continue;
				if (playerData.summary.totalPunchNum == 0) continue;
				if (playerData.summary.totalPunchNum > 120) continue;
				        if (!Directory.Exists(targetFolder))
				{
					Directory.CreateDirectory(targetFolder);
				}
				Debug.Log(path);
                File.Copy(path, targetFolder + "/" + fileName, true);
 				Debug.Log($"Copied: {fileName}");
				yield return null;
			}
		}
		
		public void PreloadDataRecords() {
			string preloadedJson = LoadJsonFromFile();
			if (string.IsNullOrEmpty(preloadedJson) == false) {
				//load from PlayerPref
				DataSet savedDataSet = JsonUtility.FromJson<DataSet>(preloadedJson);
				Debug.Log(savedDataSet);
				Scores = savedDataSet.Scores;
				MaxPunchSpeeds = savedDataSet.MaxPunchSpeeds;
				AvgPunchSpeeds = savedDataSet.AvgPunchSpeeds;
				HitRates = savedDataSet.HitRates;
				MinReactionTimes = savedDataSet.MinReactionTimes;
				AvgReactionTimes = savedDataSet.AvgReactionTimes;
				TotalPunchNums = savedDataSet.TotalPunchNums;
				return;
			}
			LoadAndSaveData();
		}

		public void LoadAndSaveData() {
			string mpdataPath = CardMaster.Instance.MPDataPath;
			Debug.Log("Data Loading from " + mpdataPath);
			LoadData(mpdataPath);
			SortAllLists();

			DataSet ds = new DataSet();
			ds.Scores = Scores;
			ds.MaxPunchSpeeds = MaxPunchSpeeds;
			ds.AvgPunchSpeeds = AvgPunchSpeeds;
			ds.HitRates = HitRates;
			ds.MinReactionTimes = MinReactionTimes;
			ds.AvgReactionTimes = AvgReactionTimes;
			ds.TotalPunchNums = TotalPunchNums;
			string json = JsonUtility.ToJson(ds);
			SaveJsonToFile(json);

			Debug.Log("Data Amount: " + Scores.Count);
			Debug.Log("Score 0:" + Scores[0] + "Score 100:" + Scores[Scores.Count - 1]);
			Debug.Log("STR 0:" + TotalPunchNums[0] + "STR 100:" + TotalPunchNums[TotalPunchNums.Count - 1]);
			Debug.Log("SPD 0:" + MinReactionTimes[0] + "SPD 100:" + MinReactionTimes[MinReactionTimes.Count - 1]);
			Debug.Log("SKL 0:" + HitRates[0] + "SKL 100:" + HitRates[HitRates.Count - 1]);
		}

		private void LoadData(string folderPath) {
			string[] allFilePaths = Directory.GetFiles(folderPath, "*.json");
			Scores.Clear();
			MaxPunchSpeeds.Clear();
			AvgPunchSpeeds.Clear();
			HitRates.Clear();
			MinReactionTimes.Clear();
			AvgReactionTimes.Clear();
			TotalPunchNums.Clear();
			for (int i = 0; i < allFilePaths.Length; i++) {
				string path = allFilePaths[i];
				Debug.Log(path);
				string fileName = Path.GetFileName(path);
				//if (fileName.StartsWith("._")) {
				//	fileName = fileName.Substring(2);
				//	path = folderPath + fileName;
				//}
				string dataAsJson = File.ReadAllText(path);
				PlayerData playerData = JsonUtility.FromJson<PlayerData>(dataAsJson);
				if (playerData.summary.score == 0) continue;
				if (playerData.summary.totalPunchNum == 0) continue;
				if (playerData.summary.totalPunchNum > 120) continue;
				Scores.Add(playerData.summary.score);
				MaxPunchSpeeds.Add(playerData.summary.maxPunchSpeed);
				AvgPunchSpeeds.Add(playerData.summary.avgPunchSpeed);
				HitRates.Add(playerData.summary.hitRate);
				MinReactionTimes.Add(playerData.summary.minReactionTime);
				AvgReactionTimes.Add(playerData.summary.avgReactionTime);
				TotalPunchNums.Add(playerData.summary.totalPunchNum);
			}
		}

		private void SortAllLists() {
			Scores.Sort();
			MaxPunchSpeeds.Sort();
			AvgPunchSpeeds.Sort();
			HitRates.Sort();
			MinReactionTimes.Sort();
			AvgReactionTimes.Sort();
			TotalPunchNums.Sort();
		}

		public float GetPercentileRanking(float value, List<float> sortedList, bool isLargerThan = true) {
			int index = -1;
			if (isLargerThan == true) {
				if (value >= sortedList[sortedList.Count - 1]) {
					return 100f;
				}
				index = sortedList.FindIndex(s => s >= value);
				float percentile = (float)index / sortedList.Count * 100f;
				return percentile;
			} else {
				if (value <= sortedList[0]) {
					return 100f;
				}
				if (value >= sortedList[sortedList.Count - 1]) {
					return 0f;
				}
				index = sortedList.FindIndex(s => s >= value);
				float percentile = 100f - ((float)index / sortedList.Count * 100f);
				return percentile;
			}
		}
		public void SaveJsonToFile(string jsonString) {
			string filePath = CardMaster.Instance.PreloadedJsonPath;

			// Ensure the directory exists
			string directoryPath = Path.GetDirectoryName(filePath);
			if (!Directory.Exists(directoryPath)) {
				Directory.CreateDirectory(directoryPath);
			}

			// Write the JSON string to the file
			File.WriteAllText(filePath, jsonString);

			Debug.Log($"PreloadedJson saved to: {filePath}");
		}

		public string LoadJsonFromFile() {
			string filePath = CardMaster.Instance.PreloadedJsonPath;

			if (File.Exists(filePath)) {
				string jsonString = File.ReadAllText(filePath);
				Debug.Log("JSON loaded successfully.");
				return jsonString;
			} else {
				Debug.LogWarning($"File not found at: {filePath}");
				return string.Empty;
			}
		}

		public class DataSet {
			public List<float> Scores = new List<float>();
			public List<float> MaxPunchSpeeds = new List<float>();
			public List<float> AvgPunchSpeeds = new List<float>();
			public List<float> HitRates = new List<float>();
			public List<float> MinReactionTimes = new List<float>();
			public List<float> AvgReactionTimes = new List<float>();
			public List<float> TotalPunchNums = new List<float>();
		}
	}
}
