using System.Collections.Generic;
using Store.Models;
using Store.IoC;
using Store.GraphQL.Query;
using GraphQL.Types;
using GQL = GraphQL;

namespace Store.GraphQL.Resolver {
  class OneResolver<T> : FieldType where T : Model {
    public OneResolver(GQLQuery<T> query) {
      var gqlType = ServiceProvider.Instance.GetType(typeof(T).Name + "Type");

      Name = "one" + typeof(T).Name;
      Type = gqlType;
      Arguments = new QueryArguments(
        new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "version", Description = "VersionControl of " + gqlType.Name },
        new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "field", Description = "The key field to search" },
        new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "value", Description = "The value for the key field to search." }
      );
      Resolver = new GQL.Resolvers.FuncFieldResolver<object, object>(
        context => {
          return query.store.one(
            context.GetArgument<string>("version"),
            typeof(T).Name,
            context.GetArgument<string>("field"),
            context.GetArgument<string>("value")
          );
        }
      );
    }
  }
  
  class ListResolver<T>: FieldType where T: Model {
    public ListResolver(GQLQuery<T> query) {
      var gqlType = ServiceProvider.Instance.GetType(typeof(T).Name + "Type");

      Name = "list" + typeof(T).Name;
      Type = gqlType;
      Arguments = new QueryArguments(
        new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "version", Description = "VersionControl of " + gqlType.Name },
        new QueryArgument<NonNullGraphType<IntGraphType>> { Name = "offset", Description = "The index number to start the list." },
        new QueryArgument<NonNullGraphType<IntGraphType>> { Name = "limit", Description = "The total number of records to list." }
      );
      Resolver = new GQL.Resolvers.FuncFieldResolver<object, object>(
        context => {
          return query.store.list(
            context.GetArgument<string>("version"),
            context.GetArgument<int>("offset"),
            context.GetArgument<int>("limit")
          );
        }
      );
    }    
  }

  class SearchResolver<T> : FieldType where T : Model {
    public SearchResolver(GQLQuery<T> query) {
      var gqlType = ServiceProvider.Instance.GetType(typeof(T).Name + "Type");

      Name = "search" + typeof(T).Name;
      Type = gqlType;
      Arguments = new QueryArguments(
        new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "version", Description = "VersionControl of " + gqlType.Name },
        new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "field", Description = "The key field to search" },
        new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "search", Description = "The value for the key field to search." }
      );
      Resolver = new GQL.Resolvers.FuncFieldResolver<object, object>(
        context => {
          return query.store.search(
            context.GetArgument<string>("version"),
            context.GetArgument<string>("field"),
            context.GetArgument<string>("search")
          );
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
