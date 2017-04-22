using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Validation;
using GraphQL.Language.AST;

using GQL = GraphQL;

namespace Store.GraphQL.Authorization {

  public class UserContext {
    public Func<bool> IsAuthenticated { get; set; }
    public IEnumerable<string> Claims { get; set; }    
  }

  public class GraphQLUserContext {
    public UserContext User { get; set; }
    public string AuthToken { get; set; }
  }

  /// <summary>
  /// Reference: https://github.com/graphql-dotnet/graphql-dotnet/blob/master/docs/learn.md#authentication--authorization
  /// </summary>
  public class RequiresAuthValidationRule : IValidationRule {
    public INodeVisitor Validate(ValidationContext context) {
      var userContext = context.UserContext.As<GraphQLUserContext>();
      var authenticated = userContext.User?.IsAuthenticated() ?? false;

      return new EnterLeaveListener(_ => {
        _.Match<Operation>(op => {
          if (op.OperationType == OperationType.Mutation && !authenticated) {
            context.ReportError(new ValidationError(
                context.OriginalQuery,
                "auth-required",
                $"Authorization is required to access {op.Name}.",
                op));
          }
        });

        // this could leak info about hidden fields in error messages
        // it would be better to implement a filter on the schema so it
        // acts as if they just don't exist vs. an auth denied error
        // - filtering the schema is not currently supported
        _.Match<Field>(fieldAst => {
          var fieldDef = context.TypeInfo.GetFieldDef();
          if (fieldDef.RequiresPermissions() &&
              (!authenticated || !fieldDef.CanAccess(userContext.User.Claims))) {
            context.ReportError(new ValidationError(
                context.OriginalQuery,
                "auth-required",
                $"You are not authorized to run this query.",
                fieldAst));
          }
        });
      });
    }
  }
}
