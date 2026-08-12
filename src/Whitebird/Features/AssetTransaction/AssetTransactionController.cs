using Microsoft.AspNetCore.Mvc;
using Whitebird.App.Features.AssetTransaction;
using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.AssetTransaction;
using Whitebird.Features.Common;

namespace Whitebird.Features.AssetTransaction.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AssetTransactionController : ControllerBase
{
    private readonly IAssetTransactionService _transactionService;
    private readonly TransactionImportService _transactionImportService;

    public AssetTransactionController(
        IAssetTransactionService transactionService,
        TransactionImportService transactionImportService)
    {
        _transactionService = transactionService;
        _transactionImportService = transactionImportService;
    }

    // ============================================================
    // CRUD
    // ============================================================

    /// <summary>
    /// Get transaction by ID
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
        => this.HandleResult(await _transactionService.GetByIdAsync(id));

    /// <summary>
    /// Get all transactions
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
        => this.HandleResult(await _transactionService.GetAllAsync());

    /// <summary>
    /// Create new transaction (ADD type only)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AssetTransactionCreateViewModel model)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _transactionService.CreateAsync(model), nameof(GetById));
    }

    /// <summary>
    /// Update transaction (pending only)
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] AssetTransactionUpdateViewModel model)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _transactionService.UpdateAsync(id, model));
    }

    /// <summary>
    /// Hard delete transaction
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
        => this.HandleResult(await _transactionService.DeleteAsync(id));

    /// <summary>
    /// Soft delete transaction
    /// </summary>
    [HttpDelete("{id:int}/soft")]
    public async Task<IActionResult> SoftDelete(int id)
        => this.HandleResult(await _transactionService.SoftDeleteAsync(id));

    // ============================================================
    // BULK APPROVAL
    // ============================================================

    /// <summary>
    /// Bulk approve transactions (pending → approved)
    /// </summary>
    [HttpPost("bulk-approved")]
    public async Task<IActionResult> BulkApproved([FromBody] BulkApprovalRequest request)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _transactionService.BulkApprovedAsync(request));
    }

    /// <summary>
    /// Bulk pending transactions (approved → pending)
    /// </summary>
    [HttpPost("bulk-pending")]
    public async Task<IActionResult> BulkPending([FromBody] BulkApprovalRequest request)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _transactionService.BulkPendingAsync(request));
    }

    /// <summary>
    /// Bulk reject transactions (pending/approved → rejected)
    /// </summary>
    [HttpPost("bulk-rejected")]
    public async Task<IActionResult> BulkRejected([FromBody] BulkApprovalRequest request)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _transactionService.BulkRejectedAsync(request));
    }

    // ============================================================
    // COMPLETE / UNCOMPLETE
    // ============================================================

    /// <summary>
    /// Complete transaction (requires all required documents uploaded and approved)
    /// </summary>
    [HttpPost("{id:int}/complete")]
    public async Task<IActionResult> Complete(int id)
        => this.HandleResult(await _transactionService.CompleteAsync(id));

    /// <summary>
    /// Uncomplete transaction (revert to approved)
    /// </summary>
    [HttpPost("{id:int}/uncomplete")]
    public async Task<IActionResult> Uncomplete(int id)
        => this.HandleResult(await _transactionService.UncompleteAsync(id));

    // ============================================================
    // GRID
    // ============================================================

    /// <summary>
    /// Get grid for ADD type transactions (HANDOVER, TRANSFER, LOAN, MAINTENANCE, DISPOSAL, OPERATIONAL OFFICE)
    /// </summary>
    [HttpGet("grid-add")]
    public async Task<IActionResult> GetGridAdd(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] int? employeeId = null,
        [FromQuery] int? assetId = null,
        [FromQuery] int? transactionType = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = true)
    {
        var request = new AssetTransactionGridRequest
        {
            Page = page,
            PageSize = pageSize,
            Search = search,
            DateFrom = dateFrom,
            DateTo = dateTo,
            EmployeeId = employeeId,
            AssetId = assetId,
            TransactionType = transactionType,
            SortBy = sortBy,
            SortDescending = sortDescending
        };

        return this.HandleResult(await _transactionService.GetGridAddAsync(request));
    }

    /// <summary>
    /// Get grid for RETURN type transactions (RETURN, LOAN_RETURN, POST_MAINTENANCE)
    /// </summary>
    [HttpGet("grid-return")]
    public async Task<IActionResult> GetGridReturn(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] int? employeeId = null,
        [FromQuery] int? assetId = null,
        [FromQuery] int? transactionType = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = true)
    {
        var request = new AssetTransactionGridRequest
        {
            Page = page,
            PageSize = pageSize,
            Search = search,
            DateFrom = dateFrom,
            DateTo = dateTo,
            EmployeeId = employeeId,
            AssetId = assetId,
            TransactionType = transactionType,
            SortBy = sortBy,
            SortDescending = sortDescending
        };

        return this.HandleResult(await _transactionService.GetGridReturnAsync(request));
    }

    /// <summary>
    /// Get grid for pending approval transactions (approved, all required documents uploaded)
    /// </summary>
    [HttpGet("grid-pending-approval")]
    public async Task<IActionResult> GetGridPendingApproval(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] int? employeeId = null,
        [FromQuery] int? assetId = null,
        [FromQuery] int? transactionType = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = true)
    {
        var request = new AssetTransactionGridRequest
        {
            Page = page,
            PageSize = pageSize,
            Search = search,
            DateFrom = dateFrom,
            DateTo = dateTo,
            EmployeeId = employeeId,
            AssetId = assetId,
            TransactionType = transactionType,
            SortBy = sortBy,
            SortDescending = sortDescending
        };

        return this.HandleResult(await _transactionService.GetGridPendingApprovalAsync(request));
    }

    // ============================================================
    // OUTSTANDING (VIEW ONLY)
    // ============================================================

    /// <summary>
    /// Get active loans (view only)
    /// </summary>
    [HttpGet("active-loans")]
    public async Task<IActionResult> GetActiveLoans(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] int? employeeId = null,
        [FromQuery] int? assetId = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = true)
    {
        var request = new AssetTransactionGridRequest
        {
            Page = page,
            PageSize = pageSize,
            Search = search,
            DateFrom = dateFrom,
            DateTo = dateTo,
            EmployeeId = employeeId,
            AssetId = assetId,
            SortBy = sortBy,
            SortDescending = sortDescending
        };

        return this.HandleResult(await _transactionService.GetActiveLoansAsync(request));
    }

    /// <summary>
    /// Get overdue loans (view only)
    /// </summary>
    [HttpGet("overdue-loans")]
    public async Task<IActionResult> GetOverdueLoans(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] int? employeeId = null,
        [FromQuery] int? assetId = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = true)
    {
        var request = new AssetTransactionGridRequest
        {
            Page = page,
            PageSize = pageSize,
            Search = search,
            DateFrom = dateFrom,
            DateTo = dateTo,
            EmployeeId = employeeId,
            AssetId = assetId,
            SortBy = sortBy,
            SortDescending = sortDescending
        };

        return this.HandleResult(await _transactionService.GetOverdueLoansAsync(request));
    }

    /// <summary>
    /// Get under maintenance (view only)
    /// </summary>
    [HttpGet("under-maintenance")]
    public async Task<IActionResult> GetUnderMaintenance(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] int? assetId = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = true)
    {
        var request = new AssetTransactionGridRequest
        {
            Page = page,
            PageSize = pageSize,
            Search = search,
            DateFrom = dateFrom,
            DateTo = dateTo,
            AssetId = assetId,
            SortBy = sortBy,
            SortDescending = sortDescending
        };

        return this.HandleResult(await _transactionService.GetUnderMaintenanceAsync(request));
    }

    // ============================================================
    // SHORTCUT
    // ============================================================

    /// <summary>
    /// Create RETURN transaction from HANDOVER/LOAN/OPERATIONAL OFFICE
    /// </summary>
    [HttpPost("{id:int}/create-return")]
    public async Task<IActionResult> CreateReturn(int id, [FromBody] AssetReturnViewModel model)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _transactionService.CreateReturnAsync(id, model), nameof(GetById));
    }

    /// <summary>
    /// Create POST_MAINTENANCE transaction from MAINTENANCE
    /// </summary>
    [HttpPost("{id:int}/create-post-maintenance")]
    public async Task<IActionResult> CreatePostMaintenance(int id, [FromBody] PostMaintenanceViewModel model)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _transactionService.CreatePostMaintenanceAsync(id, model), nameof(GetById));
    }

    // ============================================================
    // ATTACHMENT (UMUM)
    // ============================================================

    /// <summary>
    /// Upload attachment for transaction
    /// </summary>
    [HttpPost("{id:int}/attachment")]
    public async Task<IActionResult> UploadAttachment(int id, [FromForm] AssetTransactionAttachmentUploadDto model)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _transactionService.UploadAttachmentAsync(id, model));
    }

    /// <summary>
    /// Get attachments for transaction
    /// </summary>
    [HttpGet("{id:int}/attachment")]
    public async Task<IActionResult> GetAttachments(int id)
        => this.HandleResult(await _transactionService.GetAttachmentsAsync(id));

    /// <summary>
    /// Delete attachment from transaction
    /// </summary>
    [HttpDelete("{id:int}/attachment/{attachmentId:int}")]
    public async Task<IActionResult> DeleteAttachment(int id, int attachmentId)
        => this.HandleResult(await _transactionService.DeleteAttachmentAsync(id, attachmentId));

    // ============================================================
    // APPROVE ATTACHMENT
    // ============================================================

    /// <summary>
    /// Upload approve attachment for transaction (only after approved)
    /// </summary>
    [HttpPost("{id:int}/approve-attachment")]
    public async Task<IActionResult> UploadApproveAttachment(int id, [FromForm] AssetTransactionApproveAttachmentUploadDto model)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _transactionService.UploadApproveAttachmentAsync(id, model));
    }

    /// <summary>
    /// Get approve attachments for transaction
    /// </summary>
    [HttpGet("{id:int}/approve-attachment")]
    public async Task<IActionResult> GetApproveAttachments(int id)
        => this.HandleResult(await _transactionService.GetApproveAttachmentsAsync(id));

    /// <summary>
    /// Delete approve attachment from transaction
    /// </summary>
    [HttpDelete("{id:int}/approve-attachment/{attachmentId:int}")]
    public async Task<IActionResult> DeleteApproveAttachment(int id, int attachmentId)
        => this.HandleResult(await _transactionService.DeleteApproveAttachmentAsync(id, attachmentId));

    /// <summary>
    /// Action on approve attachment (NeedToChange / Approve)
    /// </summary>
    [HttpPost("{id:int}/approve-attachment/{attachmentId:int}/action")]
    public async Task<IActionResult> ApproveAttachmentAction(int id, int attachmentId, [FromBody] ApproveAttachmentActionDto model)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _transactionService.ApproveAttachmentActionAsync(id, attachmentId, model));
    }

    /// <summary>
    /// Get required document types for transaction
    /// </summary>
    [HttpGet("{id:int}/approve-attachment/required")]
    public async Task<IActionResult> GetRequiredDocumentTypes(int id)
        => this.HandleResult(await _transactionService.GetRequiredDocumentTypesAsync(id));

    // ============================================================
    // DROPDOWN
    // ============================================================

    /// <summary>
    /// Get transaction types for ADD (HANDOVER, TRANSFER, LOAN, MAINTENANCE, DISPOSAL, OPERATIONAL OFFICE)
    /// </summary>
    [HttpGet("list-transactiontype-add")]
    public async Task<IActionResult> GetTransactionTypeAddList()
        => this.HandleResult(await _transactionService.GetTransactionTypeAddListAsync());

    /// <summary>
    /// Get transaction types for RETURN (RETURN, LOAN_RETURN, POST_MAINTENANCE)
    /// </summary>
    [HttpGet("list-transactiontype-return")]
    public async Task<IActionResult> GetTransactionTypeReturnList()
        => this.HandleResult(await _transactionService.GetTransactionTypeReturnListAsync());

    /// <summary>
    /// Get maintenance types
    /// </summary>
    [HttpGet("list-maintenance-type")]
    public async Task<IActionResult> GetMaintenanceTypeList()
        => this.HandleResult(await _transactionService.GetMaintenanceTypeListAsync());

    /// <summary>
    /// Get asset conditions
    /// </summary>
    [HttpGet("list-condition")]
    public async Task<IActionResult> GetConditionList()
        => this.HandleResult(await _transactionService.GetConditionListAsync());

    /// <summary>
    /// Get approve document types
    /// </summary>
    [HttpGet("list-approve-document-type")]
    public async Task<IActionResult> GetApproveDocumentTypeList()
        => this.HandleResult(await _transactionService.GetApproveDocumentTypeListAsync());

    /// <summary>
    /// Get active employees (excluding resigned)
    /// </summary>
    [HttpGet("list-employee")]
    public async Task<IActionResult> GetEmployeeList()
        => this.HandleResult(await _transactionService.GetEmployeeListAsync());

    /// <summary>
    /// Get available assets (not in active transaction)
    /// </summary>
    [HttpGet("{id:int}/list-asset")]
    public async Task<IActionResult> GetAssetList(int id)
        => this.HandleResult(await _transactionService.GetAssetListAsync(id));

    // ============================================================
    // IMPORT
    // ============================================================

    /// <summary>
    /// Import transactions from Excel (all imported as PENDING)
    /// </summary>
    [HttpPost("import")]
    public async Task<IActionResult> Import(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file provided");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension != ".xlsx" && extension != ".xls")
        {
            return BadRequest("Unsupported file format. Please upload Excel file (.xlsx or .xls).");
        }

        using var stream = file.OpenReadStream();
        var result = await _transactionImportService.ImportFromFileAsync(stream);
        return this.HandleResult(result);
    }

    /// <summary>
    /// Download import template
    /// </summary>
    [HttpGet("import/template")]
    public async Task<IActionResult> DownloadImportTemplate()
    {
        var result = await _transactionImportService.GenerateTemplateAsync();
        if (!result.IsSuccess || result.Data == null)
            return this.HandleResult(result);

        return File(result.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Transaction_Import_Template.xlsx");
    }
}