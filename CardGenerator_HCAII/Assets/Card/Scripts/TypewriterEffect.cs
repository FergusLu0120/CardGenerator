using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TypewriterEffect : MonoBehaviour
{

    public Text textComponent;
    public string textToType;
    public float typingSpeed;

    private void Start()
    {
        //StartCoroutine(TypeText());
    }

    public void StartTypeText() {
        StartCoroutine(TypeText());
    }

    public IEnumerator TypeText()
    {
        Debug.Log(textToType);
        foreach (char letter in textToType.ToCharArray())
        {
            textComponent.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }
}
