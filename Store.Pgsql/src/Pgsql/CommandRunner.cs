﻿using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Store {
  namespace Pgsql {

    internal class CommandRunner {
      public string ConnectionString { get; set; }
      /// <summary>
      /// Constructor - takes a connection string name
      /// </summary>
      /// <param name="connectionStringName">Connection Name as Defined in your App.config</param>
      public CommandRunner(string connectionStringName) {
        // this.ConnectionString = pccrms.Properties.Settings.Default.DBContext;
        this.ConnectionString = connectionStringName;
      }

      /// <summary>
      /// Returns a single record, typed as you need
      /// </summary>
      public T ExecuteSingle<T>(string sql, params object[] args) where T : new() {
        return this.Execute<T>(sql, args).FirstOrDefault();
      }
      /// <summary>
      /// Returns a simple ExpandoObject with all results of a query
      /// </summary>
      public dynamic ExecuteSingleDynamic(string sql, params object[] args) {
        return this.ExecuteDynamic(sql, args).First();
      }

      /// Executes a typed query
      /// </summary>
      public NpgsqlDataReader OpenReader(string sql, params object[] args) {
        var conn = new NpgsqlConnection(this.ConnectionString);
        var cmd = BuildCommand(sql, args);
        cmd.Connection = conn;
        //defer opening to the last minute
        try {
          conn.Open();
        } catch (Exception e) {
          throw e;
        }
        //use a rdr here and yield back the projection
        //connection will close when rdr is finished
        Npgsql.NpgsqlDataReader rdr;
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
      public IEnumerable<T> Execute<T>(string sql, params object[] args) where T : new() {

        using (var rdr = OpenReader(sql, args)) {
          while (rdr.Read()) {
            yield return rdr.ToSingle<T>();
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
      public NpgsqlCommand BuildCommand(string sql, params object[] args) {
        // var stmt = string.Format(sql, args ?? new object[0]);
        var cmd = new NpgsqlCommand(sql);
        cmd.AddParams(args);
        return cmd;
      }

      /// <summary>
      /// A Transaction helper that executes a series of commands in a single transaction
      /// </summary>
      /// <param name="cmds">Commands built with BuildCommand</param>
      /// <returns></returns>
      public List<int> Transact(params NpgsqlCommand[] cmds) {
        var results = new List<int>();
        using (var conn = new NpgsqlConnection(this.ConnectionString)) {
          conn.Open();
          using (var tx = conn.BeginTransaction()) {
            try {
              foreach (var cmd in cmds) {
                cmd.Transaction = tx;
                cmd.Connection = conn;
                results.Add(cmd.ExecuteNonQuery());
              }
              tx.Commit();
            } catch (NpgsqlException x) {
              tx.Rollback();
              throw (x);
            } finally {
              conn.Close();
            }
          }
        }
        return results;
      }

      public void ExecuteCommand(string sql) {
        using (var conn = new NpgsqlConnection(this.ConnectionString)) {
          conn.Open();
          using (var cmd = new NpgsqlCommand()) {
            cmd.Connection = conn;

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
              // System.Console.WriteLine(e.Message);
            }
          }
        }
      }
    }
  }
}
