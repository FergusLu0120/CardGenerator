using System.Collections;
using System.Collections.Generic;
using System.IO;
using extOSC;
using UnityEngine;

namespace Card {
	public class OSCServer : MonoBehaviour {
		[SerializeField] private string ipPrefix = "192.168.0.";
		[SerializeField] private OSCTransmitter transmitter;
		[SerializeField] private OSCReceiver receiver;

		[SerializeField] private string connectedDeviceIP;
#if UNITY_EDITOR
		private void Awake() {
			receiver.Bind("/ReturnPing", ReceiveReturnPing);
			receiver.Bind("/PlayerData", ReceivePlayerData);
		}

        private void Start() {
			transmitter.Close();
			transmitter.LocalPort = 4321;
			transmitter.Connect();
			receiver.Close();
			receiver.LocalPort = 1234;
			receiver.Connect();
		}

		public void PingAllIP() {
			for (int i = 0; i <= 255; i++) {
				string ip = ipPrefix + i.ToString();
				transmitter.RemoteHost = ip;
				transmitter.Connect();
				OSCMessage msg = new OSCMessage("/Ping");
				msg.AddValue(OSCValue.String(OSCUtilities.GetLocalHost()));
				transmitter.Send(msg);
			}
		}

		private void ReceiveReturnPing(OSCMessage msg) {
			connectedDeviceIP = msg.Values[0].StringValue;
			Debug.Log("Received Return Ping from " + connectedDeviceIP);
		}

		public void RequestLatestData() {
			transmitter.RemoteHost = connectedDeviceIP;
			transmitter.Connect();
			OSCMessage msg = new OSCMessage("/DataRequest");
			msg.AddValue(OSCValue.String(OSCUtilities.GetLocalHost()));
			transmitter.Send(msg);
		}

		public void RequestTodayDatas() {
			transmitter.RemoteHost = connectedDeviceIP;
			transmitter.Connect();
			OSCMessage msg = new OSCMessage("/TodayAllDataRequest");
			msg.AddValue(OSCValue.String(OSCUtilities.GetLocalHost()));
			transmitter.Send(msg);
		}

		private void ReceivePlayerData(OSCMessage msg) {
			string json = msg.Values[1].StringValue;
			PlayerData data = JsonUtility.FromJson<PlayerData>(json);
			if (data == null) {
				Debug.Log("PlayerData received but not able to decompress!!!");
					return;
            }
			string fileName = msg.Values[0].StringValue;
			string filePath = CardMaster.Instance.ReceivedDataPath + "/" + fileName;
			try {
				StreamWriter fo = new StreamWriter(filePath);
				fo.Write(json);
				fo.Close();
			} catch (System.Exception e) {
				Debug.Log($"<color=red>[Error]</color> {e.Message}");
				return;
			}
			Debug.Log("PlayerData Received from Device, Saved at " + filePath);
		}
#endif
	}
}