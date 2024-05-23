using ICWebApp.DataStore.MSSQL.Contexts;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.DataStore.MSSQL
{
    public class DBContext
    {

#if DEBUG
        public ComunixTestDBContext CreateConnection()
        {
            var options = new DbContextOptionsBuilder<ComunixTestDBContext>();

            return new ComunixTestDBContext(options.Options);
        }
#else
        
        public ComunixDBContext CreateConnection()
        {
            var options = new DbContextOptionsBuilder<ComunixDBContext>();

            return new ComunixDBContext(options.Options);
        }
#endif
    }
}
