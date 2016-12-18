using System;
using System.Collections.Generic;
using GraphQL;
using GraphQL.Types;
using FastMember;
using Store.IoC;
using GQL = GraphQL;

namespace Store.GraphQL {
  public static class GQLExtensions {
    private static Dictionary<Type, Type> primitiveToGraphQLDict = new Dictionary<Type, Type>{
      { typeof(string), typeof(GQL.Types.StringGraphType) },
      { typeof(bool), typeof(GQL.Types.BooleanGraphType) },
      { typeof(DateTime), typeof(GQL.Types.DateGraphType) },
      { typeof(byte), typeof(GQL.Types.IntGraphType) },
      { typeof(short), typeof(GQL.Types.IntGraphType) },
      { typeof(int), typeof(GQL.Types.IntGraphType) },
      { typeof(long), typeof(GQL.Types.IntGraphType) },
      { typeof(Single), typeof(GQL.Types.FloatGraphType) },
      { typeof(double), typeof(GQL.Types.FloatGraphType) },
      { typeof(decimal), typeof(GQL.Types.DecimalGraphType) }
    };

    public static Type FromPrimitiveToGraphQLType(this Type inTy) {
      Type outTy = null;
      primitiveToGraphQLDict.TryGetValue(inTy, out outTy);
      return outTy;
    }

    public static List<FieldType> createFieldResolvers(this Type gqlType) {
      List<FieldType> fields = new List<FieldType>();

      // var objectType = TypeAccessor.Create(typeof(T));
      var objectType = TypeAccessor.Create(gqlType);
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

          string nextObjectGraphTypeName = objectGraphType.Name + "Type";

          var nextGraphQLType = ServiceProvider.Instance.GetType(nextObjectGraphTypeName);

          if (nextGraphQLType == null) {
            var nextGraphQLTypeInstance = new GQLType(objectGraphType);

            // Old way
            // Type genericStoreGraphQlType = typeof(Store.GraphQL.GQLType<>);

            // By creating the type, will be registered in IoC
            // var anotherGqlType = genericStoreGraphQlType.MakeGenericType(objectGraphType);
            // var o_ = TypeAccessor.Create(anotherGqlType);
            // var t_ = o_.CreateNew();

            // Another way of creating it:
            // System.Reflection.ConstructorInfo ci = anotherGqlType.GetConstructor(Type.EmptyTypes);
            // object o = ci.Invoke(new object[] { });

            // Should be there now.
            // TODO: Name is Type`1, not intended.
            nextGraphQLType = ServiceProvider.Instance.GetType(nextObjectGraphTypeName);
          }

          // For now, only support List<>
          if (f.Type.GetGenericTypeDefinition() == typeof(List<>) && f.Type.IsGenericType) {

            // var a = new ListGraphType(nextGraphQLType as IGraphType);
            // nextGraphQLType = typeof(ListGraphType<>).MakeGenericType(nextGraphQLType);
            // nextGraphQLType = typeof(GQL.Types.ListGraphType);
            // (nextGraphQLType as GQL.Types.ListGraphType).ResolvedType = nextGraphQLType;
          }

          fld = new FieldType {
            Name = f.Name,
            Type = nextGraphQLType,
            Resolver = new GQL.Resolvers.ExpressionFieldResolver<object, object>(
                context => context.GetProperyValue(f.Name)
              )
          };
        }

        // (gqlType as IComplexGraphType).AddField(fld);
        fields.Add(fld);

      }

      // ServiceProvider.Instance.Register(baseType.Name + "Type", gqlType.GetType());
      return fields;
    }
  }
}
