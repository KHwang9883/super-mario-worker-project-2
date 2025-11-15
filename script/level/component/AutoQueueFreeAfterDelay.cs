using Godot;

namespace SMWP.Level.Component;

[GlobalClass]
public partial class AutoQueueFreeAfterDelay : Node {
    [Export] private Node? _ancestor;
    [Export] private int _delayFrames = 100;
    [Export] private bool _fadeOut = false;
    [Export] private float _fadeSpeed = 0.1f;
    
    private int _timer;
    private CanvasItem? _targetSprite; // 用父类统一接收 Sprite2D/AnimatedSprite2D
    private float _initialAlpha;

    public override void _Ready() {
        if (_ancestor == null) {
            GD.PrintErr("_ancestor 未赋值！");
            return;
        }

        // 先转为共同父类 CanvasItem，再用 ?? 拼接
        _targetSprite = _ancestor.GetNodeOrNull<Sprite2D>("Sprite2D") as CanvasItem
                     ?? _ancestor.GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D") as CanvasItem;

        // 备选：遍历子节点查找（防止节点名称不匹配）
        if (_targetSprite == null) {
            foreach (var child in _ancestor.GetChildren()) {
                if (child is CanvasItem canvasItem && (child is Sprite2D || child is AnimatedSprite2D)) {
                    _targetSprite = canvasItem;
                    break;
                }
            }
        }

        // 记录初始透明度
        if (_targetSprite != null) {
            _initialAlpha = _targetSprite.Modulate.A;
        } else {
            GD.PrintErr("未在 _ancestor 下找到 Sprite2D 或 AnimatedSprite2D！");
        }
    }

    public override void _PhysicsProcess(double delta) {
        _timer++;
        if (_timer > _delayFrames) {
            if (!_fadeOut) {
                _ancestor?.QueueFree(); // 不淡出，直接删除
            } else {
                if (_targetSprite == null) {
                    _ancestor?.QueueFree(); // 没找到精灵，直接删除避免卡住
                    return;
                }

                // 每帧减少透明度（确保不小于 0）
                float newAlpha = _targetSprite.Modulate.A - _fadeSpeed;
                newAlpha = Mathf.Max(newAlpha, 0);

                // 更新透明度（只改 Alpha 通道，保持 RGB 颜色不变）
                _targetSprite.Modulate = new Color(
                    _targetSprite.Modulate.R,
                    _targetSprite.Modulate.G,
                    _targetSprite.Modulate.B,
                    newAlpha
                );

                // 完全透明后删除父节点
                if (newAlpha <= 0) {
                    _ancestor?.QueueFree();
                }
            }
        }
    }
}