using System;
using System.Data;
using System.IO;
using Mono.Data.Sqlite;
using UnityEngine;

namespace EchoesOfTheValley.Backbone
{
    public class DatabaseManager : MonoBehaviour
    {
        public static DatabaseManager Instance { get; private set; }
        private string dbPath;

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
            string path = Path.Combine(Application.persistentDataPath, "echoes_valley_edge.db");
            dbPath = "URI=file:" + path;

            using (IDbConnection dbConn = new SqliteConnection(dbPath))
            {
                dbConn.Open();
                using (IDbCommand dbCmd = dbConn.CreateCommand())
                {
                    // 1a. PatientSettings Table (Offline Accessibility Profiles)
                    dbCmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS PatientSettings (
                            user_id TEXT PRIMARY KEY,
                            user_name TEXT NOT NULL,
                            is_blind INTEGER DEFAULT 0,
                            is_deaf INTEGER DEFAULT 0,
                            has_parkinsons INTEGER DEFAULT 0,
                            is_partially_blind INTEGER DEFAULT 0,
                            selected_ner_culture TEXT DEFAULT 'Assamese',
                            selected_language TEXT DEFAULT 'en'
                        );";
                    dbCmd.ExecuteNonQuery();

                    // 1b. GameProgress Table (Unlocked Lands)
                    dbCmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS GameProgress (
                            user_id TEXT PRIMARY KEY,
                            unlocked_lands TEXT DEFAULT '[""Guwahati_Valley""]',
                            current_level INTEGER DEFAULT 1,
                            restoration_points INTEGER DEFAULT 0
                        );";
                    dbCmd.ExecuteNonQuery();

                    // 1c. CaregiverReminders Table
                    dbCmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS CaregiverReminders (
                            reminder_id TEXT PRIMARY KEY,
                            user_id TEXT NOT NULL,
                            title TEXT NOT NULL,
                            message TEXT NOT NULL,
                            reminder_type TEXT CHECK(reminder_type IN ('Medicine', 'Hydration', 'Appointment', 'Routine')),
                            scheduled_time TEXT NOT NULL,
                            avatar_path TEXT,
                            is_active INTEGER DEFAULT 1,
                            FOREIGN KEY(user_id) REFERENCES PatientSettings(user_id)
                        );";
                    dbCmd.ExecuteNonQuery();

                    // 2. Telemetry Table (Reaction Times & Cognitive Metrics)
                    dbCmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Telemetry (
                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                            user_id TEXT NOT NULL,
                            game_type TEXT NOT NULL,
                            latency_ms INTEGER NOT NULL,
                            errors INTEGER NOT NULL,
                            cognitive_score REAL NOT NULL,
                            timestamp DATETIME DEFAULT CURRENT_TIMESTAMP,
                            sync_status INTEGER DEFAULT 0
                        );";
                    dbCmd.ExecuteNonQuery();
                }
            }
            Debug.Log("[Person 4] SQLite Edge Database Initialized at: " + dbPath);
        }

        public IDbConnection GetConnection()
        {
            IDbConnection conn = new SqliteConnection(dbPath);
            conn.Open();
            return conn;
        }
    }
}