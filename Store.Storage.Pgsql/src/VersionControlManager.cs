using System;
using System.Data;
using System.Data.Common;
using System.Collections.Generic;
using System.Linq;
using Npgsql;
using Store.Interfaces;
using Store.Models;
using Store.Storage;
using Store.Storage.Data;

namespace Store.Storage {
  namespace Pgsql {

    public sealed class VersionControlManager : BaseVersionControlManager {

      public VersionControlManager(DBContext _dbContext) : base(_dbContext) {
        var fac = new Factory<NpgsqlConnection>();
        dbConnection = fac.Get(_dbContext);
      }
      
      /// <summary>
      /// createVersionControl() will throw if failure!
      /// </summary>
      /// <param name="friendlyName"></param>
      /// <returns></returns>
      public override VersionControl createNewVersionControl(string friendlyName) {
        return createVersionControl("master", friendlyName);
      }

      /// <summary>
      /// createNewVersionControlFromExisting() will throw if failure!
      /// </summary>
      /// <param name="existingVersionId"></param>
      /// <param name="friendlyName"></param>
      /// <returns></returns>
      public override VersionControl createNewVersionControlFromExisting(string existingVersionId, string friendlyName) {
        return createVersionControl(existingVersionId, friendlyName);
      }

      /// <summary>
      /// getVersionControls will throw if failure!
      /// </summary>
      /// <returns></returns>
      public override List<VersionControl> getVersionControls() {
        this.versioncontrols.Clear();

        var runner = new CommandRunner(dbConnection);
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

      /// <summary>
      /// Implement
      /// </summary>
      /// <param name="versionId"></param>
      /// <param name="friendlyName"></param>      
      /// <returns></returns>
      public override VersionControl createNewVersionControl(string versionId, string friendlyName) {
        return createVersionControl("master", friendlyName, versionId);
      }

      protected override VersionControl createVersionControl(string fromVersion, string friendlyName, string explicitNewVersionId = null) {
        var guid = explicitNewVersionId ?? "v$" + Guid.NewGuid().ToString().Replace("-", "_").Substring(0, 8);

        Tuple<string, object[]>[] cmds = {
          new Tuple<string, object[]>("select clone_schema(@0, @1)", new object[]{ fromVersion, guid }),
          new Tuple<string, object[]>("insert into public.versioncontrol (id, name) values (@0, @1) on conflict (id) DO UPDATE SET ts = now(), name = EXCLUDED.name", new object[]{ guid, friendlyName })
        };

        foreach (var cmd in cmds) {
          var runner = new CommandRunner(dbConnection);
          DbCommand[] sqlcmd = { runner.BuildCommand(cmd.Item1, cmd.Item2) };

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
      
    }
  }
}
