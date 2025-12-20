using Godot;
using SMWP.Level.Data;

namespace SMWP.Level.Loader.Processing;

/// <summary>
/// 对对象进行加载后处理的基类。
/// 第一个用途是处理物品的元数据，探照灯的半径，起始角度和速度就是额外数据的一种。
/// 第二个用途是修改物品的运行时设置。例如问号块的隐藏变种。
/// </summary>
public abstract partial class ObjectProcessor : Resource {
    /// <summary>
    /// 此方法可以用于根据物品元数据动态生成对象，比如 043 号敌人（龟壳）的生成。
    /// 如果返回 null 则直接使用 <see cref="SmwpObjectDatabaseEntry.Prefab"/> 作为模板进行生成。
    /// </summary>
    /// <param name="definition">对象的数据条目</param>
    /// <param name="instance">解析后的对象实例</param>
    /// <returns>生成后的游戏引擎对象实例</returns>
    public virtual Node? CreateInstance(SmwpObjectDatabaseEntry definition, ClassicSmwlObject instance) => null;
    public virtual void ProcessObject(Node node, ClassicSmwlObject instance) {}
}