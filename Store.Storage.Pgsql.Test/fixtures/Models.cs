using System;
using System.Collections.Generic;
using Store.Interfaces;
using Store.Models;

namespace Store.Storage.Pgsql.Test.fixtures {
  namespace Models {

    public class Personnel : Model { }

    public class Droid : Model {
      public Droid() {
        alliance = new List<Affiliation<Participant>>();
      }
      public List<Affiliation<Participant>> alliance { get; set; }
    }

    public class DroidExtended : Droid {
      public string language { get; set; }
    }

    public class Human : Model {
      public Human() {
        alliance = new List<Affiliation<Participant>>();
      }
      public List<Affiliation<Participant>> alliance { get; set; }
    }

    public class HumanExtended : Human { }

    public class RebelAlliance :  Affiliation<Participant> {}

    public class RebelAllianceExtended : RebelAlliance {}

    public class Empire : Affiliation<Participant> { }

    public class EmpireExtended : Affiliation<ParticipantExtended> { }

    public class FirstOrder : EmpireExtended { } 

    public class ParticipantExtended : Participant { }
 }
}
