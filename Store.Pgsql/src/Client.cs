using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Store.Interfaces;
using Store.Models;

namespace Store {
  namespace Pgsql {
      
    public class Client<T> : IStore<T> where T : class, IModel {
      
      public Client(DBContext _dbContext) {
        this.dbContext = string.Format("Server={0};Port={1};Database={2};User Id={3};Password={4};CommandTimeout={5};", _dbContext.server, _dbContext.port, _dbContext.database, _dbContext.userId, _dbContext.password, _dbContext.commandTimeout);
      }

      public List<Record<T>> list(string version, int offset, int limit) {
        List<Record<T>> list = new List<Record<T>>();
        var runner = new CommandRunner(this.dbContext);
        var sql = string.Format("select * from {0}.{1} order by id desc offset {2} limit {3}", new object[] { version, this.resolveTypeToString<T>(), offset.ToString(), limit.ToString() });
        var results = runner.ExecuteDynamic(sql, null);

        foreach (var d in results) {
          var rec = this.makeRecord(d);
          list.Add(rec);
        }
        return list;
      }

      public U one<U>(string version, string field, string value) where U : class {
        var rec = this.getOneRecord<U>(version, field, value);
        if (rec == null) throw new Exception("IModel for field: " + field + " and value: " + value + " not found.");
        return rec as U;
      }

      public U save<U>(string version, U doc) where U : class {
        Record<T> rec = null;
        Model m = null;
        if (typeof(U) == typeof(Record<T>)) {
          rec = doc as Record<T>;
        } else {
          m = doc as Model;
          rec = this.getOneRecord<Record<T>>(version, "id", m.id);
        }
        // Deque record into history
        rec.history.Insert(0, new History<T> { id = Guid.NewGuid().ToString(), ts = DateTime.Now, source = rec.current });
        // Update store
        var response = upsertStore(version, rec);
        if (response != OperatonResult.Succeeded.ToString("F")) throw new Exception(response);
        return rec as U;
      }

      public Record<T> replaceFromHistory(string version, string recordId, string historyId) {
        var rec = this.getOneRecord<Record<T>>(version, "id", recordId);
        if (rec == null) throw new Exception("Record id: " + recordId + " not found.");

        // Select from history
        var his = rec.history.FirstOrDefault(d => d.id == historyId);
        // Replace current with selected
        rec.current = his.source;

        // Deque selected <This should be done on save()!>
        return save(version, rec);
      }

      public List<Record<T>> search(string version, string field, string search) {
        List<Record<T>> list = new List<Record<T>>();
        var runner = new CommandRunner(this.dbContext);
        var sql = string.Format("select * from {0}.{1} where current->>'{2}' ~* '{3}' limit 10", new object[] { version, this.resolveTypeToString<T>(), field, search });
        var results = runner.ExecuteDynamic(sql, null);

        foreach (var d in results) {
          var rec = this.makeRecord(d);
          list.Add(rec);
        }
        return list;
      }

      public Record<T> makeRecord(dynamic d) {
        // current
        JObject o = JObject.Parse(d.current);
        T bk = o.ToObject<T>();
        // history
        JArray arr = JArray.Parse(d.history);
        var versioncontrols = arr.ToObject<List<History<T>>>();
        // record
        Record<T> rec = new Record<T> { id = d.id, current = bk, history = versioncontrols };

        return rec;
      }

      public Record<T> makeRecord(string jsonString) {
        return makeRecord(JObject.Parse(jsonString));
      }

      /*
        * Protected properties 
        */
      protected string dbContext = ""; // Properties.Settings.Default.DBContext;
      protected enum OperatonResult { Succeeded = 1, Failed = 0 };

      /*
        * Protected methods
        */
      protected string resolveTypeToString<U>() {
        if (typeof(U) == typeof(Record<T>)) return typeof(T).Name.ToLower();
        return typeof(U).Name.ToLower();
      }

      protected string upsertStore(string version, Record<T> rec) {
        // Serialize
        var currentJson = Newtonsoft.Json.JsonConvert.SerializeObject(rec.current);
        var historyJson = Newtonsoft.Json.JsonConvert.SerializeObject(rec.history);

        var runner = new CommandRunner(this.dbContext);
        var sql = string.Format("insert into {0}.{1} (id, ts, current, history) values ('{2}', now(), '{3}', '{4}') " +
          "on conflict (id) do update set ts = now(), current = EXCLUDED.current, history = EXCLUDED.history", 
          version, 
          this.resolveTypeToString<T>(), 
          rec.id, 
          currentJson, 
          historyJson
        );

        Npgsql.NpgsqlCommand[] cmds = { runner.BuildCommand(sql, null) };
        try {
          runner.Transact(cmds.ToArray());
        } catch (Exception e) {
          return e.Message;
        }
        return OperatonResult.Succeeded.ToString("F"); // Will display "Succeeded"
      }

      protected U getOneRecord<U>(string version, string field, string value) where U : class {
        var runner = new CommandRunner(this.dbContext);
        var sql = string.Format("select * from {0}.{1} where current->>'{2}' = '{3}' limit 1", new object[] { version, this.resolveTypeToString<U>(), field, value });
        var results = runner.ExecuteDynamic(sql, null);
        var d = results.FirstOrDefault();
        if (d == null) return null;
        var rec = this.makeRecord(d);
        if (typeof(U) == typeof(Record<T>)) {
          return rec;
        }
        return rec.current;
      }

    }

  }
}
