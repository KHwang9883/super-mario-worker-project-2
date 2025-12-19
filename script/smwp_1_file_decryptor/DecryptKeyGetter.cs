using Godot;
using System;
using System.Text;

namespace SMWP.Smwp1FileDecryptor;

public static partial class DecryptKeyGetter {
    // 项目内密钥资源路径
    private const string KeyResourcePath =
        "res://content/smwp_1_file_decryptor/key/smwp1_decrypt_key.txt";

    /// <summary>
    /// 从Godot项目资源中读取密钥文件（GB18030编码）
    /// 自动注册 GB18030 编码提供程序，确保读取正常
    /// </summary>
    public static void GetKey() {
        try {
            // 1. 注册 GB18030 编码提供程序（防止编码不支持）
            RegisterGb18030Encoding();

            // 2. 检查资源是否存在
            if (!FileAccess.FileExists(KeyResourcePath)) {
                GD.Print($"密钥资源不存在：{KeyResourcePath}");
                return;
            }

            // 3. 读取资源
            string fileContent = ReadTextFileFromResource(KeyResourcePath, "GB18030");
            if (string.IsNullOrEmpty(fileContent)) {
                GD.Print($"读取密钥资源内容为空：{KeyResourcePath}");
                return;
            }

            // 4. 解析行数据（仅赋值非空行，无默认值）
            string[] lines = fileContent.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
            
            // 第一行：GameId
            if (lines.Length >= 1)
                DecryptKey.GameId = lines[0].Trim();
            // 第二行：CryptGmidInit
            if (lines.Length >= 2)
                DecryptKey.CryptGmidInit = lines[1].Trim();
            // 第三行：KeyStr（保留换行/空格等特殊字符，仅Trim首尾）
            if (lines.Length >= 3)
                DecryptKey.KeyStr = lines[2].TrimEnd('\r', '\n').TrimStart();
        }
        catch (Exception ex) {
            GD.PushError($"读取密钥资源失败：{ex.Message}");
            // 读取失败保持空字符串
        }
    }

    /// <summary>
    /// 从Godot资源路径读取文本文件（指定编码）
    /// </summary>
    /// <param name="resourcePath">res:// 开头的资源路径</param>
    /// <param name="encodingName">编码名称（如GB18030/UTF8）</param>
    /// <returns>文件内容，读取失败返回空字符串</returns>
    private static string ReadTextFileFromResource(string resourcePath, string encodingName) {
        using (FileAccess file = FileAccess.Open(resourcePath, FileAccess.ModeFlags.Read)) {
            if (file == null) {
                GD.PrintErr($"无法打开文件：{resourcePath}");
                return string.Empty;
            }

            try {
                // 1. 读取所有字节
                byte[] fileBytes = file.GetBuffer((long)file.GetLength());

                // 2. 按指定编码解码
                Encoding encoding;
                try {
                    encoding = Encoding.GetEncoding(encodingName);
                }
                catch {
                    GD.PushWarning($"无法获取编码 {encodingName}，降级为UTF8");
                    encoding = Encoding.UTF8;
                }

                // 3. 移除可能的BOM（避免乱码）
                fileBytes = RemoveBomIfExists(fileBytes, encoding);

                // 4. 解码为字符串
                return encoding.GetString(fileBytes);
            }
            catch (Exception ex) {
                GD.PrintErr($"读取文件时出错：{ex.Message}");
                return string.Empty;
            }
        }
    }

    /// <summary>
    /// 移除字节流中的BOM（避免解码乱码）
    /// </summary>
    private static byte[] RemoveBomIfExists(byte[] bytes, Encoding encoding) {
        if (bytes.Length < encoding.GetPreamble().Length)
            return bytes;

        byte[] preamble = encoding.GetPreamble();
        bool hasBom = true;

        for (int i = 0; i < preamble.Length; i++) {
            if (bytes[i] != preamble[i]) {
                hasBom = false;
                break;
            }
        }

        if (!hasBom)
            return bytes;

        // 移除BOM
        byte[] result = new byte[bytes.Length - preamble.Length];
        Array.Copy(bytes, preamble.Length, result, 0, result.Length);
        return result;
    }

    /// <summary>
    /// 注册 GB18030 编码提供程序（解决编码不支持问题）
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