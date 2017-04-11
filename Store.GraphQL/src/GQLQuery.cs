using System;
using GraphQL.Types;
using GQL = GraphQL;
using System.Collections.Generic;
using Store.Models;
using Store.IoC;
using Store.GraphQL.Interface;
using Store.GraphQL.Resolver;

namespace Store.GraphQL.Query {
  public static class RootQuery {
    public static GQLQuery<Root> Get() {
      var q = new GQLQuery<Root>();
      ServiceProvider.Instance.Singleton<GQLQuery<Root>>(q);
      return ServiceProvider.Instance.GetService<GQLQuery<Root>>();
    }
  }

  public class GQLQuery<T> : GQLBase<T> where T : Model {
  
    public GQLQuery() : base() { }
    public GQLQuery(string tyName) : base(tyName) { }
    public GQLQuery(Type ty) : base(ty) { }

    public void AppendQuery(params IObjectGraphType[] queries) {
      if (this.getGraphType().Name != "RootQuery")
        return;
      foreach (var q in queries) {
        q.getGraphType().Fields.Apply(f => this.getGraphType().AddField(f));
      }
    }

    protected override ObjectGraphType createResolvers() {
      // Create an anonymous type.
      var gqlObj = new ObjectGraphType();
      gqlObj.Name = type.Name + "Query";
      gqlObj.Description = gqlObj.Name;

      // Get the corresponding GraphQLType from IoC
      var gqlType = ServiceProvider.Instance.GetType(typeof(T).Name + "Type");

      // Return if Root Query.
      if (type.Name == "Root") {
        createRootResolvers(gqlObj);
        return gqlObj;
      }

      /*
      GQL.Resolvers.FuncFieldResolver<object, object> testfunc = new GQL.Resolvers.FuncFieldResolver<object, object>(
        context => {
          return store.one("version", type.Name, "id", "abc");
        }
      );

      Dictionary<string, GQL.Resolvers.FuncFieldResolver<object, object>> queries = new Dictionary<string, GQL.Resolvers.FuncFieldResolver<object, object>> {
        { "one" + type.Name, testfunc },
        { "list" + type.Name, testfunc },
        { "search" + type.Name, testfunc }
      };

      foreach (var item in queries) {
        var fld = new FieldType { Name = item.Key, Resolver = testfunc, Type = gqlType };
        gqlObj.AddField(fld);
      }
      */
      
      Dictionary<string, FieldType> fieldQueries = new Dictionary<string, FieldType>() {
        { "one", new OneResolver<T>(this)},
        { "list", new ListResolver<T>(this)},
        { "search", new SearchResolver<T>(this)}
      };

      // Get Resolvers.
      // var listResolver = new ListResolver<T>(this);
      
      foreach (var item in fieldQueries) {
        gqlObj.AddField(item.Value);
      }
      
      return gqlObj;
    }

    protected void createRootResolvers(ObjectGraphType q) {
      var oneResolver = new FieldType {
        Name = "one",
        Arguments = new QueryArguments(
            new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "version", Description = "VersionControl" },
            new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "type", Description = "The Store model type" },
            new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "field", Description = "The key field to search" },
            new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "value", Description = "The value for the key field to search." }
          ),
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
          )
      };

      q.AddField(oneResolver);
    }
  }
}
