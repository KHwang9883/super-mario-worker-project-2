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
        var pos = smwl.Position;
        using var reader = new StreamReader(new GZipStream(smwl, CompressionMode.Decompress, true));
        bool result;
        try {
            reader.Read();
            result = true;
        } catch (InvalidDataException) {
            result = false;
        }
        smwl.Seek(pos - smwl.Position, SeekOrigin.Current);
        return result;
    }

    public static Stream MakeBuffered(Stream original) {
        return original as BufferedStream ?? new BufferedStream(original);
    }

    public static async ValueTask<Stream> MakeSeekableAsync(Stream original) {
        if (original.CanSeek) {
            return original;
        }
        using var buf = new MemoryStream();
        await original.CopyToAsync(buf);
        return new MemoryStream(buf.GetBuffer(), 0, (int)buf.Length);
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