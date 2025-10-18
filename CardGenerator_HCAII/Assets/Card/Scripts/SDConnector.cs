using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Card {
    public class SDConnector : MonoBehaviour {
        [SerializeField] private StableDiffusionText2Image SDText2Image;
        [SerializeField] private StableDiffusionImage2Image SDImage2Image;
        private float progress = 0f;
        public float Progress {
            get { return progress; }
            set {
                progress = value;
                OnGenerationProgressUpdated?.Invoke(progress);
                if (progress >= 1f) {
                    OnGenerationProgressCompleted?.Invoke();
                }
            }
        }
        public bool IsGenerating {
            get {
                if (Progress >= 1f) {
                    return false;
                } else {
                    return true;
                }
            }
        }

        private string presetPrompts = "<lora:fallguys:1>, animal avatar, cuteness 3D render, fully render, fighting pose, trading card style, cartoon style, complete features, beautiful big eyes, no distortion";
        //(human, man, human face:1.4), (hairs, nose, muscle), 
        private string presetNegativePrompts = "realistic style, multiple face, human face, realistic face, realistic face features, humanoid, human anatomy, human lips, human proportions, incomplete face, missing eyes, distorted features, asymmetry face, low quality, bad-hands-5, (multiple characters), title, words, error";

        public Action<float> OnGenerationProgressUpdated;
        public Action OnGenerationProgressCompleted;


        public void RequestAvatarImage(string prompts, string fileName) {
            SDText2Image.SavingImageName = string.Format("Image_{0}", fileName);
            string finalPrompt = presetPrompts + prompts;
            SDText2Image.prompt = finalPrompt;
            SDText2Image.negativePrompt = presetNegativePrompts;
            SDText2Image.Generate();
            Progress = 0f;
        }

        private void Update() {
            Progress = SDText2Image.Progress;
        }
    }
}
