using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using FastMember;
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

    public static Type BuildObjectGraphType(this Type type) {
      var objectType = TypeAccessor.Create(type);

      // http://mironabramson.com/blog/post/2008/06/Create-you-own-new-Type-and-use-it-on-run-time-(C).aspx
      // create a dynamic assembly and module 
      AssemblyName assemblyName = new AssemblyName("tmpAssembly");
      AssemblyBuilder assemblyBuilder = Thread.GetDomain().DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
      ModuleBuilder module = assemblyBuilder.DefineDynamicModule("tmpModule");

      //TODO: need to extend from ObjectGraphType<T>
      // create a new type builder
      TypeBuilder typeBuilder = module.DefineType(type.Name + "Type", TypeAttributes.Public | TypeAttributes.Class);
      var members = objectType.GetMembers();
      foreach (var f in members) {
        var primiTy = f.Type.FromPrimitiveToGraphQLType();
        if (primiTy != null) {
          //
          // Generate a private field
          FieldBuilder field = typeBuilder.DefineField("_" + f.Name, typeof(string), FieldAttributes.Private);
          // Generate a public property
          PropertyBuilder property =
            typeBuilder.DefineProperty(
              f.Name,
              PropertyAttributes.None,
              typeof(string),
              new Type[] { primiTy }
            );

          // The property set and property get methods require a special set of attributes:
          MethodAttributes GetSetAttr = MethodAttributes.Public | MethodAttributes.HideBySig;

          // Define the "get" accessor method for current private field.
          MethodBuilder currGetPropMthdBldr =
            typeBuilder.DefineMethod(
              "get_value",
              GetSetAttr,
              typeof(string),
              Type.EmptyTypes
            );

          // Intermediate Language stuff...
          ILGenerator currGetIL = currGetPropMthdBldr.GetILGenerator();
          currGetIL.Emit(OpCodes.Ldarg_0);
          currGetIL.Emit(OpCodes.Ldfld, field);
          currGetIL.Emit(OpCodes.Ret);

          // Define the "set" accessor method for current private field.
          MethodBuilder currSetPropMthdBldr =
            typeBuilder.DefineMethod(
              "set_value",
              GetSetAttr,
              null,
              new Type[] { typeof(string) }
            );

          // Again some Intermediate Language stuff...
          ILGenerator currSetIL = currSetPropMthdBldr.GetILGenerator();
          currSetIL.Emit(OpCodes.Ldarg_0);
          currSetIL.Emit(OpCodes.Ldarg_1);
          currSetIL.Emit(OpCodes.Stfld, field);
          currSetIL.Emit(OpCodes.Ret);

          // Last, we must map the two methods created above to our PropertyBuilder to 
          // their corresponding behaviors, "get" and "set" respectively. 
          property.SetGetMethod(currGetPropMthdBldr);
          property.SetSetMethod(currSetPropMthdBldr);
          //
        }
      }
      // Generate our type
      Type generetedType = typeBuilder.CreateType();

      return generetedType;
    }

  }
}
