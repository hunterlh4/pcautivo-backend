using PCautivoCore.Application.Features.UserProperties.Dtos;
using PCautivoCore.Domain.Interfaces;
using PCautivoCore.Domain.Models;
using Dapper;

namespace PCautivoCore.Infrastructure.Persistence.Repositories;

public class UserPropertyRepository(MssqlContext context) : IUserPropertyRepository
{
    public async Task<bool> AssignUserToProperty(int userId, int propertyId)
    {
        var db = context.CreateDefaultConnection();

        string sql = @"
        INSERT INTO UserProperties (UserId, PropertyId)
        VALUES (@UserId, @PropertyId)
        ";

        var result = await db.ExecuteAsync(sql, new
        {
            UserId = userId,
            PropertyId = propertyId,
            //CreatedAt = DateTimeOffset.Now
        });

        return result > 0;
    }

    public async Task<bool> AssignUserToProperties(int userId, List<int> propertyIds)
    {
        if (propertyIds == null || !propertyIds.Any())
            return false;

        var db = context.CreateDefaultConnection();

        string sql = @"
        INSERT INTO UserProperties (UserId, PropertyId)
        VALUES (@UserId, @PropertyId)
        ";

        var now = DateTimeOffset.Now;
        var parameters = propertyIds.Select(propertyId => new
        {
            UserId = userId,
            PropertyId = propertyId,
        });

        var result = await db.ExecuteAsync(sql, parameters);

        return result > 0;
    }

    public async Task<bool> RemoveUserFromProperty(int userId, int propertyId)
    {
        var db = context.CreateDefaultConnection();

        string sql = @"
        DELETE FROM UserProperties
        WHERE UserId = @UserId AND PropertyId = @PropertyId
        ";

        var result = await db.ExecuteAsync(sql, new
        {
            UserId = userId,
            PropertyId = propertyId
        });

        return result > 0;
    }

    public async Task<bool> RemoveUserFromProperties(int userId, List<int> propertyIds)
    {
        if (propertyIds == null || !propertyIds.Any())
            return false;

        var db = context.CreateDefaultConnection();

        string sql = @"
        DELETE FROM UserProperties
        WHERE UserId = @UserId AND PropertyId IN @PropertyIds
        ";

        var result = await db.ExecuteAsync(sql, new
        {
            UserId = userId,
            PropertyIds = propertyIds
        });

        return result > 0;
    }

    public async Task<UserProperty?> GetUserProperty(int userId, int propertyId)
    {
        var db = context.CreateDefaultConnection();

        string sql = @"
        SELECT UserId, PropertyId
        FROM UserProperties
        WHERE UserId = @UserId AND PropertyId = @PropertyId
        ";

        var result = await db.QueryFirstOrDefaultAsync<UserProperty>(sql, new
        {
            UserId = userId,
            PropertyId = propertyId
        });

        return result;
    }

    public async Task<IEnumerable<Property>> GetUserProperties(int userId)
    {
        var db = context.CreateDefaultConnection();

        string sql = @"
        SELECT 
            p.Id,
            p.ExternalId,
            p.TypeId,
            p.Name,
            p.Country,
            p.City,
            p.Street,
            p.PersonCapacity,
            p.BedroomsNumber,
            p.BedsNumber,
            p.BathroomsNumber,
            p.GuestBathroomsNumber,
            p.AverageReviewRating,
            p.CreatedAt,
            p.UpdatedAt,
            tpe.Id,
            tpe.Name,
            tpe.CreatedAt,
            tpe.UpdatedAt
        FROM UserProperties up
        INNER JOIN Properties p ON up.PropertyId = p.Id
        LEFT JOIN PropertyTypes tpe ON tpe.Id = p.TypeId
        WHERE up.UserId = @UserId
        ORDER BY p.Name
        ";

        var result = await db.QueryAsync<Property, PropertyType, Property>(sql, (property, type) =>
        {
            property.Type = type;
            return property;
        },
        new { UserId = userId },
        splitOn: "Id");

        return result;
    }

    public async Task<IEnumerable<UserPropertyDto>> GetUserPropertiesWithDetails(int userId)
    {
        var db = context.CreateDefaultConnection();

        string sql = @"
        SELECT 
            p.Id AS PropertyId,
            p.ExternalId,
            p.Name,
            p.Country,
            p.City
        FROM UserProperties up
        INNER JOIN Properties p ON up.PropertyId = p.Id
        WHERE up.UserId = @UserId
        ORDER BY p.Name
        ";

        var result = await db.QueryAsync<UserPropertyDto>(sql, new { UserId = userId });

        return result;
    }

    public async Task<Dictionary<int, int>> GetPropertiesCountByUsers() {
        var db = context.CreateDefaultConnection();

        string sql = @"
        SELECT 
            UserId,
            COUNT(*) AS PropertiesCount
        FROM UserProperties
        GROUP BY UserId
        ";

        var result = await db.QueryAsync<(int UserId, int PropertiesCount)>(sql);

        return result.ToDictionary(x => x.UserId, x => x.PropertiesCount);
    }
}

  

