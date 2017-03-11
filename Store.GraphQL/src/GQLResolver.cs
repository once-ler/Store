using System;
using System.Collections.Generic;
using Store.Models;
using Store.IoC;
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

  class ListResolverOrig : GQL.Resolvers.IFieldResolver {
    public object Resolve(ResolveFieldContext context) {
      return new List<object>();
    }
  };

  class ListResolver<T>: FieldType where T: Model {
    public ListResolver(GQLQuery<T> query) {
      var gqlType = ServiceProvider.Instance.GetType(typeof(T).Name + "Type");

      Name = "list";
      Type = gqlType;
      Resolver = new GQL.Resolvers.FuncFieldResolver<object, object>(
        context => {
          // return query.store.list("version", query.type.Name, "id", "abc");
          return null;
        }
      );
    }    
  }

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
