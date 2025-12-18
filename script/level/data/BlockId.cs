using System;
using System.Runtime.InteropServices;
using Godot;

namespace SMWP.Level.Data;

/// <summary>
/// 代表一个 BlocksData 中的两位数方块 id。
/// </summary>
/// <param name="Char0">方块 id 的第一个字符</param>
/// <param name="Char1">方块 id 的第二个字符</param>
[StructLayout(LayoutKind.Explicit)]
public readonly record struct BlockId(
    [field: FieldOffset(0)] byte Char0,
    [field: FieldOffset(8)] byte Char1
) {
    public BlockId(char ch0, char ch1) : this((byte)ch0, (byte)ch1) {
    }

    public static bool TryParse(ReadOnlySpan<char> idString, out BlockId result) {
        if (idString.Length != 2) {
            goto Error;
        }
        for (int i = 0; i < 2; i++) {
            var @char = idString[i];
            if (!char.IsAscii(@char) || char.IsControl(@char)) {
                goto Error;
            }
        }
        result = new BlockId(idString[0], idString[1]);
        return true;
        
        Error:
        GD.PrintErr($"Invalid block id: {idString}");
        result = new BlockId('0', '0');
        return false;
    }

    public override string ToString() {
        return new string(['B', 'l', 'o', 'c', 'k', ' ', (char)Char0, (char)Char1]);
    }
}