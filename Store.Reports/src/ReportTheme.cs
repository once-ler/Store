using System;
using System.Dynamic;
using System.Linq;
using System.Collections.Generic;

namespace Store.Reports {
  public class ReportTheme {
    public struct ThemeDefinition {
      public string style { get; set; }
      public string script { get; set; }
    }

    public ReportTheme() { }

    public Func<IEnumerable<dynamic>, IEnumerable<dynamic>> createStyle(string themeName, string titleName = "") {
      return (IEnumerable<dynamic> children) => {
        var t = this.getTheme(themeName);

        var head = string.Join("", new string[] {
          "".WrapWithTag("meta", @"http-equiv=""X-UA-Compatible"" content=""IE=edge"""),
          titleName.WrapWithTag("title"),
          t.style.WrapWithTag("style")
        }).WrapWithTag("head");

        var body = string.Join("", new string[] {
          string.Join("", children.Select(a => (string)a).ToArray<string>()),
          t.script.WrapWithTag("script")
        }).WrapWithTag("body");

        var html = string.Join("", new string[] { head, body }).WrapWithTag("html");

        return new string[] {
          "<!DOCTYPE html>",
          html
        };
      };
    }

    public Func<IEnumerable<dynamic>, IEnumerable<dynamic>> createBody(string titleName = "") {
      return (IEnumerable<dynamic> children) => {
        var thead = string.Join("", (children.FirstOrDefault() as ExpandoObject).Select(r => r.Key.WrapWithTag("div").WrapWithTag("th"))).WrapWithTag("tr").WrapWithTag("thead");

        var tbody = string.Join("",
          children
            .Select(d =>
              string.Join("",
                (
                  (d as ExpandoObject).Select(r =>
                    ((string)r.Value).WrapWithTag("pre").WrapWithTag("td")
                  )
                )
              ).WrapWithTag("tr")
            )
          ).WrapWithTag("tbody");

        var ul = "".WrapWithTag("ul", @"id=""fixed-header""");
        var table = string.Join("", new string[] { thead, tbody }).WrapWithTag("table");
        return new string[] { ul, table };
      };
    }

    ThemeDefinition getTheme(string themeName) {
      ThemeDefinition t;
      var b = themes_.TryGetValue(themeName, out t);
      return b == true ? t : themes_["default"];
    }

    Dictionary<string, ThemeDefinition> themes_ = new Dictionary<string, ThemeDefinition> {
      { "default", new ThemeDefinition { style = "", script = "" } },
      { "pastel", new ThemeDefinition
        {
          style = "body{scrollbar-3dlight-color:#fff;scrollbar-arrow-color:#f5f5f5;scrollbar-base-color:#696969;scrollbar-darkshadow-color:#f5f5f5;scrollbar-face-color:#696969;scrollbar-highlight-color:#fff;scrollbar-shadow-color:#f5f5f5;scrollbar-track-color:#708090;font:normal normal 10px arial,sans-serif;background-color:transparent;padding:0;margin:0;font-size:11px}table{font-size:11px;border-spacing:0;border-collapse:collapse}td{padding:0}tr:hover td{background-color:#ffffe0}tr:hover td pre{background-color:#ff0}pre{font-family:Trebuchet MS,Lucida Grande,Lucida Sans Unicode,Lucida Sans,Tahoma,sans-serif;font-size:11px}th,td{padding:5px 8px}th{background-color:#8fbcbc;color:#fff;height:34px}td{border-right:1px solid #ccc;white-space:nowrap}tr:nth-child(even){background:#f1e8e8}tr:nth-child(odd){background:#f8f3f3}tr{vertical-align:top}ul{display:inline-block;list-style-type:none;background:#8fbcbc;padding:0;margin:0;color:#fff}",
          script = "function handleWindowScroll(n){function o(n){return'y'===n?window.scrollY||document.documentElement.scrollTop:window.scrollX||document.documentElement.scrollLeft}function t(){var i=o();e!==i?(n(e=i,o('y')),d(t)):d(t)}var i,d=window.requestAnimationFrame||window.webkitRequestAnimationFrame||window.mozRequestAnimationFrame||window.msRequestAnimationFrame||window.o,e=window.scrollX,c=!1;d?t():window.onscroll(function(){c||(c=!0,clearTimeout(i),n(o(),o('y')),setTimeout(function(){c=!1},100),i=setTimeout(function(){n(o(),o('y'))},200))})}!function(){function n(n,o,t){var i=document.createElement('LI'),d=document.createElement('div');i.appendChild(d);var e=document.createTextNode(o);d.setAttribute('id','col-'+n),d.setAttribute('style',t),d.appendChild(e),document.getElementById('fixed-header').appendChild(i)}var o=function(n,o){for(var t=document.querySelectorAll('th'),i=0;i<t.length;i++){var d=8+t[i].offsetLeft-n;document.getElementById('col-'+i).style.left=d+'px'}},t=!1;document.addEventListener('DOMContentLoaded',function(){if(!t){t=!0;for(var i=document.querySelectorAll('th'),d=0,e=0;e<i.length;e++){d+=i[e].offsetWidth;var c=8+i[e].offsetLeft;n(e,i[e].firstChild.innerHTML,'position: fixed; top: 8px; left: '+c+'px;')}document.querySelector('thead').setAttribute('style','visibility: hidden;'),document.getElementById('fixed-header').setAttribute('style','width: '+d+'px;height: 44px;position:fixed;top:0;'),handleWindowScroll(o)}},!1)}();"
        }
      }
    };
  }
}
