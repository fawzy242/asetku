using Microsoft.Extensions.Logging;
using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.EmployeeSummary;
using Whitebird.Infra.Features.EmployeeSummary;
using Whitebird.Infra.Features.Common;

namespace Whitebird.App.Features.EmployeeSummary;

/// <summary>
/// Service implementation for Employee Summary business logic
/// </summary>
public class EmployeeSummaryService : BaseService, IEmployeeSummaryService
{
    private readonly IEmployeeSummaryReps _employeeSummaryReps;

    public EmployeeSummaryService(
        IEmployeeSummaryReps employeeSummaryReps,
        ILogger<EmployeeSummaryService> logger) : base(logger)
    {
        _employeeSummaryReps = employeeSummaryReps;
    }

    // ============================================================
    // LOOKUP / DROPDOWN
    // ============================================================

    public async Task<ServiceResult<EmployeeSummaryGetDataDto>> GetEmployeeDataAsync(int employeeId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var result = await _employeeSummaryReps.GetEmployeeDataAsync(employeeId);
            if (result == null)
            {
                return ServiceResult<EmployeeSummaryGetDataDto>.NotFound($"Employee with id {employeeId} not found");
            }
            return ServiceResult<EmployeeSummaryGetDataDto>.Success(result);
        }, "get employee data for summary");
    }

    // ============================================================
    // COMPLETE SUMMARY
    // ============================================================

    public async Task<ServiceResult<EmployeeSummaryView>> GetSummaryAsync(int employeeId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var result = await _employeeSummaryReps.GetSummaryAsync(employeeId);
            if (result == null)
            {
                return ServiceResult<EmployeeSummaryView>.NotFound($"Employee with id {employeeId} not found");
            }
            return ServiceResult<EmployeeSummaryView>.Success(result);
        }, "get employee summary");
    }

    // ============================================================
    // STATISTICS
    // ============================================================

    public async Task<ServiceResult<EmployeeSummaryStatsDto>> GetStatsAsync(int employeeId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var result = await _employeeSummaryReps.GetStatsAsync(employeeId);
            return ServiceResult<EmployeeSummaryStatsDto>.Success(result);
        }, "get employee stats");
    }

    // ============================================================
    // CURRENT HOLDING
    // ============================================================

    public async Task<ServiceResult<PaginatedResult<EmployeeCurrentHoldingDto>>> GetCurrentHoldingAsync(
        int employeeId, EmployeeSummaryRequest request)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var result = await _employeeSummaryReps.GetCurrentHoldingAsync(
                employeeId, 
                request.Page, 
                request.PageSize, 
                request.Search, 
                request.SortBy, 
                request.SortDescending);
            return ServiceResult<PaginatedResult<EmployeeCurrentHoldingDto>>.Success(result);
        }, "get current holding");
    }

    // ============================================================
    // ASSETS HISTORY
    // ============================================================

    public async Task<ServiceResult<PaginatedResult<EmployeeAssetsHistoryDto>>> GetAssetsHistoryAsync(
        int employeeId, EmployeeSummaryRequest request)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var result = await _employeeSummaryReps.GetAssetsHistoryAsync(
                employeeId, 
                request.Page, 
                request.PageSize, 
                request.Search, 
                request.SortBy, 
                request.SortDescending);
            return ServiceResult<PaginatedResult<EmployeeAssetsHistoryDto>>.Success(result);
        }, "get assets history");
    }

    // ============================================================
    // TRANSACTION HISTORY
    // ============================================================

    public async Task<ServiceResult<PaginatedResult<EmployeeTransactionHistoryDto>>> GetTransactionHistoryAsync(
        int employeeId, EmployeeSummaryRequest request)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var result = await _employeeSummaryReps.GetTransactionHistoryAsync(
                employeeId, 
                request.Page, 
                request.PageSize, 
                request.Search, 
                request.SortBy, 
                request.SortDescending);
            return ServiceResult<PaginatedResult<EmployeeTransactionHistoryDto>>.Success(result);
        }, "get transaction history");
    }

    // ============================================================
    // CURRENT HOLDING STATS (Category breakdown)
    // ============================================================

    public async Task<ServiceResult<EmployeeCategoryStatsResponse>> GetCurrentHoldingStatsAsync(int employeeId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var result = await _employeeSummaryReps.GetCurrentHoldingStatsAsync(employeeId);
            return ServiceResult<EmployeeCategoryStatsResponse>.Success(result);
        }, "get current holding stats");
    }

    // ============================================================
    // TRANSACTION STATS
    // ============================================================

    public async Task<ServiceResult<EmployeeTransactionStatsResponse>> GetTransactionStatsAsync(int employeeId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var result = await _employeeSummaryReps.GetTransactionStatsAsync(employeeId);
            return ServiceResult<EmployeeTransactionStatsResponse>.Success(result);
        }, "get transaction stats");
    }

    // ============================================================
    // SCHEDULE STATS
    // ============================================================

    public async Task<ServiceResult<EmployeeScheduleStatsDto>> GetScheduleStatsAsync(int employeeId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var result = await _employeeSummaryReps.GetScheduleStatsAsync(employeeId);
            return ServiceResult<EmployeeScheduleStatsDto>.Success(result);
        }, "get schedule stats");
    }
}