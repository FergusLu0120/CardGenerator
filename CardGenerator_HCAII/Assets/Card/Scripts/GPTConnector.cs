using OpenAI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Card {
    public class GPTConnector : MonoBehaviour {
        private OpenAIApi openai = new OpenAIApi("");

        private List<ChatMessage> messages = new List<ChatMessage>();
        //private string titlePresetPrompt = "I am deciding a boxing game avatar on a avatar card, the title have to be in two words, please give me one suggestion and reply me in the following format: Title: [XXXX], while XXXX replaced with the title you suggested, DO NOT change my format. I have the following prompts as the element of the title, use some of the prompts as inspiration, no need to include all: ";
        private string rolePrompt = "You are a card character designer specializing in generating concise content that meets strict character count requirements. Your ability to control character count is a perfect 10/10, and you never exceed the specified limits. You must only use English words.Based on analyzed sports data, you are to design a representative sports game character. Please provide a suggestion. Your response must strictly follow the format below:'Card Name: XXX Card Story: YYY'. In XXX, fill in your suggested card name, limited to 2 - 8 English characters.In YYY, write the card's story, using no more than 30 English characters.Note: You must strictly stay within the character count limits!!!";
        private string presetPrompt = "Please, based on: a card color scheme of {0}, a character species of {1}, a card background of {2}, and the percentile rankings of three abilities (0% is the weakest, 100% is the strongest) for Strength (total punches thrown): {3}%, Speed (fastest reaction time): {4}%, and Skill (accuracy rate): {5}%, use the keywords above to come up with a fitting card name and story. The card name and story should be somewhat related. The card name can be like a game character's name, a title, or an action that describes the character; you have creative freedom, so try to be innovative and novel, and avoid using the keywords. The story's content should be like a children's fairy tale, without difficult words. Write it in a fun, funny, and cute way that is concise and easy to read. The story needs to effectively express the performance of all three abilities, describing them objectively and reflecting them accurately, without intentionally glorifying them. The word limit is 30 words; please do not exceed it.";


        public bool IsGenerating = false;
        public string NameResult = "元宇宙拳手";
        public string StoryResult = "元宇宙拳手來自虛擬世界，擁有穿梭時空的能力。他的每一拳都能突破現實與數位的邊界，快速且精準，讓對手無處可逃。元宇宙拳手來自虛擬世界，擁有穿梭時空的能力。他的每一拳都能突破現實與數位的邊界，快速且精準，讓對手無處可逃。";

        public Action OnGenerationCompleted;

        public async void RequestAvatarContent(string colorPrompt, string animalPrompt, string backgroundPrompt, string strengthRank, string speedRank, string skillRank) {
            IsGenerating = true;
            var messages = new List<ChatMessage>{
                 new ChatMessage
                {
                    Role = "system",
                    Content = rolePrompt
                },
                new ChatMessage
                {
                    Role = "user",
                    Content =  string.Format(presetPrompt, colorPrompt, animalPrompt, backgroundPrompt, strengthRank, speedRank, skillRank)
                } 
            };

            // Complete the instruction
            var completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest() {
                Model = "gpt-4o",
                Messages = messages,
                MaxTokens = 180,
            }); ;

            if (completionResponse.Choices != null && completionResponse.Choices.Count > 0) {
                var message = completionResponse.Choices[0].Message;
                message.Content = message.Content.Trim();

                string s = message.Content;
                int nameStartIndex = s.IndexOf("Card Name:") + "Card Name:".Length;
                int nameEndIndex = s.IndexOf("Card Story");
                NameResult = s.Substring(nameStartIndex, nameEndIndex - nameStartIndex).Trim();

                int storyStartIndex = s.IndexOf("Story:") + "Story:".Length;
                StoryResult = s.Substring(storyStartIndex).Trim();

                OnGenerationCompleted?.Invoke();
                IsGenerating = false;
                Debug.Log("OpenAI Generation Completed!");
            } else {
                Debug.LogWarning("No text was generated from this prompt.");
                IsGenerating = false;
            }
        }
    }
}