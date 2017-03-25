using System;
using System.Data.Common;
using Store.Models;

namespace Store.Storage.Data {
  public class Factory<T> where T : DbConnection, new() {
    public string dbContext { get; set; }

    /// <summary>
    /// Default connection string for connecting to PostgreSQL.
    /// </summary>
    /// <param name="_dbContext"></param>
    /// <returns>DbConnection</returns>
    public T Get(DBContext _dbContext) {
      dbContext = string.Format("Server={0};Port={1};Database={2};User Id={3};Password={4};CommandTimeout={5};", 
        _dbContext.server, 
        _dbContext.port, 
        _dbContext.database, 
        _dbContext.userId, 
        _dbContext.password, 
        _dbContext.commandTimeout);

      var conn = new T();
      conn.ConnectionString = dbContext;
      return conn;
    }

    /// <summary>
    /// Use when the connection string is abitrary like SqlLite.
    /// i.e. "Data Source=C:\SQLITEDATABASES\SQLITEDB1.sqlite;Version=3;Count Changes=off;Journal Mode=off;Pooling=true;Cache Size=10000;Page Size=4096;Synchronous=off;"
    /// </summary>
    /// <param name="connectionString"></param>
    /// <returns>DbConnection</returns>
    public T Get(string connectionString) {
      dbContext = connectionString;
      var conn = new T();
      conn.ConnectionString = dbContext;
      return conn;
    }

    /// <summary>
    /// If you still like some structure:
    /// var db = new DBContext { server = "sql-server", database = "Foo" };
    /// Func&LT;DBContext, string&GT; func1 = (DBContext _db) => string.Format("Data Source={0};Initial Catalog={1};Integrated Security=SSPI;", _db.server, _db.database);
    /// var fac = new Factory<System.Data.SqlClient.SqlConnection>();
    /// DbConnection dbConnection = fac.Get(db, func1);
    /// </summary>
    /// <param name="_dbContext"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public T Get(DBContext _dbContext, Func<DBContext, string> func) {
      dbContext = func(_dbContext);
      var conn = new T();
      conn.ConnectionString = dbContext;
      return conn;
    }
  }
}
