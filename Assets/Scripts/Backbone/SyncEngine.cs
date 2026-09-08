using System;
using System.Collections;
using System.Collections.Generic;
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
        private readonly byte[] aesKey = Encoding.UTF8.GetBytes("12345678901234567890123456789012");

        private void Awake()
        {
            if (Instance == null) Instance = this;
        }

        public void TriggerSync()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Debug.Log("[SyncEngine] Device is offline. Data retained locally in Edge Database.");
                return;
            }
            StartCoroutine(SyncPendingTelemetryRoutine());
        }

        private IEnumerator SyncPendingTelemetryRoutine()
        {
            List<TelemetryRecord> pendingRecords = DatabaseManager.Instance.DB.telemetryLogs.FindAll(r => r.sync_status == 0);

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
                foreach (var rec in pendingRecords)
                {
                    rec.sync_status = 1;
                }
                DatabaseManager.Instance.SaveDatabase();
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

        [Serializable] private struct TelemetryBatch { public List<TelemetryRecord> records; }
    }
}