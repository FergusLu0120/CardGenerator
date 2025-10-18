using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardCreator : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI scoreText;
    public int Score {
        set {
            if (value < 0) { scoreText.text = "-"; return;}
            scoreText.text = value.ToString(); }
    }
    [SerializeField] private Text timeStampOldText;
    [SerializeField] private TextMeshProUGUI timeStampText;
    public string TimeStamp {
        set {
            timeStampOldText.text = value;
            timeStampText.text = value;
        }
    }
    [SerializeField] private TextMeshProUGUI strengthDataText;
    public int StrengthData {
        set
        {
            if (value < 0) { strengthDataText.text = "-"; return; }
            strengthDataText.text = value.ToString();
        }
    }
    [SerializeField] private TextMeshProUGUI strengthRankText;
    public float StrengthRank {
        set {
            if (Mathf.Round(value) >= 100f) {
                strengthRankText.text = "100<size=15px>%</size>";
            } else {
                strengthRankText.text = string.Format("{0}<size=15px>%</size>", value.ToString("F1"));
            }
        }
    }
    [SerializeField] private TextMeshProUGUI speedDataText;
    public float SpeedData {
        set {
            if (value < 0f) { speedDataText.text = "-"; return;}
            speedDataText.text = string.Format("{0}s", value.ToString()); }
    }
    [SerializeField] private TextMeshProUGUI speedRankText;
    public float SpeedRank {
        set {
            if (Mathf.Round(value) >= 100f) {
                speedRankText.text = "100<size=15px>%</size>";
            } else {
                speedRankText.text = string.Format("{0}<size=15px>%</size>", value.ToString("F1"));
            }
        }
    }
    [SerializeField] private TextMeshProUGUI skillDataText;
    public float SkillData {
        set {
            if (value < 0f) { skillDataText.text = "-"; return; }
            skillDataText.text = string.Format("{0}%", value.ToString("F2")); }
    }
    [SerializeField] private TextMeshProUGUI skillRankText;
    public float SkillRank {
        set {
            if (Mathf.Round(value) >= 100f) {
                skillRankText.text = "100<size=15px>%</size>";
            } else {
                skillRankText.text = string.Format("{0}<size=15px>%</size>", value.ToString("F1"));
            }
        }
    }
    [SerializeField] private Text avatarNameOldText;
    [SerializeField] private TextMeshProUGUI avatarNameText;
    public string AvatarName {
        set {
            avatarNameOldText.text = value;
            avatarNameText.text = value;
        }
    }
    [SerializeField] private Text avatarStoryOldText;
    [SerializeField] private TextMeshProUGUI avatarStoryText;
    public string AvatarStory {
        set {
            avatarStoryOldText.text = value;
            avatarStoryText.text = value;
        }
    }

}
