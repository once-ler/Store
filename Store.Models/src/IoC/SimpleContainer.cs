﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Store.IoC {
  public interface ISimpleContainer : IDisposable {
    object Get(Type serviceType);
    T Get<T>();
    void Register<TService>();
    void Register<TService>(Func<TService> instanceCreator);
    void Register<TService, TImpl>() where TImpl : TService;
    void Singleton<TService>(TService instance);
    void Singleton<TService>(Func<TService> instanceCreator);
  }

  /// <summary>
  /// 
  /// </summary>
  public class SimpleContainer : ISimpleContainer {
    private readonly Dictionary<Type, Func<object>> _registrations = new Dictionary<Type, Func<object>>();

    public void Register<TService>() {
      Register<TService, TService>();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <typeparam name="TImpl"></typeparam>
    public void Register<TService, TImpl>() where TImpl : TService {
      _registrations.Add(typeof(TService), () => {
        var implType = typeof(TImpl);
        return typeof(TService) == implType ? CreateInstance(implType) : Get(implType);
      });
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <param name="instanceCreator"></param>
    public void Register<TService>(Func<TService> instanceCreator) {
      _registrations.Add(typeof(TService), () => instanceCreator());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <param name="instance"></param>
    public void Singleton<TService>(TService instance) {
      _registrations.Add(typeof(TService), () => instance);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <param name="instanceCreator"></param>
    public void Singleton<TService>(Func<TService> instanceCreator) {
      var lazy = new Lazy<TService>(instanceCreator);
      Register(() => lazy.Value);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T Get<T>() {
      return (T)Get(typeof(T));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="serviceType"></param>
    /// <returns></returns>
    public object Get(Type serviceType) {
      Func<object> creator;
      if (_registrations.TryGetValue(serviceType, out creator)) {
        return creator();
      }

      if (!serviceType.GetTypeInfo().IsAbstract) {
        return CreateInstance(serviceType);
      }

      throw new InvalidOperationException("No registration for " + serviceType);
    }

    public void Dispose() {
      _registrations.Clear();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="implementationType"></param>
    /// <returns></returns>
    private object CreateInstance(Type implementationType) {
      var ctor = implementationType.GetTypeInfo().GetConstructors().Single();
      var parameterTypes = ctor.GetParameters().Select(p => p.ParameterType);
      var dependencies = parameterTypes.Select(Get).ToArray();
      return Activator.CreateInstance(implementationType, dependencies);
    }
  }
}
