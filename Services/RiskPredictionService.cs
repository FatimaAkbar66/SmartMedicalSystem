using Microsoft.ML;
using Microsoft.ML.Data;
using SmartMedicalSystem.Models.Entities;
using SmartMedicalSystem.Models.MLModels;

namespace SmartMedicalSystem.Services
{
    public class RiskPredictionService
    {
        private readonly MLContext _mlContext;
        private ITransformer? _model;
        private readonly string _modelPath;
        private readonly ILogger<RiskPredictionService> _logger;

        public RiskPredictionService(
            IWebHostEnvironment env,
            ILogger<RiskPredictionService> logger)
        {
            _mlContext = new MLContext(seed: 42);
            _logger = logger;
            _modelPath = Path.Combine(
                env.ContentRootPath, "MLModels", "risk_model.zip");

            LoadOrTrainModel();
        }

        // ─────────────────────────────────────────────────────
        // LOAD existing model OR train a new one
        // ─────────────────────────────────────────────────────
        private void LoadOrTrainModel()
        {
            if (File.Exists(_modelPath))
            {
                try
                {
                    _model = _mlContext.Model.Load(_modelPath, out _);
                    _logger.LogInformation(
                        "✅ ML.NET model loaded from: {path}", _modelPath);
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        "Model load failed: {msg}. Retraining...", ex.Message);
                }
            }

            TrainAndSaveModel();
        }

        // ─────────────────────────────────────────────────────
        // TRAIN model using synthetic clinical data
        // ─────────────────────────────────────────────────────
        private void TrainAndSaveModel()
        {
            _logger.LogInformation("🔄 Training ML.NET risk model...");

            // Generate 1500 synthetic patient records
            var trainingData = GenerateSyntheticData(1500);
            var dataView = _mlContext.Data.LoadFromEnumerable(trainingData);

            // Build training pipeline
            var pipeline = _mlContext.Transforms
                .Concatenate("Features",
                    nameof(PatientRiskInput.Age),
                    nameof(PatientRiskInput.Weight),
                    nameof(PatientRiskInput.NumberOfMedications),
                    nameof(PatientRiskInput.AllergyCount),
                    nameof(PatientRiskInput.ChronicConditionCount),
                    nameof(PatientRiskInput.HasDiabetes),
                    nameof(PatientRiskInput.HasHeartDisease),
                    nameof(PatientRiskInput.HasKidneyDisease),
                    nameof(PatientRiskInput.HasLiverDisease),
                    nameof(PatientRiskInput.PreviousAdverseEvents),
                    nameof(PatientRiskInput.IsElderly),
                    nameof(PatientRiskInput.IsPediatric))
                .Append(_mlContext.BinaryClassification.Trainers.FastTree(
                    labelColumnName: "Label",
                    featureColumnName: "Features",
                    numberOfTrees: 100,
                    numberOfLeaves: 20,
                    minimumExampleCountPerLeaf: 5,
                    learningRate: 0.1));

            // Split data: 80% train, 20% test
            var split = _mlContext.Data.TrainTestSplit(
                dataView, testFraction: 0.2);

            _model = pipeline.Fit(split.TrainSet);

            // Evaluate accuracy
            var predictions = _model.Transform(split.TestSet);
            var metrics = _mlContext.BinaryClassification
                                        .Evaluate(predictions, labelColumnName: "Label");

            _logger.LogInformation(
                "✅ Model trained — Accuracy: {acc:P1} | AUC: {auc:P1}",
                metrics.Accuracy,
                metrics.AreaUnderRocCurve);

            // Save model to disk
            Directory.CreateDirectory(
                Path.GetDirectoryName(_modelPath)!);

            _mlContext.Model.Save(_model, dataView.Schema, _modelPath);

            _logger.LogInformation(
                "💾 Model saved to: {path}", _modelPath);
        }

        // ─────────────────────────────────────────────────────
        // GENERATE synthetic training data
        // Based on WHO clinical risk guidelines
        // ─────────────────────────────────────────────────────
        private List<PatientRiskInput> GenerateSyntheticData(int count)
        {
            var rng = new Random(42);
            var data = new List<PatientRiskInput>();

            for (int i = 0; i < count; i++)
            {
                var age = (float)rng.Next(2, 90);
                var weight = (float)rng.Next(10, 150);
                var meds = (float)rng.Next(0, 12);
                var allerg = (float)rng.Next(0, 6);
                var chronic = (float)rng.Next(0, 5);
                var diab = rng.NextDouble() < 0.20 ? 1f : 0f;
                var heart = rng.NextDouble() < 0.15 ? 1f : 0f;
                var kidney = rng.NextDouble() < 0.10 ? 1f : 0f;
                var liver = rng.NextDouble() < 0.08 ? 1f : 0f;
                var adverse = (float)rng.Next(0, 4);
                var elderly = age >= 65 ? 1f : 0f;
                var peds = age <= 12 ? 1f : 0f;

                // Clinical risk scoring formula
                float score = 0;
                score += meds * 0.15f;
                score += allerg * 0.20f;
                score += chronic * 0.20f;
                score += diab * 0.30f;
                score += heart * 0.40f;
                score += kidney * 0.40f;
                score += liver * 0.35f;
                score += adverse * 0.40f;
                score += elderly * 0.25f;
                score += peds * 0.15f;
                if (weight < 20 || weight > 130) score += 0.15f;

                data.Add(new PatientRiskInput
                {
                    Age = age,
                    Weight = weight,
                    NumberOfMedications = meds,
                    AllergyCount = allerg,
                    ChronicConditionCount = chronic,
                    HasDiabetes = diab,
                    HasHeartDisease = heart,
                    HasKidneyDisease = kidney,
                    HasLiverDisease = liver,
                    PreviousAdverseEvents = adverse,
                    IsElderly = elderly,
                    IsPediatric = peds,
                    Label = score > 1.2f   // high risk threshold
                });
            }

            return data;
        }

        // ─────────────────────────────────────────────────────
        // PREDICT — called from Controllers
        // ─────────────────────────────────────────────────────
        public RiskAssessmentResult AssessRisk(
            Patient patient,
            List<MedicalHistory> histories,
            List<Allergy> allergies,
            int medicationCount = 0)
        {
            if (_model == null)
                return new RiskAssessmentResult
                {
                    RiskLevel = "Unknown",
                    Summary = "ML model not available."
                };

            // Calculate age
            float age = patient.DateOfBirth.HasValue
                ? (float)(DateTime.Today - patient.DateOfBirth.Value)
                         .TotalDays / 365.25f
                : 35f;

            // Extract chronic conditions from history
            var conditions = histories
                .SelectMany(h =>
                    (h.ChronicConditions ?? "")
                    .Split(',', StringSplitOptions.RemoveEmptyEntries))
                .Select(c => c.Trim().ToLower())
                .ToList();

            // Build input object
            var input = new PatientRiskInput
            {
                Age = age,
                Weight = (float)patient.Weight,
                NumberOfMedications = medicationCount,
                AllergyCount = allergies.Count,
                ChronicConditionCount = conditions.Count,
                HasDiabetes = conditions.Any(c =>
                    c.Contains("diabet")) ? 1f : 0f,
                HasHeartDisease = conditions.Any(c =>
                    c.Contains("heart") ||
                    c.Contains("cardiac") ||
                    c.Contains("coronary")) ? 1f : 0f,
                HasKidneyDisease = conditions.Any(c =>
                    c.Contains("kidney") ||
                    c.Contains("renal")) ? 1f : 0f,
                HasLiverDisease = conditions.Any(c =>
                    c.Contains("liver") ||
                    c.Contains("hepat")) ? 1f : 0f,
                PreviousAdverseEvents = 0f,
                IsElderly = age >= 65 ? 1f : 0f,
                IsPediatric = age <= 12 ? 1f : 0f
            };

            // Run prediction
            var engine = _mlContext.Model
                .CreatePredictionEngine<PatientRiskInput,
                                        PatientRiskPrediction>(_model);

            var prediction = engine.Predict(input);

            // Build result
            var result = new RiskAssessmentResult
            {
                RiskScore = prediction.Probability,
                Confidence = MathF.Abs(prediction.Score)
            };

            // Classify risk level
            if (prediction.Probability >= 0.70f)
            { result.RiskLevel = "High"; result.RiskColor = "danger"; }
            else if (prediction.Probability >= 0.35f)
            { result.RiskLevel = "Medium"; result.RiskColor = "warning"; }
            else
            { result.RiskLevel = "Low"; result.RiskColor = "success"; }

            // Contributing factors
            if (input.HasDiabetes == 1) result.ContributingFactors.Add("Diabetes Mellitus");
            if (input.HasHeartDisease == 1) result.ContributingFactors.Add("Cardiac Disease");
            if (input.HasKidneyDisease == 1) result.ContributingFactors.Add("Renal Impairment");
            if (input.HasLiverDisease == 1) result.ContributingFactors.Add("Hepatic Disease");
            if (input.IsElderly == 1) result.ContributingFactors.Add("Elderly Patient (≥65 yrs)");
            if (input.IsPediatric == 1) result.ContributingFactors.Add("Pediatric Patient (≤12 yrs)");
            if (input.AllergyCount > 2) result.ContributingFactors.Add($"Multiple Allergies ({input.AllergyCount})");
            if (input.NumberOfMedications > 5) result.ContributingFactors.Add($"Polypharmacy ({input.NumberOfMedications} drugs)");
            if (input.Weight < 40 || input.Weight > 120)
                result.ContributingFactors.Add($"Abnormal Weight ({input.Weight} kg)");

            // Recommendations
            if (result.RiskLevel == "High")
            {
                result.Recommendations.Add("Enhanced monitoring before prescribing");
                result.Recommendations.Add("Start at lowest effective dose");
                result.Recommendations.Add("Consult specialist if multiple risk factors");
                result.Recommendations.Add("Schedule follow-up within 48 hours");
            }
            else if (result.RiskLevel == "Medium")
            {
                result.Recommendations.Add("Monitor drug response closely");
                result.Recommendations.Add("Review all interactions carefully");
                result.Recommendations.Add("Follow-up within 1 week");
            }
            else
            {
                result.Recommendations.Add("Standard monitoring protocol applies");
                result.Recommendations.Add("Educate patient on medication adherence");
            }

            // Summary text
            var factorText = result.ContributingFactors.Any()
                ? $"Key factors: {string.Join(", ", result.ContributingFactors.Take(3))}."
                : "No major risk factors identified.";

            result.Summary =
                $"Patient is classified as {result.RiskLevel} Risk " +
                $"({result.RiskScore:P0} adverse event probability). {factorText}";

            return result;
        }
    }
}