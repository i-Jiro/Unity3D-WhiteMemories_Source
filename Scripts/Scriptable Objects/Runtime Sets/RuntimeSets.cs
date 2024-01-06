using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RuntimeSets<T> : ScriptableObject
{
    public List<T> Items = new List<T>();

    public void Add(T item)
    {
        if (!Items.Contains(item))
        {
            Items.Add(item);
        }
    }

    public void Remove(T item)
    {
        if (Items.Contains(item))
        {
            Items.Remove(item);
        }
    }
    
    public void Clear()
    {
        Items.Clear();
    }

    public T GetItemIndex(int index)
    {
        return Items[index];
    }
}
