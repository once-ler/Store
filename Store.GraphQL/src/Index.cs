using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FastMember;
using GraphQL.Types;
using GraphQL.Utilities;
using Store.Models;
using Store.IoC;
using Store.GraphQLSupport;

namespace Store {
  public class Index {
    static void Main(string[] args) {
      Console.WriteLine("Droid\r\n=====");
      var droid = TypeAccessor.Create(typeof(Droid));
      
      // Create an anonymous type
      var gqlObj = new ObjectGraphType();
      gqlObj.Name = typeof(Droid).ToString();
      gqlObj.IsTypeOf =(value) => value is Droid;

      var members = droid.GetMembers();
      foreach(var f in members) {
        Console.WriteLine("{0} {1} {2}", f.Name, f.Type, f.Type.FromPrimitiveToGraphQLType());
        // Add to gqlObj
        var primiTy = f.Type.FromPrimitiveToGraphQLType();
        if (primiTy != null) {
          var fld = new FieldType { Name = f.Name, Type = primiTy };
          if (fld.Type == typeof(DateGraphType)) {
            fld.DefaultValue = DateTime.Now;
          }
          gqlObj.AddField(fld);
        } else {
          // If it's in the locator, we can add it
          var anotherGqlType = ServiceProvider.Instance.GetType(f.Name + "Type");
          var anotherType = ServiceProvider.Instance.GetType(f.Name);
          if (anotherGqlType == null) {
            // What about the originating type?
            if (anotherType != null) {
              // Create a GraphQL type for this type.
            }
          }
        }
      }

      // Get the definition for generated GraphQL Droid type
      var gqlObjAccess = TypeAccessor.Create(gqlObj.GetType());
      var membersGqlObj = gqlObjAccess.GetMembers();
      foreach (var f in membersGqlObj) {
        Console.WriteLine("{0} {1}", f.Name, f.Type);
      }
      
      // Register the type in the Locator
      ServiceProvider.Instance.Register(droid.GetType().Name + "Type", gqlObjAccess.GetType());
      // Get the GraphQL type from the Locator
      var gqlObjTy = ServiceProvider.Instance.GetType(droid.GetType().Name + "Type");

      Console.WriteLine("Record<Droid>\r\n=============");
      var recDroid = TypeAccessor.Create(typeof(Record<Droid>));
      var membersOfRec = recDroid.GetMembers();
      foreach (var f in membersOfRec) {
        Console.WriteLine("{0} {1} {2}", f.Name, f.Type, f.Type.FromPrimitiveToGraphQLType());
        if (f.Type.IsGenericType && f.Type.GetGenericTypeDefinition() == typeof(List<>)) {
          var tyOfT = f.Type.GetGenericArguments().FirstOrDefault();
          bool isStoreHistoryType = tyOfT.IsGenericType && tyOfT.GetGenericTypeDefinition() == typeof(History<>);
          Console.WriteLine(isStoreHistoryType);
        }          
      }
    }
  }
}
