using System.Collections.Generic;
using System.IO;
using NPOI.OpenXml4Net.OPC.Internal.Marshallers;

namespace NPOI.OpenXml4Net.OPC.Internal;

public class TempFilePackagePart : PackagePart
{
    private const int BufferSize = 8192;

    private readonly List<Stream> _readers = [];

    internal FileStream _data;

    public TempFilePackagePart(OPCPackage pack, PackagePartName partName, string contentType)
        : base(pack, partName, contentType)
    {
    }

    public TempFilePackagePart(OPCPackage pack, PackagePartName partName, string contentType, bool loadRelationships)
        : base(pack, partName, new ContentType(contentType), loadRelationships)
    {
    }

    protected override Stream GetInputStreamImpl()
    {
        if (_data is null)
        {
            return new MemoryStream();
        }

        _data.Flush();

        // Opens a second handle on the same file, so each reader gets its own position and no copy is made.
        var reader = new FileStream(_data.Name, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, BufferSize);

        // ZipPartMarshaller never disposes the stream it reads, and Windows deletes the file only once every handle is closed.
        _readers.Add(reader);

        return reader;
    }

    protected override Stream GetOutputStreamImpl()
    {
        return new TempFilePackagePartOutputStream(this);
    }

    public override long Size => _data?.Length ?? 0;

    public override void Clear()
    {
        foreach (Stream reader in _readers)
        {
            reader.Dispose();
        }

        _readers.Clear();
        _data?.Dispose();
        _data = null;
    }

    public override bool Save(Stream zos)
    {
        return new ZipPartMarshaller().Marshall(this, zos);
    }

    public override bool Load(Stream ios)
    {
        StreamHelper.CopyStream(ios, ReplaceData());

        return true;
    }

    public override void Close()
    {
        // Do nothing
    }

    public override void Flush()
    {
        // Do nothing
    }

    internal FileStream ReplaceData()
    {
        Clear();

        string fileName = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        _data = new FileStream(fileName, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read | FileShare.Delete, BufferSize, FileOptions.DeleteOnClose);

        return _data;
    }
}
