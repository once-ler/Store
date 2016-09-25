using Store.Models;
using Store.Pgsql.Test.fixtures.Models;

namespace Store.Pgsql.Test.fixtures {
  namespace Clients {

    internal sealed class DroidClient<T> : Client<T> where T : Droid {
      public DroidClient(DBContext _dbContext)
        : base(_dbContext) { }
    }

    internal sealed class HumanClient<T> : Client<T> where T : Human {
      public HumanClient(DBContext _dbContext)
        : base(_dbContext) { }
    }

    internal sealed class RebelAllianceClient<T> : Client<T> where T : RebelAlliance {
      public RebelAllianceClient(DBContext _dbContext)
        : base(_dbContext) { }
    }

    internal sealed class EmpireClient<T> : Client<T> where T : Empire {
      public EmpireClient(DBContext _dbContext)
        : base(_dbContext) { }
    }
  }
}
