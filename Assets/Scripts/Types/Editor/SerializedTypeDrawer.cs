using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;


[CustomPropertyDrawer(typeof(SerializedType<>), true)]
public class SerializedTypeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var nameProp = property.FindPropertyRelative("_typeName");
        if (nameProp == null)
        {
            EditorGUI.LabelField(position, label.text, "[SerializedType] missing _assemblyQualifiedName");
            EditorGUI.EndProperty();
            return;
        }

        Type baseType = ResolveBaseType(property);
        if (baseType == null)
        {
            EditorGUI.LabelField(position, label.text, "[SerializedType] could not resolve base type");
            EditorGUI.EndProperty();
            return;
        }

        List<Type> subtypes = GetSubtypes(baseType);
        string[] displayNames = subtypes.Select(t => t.Name).Prepend("None").ToArray();

        Type currentType = string.IsNullOrEmpty(nameProp.stringValue)
                              ? null
                              : Type.GetType(nameProp.stringValue);
        int currentIndex = currentType == null ? 0 : subtypes.IndexOf(currentType) + 1;

        int newIndex = EditorGUI.Popup(position, label.text, currentIndex, displayNames);
        if (newIndex != currentIndex)
            nameProp.stringValue = newIndex == 0
              ? string.Empty
              : subtypes[newIndex - 1].AssemblyQualifiedName;

        EditorGUI.EndProperty();
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private static Type ResolveBaseType(SerializedProperty property)
    {
        FieldInfo fi = ResolveFieldInfo(property);
        if (fi == null) return null;

        Type fieldType = fi.FieldType;
        while (fieldType != null)
        {
            if (fieldType.IsGenericType &&
                fieldType.GetGenericTypeDefinition() == typeof(SerializedType<>))
                return fieldType.GetGenericArguments()[0];

            fieldType = fieldType.BaseType;
        }
        return null;
    }

    /// <summary>
    /// Walks the property path and climbs the inheritance chain at each step,
    /// so fields declared on a base class are found correctly.
    /// </summary>
    private static FieldInfo ResolveFieldInfo(SerializedProperty property)
    {
        Type type = property.serializedObject.targetObject.GetType();

        FieldInfo fi = null;
        foreach (string segment in property.propertyPath.Split('.'))
        {
            fi = null;
            Type t = type;
            while (t != null && fi == null)
            {
                fi = t.GetField(segment,
                       BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                t = t.BaseType;
            }
            type = fi?.FieldType;
        }
        return fi;
    }

    private static List<Type> GetSubtypes(Type baseType)
    {
        return AppDomain.CurrentDomain.GetAssemblies()
          .SelectMany(a => { try { return a.GetTypes(); } catch { return Array.Empty<Type>(); } })
          .Where(t => !t.IsAbstract && !t.IsInterface && baseType.IsAssignableFrom(t))
          .OrderBy(t => t.Name)
          .ToList();
    }
}

