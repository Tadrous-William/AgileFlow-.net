using AgileTaskManager.Models.ViewModels;
using AgileTaskManager.Services.Interfaces;
using AgileTaskManager.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgileTaskManager.Controllers.Api;

[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportingApiController : ControllerBase
{
    private readonly IReportingService _reportingService;

    public ReportingApiController(IReportingService reportingService)
    {
        _reportingService = reportingService;
    }

    [HttpGet("templates")]
    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.TeamLead}")]
    public async Task<IActionResult> GetReportTemplates()
    {
        var templates = await _reportingService.GetReportTemplatesAsync();
        return Ok(templates);
    }

    [HttpGet("templates/{id:int}")]
    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.TeamLead}")]
    public async Task<IActionResult> GetReportTemplate(int id)
    {
        var template = await _reportingService.GetReportTemplateAsync(id);
        if (template == null)
            return NotFound(new { error = "Report template not found" });

        return Ok(template);
    }

    [HttpPost("templates")]
    [Authorize(Roles = RoleConstants.Admin)]
    public async Task<IActionResult> CreateReportTemplate([FromBody] CreateReportTemplateViewModel request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var template = await _reportingService.CreateReportTemplateAsync(request);
        return CreatedAtAction(nameof(GetReportTemplate), new { id = template.Id }, template);
    }

    [HttpPut("templates/{id:int}")]
    [Authorize(Roles = RoleConstants.Admin)]
    public async Task<IActionResult> UpdateReportTemplate(int id, [FromBody] UpdateReportTemplateViewModel request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var success = await _reportingService.UpdateReportTemplateAsync(id, request);
        if (!success)
            return NotFound(new { error = "Report template not found" });

        return NoContent();
    }

    [HttpDelete("templates/{id:int}")]
    [Authorize(Roles = RoleConstants.Admin)]
    public async Task<IActionResult> DeleteReportTemplate(int id)
    {
        var success = await _reportingService.DeleteReportTemplateAsync(id);
        if (!success)
            return NotFound(new { error = "Report template not found" });

        return NoContent();
    }

    [HttpGet("scheduled")]
    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.TeamLead}")]
    public async Task<IActionResult> GetScheduledReports()
    {
        var reports = await _reportingService.GetScheduledReportsAsync();
        return Ok(reports);
    }

    [HttpGet("scheduled/{id:int}")]
    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.TeamLead}")]
    public async Task<IActionResult> GetScheduledReport(int id)
    {
        var report = await _reportingService.GetScheduledReportAsync(id);
        if (report == null)
            return NotFound(new { error = "Scheduled report not found" });

        return Ok(report);
    }

    [HttpPost("scheduled")]
    [Authorize(Roles = RoleConstants.Admin)]
    public async Task<IActionResult> CreateScheduledReport([FromBody] CreateScheduledReportViewModel request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var report = await _reportingService.CreateScheduledReportAsync(request);
        return CreatedAtAction(nameof(GetScheduledReport), new { id = report.Id }, report);
    }

    [HttpPut("scheduled/{id:int}")]
    [Authorize(Roles = RoleConstants.Admin)]
    public async Task<IActionResult> UpdateScheduledReport(int id, [FromBody] UpdateScheduledReportViewModel request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var success = await _reportingService.UpdateScheduledReportAsync(id, request);
        if (!success)
            return NotFound(new { error = "Scheduled report not found" });

        return NoContent();
    }

    [HttpDelete("scheduled/{id:int}")]
    [Authorize(Roles = RoleConstants.Admin)]
    public async Task<IActionResult> DeleteScheduledReport(int id)
    {
        var success = await _reportingService.DeleteScheduledReportAsync(id);
        if (!success)
            return NotFound(new { error = "Scheduled report not found" });

        return NoContent();
    }

    [HttpPost("scheduled/{id:int}/execute")]
    [Authorize(Roles = RoleConstants.Admin)]
    public async Task<IActionResult> ExecuteScheduledReport(int id)
    {
        var success = await _reportingService.ExecuteScheduledReportAsync(id);
        if (!success)
            return NotFound(new { error = "Scheduled report not found" });

        return Ok(new { message = "Report execution initiated" });
    }

    [HttpGet("history")]
    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.TeamLead}")]
    public async Task<IActionResult> GetReportHistory([FromQuery] string? userId = null, [FromQuery] int count = 50)
    {
        var history = await _reportingService.GetReportHistoryAsync(userId, count);
        return Ok(history);
    }

    [HttpGet("history/{id:int}")]
    [Authorize]
    public async Task<IActionResult> GetReportHistoryItem(int id)
    {
        var history = await _reportingService.GetReportHistoryAsync(id);
        if (history == null)
            return NotFound(new { error = "Report history not found" });

        return Ok(history);
    }

    [HttpPost("generate")]
    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.TeamLead}")]
    public async Task<IActionResult> GenerateReport([FromBody] ReportRequestViewModel request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var report = await _reportingService.GenerateReportAsync(request);
        return Ok(report);
    }

    [HttpPost("preview")]
    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.TeamLead}")]
    public async Task<IActionResult> PreviewReport([FromBody] ReportRequestViewModel request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var preview = await _reportingService.PreviewReportAsync(request);
        return Ok(preview);
    }

    [HttpPost("export")]
    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.TeamLead}")]
    public async Task<IActionResult> ExportReport([FromBody] ReportGenerationViewModel report, [FromQuery] string format = "PDF")
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var fileBytes = await _reportingService.ExportReportAsync(report, format);
        var contentType = format.ToLowerInvariant() switch
        {
            "pdf" => "application/pdf",
            "excel" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "csv" => "text/csv",
            "json" => "application/json",
            _ => "application/octet-stream"
        };

        var fileName = $"{report.ReportName.Replace(" ", "_")}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{format.ToLowerInvariant()}";

        return File(fileBytes, contentType, fileName);
    }

    [HttpGet("statistics")]
    [Authorize(Roles = RoleConstants.Admin)]
    public async Task<IActionResult> GetReportStatistics([FromQuery] DateTime? fromDate = null, DateTime? toDate = null)
    {
        var stats = await _reportingService.GetReportStatisticsAsync(fromDate, toDate);
        return Ok(stats);
    }

    [HttpGet("custom-fields/{reportType}")]
    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.TeamLead}")]
    public async Task<IActionResult> GetCustomFields(string reportType)
    {
        var fields = await _reportingService.GetCustomFieldsAsync(reportType);
        return Ok(fields);
    }

    [HttpPost("custom-fields/{reportType}")]
    [Authorize(Roles = RoleConstants.Admin)]
    public async Task<IActionResult> SaveCustomFields(string reportType, [FromBody] List<CustomFieldViewModel> fields)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var success = await _reportingService.SaveCustomFieldsAsync(reportType, fields);
        if (success)
            return Ok(new { message = "Custom fields saved successfully" });

        return BadRequest(new { error = "Failed to save custom fields" });
    }

    [HttpPost("validate")]
    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.TeamLead}")]
    public async Task<IActionResult> ValidateReportRequest([FromBody] ReportRequestViewModel request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var validation = await _reportingService.ValidateReportRequestAsync(request);
        return Ok(validation);
    }

    [HttpGet("permissions/{userId}")]
    [Authorize(Roles = RoleConstants.Admin)]
    public async Task<IActionResult> GetReportPermissions(string userId)
    {
        var permissions = await _reportingService.GetReportPermissionsAsync(userId);
        return Ok(permissions);
    }

    [HttpPost("permissions/{userId}/grant")]
    [Authorize(Roles = RoleConstants.Admin)]
    public async Task<IActionResult> GrantReportPermission(string userId, [FromBody] ReportPermissionRequestViewModel request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var success = await _reportingService.GrantReportPermissionAsync(userId, request.ReportType, request.Permission);
        if (success)
            return Ok(new { message = "Permission granted successfully" });

        return BadRequest(new { error = "Failed to grant permission" });
    }

    [HttpPost("permissions/{userId}/revoke")]
    [Authorize(Roles = RoleConstants.Admin)]
    public async Task<IActionResult> RevokeReportPermission(string userId, [FromBody] ReportPermissionRequestViewModel request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var success = await _reportingService.RevokeReportPermissionAsync(userId, request.ReportType, request.Permission);
        if (success)
            return Ok(new { message = "Permission revoked successfully" });

        return BadRequest(new { error = "Failed to revoke permission" });
    }
}

public class ReportPermissionRequestViewModel
{
    public string ReportType { get; set; } = string.Empty;
    public string Permission { get; set; } = string.Empty;
}
