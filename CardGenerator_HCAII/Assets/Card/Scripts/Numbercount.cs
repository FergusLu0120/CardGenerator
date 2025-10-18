using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Numbercount : MonoBehaviour
{
    public Text numberText;
    public int targetNumber;
    public float timeToCount;

    private void Start()
    {
        StartCoroutine(CountToNumber());
    }

    IEnumerator CountToNumber()
    {
        float currentNumber = 0;
        float timePassed = 0;
        float rateOfChange;

        while (currentNumber < targetNumber)
        {
            timePassed += Time.deltaTime;
            float progress = timePassed / timeToCount;

            rateOfChange = Mathf.Pow(progress, 2);
            currentNumber = Mathf.Lerp(0, targetNumber, rateOfChange);

            numberText.text = ((int)currentNumber).ToString();
            yield return null;
        }

        numberText.text = targetNumber.ToString();
    }
}
