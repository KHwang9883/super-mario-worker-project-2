using Godot;

namespace SMWP.Level.Data;

/// <summary>
/// 这个类代表 smwl 文件中的对象实例。
/// 可以是一块实心，一个板栗，一个紫树等。
/// 不是数据库条目！
/// <p/>
/// </summary>
/// <see cref="SmwpObjectDatabaseEntry"/> 数据库条目
[GlobalClass]
public partial class ClassicSmwlObject : Resource {
    [Export] public int Id { get; set; }
    [Export] public Vector2 Position { get; set; }
    [Export] public string Metadata { get; set; } = "";
}