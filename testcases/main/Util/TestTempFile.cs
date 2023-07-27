﻿using NPOI.Util;
using NUnit.Framework;
using System.IO;

namespace TestCases.Util
{
    /// <summary>
    /// Tests of creating temp files
    /// </summary>
    [TestFixture]
    [SingleThreaded]
    internal class TestTempFile
    {
        [Test]
        public void TestCreateTempFile()
        {
            FileInfo fileInfo = null;
            Assert.DoesNotThrow(() => fileInfo = TempFile.CreateTempFile("test", ".xls"));

            Assert.IsTrue(File.Exists(fileInfo.FullName));

            string tempDirPath = Path.GetDirectoryName(fileInfo.FullName);

            if (Directory.Exists(tempDirPath))
                Directory.Delete(tempDirPath, true);
            
            Assert.IsFalse(File.Exists(fileInfo.FullName));
            Assert.IsFalse(Directory.Exists(tempDirPath));

            Assert.DoesNotThrow(() => TempFile.CreateTempFile("test2", ".xls"));
            Assert.IsTrue(Directory.Exists(tempDirPath));
        }

        [Test]
        public void TestGetTempFilePath()
        {
            string path = "";
            Assert.DoesNotThrow(() => path = TempFile.GetTempFilePath("test", ".xls"));

            Assert.IsTrue(!string.IsNullOrWhiteSpace(path));

            string tempDirPath = Path.GetDirectoryName(path);

            if (Directory.Exists(tempDirPath))
                Directory.Delete(tempDirPath, true);

            Assert.IsFalse(Directory.Exists(tempDirPath));

            Assert.DoesNotThrow(() => TempFile.GetTempFilePath("test", ".xls"));
            Assert.IsTrue(Directory.Exists(tempDirPath));
        }
    }
}
