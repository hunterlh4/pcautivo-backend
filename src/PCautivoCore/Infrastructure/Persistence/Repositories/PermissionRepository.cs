using PCautivoCore.Domain.Interfaces;
using PCautivoCore.Domain.Models;
using Dapper;

namespace PCautivoCore.Infrastructure.Persistence.Repositories;

public class PermissionRepository(MssqlContext context) : IPermissionRepository
{
    public async Task<bool> CreateManyPermissions(IEnumerable<Permission> items)
    {
        var db = context.CreateDefaultConnection();

        string sql = @"
        INSERT INTO Permissions
        (
            Name,
            Controller,
            ActionName,
            HttpMethod,
            ActionType,
            CreatedAt
        )
        VALUES
        (
            @Name,
            @Controller,
            @ActionName,
            @HttpMethod,
            @ActionType,
            @CreatedAt
        )
        ";

        var affectedRows = await db.ExecuteAsync(sql, items);

        return affectedRows > 0;
    }

    public async Task<bool> UpdateManyPermissions(IEnumerable<Permission> items)
    {
        var db = context.CreateDefaultConnection();

        string sql = @"
        UPDATE Permissions
        SET
            UpdatedAt = @UpdatedAt
        WHERE
            Id = @Id
        ";

        var affectedRows = await db.ExecuteAsync(sql, items);

        return affectedRows > 0;
    }

    public async Task<bool> DeletePermissionById(int itemId)
    {
        using var db = context.CreateDefaultConnection();

        db.Open();

        using var transaction = db.BeginTransaction();

        try
        {
            var param = new { Id = itemId };

            await db.ExecuteAsync("DELETE FROM RolePermissions WHERE PermissionId = @Id", param, transaction);

            var affectedRows = await db.ExecuteAsync("DELETE FROM Permissions WHERE Id = @Id", param, transaction);

            transaction.Commit();

            return affectedRows > 0;
        }
        catch
        {
            transaction.Rollback();

            return false;
        }
    }

    public async Task<Permission?> GetPermissionById(int itemId)
    {
        var db = context.CreateDefaultConnection();

        string sql = @"
        SELECT
            Id,
            Name,
            Controller,
            ActionName,
            HttpMethod,
            ActionType,
            CreatedAt,
            UpdatedAt
        FROM Permissions
        WHERE
            Id = @Id";

        var result = await db.QueryFirstOrDefaultAsync<Permission>(sql, new
        {
            Id = itemId
        });

        return result;
    }

    public async Task<IEnumerable<Permission>> GetAllPermissions()
    {
        var db = context.CreateDefaultConnection();

        string sql = @"
        SELECT
            Id,
            Name,
            Controller,
            ActionName,
            HttpMethod,
            ActionType,
            CreatedAt,
            UpdatedAt
        FROM Permissions";

        var result = await db.QueryAsync<Permission>(sql);

        return result;
    }

    public async Task<IEnumerable<Permission>> GetPermissionsByUserId(int userId)
    {
        var db = context.CreateDefaultConnection();

        string sql = @"
        SELECT
	        prm.Id,
	        prm.Name,
	        prm.Controller,
	        prm.ActionName,
	        prm.HttpMethod,
	        prm.ActionType,
	        prm.CreatedAt,
	        prm.UpdatedAt
        FROM Permissions prm
        INNER JOIN RolePermissions rleprm ON rleprm.PermissionId = prm.Id
        INNER JOIN UserRoles usrle ON usrle.RoleId = rleprm.RoleId
        WHERE
	        usrle.UserId = @UserId
        ";

        var result = await db.QueryAsync<Permission>(sql, new
        {
            UserId = userId
        });

        return result;
    }

    public async Task<bool> CreateRolePermission(RolePermission item)
    {
        var db = context.CreateDefaultConnection();

        string sql = @"
        INSERT INTO RolePermissions
        (
            RoleId,
            PermissionId,
            CreatedAt
        )
        VALUES
        (
            @RoleId,
            @PermissionId,
            @CreatedAt
        )
        ";

        var result = await db.ExecuteAsync(sql, item);

        return result > 0;
    }

    public async Task<bool> DeleteRolePermission(int roleId, int permissionId)
    {
        var db = context.CreateDefaultConnection();

        string sql = "DELETE FROM RolePermissions WHERE RoleId = @RoleId AND PermissionId = @PermissionId";

        var result = await db.ExecuteAsync(sql, new
        {
            RoleId = roleId,
            PermissionId = permissionId
        });

        return result > 0;
    }

    public async Task<bool> DeleteRolePermissionByRole(int roleId)
    {
        var db = context.CreateDefaultConnection();

        string sql = "DELETE FROM RolePermissions WHERE RoleId = @RoleId";

        var result = await db.ExecuteAsync(sql, new
        {
            RoleId = roleId,
        });

        return result > 0;
    }

    public async Task<bool> CreateManyRolePermission(IEnumerable<RolePermission> items)
    {
        var db = context.CreateDefaultConnection();

        string sql = @"
        INSERT INTO RolePermissions
        (
            RoleId,
            PermissionId,
            CreatedAt
        )
        VALUES
        (
            @RoleId,
            @PermissionId,
            @CreatedAt
        )
        ";

        var affectedRows = await db.ExecuteAsync(sql, items);

        return affectedRows > 0;
    }

    public async Task<bool> DeleteManyRolePermission(IEnumerable<RolePermission> items)
    {
        var db = context.CreateDefaultConnection();

        string sql = "DELETE FROM RolePermissions WHERE RoleId = @RoleId AND PermissionId = @PermissionId";

        var affectedRows = await db.ExecuteAsync(sql, items);

        return affectedRows > 0;
    }

    public async Task<RolePermission?> GetRolePermissionByIds(int roleId, int permissionId)
    {
        var db = context.CreateDefaultConnection();

        string sql = @"
        SELECT
            RoleId,
            PermissionId,
            CreatedAt
        FROM RolePermissions
        WHERE
            RoleId = @RoleId
            AND PermissionId = @PermissionId";

        var result = await db.QueryFirstOrDefaultAsync<RolePermission>(sql, new
        {
            RoleId = roleId,
            PermissionId = permissionId
        });

        return result;
    }

    public async Task<IEnumerable<RolePermission>> GetPermissionsByRoleId(int roleId)
    {
        var db = context.CreateDefaultConnection();

        string sql = @"
        SELECT
            RoleId,
	        PermissionId,
	        CreatedAt
        FROM RolePermissions
        WHERE
            RoleId = @RoleId
        ";

        var result = await db.QueryAsync<RolePermission>(sql, new
        {
            RoleId = roleId
        });

        return result;
    }
}