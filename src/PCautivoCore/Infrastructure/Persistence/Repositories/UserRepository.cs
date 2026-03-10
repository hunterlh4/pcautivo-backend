using PCautivoCore.Domain.Interfaces;
using PCautivoCore.Domain.Models;
using Dapper;

namespace PCautivoCore.Infrastructure.Persistence.Repositories;

public class UserRepository(MssqlContext context) : IUserRepository
{
    public async Task<int> CreateUser(User item)
    {
        var db = context.CreateDefaultConnection();

        var sql = @"
        INSERT INTO Users
        (
            Username,
            PasswordHash,
            CreatedAt
        )
        VALUES
        (
            @Username,
            @PasswordHash,
            @CreatedAt
        )

        SELECT SCOPE_IDENTITY()
        ";

        var result = await db.QuerySingleAsync<int>(sql, item);

        return result;
    }

    public async Task<bool> UpdateUser(User item)
    {
        var db = context.CreateDefaultConnection();

        string sql = @"
        UPDATE Users
        SET
            UpdatedAt = @UpdatedAt
        WHERE
            Id = @Id
        ";

        var result = await db.ExecuteAsync(sql, item);

        return result > 0;
    }
    
    public async Task<User?> GetUserByUsername(string username)
    {
        var db = context.CreateDefaultConnection();

        var sql = @"
        SELECT
            Id,
            Username,
            PasswordHash,
            SuperUser,
            CreatedAt,
            UpdatedAt
        FROM Users
        WHERE
            Username = @Username
        ";

        var result = await db.QueryFirstOrDefaultAsync<User>(sql, new
        {
            Username = username
        });

        return result;
    }

    public async Task<User?> GetUserById(int userId)
    {
        var db = context.CreateDefaultConnection();

        var sql = @"
        SELECT
            Id,
            Username,
            PasswordHash,
            SuperUser,
            CreatedAt,
            UpdatedAt
        FROM Users
        WHERE
            Id = @Id
        ";

        var result = await db.QueryFirstOrDefaultAsync<User>(sql, new
        {
            Id = userId
        });

        return result;
    }

    public async Task<IEnumerable<User>> GetAllUsers()
    {
        var db = context.CreateDefaultConnection();

        var sql = @"
        SELECT
            Id,
            Username,
            SuperUser,
            CreatedAt,
            UpdatedAt
        FROM Users";

        var result = await db.QueryAsync<User>(sql);

        return result;
    }

    public async Task<bool> CreateUserRole(UserRole item)
    {
        var db = context.CreateDefaultConnection();

        string sql = @"
        INSERT INTO UserRoles
        (
            UserId,
            RoleId,
            CreatedAt
        )
        VALUES
        (
            @UserId,
            @RoleId,
            @CreatedAt
        )
        ";

        var result = await db.ExecuteAsync(sql, item);

        return result > 0;
    }

    public async Task<bool> DeleteUserRole(int userId, int roleId)
    {
        var db = context.CreateDefaultConnection();

        string sql = "DELETE FROM UserRoles WHERE UserId = @UserId AND RoleId = @RoleId";

        var result = await db.ExecuteAsync(sql, new
        {
            UserId = userId,
            RoleId = roleId
        });

        return result > 0;
    }

    public async Task<UserRole?> GetUserRoleByIds(int userId, int roleId)
    {
        var db = context.CreateDefaultConnection();

        string sql = @"
        SELECT
            UserId,
            RoleId,
            CreatedAt
        FROM UserRoles
        WHERE
            UserId = @UserId
            AND RoleId = @RoleId";

        var result = await db.QueryFirstOrDefaultAsync<UserRole>(sql, new
        {
            UserId = userId,
            RoleId = roleId
        });

        return result;
    }

    public async Task<IEnumerable<UserRole>> GetAllUserRoles()
    {
        var db = context.CreateDefaultConnection();

        var sql = @"
        SELECT
            UserId,
            RoleId,
            CreatedAt
        FROM UserRoles";

        var result = await db.QueryAsync<UserRole>(sql);

        return result;
    }

    public async Task<IEnumerable<User>> GetUsersByRoleId(int roleId)
    {
        var db = context.CreateDefaultConnection();

        var sql = @"
        SELECT
            usr.Id,
            usr.Username,
            usr.SuperUser,
            usr.CreatedAt,
            usr.UpdatedAt
        FROM Users usr
        INNER JOIN UserRoles usrle ON usrle.UserId = usr.Id
        WHERE
	        usrle.RoleId = @RoleId
        ";

        var result = await db.QueryAsync<User>(sql, new
        {
            RoleId = roleId
        });

        return result;
    }

    public async Task<bool> UpdatePassword(User item)
    {
        var db = context.CreateDefaultConnection();

        string sql = @"
        UPDATE Users
        SET
            PasswordHash = @PasswordHash,
            UpdatedAt = @UpdatedAt
        WHERE
            Id = @Id
        ";

        var result = await db.ExecuteAsync(sql, item);

        return result > 0;
    }

    public async Task<IEnumerable<UserRole>> GetUserRolesByUserId(int userId)
    {
        var db = context.CreateDefaultConnection();

        var sql = @"
        SELECT
            UserId,
            RoleId,
            CreatedAt
        FROM UserRoles
        WHERE
            UserId = @UserId
        ";

        var result = await db.QueryAsync<UserRole>(sql, new
        {
            UserId = userId
        });

        return result;
    }

    public async Task<bool> CreateUserDetail(UserDetail item)
    {
        var db = context.CreateDefaultConnection();

        string sql = @"
        INSERT INTO UserDetails
        (
            UserId,
            FirstName,
            LastName,
            Email,
            PhoneNumber,
            CreatedAt
        )
        VALUES
        (
            @UserId,
            @FirstName,
            @LastName,
            @Email,
            @PhoneNumber,
            @CreatedAt
        )
        ";

        var result = await db.ExecuteAsync(sql, item);

        return result > 0;
    }

    public async Task<int> CreateUserWithDetail(User user, UserDetail detail)
    {
        using var db = context.CreateDefaultConnection();

        db.Open();

        using var transaction = db.BeginTransaction();

        try
        {
            var sqlUsers = @"
            INSERT INTO Users
            (
                Username,
                PasswordHash,
                UserType,
                CreatedAt
            )
            VALUES
            (
                @Username,
                @PasswordHash,
                @UserType,
                @CreatedAt
            )

            SELECT SCOPE_IDENTITY()
            ";

            var sqlDetail = @"
            INSERT INTO UserDetails
            (
                UserId,
                FirstName,
                LastName,
                Email,
                PhoneNumber,
                CreatedAt
            )
            VALUES
            (
                @UserId,
                @FirstName,
                @LastName,
                @Email,
                @PhoneNumber,
                @CreatedAt
            )
            ";

            var userId = await db.QuerySingleAsync<int>(sqlUsers, user, transaction);

            detail.UserId = userId;

            await db.ExecuteAsync(sqlDetail, detail, transaction);

            transaction.Commit();

            return userId;
        }
        catch (Exception)
        {
            transaction.Rollback();

            throw;
        }
        finally
        {
            transaction.Dispose();
        }
    }

    public async Task<IEnumerable<User>> GetAllUsersWithDetails()
    {
        var db = context.CreateDefaultConnection();

        var sql = @"
        SELECT
            usr.Id,
            usr.Username,
            usr.SuperUser,
            usr.UserType,
            usr.CreatedAt,
            usr.UpdatedAt,
            usrdtl.UserId,
	        usrdtl.FirstName,
	        usrdtl.LastName,
	        usrdtl.Email,
	        usrdtl.PhoneNumber,
	        usrdtl.CountryCode,
	        usrdtl.CreatedAt,
	        usrdtl.UpdatedAt
        FROM Users usr
        LEFT JOIN UserDetails usrdtl ON usrdtl.UserId = usr.Id
        ";

        var result = await db.QueryAsync<User, UserDetail, User>(sql, (user, detail) =>
        {
            user.Detail = detail;

            return user;
        },
        splitOn: "UserId");

        return result;
    }

    public async Task<IEnumerable<User>> GetUsersByType(int userType)
    {
        var db = context.CreateDefaultConnection();

        var sql = @"
        SELECT
            usr.Id,
            usr.Username,
            usr.SuperUser,
            usr.UserType,
            usr.CreatedAt,
            usr.UpdatedAt,
            usrdtl.UserId,
	        usrdtl.FirstName,
	        usrdtl.LastName,
	        usrdtl.Email,
	        usrdtl.PhoneNumber,
	        usrdtl.CountryCode,
	        usrdtl.CreatedAt,
	        usrdtl.UpdatedAt
        FROM Users usr
        LEFT JOIN UserDetails usrdtl ON usrdtl.UserId = usr.Id
        WHERE usr.UserType = @UserType
        ";

        var result = await db.QueryAsync<User, UserDetail, User>(sql, (user, detail) =>
        {
            user.Detail = detail;

            return user;
        },
        new { UserType = userType },
        splitOn: "UserId");

        return result;
    }

    public async Task<UserDetail?> GetUserDetailById(int userId)
    {
        var db = context.CreateDefaultConnection();

        var sql = @"
        SELECT
            UserId,
            FirstName,
            LastName,
            Email,
            PhoneNumber,
            CountryCode,
            CreatedAt,
            UpdatedAt
        FROM UserDetails
        WHERE
            UserId = @UserId
        ";

        var result = await db.QueryFirstOrDefaultAsync<UserDetail>(sql, new
        {
            UserId = userId
        });

        return result;
    }

    public async Task<bool> UpdateUserDetail(UserDetail item)
    {
        var db = context.CreateDefaultConnection();

        string sql = @"
        UPDATE UserDetails
        SET
            FirstName = @FirstName,
            LastName = @LastName,
            Email = @Email,
            PhoneNumber = @PhoneNumber,
            CountryCode = @CountryCode,
            UpdatedAt = @UpdatedAt
        WHERE
            UserId = @UserId
        ";

        var result = await db.ExecuteAsync(sql, item);

        return result > 0;
    }
}