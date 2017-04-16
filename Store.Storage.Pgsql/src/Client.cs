using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using Store.Interfaces;
using Store.Enumerations;
using Store.Models;
using Store.IoC;
using Store.Storage;
using Store.Storage.Data;

namespace Store.Storage {
  namespace Pgsql {
      
    public class Client<T> : Store.Storage.BaseClient<T>, IStore<T> where T : class, IModel, new() {

      public Client(DBContext _dbContext) : base(_dbContext) {
        var fac = new Factory<NpgsqlConnection>();
        dbConnection = fac.Get(_dbContext);
      }
      
      public override List<Record<T>> list(string version, int offset, int limit, string sortKey = "id", SortDirection sortDirection = SortDirection.Asc) {
        var resp = runner<dynamic>(
          () => createSchema(version),
          () => createStore(version, resolveTypeToString<T>()),
          () => runSqlDynamic(string.Format("select * from {0}.{1} order by current->>'{4}' {5} offset {2} limit {3}", new object[] { version, this.resolveTypeToString<T>(), offset.ToString(), limit.ToString(), sortKey, sortDirection.ToString() }))
        );
        var results = resp.LastOrDefault() as IEnumerable<dynamic>;
        
        List<Record<T>> list = new List<Record<T>>();

        foreach (var d in results) {
          var rec = makeRecord(d);
          list.Add(rec);
        }
        return list;
      }

      public override List<dynamic> list(string version, string typeOfStore, int offset, int limit, string sortKey = "id", string sortDirection = "Asc") {
        var resp = runner<dynamic>(
          () => createSchema(version),
          () => createStore(version, typeOfStore),
          () => runSqlDynamic(string.Format("select * from {0}.{1} order by current->>'{4}' {5} offset {2} limit {3}", new object[] { version, typeOfStore, offset.ToString(), limit.ToString(), sortKey, sortDirection }))
        );
        var results = resp.LastOrDefault() as IEnumerable<dynamic>;

        List<dynamic> list = new List<dynamic>();

        var ty = ServiceProvider.Instance.GetType(typeOfStore);

        foreach (var d in results) {
          JObject o = JObject.Parse(d.current);
          list.Add(o.ToObject(ty));
        }
        return list;
      }

      public override U save<U>(string version, U doc) {
        var dy = doc as dynamic;
        var id = typeof(U) == typeof(T) ? dy.id : dy.current.id;

        // Always fetch current record from storage
        Record<T> destRec = one<Record<T>>(version, "id", id);
        if (destRec == null) destRec = new Record<T> { id = id, name = id, ts = DateTime.Now, current = new T(), history = new List<History<T>>() };

        // Try to re-populate all properties of attributes that are types.
        // In this scenario, the incoming doc may only contain the "id" attribute and nothing else.
        // If we plainly use the incoming objects for those attributes, the parent document will be missing those properties that belong to those classes.
        var src = typeof(U) == typeof(T) ? doc as T : ((doc as Record<T>).current as T);
        recursePopulate(version, src); // Should it be from "master" or version when repopulating?

        // Merge the incoming source doc to the incumbent doc.
        T o = merge(destRec.current, src);
        (o as dynamic).ts = DateTime.Now;

        // Current is now merged
        destRec.current = o;
        destRec.ts = DateTime.Now;
        destRec.name = destRec.name ?? destRec.id;

        // Deque record into history
        destRec.history.Insert(0, new History<T> { id = Guid.NewGuid().ToString(), ts = DateTime.Now, source = o });
        
        // 1. Create schema and store if not exist
        // 2. Update store
        var resp = runner(
          () => createSchema(version),
          () => createStore(version, resolveTypeToString<T>()),
          () => upsertStore(version, destRec)
        );

        var response = resp.LastOrDefault();

        if (response != OperatonResult.Succeeded.ToString("F")) throw new Exception(response);
        return typeof(U) == typeof(T) ? destRec.current as U : destRec as U;
      }
      
      public override List<Record<T>> search(string version, string field, string search, int offset = 0, int limit = 10, string sortKey = "id", SortDirection sortDirection = SortDirection.Asc) {
        if (limit < 0 || limit > 500) throw new NotSupportedException("The limit for search must be between 1 and 500.");
        
        var resp = runner<dynamic>(
          () => createSchema(version),
          () => createStore(version, resolveTypeToString<T>()),
          () => runSqlDynamic(string.Format("select * from {0}.{1} where current->>'{2}' ~* '{3}' order by current->>'{6}' {7} offset {4} limit {5}", new object[] { version, this.resolveTypeToString<T>(), field, search, offset.ToString(), limit.ToString(), sortKey, sortDirection.ToString() }))
        );
        var results = resp.LastOrDefault() as IEnumerable<dynamic>;

        List<Record<T>> list = new List<Record<T>>();

        foreach (var d in results) {
          var rec = makeRecord(d);
          list.Add(rec);
        }
        return list;
      }

      public override long count(string version, string field = null, string search = null) {
        var sql = string.Format("select count(1) count from {0}.{1}", new object[] { version, this.resolveTypeToString<T>() });
        if (field != null && search != null) sql += string.Format(" where current->>'{0}' ~* '{1}'", new object[] { field, search });
        
        var resp = runner<dynamic>(
          () => createSchema(version),
          () => createStore(version, resolveTypeToString<T>()),
          () => runSqlDynamic(sql)
        );
        var results = resp.LastOrDefault() as IEnumerable<dynamic>;

        return results.FirstOrDefault().count;
      }

      /*
       * Protected methods
       */
      protected override string createSchema(string version) {
        return runSql(string.Format("select create_schema('{0}')", version));
      }

      protected override string createStore(string version, string store) {
        return string.Join(",", runner<string>(
          () => runSql(string.Format("select create_table('{0}', '{1}')", version, store)),
          () => runSql(string.Format("insert into public.versioncontrol(id, name) values('{0}', '{1}') on conflict(id) DO NOTHING", version, store))
        ));
      }
      
      protected override string upsertStore(string version, Record<T> rec) {
        // Serialize
        var currentJson = Newtonsoft.Json.JsonConvert.SerializeObject(rec.current);
        var historyJson = Newtonsoft.Json.JsonConvert.SerializeObject(rec.history);

        var sql = string.Format("insert into {0}.{1} (id, name, ts, current, history) values ('{2}', '{3}', now(), '{4}', '{5}') " +
          "on conflict (id) do update set ts = now(), current = EXCLUDED.current, history = EXCLUDED.history", 
          version, 
          resolveTypeToString<T>(), 
          rec.id,
          rec.name,
          currentJson, 
          historyJson
        );

        return runner(
          () => createSchema(version),
          () => createStore(version, resolveTypeToString<T>()),
          () => runSql(sql)
        ).LastOrDefault();
      }

      protected override U getOneRecord<U>(string version, string field, string value, Type typeOfParty = null) {
        var resp = runner<dynamic>(
          () => createSchema(version),
          () => createStore(version, resolveTypeToString<T>()),
          () => runSqlDynamic(string.Format("select * from {0}.{1} where current->>'{2}' = '{3}' limit 1", new object[] { version, this.resolveTypeToString<U>(), field, value }))
        );
        var results = resp.LastOrDefault() as IEnumerable<dynamic>;

        var d = results.FirstOrDefault();
        if (d == null) return null;
        var rec = makeRecord(d);
        // Find the last descendant of Affiliation<T> that derived from Model.
        // For example, T could be a class that derived from another class that derived from Affiliation<T>.  "C : B" where "B : Affiliation<T>"
        System.Reflection.TypeInfo ty = rec.current.GetType();        
        List<Type> baseTypes = ty.InheritsFrom();
        var affiliationWrapper = baseTypes.TakeWhile(t => t != typeof(Model)).LastOrDefault();
        // "party" of Participant and is unknown at runtime.
        if (affiliationWrapper != null && affiliationWrapper.Name == "Affiliation`1" && typeOfParty != null) convertPartyToType(version, rec.current.roster, typeOfParty);
        if (typeof(U) == typeof(T)) return rec.current;
        return rec;        
      }

      protected override dynamic getOneRecord(string version, string typeOfStore, string field, string value) {
        var store = typeOfStore.ToLower();

        var resp = runner<dynamic>(
          () => createSchema(version),
          () => createStore(version, resolveTypeToString<T>()),
          () => runSqlDynamic(string.Format("select * from {0}.{1} where current->>'{2}' = '{3}' limit 1", new object[] { version, store, field, value }))
        );
        var results = resp.LastOrDefault() as IEnumerable<dynamic>;

        var d = results.FirstOrDefault();
        if (d == null) return null;
        JObject o = JObject.Parse(d.current);
        var ty = ServiceProvider.Instance.GetType(typeOfStore);
        return o.ToObject(ty);
      }      
    }

  }
}
