using Godot;
using System;
using System.IO;
using System.Text;

public static partial class DecryptKey
{
    // 密钥变量
    public static string GameId = "";
    public static string CryptGmidInit = "";
    public static string KeyStr = "";

    // 密钥文件名称（固定）
    private const string KeyFileName = "smwp1_decrypt_key.txt";

    /// <summary>
    /// 从游戏程序同目录读取密钥文件（GB18030编码）
    /// 自动注册 GB18030 编码提供程序，确保读取正常
    /// </summary>
    public static void GetKey() {
        // 初始化为空字符串
        GameId = "";
        CryptGmidInit = "";
        KeyStr = "";

        try {
            // 1. 注册 GB18030 编码提供程序（防止编码不支持）
            RegisterGb18030Encoding();

            // 2. 获取游戏程序同目录的密钥文件路径
            string keyFilePath = GetKeyFilePath();
            if (!File.Exists(keyFilePath)) {
                GD.PrintErr($"密钥文件不存在：{keyFilePath}");
                return;
            }

            // 3. 以 GB18030 编码读取文件所有行
            Encoding gb18030 = Encoding.GetEncoding("GB18030");
            string[] lines = File.ReadAllLines(keyFilePath, gb18030);

            // 4. 解析行数据（仅赋值非空行，无默认值）
            // 第一行：GameId
            if (lines.Length >= 1)
                GameId = lines[0].Trim();
            // 第二行：CryptGmidInit
            if (lines.Length >= 2)
                CryptGmidInit = lines[1].Trim();
            // 第三行：KeyStr（保留换行/空格等特殊字符，仅Trim首尾）
            if (lines.Length >= 3)
                KeyStr = lines[2].TrimEnd('\r', '\n').TrimStart();
            
            GD.Print($"密钥读取成功！");
            GD.Print($"GameId: {GameId}");
            GD.Print($"CryptGmidInit: {CryptGmidInit}");
            GD.Print($"KeyStr: {KeyStr}");
        }
        catch (Exception ex) {
            GD.PrintErr($"读取密钥文件失败：{ex.Message}");
            GD.PrintErr($"异常详情：{ex.StackTrace}");
            // 读取失败保持空字符串
        }
    }

    /// <summary>
    /// 获取游戏程序同目录的密钥文件路径
    /// 优先使用 Godot 的执行目录，兼容不同部署场景
    /// </summary>
    private static string GetKeyFilePath() {
        string? exeDir = Path.GetDirectoryName(OS.GetExecutablePath());
        return Path.Combine(exeDir!, KeyFileName);
    }

    /// <summary>
    /// 注册GB18030编码提供程序（解决编码不支持问题）
    /// </summary>
    private static void RegisterGb18030Encoding() {
        try {
            // 需确保已安装System.Text.Encoding.CodePages NuGet包
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
        catch (Exception ex) {
            GD.PushError($"注册 GB18030 编码失败：{ex.Message}");
        }
    }
}