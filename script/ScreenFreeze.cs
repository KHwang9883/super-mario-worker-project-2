using Godot;
using System;
using System.IO;

public partial class ScreenFreeze : CanvasLayer {
    [Signal]
    public delegate void ScreenFrozenEventHandler();

    [Export] private Sprite2D _frozenScreenSprite = null!;
    
    private Image _frozenScreenImage = new();
    private Vector2 _resolution = new Vector2(640f, 480f);
    private bool _visibility;

    public override void _Ready() {
        RenderingServer.FramePostDraw += DrawFreeze;
    }
    public override void _Process(double delta) {
        if (Visible && _visibility != Visible) {
            _frozenScreenSprite.Texture = ImageTexture.CreateFromImage(_frozenScreenImage);
            /*_frozenScreenSprite.Scale = new Vector2(640f, 480f) / _resolution;
            GD.Print(_resolution);*/
            EmitSignal(SignalName.ScreenFrozen);
        }
        _visibility = Visible;
    }

    public void DrawFreeze() {
        if (Input.IsActionJustPressed("screenshot")) {
            var viewport = GetTree().Root.GetViewport();
            var viewportTexture = viewport.GetTexture();
            _resolution = viewportTexture.GetSize();
            _frozenScreenImage = viewportTexture.GetImage();
            Visible = true;
            
            // 保存游戏截图
            SavePngFile();
            GD.Print(_frozenScreenImage);
        }
    }

    public void SavePngFile() {
        string directory = Path.GetDirectoryName(OS.GetExecutablePath())!;
        string baseName = "smwp2_screenshot_";
        
        // 获取目录中所有已有的截图文件
        string pattern = baseName + "*.png";
        string[] existingFiles = Directory.GetFiles(directory, pattern);
        
        int maxNumber = 0;
        
        // 提取已有的最大编号
        foreach (string file in existingFiles) {
            string fileName = Path.GetFileNameWithoutExtension(file);
            if (fileName.StartsWith(baseName)) {
                string numberPart = fileName.Substring(baseName.Length);
                if (int.TryParse(numberPart, out int number)) {
                    if (number > maxNumber) {
                        maxNumber = number;
                    }
                }
            }
        }
        
        // 下一个编号
        int nextNumber = maxNumber + 1;
        string filePath = Path.Combine(directory, $"{baseName}{nextNumber}.png");
        
        _frozenScreenImage.SavePng(filePath);
        
        GD.Print($"截图已保存: {filePath}");
    }

    // 异常的屏幕冻结时长（超时）
    public void SetInvisible() {
        Visible = false;
    }
}
