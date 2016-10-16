using System;
using GraphQL.Types;
using FastMember;
using Store.Interfaces;
using Store.Models;
using Store.IoC;

namespace Store.GraphQL {

  /// <summary>
  /// Assumption is that all required dependent types have been registered into the ServiceProvider.Instance IoC
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class Type<T> where T : Model {
    public Type() {
      string tyName = typeof(T).Name;
      dynamic _store = ServiceProvider.Instance.GetStore(tyName);
      store = _store as IStore<T>;
    }

    public Type(string tyName) {
      dynamic _store = ServiceProvider.Instance.GetStore(tyName);
      store = _store as IStore<T>;
    }

    public Type(Type ty) {
      string tyName = ty.GetType().Name;
      dynamic _store = ServiceProvider.Instance.GetStore(tyName);
      store = _store as IStore<T>;
    }

    public Type(IStore<T> _store) {
      store = _store;
    }

    public ObjectGraphType CreateGraphQLType() {
      var objectType = TypeAccessor.Create(typeof(T));

      var o = new ObjectGraphType<T>();
      var p = TypeAccessor.Create(o.GetType());

      // Create an anonymous type
      var gqlObj = new ObjectGraphType();
      gqlObj.Name = typeof(T).ToString();
      gqlObj.IsTypeOf = (value) => value is T;

      var members = objectType.GetMembers();
      foreach (var f in members) {
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
              var aType = new Type<Model>(anotherType);
              var aGqlType = aType.CreateGraphQLType();
              var fld = new FieldType { Name = f.Name, Type = aGqlType.GetType() };
              gqlObj.AddField(fld);
            }
          }
        }
      }
      return gqlObj;
    }

    private IStore<T> store;
  }

  /* GraphQL Scalars */
  /*
  const GraphQLInt: GraphQLType = 'GraphQLInt';
  const GraphQLFloat: GraphQLType = 'GraphQLFloat';
  const GraphQLString: GraphQLType = 'GraphQLString';
  const GraphQLBoolean: GraphQLType = 'GraphQLBoolean';
  const GraphQLID: GraphQLType = 'GraphQLID';
  */

  /* Wrapping types */
  /*
  GraphQLNonNull<Type: GraphQLType>
  GraphQLList<Type: GraphQLType>
  */

  /*
  const NativeStringType: NativeType = 'string';
  const NativeArrayType: NativeType = 'array';
  const NativeObjectType: NativeType = 'object';
  const NativeNullType: NativeType = 'null';
  const NativeNumberType: NativeType = 'number';
  const NativeBooleanType: NativeType = 'boolean';
  const NativeUndefinedType: NativeType = 'undefined';
  */

  /*  Not Implemented */
  /* 
  GraphQLEnumType
  GraphQLInterfaceType
  GraphQLUnionType
  GraphQLInputObjectType   
  */

  /*
  public class StarWarsQuery : ObjectGraphType {
    public StarWarsQuery() {
      Name = "Query";
      Field<DroidType>(
        "hero",
        resolve: context => new Droid { id = "1", name = "R2-D2" }
      );
    }
  }
  */
  /*
  public class DroidType : ObjectGraphType {
    public DroidType() {
      Name = "Droid";
      Field<NonNullGraphType<StringGraphType>>("id", "The id of the droid.");
      Field<StringGraphType>("name", "The name of the droid.");
      IsTypeOf = value => value is Droid;
    }
  }
  */
}
