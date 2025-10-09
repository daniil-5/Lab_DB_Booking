
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using BookingSystem.Domain.Interfaces;
using BookingSystem.Infrastructure.Data;
using Dapper;

namespace BookingSystem.Infrastructure.Repositories;

public abstract class BaseRepository<T> : IRepository<T> where T : class
{
    protected readonly DapperDbContext _context;
    private readonly string _tableName;
    private readonly PropertyInfo _isDeletedProperty;

    protected BaseRepository(DapperDbContext context)
    {
        _context = context;
        _tableName = typeof(T).Name + "s";
        _isDeletedProperty = typeof(T).GetProperty("IsDeleted");
    }

    public async Task<T> GetByIdAsync(int id)
    {
        using var connection = _context.CreateConnection();
        var sql = $"SELECT * FROM \"{_tableName}\" WHERE \"Id\" = @Id";
        if (_isDeletedProperty != null)
        {
            sql += " AND \"IsDeleted\" = false";
        }
        return await connection.QuerySingleOrDefaultAsync<T>(sql, new { Id = id });
    }

    public Task<T> GetByIdAsync(int id, Func<IQueryable<T>, IQueryable<T>> include)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        using var connection = _context.CreateConnection();
        var sql = $"SELECT * FROM \"{_tableName}\" ";
        if (_isDeletedProperty != null)
        {
            sql += " WHERE \"IsDeleted\" = false";
        }
        return await connection.QueryAsync<T>(sql);
    }

    public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> predicate)
    {
        // This is a simplified implementation. A more robust solution would require a predicate visitor to convert the expression tree to a SQL WHERE clause.
        // For now, we'll just get all entities and filter in memory.
        var all = await GetAllAsync();
        return all.Where(predicate.Compile());
    }

    public Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IQueryable<T>> include = null)
    {
        throw new NotImplementedException();
    }

    public async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        var all = await GetAllAsync();
        return all.FirstOrDefault(predicate.Compile());
    }

    public Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IQueryable<T>> include)
    {
        throw new NotImplementedException();
    }

    public async Task<int> CountAsync(Expression<Func<T, bool>> predicate = null)
    {
        using var connection = _context.CreateConnection();
        var sql = $"SELECT COUNT(*) FROM \"{_tableName}\" ";
        if (_isDeletedProperty != null)
        {
            sql += " WHERE \"IsDeleted\" = false";
        }
        return await connection.ExecuteScalarAsync<int>(sql);
    }

    public Task<int> CountAsync(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IQueryable<T>> include)
    {
        throw new NotImplementedException();
    }

    public IQueryable<T> GetQueryable()
    {
        throw new NotImplementedException();
    }

    public async Task AddAsync(T entity)
    {
        if (_isDeletedProperty != null)
        {
            _isDeletedProperty.SetValue(entity, false);
        }
        using var connection = _context.CreateConnection();
        var columns = GetColumns(entity, excludeId: true);
        var values = GetValues(entity, excludeId: true);
        var sql = $"INSERT INTO \"{_tableName}\" ({columns}) VALUES ({values}) RETURNING \"Id\"";
        var id = await connection.ExecuteScalarAsync<int>(sql, entity);
        var idProperty = typeof(T).GetProperty("Id");
        if (idProperty != null)
        {
            idProperty.SetValue(entity, id);
        }
    }

    public async Task UpdateAsync(T entity)
    {
        using var connection = _context.CreateConnection();
        var setClause = GetSetClause(entity);
        var idProperty = typeof(T).GetProperty("Id");
        var id = idProperty.GetValue(entity);
        var sql = $"UPDATE \"{_tableName}\" SET {setClause} WHERE \"Id\" = @Id";
        await connection.ExecuteAsync(sql, entity);
    }

    public async Task DeleteAsync(int id)
    {
        using var connection = _context.CreateConnection();
        if (_isDeletedProperty != null)
        {
            var updatedAtProperty = typeof(T).GetProperty("UpdatedAt");
            var sql = $"UPDATE \"{_tableName}\" SET \"IsDeleted\" = true";
            if (updatedAtProperty != null)
            {
                sql += ", \"UpdatedAt\" = @UpdatedAt";
            }
            sql += " WHERE \"Id\" = @Id";
            await connection.ExecuteAsync(sql, new { Id = id, UpdatedAt = DateTime.UtcNow });
        }
        else
        {
            var sql = $"DELETE FROM \"{_tableName}\" WHERE \"Id\" = @Id";
            await connection.ExecuteAsync(sql, new { Id = id });
        }
    }

    public async Task DeletePermanentlyAsync(int id)
    {
        using var connection = _context.CreateConnection();
        var sql = $"DELETE FROM \"{_tableName}\" WHERE \"Id\" = @Id";
        await connection.ExecuteAsync(sql, new { Id = id });
    }

    private string GetColumns(T entity, bool excludeId = false)
    {
        var type = typeof(T);
        var properties = type.GetProperties().Where(p => p.Name != "Id" || !excludeId);
        return string.Join(", ", properties.Select(p => $"\"{p.Name}\" "));
    }

    private string GetValues(T entity, bool excludeId = false)
    {
        var type = typeof(T);
        var properties = type.GetProperties().Where(p => p.Name != "Id" || !excludeId);
        return string.Join(", ", properties.Select(p => $"@{p.Name}"));
    }

    private string GetSetClause(T entity)
    {
        var type = typeof(T);
        var properties = type.GetProperties().Where(p => p.Name != "Id");
        return string.Join(", ", properties.Select(p => $"\"{p.Name}\" = @{p.Name}"));
    }
}
