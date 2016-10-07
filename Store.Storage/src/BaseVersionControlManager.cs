using System.Data.Common;
using System.Collections.Generic;
using Store.Interfaces;
using Store.Models;

namespace Store {
  namespace Storage {

    public abstract class BaseVersionControlManager : IVersionControlManager {
      public BaseVersionControlManager(DBContext _dbContext) {
        dbContext = _dbContext;
      }

      /// <summary>
      /// createVersionControl() will throw if failure!
      /// </summary>
      /// <param name="friendlyName"></param>
      /// <returns></returns>
      public abstract VersionControl createNewVersionControl(string friendlyName);

      /// <summary>
      /// createNewVersionControlFromExisting() will throw if failure!
      /// </summary>
      /// <param name="existingVersionId"></param>
      /// <param name="friendlyName"></param>
      /// <returns></returns>
      public abstract VersionControl createNewVersionControlFromExisting(string existingVersionId, string friendlyName);

      /// <summary>
      /// getVersionControls will throw if failure!
      /// </summary>
      /// <returns></returns>
      public abstract List<VersionControl> getVersionControls();

      public abstract VersionControl createVersionControl(string fromVersion, string friendlyName);

      /*
       * Protected properties 
       */
      protected List<VersionControl> versioncontrols = new List<VersionControl>();
      protected DBContext dbContext;
      protected DbConnection dbConnection;
    }
  }
}
