using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using Godot;

namespace SMWP.Util;

/// <summary>
/// RotoDiscMovement，你好啊
/// </summary>
public static class ComponentCache {
    public static T? GetComponent<T>(this Node node) {
        return TryGetComponent(node, out T? result) ? result : default;
    }
    
    public static bool TryGetComponent<T>(this Node node, [NotNullWhen(true)] out T? target) {
        return (target = GetComponents<T>(node).FirstOrDefault(IsValidObject)) != null;
    }
    
    public static IEnumerable<T> GetComponents<T>(this Node node) {
        var map = CacheByType<T>.Entries;
        if (map.TryGetValue(node, out var references)) {
            return references;
        }
        var count = node.GetChildCount();
        var buffer = new List<T>();
        for (int i = 0; i < count; i++) {
            if (node.GetChild(i) is T t) {
                buffer.Add(t);
            }
        }
        var values = buffer.ToArray();
        map.Add(node, values);
        return values;
    }
    
    private static class CacheByType<T> {
        public static readonly ConditionalWeakTable<Node, T[]> Entries = [];
    }

    private static bool IsValidObject<T>(T obj) {
        return obj is not GodotObject gdo || GodotObject.IsInstanceValid(gdo);
    }
}