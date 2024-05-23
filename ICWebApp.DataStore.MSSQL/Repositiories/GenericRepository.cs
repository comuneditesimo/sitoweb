using ICWebApp.DataStore.MSSQL.Interfaces;
using ICWebApp.Domain.DBModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ICWebApp.Domain.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Security.AccessControl;
using ICWebApp.DataStore.MSSQL.Contexts;


namespace ICWebApp.DataStore.MSSQL.Repositories
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        private readonly DBContext context;
        public Guid? Auth_Users_ID { get; set; }
        public Guid? Auth_Municipality_ID { get; set; }

        public GenericRepository(DBContext dbcontext)
        {
            context = dbcontext;
        }

        private DbSet<TEntity> GetDbSet()
        {
            return context.CreateConnection().Set<TEntity>();
        }
        public virtual IQueryable<TEntity> Where(Expression<Func<TEntity, bool>>? filter = null)
        {
            IQueryable<TEntity> query = context.CreateConnection().Set<TEntity>();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return query.AsNoTracking().AsSplitQuery();
        }
        public virtual async Task<List<TEntity>> ToListAsync(Expression<Func<TEntity, bool>>? filter = null)
        {
            IQueryable<TEntity> query = context.CreateConnection().Set<TEntity>();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return await query.AsNoTracking().ToListAsync();            
        }
        public virtual List<TEntity> ToList(Expression<Func<TEntity, bool>>? filter = null)
        {
            IQueryable<TEntity> query = context.CreateConnection().Set<TEntity>();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return query.AsNoTracking().ToList();            
        }
        public virtual async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>>? filter = null)
        {
            IQueryable<TEntity> query = context.CreateConnection().Set<TEntity>();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return await query.AsNoTracking().FirstOrDefaultAsync();
        }
        public virtual TEntity? FirstOrDefault(Expression<Func<TEntity, bool>>? filter = null)
        {
            IQueryable<TEntity> query = context.CreateConnection().Set<TEntity>();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return query.AsNoTracking().FirstOrDefault();
        }
        public virtual async Task<TEntity> FirstAsync(Expression<Func<TEntity, bool>>? filter = null)
        {
            IQueryable<TEntity> query = context.CreateConnection().Set<TEntity>();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return await query.AsNoTracking().FirstAsync();
        }
        public virtual TEntity First(Expression<Func<TEntity, bool>>? filter = null)
        {
            IQueryable<TEntity> query = context.CreateConnection().Set<TEntity>();

            if (filter != null)
            {
                query = query.AsNoTracking().Where(filter);
            }

            return query.First();
        }
        public virtual async Task<TEntity?> GetByIDAsync(object ID)
        {
            return await GetDbSet().FindAsync(ID);
        }
        public virtual TEntity? GetByID(object ID)
        {
            return GetDbSet().Find(ID);
        }
        public virtual async Task<TEntity?> InsertAsync(TEntity Entity)
        {
            using (var connection = context.CreateConnection())
            {
                var dbSet = connection.Set<TEntity>();

                Entity = SetKeyDefaultValue(Entity);

                dbSet.Attach(Entity);
                connection.Entry(Entity).State = EntityState.Added;

                await GenerateAuditLogAsync(connection.ChangeTracker.Entries().ToList());

                await connection.SaveChangesAsync();

                return Entity;
            }
        }
        public TEntity? Insert(TEntity Entity)
        {
            using (var connection = context.CreateConnection())
            {
                var dbSet = connection.Set<TEntity>();

                Entity = SetKeyDefaultValue(Entity);

                dbSet.Attach(Entity);
                connection.Entry(Entity).State = EntityState.Added;

                GenerateAuditLog(connection.ChangeTracker.Entries().ToList());

                connection.SaveChanges();

                return Entity;
            }
        }
        public virtual async Task<TEntity?> InsertOrUpdateAsync(TEntity Entity)
        {
            var key = GetKey(Entity);

            var existingItem = await GetByIDAsync(key);

            if (existingItem != null)
            {
                return await this.UpdateAsync(Entity);
            }

            return await this.InsertAsync(Entity);
        }
        public virtual TEntity? InsertOrUpdate(TEntity Entity)
        {
            var key = GetKey(Entity);

            var existingItem = GetByID(key);

            if (existingItem != null)
            {
                return this.Update(Entity);
            }

            return this.Insert(Entity);
        }
        public virtual async Task<TEntity?> UpdateAsync(TEntity EntityToUpdate)
        {
            using (var connection = context.CreateConnection())
            {
                var dbSet = connection.Set<TEntity>();
                
                dbSet.Attach(EntityToUpdate);
                connection.Entry(EntityToUpdate).State = EntityState.Modified;
                
                await GenerateAuditLogAsync(connection.ChangeTracker.Entries().ToList());
                await connection.SaveChangesAsync();

                return EntityToUpdate;
            }
        }
        public virtual TEntity? Update(TEntity EntityToUpdate)
        {
            using (var connection = context.CreateConnection())
            {
                var dbSet = connection.Set<TEntity>();

                dbSet.Attach(EntityToUpdate);
                connection.Entry(EntityToUpdate).State = EntityState.Modified;                

                GenerateAuditLog(connection.ChangeTracker.Entries().ToList());
                connection.SaveChanges();

                return EntityToUpdate;
            }
        }
        public virtual async Task<bool> DeleteAsync(object ID)
        {
            TEntity? entityToDelete = await GetDbSet().FindAsync(ID);

            if (entityToDelete != null)
            {
                return await DeleteAsync(entityToDelete);
            }

            return false;
        }
        public virtual async Task<bool> DeleteAsync(TEntity EntityToDelete)
        {
            using (var connection = context.CreateConnection())
            {
                var dbSet = connection.Set<TEntity>();

                var key = GetKey(EntityToDelete);

                var existingItem = await GetByIDAsync(key);

                if (existingItem != null) 
                { 
                    if (context.CreateConnection().Entry(EntityToDelete).State == EntityState.Detached)
                    {
                        dbSet.Attach(EntityToDelete);
                    }

                    dbSet.Remove(EntityToDelete);

                    await GenerateAuditLogAsync(connection.ChangeTracker.Entries().ToList());

                    await connection.SaveChangesAsync();
                }

                return true;
            }
        }
        public virtual bool Delete(object ID)
        {
            TEntity? entityToDelete = GetDbSet().Find(ID);

            if (entityToDelete != null)
            {
                return Delete(entityToDelete);
            }

            return false;
        }
        public virtual bool Delete(TEntity EntityToDelete)
        {

            using (var connection = context.CreateConnection())
            {
                var dbSet = connection.Set<TEntity>();

                var key = GetKey(EntityToDelete);

                var existingItem = GetByID(key);

                if (existingItem != null)
                {
                    if (context.CreateConnection().Entry(EntityToDelete).State == EntityState.Detached)
                    {
                        dbSet.Attach(EntityToDelete);
                    }

                    dbSet.Remove(EntityToDelete);

                    GenerateAuditLog(connection.ChangeTracker.Entries().ToList());

                    connection.SaveChanges();
                }

                return true;
            }
        }
        public virtual async Task<bool> BulkUpdateAsync(IEnumerable<TEntity?> Data)
        {
            if (Data != null)
            {
                using (var connection = context.CreateConnection())
                {
                    connection.AttachRange(Data);

                    foreach (var entity in Data)
                    {
                        connection.Entry<TEntity>(entity).State = EntityState.Modified;
                    }

                    await GenerateAuditLogAsync(connection.ChangeTracker.Entries().ToList());
                    await connection.SaveChangesAsync();

                    return true;
                }
            }

            return false;
        }
        public virtual bool BulkUpdate(IEnumerable<TEntity?> Data)
        {
            if (Data != null)
            {
                using (var connection = context.CreateConnection())
                {
                    connection.AttachRange(Data);

                    foreach (var entity in Data)
                    {
                        connection.Entry<TEntity>(entity).State = EntityState.Modified;
                    }

                    GenerateAuditLog(connection.ChangeTracker.Entries().ToList());
                    connection.SaveChanges();

                    return true;
                }
            }

            return false;
        }
        public IEnumerable<TEntity> FindWithSpecificationPattern(ISpecification<TEntity>? specification = null)
        {
            using (var connection = context.CreateConnection())
            {
                if (specification != null)
                {
                    return SpecificationEvaluator<TEntity>.GetQuery(connection.Set<TEntity>().AsQueryable(), specification);
                }

                return connection.Set<TEntity>().AsEnumerable();
            }
        }
        private async Task<bool> GenerateAuditLogAsync(List<EntityEntry> ChangedEntries)
        {
            var AuditLogs = new List<SYS_AuditLog>();

            foreach (var entry in ChangedEntries)
            {
                if (entry.Entity is SYS_AuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;

                if (entry.Entity.GetType().Name.ToLower().StartsWith("sys_log"))
                    continue;

                if (entry.Entity.GetType().Name.ToLower().StartsWith("news_"))
                    continue;

                if (entry.Entity.GetType().Name.ToLower().StartsWith("msg_"))
                    continue;

                if (entry.Entity.GetType().Name.ToLower().StartsWith("text_system"))
                    continue;

                if (entry.Entity.GetType().Name.ToLower().StartsWith("conf_"))
                    continue;

                if (entry.Entity.GetType().Name.ToLower().StartsWith("auth_spid_log"))
                    continue;

                if (entry.Entity.GetType().Name.ToLower().StartsWith("auth_external"))
                    continue;

                if (entry.Entity.GetType().Name.ToLower().StartsWith("auth_veriff"))
                    continue;

                if (entry.Entity.GetType().Name.ToLower().StartsWith("pay_pago"))
                    continue;

                if (entry.Entity.GetType().Name.ToLower().StartsWith("d3_"))
                    continue;

                if (entry.Entity.GetType().Name.ToLower().StartsWith("auth_usersettings"))
                    continue;

                foreach (var property in entry.Properties)
                {
                    if (property != null && property.OriginalValue != property.CurrentValue)
                    {
                        var sysAudit = new SYS_AuditLog();

                        sysAudit.ID = Guid.NewGuid();

                        string propertyName = property.Metadata.Name;

                        sysAudit.AUTH_Users_ID = Auth_Users_ID;
                        sysAudit.AUTH_Municipality_ID = Auth_Municipality_ID;

                        if (property.OriginalValue != null)
                        {
                            sysAudit.OriginalValue = property.OriginalValue.ToString();
                        }
                        if (property.CurrentValue != null)
                        {
                            sysAudit.CurrentValue = property.CurrentValue.ToString();
                        }

                        sysAudit.AuditType = entry.State.ToString();
                        sysAudit.TableName = entry.Entity.GetType().Name;
                        sysAudit.ColumnName = propertyName;
                        sysAudit.CreationDate = DateTime.Now;

                        AuditLogs.Add(sysAudit);
                    }
                }
            }

            using (var connection = context.CreateConnection())
            {
                connection.SYS_AuditLog.AddRange(AuditLogs);
                await connection.SaveChangesAsync();
            }

            return true;
        }
        private bool GenerateAuditLog(List<EntityEntry> ChangedEntries)
        {
            var AuditLogs = new List<SYS_AuditLog>();

            foreach (var entry in ChangedEntries)
            {
                if (entry.Entity is SYS_AuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;

                if (entry.Entity.GetType().Name.ToLower().StartsWith("sys_log"))
                    continue;

                if (entry.Entity.GetType().Name.ToLower().StartsWith("news_"))
                    continue;

                if (entry.Entity.GetType().Name.ToLower().StartsWith("msg_"))
                    continue;

                if (entry.Entity.GetType().Name.ToLower().StartsWith("text_system"))
                    continue;

                if (entry.Entity.GetType().Name.ToLower().StartsWith("conf_"))
                    continue;

                if (entry.Entity.GetType().Name.ToLower().StartsWith("auth_spid_log"))
                    continue;

                if (entry.Entity.GetType().Name.ToLower().StartsWith("auth_external"))
                    continue;

                if (entry.Entity.GetType().Name.ToLower().StartsWith("auth_veriff"))
                    continue;

                if (entry.Entity.GetType().Name.ToLower().StartsWith("pay_pago"))
                    continue;

                if (entry.Entity.GetType().Name.ToLower().StartsWith("d3_"))
                    continue;

                if (entry.Entity.GetType().Name.ToLower().StartsWith("auth_usersettings"))
                    continue;

                foreach (var property in entry.Properties)
                {
                    if (property != null && property.OriginalValue != property.CurrentValue)
                    {
                        var sysAudit = new SYS_AuditLog();

                        sysAudit.ID = Guid.NewGuid();

                        string propertyName = property.Metadata.Name;

                        sysAudit.AUTH_Users_ID = Auth_Users_ID;                        
                        sysAudit.AUTH_Municipality_ID = Auth_Municipality_ID;

                        if (property.OriginalValue != null)
                        {
                            sysAudit.OriginalValue = property.OriginalValue.ToString();
                        }
                        if (property.CurrentValue != null)
                        {
                            sysAudit.CurrentValue = property.CurrentValue.ToString();
                        }

                        sysAudit.AuditType = entry.State.ToString();
                        sysAudit.TableName = entry.Entity.GetType().Name;
                        sysAudit.ColumnName = propertyName;
                        sysAudit.CreationDate = DateTime.Now;

                        AuditLogs.Add(sysAudit);
                    }
                }
            }

            using (var connection = context.CreateConnection())
            {
                connection.SYS_AuditLog.AddRange(AuditLogs);
                connection.SaveChanges();
            }

            return true;
        }
        private object GetKey<T>(T entity)
        {
            var dbSet = GetDbSet();

            if (dbSet != null && dbSet.EntityType != null && dbSet.EntityType.FindPrimaryKey() != null)
            {
                var keyName = dbSet.EntityType.FindPrimaryKey().Properties.Select(x => x.Name).Single();

                return entity.GetType().GetProperty(keyName).GetValue(entity, null);
            }

            return null;
        }
        private T SetKeyDefaultValue<T>(T entity)
        {
            var dbSet = GetDbSet();

            if (dbSet != null && dbSet.EntityType != null && dbSet.EntityType.FindPrimaryKey() != null)
            {
                var keyName = dbSet.EntityType.FindPrimaryKey().Properties.Select(x => x.Name).Single();

                if(entity.GetType().GetProperty(keyName).PropertyType == typeof(Guid))
                {
                    var value = entity.GetType().GetProperty(keyName).GetValue(entity, null);

                    if (value == null || (Guid)value == Guid.Empty)
                    {
                        entity.GetType().GetProperty(keyName).SetValue(entity, Guid.NewGuid());
                    }
                }
            }

            return entity;
        }
    }
}
