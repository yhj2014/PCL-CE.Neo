using System.IO;
using System.Text;

namespace PCL_CE.Neo.Core.Utils;

public class StringStream : Stream
{
    private readonly MemoryStream _stream;
    private readonly Encoding _encoding;

    public StringStream(Encoding? encoding = null)
    {
        _encoding = encoding ?? Encoding.UTF8;
        _stream = new MemoryStream();
    }

    public StringStream(string content, Encoding? encoding = null)
    {
        _encoding = encoding ?? Encoding.UTF8;
        _stream = new MemoryStream(_encoding.GetBytes(content));
    }

    public override bool CanRead => _stream.CanRead;
    public override bool CanSeek => _stream.CanSeek;
    public override bool CanWrite => _stream.CanWrite;
    public override long Length => _stream.Length;

    public override long Position
    {
        get => _stream.Position;
        set => _stream.Position = value;
    }

    public override void Flush()
    {
        _stream.Flush();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        return _stream.Read(buffer, offset, count);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        return _stream.Seek(offset, origin);
    }

    public override void SetLength(long value)
    {
        _stream.SetLength(value);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        _stream.Write(buffer, offset, count);
    }

    public string GetString()
    {
        var position = _stream.Position;
        _stream.Position = 0;
        var bytes = new byte[_stream.Length];
        _stream.Read(bytes, 0, bytes.Length);
        _stream.Position = position;
        return _encoding.GetString(bytes);
    }

    public void WriteString(string content)
    {
        var bytes = _encoding.GetBytes(content);
        _stream.Write(bytes, 0, bytes.Length);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _stream.Dispose();
        base.Dispose(disposing);
    }
}