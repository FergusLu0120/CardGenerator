using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;

namespace Card {
	public class CardScanner : MonoBehaviour {

		[SerializeField] private Text prescanText;
		[SerializeField] private Text scanningText;
		[SerializeField] private Text scannedText;
		[SerializeField] private RawImage webcamDisplay;
		private WebCamTexture webCamTexture;

		private CardQRCodeContent loadedCardContent;

		private ScanStateType scanState = ScanStateType.Prescan;
		public ScanStateType ScanState {
			get { return scanState; }
			set {
				switch (scanState) {
					case ScanStateType.Prescan:
					prescanText.gameObject.SetActive(false);
					break;
					case ScanStateType.Scanning:
					scanningText.gameObject.SetActive(false);
					webcamDisplay.gameObject.SetActive(false);
					webCamTexture.Stop();
					StartCoroutine(ScanQRCode());
					break;
					case ScanStateType.Scanned:
					scannedText.gameObject.SetActive(false);
					break;
				}
				scanState = value;
				switch (scanState) {
					case ScanStateType.Prescan:
					prescanText.gameObject.SetActive(true);
					loadedCardContent = null;
					break;
					case ScanStateType.Scanning:
					scanningText.gameObject.SetActive(true);
					webcamDisplay.gameObject.SetActive(true);
					webcamDisplay.texture = webCamTexture;
					webCamTexture.Play();
					StartCoroutine(ScanQRCode());
					break;
					case ScanStateType.Scanned:
					scannedText.gameObject.SetActive(true);
					break;
					case ScanStateType.Off:
					prescanText.gameObject.SetActive(false);
					scanningText.gameObject.SetActive(false);
					scannedText.gameObject.SetActive(false);
					webcamDisplay.gameObject.SetActive(false);
					webCamTexture.Stop();
					break;
				}
			}
		}

        private void Awake() {
			webCamTexture = new WebCamTexture(1920, 1080);
		}

		private void Update() {
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
					CardQRCodeContent cc = JsonUtility.FromJson<CardQRCodeContent>(result.Text);

					// Stop scanning after a successful decode
					if (cc != null) {
						scannedText.text = "Card Scanned: " + cc.timestamp;
						loadedCardContent = cc;
						ScanState = ScanStateType.Scanned;
						yield break;
					}
				}
			}
		}
	}
}