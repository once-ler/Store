using System;
using GraphQL.Types;
using GQL = GraphQL;
using System.Collections.Generic;
using FastMember;
using Store.Models;
using Store.IoC;

namespace Store.GraphQL {
  public class Query<T> : Base<T> where T : Model {
  
    public Query() : base() { }
    public Query(string tyName) : base(tyName) { }
    public Query(Type ty) : base(ty) { }

    protected override ObjectGraphType createResolvers() {
      // Create an anonymous type.
      var gqlObj = new ObjectGraphType();
      gqlObj.Name = type.Name + "Query";

      // Get the corresponding GraphQLType from IoC
      var gqlType = ServiceProvider.Instance.GetType(typeof(T).Name + "Type");

      GQL.Resolvers.FuncFieldResolver<object, object> testfunc = new GQL.Resolvers.FuncFieldResolver<object, object>(
        context => {
          return store.one("version", type.Name, "id", "abc");
        }
      );
      // TODO: Need to define Arguments for each resolver.
      Dictionary<string, GQL.Resolvers.FuncFieldResolver<object, object>> queries = new Dictionary<string, GQL.Resolvers.FuncFieldResolver<object, object>> {
        { "one", testfunc },
        { "list", testfunc },
        { "search", testfunc }
      };
      
      foreach(var item in queries) {
        var fld = new FieldType { Name = item.Key, Resolver = testfunc, Type = gqlType };
        gqlObj.AddField(fld);
      } 

      return gqlObj;
    }
  }
}
