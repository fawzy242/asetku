using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Whitebird.App.Features.AssetTransaction;
using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.AssetTransaction;
using Whitebird.Features.Common;

namespace Whitebird.Features.AssetTransaction.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
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

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
        => this.HandleResult(await _transactionService.GetByIdAsync(id));

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => this.HandleResult(await _transactionService.GetAllAsync());

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AssetTransactionCreateViewModel model)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _transactionService.CreateAsync(model), nameof(GetById));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateWithAttachments(int id, [FromBody] AssetTransactionUpdateFormDto model)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _transactionService.UpdateWithAttachmentsAsync(id, model));
    }

    [HttpPut("{id:int}/approval")]
    public async Task<IActionResult> UpdateApprovalWithAttachments(int id, [FromBody] AssetTransactionUpdateApprovalFormDto model)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _transactionService.UpdateApprovalWithAttachmentsAsync(id, model));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
        => this.HandleResult(await _transactionService.DeleteAsync(id));

    [HttpDelete("{id:int}/soft")]
    public async Task<IActionResult> SoftDelete(int id)
        => this.HandleResult(await _transactionService.SoftDeleteAsync(id));

    // ============================================================
    // BULK APPROVAL
    // ============================================================

    [HttpPost("bulk-approved")]
    public async Task<IActionResult> BulkApproved([FromBody] BulkApprovalRequest request)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _transactionService.BulkApprovedAsync(request));
    }

    [HttpPost("bulk-pending")]
    public async Task<IActionResult> BulkPending([FromBody] BulkApprovalRequest request)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _transactionService.BulkPendingAsync(request));
    }

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

    [HttpPost("{id:int}/complete")]
    public async Task<IActionResult> Complete(int id)
        => this.HandleResult(await _transactionService.CompleteAsync(id));

    [HttpPost("{id:int}/uncomplete")]
    public async Task<IActionResult> Uncomplete(int id)
        => this.HandleResult(await _transactionService.UncompleteAsync(id));

    // ============================================================
    // GRID
    // ============================================================

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
        [FromQuery] string? status = null,
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
            Status = status,
            SortBy = sortBy,
            SortDescending = sortDescending
        };

        return this.HandleResult(await _transactionService.GetGridAddAsync(request));
    }

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
        [FromQuery] string? status = null,
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
            Status = status,
            SortBy = sortBy,
            SortDescending = sortDescending
        };

        return this.HandleResult(await _transactionService.GetGridReturnAsync(request));
    }

    [HttpGet("grid-uncomplete")]
    public async Task<IActionResult> GetUncompleteTransactions(
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

    [HttpGet("grid-complete")]
    public async Task<IActionResult> GetCompleteTransactions(
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
        return this.HandleResult(await _transactionService.GetCompleteTransactionsAsync(request));
    }

    // ============================================================
    // OUTSTANDING (VIEW ONLY)
    // ============================================================

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

    [HttpPost("{id:int}/create-return")]
    public async Task<IActionResult> CreateReturn(int id, [FromBody] AssetReturnViewModel model)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _transactionService.CreateReturnAsync(id, model), nameof(GetById));
    }

    [HttpPost("{id:int}/create-post-maintenance")]
    public async Task<IActionResult> CreatePostMaintenance(int id, [FromBody] PostMaintenanceViewModel model)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _transactionService.CreatePostMaintenanceAsync(id, model), nameof(GetById));
    }

    // ============================================================
    // DROPDOWN
    // ============================================================

    [HttpGet("list-transactiontype-add")]
    public async Task<IActionResult> GetTransactionTypeAddList()
        => this.HandleResult(await _transactionService.GetTransactionTypeAddListAsync());

    [HttpGet("list-transactiontype-return")]
    public async Task<IActionResult> GetTransactionTypeReturnList()
        => this.HandleResult(await _transactionService.GetTransactionTypeReturnListAsync());

    [HttpGet("list-maintenance-type")]
    public async Task<IActionResult> GetMaintenanceTypeList()
        => this.HandleResult(await _transactionService.GetMaintenanceTypeListAsync());

    [HttpGet("list-condition")]
    public async Task<IActionResult> GetConditionList()
        => this.HandleResult(await _transactionService.GetConditionListAsync());

    [HttpGet("list-approve-document-type")]
    public async Task<IActionResult> GetApproveDocumentTypeList()
        => this.HandleResult(await _transactionService.GetApproveDocumentTypeListAsync());

    [HttpGet("list-employee")]
    public async Task<IActionResult> GetEmployeeList()
        => this.HandleResult(await _transactionService.GetEmployeeListAsync());

    [HttpGet("{id:int}/list-asset")]
    public async Task<IActionResult> GetAssetList(int id)
        => this.HandleResult(await _transactionService.GetAssetListAsync(id));

    // ============================================================
    // IMPORT
    // ============================================================

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

    [HttpGet("import/template")]
    public async Task<IActionResult> DownloadImportTemplate()
    {
        var result = await _transactionImportService.GenerateTemplateAsync();
        if (!result.IsSuccess || result.Data == null)
            return this.HandleResult(result);

        return File(result.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Transaction_Import_Template.xlsx");
    }

    // ============================================================
    // ATTACHMENTS (standalone retrieval)
    // ============================================================

    [HttpGet("{id:int}/attachments")]
    public async Task<IActionResult> GetAttachments(int id)
        => this.HandleResult(await _transactionService.GetAttachmentsAsync(id));

    [HttpGet("{id:int}/approve-attachments")]
    public async Task<IActionResult> GetApproveAttachments(int id)
        => this.HandleResult(await _transactionService.GetApproveAttachmentsAsync(id));

    // ============================================================
    // APPROVAL DOCUMENT WORKFLOW
    // ============================================================

    /// <summary>
    /// Returns the required approval-document checklist for the transaction.
    /// Derived from MasterData.ReferenceName = 'ApproveDocumentType' joined with
    /// the transaction's current approval attachments.
    /// </summary>
    [HttpGet("{id:int}/required-documents")]
    public async Task<IActionResult> GetRequiredDocuments(int id)
        => this.HandleResult(await _transactionService.GetRequiredDocumentsAsync(id));

    /// <summary>
    /// Apply an action (approve / revision) to a single approval document.
    /// The transaction's Completed flag is automatically re-evaluated afterward.
    /// </summary>
    [HttpPost("{id:int}/approve-attachments/{attachmentId:int}/action")]
    public async Task<IActionResult> SetApproveAttachmentAction(
        int id, int attachmentId, [FromBody] ApproveAttachmentActionDto dto)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _transactionService.SetApproveAttachmentActionAsync(id, attachmentId, dto));
    }
}