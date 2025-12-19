using System;
using Godot;

namespace SMWP.Level.Data;

/// <summary>
/// 编解码旧版 smwp 的压缩数据的方法，
/// 包括但不限于大于 9999 的四位数坐标（A000 - z999）等。
/// </summary>
public static class ClassicSmwpCodec {
    public static bool TryDecodeCoordinate(ReadOnlySpan<char> serialized, out Vector2I result) {
        if (serialized.Length != 8) {
            return OnTriageError($"Classic SMWP coordinate must be 8 characters length, given {serialized}", out result, Vector2I.Zero);
        }
        if (TryDecodeCoordinateValue(serialized[..4], out int x) &&
            TryDecodeCoordinateValue(serialized[4..], out int y)) {
            result = new Vector2I(x, y);
            return true;
        }
        result = Vector2I.Zero;
        return false;
    }

    public static bool TryEncodeCoordinate(Span<char> buffer, Vector2I value) {
        if (buffer.Length != 8) {
            GD.PushError($"Classic SMWP coordinate must be 8 characters length, given buffer size: {buffer.Length}");
            return false;
        }
        return TryEncodeCoordinateValue(buffer[..4], value.X) && TryEncodeCoordinateValue(buffer[4..], value.Y);
    }

    public static bool TryDecodeCoordinateValue(ReadOnlySpan<char> serialized, out int result) {
        if (serialized.Length != 4) {
            return OnTriageError($"Classic SMWP coordinate value must be 4 characters length, given {serialized}", out result);
        }
        // 负数坐标的情况，
        // 不支持字母，-A00 这样的坐标会直接报错
        if (serialized[0] == '-') {
            return int.TryParse(serialized[1..], out result) || OnTriageError($"Unsupported negative coordinate value: {serialized}", out result);
        }
        // 计算余 1000 的部分
        if (!int.TryParse(serialized[1..], out int tail)) {
            return OnTriageError($"Found non-number digit after first character when decoding coordinate value {serialized}", out result);
        }
        // 计算最高位
        int head = Base62Decode(serialized[0]);
        
        result = head * 1000 + tail;
        return true;
    }

    public static bool TryEncodeCoordinateValue(Span<char> buffer, int value) {
        if (buffer.Length != 4) {
            GD.PushError($"Classic SMWP coordinate value must be 4 characters length, given buffer size: {buffer.Length}");
            return false;
        }
        // 编码负数坐标
        // 不支持字母，-A00 这样的坐标会直接报错
        if (value < 0) {
            if (value <= -1000) {
                GD.PushError($"Negative coordinate value <= -1000 is not supported, found {value}");
                return false;
            }
            buffer[0] = '-';
            $"{-value:D3}".CopyTo(buffer[1..]);
            return true;
        }
        // 计算最高位，顺便计算余 1000 的部分（tail）
        int head = Base62Encode(Math.DivRem(value, 1000, out int tail));
        if (head < 0) {
            return false;
        }
        
        buffer[0] = (char) head;
        $"{tail:D3}".CopyTo(buffer[1..]);
        return true;
    }

    /// <summary>
    /// 将一个字符解码为 0 ~ 62 范围内的数值。
    /// </summary>
    /// <param name="char">待解码的字符</param>
    /// <returns>解码后的数值</returns>
    public static int Base62Decode(char @char) {
        return @char switch {
            >= '0' and <= '9' => @char - '0',
            >= 'A' and <= 'Z' => @char - 'A' + 10,
            >= 'a' and <= 'z' => @char - 'a' + 36,
            _ => OnError<int>($"Unsupported base62 character: {@char}"),
        };
    }

    /// <summary>
    /// 将 0 ~ 62 范围内的数值编码为一个字符。
    /// </summary>
    /// <param name="value">待编码的数值</param>
    /// <returns>编码后的字符，遇到错误时返回 -1</returns>
    public static int Base62Encode(int value) {
        return (char) (value switch {
            >= 0 and < 10 => '0' + value,
            >= 10 and < 36 => 'A' + value - 10,
            >= 36 and < 62 => 'a' + value - 36,
            _ => OnError($"Base62 value {value} out of range", -1),
        });
    }

    private static T OnError<T>(string message, T fallback = default) where T : struct {
        GD.PushError(message);
        return fallback;
    }

    private static bool OnTriageError<T>(string message, out T fallbackOutput, T fallback = default) where T : struct {
        GD.PushError(message);
        fallbackOutput = fallback;
        return false;
    }
}