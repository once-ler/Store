using System.Threading.Tasks;
using System.IO;

namespace Store.GraphQL.Util {
  public static class Helper {
    public static async Task<string> ReadAll(string path) {
      using (var reader = File.OpenText(path)) {
        return await reader.ReadToEndAsync();        
      }
    }

    public static void WriteAll(this string text, string path) {
      var dir = Path.GetDirectoryName(path);
      Directory.CreateDirectory(dir);
      File.WriteAllText(path, text);
    }
  }
}
