using PCautivoCore.Domain.Interfaces;
using PCautivoCore.Domain.Models;
using Dapper;

namespace PCautivoCore.Infrastructure.Persistence.Repositories;

public class RoleRepository(MssqlContext context) : IRoleRepository
{
    public async Task<int> CreateRole(Role item)
    {
        var db = context.CreateDefaultConnection();

        string sql = @"
        INSERT INTO Roles
        (
            Name,
            CreatedAt
        )
        VALUES
        (
            @Name,
            @CreatedAt
        )

        SELECT SCOPE_IDENTITY()
        ";

        var result = await db.QuerySingleAsync<int>(sql, item);

        return result;
    }

    public async Task<bool> UpdateRole(Role item)
    {
        var db = context.CreateDefaultConnection();

        string sql = @"
        UPDATE Roles
        SET
            Name = @Name,
            UpdatedAt = @UpdatedAt
        WHERE
            Id = @Id
        ";

        var result = await db.ExecuteAsync(sql, item);

        return result > 0;
    }

    public async Task<IEnumerable<Role>> GetAllRoles()
    {
        var db = context.CreateDefaultConnection();

        string sql = @"
        SELECT
            Id,
            Name,
            CreatedAt,
            UpdatedAt
        FROM Roles";

        var result = await db.QueryAsync<Role>(sql);

        return result;
    }

    public async Task<Role?> GetRoleById(int itemId)
    {
        var db = context.CreateDefaultConnection();

        string sql = @"
        SELECT
            Id,
            Name,
            CreatedAt,
            UpdatedAt
        FROM Roles
        WHERE
            Id = @Id
        ";

        var result = await db.QueryFirstOrDefaultAsync<Role>(sql, new
        {
            Id = itemId
        });

        return result;
    }

    public async Task<IEnumerable<Role>> GetRolesByUserId(int userId)
    {
        var db = context.CreateDefaultConnection();

        string sql = @"
        SELECT
            rle.Id,
            rle.Name,
            rle.CreatedAt,
            rle.UpdatedAt
        FROM Roles rle
        INNER JOIN UserRoles usrle ON usrle.UserId = rle.Id
        WHERE
	        usrle.UserId = @UserId
        ";

        var result = await db.QueryAsync<Role>(sql, new
        {
            UserId = userId
        });

        return result;
    }
}