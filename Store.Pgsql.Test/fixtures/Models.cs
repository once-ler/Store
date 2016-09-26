﻿using System;
using System.Collections.Generic;
using Store.Interfaces;
using Store.Models;

namespace Store.Pgsql.Test.fixtures {
  namespace Models {

    public class Personnel : Model {}

    internal sealed class PersonnelClient<T> : Client<T> where T : Personnel {
      public PersonnelClient(DBContext _dbContext) : base(_dbContext) { }
    }

    public class Droid : Model {
      public List<IModel> friends { get; set; }
    }

    public class Human : Model {
      public List<IModel> friends { get; set; }
    }

    public class RebelAlliance :  Affiliation<Participant> {}

    public class Empire : Affiliation<Participant> { }
  }
}