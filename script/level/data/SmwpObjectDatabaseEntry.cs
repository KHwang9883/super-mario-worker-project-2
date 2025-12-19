using Godot;
using SMWP.Level.Loader.Processing;

namespace SMWP.Level.Data;

/// <summary>
/// 对象数据条目，一个实例代表一类对象。
/// </summary>
[GlobalClass]
public partial class SmwpObjectDatabaseEntry : Resource {
    /// <summary>
    /// 该物件的 id
    /// </summary>
    [Export] public int Id { get; set; }
    
    /// <summary>
    /// 该物件对应的预制体场景。
    /// 考虑到以后可能会有不需要实例的特殊物品，所以这个字段是 nullable 的。
    /// </summary>
    [Export] public PackedScene? Prefab { get; set; }
    
    /// <summary>
    /// 该物件生成位置与编辑器中位置的相对位置
    /// </summary>
    [Export] public Vector2 SpawnOffset { get; set; } = new(16, 16);
    
    /// <summary>
    /// 该物件额外数据（例如探照灯的数据）的处理器。
    /// </summary>
    [Export] public ObjectProcessor? MetadataProcessor { get; set; }
}