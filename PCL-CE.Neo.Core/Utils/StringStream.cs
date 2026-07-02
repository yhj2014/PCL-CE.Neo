using System;
using System.IO;
using System.Text;

namespace PCL_CE.Neo.Core.Utils;

/// <summary>
/// 字符串流，用于高效地将字符串转换为流进行读取
/// </summary>
public class StringStream : Stream
{
    private readonly string _source;
    private readonly Encoding _encoding;
    private int _position;
    private readonly byte[] _buffer;
    private readonly int _byteLength;

    /// <summary>
    /// 创建字符串流
    /// </summary>
    /// <param name="source">源字符串</param>
    /// <param name="encoding">编码方式（默认UTF-8）</param>
    public StringStream(string source, Encoding? encoding = null)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _encoding = encoding ?? Encoding.UTF8;
        _buffer = _encoding.GetBytes(_source);
        _byteLength = _buffer.Length;
        _position = 0;
    }

    /// <summary>
    /// 是否可读
    /// </summary>
    public override bool CanRead => true;

    /// <summary>
    /// 是否可寻
    /// </summary>
    public override bool CanSeek => true;

    /// <summary>
    /// 是否可写
    /// </summary>
    public override bool CanWrite => false;

    /// <summary>
    /// 流长度
    /// </summary>
    public override long Length => _byteLength;

    /// <summary>
    /// 当前位置
    /// </summary>
    public override long Position
    {
        get => _position;
        set
        {
            if (value < 0 || value > _byteLength)
                throw new ArgumentOutOfRangeException(nameof(value));
            _position = (int)value;
        }
    }

    /// <summary>
    /// 读取数据
    /// </summary>
    public override int Read(byte[] buffer, int offset, int count)
    {
        if (buffer == null) throw new ArgumentNullException(nameof(buffer));
        if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
        if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
        if (offset + count > buffer.Length) throw new ArgumentException("缓冲区太小");

        var remaining = _byteLength - _position;
        var toRead = Math.Min(count, remaining);
        
        if (toRead == 0) return 0;

        Buffer.BlockCopy(_buffer, _position, buffer, offset, toRead);
        _position += toRead;
        
        return toRead;
    }

    /// <summary>
    /// 寻址
    /// </summary>
    public override long Seek(long offset, SeekOrigin origin)
    {
        var newPosition = origin switch
        {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => _position + offset,
            SeekOrigin.End => _byteLength + offset,
            _ => throw new ArgumentException("无效的寻址起点", nameof(origin))
        };

        if (newPosition < 0 || newPosition > _byteLength)
            throw new ArgumentOutOfRangeException(nameof(offset));

        _position = (int)newPosition;
        return _position;
    }

    /// <summary>
    /// 设置长度（不支持）
    /// </summary>
    public override void SetLength(long value)
    {
        throw new NotSupportedException("StringStream 不支持设置长度");
    }

    /// <summary>
    /// 写入数据（不支持）
    /// </summary>
    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException("StringStream 不支持写入");
    }

    /// <summary>
    /// 刷新流
    /// </summary>
    public override void Flush()
    {
        // 无操作（只读流）
    }

    /// <summary>
    /// 获取剩余未读取的字节数
    /// </summary>
    public int RemainingBytes => _byteLength - _position;

    /// <summary>
    /// 是否已读取完毕
    /// </summary>
    public bool IsEndOfStream => _position >= _byteLength;
}