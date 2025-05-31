using BookingSystem.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;
using BookingSystem.Infrastructure.Data;

namespace BookingSystem.Infrastructure.Repositories
{
    public class BaseRepository<T> : IRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;
        private readonly PropertyInfo _isDeletedProperty;

        public BaseRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
            
            // Check if the entity has an IsDeleted property
            _isDeletedProperty = typeof(T).GetProperty("IsDeleted");
        }

        // Apply IsDeleted filter if the property exists
        protected IQueryable<T> ApplySoftDeleteFilter(IQueryable<T> query)
        {
            if (_isDeletedProperty != null)
            {
                // Create expression: entity => (bool)entity.IsDeleted == false
                var parameter = Expression.Parameter(typeof(T), "entity");
                var property = Expression.Property(parameter, _isDeletedProperty);
                var falseValue = Expression.Constant(false);
                var condition = Expression.Equal(property, falseValue);
                var lambda = Expression.Lambda<Func<T, bool>>(condition, parameter);

                query = query.Where(lambda);
            }
            
            return query;
        }

        public async Task<T> GetByIdAsync(int id)
        {
            var entity = await _dbSet.FindAsync(id);
            
            // Check if entity is soft-deleted
            if (entity != null && _isDeletedProperty != null)
            {
                var isDeleted = (bool)_isDeletedProperty.GetValue(entity);
                if (isDeleted)
                {
                    return null; // Return null for soft-deleted entities
                }
            }
            
            return entity;
        }

        public async Task<T> GetByIdAsync(int id, Func<IQueryable<T>, IQueryable<T>> include)
        {
            var query = _dbSet.AsQueryable();
            query = ApplySoftDeleteFilter(query);
            
            if (include != null)
            {
                query = include(query);
            }
            
            // Create an expression to filter by ID
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, "Id");
            var constant = Expression.Constant(id);
            var equal = Expression.Equal(property, constant);
            var lambda = Expression.Lambda<Func<T, bool>>(equal, parameter);
            
            return await query.FirstOrDefaultAsync(lambda);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            var query = _dbSet.AsQueryable();
            query = ApplySoftDeleteFilter(query);
            return await query.ToListAsync();
        }

        public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> predicate)
        {
            var query = _dbSet.Where(predicate);
            query = ApplySoftDeleteFilter(query);
            return await query.ToListAsync();
        }

        public async Task<IEnumerable<T>> GetAllAsync(
            Expression<Func<T, bool>> predicate = null, 
            Func<IQueryable<T>, IQueryable<T>> include = null)
        {
            IQueryable<T> query = _dbSet;
            query = ApplySoftDeleteFilter(query);
            
            if (predicate != null)
            {
                query = query.Where(predicate);
            }
            
            if (include != null)
            {
                query = include(query);
            }
            
            return await query.ToListAsync();
        }

        public async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            var query = _dbSet.AsQueryable();
            query = ApplySoftDeleteFilter(query);
            return await query.FirstOrDefaultAsync(predicate);
        }

        public async Task<T> FirstOrDefaultAsync(
            Expression<Func<T, bool>> predicate, 
            Func<IQueryable<T>, IQueryable<T>> include)
        {
            IQueryable<T> query = _dbSet;
            query = ApplySoftDeleteFilter(query);
            
            if (include != null)
            {
                query = include(query);
            }
            
            return await query.FirstOrDefaultAsync(predicate);
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>> predicate = null)
        {
            var query = _dbSet.AsQueryable();
            query = ApplySoftDeleteFilter(query);
            
            if (predicate == null)
            {
                return await query.CountAsync();
            }
            
            return await query.CountAsync(predicate);
        }

        public async Task<int> CountAsync(
            Expression<Func<T, bool>> predicate, 
            Func<IQueryable<T>, IQueryable<T>> include)
        {
            IQueryable<T> query = _dbSet;
            query = ApplySoftDeleteFilter(query);
            
            if (include != null)
            {
                query = include(query);
            }
            
            return await query.CountAsync(predicate);
        }

        public IQueryable<T> GetQueryable()
        {
            var query = _dbSet.AsQueryable();
            return ApplySoftDeleteFilter(query);
        }

        public async Task AddAsync(T entity)
        {
            // Set IsDeleted to false for new entities if the property exists
            if (_isDeletedProperty != null)
            {
                _isDeletedProperty.SetValue(entity, false);
            }
            
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                // If the entity has IsDeleted property, perform soft delete
                if (_isDeletedProperty != null)
                {
                    _isDeletedProperty.SetValue(entity, true);
                    
                    // If the entity has UpdatedAt property, update it
                    var updatedAtProperty = typeof(T).GetProperty("UpdatedAt");
                    if (updatedAtProperty != null && updatedAtProperty.PropertyType == typeof(DateTime?))
                    {
                        updatedAtProperty.SetValue(entity, DateTime.UtcNow);
                    }
                    
                    _dbSet.Update(entity);
                }
                else
                {
                    // If no IsDeleted property, perform hard delete
                    _dbSet.Remove(entity);
                }
                
                await _context.SaveChangesAsync();
            }
        }
        
        // Add a method for permanent deletion if needed
        public async Task DeletePermanentlyAsync(int id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}