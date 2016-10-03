using System;
using System.Collections.Generic;
using Store.Interfaces;
using Store.Models;

namespace Store.Pgsql.Test.fixtures {
  namespace Models {

    public class Personnel : Model { }

    public class Droid : Model {
      // public List<IModel> friends { get; set; }
    }

    public class DroidExtended : Droid {
      public string language { get; set; }
    }

    public class Human : Model {
      // public List<IModel> friends { get; set; }
    }

    public class HumanExtended : Human { }

    public class RebelAlliance :  Affiliation<Participant> {}

    public class RebelAllianceExtended : RebelAlliance {}

    public class Empire : Affiliation<Participant> { }

    public class EmpireExtended : Affiliation<ParticipantExtended> { }

    public class ParticipantExtended : Participant { }
 }
}
