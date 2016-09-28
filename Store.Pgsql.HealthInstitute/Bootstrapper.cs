using Store.Models;
using Store.Interfaces;
using Store.IoC;
using Store.Pgsql.HealthInstitute.Models;

namespace Store.Pgsql.HealthInstitute {
  public class Bootstrapper {
    public Bootstrapper() {
      dbContext = new DBContext { server = "127.0.0.1", port = 5432, database = "pccrms", userId = "editor", password = "editor" };
      ServiceProvider.Instance.Singleton<IVersionControlManager>(() => new VersionControlManager(this.dbContext));
      ServiceProvider.Instance.Singleton<Client<Personnel>>(() => new Client<Personnel>(this.dbContext));
    }

    private DBContext dbContext;
  }
}
