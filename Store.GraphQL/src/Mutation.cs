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

    protected override ObjectGraphType createResolvers() {
      throw new NotImplementedException();
    }
  }
}
