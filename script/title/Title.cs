using Godot;
using System;

public partial class Title : Node2D {
    [Export] private Node2D? _titleToSpin;
    [Export] private AnimationPlayer? _animationPlayer;
    [Export] private Node2D? _marioworkerCup;
    [Export] private bool _creatingLightStar;
    [Export] private PackedScene _lightStarScene = GD.Load<PackedScene>("uid://cg1273gwl8g68");
    [Export] private Control? _control;
    //[Export] private String _optionSceneUid = "uid://bh01ow2pgnsh5";
    
    public enum TitleAnimationStatus { Spin, Light, Lighting, Over}
    private TitleAnimationStatus _animationStatus = TitleAnimationStatus.Spin;
    private float _spinAngle = 180f;
    private CanvasItemMaterial? _titleMaterial;
    private RandomNumberGenerator _rng = new();

    public override void _Ready() {
        if (_titleToSpin == null) return;
        _titleToSpin.Visible = true;
        _titleToSpin.RotationDegrees -= 90f;
    }
    public override void _PhysicsProcess(double delta) {
        if (_titleToSpin == null) return;
        
        switch (_animationStatus) {
            case TitleAnimationStatus.Spin:
                _titleToSpin.RotationDegrees += _spinAngle;
                _spinAngle = Mathf.MoveToward(_spinAngle, 0f, 1f);
                if (_spinAngle == 0f || Input.IsActionJustPressed("mouse_left")) {
                    _animationStatus = TitleAnimationStatus.Light;
                }
                break;
            
            case TitleAnimationStatus.Light:
                _titleToSpin.RotationDegrees = 0f;
                //_titleToSpin.Modulate = _titleToSpin.Modulate with { A = 0f };
                _titleMaterial = (CanvasItemMaterial)_titleToSpin.Material.Duplicate();
                _titleMaterial.BlendMode = CanvasItemMaterial.BlendModeEnum.Add;
                _titleToSpin.Material = _titleMaterial;
                _animationStatus = TitleAnimationStatus.Lighting;
                break;
            
            case TitleAnimationStatus.Lighting:
                var alpha = _titleToSpin.Modulate.A;
                _titleToSpin.Modulate =
                    _titleToSpin.Modulate with { A = Mathf.MoveToward(alpha, 0f, 0.02f) };
                if (Math.Abs(alpha - 0f) < 0.02f) {
                    if (_titleMaterial != null) {
                        _titleMaterial.BlendMode = CanvasItemMaterial.BlendModeEnum.Mix;
                        _titleToSpin.Material = _titleMaterial;
                    }
                    _animationStatus = TitleAnimationStatus.Over;
                    
                    // Cursor focus
                    Callable.From(() => {
                        GetNode<Control>("Control/Edit").GrabFocus();
                    }).CallDeferred();
                    
                    if (_animationPlayer != null) {
                        _animationPlayer.Active = true;
                        _animationPlayer.Play();
                    }
                    if (_control != null) _control.ProcessMode = ProcessModeEnum.Inherit;
                }
                break;
        }
        
        if (_creatingLightStar) CreateLightStar();
    }
    public void CreateLightStar() {
        if (_marioworkerCup == null) return;
        var lightStar = _lightStarScene.Instantiate<Node2D>();
        lightStar.Position =
            _marioworkerCup.Position
            + new Vector2(_rng.RandfRange(0, 60) - _rng.RandfRange(0, 60),
                _rng.RandfRange(0, 150) - _rng.RandfRange(0, 200));
        AddSibling(lightStar);
    }

    public void OnEditPressed() {
        // Todo: Edit 界面跳转
        GD.Print("Todo: Edit 界面跳转");
    }
    public void OnUploadDownloadPressed() {
        OS.ShellOpen("https://download.marioforever.net/mw-levels.html");
    }
    public void OnCreateScenarioPressed() {
        // Todo: Create Scenario 功能
        GD.Print("Todo: Create Scenario 功能");
    }
    public void OnPlayLevelPressed() {
        // Todo: Play Level 跳转
        GD.Print("Todo: Play Level 跳转");
    }
    public void OnPlayScenarioPressed() {
        // Todo: Play Scenario 跳转
        GD.Print("Todo: Play Scenario 跳转");
    }
    public void OnQuitPressed() {
        GetTree().Quit();
    }
    
    public void OnHomePressed() {
        OS.ShellOpen("https://www.marioforever.net/");
    }
    public void OnHelpPressed() {
        OS.ShellOpen("https://zh.wiki.marioforever.net/wiki/%E9%A6%96%E9%A1%B5");
    }
    
    public void JumpToScene(String sceneUid) {
        GetTree().ChangeSceneToPacked(GD.Load<PackedScene>(sceneUid));
    }
    
}
