namespace ICWebApp.DataStore.MSSQL.Interfaces.UnitOfWork
{
    public interface IUnitOfWork
    {
        public Guid? Auth_Users_ID { get; set; }
        public Guid? Auth_Municipality_ID { get; set; }
        public IGenericRepository<T> Repository<T>() where T : class;
        public Task<int> SQLQueryAsync(FormattableString sql);
        public int SQLQuery(FormattableString sql);
        public void SetAuditLogdata(Guid? Auth_Users_ID, Guid? Auth_Municipality_ID);

    }
}
