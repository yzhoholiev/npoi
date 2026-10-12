using System;
using System.IO;

namespace NPOI.OpenXml4Net.OPC.Internal;

public sealed class TempFilePackagePartOutputStream : Stream
{
    private readonly Stream _buff;

    public TempFilePackagePartOutputStream(TempFilePackagePart part)
    {
        _buff = part.ReplaceData();
    }

    public override bool CanRead => false;

    public override bool CanWrite => true;

    public override bool CanSeek => false;

    public override long Length => _buff.Length;

    public override long Position
    {
        get => _buff.Position;
        set => _buff.Position = value;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }

    public override void SetLength(long value)
    {
        _buff.SetLength(value);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        return _buff.Seek(offset, origin);
    }

    public override void Close()
    {
        Flush();
    }

    public override void Flush()
    {
        _buff.Flush();
    }

    public void Write(int b)
    {
        _buff.WriteByte((byte)b);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        _buff.Write(buffer, offset, count);
    }

    public void Write(byte[] buffer)
    {
        _buff.Write(buffer, 0, buffer.Length);
    }
}
