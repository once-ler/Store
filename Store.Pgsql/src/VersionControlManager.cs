using System;
using System.Collections.Generic;
using System.Linq;
using Store.Interfaces;
using Store.Models;
using Store.Pgsql;

namespace Store {
  namespace Pgsql {

    public sealed class VersionControlManager : IVersionControlManager {
      public VersionControlManager(DBContext _dbContext) {
        this.dbContext = string.Format("Server={0};Port={1};Database={2};User Id={3};Password={4};CommandTimeout={5};",
          _dbContext.server,
          _dbContext.port,
          _dbContext.database,
          _dbContext.userId,
          _dbContext.password,
          _dbContext.commandTimeout
        );
      }

      /// <summary>
      /// createVersionControl() will throw if failure!
      /// </summary>
      /// <param name="friendlyName"></param>
      /// <returns></returns>
      public VersionControl createNewVersionControl(string friendlyName) {
        return createVersionControl("master", friendlyName);
      }

      /// <summary>
      /// createNewVersionControlFromExisting() will throw if failure!
      /// </summary>
      /// <param name="existingVersionId"></param>
      /// <param name="friendlyName"></param>
      /// <returns></returns>
      public VersionControl createNewVersionControlFromExisting(string existingVersionId, string friendlyName) {
        return createVersionControl(existingVersionId, friendlyName);
      }

      /// <summary>
      /// getVersionControls will throw if failure!
      /// </summary>
      /// <returns></returns>
      public List<VersionControl> getVersionControls() {
        this.versioncontrols.Clear();

        var runner = new CommandRunner(this.dbContext);
        var results = runner.ExecuteDynamic("select * from public.versioncontrol", null);

        foreach (var d in results) {
          this.versioncontrols.Add(new VersionControl {
            id = d.id,
            ts = d.ts,
            name = d.name
          });
        }
        return this.versioncontrols as List<VersionControl>;
      }

      private VersionControl createVersionControl(string fromVersion, string friendlyName) {
        var guid = "v$" + Guid.NewGuid().ToString().Replace("-", "_").Substring(0, 8);

        Tuple<string, object[]>[] cmds = {
          new Tuple<string, object[]>("select clone_schema(@0, @1)", new object[]{ fromVersion, guid }),
          new Tuple<string, object[]>("insert into public.versioncontrol (id, name) values (@0, @1) on conflict (id) DO UPDATE SET ts = now(), name = EXCLUDED.name", new object[]{ guid, friendlyName })
        };

        foreach (var cmd in cmds) {
          var runner = new CommandRunner(this.dbContext);
          Npgsql.NpgsqlCommand[] sqlcmd = { runner.BuildCommand(cmd.Item1, cmd.Item2) };

          try {
            runner.Transact(sqlcmd);
          } catch (Exception e) {
            throw e;
          }
        }

        // Try getting the record that was just added.
        this.getVersionControls();

        var rec = this.versioncontrols.FirstOrDefault(d => d.id == guid);
        if (rec == null) throw new KeyNotFoundException("VersionControl id: " + guid + " not found.");

        return rec;
      }

      /*
       * Private properties 
       */
      private List<VersionControl> versioncontrols = new List<VersionControl>();
      private string dbContext;
    }
  }
}
