using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Store.Interfaces;
using Store.Models;
using Store.IoC;
using Store.Storage.Data;

namespace Store {
  namespace Storage {

    public abstract class BaseClient<T> : IStore<T> where T : class, IModel, new() {

      public BaseClient(DBContext _dbContext) {
        dbContext = _dbContext;
      }
      
      /// <summary>
      /// list must be overridden.
      /// </summary>
      /// <param name="version"></param>
      /// <param name="offset"></param>
      /// <param name="limit"></param>
      /// <returns></returns>
      public abstract List<Record<T>> list(string version, int offset, int limit);

      /// <summary>
      /// save must be overridden.
      /// </summary>
      /// <typeparam name="U"></typeparam>
      /// <param name="version"></param>
      /// <param name="doc"></param>
      /// <returns></returns>
      public abstract U save<U>(string version, U doc) where U : class;

      /// <summary>
      /// search must be overridden.
      /// </summary>
      /// <param name="version"></param>
      /// <param name="field"></param>
      /// <param name="search"></param>
      /// <returns></returns>
      public abstract List<Record<T>> search(string version, string field, string search);

      public abstract int count(string version, string field = null, string search = null);
      
      public U one<U>(string version, string field, string value, Type typeOfParty = null) where U : class {
        var rec = getOneRecord<U>(version, field, value, typeOfParty);
        if (rec == null) throw new Exception("IModel for field: " + field + " and value: " + value + " not found.");
        return rec as U;
      }

      public dynamic one(string version, string typeOfStore, string field, string value) {
        var rec = getOneRecord(version, typeOfStore, field, value);
        if (rec == null) throw new Exception("IModel for field: " + field + " and value: " + value + " not found.");
        return rec;
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
        // The expected behavior for arrays is replace, not the default concat
        var mergeSettings = new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Replace };
        var destObj = JObject.FromObject(dest);
        var srcObj = JObject.FromObject(source);
        destObj.Merge(srcObj, mergeSettings);
        return destObj.ToObject<T>();
      }

      public Record<Affiliation<U>> associate<U, M>(string version, string recordId, string partyId) where U : Participant, new() where M : Model {
        var ty = typeof(T);

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
        return save(version, rec) as Record<Affiliation<U>>;
      }

      /*
       * Protected Methods
       */

      /// <summary>
      /// createSchema must be overridden.
      /// </summary>
      /// <param name="version"></param>
      /// <returns></returns>
      protected abstract string createSchema(string version);

      /// <summary>
      /// createStore must be overridden
      /// </summary>
      /// <param name="version"></param>
      /// <param name="store"></param>
      /// <returns></returns>
      protected abstract string createStore(string version, string store);

      /// <summary>
      /// upsertStore must be overriden.
      /// </summary>
      /// <param name="version"></param>
      /// <param name="rec"></param>
      /// <returns></returns>
      protected abstract string upsertStore(string version, Record<T> rec);

      /// <summary>
      /// getOneRecord<U> must be overridden.
      /// </summary>
      /// <typeparam name="U"></typeparam>
      /// <param name="version"></param>
      /// <param name="field"></param>
      /// <param name="value"></param>
      /// <param name="typeOfParty"></param>
      /// <returns></returns>
      protected abstract U getOneRecord<U>(string version, string field, string value, Type typeOfParty = null) where U : class;

      /// <summary>
      /// getOneRecord must be overridden.
      /// </summary>
      /// <param name="version"></param>
      /// <param name="typeOfStore"></param>
      /// <param name="field"></param>
      /// <param name="value"></param>
      /// <returns></returns>
      protected abstract dynamic getOneRecord(string version, string typeOfStore, string field, string value);

      protected string resolveTypeToString<U>() {
        if (typeof(U) == typeof(Record<T>)) return typeof(T).Name.ToLower();
        if (typeof(U).IsGenericType == true) return typeof(U).GetGenericArguments().FirstOrDefault().Name.ToLower();
        return typeof(U).Name.ToLower();
      }

      protected void recursePopulate(string version, object doc) {
        var props = doc.GetType().GetProperties();

        foreach (var prop in props) {
          var value = prop.GetValue(doc, null);

          if (prop.PropertyType.BaseType == typeof(Model)) {
            dynamic svc = ServiceProvider.Instance.GetStore(prop.PropertyType.Name);
            object o = null;
            try { o = svc.one(version, prop.PropertyType.Name, "id", (value as Model).id); } catch (Exception e) { }

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

      protected void convertPartyToType(dynamic partipants, Type typeOfParty) {
        if (partipants == null) return;
        foreach (var p in partipants) {
          var json = JsonConvert.SerializeObject(p.party);
          JObject o = JObject.Parse(json);
          p.party = o.ToObject(typeOfParty);
        }
      }

      /*
       * NewSQL Specific Methods
       */
      protected string runner(params Func<string>[] actions) { List<string> results = new List<string>(); foreach (var act in actions) results.Add(act()); return string.Join(",", results); }

      protected string tryCatch(Action act) { try { act(); } catch (Exception e) { return e.Message; } return OperatonResult.Succeeded.ToString("F"); }

      protected string runSql(string sql) {
        return tryCatch(() => { var runner = new CommandRunner(dbConnection); runner.Transact(new DbCommand[] { runner.BuildCommand(sql, null) }); });
      }

      /*
       * Protected Properties 
       */
      protected DbConnection dbConnection;
      protected DBContext dbContext;
      protected enum OperatonResult { Succeeded = 1, Failed = 0 };
    }

  }
}
