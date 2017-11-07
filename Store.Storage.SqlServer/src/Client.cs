using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using Store.Models;
using Store.Storage.Data;

namespace Store.Storage.SqlServer {
  public class Client {
    public Client(DBContext _dbContext) {
      var fac = new Factory<SqlConnection>();
      dbConnection = fac.Get(_dbContext);
    }

    protected string tryCatch(Action act) { try { act(); } catch (Exception e) { return e.Message; } return OperatonResult.Succeeded.ToString("F"); }

    internal string runSql(string sql) {
      return tryCatch(() => { var runner = new CommandRunner(dbConnection); runner.Transact(new DbCommand[] { runner.BuildCommand(sql, null) }); });
    }

    internal IEnumerable<dynamic> runSqlDynamic(string sql) {
      var runner = new CommandRunner(dbConnection); return runner.ExecuteDynamic(sql, null);
    }

    protected DbConnection dbConnection;
    protected DBContext dbContext;
    protected enum OperatonResult { Succeeded = 1, Failed = 0 };
  }
}
