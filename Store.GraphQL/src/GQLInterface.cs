using GraphQL.Types;
using Store.Models;
using Store.IoC;

namespace Store.GraphQL.Interface {
  public class ModelType : InterfaceGraphType<Model> {
    public ModelType() {
      Name = "ModelType";
      Field(d => d.id).Description("The id of the Store.Model.");
      Field(d => d.name, nullable: true).Description("The name of the Store.Model.");
      Field(d => d.ts, nullable: true).Description("The modification date of the Store.Model.");
    }
  }
}
