namespace Store.Reports {
  public static class Extensions {
    public static string WrapWithTag(this string input, string tagName, string attributes = "") {
      return "<" + tagName + " " + attributes + ">" + input + "</" + tagName + ">";
    }
  }
}
