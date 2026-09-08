using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace EchoesOfTheValley.Backbone
{
    public class SyncEngine : MonoBehaviour
    {
        public static SyncEngine Instance { get; private set; }

        [SerializeField] private string cloudEndpoint = "https://api.echoesvalley.org/v1/sync";
        private readonly byte[] aesKey = Encoding.UTF8.GetBytes("12345678901234567890123456789012"); // 32-byte AES key

        private void Awake()
        {
            if (Instance == null) Instance = this;
        }

        public void TriggerSync()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Debug.Log("[SyncEngine] Device is offline. Data retained locally in SQLite.");
                return;
            }
            StartCoroutine(SyncPendingTelemetryRoutine());
        }

        private IEnumerator SyncPendingTelemetryRoutine()
        {
            List<TelemetryRecord> pendingRecords = new List<TelemetryRecord>();

            using (IDbConnection conn = DatabaseManager.Instance.GetConnection())
            {
                using (IDbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT id, user_id, game_type, latency_ms, errors, cognitive_score FROM Telemetry WHERE sync_status = 0;";
                    using (IDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            pendingRecords.Add(new TelemetryRecord
                            {
                                id = reader.GetInt32(0),
                                user_id = reader.GetString(1),
                                game_type = reader.GetString(2),
                                latency_ms = reader.GetInt32(3),
                                errors = reader.GetInt32(4),
                                cognitive_score = reader.GetFloat(5)
                            });
                        }
                    }
                }
            }

            if (pendingRecords.Count == 0) yield break;

            string rawJson = JsonUtility.ToJson(new TelemetryBatch { records = pendingRecords });
            string encryptedPayload = EncryptAES256(rawJson, aesKey);

            UnityWebRequest request = new UnityWebRequest(cloudEndpoint, "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(encryptedPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                MarkRecordsAsSynced(pendingRecords);
                Debug.Log($"[SyncEngine] {pendingRecords.Count} records encrypted (AES-256) and synced to cloud.");
            }
        }

        private string EncryptAES256(string plainText, byte[] key)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.GenerateIV();
                using (MemoryStream ms = new MemoryStream())
                {
                    ms.Write(aes.IV, 0, aes.IV.Length);
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        byte[] bytes = Encoding.UTF8.GetBytes(plainText);
                        cs.Write(bytes, 0, bytes.Length);
                        cs.FlushFinalBlock();
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        private void MarkRecordsAsSynced(List<TelemetryRecord> records)
        {
            using (IDbConnection conn = DatabaseManager.Instance.GetConnection())
            {
                using (IDbCommand cmd = conn.CreateCommand())
                {
                    foreach (var rec in records)
                    {
                        cmd.CommandText = $"UPDATE Telemetry SET sync_status = 1 WHERE id = {rec.id};";
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        [Serializable] private struct TelemetryRecord { public int id; public string user_id; public string game_type; public int latency_ms; public int errors; public float cognitive_score; }
        [Serializable] private struct TelemetryBatch { public List<TelemetryRecord> records; }
    }
}