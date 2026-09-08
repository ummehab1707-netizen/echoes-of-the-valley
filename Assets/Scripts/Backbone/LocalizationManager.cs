using System;
using System.Collections.Generic;
using UnityEngine;

namespace EchoesOfTheValley.Backbone
{
    public class LocalizationManager : MonoBehaviour
    {
        public static LocalizationManager Instance { get; private set; }
        public string CurrentLanguage = "as"; // Default: Assamese

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        public string GetText(string key)
        {
            TextAsset jsonFile = Resources.Load<TextAsset>("localization");
            if (jsonFile == null) return key;

            if (key == "REMINDER_MEDS" && CurrentLanguage == "as") return "আপোনাৰ নিৰ্ধাৰিত ঔষধ খোৱাৰ সময় হৈছে।";
            if (key == "REMINDER_MEDS" && CurrentLanguage == "en") return "Time to take your scheduled medicine.";
            if (key == "REMINDER_HYDRATION" && CurrentLanguage == "as") return "অনুগ্ৰহ কৰি এক গ্লাছ বিশুদ্ধ পানী খাব।";
            if (key == "REMINDER_HYDRATION" && CurrentLanguage == "en") return "Please drink a glass of fresh water.";

            return key;
        }
    }
}