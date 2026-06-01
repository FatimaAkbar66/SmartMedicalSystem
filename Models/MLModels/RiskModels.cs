using Microsoft.ML.Data;

namespace SmartMedicalSystem.Models.MLModels
{
    // ─────────────────────────────────────────
    // INPUT — fed into ML.NET model
    // ─────────────────────────────────────────
    public class PatientRiskInput
    {
        [LoadColumn(0)]
        public float Age { get; set; }

        [LoadColumn(1)]
        public float Weight { get; set; }

        [LoadColumn(2)]
        public float NumberOfMedications { get; set; }

        [LoadColumn(3)]
        public float AllergyCount { get; set; }

        [LoadColumn(4)]
        public float ChronicConditionCount { get; set; }

        [LoadColumn(5)]
        public float HasDiabetes { get; set; }        // 0 or 1

        [LoadColumn(6)]
        public float HasHeartDisease { get; set; }    // 0 or 1

        [LoadColumn(7)]
        public float HasKidneyDisease { get; set; }   // 0 or 1

        [LoadColumn(8)]
        public float HasLiverDisease { get; set; }    // 0 or 1

        [LoadColumn(9)]
        public float PreviousAdverseEvents { get; set; }

        [LoadColumn(10)]
        public float IsElderly { get; set; }          // age >= 65

        [LoadColumn(11)]
        public float IsPediatric { get; set; }        // age <= 12

        [LoadColumn(12), ColumnName("Label")]
        public bool Label { get; set; }               // true = high risk
    }

    // ─────────────────────────────────────────
    // OUTPUT — returned by ML.NET model
    // ─────────────────────────────────────────
    public class PatientRiskPrediction
    {
        [ColumnName("PredictedLabel")]
        public bool PredictedLabel { get; set; }

        [ColumnName("Probability")]
        public float Probability { get; set; }

        [ColumnName("Score")]
        public float Score { get; set; }
    }

    // ─────────────────────────────────────────
    // RESULT — returned to UI/Controller
    // ─────────────────────────────────────────
    public class RiskAssessmentResult
    {
        public string RiskLevel { get; set; } = "Low";
        // Low | Medium | High

        public float RiskScore { get; set; }
        public float Confidence { get; set; }

        public string RiskColor { get; set; } = "success";
        // Bootstrap: success | warning | danger

        public List<string> ContributingFactors { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
        public string Summary { get; set; } = string.Empty;
    }
}