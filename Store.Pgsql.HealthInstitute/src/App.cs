using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Store.IoC;
using Store.Models;
using Store.Pgsql.HealthInstitute.Models;

namespace Store.Pgsql.HealthInstitute {
  public class App {

    public object tryCatch(Func<object> f) { try { return f(); } catch (Exception e) { Console.WriteLine(e.StackTrace); return e; } }

    public App() {
      var personnelClient = ServiceProvider.Instance.GetService<Client<Personnel>>();

      var vc = tryCatch(() => personnelClient.one<Personnel>("v$0", "id", "kid-you-not"));
    }
  }
}
