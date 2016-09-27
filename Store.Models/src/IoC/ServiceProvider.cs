﻿using System;

namespace Store.IoC {
  public interface IServiceProvider {
    T GetService<T>();
    object GetStore(string typeOfStore);
    Type GetType(string typeName);
    void Register<T>();
    void Register<T>(Func<T> instanceCreator);
    void Singleton<T>(T instance);
    void Singleton<T>(Func<T> instanceCreator);
  }

  /// <summary>
  /// Singleton implementation of the Service Locator Pattern.
  /// </summary>
  public class ServiceProvider : IServiceProvider {
    private static readonly object locker = new Object();
    private static IServiceProvider instance;
    private ISimpleContainer container = new SimpleContainer();
    
    private ServiceProvider() {}

    /// <summary>
    /// 
    /// </summary>
    public static IServiceProvider Instance {
      get {
        lock (locker) {
          if (instance == null) {
            instance = new ServiceProvider();
          }
        }
        return instance;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T GetService<T>() {
      return container.Get<T>();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public void Register<T>() {
      lock (locker) {
        container.Register<T>();
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="instanceCreator"></param>
    public void Register<T>(Func<T> instanceCreator) {
      lock (locker) {
        container.Register<T>(instanceCreator);
      }
    }

    /// <summary>
    /// Try adding instance of type in Key Value store.
    /// If it exists, error will be thrown, but we continue since we actually want to create a new instance.
    /// </summary>
    /// <typeparam name="T">Any class.</typeparam>
    /// <param name="instance">Singleton object of type T</param>
    public void Singleton<T>(T instance) {
      T t = default(T);
      try {
        t = GetService<T>();
      } catch (InvalidOperationException e) {
        // no op
      }
      if (t != null) return;
      lock (locker) {
        container.Singleton<T>(instance);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="instanceCreator"></param>
    public void Singleton<T>(Func<T> instanceCreator) {
      T t = default(T);
      try {
        t = GetService<T>();
      } catch (InvalidOperationException e) {
        // no op
      }
      if (t != null) return;
      lock (locker) {
        container.Singleton<T>(instanceCreator);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="typeOfStore"></param>
    /// <returns></returns>
    public object GetStore(string typeOfStore) {
      var ty = container.GetStore(typeOfStore);
      if (ty != null) return container.Get(ty as Type);
      return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="typeName"></param>
    /// <returns></returns>
    public Type GetType(string typeName) {
      return container.GetType(typeName);
    }
  }  
}
