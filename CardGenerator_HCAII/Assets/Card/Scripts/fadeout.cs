using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fadeout : MonoBehaviour
{
    [SerializeField] private bool isAutoPlay = true;
    public GameObject bk, card, all,rightall;
    private Animator allAnimator;
    private bool isAnimationPlaying;

    private void Awake()
    {
        allAnimator = rightall.GetComponent<Animator>();
    }

    private void Update()
    {
        if (isAutoPlay == false) return;
        if (allAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !allAnimator.IsInTransition(0) && !isAnimationPlaying)
        {
            isAnimationPlaying = true;
            StartCoroutine(WaitAndExecute(4f)); 
        }
    }

    IEnumerator WaitAndExecute(float delay)
    {
        yield return new WaitForSeconds(delay);
        OnAnimationEnd();
    }

    private void OnAnimationEnd()
    {
        
        bk.SetActive(true);
        all.GetComponent<Animator>().enabled = true;
        Invoke("CardSpin", 2f);
    }
    private void CardSpin()
    {
        card.SetActive(true);
    }
}


