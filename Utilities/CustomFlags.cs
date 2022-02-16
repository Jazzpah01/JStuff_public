using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomFlags : MonoBehaviour
{
    [SerializeField] private List<string> flags;
    private HashSet<string> actualFlags;

    private void Awake()
    {
        actualFlags = new HashSet<string>();
        actualFlags.UnionWith(flags);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="flag">Flags should be lower caps.</param>
    public void AddFlag(string flag)
    {
        actualFlags.Add(flag);
    }

    public bool HasFlag(string flag)
    {
        return (actualFlags.Contains(flag));
    }
}