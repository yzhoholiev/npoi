using NPOI.OpenXml4Net.Exceptions;
using System.IO;
using NPOI.OpenXml4Net.OPC.Internal;

namespace NPOI.OpenXml4Net.OPC;

public class TempFileZipPackage : ZipPackage
{
    public TempFileZipPackage()
    {
    }

    public TempFileZipPackage(Stream in1, PackageAccess access)
        : base(in1, access)
    {
    }

    protected override PackagePart CreatePartImpl(PackagePartName partName, string contentType, bool loadRelationships)
    {
        try
        {
            return new TempFilePackagePart(
                this,
                partName,
                contentType,
                loadRelationships);
        }
        catch (InvalidFormatException)
        {
            return null;
        }
    }
}
