using System.Collections.Generic;
using UnityEngine;


/// 의존성 주입 클래스

public static class DependencyInjector
{
    private readonly static Dictionary<string, MonoBehaviour> script_Dictionary = new Dictionary<string, MonoBehaviour>();

    public static void Constructor(MonoBehaviour script)
    {
        string key = script.GetType().Name;
        if (script_Dictionary.ContainsKey(key))
        {
            script_Dictionary[key] = script;
        }
        else
        {
            script_Dictionary.Add(key, script);
        }
    }


    public static void Demolisher(MonoBehaviour script) => script_Dictionary.Remove(script.GetType().Name);


    public static T Get<T>() where T : MonoBehaviour => GetScript<T>(true);


    private static T GetScript<T>(bool canFail = false) where T : MonoBehaviour
    {
        string key = typeof(T).Name;
        if (!script_Dictionary.ContainsKey(key) && !canFail)
        {
            //Debug.Log($"{key} dose not exist");
        }
        return script_Dictionary.ContainsKey(key) ? (T)script_Dictionary[key] : null;
    }

}
