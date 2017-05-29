using System;
using Store.IoC;
using GraphQL.Types;
using GQL = GraphQL;

namespace Store.GraphQL.Resolver.Root {
  class OneResolver : FieldType  {
    Type gqlType = ServiceProvider.Instance.GetType("RootType");

    public OneResolver() {
      Name = "one";
      Type = gqlType;
      Arguments = new QueryArguments(
          new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "version", Description = "VersionControl" },
          new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "type", Description = "The Store model type" },
          new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "field", Description = "The key field to search" },
          new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "value", Description = "The value for the key field to search." }
        );
      Resolver = new GQL.Resolvers.FuncFieldResolver<object, object>(
          context => {
            string ty = context.GetArgument<string>("type").Replace("Type", "");
            dynamic store = ServiceProvider.Instance.GetStore(ty);

            return store.one(
              context.GetArgument<string>("version"),
              ty,
              context.GetArgument<string>("field"),
              context.GetArgument<string>("value")
            );
          }
        );
    }
  }

  class ListResolver : FieldType {
    Type gqlType = ServiceProvider.Instance.GetType("RootType");

    public ListResolver() {
      Name = "list";
      Type = gqlType;
      Arguments = new QueryArguments(
        new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "version", Description = "VersionControl of " + gqlType.Name },
        new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "type", Description = "The Store model type" },
        new QueryArgument<NonNullGraphType<IntGraphType>> { Name = "offset", Description = "The index number to start the list." },
        new QueryArgument<NonNullGraphType<IntGraphType>> { Name = "limit", Description = "The total number of records to list." }
      );
      Resolver = new GQL.Resolvers.FuncFieldResolver<object, object>(
        context => {
          string ty = context.GetArgument<string>("type").Replace("Type", "");
          dynamic store = ServiceProvider.Instance.GetStore(ty);

          return store.list(
            context.GetArgument<string>("version"),
            ty,
            context.GetArgument<int>("offset"),
            context.GetArgument<int>("limit")
          );
        }
      );
    }
  }

  class SearchResolver : FieldType {
    Type gqlType = ServiceProvider.Instance.GetType("RootType");

    public SearchResolver() {
      Name = "search";
      Type = gqlType;
      Arguments = new QueryArguments(
        new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "version", Description = "VersionControl of " + gqlType.Name },
        new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "type", Description = "The Store model type" },
        new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "field", Description = "The key field to search.  If nested, use dot notation." },
        new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "search", Description = "The value for the key field to search." },
        new QueryArgument<NonNullGraphType<IntGraphType>> { Name = "offset", Description = "The index number to start the list." },
        new QueryArgument<NonNullGraphType<IntGraphType>> { Name = "limit", Description = "The total number of records to list." }
      );
      Resolver = new GQL.Resolvers.FuncFieldResolver<object, object>(
        context => {
          string ty = context.GetArgument<string>("type").Replace("Type", "");
          dynamic store = ServiceProvider.Instance.GetStore(ty);

          return store.search(
            context.GetArgument<string>("version"),
            ty,
            context.GetArgument<string>("field"),
            context.GetArgument<string>("search"),
            context.GetArgument<int>("offset"),
            context.GetArgument<int>("limit")
          );
        }
      );
    }
  }

}
