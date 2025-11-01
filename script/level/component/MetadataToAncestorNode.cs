using Godot;
using System;
using System.Collections.Generic;

public partial class MetadataToAncestorNode : Node 
{
    [Export] private NodePath _sourcePath = ".."; // 默认：父节点
    [Export] private NodePath _targetPath = "../.."; // 默认：祖父节点
    [Export] private bool _copyOnReady = true;
    [Export] private bool _overwriteExisting = true;
    
    private string _logPrefix = "[MetadataCopy]";
    
    public override void _Ready()
    {
        if (_copyOnReady)
        {
            CopyMetadataToAncestor();
        }
    }
    
    /// <summary>
    /// 将源节点的所有metadata复制到目标祖先节点
    /// </summary>
    public void CopyMetadataToAncestor()
    {
        try
        {
            // 1. 获取源节点（默认为父节点）
            Node sourceNode = GetNodeOrNull(_sourcePath);
            if (sourceNode == null)
            {
                GD.PrintErr($"{_logPrefix} 源节点不存在: {_sourcePath}");
                return;
            }
            
            // 2. 获取目标节点（默认为祖父节点）
            Node targetNode = GetNodeOrNull(_targetPath);
            if (targetNode == null)
            {
                GD.PrintErr($"{_logPrefix} 目标节点不存在: {_targetPath}");
                return;
            }
            
            // 3. 获取源节点的所有metadata键
            var metaKeys = GetMetaKeys(sourceNode);
            if (metaKeys.Count == 0)
            {
                GD.Print($"{_logPrefix} 源节点没有metadata可复制");
                return;
            }
            
            // 4. 复制metadata
            int copiedCount = 0;
            foreach (string key in metaKeys)
            {
                if (CopySingleMeta(sourceNode, targetNode, key))
                {
                    copiedCount++;
                }
            }
            
            GD.Print($"{_logPrefix} 成功复制 {copiedCount}/{metaKeys.Count} 个metadata");
        }
        catch (Exception e)
        {
            GD.PrintErr($"{_logPrefix} 复制metadata时出错: {e.Message}");
        }
    }
    
    /// <summary>
    /// 获取节点的所有metadata键（正确的实现）
    /// </summary>
    private List<string> GetMetaKeys(Node node)
    {
        var keys = new List<string>();
        
        // GetMetaList() 返回的是 StringName 数组，包含所有metadata键
        Godot.Collections.Array<StringName> metaList = node.GetMetaList();
        
        foreach (StringName key in metaList)
        {
            keys.Add(key.ToString());
        }
        
        return keys;
    }
    
    /// <summary>
    /// 复制单个metadata键值对
    /// </summary>
    private bool CopySingleMeta(Node source, Node target, string key)
    {
        try
        {
            // 检查目标节点是否已存在该metadata
            if (target.HasMeta(key))
            {
                if (!_overwriteExisting)
                {
                    GD.Print($"{_logPrefix} 跳过已存在的键: {key}");
                    return false;
                }
                GD.Print($"{_logPrefix} 覆盖已存在的键: {key}");
            }
            
            // 获取源节点的metadata值
            Variant value = source.GetMeta(key);
            
            // 设置到目标节点
            target.SetMeta(key, value);
            
            GD.Print($"{_logPrefix} 复制: {key} = {ValueToString(value)}");
            return true;
        }
        catch (Exception e)
        {
            GD.PrintErr($"{_logPrefix} 复制键 '{key}' 时出错: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 将Variant值转换为可读字符串
    /// </summary>
    private string ValueToString(Variant value)
    {
        try
        {
            switch (value.VariantType)
            {
                case Variant.Type.String:
                    return $"\"{value.AsString()}\"";
                case Variant.Type.StringName:
                    return $"StringName(\"{value.AsStringName()}\")";
                case Variant.Type.Int:
                    return value.AsInt32().ToString();
                case Variant.Type.Float:
                    return value.AsDouble().ToString("F2");
                case Variant.Type.Bool:
                    return value.AsBool().ToString();
                case Variant.Type.Vector2:
                    return value.AsVector2().ToString();
                case Variant.Type.Object:
                    var obj = value.AsGodotObject();
                    return obj != null ? $"[{obj.GetType().Name}]" : "[Null Object]";
                default:
                    return $"[{value.VariantType}]";
            }
        }
        catch
        {
            return "[无法转换的值]";
        }
    }
    
    /// <summary>
    /// 手动触发复制（可在代码中调用）
    /// </summary>
    public void ExecuteCopy()
    {
        CopyMetadataToAncestor();
    }
    
    /// <summary>
    /// 清空目标节点的metadata
    /// </summary>
    public void ClearTargetMetadata()
    {
        Node targetNode = GetNodeOrNull(_targetPath);
        if (targetNode != null)
        {
            var keys = GetMetaKeys(targetNode);
            foreach (string key in keys)
            {
                targetNode.RemoveMeta(key);
            }
            GD.Print($"{_logPrefix} 已清空目标节点的 {keys.Count} 个metadata");
        }
    }
}