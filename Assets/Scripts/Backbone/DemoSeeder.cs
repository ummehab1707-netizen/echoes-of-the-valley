using UnityEngine;
using EchoesOfTheValley.Backbone;

public class DemoSeeder : MonoBehaviour
{
    void Start()
    {
        if (TelemetryLogger.Instance != null)
        {
            // Seed 5 historical gameplay sessions showing cognitive improvement over time
            TelemetryLogger.Instance.RecordGameSession("PATIENT_DEMO", "GamosaPatternMatch", 12500, 3);
            TelemetryLogger.Instance.RecordGameSession("PATIENT_DEMO", "GamosaPatternMatch", 10200, 2);
            TelemetryLogger.Instance.RecordGameSession("PATIENT_DEMO", "BambooSpatialPuzzle", 8800, 1);
            TelemetryLogger.Instance.RecordGameSession("PATIENT_DEMO", "GamosaPatternMatch", 7500, 0);
            TelemetryLogger.Instance.RecordGameSession("PATIENT_DEMO", "TeaLeafRoutine", 6200, 0);
            Debug.Log("[Person 4] Demo Telemetry Seeded Successfully!");
        }
    }
}