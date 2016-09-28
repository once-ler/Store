using System;
using Store.Models;
using Store.Interfaces;
using Store.IoC;
using Store.Pgsql.HealthInstitute.Models;

namespace Store.Pgsql.HealthInstitute {
  public class Bootstrapper {
    public Bootstrapper(Action next) {
      var dbContext = new DBContext { server = "127.0.0.1", port = 5432, database = "pccrms", userId = "editor", password = "editor" };
      ServiceProvider.Instance.Singleton<IVersionControlManager>(() => new VersionControlManager(dbContext));
      ServiceProvider.Instance.Singleton<Client<Personnel>>(() => new Client<Personnel>(dbContext));
      next();
    }
  }
}
