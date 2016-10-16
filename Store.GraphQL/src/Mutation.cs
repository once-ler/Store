using System;
using GraphQL.Types;
using FastMember;
using Store.Models;
using Store.IoC;

namespace Store.GraphQL {
  public class Mutation<T> : Base<T> where T : Model {

    public Mutation() : base() { }
    public Mutation(string tyName) : base(tyName) { }
    public Mutation(Type ty) : base(ty) { }

    public override ObjectGraphType CreateGraphQLType() {
      return null;
    }
  }
}
