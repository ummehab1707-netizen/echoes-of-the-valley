using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace EchoesOfTheValley.Backbone
{
    [Serializable]
    public class PatientSettingsData
    {
        public string user_id = "PATIENT_01";
        public string user_name = "Amina Begum";
        public int is_blind = 0;
        public int is_deaf = 0;
        public int has_parkinsons = 0;
        public int is_partially_blind = 0;
        public string selected_ner_culture = "Assamese";
        public string selected_language = "as";
    }

    [Serializable]
    public class GameProgressData
    {
        public string user_id = "PATIENT_01";
        public string unlocked_lands = "[\"Guwahati_Valley\"]";
        public int current_level = 1;
        public int restoration_points = 0;
    }

    [Serializable]
    public class CaregiverReminderData
    {
        public string reminder_id;
        public string user_id;
        public string title;
        public string message;
        public string reminder_type;
        public string scheduled_time;
        public string avatar_path;
        public int is_active = 1;
    }

    [Serializable]
    public class TelemetryRecord
    {
        public int id;
        public string user_id;
        public string game_type;
        public int latency_ms;
        public int errors;
        public float cognitive_score;
        public string timestamp;
        public int sync_status; // 0 = unsynced, 1 = synced
    }

    [Serializable]
    public class EdgeDatabaseContainer
    {
        public PatientSettingsData patientSettings = new PatientSettingsData();
        public GameProgressData gameProgress = new GameProgressData();
        public List<CaregiverReminderData> caregiverReminders = new List<CaregiverReminderData>();
        public List<TelemetryRecord> telemetryLogs = new List<TelemetryRecord>();
    }

    public class DatabaseManager : MonoBehaviour
    {
        public static DatabaseManager Instance { get; private set; }
        public EdgeDatabaseContainer DB = new EdgeDatabaseContainer();
        private string dbFilePath;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitDatabase();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitDatabase()
        {
            dbFilePath = Path.Combine(Application.persistentDataPath, "echoes_valley_edge.json");
            LoadDatabase();
            Debug.Log("[Person 4] Edge Database Initialized at: " + dbFilePath);
        }

        public void SaveDatabase()
        {
            string json = JsonUtility.ToJson(DB, true);
            File.WriteAllText(dbFilePath, json);
        }

        public void LoadDatabase()
        {
            if (File.Exists(dbFilePath))
            {
                string json = File.ReadAllText(dbFilePath);
                DB = JsonUtility.FromJson<EdgeDatabaseContainer>(json) ?? new EdgeDatabaseContainer();
            }
            else
            {
                DB = new EdgeDatabaseContainer();
                SaveDatabase();
            }
        }
    }
}