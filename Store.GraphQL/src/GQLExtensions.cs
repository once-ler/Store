using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL;
using GraphQL.Types;
using GraphQL.Builders;
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

    public static void Apply<T>(this IEnumerable<T> items, Action<T> action) {
      foreach(var item in items) action(item);
    }

    /// <summary>
    /// https://github.com/graphql-dotnet/graphql-dotnet/blob/master/docs/learn.md#permission-extension-methods
    /// Usage: Field(x => x.Name).AddPermission("Some permission");
    /// </summary>
    public static readonly string PermissionsKey = "Permissions";

    public static bool RequiresPermissions(this IProvideMetadata type) {
      var permissions = type.GetMetadata<IEnumerable<string>>(PermissionsKey, new List<string>());
      return permissions.Any();
    }

    public static bool CanAccess(this IProvideMetadata type, IEnumerable<string> claims) {
      var permissions = type.GetMetadata<IEnumerable<string>>(PermissionsKey, new List<string>());
      return permissions.All(x => claims?.Contains(x) ?? false);
    }

    public static bool HasPermission(this IProvideMetadata type, string permission) {
      var permissions = type.GetMetadata<IEnumerable<string>>(PermissionsKey, new List<string>());
      return permissions.Any(x => string.Equals(x, permission));
    }

    public static void AddPermission(this IProvideMetadata type, string permission) {
      var permissions = type.GetMetadata<List<string>>(PermissionsKey);

      if (permissions == null) {
        permissions = new List<string>();
        type.Metadata[PermissionsKey] = permissions;
      }

      permissions.Fill(permission);
    }

    public static FieldBuilder<TSourceType, TReturnType> AddPermission<TSourceType, TReturnType>(
        this FieldBuilder<TSourceType, TReturnType> builder, string permission) {
      builder.FieldType.AddPermission(permission);
      return builder;
    }
  }
}
