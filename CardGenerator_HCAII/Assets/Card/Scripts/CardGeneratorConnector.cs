using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using extOSC;
using System.IO;
using System;
using System.Text.RegularExpressions;
using UnityEngine.Android;

namespace Card
{
    public class CardGeneratorConnector : MonoBehaviour
    {
        [SerializeField] private OSCReceiver receiver;
        [SerializeField] private OSCTransmitter transmitter;
        private string streamingAssetsPath = Application.streamingAssetsPath;

        private string serverIP = "192.168.0.0";
        private void Awake()
        {
        #if UNITY_ANDROID
            // Check Permissions
            bool rwPermissionCheck =
                UnityEngine.Android.Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead) &&
                UnityEngine.Android.Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite);
            // bool rwPermissionCheck = UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.ExternalStorageRead);
            Debug.Log("<color=cyan>[Permission]</color> android.permission.READ_EXTERNAL_STORAGE & WRITE_EXTERNAL_STORAGE: <b>" + rwPermissionCheck + "</b>");
            if (!rwPermissionCheck)
            {
                string[] permissions = {
                    Permission.ExternalStorageRead,
                    Permission.ExternalStorageWrite
                };

                Permission.RequestUserPermissions(permissions);
                // UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.ExternalStorageRead, androidCB);
            }
#endif
#if UNITY_ANDROID && !UNITY_EDITOR
            receiver.Bind("/Ping", ReceivePing);
            receiver.Bind("/DataRequest", ReceiveDataRequest);
            receiver.Bind("/TodayAllDataRequest", ReceiveTodayAllDataRequest);
#endif
        }

        private void ReceivePing(OSCMessage message)
        {
            serverIP = message.Values[0].StringValue;
            ReturnPing();
        }

        private void ReturnPing()
        {
            transmitter.Close();
            transmitter.RemoteHost = serverIP;
            transmitter.Connect();
            Debug.Log("OSC ReturnPing: " + transmitter.RemoteHost + ":" + transmitter.RemotePort + " - " + OSCUtilities.GetLocalHost());
            OSCMessage msg = new OSCMessage("/ReturnPing");
            msg.AddValue(OSCValue.String(OSCUtilities.GetLocalHost()));
            transmitter.Send(msg);
        }

        public void SendPlayerData(string fileName, string fileJson)
        {
            transmitter.RemoteHost = serverIP;
            transmitter.Connect();
            OSCMessage msg = new OSCMessage("/PlayerData");
            msg.AddValue(OSCValue.String(fileName));
            msg.AddValue(OSCValue.String(fileJson));
            transmitter.Send(msg);
        }

        private void ReceiveDataRequest(OSCMessage message)
        {
            SendLatestData();
        }

        private void ReceiveTodayAllDataRequest(OSCMessage message)
        {
            SendTodayAllDatas();
        }

        public void SendLatestData()
        {
            string filePath;
#if UNITY_EDITOR
            filePath = streamingAssetsPath;
#endif
#if UNITY_ANDROID
            //rely on metapunch
            filePath = GM.GMParamCtr.instance.dataPath;
            //filePath = Application.persistentDataPath;
#endif
            Debug.Log("Loading Data from " + filePath);
            string latestFileName = CardUtilities.GetLatestFileName(filePath);
            string latestFilePath = filePath + "/" + latestFileName;
            if (latestFilePath == null)
            {
                Debug.LogError("No JSON files found in the specified directory.");
                return;
            }
            Debug.Log("Sending PlayerData of Path: " + latestFilePath);
            PlayerData latestPlayerData = CardUtilities.LoadSpecificPlayerData(latestFilePath);
            string json = JsonUtility.ToJson(latestPlayerData);
            //string json;
            //GM.GMParamCtr.LoadJsonFile(filePath + ", out json);
            Debug.Log(json.Length);
            SendPlayerData(latestFileName, json);
        }

        public void SendTodayAllDatas()
        {
            string filePath;

#if UNITY_EDITOR
            filePath = streamingAssetsPath;
#endif
#if UNITY_ANDROID
            //rely on metapunch
            filePath = GM.GMParamCtr.instance.dataPath;
            //filePath = Application.persistentDataPath;
#endif

            Debug.Log("Loading All Data from: " + filePath);
            if (!Directory.Exists(filePath))
            {
                Debug.LogError("Directory does not exist: " + filePath);
                return;
            }

            string[] allFiles = Directory.GetFiles(filePath, "*.json");
            int sentCount = 0;

            foreach (string path in allFiles)
            {
                DateTime lastWriteTime = File.GetLastWriteTime(path);
                if (lastWriteTime.Date == DateTime.Now.Date)
                {
                    string fileName = Path.GetFileName(path);
                    Debug.Log("Found today's file: " + fileName);

                    PlayerData data = CardUtilities.LoadSpecificPlayerData(path);
                    if (data != null)
                    {
                        string json = JsonUtility.ToJson(data);
                        SendPlayerData(fileName, json);
                        sentCount++;
                    }
                    else
                    {
                        Debug.LogWarning("Failed to load data from: " + path);
                    }
                }
            }

            Debug.Log($"Total files sent today: {sentCount}");
        }
    }
}
