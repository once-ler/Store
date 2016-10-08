using System;
using System.Collections.Generic;
using System.Linq;
using Store.Models;
using Store.Storage.Pgsql.Test.fixtures.Models;

namespace Store.Storage.Pgsql.Test.fixtures {
  namespace Util {

    public class DataGenerator {

      public DataGenerator() {
        droids = new List<Droid> {
          new Droid { id = "r2d2", name= "R2-D2", ts = DateTime.Now },
          new Droid { id = "c3po", name = "C3PO", ts = DateTime.Now },
          new Droid { id = "probe", name = "Imperial Probe", ts = DateTime.Now }
        };

        humans = new List<Human> {
          new Human { id = "luke", name = "Luke Skywalker", ts = DateTime.Now },
          new Human { id = "han", name = "Han Solo", ts = DateTime.Now },
          new Human { id = "vader", name = "Darth Vader", ts = DateTime.Now },
          new Human { id = "sidious", name = "Darth Sidious", ts = DateTime.Now }
        };

        var goodRoster = new List<Participant>{
          new Participant { id= "r2d2", party = droids.FirstOrDefault(d => d.id == "r2d2") },
          new Participant { id= "c3po", party = droids.FirstOrDefault(d => d.id == "c3po") },
          new Participant { id= "luke", party = humans.FirstOrDefault(d => d.id == "luke") },
          new Participant { id= "han", party = humans.FirstOrDefault(d => d.id == "han") }
        };

        var badRoster = new List<Participant>{
          new Participant { id= "probe", party = droids.FirstOrDefault(d => d.id == "probe") },
          new Participant { id= "vader", party = humans.FirstOrDefault(d => d.id == "vader") },
          new Participant { id= "sidious", party = humans.FirstOrDefault(d => d.id == "sidious") }
        };

        rebels = new RebelAlliance { id = "rebels", roster = goodRoster };
        empire = new Empire { id = "empire", roster = badRoster };
      }

      public RebelAlliance rebels { get; set; }
      public Empire empire { get; set; }
      public List<Droid> droids { get; set; }
      public List<Human> humans { get; set; }
    }
  }
}
