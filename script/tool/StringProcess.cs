using Godot;
using System;

public static class StringProcess {
    public static string ConvertHashAndNewline(string input) {
        if (string.IsNullOrEmpty(input)) return input;

        const string tempPlaceholder = "☃";
        var step1 = input.Replace(@"\#", tempPlaceholder);
        var step2 = step1.Replace("#", "\n");
        var result = step2.Replace(tempPlaceholder, "#");
        return result;
    }
    public static bool IsWideCharacter(char c) {
        // 基于Unicode范围的宽字符判断
        var codePoint = (int)c;
    
        // 中文、日文、韩文等宽字符范围
        if (codePoint is >= 0x4E00 and <= 0x9FFF ||    // CJK统一表意文字
            codePoint is >= 0x3040 and <= 0x309F ||    // 平假名
            codePoint is >= 0x30A0 and <= 0x30FF ||    // 片假名
            codePoint is >= 0xAC00 and <= 0xD7AF ||    // 韩文
            codePoint is >= 0xFF00 and <= 0xFFEF)      // 全角字符
        {
            return true;
        }
    
        // 全角标点符号等
        if (codePoint is >= 0x3000 and <= 0x303F ||    // CJK符号和标点
            codePoint is >= 0xFE10 and <= 0xFE1F ||    // 竖排形式
            codePoint is >= 0xFE30 and <= 0xFE4F)      // CJK兼容形式
        {
            return true;
        }
    
        return false;
    }
}
