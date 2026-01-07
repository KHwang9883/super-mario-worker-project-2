using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SMWP.Level;

public static class LoaderHelper {
    public static bool IsBinaryFile(Stream smwl) {
        if (!smwl.CanSeek) {
            throw new NotSupportedException("Unseekable file stream: unable to detect smwl type");
        }
        var originalOffset = smwl.Position;
        using var reader = new StreamReader(new GZipStream(smwl, CompressionMode.Decompress, true));
        bool result;
        try {
            reader.Read();
            result = true;
        } catch (InvalidDataException) {
            result = false;
        }
        smwl.Seek(originalOffset, SeekOrigin.Begin);
        return result;
    }

    public static async ValueTask<string?> PeekLineAsync(TextReader reader) {
        if (reader is not StreamReader sr) {
            throw new NotSupportedException($"Only {nameof(StreamReader)}s can be peeked");
        }
        var stream = sr.BaseStream;
        var pos = stream.Position;
        var line = await reader.ReadLineAsync();
        stream.Seek(pos, SeekOrigin.Begin);
        return line;
    }

    public static Encoding GetGb18030() {
        lock (Lock) {
            if (_gb18030 != null) {
                return _gb18030;
            }
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            return _gb18030 = Encoding.GetEncoding("gb18030");
        }
    }

    private static readonly Lock Lock = new();
    private static volatile Encoding? _gb18030;
}