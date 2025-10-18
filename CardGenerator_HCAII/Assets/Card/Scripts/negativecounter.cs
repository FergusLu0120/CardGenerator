using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class negativecounter : MonoBehaviour
{
    public Text numberText;
    public int startNumber = 999;
    public int endNumber;
    public float timeToCount;
    public GameObject updown;

    private void Start()
    {
        StartCoroutine(CountDown());
    }

    IEnumerator CountDown()
    {
        float currentNumber = startNumber;
        float timePassed = 0;
        float rateOfChange;

        while (currentNumber > endNumber)
        {
            timePassed += Time.deltaTime;
            float progress = timePassed / timeToCount;

            // 使用非線性函數來計算數字減少
            rateOfChange = Mathf.Pow(progress, 2); // 例如，使用平方函數來實現初慢後快的效果
            currentNumber = Mathf.Lerp(startNumber, endNumber, rateOfChange);

            numberText.text = Mathf.CeilToInt(currentNumber).ToString();
            yield return null;
        }

        numberText.text = endNumber.ToString(); // 確保最終數字正確
        updown.SetActive(true);

    }
}
