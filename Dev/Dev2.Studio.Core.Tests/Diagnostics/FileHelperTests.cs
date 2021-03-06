
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Diagnostics.CodeAnalysis;
using System.IO;
using Dev2.Studio.Core.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests.Diagnostics
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class FileHelperTests
    {
        private static string NewPath;
        private static string OldPath;
        static TestContext Context;

        [ClassInitialize]
        public static void ClassInit(TestContext testContext)
        {
            Context = testContext;
            NewPath = Context.TestDir + @"\Warewolf\";
            OldPath = Context.TestDir + @"\Dev2\";
        }

        #region Migrate Temp Data

        #endregion

        #region Create Directory from String

        [TestMethod]
        [TestCategory("StringExtensionUnitTest")]
        [Description("Test for FileHelper's CreateDirectoryFromString method: A valid file directory is passed to it and that files directory is created")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void FileHelper_FileHelperUnitTest_CreateDirectoryFromString_DirectoryCreated()
        // ReSharper restore InconsistentNaming
        {
            //init
            var fileDir = Context.TestDir + "\\Sub Directory\\some file name.ext";

            //exe
            FileHelper.CreateDirectoryFromString(fileDir);

            //assert
            Assert.IsTrue(Directory.Exists(Context.TestDir + "\\Sub Directory"));
        }

        #endregion
    }
}
