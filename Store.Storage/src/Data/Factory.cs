using System.Data.Common;
using Store.Models;

namespace Store.Storage.Data {
  public class Factory<T> where T : DbConnection, new() {
    public string dbContext { get; set; }
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
  }
}
