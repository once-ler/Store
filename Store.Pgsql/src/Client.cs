using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Store.Interfaces;
using Store.Models;
using Store.IoC;

namespace Store {
  namespace Pgsql {
      
    public class Client<T> : IStore<T> where T : class, IModel, new() {
      
      public Client(DBContext _dbContext) {
        dbContext = string.Format("Server={0};Port={1};Database={2};User Id={3};Password={4};CommandTimeout={5};", _dbContext.server, _dbContext.port, _dbContext.database, _dbContext.userId, _dbContext.password, _dbContext.commandTimeout);
      }

      public List<Record<T>> list(string version, int offset, int limit) {
        List<Record<T>> list = new List<Record<T>>();
        var runner = new CommandRunner(dbContext);
        var sql = string.Format("select * from {0}.{1} order by id desc offset {2} limit {3}", new object[] { version, this.resolveTypeToString<T>(), offset.ToString(), limit.ToString() });
        var results = runner.ExecuteDynamic(sql, null);

        foreach (var d in results) {
          var rec = makeRecord(d);
          list.Add(rec);
        }
        return list;
      }

      public U one<U>(string version, string field, string value) where U : class {
        var rec = getOneRecord<U>(version, field, value);
        if (rec == null) throw new Exception("IModel for field: " + field + " and value: " + value + " not found.");
        return rec as U;
      }

      public dynamic one(string version, string typeOfStore, string field, string value) {
        var rec = getOneRecord(version, typeOfStore, field, value);
        if (rec == null) throw new Exception("IModel for field: " + field + " and value: " + value + " not found.");
        return rec;
      }

      public U save<U>(string version, U doc, bool mergeBeforeSave = true) where U : class {
        var id = typeof(U) == typeof(T) ? (doc as Model).id : ((doc as Record<T>).current as Model).id;
        
        // Always fetch current record from storage
        Record<T> destRec = null;
        try {
          destRec = one<Record<T>>(version, "id", id);
        } catch (Exception e) {
          destRec = new Record<T> { id = id, ts = DateTime.Now, current = new T(), history = new List<History<T>>() };
        }

        // Try to re-populate all properties of attributes that are types.
        // In this scenario, the incoming doc may only contain the "id" attribute and nothing else.
        // If we plainly use the incoming objects for those attributes, the parent document will be missing those properties that belong to those classes.
        var src = typeof(U) == typeof(T) ? doc as T : ((doc as Record<T>).current as T);
        recursePopulate(version, src); // Should it be from "master" or version when repopulating?

        // Merge the incoming source doc to the incumbent doc.
        T o = (mergeBeforeSave == true ? merge(destRec.current, src) : src);
        (o as Model).ts = DateTime.Now;

        // Current is now merged
        destRec.current = o;
        destRec.ts = DateTime.Now;
        // Deque record into history
        destRec.history.Insert(0, new History<T> { id = Guid.NewGuid().ToString(), ts = DateTime.Now, source = o });
        // Create schema and store if not exist
        runner(() => createSchema(version), () => createStore(version, resolveTypeToString<T>()));
        // Update store
        var response = upsertStore(version, destRec);
        if (response != OperatonResult.Succeeded.ToString("F")) throw new Exception(response);
        return typeof(U) == typeof(T) ? destRec.current as U : destRec as U;
      }

      public Record<T> replaceFromHistory(string version, string recordId, string historyId) {
        var rec = getOneRecord<Record<T>>(version, "id", recordId);
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
        var runner = new CommandRunner(dbContext);
        var sql = string.Format("select * from {0}.{1} where current->>'{2}' ~* '{3}' limit 10", new object[] { version, this.resolveTypeToString<T>(), field, search });
        var results = runner.ExecuteDynamic(sql, null);

        foreach (var d in results) {
          var rec = makeRecord(d);
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
        var hist = arr.ToObject<List<History<T>>>();
        // record
        Record<T> rec = new Record<T> { id = d.id, current = bk, history = hist };

        return rec;
      }

      public Record<T> makeRecord(string jsonString) {
        return makeRecord(JObject.Parse(jsonString));
      }

      public T merge(T dest, T source) {
        var destObj = JObject.FromObject(dest);
        var srcObj = JObject.FromObject(source);
        destObj.Merge(srcObj);
        return destObj.ToObject<T>();
      }

      public Record<Affiliation<U>> associate<U, M>(string version, string recordId, string partyId) where U : Participant, new() where M : Model {
        var rec = one<Record<T>>(version, "id", recordId);
        if (rec == null) throw new KeyNotFoundException("Record id: " + recordId + " not found.");

        // Get the Store of type M
        dynamic partyStore = ServiceProvider.Instance.GetStore(typeof(M).Name);
        
        var party = partyStore.one<Record<M>>(version, "id", partyId);
        if (party == null) throw new KeyNotFoundException("party id: " + partyId + " not found.");

        var program = rec.current as Affiliation<U>;
        var partyExist = program.roster.FirstOrDefault(d => d.party != null && d.party.id == partyId);
        if (partyExist == null) {
          // Derived Participant can have custom attributes like effectiveDate a d isLeadership
          U u = new U();
          u.id = party.id;
          u.ts = DateTime.Now;
          u.party = party.current as M;
          (rec.current as Affiliation<U>).roster.Add(u);
        }

        return save(version, rec) as Record<Affiliation<U>>;
      }

      public Record<Affiliation<U>> disassociate<U>(string version, string recordId, string partyId) where U : Participant {
        var rec = one<Record<T>>(version, "id", recordId);
        if (rec == null) throw new KeyNotFoundException("Record id: " + recordId + " not found.");

        var program = rec.current as Affiliation<U>;
        var removedMember = program.roster.Where(d => d.party.id != partyId);
        (rec.current as Affiliation<U>).roster = removedMember.ToList();
        return save(version, rec, false) as Record<Affiliation<U>>;
      }

      /*
       * Protected properties 
       */
      protected string dbContext = "";
      protected enum OperatonResult { Succeeded = 1, Failed = 0 };

      /*
       * Protected methods
       */
      protected string createSchema(string version) {
        return runSql(string.Format("select create_schema('{0}')", version));
      }

      protected string createStore(string version, string store) {
        return runner(
          () => runSql(string.Format("select create_table('{0}', '{1}')", version, store)),
          () => runSql(string.Format("insert into public.versioncontrol(id, name) values('{0}', '{1}') on conflict(id) DO NOTHING", version, store))
        );
      }
      
      protected string resolveTypeToString<U>() {
        if (typeof(U) == typeof(Record<T>)) return typeof(T).Name.ToLower();
        if (typeof(U).IsGenericType == true) return typeof(U).GetGenericArguments().FirstOrDefault().Name.ToLower();
        return typeof(U).Name.ToLower();
      }

      protected string upsertStore(string version, Record<T> rec) {
        // Serialize
        var currentJson = Newtonsoft.Json.JsonConvert.SerializeObject(rec.current);
        var historyJson = Newtonsoft.Json.JsonConvert.SerializeObject(rec.history);

        var runner = new CommandRunner(dbContext);
        var sql = string.Format("insert into {0}.{1} (id, ts, current, history) values ('{2}', now(), '{3}', '{4}') " +
          "on conflict (id) do update set ts = now(), current = EXCLUDED.current, history = EXCLUDED.history", 
          version, 
          resolveTypeToString<T>(), 
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
        var runner = new CommandRunner(dbContext);
        var sql = string.Format("select * from {0}.{1} where current->>'{2}' = '{3}' limit 1", new object[] { version, this.resolveTypeToString<U>(), field, value });
        var results = runner.ExecuteDynamic(sql, null);
        var d = results.FirstOrDefault();
        if (d == null) return null;
        var rec = makeRecord(d);
        if (typeof(U) == typeof(T)) return rec.current;
        return rec;
      }

      protected dynamic getOneRecord(string version, string typeOfStore, string field, string value) {
        var store = typeOfStore.ToLower();
        var runner = new CommandRunner(dbContext);
        var sql = string.Format("select * from {0}.{1} where current->>'{2}' = '{3}' limit 1", new object[] { version, store, field, value });
        var results = runner.ExecuteDynamic(sql, null);
        var d = results.FirstOrDefault();
        if (d == null) return null;
        JObject o = JObject.Parse(d.current);
        var ty = ServiceProvider.Instance.GetType(typeOfStore);
        return o.ToObject(ty);
      }

      protected void recursePopulate(string version, object doc) {
        var props = doc.GetType().GetProperties();

        foreach (var prop in props) {
          var value = prop.GetValue(doc, null);

          if (prop.PropertyType.BaseType == typeof(Model)) {
            dynamic svc = ServiceProvider.Instance.GetStore(prop.PropertyType.Name);
            object o = null;
            try { o = svc.one(version, prop.PropertyType.Name, "id", (value as Model).id); } 
            catch (Exception e) { }

            if (o != null) doc.GetType().GetProperty(prop.Name).SetValue(doc, o, null);
            if (value != null) recursePopulate(version, value);
          }
          if (value is IList && value.GetType().IsGenericType) {
            foreach (var item in value as IList) {
              recursePopulate(version, item);
            }
          }
        }
      }

      private string runner(params Func<string>[] actions) { List<string> results = new List<string>(); foreach (var act in actions) results.Add(act()); return string.Join(",", results); }

      private string tryCatch(Action act) { try { act(); } catch (Exception e) { return e.Message; } return OperatonResult.Succeeded.ToString("F"); }

      private string runSql(string sql) {
        return tryCatch(() => { var runner = new CommandRunner(dbContext); runner.Transact(new Npgsql.NpgsqlCommand[] { runner.BuildCommand(sql, null) }); });
      }

    }

  }
}
