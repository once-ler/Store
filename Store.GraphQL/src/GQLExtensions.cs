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

    public static Type registerGQLType(this Type inTy) {
      Type genericStoreGraphQlType = typeof(Store.GraphQL.GQLType<>);

      // By creating the type, will be registered in IoC
      var anotherGqlType = genericStoreGraphQlType.MakeGenericType(inTy);
      var o_ = TypeAccessor.Create(anotherGqlType);
      var t_ = o_.CreateNew();

      // Another way of creating it:
      // System.Reflection.ConstructorInfo ci = anotherGqlType.GetConstructor(Type.EmptyTypes);
      // object o = ci.Invoke(new object[] { });

      return anotherGqlType;
    }

  }
}
