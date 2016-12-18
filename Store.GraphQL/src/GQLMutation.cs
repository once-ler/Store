using System;
using GraphQL.Types;
using FastMember;
using Store.Models;
using Store.IoC;

namespace Store.GraphQL {
  public class GQLMutation<T> : GQLBase<T> where T : Model {

    public GQLMutation() : base() { }
    public GQLMutation(string tyName) : base(tyName) { }
    public GQLMutation(Type ty) : base(ty) { }

    protected override ObjectGraphType createResolvers() {
      throw new NotImplementedException();
    }
  }
}
