using System;
using System.IO;
using System.Linq;
using NPOI.OpenXml4Net.Exceptions;
using NPOI.OpenXml4Net.OPC.Internal;

namespace NPOI.OpenXml4Net.OPC;

/// <summary>
/// A zip package that keeps the content of new and changed parts in temporary files instead of memory.
/// </summary>
public class TempFileZipPackage : ZipPackage, IDisposable
{
    public TempFileZipPackage()
    {
    }

    public TempFileZipPackage(Stream in1, PackageAccess access)
        : base(in1, access)
    {
    }

    public void Dispose()
    {
        if (partList is null)
        {
            return;
        }

        foreach (TempFilePackagePart part in partList.Values.OfType<TempFilePackagePart>())
        {
            part.Clear();
        }
    }

    protected override PackagePart CreatePartImpl(PackagePartName partName, string contentType, bool loadRelationships)
    {
        if (contentType == null)
        {
            throw new ArgumentException("contentType");
        }

        if (partName == null)
        {
            throw new ArgumentException("partName");
        }

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
