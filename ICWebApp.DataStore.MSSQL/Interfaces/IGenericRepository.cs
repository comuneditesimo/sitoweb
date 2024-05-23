using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.DataStore.MSSQL.Interfaces
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        public Guid? Auth_Users_ID { get; set; }
        public Guid? Auth_Municipality_ID { get; set; }
        public IQueryable<TEntity> Where(Expression<Func<TEntity, bool>>? filter = null);
        public Task<List<TEntity>> ToListAsync(Expression<Func<TEntity, bool>>? filter = null);
        public List<TEntity> ToList(Expression<Func<TEntity, bool>>? filter = null);
        public Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>>? filter = null);
        public TEntity? FirstOrDefault(Expression<Func<TEntity, bool>>? filter = null);
        public Task<TEntity> FirstAsync(Expression<Func<TEntity, bool>>? filter = null);
        public TEntity First(Expression<Func<TEntity, bool>>? filter = null);
        public Task<TEntity?> GetByIDAsync(object ID);
        public TEntity? GetByID(object ID);
        public Task<TEntity?> InsertAsync(TEntity Entity);
        public TEntity? Insert(TEntity Entity);
        public Task<TEntity?> InsertOrUpdateAsync(TEntity Entity);
        public TEntity? InsertOrUpdate(TEntity Entity);
        public Task<TEntity?> UpdateAsync(TEntity EntityToUpdate);
        public TEntity? Update(TEntity EntityToUpdate);
        public Task<bool> DeleteAsync(TEntity EntityToDelete);
        public Task<bool> DeleteAsync(object ID);
        public bool Delete(object ID);
        public bool Delete(TEntity EntityToDelete);
        public Task<bool> BulkUpdateAsync(IEnumerable<TEntity> Data);
        public bool BulkUpdate(IEnumerable<TEntity> Data);
        public IEnumerable<TEntity> FindWithSpecificationPattern(ISpecification<TEntity>? Specification = null);
    }
}
