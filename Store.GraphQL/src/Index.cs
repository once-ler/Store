using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FastMember;
using GraphQL;
using GraphQL.Types;
using GraphQL.Http;
using Store.Models;
using Store.Interfaces;
using Store.IoC;
using Store.Storage;
using Store.GraphQL.Util;

using GQL = GraphQL;

namespace Store.GraphQL {
  // Mock Model  
  public class Droid : Model {
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

  // Mock Client
  public class MockClient<T> : BaseClient<T>, IStore<T> where T : class, IModel, new() {
    public MockClient(DBContext _dbContext) : base(_dbContext) { }
    public override List<Record<T>> list(string version, int offset, int limit) { throw new NotImplementedException(); }
    public override U save<U>(string version, U doc) { throw new NotImplementedException(); }
    public override List<Record<T>> search(string version, string field, string search, int offset = 0, int limit = 10) { throw new NotImplementedException(); }
    protected override string createSchema(string version) { throw new NotImplementedException(); }
    protected override string createStore(string version, string store) { throw new NotImplementedException(); }
    protected override string upsertStore(string version, Record<T> rec) { throw new NotImplementedException(); }
    protected override U getOneRecord<U>(string version, string field, string value, Type typeOfParty = null) {
      return new Droid { id = "1", name = "R2-D2" } as U;
      throw new NotImplementedException();
    }
    protected override dynamic getOneRecord(string version, string typeOfStore, string field, string value) {
      return new Droid { id = "1", name = "C3PO" };
      throw new NotImplementedException();
    }
    public override long count(string version, string field = null, string search = null) { throw new NotImplementedException(); }
  };
 
  public class Index {
    
    private static async void RunFromFile(Schema schema) {

      var t = Helper.ReadAll("gql/Introspection-query.gql");
      try {
        t.Wait();
        Console.WriteLine(t.Result);
      } catch (Exception err) {
        Console.WriteLine(err.StackTrace);
      }
      var result = await new DocumentExecuter().ExecuteAsync(_ => {
        _.Schema = schema;
        _.Query = t.Result;
      }).ConfigureAwait(false);

      var json = new DocumentWriter(indent: true).Write(result);

      json.WriteAll(@"out/introspection-query.json");
      // Console.WriteLine(json);
    }

    private static async void Run(Schema schema) {
      var result = await new DocumentExecuter().ExecuteAsync(_ =>
      {
        _.Schema = schema;
        _.Query = @"
                query {
                  one {
                    id
                    name
                    integer1
                  }
                }
              ";
      }).ConfigureAwait(false);

      var json = new DocumentWriter(indent: true).Write(result);

      Console.WriteLine(json);
    }

    static void Main(string[] args) {
      
      // Must register Stores
      var dbContext = new DBContext { server = "127.0.0.1", port = 5432, database = "pccrms", userId = "editor", password = "editor" };
      ServiceProvider.Instance.Singleton<MockClient<Droid>>(() => new MockClient<Droid>(dbContext));
      
      var q = new Query<Droid>();
      var schema = new Schema { Query = q.getGraphType() };
      // Run(schema);
      RunFromFile(schema);
    }

    public void experiment() {

      Console.WriteLine("Droid\r\n=====");
      var droid = TypeAccessor.Create(typeof(Droid));

      var o = new ObjectGraphType<Droid>();
      var p = TypeAccessor.Create(o.GetType());

      // Create an anonymous type
      var gqlObj = new ObjectGraphType();
      gqlObj.Name = typeof(Droid).ToString();
      gqlObj.IsTypeOf = (value) => value is Droid;

      var members = droid.GetMembers();
      foreach (var f in members) {
        // Console.WriteLine("{0} {1} {2}", f.Name, f.Type, f.Type.FromPrimitiveToGraphQLType());
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
            }
          }
        }
      }

      /*
      // Get the definition for generated GraphQL Droid type
      var gqlObjAccess = TypeAccessor.Create(gqlObj.GetType());
      var membersGqlObj = gqlObjAccess.GetMembers();
      foreach (var f in membersGqlObj) {
        Console.WriteLine("{0} {1}", f.Name, f.Type);
      }
      */

      ServiceProvider.Instance.Register(droid.GetType().Name + "Type", gqlObj);
      var objCreator = ServiceProvider.Instance.Get(droid.GetType().Name + "Type");
      ObjectGraphType obj = objCreator();

      // Func<ResolveFieldContext, object> deleg = context => new object();

      // Create simple query
      var query = new ObjectGraphType();
      query.Name = "DroidQuery";
      FieldType fty = new FieldType {
        Type = obj.GetType(),
        Name = "one",
        Resolver = new OneResolver(typeof(System.Object))
      };
      query.AddField(fty);

      /*
      // Register the type in the Locator
      ServiceProvider.Instance.Register(droid.GetType().Name + "Type", gqlObj.GetType());
      
      // Get the GraphQL type from the Locator
      var gqlObjTy = ServiceProvider.Instance.GetType(droid.GetType().Name + "Type");
      
      // Get the definition for generated GraphQL Droid type
      var gqlObjAccess = TypeAccessor.Create(gqlObjTy.GetType());
      var membersGqlObj = gqlObjAccess.GetType().GetMembers();
      foreach (var f in membersGqlObj) {
        Console.WriteLine("{0} {1}", f.Name, f.MemberType);
      }
      */

      Console.WriteLine("Record<Droid>\r\n=============");
      var recDroid = TypeAccessor.Create(typeof(Record<Droid>));
      var membersOfRec = recDroid.GetMembers();
      foreach (var f in membersOfRec) {
        Console.WriteLine("{0} {1} {2}", f.Name, f.Type, f.Type.FromPrimitiveToGraphQLType());
        if (f.Type.IsGenericType && f.Type.GetGenericTypeDefinition() == typeof(List<>)) {
          var tyOfT = f.Type.GetGenericArguments().FirstOrDefault();
          bool isStoreHistoryType = tyOfT.IsGenericType && tyOfT.GetGenericTypeDefinition() == typeof(History<>);
          Console.WriteLine(isStoreHistoryType);
        }
      }
    }
  } 
}
