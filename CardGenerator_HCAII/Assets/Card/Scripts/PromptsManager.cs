using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Card {
    public class PromptsManager {
        public static string[] StrengthTextPrompts = {
            "黑白灰單色調",
            "淺色色調",
            "高彩度紅藍黃三色",
            "彩虹顏色色調",
            "高彩色調加上金銀色元素"
        };
        public static string[] StrengthImagePrompts = {
            "low contrast gray monochrome design, minimalistic with focus on black and white and gray tones, serene and peaceful atmosphere",
            "gentle pastel tones with a focus on red, yellow, and blue hues, soft and subtle color accents",
            "bright red, yellow, and blue tones on the costume, vivid and saturated hues, high contrast with bold color accents, heerful and energetic atmosphere, soft lighting with a focus on primary colors",
            "bright rainbow hues color on the costume and surroundings, bright and colorful rainbow color lighting, bright and cheerful atmosphere",
            "bright and colorful high saturation colors, metallic gold and silver colors with metallic gold and silver details on the costume, shimmering fur and accessories, high contrast with bold, lively colors, glowing light from metallics, magical and whimsical atmosphere" };
        public static string[] SpeedTextPrompts = {
            "倉鼠",
            "兔子",
            "貓咪",
            "鱷魚",
            "獨角獸"
        };
        public static string[] SpeedImagePrompts = {
            "Fall Guys animal creature character in a cute hamster costume, the hamster character is playful and adorable, little size",
            "Fall Guys animal creature character in a cute bunny costume, the bunny character is playful and adorable, with long fluffy bunny ears",
            "Fall Guys animal creature character in a cute cat costume, the cat character is playful and adorable, with big cat ears",
            "Fall Guys animal creature character in a cute crocodile costume, the crocodile character is playful and adorable, teeths on costume",
            "Fall Guys animal creature character in a cute unicorn costume,  the unicorn character is playful and adorable, with magical feel"
        };
        public static string[] SkillTextPrompts = {
            "沙漠",
            "海洋",
            "森林",
            "冰天雪地",
            "銀河和宇宙"
        };
        public static string[] SkillImagePrompts = {
            "set in a desert background, detailed textures, desert sand dunes and cacti in the background",
            "set in an ocean background, detailed textures, waves crashing, seagulls flying",
            "set in a forest background, detailed textures, dense trees with twisted branches and scattered foliage",
            "set in a bright snowy background, detailed textures, snow-covered ground with sparkling snowflakes",
            "set in a deep blue space background, detailed textures, glowing stars and distant galaxies in the background"
        };
        public static string GetPrompt(float percentile, string[] promptList) {
            List<float> percentileTable = new List<float>() { 50f, 75f, 90f, 95f };
            int index = percentileTable.FindIndex(s => s >= percentile);
            if (index == -1) {
                index = 4;
            }
            string prompt = promptList[index];
            return prompt;
        }
    }
}