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
        var headCharacter = serialized[0];
        int head = headCharacter switch {
            >= '0' and <= '9' => headCharacter - '0',
            >= 'A' and <= 'Z' => headCharacter - 'A' + 10,
            >= 'a' and <= 'z' => headCharacter - 'a' + 36,
            _ => OnError<int>($"Unsupported first character: {headCharacter} when decoding coordinate value {serialized}"),
        };
        
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
        int head = Math.DivRem(value, 1000, out int tail);
        int headCharacter = head switch {
            >= 0 and < 10 => '0' + head,
            >= 10 and < 36 => 'A' + head - 10,
            >= 36 and < 62 => 'a' + head - 36,
            _ => OnError("Coordinate value out of range, must be >= 0 and < 62000", -1),
        };
        if (headCharacter < 0) {
            return false;
        }
        
        buffer[0] = (char) headCharacter;
        $"{tail:D3}".CopyTo(buffer[1..]);
        return true;
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