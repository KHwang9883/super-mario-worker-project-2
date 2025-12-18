using Godot;

namespace SMWP.Level.Loader.Processing;

/// <summary>
/// 对对象进行加载后处理的基类。
/// 第一个用途是处理物品的元数据，探照灯的半径，起始角度和速度就是额外数据的一种。
/// 第二个用途是修改物品的运行时设置。例如问号块的隐藏变种。
/// </summary>
public abstract partial class ObjectProcessor : Resource {
    public abstract void ProcessObject(Node instance, string metadata);
}