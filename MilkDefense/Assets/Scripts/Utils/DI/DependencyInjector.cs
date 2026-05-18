using System;
using System.Collections.Generic;
using UnityEngine;

/// 의존성 주입 클래스
public static class DependencyInjector
{
    private static readonly Dictionary<Type, MonoBehaviour> _registry = new Dictionary<Type, MonoBehaviour>();

    public static void Constructor(MonoBehaviour script)
    {
        Type key = script.GetType();
        if (_registry.ContainsKey(key))
            _registry[key] = script;
        else
            _registry.Add(key, script);
    }

    public static void Demolisher(MonoBehaviour script)
    {
        _registry.Remove(script.GetType());
    }

    public static T Get<T>() where T : MonoBehaviour
    {
        if (_registry.TryGetValue(typeof(T), out var script))
            return (T)script;

        Debug.LogWarning($"[DependencyInjector] {typeof(T).Name} 이 등록되지 않았습니다.");
        return null;
    }
}