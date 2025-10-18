using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstructionToggleController : MonoBehaviour
{
[SerializeField] private GameObject toggleObject;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab)) {
        toggleObject.gameObject.SetActive(!toggleObject.gameObject.activeSelf);
        }
    }
}
