using System;
using System.Collections.Generic;
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
  public class GQLType<T> : ObjectGraphType<T> {
    public GQLType() {
      Name = getGraphQlTypeName();
      IsTypeOf = value => value is T;
      createResolvers();
    }

    public Type getBaseType() {
      return typeof(T);
    }

    public string getGraphQlTypeName() {
      return typeof(T).Name + "Type";
    }

    public void createResolvers() {
      var fields = createFieldResolvers();
      foreach (var fld in fields) this.AddField(fld);
      ServiceProvider.Instance.Register(getGraphQlTypeName(), this.GetType());
    }
    
    private List<FieldType> createFieldResolvers() {
      List<FieldType> fields = new List<FieldType>();

      var objectType = TypeAccessor.Create(typeof(T));
      // var objectType = TypeAccessor.Create(gqlType);
      var members = objectType.GetMembers();

      foreach (var f in members) {

        FieldType fld = new FieldType {
          Name = f.Name,
          Description = f.Name,
          Resolver = new GQL.Resolvers.ExpressionFieldResolver<object, object>(
            context => context.GetProperyValue(f.Name)
          )
        };

        // Is this a primitive GraphQL type?
        var primiTy = f.Type.FromPrimitiveToGraphQLType();
        if (primiTy != null) {
          fld.Type = primiTy;
          // Is this default necessary?
          if (fld.Type == typeof(DateGraphType)) {
            fld.DefaultValue = DateTime.Now;
          }
        } else {
          Type objectGraphType = f.Type;

          // Is this a generic collection?
          if (f.Type.IsGenericType) {
            objectGraphType = f.Type.GenericTypeArguments[0];
          }

          // Is this GraphQL type already registered in IoC?
          var nextGraphQLType = ServiceProvider.Instance.GetType(objectGraphType.Name + "Type");

          if (nextGraphQLType == null) {
            // After creating GQLType, IoC is updated and this type should be there now.
            nextGraphQLType = objectGraphType.registerGQLType();
          }

          // For now, only support List<>
          if (f.Type.IsGenericType && f.Type.GetGenericTypeDefinition() == typeof(List<>)) {
            var a = new ListGraphType(nextGraphQLType as IGraphType);
            nextGraphQLType = typeof(ListGraphType<>).MakeGenericType(nextGraphQLType);
          }

          fld.Type = nextGraphQLType;
        }

        fields.Add(fld);

      }

      return fields;
    }
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

  /*  Not Implemented Yet */
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
