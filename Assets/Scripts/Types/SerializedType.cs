using System;
using UnityEngine;



[Serializable]
public class SerializedType<T>
{
    [SerializeField] private string _typeName;

    public Type Type
    {
        get => string.IsNullOrEmpty(_typeName) ? null : System.Type.GetType(_typeName);
        set => _typeName = value?.AssemblyQualifiedName;
    }
}

