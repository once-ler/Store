using System;
using System.Collections.Generic;
using GQL = GraphQL;

namespace Store.GraphQL {
  public static class Extensions {
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
  }
}
