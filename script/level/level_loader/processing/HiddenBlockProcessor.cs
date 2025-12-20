using Godot;
using SMWP.Level.Data;
using SMWP.Util;

namespace SMWP.Level.Loader.Processing;

/// <summary>
/// 让问号块变成隐藏块
/// </summary>
public partial class HiddenBlockProcessor : ObjectProcessor {
    public override void ProcessObject(Node node, ClassicSmwlObject instance) {
        if (node.TryGetComponent(out QuestionBlock? component)) {
            component.Hidden = true;
        }
    }
}