using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using EchoesOfTheValley.Backbone; // Connects to Person 4's Telemetry

public class SignLanguageReceiver : MonoBehaviour
{
    [Header("API Settings")]
    [SerializeField] private string apiUrl = "http://127.0.0.1:8000/sign/latest";
    [SerializeField] private float pollInterval = 0.5f;

    private string lastProcessedSign = "";

    void Start()
    {
        // Poll Person 1's Python FastAPI server twice every second
        InvokeRepeating(nameof(PollAI), 0.5f, pollInterval);
    }

    void PollAI()
    {
        StartCoroutine(GetLatestSignRoutine());
    }

    IEnumerator GetLatestSignRoutine()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(apiUrl))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                string jsonText = webRequest.downloadHandler.text;
                SignData data = JsonUtility.FromJson<SignData>(jsonText);

                if (data != null && data.confidence > 0.8f && data.text != lastProcessedSign)
                {
                    lastProcessedSign = data.text;
                    HandleSignAction(data.text);
                }
            }
        }
    }

    void HandleSignAction(string signText)
    {
        Debug.Log($"[Sign AI Recognized]: {signText}");

        // 1. Log telemetry to Person 4 SQLite database
        if (TelemetryLogger.Instance != null)
        {
            TelemetryLogger.Instance.RecordGameSession("PATIENT_01", $"SignInput_{signText}", 500, 0);
        }

        // 2. Trigger Game Events for Person 2 (UI) and Person 3 (Game Mechanics)
        if (signText == "WATER")
        {
            Debug.Log("Triggering Hydration / Water Event in Game UI!");
        }
        else if (signText == "MEDICINE")
        {
            Debug.Log("Triggering Medicine Confirmation in Game UI!");
        }
    }

    [Serializable]
    public class SignData
    {
        public string text;
        public float confidence;
    }
}