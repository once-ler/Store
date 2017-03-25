﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using GraphQL.Types;
using Store.Interfaces;
using Store.Models;
using Store.IoC;

using GQL = GraphQL;

namespace Store.GraphQL {
  public interface IObjectGraphType {
    ObjectGraphType getGraphType();
  }

  /// <summary>
  /// Used for RootQuery and RootMutation.
  /// </summary>
  public class Root : Model {}

  public abstract class RegisterBase<T> where T : Model {
    public RegisterBase() {
      registerType();
    }

    protected void registerType() {
      // A new type derived from ObjectGraphType<T> will be created.
      // Type will then be added to IoC.
      var t = new GQLType<T>();
    }
  }

  /// <summary>
  /// Query and Mutation will derive from this class.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public abstract class GQLBase<T> : RegisterBase<T>, IObjectGraphType where T : Model {
    public GQLBase() : base() {
      string tyName = typeof(T).Name;
      dynamic _store = ServiceProvider.Instance.GetStore(tyName);
      store = _store as IStore<T>;
      type = typeof(T);

      createGraphType();
    }

    public GQLBase(string tyName) : base() {
      dynamic _store = ServiceProvider.Instance.GetStore(tyName);
      store = _store as IStore<T>;
      type = ServiceProvider.Instance.GetType(tyName);

      createGraphType();
    }

    public GQLBase(Type ty) : base() {
      string tyName = ty.GetType().Name;
      dynamic _store = ServiceProvider.Instance.GetStore(tyName);
      store = _store as IStore<T>;
      type = ty;

      createGraphType();
    }

    public GQLBase(IStore<T> _store) : base() {
      store = _store;

      createGraphType();
    }

    public ObjectGraphType getGraphType() {
      return graphType;
    }

    protected abstract ObjectGraphType createResolvers();

    internal IStore<T> store;
    internal Type type;
    protected ObjectGraphType graphType;

    private void createGraphType() {
      graphType = createResolvers();
    }

  }
}