namespace TestCases.OpenXml4Net.OPC
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using NPOI.OpenXml4Net.OPC;
    using NPOI.OpenXml4Net.OPC.Internal;
    using NPOI.SS.UserModel;
    using NPOI.XSSF.UserModel;
    using NUnit.Framework;
    using NUnit.Framework.Legacy;

    [TestFixture]
    public class TestTempFileZipPackage
    {
        private static readonly FieldInfo DataField = typeof(TempFilePackagePart).GetField("_data", BindingFlags.Instance | BindingFlags.NonPublic);

        [Test]
        public void TestPartCanBeReadMoreThanOnce()
        {
            using TempFileZipPackage package = OpenEmptyWorkbookPackage();
            PackagePart part = CreatePart(package, Encoding.ASCII.GetBytes("part content"));

            ClassicAssert.AreEqual("part content", ReadAll(part));
            ClassicAssert.AreEqual("part content", ReadAll(part));
        }

        [Test]
        public void TestPictureReadBeforeWriteIsSaved()
        {
            byte[] picture = Enumerable.Range(0, 500).Select(i => (byte)i).ToArray();
            byte[] saved;

            using (TempFileZipPackage package = OpenEmptyWorkbookPackage())
            {
                var workbook = new XSSFWorkbook(package);
                workbook.AddPicture(picture, PictureType.PNG);
                ClassicAssert.AreEqual(picture, workbook.GetAllPictures().Cast<XSSFPictureData>().Single().Data);

                using var output = new MemoryStream();
                workbook.Write(output, true);
                saved = output.ToArray();
            }

            var reopened = new XSSFWorkbook(new MemoryStream(saved));
            ClassicAssert.AreEqual(picture, reopened.GetAllPictures().Cast<XSSFPictureData>().Single().Data);
        }

        [Test]
        public void TestWritingAgainReplacesContentAndDeletesPreviousTempFile()
        {
            using TempFileZipPackage package = OpenEmptyWorkbookPackage();
            PackagePart part = CreatePart(package, Encoding.ASCII.GetBytes("first content"));
            string firstFile = GetTempFileName(part);

            WriteAll(part, Encoding.ASCII.GetBytes("second"));

            ClassicAssert.AreEqual("second", ReadAll(part));
            ClassicAssert.IsFalse(File.Exists(firstFile));
        }

        [Test]
        public void TestFlushDuringWriteKeepsWrittenContent()
        {
            using TempFileZipPackage package = OpenEmptyWorkbookPackage();
            PackagePart part = CreatePart(package, Array.Empty<byte>());

            using (Stream output = part.GetOutputStream())
            {
                output.Write(Encoding.ASCII.GetBytes("abc"), 0, 3);
                output.Flush();
                output.Write(Encoding.ASCII.GetBytes("def"), 0, 3);
            }

            ClassicAssert.AreEqual("abcdef", ReadAll(part));
        }

        [Test]
        public void TestWriteWholeBufferAppendsAfterPreviousWrites()
        {
            using TempFileZipPackage package = OpenEmptyWorkbookPackage();
            PackagePart part = CreatePart(package, Array.Empty<byte>());

            using (var output = (TempFilePackagePartOutputStream)part.GetOutputStream())
            {
                output.Write(Encoding.ASCII.GetBytes("abc"));
                output.Write(Encoding.ASCII.GetBytes("de"));
            }

            ClassicAssert.AreEqual("abcde", ReadAll(part));
        }

        [Test]
        public void TestLoadCopiesContentIntoEmptyPart()
        {
            using TempFileZipPackage package = OpenEmptyWorkbookPackage();
            package.GetParts();
            PackagePart part = package.CreatePart(PackagingUriHelper.CreatePartName("/xl/media/loaded.bin"), "application/octet-stream");

            part.Load(new MemoryStream(Encoding.ASCII.GetBytes("loaded")));

            ClassicAssert.AreEqual("loaded", ReadAll(part));
        }

        [Test]
        public void TestDisposeDeletesTempFiles()
        {
            string tempFile;

            using (TempFileZipPackage package = OpenEmptyWorkbookPackage())
            {
                PackagePart part = CreatePart(package, Encoding.ASCII.GetBytes("part content"));
                tempFile = GetTempFileName(part);
                ClassicAssert.IsTrue(File.Exists(tempFile));
            }

            ClassicAssert.IsFalse(File.Exists(tempFile));
        }

        [Test]
        public void TestDisposeDeletesTempFilesOfClosedWorkbook()
        {
            TempFileZipPackage package = OpenEmptyWorkbookPackage();
            string[] tempFiles;

            using (var workbook = new XSSFWorkbook(package))
            {
                workbook.CreateSheet("Sheet").CreateRow(0).CreateCell(0).SetCellValue("value");
                workbook.Write(new MemoryStream(), true);
                tempFiles = package.GetParts().OfType<TempFilePackagePart>().Select(GetTempFileName).Where(name => name != null).ToArray();
            }

            ClassicAssert.IsNotEmpty(tempFiles);
            package.Dispose();
            ClassicAssert.IsEmpty(tempFiles.Where(File.Exists));
        }

        private static TempFileZipPackage OpenEmptyWorkbookPackage()
        {
            using var workbook = new XSSFWorkbook();
            using var content = new MemoryStream();
            workbook.Write(content, true);
            content.Position = 0;

            return new TempFileZipPackage(content, PackageAccess.READ_WRITE);
        }

        private static PackagePart CreatePart(OPCPackage package, byte[] content)
        {
            package.GetParts();
            PackagePart part = package.CreatePart(PackagingUriHelper.CreatePartName("/xl/media/test" + Guid.NewGuid().ToString("N") + ".bin"), "application/octet-stream");
            WriteAll(part, content);

            return part;
        }

        private static void WriteAll(PackagePart part, byte[] content)
        {
            using Stream output = part.GetOutputStream();
            output.Write(content, 0, content.Length);
        }

        private static string ReadAll(PackagePart part)
        {
            using Stream input = part.GetInputStream();
            using var reader = new StreamReader(input, Encoding.ASCII);

            return reader.ReadToEnd();
        }

        private static string GetTempFileName(PackagePart part)
        {
            return (DataField.GetValue(part) as FileStream)?.Name;
        }
    }
}
