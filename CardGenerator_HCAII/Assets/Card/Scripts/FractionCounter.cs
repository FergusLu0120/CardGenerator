using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FractionCounter : MonoBehaviour
{
    public Text fractionText;
    public int numeratorTarget;    // 分子目標值
    public int denominatorTarget;  // 分母目標值
    public float timeToCount;

    private void Start()
    {
        StartCoroutine(CountFraction());
    }

    IEnumerator CountFraction()
    {
        int numerator = 0;
        int denominator = 0;
        float timePerIncrement = timeToCount / (numeratorTarget + denominatorTarget);

        while (numerator < numeratorTarget || denominator < denominatorTarget)
        {
            if (numerator < numeratorTarget)
            {
                numerator++;
            }

            if (denominator < denominatorTarget)
            {
                denominator++;
            }

            fractionText.text = $"{numerator}/{denominator}";
            yield return new WaitForSeconds(timePerIncrement);
        }
    }
}

