
// ── Implementation ────────────────────────────────────────────

using System.Data;
using System.Text.Json;
using Dapper;
using Microsoft.Data.SqlClient;
using Shop.Web.API.Exceptions;
using Shop.Web.API.Models.Domain;
using Shop.Web.API.Models.Requests;

namespace Shop.Web.API.Repositories;

public class SettingsRepository : ISettingsRepository
{
    private readonly string _conn;
    private readonly ILogger<SettingsRepository> _logger;

    public SettingsRepository(IConfiguration config, ILogger<SettingsRepository> logger)
    {
        _conn   = config.GetConnectionString("Default")
                  ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");
        _logger = logger;
    }

    // ── Get Settings ──────────────────────────────────────────

    public async Task<SettingsRecord?> GetSettingsAsync(int businessAccountId, int requestingUserId)
    {
        using var db = new SqlConnection(_conn);

        try
        {
            var record = await db.QuerySingleOrDefaultAsync<SettingsRecord>(
                "dbo.usp_Settings_Get",
                new { BusinessAccountId = businessAccountId, RequestingUserId = requestingUserId },
                commandType: CommandType.StoredProcedure);

            return record;
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex,
                "SQL error in GetSettingsAsync for BusinessAccountId={Id}", businessAccountId);
            throw;
        }
    }

    // ── Update Settings ───────────────────────────────────────

    //public async Task<SettingsRecord?> UpdateSettingsAsync(
    //    int businessAccountId, int requestingUserId, UpdateSettingsRequest req)
    //{
    //    using var db = new SqlConnection(_conn);

    //    // Decode the Base64 RowVersion sent from the client
    //    byte[] rowVersionBytes;
    //    try
    //    {
    //        rowVersionBytes = Convert.FromBase64String(req.RowVersion);
    //    }
    //    catch
    //    {
    //        throw new ArgumentException("Invalid row version format.", nameof(req.RowVersion));
    //    }

    //    var p = new DynamicParameters();
    //    p.Add("@BusinessAccountId", businessAccountId);
    //    p.Add("@RequestingUserId",  requestingUserId);
    //    p.Add("@BusinessName",      req.BusinessName);
    //    p.Add("@OwnerName",         req.OwnerName);
    //    p.Add("@BusinessPhone",     req.BusinessPhone);
    //    p.Add("@BusinessEmail",     req.BusinessEmail);
    //    p.Add("@Address",           req.Address);
    //    p.Add("@GSTIN",             req.GSTIN);
    //    p.Add("@ShopType",          req.ShopType);
    //    p.Add("@ThemeColor",        req.ThemeColor);
    //    p.Add("@Currency",          req.Currency);
    //    p.Add("@CurrencySymbol",    ResolveCurrencySymbol(req.Currency));
    //    p.Add("@RowVersion",        rowVersionBytes, DbType.Binary, size: 8);
    //    p.Add("@ErrorCode",         dbType: DbType.Int32,  direction: ParameterDirection.Output);
    //    p.Add("@ErrorMessage",      dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

    //    try
    //    {
    //        // SP re-queries and returns the updated row
    //        var record = await db.QuerySingleOrDefaultAsync<SettingsRecord>(
    //            "dbo.usp_Settings_Update", p,
    //            commandType: CommandType.StoredProcedure);

    //        int    errorCode    = p.Get<int>("@ErrorCode");
    //        string errorMessage = p.Get<string>("@ErrorMessage") ?? string.Empty;

    //        if (errorCode != 0)
    //        {
    //            _logger.LogWarning(
    //                "usp_Settings_Update returned error {Code}: {Msg} for BusinessAccountId={Id}",
    //                errorCode, errorMessage, businessAccountId);

    //            throw new AppException(errorMessage,
    //                errorCode == 2009 ? "CONCURRENCY_CONFLICT" : $"SETTINGS_{errorCode}",
    //                errorCode == 2009 ? 409 : 400);
    //        }

    //        return record;
    //    }
    //    catch (AppException) { throw; }
    //    catch (SqlException ex)
    //    {
    //        _logger.LogError(ex,
    //            "SQL error in UpdateSettingsAsync for BusinessAccountId={Id}", businessAccountId);
    //        throw;
    //    }
    //}

    public async Task<SettingsRecord?> UpdateSettingsAsync(
    int businessAccountId, int requestingUserId, UpdateSettingsRequest req)
    {
        using var db = new SqlConnection(_conn);

        byte[] rowVersionBytes;
        try { rowVersionBytes = Convert.FromBase64String(req.RowVersion); }
        catch { throw new ArgumentException("Invalid row version format.", nameof(req.RowVersion)); }

        var p = new DynamicParameters();

        // ── Existing parameters ────────────────────────────────────────────────────
        p.Add("@BusinessAccountId", businessAccountId);
        p.Add("@RequestingUserId", requestingUserId);
        p.Add("@BusinessName", req.BusinessName);
        p.Add("@OwnerName", req.OwnerName);
        p.Add("@BusinessPhone", req.BusinessPhone);
        p.Add("@BusinessEmail", req.BusinessEmail);
        p.Add("@Address", req.Address);
        p.Add("@GSTIN", req.GSTIN);
        p.Add("@ShopType", req.ShopType);
        p.Add("@ThemeColor", req.ThemeColor);
        p.Add("@Currency", req.Currency);
        p.Add("@CurrencySymbol", ResolveCurrencySymbol(req.Currency));
        p.Add("@RowVersion", rowVersionBytes, DbType.Binary, size: 8);

        // ── NEW parameters ─────────────────────────────────────────────────────────
        p.Add("@GstEnabled", req.GstEnabled);
        p.Add("@GstType", req.GstType);
        p.Add("@CgstRate", req.CgstRate);
        p.Add("@SgstRate", req.SgstRate);
        p.Add("@IgstRate", req.IgstRate);
        p.Add("@ShowGstOnInvoice", req.ShowGstOnInvoice);
        p.Add("@LogoUrl", req.LogoUrl);
        p.Add("@ShowLogoOnInvoice", req.ShowLogoOnInvoice);
        p.Add("@InvoiceShowTime", req.InvoiceShowTime);

        // ── Output parameters ──────────────────────────────────────────────────────
        p.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
        p.Add("@ErrorMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

        try
        {
            var record = await db.QuerySingleOrDefaultAsync<SettingsRecord>(
                "dbo.usp_Settings_Update", p,
                commandType: CommandType.StoredProcedure);

            int errorCode = p.Get<int>("@ErrorCode");
            string errorMessage = p.Get<string>("@ErrorMessage") ?? string.Empty;

            if (errorCode != 0)
            {
                _logger.LogWarning(
                    "usp_Settings_Update returned error {Code}: {Msg} for BusinessAccountId={Id}",
                    errorCode, errorMessage, businessAccountId);

                throw new AppException(
                    errorMessage,
                    errorCode == 2009 ? "CONCURRENCY_CONFLICT" : $"SETTINGS_{errorCode}",
                    errorCode == 2009 ? 409 : 400);
            }

            return record;
        }
        catch (AppException) { throw; }
        catch (SqlException ex)
        {
            _logger.LogError(ex,
                "SQL error in UpdateSettingsAsync for BusinessAccountId={Id}", businessAccountId);
            throw;
        }
    }

    // ── Get Role Permissions ──────────────────────────────────

    public async Task<List<RolePermissionRecord>> GetRolePermissionsAsync(
        int businessAccountId, string? role = null)
    {
        using var db = new SqlConnection(_conn);

        try
        {
            var rows = await db.QueryAsync<RolePermissionRecord>(
                "dbo.usp_RolePermissions_Get",
                new { BusinessAccountId = businessAccountId, Role = role },
                commandType: CommandType.StoredProcedure);

            return rows.AsList();
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex,
                "SQL error in GetRolePermissionsAsync for BusinessAccountId={Id} Role={Role}",
                businessAccountId, role);
            throw;
        }
    }

    // ── Save Role Permissions ─────────────────────────────────

    public async Task<List<RolePermissionRecord>> SaveRolePermissionsAsync(
        int businessAccountId, int requestingUserId,
        string role, List<PermissionEntry> permissions)
    {
        using var db = new SqlConnection(_conn);

        // Serialize permission list to JSON for the SP parameter
        var json = JsonSerializer.Serialize(permissions.Select(p => new
        {
            module    = p.Module,
            canView   = p.CanView   ? 1 : 0,
            canCreate = p.CanCreate ? 1 : 0,
            canEdit   = p.CanEdit   ? 1 : 0,
            canDelete = p.CanDelete ? 1 : 0,
        }));

        var dp = new DynamicParameters();
        dp.Add("@BusinessAccountId", businessAccountId);
        dp.Add("@RequestingUserId",  requestingUserId);
        dp.Add("@Role",              role);
        dp.Add("@PermissionsJson",   json);
        dp.Add("@ErrorCode",    dbType: DbType.Int32,  direction: ParameterDirection.Output);
        dp.Add("@ErrorMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

        try
        {
            var rows = await db.QueryAsync<RolePermissionRecord>(
                "dbo.usp_RolePermissions_Save", dp,
                commandType: CommandType.StoredProcedure);

            int    errorCode    = dp.Get<int>("@ErrorCode");
            string errorMessage = dp.Get<string>("@ErrorMessage") ?? string.Empty;

            if (errorCode != 0)
            {
                _logger.LogWarning(
                    "usp_RolePermissions_Save error {Code}: {Msg} Role={Role} Biz={Id}",
                    errorCode, errorMessage, role, businessAccountId);

                throw new AppException(errorMessage, $"PERMS_{errorCode}");
            }

            return rows.AsList();
        }
        catch (AppException) { throw; }
        catch (SqlException ex)
        {
            _logger.LogError(ex,
                "SQL error in SaveRolePermissionsAsync Role={Role} Biz={Id}", role, businessAccountId);
            throw;
        }
    }

    public async Task<List<RolePermissionRecord>> GetMyRolePermissionsAsync(
    int businessAccountId, string role)
    {
        using var db = new SqlConnection(_conn);
        try
        {
            var rows = await db.QueryAsync<RolePermissionRecord>(
                "dbo.usp_RolePermissions_GetMyRole",
                new { BusinessAccountId = businessAccountId, Role = role },
                commandType: CommandType.StoredProcedure);
            return rows.AsList();
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex,
                "SQL error in GetMyRolePermissionsAsync BusinessAccountId={Id} Role={Role}",
                businessAccountId, role);
            throw;
        }
    }

    public async Task<PublicSettingsRecord?> GetPublicSettingsAsync(int businessAccountId)
    {
        using var db = new SqlConnection(_conn);
        try
        {
            return await db.QuerySingleOrDefaultAsync<PublicSettingsRecord>(
                "dbo.usp_Settings_GetPublic",
                new { BusinessAccountId = businessAccountId },
                commandType: CommandType.StoredProcedure);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex,
                "SQL error in GetPublicSettingsAsync BusinessAccountId={Id}", businessAccountId);
            throw;
        }
    }

    // ── Helpers ───────────────────────────────────────────────

    private static string ResolveCurrencySymbol(string currency) => currency switch
    {
        "INR" => "₹",
        "USD" => "$",
        "EUR" => "€",
        "GBP" => "£",
        _     => "₹"
    };
}
