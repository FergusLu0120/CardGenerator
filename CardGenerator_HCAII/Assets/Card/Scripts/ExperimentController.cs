using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Card {
    public class ExperimentController : MonoBehaviour {
        [SerializeField] private List<GameObject> textObjects = new List<GameObject>();
        [SerializeField] private List<GameObject> imageObjects = new List<GameObject>();
        private void Update() {
            if (Input.GetKeyDown(KeyCode.Alpha1)) {
                for (int i = 0; i < textObjects.Count; i++) {
                    textObjects[i].gameObject.SetActive(true);
                }
                for (int i = 0; i < imageObjects.Count; i++) {
                    imageObjects[i].gameObject.SetActive(false);
                }
            }
            if (Input.GetKeyDown(KeyCode.Alpha2)) {
                for (int i = 0; i < textObjects.Count; i++) {
                    textObjects[i].gameObject.SetActive(false);
                }
                for (int i = 0; i < imageObjects.Count; i++) {
                    imageObjects[i].gameObject.SetActive(true);
                }
            }
            if (Input.GetKeyDown(KeyCode.Alpha3)) {
                for (int i = 0; i < textObjects.Count; i++) {
                    textObjects[i].gameObject.SetActive(true);
                }
                for (int i = 0; i < imageObjects.Count; i++) {
                    imageObjects[i].gameObject.SetActive(true);
                }
            }
        }
    }
}