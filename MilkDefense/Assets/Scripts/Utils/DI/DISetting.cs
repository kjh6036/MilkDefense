using System.Collections.Generic;
using UnityEngine;

public class DISetting : MonoBehaviour
{
    
    [SerializeField] private List<MonoBehaviour> Script_List = null;

    private bool isConstructed = false;
    public bool isMainDISetter = false;

    void OnValidate()
    {
        foreach (var script in Script_List)
        {
            if (script == null)
            {
                Debug.Log("Unavailable Script", this);
            }
        }
    }

    private void Awake()
    {
        Construct();
    }

    public void Construct()
    {
        if (isConstructed)
            return;

        isConstructed = true;

        foreach (var script in Script_List)
        {
            DependencyInjector.Constructor(script);
        }
    }

    private void OnDestroy()
    {
        foreach (var script in Script_List)
        {
            DependencyInjector.Demolisher(script);
        }
    }
}
