using System;
using GraphQL.Types;
using GQL = GraphQL;
using System.Collections.Generic;
using Store.Models;
using Store.IoC;
using Store.GraphQL.Interface;
using Store.GraphQL.Resolver;
using Store.GraphQL.Resolver.Root;

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
      
      // Return if Root Query.
      if (type.Name == "Root") {
        this.createRootResolvers(gqlObj);
        return gqlObj;
      }
      
      Dictionary<string, FieldType> fieldQueries = new Dictionary<string, FieldType>() {
        { "one", new OneResolver<T>(this)},
        { "list", new ListResolver<T>(this)},
        { "search", new SearchResolver<T>(this)}
      };
      
      foreach (var item in fieldQueries) {
        gqlObj.AddField(item.Value);
      }
      
      return gqlObj;
    }

    private void createRootResolvers(ObjectGraphType gqlObj) {
      gqlObj.AddField(new OneResolver());
      gqlObj.AddField(new ListResolver());
    }
  }
}
