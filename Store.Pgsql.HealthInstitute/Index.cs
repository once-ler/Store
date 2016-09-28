using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Store.Pgsql.HealthInstitute;

namespace Store.Pgsql.HealthInstitute {
  class Index {
    static void Run(Action runner) { runner(); }

    static void Main(string[] args) {
      Run(() => new Bootstrapper(() => new App()));
    }
  }
}
