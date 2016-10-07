using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace Store {
  namespace Storage {
    namespace Data {

      public class CommandRunner {
        // public string ConnectionString { get; set; }
        private DbConnection conn;
        /// <summary>
        /// Constructor - takes a connection string name
        /// </summary>
        /// <param name="connectionStringName">Connection Name as Defined in your App.config</param>
        // public CommandRunner(string connectionStringName) {
        //  this.ConnectionString = connectionStringName;
        // }

        public CommandRunner(DbConnection _dbConnection) {
          conn = _dbConnection;
        }

        /// <summary>
        /// Returns a single record, typed as you need
        /// </summary>
        public U ExecuteSingle<U>(string sql, params object[] args) where U : new() {
          return this.Execute<U>(sql, args).FirstOrDefault();
        }
        /// <summary>
        /// Returns a simple ExpandoObject with all results of a query
        /// </summary>
        public dynamic ExecuteSingleDynamic(string sql, params object[] args) {
          return this.ExecuteDynamic(sql, args).First();
        }

        /// Executes a typed query
        /// </summary>
        public DbDataReader OpenReader(string sql, params object[] args) {
          // var conn = new T(this.ConnectionString);
          // conn = new T();
          // conn.ConnectionString = this.ConnectionString;
          DbProviderFactory factory = DbProviderFactories.GetFactory(conn);
          var _conn = factory.CreateConnection();

          var cmd = BuildCommand(sql, args);
          cmd.Connection = _conn;
          //defer opening to the last minute
          try {
            _conn.Open();
          } catch (Exception e) {
            throw e;
          }
          //use a rdr here and yield back the projection
          //connection will close when rdr is finished
          DbDataReader rdr;
          try {
            rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
          } catch (Exception e) {
            throw e;
          }
          return rdr;
        }


        /// <summary>
        /// Executes a typed query
        /// </summary>
        public IEnumerable<U> Execute<U>(string sql, params object[] args) where U : new() {

          using (var rdr = OpenReader(sql, args)) {
            while (rdr.Read()) {
              yield return rdr.ToSingle<U>();
            }
            rdr.Dispose();
          }

        }

        /// <summary>
        /// Executes a query returning items in a dynamic list
        /// </summary>
        public IEnumerable<dynamic> ExecuteDynamic(string sql, params object[] args) {
          using (var rdr = OpenReader(sql, args)) {
            while (rdr.Read()) {
              yield return rdr.RecordToExpando();
            }
            rdr.Dispose();
          }
        }

        /// <summary>
        /// Convenience method for building a command
        /// </summary>
        /// <param name="sql">The SQL to execute with param names as @0, @1, @2 etc</param>
        /// <param name="args">The parameters to match the @ notations</param>
        /// <returns></returns>
        public DbCommand BuildCommand(string sql, params object[] args) {
          // var cmd = new DbCommand(sql);
          // var cmd = conn.CreateCommand();
          DbProviderFactory factory = DbProviderFactories.GetFactory(conn);
          var cmd = factory.CreateCommand();

          cmd.CommandText = sql;
          cmd.AddParams(args);
          return cmd;
        }

        /// <summary>
        /// A Transaction helper that executes a series of commands in a single transaction
        /// </summary>
        /// <param name="cmds">Commands built with BuildCommand</param>
        /// <returns></returns>
        public List<int> Transact(params DbCommand[] cmds) {
          var results = new List<int>();
          // var conn = new T();
          // conn.ConnectionString = this.ConnectionString;
          DbProviderFactory factory = DbProviderFactories.GetFactory(conn);
          var _conn = factory.CreateConnection();

          using (_conn) {
            _conn.Open();
            using (var tx = _conn.BeginTransaction()) {
              try {
                foreach (var cmd in cmds) {
                  cmd.Transaction = tx;
                  cmd.Connection = _conn;
                  results.Add(cmd.ExecuteNonQuery());
                }
                tx.Commit();
              } catch (DbException x) {
                tx.Rollback();
                throw (x);
              } finally {
                _conn.Close();
              }
            }
          }
          return results;
        }

        public void ExecuteCommand(string sql) {
          // var conn = new T();
          // conn.ConnectionString = this.ConnectionString;
          DbProviderFactory factory = DbProviderFactories.GetFactory(conn);
          var _conn = factory.CreateConnection();

          using (_conn) {
            _conn.Open();
            var cmd = _conn.CreateCommand();
            cmd.CommandText = sql;
            using (cmd) {
              cmd.Connection = _conn;

              // Retrieve all rows
              cmd.CommandText = sql;

              try {
                using (var reader = cmd.ExecuteReader()) {
                  while (reader.Read()) {
                    dynamic o = reader.GetString(2);
                    Console.WriteLine(reader.GetString(2));
                  }
                }
              } catch (Exception e) {
                throw e;
              }
            }
          }
        }
      }

    }
  }
}
