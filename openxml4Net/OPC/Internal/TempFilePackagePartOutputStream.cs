using System.IO;
using System;

namespace NPOI.OpenXml4Net.OPC.Internal;

public sealed class TempFilePackagePartOutputStream : Stream
{
    private readonly Stream _buff;

    public TempFilePackagePartOutputStream(TempFilePackagePart part)
    {
        part._data = FileHelper.GetTempFileStream();
        _buff = part._data;
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
        throw new NotImplementedException();
    }

    public override void SetLength(long value)
    {
        _buff.SetLength(value);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        return _buff.Seek(offset, origin);
    }

    /**
     * Close this stream and flush the content.
     * @see #flush()
     */
    public override void Close()
    {
        Flush();
    }

    /**
     * Flush this output stream. This method is called by the close() method.
     * Warning : don't call this method for output consistency.
     * @see #close()
     */
    public override void Flush()
    {
        _buff.Flush();

        /*
         * Clear this streams buffer, in case flush() is called a second time
         * Fix bug 1921637 - provided by Rainer Schwarze
         */
        _buff.Position = 0;
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
        _buff.Write(buffer, (int)_buff.Position, buffer.Length);
    }
}