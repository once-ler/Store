using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using GraphQL;
using GraphQL.Types;
using GraphQL.Language.AST;
using Store.Interfaces;
using Store.Models;
using Store.IoC;

namespace Store.GraphQLSupport {

  public static class Extensions {
    private static Dictionary<Type, Type> primitiveToGraphQLDict = new Dictionary<Type, Type>{
      { typeof(string), typeof(GraphQL.Types.StringGraphType) },
      { typeof(bool), typeof(GraphQL.Types.BooleanGraphType) },
      { typeof(DateTime), typeof(GraphQL.Types.DateGraphType) },
      { typeof(byte), typeof(GraphQL.Types.IntGraphType) },
      { typeof(short), typeof(GraphQL.Types.IntGraphType) },
      { typeof(int), typeof(GraphQL.Types.IntGraphType) },
      { typeof(long), typeof(GraphQL.Types.IntGraphType) },
      { typeof(Single), typeof(GraphQL.Types.FloatGraphType) },
      { typeof(double), typeof(GraphQL.Types.FloatGraphType) },
      { typeof(decimal), typeof(GraphQL.Types.DecimalGraphType) }
    };

    public static Type FromPrimitiveToGraphQLType(this Type inTy) {
      Type outTy = null;
      primitiveToGraphQLDict.TryGetValue(inTy, out outTy);
      return outTy;
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
  
  // From graphql-dotnet docs
  public class Droid {
    public string id { get; set; }
    public string name { get; set; }
    public DateTime ts { get; set; }
    public bool boolean { get; set; }
    public byte integer1 { get; set; }
    public short integer16 { get; set; }
    public int integer { get; set; }
    public long integer64 { get; set; }
    public Single float4 { get; set; }
    public double float8 { get; set; }
    public decimal decimal16 { get; set; }    
  }

  public class StarWarsQuery : ObjectGraphType {
    public StarWarsQuery() {
      Name = "Query";
      Field<DroidType>(
        "hero",
        resolve: context => new Droid { id = "1", name = "R2-D2" }
      );
    }
  }

  public class DroidType : ObjectGraphType {
    public DroidType() {
      Name = "Droid";
      Field<NonNullGraphType<StringGraphType>>("id", "The id of the droid.");
      Field<StringGraphType>("name", "The name of the droid.");
      IsTypeOf = value => value is Droid;
    }
  }

  /// <summary>
  /// Assumption is that all required dependent types have been registered into the ServiceProvider.Instance IoC
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class Type<T> where T : Model {

    public Type(IStore<T> _store) {
      store = _store;
    }

    public ObjectGraphType CreateGraphQLType() {

      dynamic partyStore = ServiceProvider.Instance.GetStore("TypeName");
      Type ty = ServiceProvider.Instance.GetType("");
      // var party = partyStore.one<Record<M>>(version, "id", partyId);

      return null;
    }

    private IStore<T> store;
  }
}
