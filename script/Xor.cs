using Godot;
using System;
using System.IO;
using System.Text;

namespace SMWP;

public static class Xor
{
    // 静态构造函数：注册 GB18030 编码（全局只执行一次）
    static Xor() {
        try {
            // 注册 GB18030 / GBK 等编码提供程序
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            GD.Print("GB18030 编码提供程序注册成功");
        }
        catch (Exception ex) {
            GD.PrintErr($"注册编码提供程序失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 模拟 GML 的 string_char_at 函数（索引从 1 开始，超出则循环）
    /// </summary>
    private static char StringCharAtGml(string s, int pos) {
        if (string.IsNullOrEmpty(s))
            return '\0';
        
        pos = (pos - 1) % s.Length + 1;
        return s[pos - 1];
    }

    /// <summary>
    /// 模拟 GML 的 string_char_at 函数（针对字节数组，索引从 1 开始，超出则循环）
    /// </summary>
    private static byte StringCharAtBytes(byte[] b, int pos) {
        if (b == null || b.Length == 0)
            return 0;
        
        pos = (pos - 1) % b.Length + 1;
        return b[pos - 1];
    }

    /// <summary>
    /// 私有核心方法：生成 XOR 加密密钥（复用逻辑）
    /// </summary>
    private static string GenerateCryptKey(string keyStr, string gameId, string cryptGmidInit, string encodingName) {
        // 安全获取 GB18030 编码（兼容容错）
        Encoding encoding = GetEncodingSafe(encodingName);
        
        // 步骤1：处理 crypt_gmid（重复5次拼接自身）
        string cryptGmid = cryptGmidInit;
        for (int i = 0; i < 5; i++) {
            cryptGmid += cryptGmid;
        }

        // 步骤2：密钥编码转换
        byte[] keyBytes = encoding.GetBytes(keyStr);
        int keyLength = keyBytes.Length > 0 ? keyBytes.Length : 1;

        // 步骤3：初始化变量
        int cryptI = 0;
        int cryptKeypos = 0;
        StringBuilder cryptKey = new StringBuilder();
        int loopCount = (int)Math.Floor(gameId.Length * 5.0);

        // 步骤4：生成 crypt_key
        for (int i = 0; i < loopCount; i++) {
            int gmlCharPos = cryptI >= 1 ? cryptI : 1;
            char gmidChar = StringCharAtGml(cryptGmid, gmlCharPos);
            int gmidOrd = gmidChar != '\0' ? (int)gmidChar : 0;

            int keyCharPos = cryptKeypos >= 1 ? cryptKeypos : 1;
            byte keyOrd = StringCharAtBytes(keyBytes, keyCharPos);
            int subtractVal = (int)Math.Floor(cryptI / 3.0);
            int keyCalc = keyOrd - subtractVal;

            int xorResult = gmidOrd ^ keyCalc;
            cryptKey.Append((char)xorResult);

            cryptI++;
            cryptKeypos++;
            if (cryptKeypos > keyLength) {
                cryptKeypos = 1;
            }
        }

        return cryptKey.ToString();
    }

    /// <summary>
    /// 安全获取编码（兼容 GB18030 / GBK / UTF8）
    /// </summary>
    private static Encoding GetEncodingSafe(string encodingName) {
        try {
            return Encoding.GetEncoding(encodingName);
        }
        catch {
            // 降级为 UTF8（防止编码获取失败）
            GD.PushWarning($"无法获取编码 {encodingName}，降级为 UTF8");
            return Encoding.UTF8;
        }
    }

    /// <summary>
    /// 处理字节数组的 XOR 加解密
    /// </summary>
    public static byte[] ScriptTextCryptBytes(byte[] inputBytes, string keyStr, string gameId, string cryptGmidInit, string encodingName = "GB18030") {
        if (inputBytes == null || inputBytes.Length == 0)
            throw new ArgumentException("输入字节数组不能为空", nameof(inputBytes));

        // 生成加密密钥
        string cryptKey = GenerateCryptKey(keyStr, gameId, cryptGmidInit, encodingName);
        int cryptKeyLength = cryptKey.Length;
        int cryptKeypos = 0;

        // 创建输出数组（避免修改原数组）
        byte[] outputBytes = new byte[inputBytes.Length];

        // 逐字节 XOR 处理（与原逻辑一致）
        for (int i = 0; i < inputBytes.Length; i++)
        {
            // 读取当前字节
            int cryptRead = inputBytes[i];

            // 取 crypt_key 的字符值
            int ckPos = cryptKeypos >= 1 ? cryptKeypos : 1;
            char ckChar = StringCharAtGml(cryptKey, ckPos);
            int ckOrd = ckChar != '\0' ? (int)ckChar : 0;

            // XOR 运算
            byte xorByte = (byte)(cryptRead ^ ckOrd);
            outputBytes[i] = xorByte;

            // 更新密钥指针
            cryptKeypos++;
            if (cryptKeypos > cryptKeyLength) {
                cryptKeypos = 1;
            }
        }

        return outputBytes;
    }
}