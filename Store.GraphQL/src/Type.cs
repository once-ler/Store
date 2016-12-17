using System;
using GraphQL;
using GraphQL.Types;
using FastMember;
using Store.IoC;

using GQL = GraphQL;

namespace Store.GraphQL {

  /// <summary>
  /// Assumption is that all required dependent types have been registered into the ServiceProvider.Instance IoC
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class Type<T> : ObjectGraphType<T> {
    public Type() {
      this.Name = getGraphQlTypeName();
      createResolvers();
    }

    public System.Type getBaseType() {
      return typeof(T);
    }

    public string getGraphQlTypeName() {
      return typeof(T).Name + "Type";
    }

    public void createResolvers() {
      var objectType = TypeAccessor.Create(typeof(T));
      var members = objectType.GetMembers();

      foreach (var f in members) {
        // Add to gqlObj
        var primiTy = f.Type.FromPrimitiveToGraphQLType();
        if (primiTy != null) {
          var fld = new FieldType {
            Name = f.Name,
            Type = primiTy,
            Resolver = new GQL.Resolvers.ExpressionFieldResolver<object, object>(
                context => context.GetProperyValue(f.Name)
              )
          };
          if (fld.Type == typeof(DateGraphType)) {
            fld.DefaultValue = DateTime.Now;
          }
          this.AddField(fld);
        } else {
          // WIP
          // If it's in the locator, we can add it
          var anotherGqlType = ServiceProvider.Instance.GetType(f.Name + "Type");
          var anotherType = ServiceProvider.Instance.GetType(f.Name);
          if (anotherGqlType == null) {
            // What about the originating type?
            if (anotherType != null) {
              //Type generic = typeof<Store.GraphQL.Type<>>;
              //Type specific = generic.MakeGenericType(typeof(int));
              //ConstructorInfo ci = specific.GetConstructor(Type.EmptyTypes);
              //object o = ci.Invoke(new object[] { });

              // Create a GraphQL type for this type.
              // var aType = new Type<Model>(anotherType);
              // var aGqlType = aType.CreateGraphQLType();
              // What about the resolver?
              // var fld = new FieldType { Name = f.Name, Type = aGqlType.GetType() };
              // this.AddField(fld);
            }
          }
        }
      }
      
      ServiceProvider.Instance.Register(getGraphQlTypeName(), this.GetType());
      
    }

    private string graphQlType;

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
