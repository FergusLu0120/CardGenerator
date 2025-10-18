using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Card
{
    public class CardUtilities
    {
        public static string GetLatestFileName(string folderPath) {
            var directoryInfo = new DirectoryInfo(folderPath);
            FileInfo[] files = directoryInfo.GetFiles();
            Debug.Log("FoundFiles: " + files.Length + "in " + folderPath);
            DateTime latestDate = DateTime.MinValue;
            FileInfo latestFile = null;

            foreach (var file in files) {
                // Match match = Regex.Match(file.Name, @"user__(\d{4}_\d{1,2}_\d{1,2} \d{1,2}_\d{1,2}_\d{1,2})\d+\.json");
                //below fixed problem for single digit seconds problem 
                Match match = Regex.Match(file.Name, @"user__(\d{4}_\d{1,2}_\d{1,2} \d{1,2}_\d{1,2}_\d{1,2})");
                if (match.Success) {
                    if (DateTime.TryParseExact(match.Groups[1].Value, "yyyy_M_d H_m_s", null, System.Globalization.DateTimeStyles.None, out DateTime fileDate)) {
                        if (fileDate > latestDate) {
                            latestDate = fileDate;
                            latestFile = file;
                        }
                    }
                }
            }
            return latestFile != null ? latestFile.Name : null;
        }

        public static PlayerData LoadSpecificPlayerData(string filePath)
        {
            Debug.Log($"Reading data from: {Path.GetFileName(filePath)}");
            StreamReader fi = new StreamReader(filePath);
            string json = fi.ReadToEnd();
            fi.Close();
            //string dataAsJson = File.ReadAllText(filePath);
            PlayerData playerData = JsonUtility.FromJson<PlayerData>(json);

            return playerData;
        }
    }
}
