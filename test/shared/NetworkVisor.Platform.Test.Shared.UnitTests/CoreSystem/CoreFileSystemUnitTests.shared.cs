// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// ***********************************************************************
// <copyright file="CoreFileSystemUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Text.Json;
using FluentAssertions;
using NetworkVisor.Core.CoreApp.Settings;
using NetworkVisor.Core.CoreSystem;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Networking.Hosting;
using NetworkVisor.Core.Serialization;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.CoreSystem
{
    /// <summary>
    /// Class CoreFileSystemUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreFileSystemUnitTests))]

    public class CoreFileSystemUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// The file system no test.
        /// </summary>
        private readonly ICoreFileSystem _fileSystemProduction;

        /// <summary>
        /// The is disposed.
        /// </summary>
        private bool isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreFileSystemUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreFileSystemUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
            ICoreAppSettings appSettingsProduction = CoreAppSettings.CreateFromAssemblyOrDefault(this.TestAssembly, null, CoreHostEnvironment.Production);
            this._fileSystemProduction = new CoreFileSystem(this.TestOperatingSystem, appSettingsProduction)
            {
                GlobalLogger = this.TestCaseLogger,
            };
        }

        [Fact]
        public void FileSystemUnit_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Core, TraitTestType.Unit);
        }

        /// <summary>
        /// Defines the test method Ctor.
        /// </summary>
        [Fact]
        public void FileSystemUnit_Ctor()
        {
            _ = this.TestOutputHelper.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestOutputHelper>();
            _ = this.TestFileSystem.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreFileSystem>();
        }

        /// <summary>
        /// Defines the test method FileSystemUnit_Serialize_TestFileSystem_Output.
        /// </summary>
        [Fact]
        public void FileSystemUnit_Serialize_TestFileSystem_Output()
        {
            _ = this.TestFileSystem.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreFileSystem>();
            string? jsonString = JsonSerializer.Serialize(this.TestFileSystem, CoreDefaultJsonSerializerOptions.GetDefaultJsonSerializerOptions(CoreSerializationFormatFlags.JsonFormatted, this.TestCaseServiceProvider));
            _ = jsonString.Should().NotBeNullOrEmpty();
            this.TestOutputHelper.WriteLine(jsonString);
        }

        /// <summary>
        /// Defines the test method FileSystemUnit_Serialize_FileSystem_Output.
        /// </summary>
        [Fact]
        public void FileSystemUnit_Serialize_FileSystem_Output()
        {
            _ = this.TestFileSystem.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreFileSystem>();
            string? jsonString = JsonSerializer.Serialize(this.TestFileSystem, CoreDefaultJsonSerializerOptions.GetDefaultJsonSerializerOptions(CoreSerializationFormatFlags.JsonFormatted, this.TestCaseServiceProvider));
            _ = jsonString.Should().NotBeNullOrEmpty();
            this.TestOutputHelper.WriteLine(jsonString);
        }

        /// <summary>
        /// Defines the test method FileSystemUnit_Serialize_FileSystemNoTest_Output.
        /// </summary>
        [Fact]
        public void FileSystemUnit_Serialize_FileSystemNoTest_Output()
        {
            _ = this.TestFileSystem.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreFileSystem>();
            string? jsonString = JsonSerializer.Serialize(this._fileSystemProduction, CoreDefaultJsonSerializerOptions.GetDefaultJsonSerializerOptions(CoreSerializationFormatFlags.JsonFormatted, this.TestCaseServiceProvider));
            _ = jsonString.Should().NotBeNullOrEmpty();
            this.TestOutputHelper.WriteLine(jsonString);
        }

        /// <summary>
        /// Defines the test method FileSystemUnit_Ctor_OperatingSystem_Null.
        /// </summary>
        [Fact]
        public void FileSystemUnit_Ctor_OperatingSystem_Null()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            Func<ICoreFileSystem> fx = () => new CoreFileSystem(null, null);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            _ = fx.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("operatingSystem");
        }

        /// <summary>
        /// Defines the test method FileSystemUnit_Ctor_AppSettings_Null.
        /// </summary>
        [Fact]
        public void FileSystemUnit_Ctor_AppSettings_Null()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            Func<ICoreFileSystem> fx = () => new CoreFileSystem(this.TestOperatingSystem, null);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            _ = fx.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("appSettings");
        }

        /// <summary>
        /// Defines the test method FileSystemUnit_AppFolderName.
        /// </summary>
        [Fact]
        public void FileSystemUnit_AppFolderName()
        {
            _ = this.TestFileSystem.AppSettings.AppFolderName.Should().NotBeNullOrWhiteSpace();
            this.TestOutputHelper.WriteLine($"AppFolderName: {this._fileSystemProduction.AppSettings.AppFolderName}");
            this.TestOutputHelper.WriteLine($"\nTestAppFolderName: {this.TestFileSystem.AppSettings.AppFolderName}");

            if (this.TestFileSystem.OperatingSystem.IsWindowsPlatform)
            {
                _ = this.TestFileSystem.AppSettings.AppFolderName.Should().NotContain("/", "AppFolder contains forward slash on Windows");
            }
            else
            {
                _ = this.TestFileSystem.AppSettings.AppFolderName.Should().NotContain("\\", "AppFolder contains back slash on non-Windows");
            }
        }

        /// <summary>
        /// Defines the test method FileSystemUnit_ThrowOnInvalidPath_Null.
        /// </summary>
        [Fact]
        public void FileSystemUnit_ThrowOnInvalidPath_Null()
        {
            Action act = () => this.TestFileSystem.CreateFolder(null);
            _ = act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("path");
        }

        /// <summary>
        /// Defines the test method FileSystemUnit_ThrowOnInvalidPath_Empty.
        /// </summary>
        [Fact]
        public void FileSystemUnit_ThrowOnInvalidPath_Empty()
        {
            Action act = () => this.TestFileSystem.CreateFolder(string.Empty);
            _ = act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("path");
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!this.isDisposed)
            {
                try
                {
                    if (disposing)
                    {
                        this._fileSystemProduction?.Dispose();
                    }
                }
                finally
                {
                    this.isDisposed = true;
                }
            }

            base.Dispose(disposing);
        }
    }
}
