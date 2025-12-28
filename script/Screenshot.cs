using Godot;
using System;
using System.IO;

public partial class Screenshot : Node {
    private Image _frozenScreenImage = new();
    
    public override void _Ready() {
        RenderingServer.FramePostDraw += DrawFreeze;
    }

    public void DrawFreeze() {
        // 保存游戏截图
        if (Input.IsActionJustPressed("screenshot")) {
            var viewport = GetTree().Root.GetViewport();
            var viewportTexture = viewport.GetTexture();
            _frozenScreenImage = viewportTexture.GetImage();
            SavePngFile();
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
            if (!fileName.StartsWith(baseName)) {
                continue;
            }
            string numberPart = fileName.Substring(baseName.Length);
            if (!int.TryParse(numberPart, out int number)) {
                continue;
            }
            if (number > maxNumber) {
                maxNumber = number;
            }
        }
        
        // 下一个编号
        int nextNumber = maxNumber + 1;
        string filePath = Path.Combine(directory, $"{baseName}{nextNumber}.png");
        
        _frozenScreenImage.SavePng(filePath);
        
        //GD.Print($"截图已保存: {filePath}");
    }
}
