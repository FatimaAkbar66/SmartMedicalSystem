using System.Text;
using SmartMedicalSystem.Models.Entities;

namespace SmartMedicalSystem.Services
{
    public class PrescriptionPdfService
    {
        public byte[] GeneratePdf(Prescription prescription)
        {
            // Build HTML string
            var html = BuildHtml(prescription);

            // Return as UTF8 bytes
            // We serve this as HTML download
            // which browsers render perfectly
            return Encoding.UTF8.GetBytes(html);
        }

        private string BuildHtml(Prescription prescription)
        {
            var alerts = prescription.ValidationAlerts
                         ?? new List<ValidationAlert>();
            var items = prescription.Items
                         ?? new List<PrescriptionItem>();

            var sb = new StringBuilder();

            sb.Append(@"<!DOCTYPE html>
<html>
<head>
<meta charset='utf-8'/>
<title>Prescription - " + prescription.PrescriptionCode + @"</title>
<style>
  * { box-sizing: border-box; margin: 0; padding: 0; }
  body {
    font-family: Arial, sans-serif;
    font-size: 13px;
    color: #1e293b;
    padding: 30px;
    background: white;
  }
  .header {
    text-align: center;
    border-bottom: 3px solid #4b0082;
    padding-bottom: 15px;
    margin-bottom: 20px;
  }
  .header h1 {
    font-size: 20px;
    color: #4b0082;
    margin-bottom: 4px;
  }
  .header p {
    font-size: 11px;
    color: #64748b;
  }
  .rx-code {
    background: #f0e6ff;
    border: 1px solid #7b2fbe;
    border-radius: 8px;
    padding: 10px 16px;
    margin-bottom: 18px;
    display: flex;
    justify-content: space-between;
    align-items: center;
    flex-wrap: wrap;
    gap: 8px;
  }
  .rx-code span {
    font-size: 12px;
    color: #4b0082;
    font-weight: bold;
  }
  .info-grid {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 14px;
    margin-bottom: 18px;
  }
  .info-box {
    background: #f8f9fc;
    border: 1px solid #e2e8f0;
    border-radius: 8px;
    padding: 12px;
  }
  .info-box .label {
    font-size: 10px;
    font-weight: bold;
    color: #4b0082;
    text-transform: uppercase;
    letter-spacing: 0.05em;
    margin-bottom: 6px;
  }
  .info-box .name {
    font-size: 15px;
    font-weight: bold;
    color: #1e293b;
    margin-bottom: 4px;
  }
  .info-box .detail {
    font-size: 11px;
    color: #64748b;
    line-height: 1.6;
  }
  .section-title {
    font-size: 13px;
    font-weight: bold;
    color: #4b0082;
    text-transform: uppercase;
    letter-spacing: 0.05em;
    margin-bottom: 10px;
    margin-top: 18px;
  }
  .diagnosis-box {
    background: #f8f9fc;
    border-left: 4px solid #4b0082;
    padding: 10px 14px;
    margin-bottom: 18px;
    border-radius: 0 8px 8px 0;
  }
  table {
    width: 100%;
    border-collapse: collapse;
    margin-bottom: 18px;
    font-size: 12px;
  }
  table thead tr {
    background: #4b0082;
    color: white;
  }
  table thead th {
    padding: 9px 10px;
    text-align: left;
    font-size: 11px;
    font-weight: bold;
  }
  table tbody tr:nth-child(even) {
    background: #f8f4ff;
  }
  table tbody td {
    padding: 8px 10px;
    border-bottom: 1px solid #e2e8f0;
    vertical-align: middle;
  }
  .risk-box {
    display: inline-block;
    padding: 4px 12px;
    border-radius: 20px;
    font-weight: bold;
    font-size: 12px;
  }
  .risk-High   { background: #fee2e2; color: #b91c1c; }
  .risk-Medium { background: #fef3c7; color: #92400e; }
  .risk-Low    { background: #d1fae5; color: #065f46; }
  .alert-box {
    border-radius: 6px;
    padding: 10px 12px;
    margin-bottom: 8px;
    font-size: 12px;
  }
  .alert-Critical {
    background: #fef2f2;
    border-left: 4px solid #ef4444;
  }
  .alert-Warning {
    background: #fffbeb;
    border-left: 4px solid #f59e0b;
  }
  .alert-Info {
    background: #eff6ff;
    border-left: 4px solid #3b82f6;
  }
  .alert-type {
    font-weight: bold;
    font-size: 11px;
    margin-bottom: 3px;
  }
  .no-alerts {
    background: #f0fdf4;
    border: 1px solid #bbf7d0;
    border-radius: 8px;
    padding: 12px;
    text-align: center;
    color: #065f46;
    font-weight: bold;
    font-size: 13px;
  }
  .footer {
    margin-top: 30px;
    padding-top: 12px;
    border-top: 1px solid #e2e8f0;
    text-align: center;
    font-size: 10px;
    color: #94a3b8;
  }
  @media print {
    body { padding: 15px; }
    .no-print { display: none; }
  }
</style>
</head>
<body>

<!-- PRINT BUTTON -->
<div class='no-print' style='text-align:right;margin-bottom:15px;'>
  <button onclick='window.print()'
    style='background:#4b0082;color:white;border:none;
           padding:8px 20px;border-radius:6px;
           cursor:pointer;font-size:13px;'>
    🖨️ Print / Save as PDF
  </button>
</div>

<!-- HEADER -->
<div class='header'>
  <h1>Smart Medical Error Prevention System</h1>
  <p>Prescription Validation Report</p>
  <p>KICSIT, Kahuta — Department of Computer Science</p>
</div>

<!-- RX CODE ROW -->
<div class='rx-code'>
  <span>Rx No: " + prescription.PrescriptionCode + @"</span>
  <span>Date: " + prescription.DateIssued.ToString("dd MMM yyyy HH:mm") + @"</span>
  <span>Status: " + prescription.Status + @"</span>
  <span>
    AI Risk:
    <span class='risk-box risk-" + prescription.AIRiskLevel + @"'>
      " + prescription.AIRiskLevel + @"
      (" + (prescription.AIRiskScore * 100).ToString("F0") + @"%)
    </span>
  </span>
</div>

<!-- PATIENT + DOCTOR INFO -->
<div class='info-grid'>
  <div class='info-box'>
    <div class='label'>Patient Information</div>
    <div class='name'>" +
        (prescription.Patient?.User?.Name ?? "N/A") + @"</div>
    <div class='detail'>
      Gender  : " + (prescription.Patient?.Gender ?? "N/A") + @"<br/>
      Blood   : " + (prescription.Patient?.BloodGroup ?? "N/A") + @"<br/>
      Weight  : " + (prescription.Patient?.Weight.ToString() ?? "N/A") + @" kg<br/>");

            // Patient allergies
            if (prescription.Patient?.Allergies?.Any() == true)
            {
                sb.Append("<span style='color:#b91c1c;font-weight:bold;'>⚠ Allergies: ");
                sb.Append(string.Join(", ",
                    prescription.Patient.Allergies
                        .Select(a => $"{a.Allergen} ({a.Severity})")));
                sb.Append("</span>");
            }

            sb.Append(@"
    </div>
  </div>
  <div class='info-box'>
    <div class='label'>Prescribing Physician</div>
    <div class='name'>Dr. " +
        (prescription.Doctor?.User?.Name ?? "N/A") + @"</div>
    <div class='detail'>
      Specialization : " + (prescription.Doctor?.Specialization ?? "N/A") + @"<br/>
      License No     : " + (prescription.Doctor?.LicenseNo ?? "N/A") + @"<br/>
      Department     : " + (prescription.Doctor?.Department ?? "N/A") + @"
    </div>
  </div>
</div>

<!-- DIAGNOSIS -->
<div class='diagnosis-box'>
  <strong>Diagnosis:</strong> " + prescription.Diagnosis + @"
  " + (!string.IsNullOrEmpty(prescription.Notes)
        ? $"<br/><strong>Notes:</strong> {prescription.Notes}"
        : "") + @"
</div>

<!-- MEDICATIONS -->
<div class='section-title'>Prescribed Medications</div>
<table>
  <thead>
    <tr>
      <th>#</th>
      <th>Medicine</th>
      <th>Active Ingredient</th>
      <th>Dose (mg)</th>
      <th>Frequency</th>
      <th>Duration</th>
      <th>Route</th>
      <th>Instructions</th>
    </tr>
  </thead>
  <tbody>");

            if (items.Any())
            {
                int n = 1;
                foreach (var item in items)
                {
                    sb.Append($@"
    <tr>
      <td>{n++}</td>
      <td><strong>{item.Medicine?.GenericName ?? "N/A"}</strong><br/>
          <span style='font-size:10px;color:#64748b;'>
            {item.Medicine?.BrandNames?.Split(',').FirstOrDefault() ?? ""}
          </span>
      </td>
      <td style='font-size:11px;'>{item.Medicine?.ActiveIngredient ?? "N/A"}</td>
      <td><strong>{item.Dosage:F0}</strong></td>
      <td>{item.Frequency}</td>
      <td>{item.DurationDays} days</td>
      <td>{item.Route}</td>
      <td style='font-size:11px;'>{item.Instructions ?? "—"}</td>
    </tr>");
                }
            }
            else
            {
                sb.Append(@"
    <tr>
      <td colspan='8' style='text-align:center;
                              color:#64748b;padding:16px;'>
        No medications recorded
      </td>
    </tr>");
            }

            sb.Append(@"
  </tbody>
</table>

<!-- SAFETY ALERTS -->
<div class='section-title'>Safety Validation Results</div>");

            if (alerts.Any())
            {
                foreach (var alert in alerts)
                {
                    sb.Append($@"
<div class='alert-box alert-{alert.Severity}'>
  <div class='alert-type'>
    [{alert.AlertType}] — {alert.Severity}
    {(alert.IsOverridden
        ? "<span style='color:#d97706;'> [OVERRIDDEN]</span>"
        : "")}
  </div>
  <div>{alert.Message}</div>
  {(alert.IsOverridden && !string.IsNullOrEmpty(alert.OverrideReason)
      ? $"<div style='margin-top:4px;color:#64748b;font-size:11px;'>Override by {alert.OverriddenBy}: {alert.OverrideReason}</div>"
      : "")}
</div>");
                }
            }
            else
            {
                sb.Append(@"
<div class='no-alerts'>
  ✓ No safety alerts — This is a clean prescription
</div>");
            }

            sb.Append(@"

<!-- FOOTER -->
<div class='footer'>
  Generated by Smart Medical Error Prevention System |
  KICSIT Kahuta, Dept. of Computer Science |
  Printed: " + DateTime.Now.ToString("dd MMM yyyy HH:mm") + @"
</div>

</body>
</html>");

            return sb.ToString();
        }
    }
}