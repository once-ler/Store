using System;
using GraphQL.Types;
using Store.Interfaces;
using Store.Models;
using Store.IoC;

namespace Store.GraphQL {
  public abstract class RegisterBase<T> where T : Model {
    public RegisterBase() {
      registerType();
    }

    protected void registerType() {
      // Type will be added to IoC.
      var t = new Type<T>();
    }
  }

  public abstract class Base<T> : RegisterBase<T> where T : Model {
    public Base() : base() {
      string tyName = typeof(T).Name;
      dynamic _store = ServiceProvider.Instance.GetStore(tyName);
      store = _store as IStore<T>;
      type = typeof(T);

      createGraphType();
    }

    public Base(string tyName) : base() {
      dynamic _store = ServiceProvider.Instance.GetStore(tyName);
      store = _store as IStore<T>;
      type = ServiceProvider.Instance.GetType(tyName);

      createGraphType();
    }

    public Base(Type ty) : base() {
      string tyName = ty.GetType().Name;
      dynamic _store = ServiceProvider.Instance.GetStore(tyName);
      store = _store as IStore<T>;
      type = ty;

      createGraphType();
    }

    public Base(IStore<T> _store) : base() {
      store = _store;

      createGraphType();
    }

    public ObjectGraphType getGraphType() {
      return graphType;
    }

    protected abstract ObjectGraphType createResolvers();

    protected IStore<T> store;
    protected Type type;
    protected ObjectGraphType graphType;

    private void createGraphType() {
      graphType = createResolvers();
    }

  }
}
