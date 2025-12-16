using Godot;
using Godot.Collections;
using SMWP.Level;

public partial class KoopaHUD : Node2D {
    [Export] public Node HpNormal = null!;
    [Export] public Node HpAdvanced = null!;
    [Export] public Label OverHpCounter = null!;
    
    private LevelConfig? _levelConfig;
    public bool Activate;
    private float _originPosY;
    private int _hp;
    
    private Array<Node> _hpNormalBar = new();
    private Array<Node> _hpAdvancedBar = new();
    
    private int _lastNormalVisibleCount = -1;
    private int _lastAdvancedVisibleCount = -1;

    public override void _Ready() {
        _originPosY = Position.Y;
        Position += Vector2.Up * 120;
        ResetPhysicsInterpolation();
        _levelConfig ??= LevelConfigAccess.GetLevelConfig(this);
        _hpNormalBar = HpNormal.GetChildren();
        _hpAdvancedBar = HpAdvanced.GetChildren();
    }
    
    public override void _PhysicsProcess(double delta) {
        // 血条入场
        if (Activate) {
            Position = Position with { Y = Mathf.MoveToward(Position.Y, _originPosY, 1f) };
        }
        
        // 血量显示
        if (_levelConfig == null) {
            GD.PushError($"{this}: LevelConfig is null!");
            return;
        }
        
        _hp = _levelConfig.KoopaEnergy;
        
        // 大于 500 HP 隐藏
        Visible = _hp <= 500;
        
        // 血条显示逻辑
        if (_hp is >= 0 and <= 20) {
            // 计算应该显示的血条数量
            var normalToShow = Mathf.Min(_hp, 10);
            var advancedToShow = Mathf.Max(_hp - 10, 0);
            
            // 普通血条
            for (var i = 0; i < _hpNormalBar.Count; i++) {
                if (_hpNormalBar[i] is Control hpBar) {
                    hpBar.Visible = i < normalToShow;
                }
            }
            
            // 高级血条
            for (var i = 0; i < _hpAdvancedBar.Count; i++) {
                if (_hpAdvancedBar[i] is Control hpBar) {
                    hpBar.Visible = i < advancedToShow;
                }
            }
            
            // 缓存当前显示的血条数量
            _lastNormalVisibleCount = normalToShow;
            _lastAdvancedVisibleCount = advancedToShow;
        }
        
        // 数字显示
        if (_hp is > 20 and <= 500) {
            OverHpCounter.Visible = true;
            OverHpCounter.Text = _hp.ToString();
        } else {
            OverHpCounter.Visible = false;
        }
    }
}