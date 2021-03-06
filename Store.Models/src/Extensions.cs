﻿using System;
using System.Collections.Generic;

namespace Store {
  /// <summary>
  /// 
  /// </summary>
  public static class Extensions {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="t">Type of TypeInfo : Type</param>
    /// <returns></returns>
    public static List<Type> InheritsFrom(this Type t) {
      List<Type> types = new List<Type>();
      Type cur = t.BaseType;
      types.Add(cur);
      while (cur != null) {
        cur = cur.BaseType;
        if (cur != null) types.Add(cur);
      }
      return types;
    }
  }
}
