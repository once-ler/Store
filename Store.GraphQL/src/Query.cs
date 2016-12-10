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

    public override ObjectGraphType CreateGraphQLType() {
      
      // Create an anonymous type
      var gqlObj = new ObjectGraphType();
      gqlObj.Name = type.Name + "Query";

      // Get the corresponding GraphQLType from IoC
      var gqlTypeInstance = ServiceProvider.Instance.GetType(typeof(T).Name + "Type");

      // TODO: Need to define Arguments for each resolver
      Dictionary<string, GQL.Resolvers.IFieldResolver> queries = new Dictionary<string, GQL.Resolvers.IFieldResolver> {
        { "one", new OneResolver() },
        { "list", new ListResolver() },
        { "search", new SearchResolver() }
      };

      foreach(var item in queries) {
        var fld = new FieldType { Name = item.Key, Resolver = item.Value, Type = gqlTypeInstance };
        gqlObj.AddField(fld);
      } 

      return gqlObj;
    }
  }
}
