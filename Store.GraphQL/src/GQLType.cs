using System;
using System.Collections.Generic;
using GraphQL;
using GraphQL.Types;
using FastMember;
using Store.IoC;

using GQL = GraphQL;

namespace Store.GraphQL {
  
  public interface IGQLType {
    Type getBaseType();
    string getGraphQlTypeName();
    void createResolvers();
  }
  
  public class GQLType : ObjectGraphType, IGQLType {
    
    public GQLType(Type ty) {
      ty_ = ty;
      this.Name = getGraphQlTypeName();
      this.createResolvers();
    }

    public Type getBaseType() {
      return ty_;
    }

    public string getGraphQlTypeName() {
      return ty_.Name + "Type";
    }

    public void createResolvers() {
      var fields = ty_.createFieldResolvers();
      foreach (var fld in fields) this.AddField(fld);
      ServiceProvider.Instance.Register(getGraphQlTypeName(), this.GetType());
    }

    private Type ty_;
  }

  /// <summary>
  /// Assumption is that all required dependent types have been registered into the ServiceProvider.Instance IoC
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class GQLType<T> : ObjectGraphType<T>, IGQLType {
    public GQLType() {
      this.Name = getGraphQlTypeName();      
      this.createResolvers();
    }

    public Type getBaseType() {
      return typeof(T);
    }

    public string getGraphQlTypeName() {
      return typeof(T).Name + "Type";
    }

    public void createResolvers() {
      var fields = (typeof(T)).createFieldResolvers();
      foreach (var fld in fields) this.AddField(fld);
      ServiceProvider.Instance.Register(getGraphQlTypeName(), this.GetType());
    }

    public void createResolversDeprecate() {
      var objectType = TypeAccessor.Create(typeof(T));
      var members = objectType.GetMembers();

      foreach (var f in members) {

        FieldType fld = null;
        // Add to gqlObj
        var primiTy = f.Type.FromPrimitiveToGraphQLType();
        if (primiTy != null) {
          fld = new FieldType {
            Name = f.Name,
            Type = primiTy,
            Resolver = new GQL.Resolvers.ExpressionFieldResolver<object, object>(
                context => context.GetProperyValue(f.Name)
              )
          };
          if (fld.Type == typeof(DateGraphType)) {
            fld.DefaultValue = DateTime.Now;
          }          
        } else {
          Type objectGraphType = f.Type;

          // Is this a generic collection?
          if (f.Type.IsGenericType) {
            objectGraphType = f.Type.GenericTypeArguments[0];
          }

          string objectGraphTypeName = objectGraphType.Name + "Type";

          var nextGraphQLType = ServiceProvider.Instance.GetType(objectGraphTypeName);

          if (nextGraphQLType == null) {
            Type genericStoreGraphQlType = typeof(Store.GraphQL.GQLType<>);
            
            // By creating the type, will be registered in IoC
            var anotherGqlType = genericStoreGraphQlType.MakeGenericType(objectGraphType);
            
            var o_ = TypeAccessor.Create(anotherGqlType);
            var t_ = o_.CreateNew();

            // Another way of creating it:
            // System.Reflection.ConstructorInfo ci = anotherGqlType.GetConstructor(Type.EmptyTypes);
            // object o = ci.Invoke(new object[] { });

            // Should be there now.
            // TODO: Name is Type`1, not intended.
            nextGraphQLType = ServiceProvider.Instance.GetType(objectGraphTypeName);
          }

          // For now, only support List<>
          if (f.Type.GetGenericTypeDefinition() == typeof(List<>) && f.Type.IsGenericType) {

            // var a = new ListGraphType(nextGraphQLType as IGraphType);
            nextGraphQLType = typeof(ListGraphType<>).MakeGenericType(nextGraphQLType);
          }

          fld = new FieldType {
            Name = f.Name,
            Type = nextGraphQLType,
            Resolver = new GQL.Resolvers.ExpressionFieldResolver<object, object>(
                context => context.GetProperyValue(f.Name)
              )
          };
        }

        this.AddField(fld);

      }

      ServiceProvider.Instance.Register(getGraphQlTypeName(), this.GetType());      
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
