using System;
using System.Linq;
using System.Collections.Generic;
using Store.Models;
using Store.Storage;

namespace Store.Reports {
  public class ReportType {
    public enum ContentFormat {
      HTML,
      JSON,
      XML,
      EXCEL
    }

    public ReportType(ContentFormat contentType) {
      outgoingFormat = contentType;
    }

    public ContentFormat outgoingFormat { get; set; }

    public string process(IEnumerable<dynamic> context, Func<IEnumerable<dynamic>, IEnumerable<dynamic>> transformFunc, Func<IEnumerable<dynamic>, IEnumerable<dynamic>> themeFunc = null, params Func<IEnumerable<dynamic>, IEnumerable<dynamic>>[] userDefinedFuncs) {
      var e = processImpl(context, transformFunc, themeFunc);
      e = processImpl(e, userDefinedFuncs);
      return string.Join("", e.Select(a => (string)a).ToArray<string>());
    }

    private IEnumerable<dynamic> processImpl(IEnumerable<dynamic> context, params Func<IEnumerable<dynamic>, IEnumerable<dynamic>>[] funcs) {
      foreach (var func in funcs) {
        if (func != null)
          context = func(context);
      }
      return context;
    }

  }

  public class ReportFactory<T> where T : ReportType {
    public ReportType reportType { get; private set; }

    public ReportFactory(T reportType_) {
      reportType = reportType_;
    }

    public dynamic execute(BasicClient client, string statement, Func<IEnumerable<dynamic>, IEnumerable<dynamic>> transform, Func<IEnumerable<dynamic>, IEnumerable<dynamic>> themeFunc = null, params Func<IEnumerable<dynamic>, IEnumerable<dynamic>>[] userDefinedFuncs) {
      dynamic retval = null;
      try {
        retval = client.runSqlDynamic(statement);
      } catch (Exception err) {
        // No op
      }

      return reportType.process(retval, transform, themeFunc, userDefinedFuncs);
    }
        
  }
}
