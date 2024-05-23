using ICWebApp.DataStore.MSSQL.Interfaces;
using ICWebApp.DataStore.MSSQL.Interfaces.UnitOfWork;
using ICWebApp.DataStore.MSSQL.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ICWebApp.DataStore.MSSQL.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        public Guid? Auth_Users_ID { get; set; }
        public Guid? Auth_Municipality_ID { get; set; }
        private readonly DBContext _context;
        public UnitOfWork(DBContext context)
        {
            _context = context;
        }
        public Dictionary<Type, object> repositories = new Dictionary<Type, object>();
        public IGenericRepository<T> Repository<T>() where T : class
        {
            if (repositories.Keys.Contains(typeof(T)) == true)
            {
                return repositories[typeof(T)] as IGenericRepository<T>;
            }

            IGenericRepository<T> repo = new GenericRepository<T>(_context);

            repo.Auth_Users_ID = Auth_Users_ID;
            repo.Auth_Municipality_ID = Auth_Municipality_ID;

            repositories.Add(typeof(T), repo);

            return repo;
        }
        public async Task<int> SQLQueryAsync(FormattableString sql)
        {
            int result = await _context.CreateConnection().Database.ExecuteSqlAsync(sql);
            return result;
        }
        public int SQLQuery(FormattableString sql)
        {
            int result = _context.CreateConnection().Database.ExecuteSql(sql);
            return result;
        }
        public void SetAuditLogdata(Guid? Auth_Users_ID, Guid? Auth_Municipality_ID)
        {
            this.Auth_Users_ID = Auth_Users_ID;            
            this.Auth_Municipality_ID = Auth_Municipality_ID;

            foreach(var rep in repositories)
            {
                if(rep.Value != null)
                {
                    rep.Value.GetType().GetProperty("Auth_Municipality_ID").SetValue(rep.Value, this.Auth_Municipality_ID);
                    rep.Value.GetType().GetProperty("Auth_Users_ID").SetValue(rep.Value, this.Auth_Users_ID);
                }
            }
        }
    }
}
