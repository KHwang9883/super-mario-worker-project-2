using Godot;
using Godot.Collections;

namespace SMWP.Level.Data;

[GlobalClass]
public partial class ClassicSmwlBlocksData : Resource {
    /// <summary>
    /// 每一个元素都代表 Block 中一个字符，
    /// 即两个字符代表一个 Block
    /// </summary>
    [Export] public Array<byte[]> BlockValues { get; private set; } = [];
    [Export] public int Width { get; private set; }

    public ClassicSmwlBlocksData() {
    }

    public ClassicSmwlBlocksData(Array<byte[]> blocks, int width) {
        BlockValues = blocks;
        Width = width;
    }
}