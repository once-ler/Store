using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Store.IoC {
  public interface ISimpleContainer : IDisposable {
    object Get(Type serviceType);
    T Get<T>();
    object GetStore(string typeOfStore);
    Type GetType(string typeName);
    dynamic Get(string typeName);
    void Register(string typeName, Type type);
    void Register(string typeName, dynamic instance);
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
    private readonly Dictionary<string, Func<Type>> _registrationsByName = new Dictionary<string, Func<Type>>();
    private readonly Dictionary<string, Func<object>> _registrationsInstanceByName = new Dictionary<string, Func<object>>();

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    public void Register<TService>() {
      Register<TService, TService>();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="typeName"></param>
    /// <param name="type"></param>
    public void Register(string typeName, Type type) {
      Func<Type> f1;
      _registrationsByName.TryGetValue(typeName, out f1);
      if (f1 == null) _registrationsByName.Add(typeName, () => type);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="typeName"></param>
    /// <param name="instance"></param>
    public void Register(string typeName, dynamic instance) {
      Func<object> f1;
      _registrationsInstanceByName.TryGetValue(typeName, out f1);
      if (f1 == null) _registrationsInstanceByName.Add(typeName, () => instance);
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="typeOfStore"></param>
    /// <returns></returns>
    public dynamic GetStore(string typeOfStore) {
      IList<Type> typesToRegister = new List<Type>();

      var a = _registrations.Where(d => {
        var ty = d.Key;
        var innerType = ty.IsGenericType;
        if (innerType == true) {
          var b = ty.GetGenericArguments().FirstOrDefault();
          if (b != null && typeOfStore == b.Name) {
            typesToRegister.Add(b);
            return true;
          }
        }
        return false;
      }).ToList();
      
      // Register the type internally
      foreach (var b in typesToRegister) {
        Func<object> f;
        _registrations.TryGetValue(b, out f);
        if (f == null) _registrations.Add(b, () => b);
        // Register by type name
        Register(b.Name, b);
      }

      if (a.Count() > 0) return a.FirstOrDefault().Key;

      return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="typeName"></param>
    /// <returns></returns>
    public Type GetType(string typeName) {
      Func<Type> f;
      _registrationsByName.TryGetValue(typeName, out f);
      if (f != null) {
        return f();
      }
      return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="typeName"></param>
    /// <returns></returns>
    public dynamic Get(string typeName) {
      Func<object> f1;
      _registrationsInstanceByName.TryGetValue(typeName, out f1);
      return f1;
    }

    /// <summary>
    /// 
    /// </summary>
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
