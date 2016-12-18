using System;
using System.Collections.Generic;
using GraphQL.Types;
using GQL = GraphQL;

// Will probably remove these classes.
namespace Store.GraphQL {
  class OneResolver : GQL.Resolvers.IFieldResolver{
    public OneResolver(Type ty) {
      ty_ = ty;
    }
    public object Resolve(ResolveFieldContext context) {
      return System.Activator.CreateInstance(ty_);
      // return new ty_();
    }
    private Type ty_;
  };

  class ListResolver : GQL.Resolvers.IFieldResolver {
    public object Resolve(ResolveFieldContext context) {
      return new List<object>();
    }
  };

  class SearchResolver : GQL.Resolvers.IFieldResolver {
    public object Resolve(ResolveFieldContext context) {
      return new List<object>();
    }
  };

  class SaveResolver : GQL.Resolvers.IFieldResolver {
    public object Resolve(ResolveFieldContext context) {
      return new object();
    }
  };

  class AssociateResolver : GQL.Resolvers.IFieldResolver {
    public object Resolve(ResolveFieldContext context) {
      return new object();
    }
  };

  class DisassociateResolver : GQL.Resolvers.IFieldResolver {
    public object Resolve(ResolveFieldContext context) {
      return new object();
    }
  };
}
