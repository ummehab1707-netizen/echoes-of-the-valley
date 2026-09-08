using System;
using UnityEngine;

namespace EchoesOfTheValley.Backbone
{
    public class TelemetryLogger : MonoBehaviour
    {
        public static TelemetryLogger Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
        }

        public void RecordGameSession(string userId, string gameType, int latencyMs, int errors)
        {
            float latencyPenalty = (latencyMs / 1000f) * 2f;
            float errorPenalty = errors * 10f;
            float cognitiveScore = Mathf.Max(0f, 100f - latencyPenalty - errorPenalty);

            TelemetryRecord newRecord = new TelemetryRecord
            {
                id = DatabaseManager.Instance.DB.telemetryLogs.Count + 1,
                user_id = userId,
                game_type = gameType,
                latency_ms = latencyMs,
                errors = errors,
                cognitive_score = cognitiveScore,
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                sync_status = 0
            };

            DatabaseManager.Instance.DB.telemetryLogs.Add(newRecord);
            DatabaseManager.Instance.SaveDatabase();

            Debug.Log($"[Telemetry] Recorded: {gameType} | Latency: {latencyMs}ms | Errors: {errors} | Score: {cognitiveScore}");
        }
    }
}