using Godot;
using SMWP.Util;

namespace SMWP.Level.Loader.Processing;

/// <summary>
/// 让问号块变成隐藏块
/// </summary>
public partial class HiddenBlockProcessor : ObjectProcessor {
    public override void ProcessObject(Node instance, string metadata) {
        if (!instance.TryGetComponent(out QuestionBlock? component)) {
            return;
        }
        component.SetHidden();
    }
}