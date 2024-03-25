using System.IO;
using NPOI.OpenXml4Net.OPC.Internal.Marshallers;

namespace NPOI.OpenXml4Net.OPC.Internal;

public class TempFilePackagePart : PackagePart
{
    internal Stream _data;

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
        FileStream stream = FileHelper.GetTempFileStream();

        if (_data is null)
        {
            return stream;
        }

        StreamHelper.CopyStream(_data, stream);
        stream.Position = 0;

        return stream;
    }

    protected override Stream GetOutputStreamImpl()
    {
        return new TempFilePackagePartOutputStream(this);
    }

    public override long Size => _data?.Length ?? 0;

    public override void Clear()
    {
        _data?.Dispose();
        _data = null;
    }

    public override bool Save(Stream zos)
    {
        return new ZipPartMarshaller().Marshall(this, zos);
    }

    public override bool Load(Stream ios)
    {
        // Save it
        StreamHelper.CopyStream(ios, _data);
        // All done
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
}