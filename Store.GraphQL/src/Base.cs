using System;
using GraphQL.Types;
using Store.Interfaces;
using Store.Models;
using Store.IoC;

namespace Store.GraphQL {
  public abstract class Base<T> where T : Model {
    public Base() {
      string tyName = typeof(T).Name;
      dynamic _store = ServiceProvider.Instance.GetStore(tyName);
      store = _store as IStore<T>;
      type = typeof(T);
    }

    public Base(string tyName) {
      dynamic _store = ServiceProvider.Instance.GetStore(tyName);
      store = _store as IStore<T>;
      type = ServiceProvider.Instance.GetType(tyName);
    }

    public Base(Type ty) {
      string tyName = ty.GetType().Name;
      dynamic _store = ServiceProvider.Instance.GetStore(tyName);
      store = _store as IStore<T>;
      type = ty;
    }

    public Base(IStore<T> _store) {
      store = _store;
    }

    public abstract ObjectGraphType CreateGraphQLType();

    protected IStore<T> store;
    protected Type type;
  }
}
