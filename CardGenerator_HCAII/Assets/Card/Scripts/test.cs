using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
// using GM;
using MoveBag;
using UnityEngine.UI;

namespace MoveBag
{
    public class test : MonoBehaviour
    {

        public float focusI, luckI, staminaI, agilityI, skillI, strengthI;


        public GameObject strengthpic, agilitypic, focuspic, staminapic, luckpic, skillpic;
        public Sprite[] strength;
        public Sprite[] agility;
        public Sprite[] focus;
        public Sprite[] stamina;
        public Sprite[] luck;
        public Sprite[] skill;

        private float[] focusThresholds = { 0.370f, 0.394f, 0.469f, 0.6f };
        private float[] luckThresholds = { 0.192f, 0.236f, 0.278f, 0.333f };
        private float[] staminaThresholds = { 1.567f, 1.884f, 2.173f, 2.382f };
        private float[] agilityThresholds = { 2.158f, 2.562f, 3.071f, 3.469f };
        private float[] skillThresholds = { 84.9f, 93.4f, 97.3f, 98.4f };
        private float[] strengthThresholds = { 101, 119, 135, 144 };

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            focuspic.GetComponent<Image>().sprite = GetSpriteByThreshold(focusI, focus, focusThresholds);//avgRT
            luckpic.GetComponent<Image>().sprite = GetSpriteByThreshold(luckI, luck, luckThresholds);//minRT
            staminapic.GetComponent<Image>().sprite = GetSpriteByThreshold(staminaI, stamina, staminaThresholds);//avgPS
            agilitypic.GetComponent<Image>().sprite = GetSpriteByThreshold(agilityI, agility, agilityThresholds);//maxPS
            skillpic.GetComponent<Image>().sprite = GetSpriteByThreshold(skillI, skill, skillThresholds);//hitRate
            strengthpic.GetComponent<Image>().sprite = GetSpriteByThreshold(strengthI, strength, strengthThresholds);//TPN
        }

        Sprite GetSpriteByThreshold(float value, Sprite[] sprites, float[] thresholds)
        {
            for (int i = 0; i < thresholds.Length; i++)
            {
                if (value < thresholds[i])
                {
                    return sprites[i];
                }
            }
            return sprites[sprites.Length - 1];
        }
    }
}

