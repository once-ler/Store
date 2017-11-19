using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Store.Models;
using Store.Storage.Data;
using Store.Interfaces;
using Store.Enumerations;

namespace Store.Storage.SqlServer {
  /// <summary>
  /// When you really just want to use the BaseClient functions like runDynamicSql().
  /// </summary>
  public class Client : BasicClient {
    public Client(DBContext _dbContext) : base(_dbContext) {
      var fac = new Factory<SqlConnection>();
      Func<DBContext, string> func = (DBContext _db) => _db.userId.Length == 0 || _db.password.Length == 0 ? string.Format("Data Source={0};Initial Catalog={1};Integrated Security=SSPI;", _db.server, _db.database) : string.Format("Data Source={0};Initial Catalog={1};User ID={2};Password={3};", _db.server, _db.database, _db.userId, _db.password);
      dbConnection = fac.Get(_dbContext, func);
    }
  }

  public class Client<T> : BaseClient<T>, IStore<T> where T : class, IModel, new() {
    public Client(DBContext _dbContext) : base(_dbContext) {
      var fac = new Factory<SqlConnection>();
      Func<DBContext, string> func = (DBContext _db) => _db.userId.Length == 0 || _db.password.Length == 0 ? string.Format("Data Source={0};Initial Catalog={1};Integrated Security=SSPI;", _db.server, _db.database) : string.Format("Data Source={0};Initial Catalog={1};User ID={2};Password={3};", _db.server, _db.database, _db.userId, _db.password);
      dbConnection = fac.Get(_dbContext, func);
    }

    public override List<Record<T>> list(string version, int offset, int limit, string sortKey, SortDirection sortDirection) {
      throw new NotImplementedException();
    }

    public override List<dynamic> list(string version, string typeOfStore, int offset, int limit, string sortKey = "id", string sortDirection = "Asc") {
      throw new NotImplementedException();
    }

    public override U save<U>(string version, U doc) {
      throw new NotImplementedException();
    }

    public override List<Record<T>> search(string version, string field, string search, int offset = 0, int limit = 10, string sortKey = "id", SortDirection sortDirection = SortDirection.Asc) {
      throw new NotImplementedException();
    }

    public override List<dynamic> search(string version, string typeOfStore, string field, string search, int offset = 0, int limit = 10, string sortKey = "id", string sortDirection = "Asc") {
      throw new NotImplementedException();
    }

    protected override string createSchema(string version) {
      throw new NotImplementedException();
    }

    protected override string createStore(string version, string store) {
      throw new NotImplementedException();
    }

    protected override string upsertStore(string version, Record<T> rec) {
      throw new NotImplementedException();
    }

    protected override U getOneRecord<U>(string version, string field, string value, Type typeOfParty = null) {
      throw new NotImplementedException();
    }

    protected override dynamic getOneRecord(string version, string typeOfStore, string field, string value) {
      throw new NotImplementedException();
    }

    public override long count(string version, string field = null, string search = null) {
      throw new NotImplementedException();
    }
    
  }

}
