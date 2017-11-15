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
      Func<DBContext, string> func = (DBContext _db) => _db.userId.Length == 0 || _db.password.Length == 0 ? string.Format("Data Source={0};Initial Catalog={1};Integrated Security=SSPI;", _db.server, _db.database) : string.Format("Data Source={0};Initial Catalog={1};User ID={2};Password={3};", _db.server, _db.database, _db.userId, _db.password);
      dbConnection = fac.Get(_dbContext, func);
    }

    protected string tryCatch(Action act) { try { act(); } catch (Exception e) { return e.Message; } return OperatonResult.Succeeded.ToString("F"); }

    public string runSql(string sql) {
      return tryCatch(() => { var runner = new CommandRunner(dbConnection); runner.Transact(new DbCommand[] { runner.BuildCommand(sql, null) }); });
    }

    public IEnumerable<dynamic> runSqlDynamic(string sql) {
      var runner = new CommandRunner(dbConnection); return runner.ExecuteDynamic(sql, null);
    }

    protected DbConnection dbConnection;
    protected DBContext dbContext;
    protected enum OperatonResult { Succeeded = 1, Failed = 0 };
  }
}
