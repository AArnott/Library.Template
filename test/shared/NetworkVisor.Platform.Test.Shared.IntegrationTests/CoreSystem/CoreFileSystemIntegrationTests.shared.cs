// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// ***********************************************************************
// <copyright file="CoreFileSystemIntegrationTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Diagnostics.CodeAnalysis;
using System.Text;
using FluentAssertions;
using NetworkVisor.Core.CoreApp.Settings;
using NetworkVisor.Core.CoreSystem;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Logging.LogProvider;
using NetworkVisor.Core.Networking.Hosting;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.CoreSystem
{
    /// <summary>
    /// Class CoreFileSystemIntegrationTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreFileSystemIntegrationTests))]

    public class CoreFileSystemIntegrationTests : CoreTestCaseBase
    {
        /// <summary>
        /// The file system no test.
        /// </summary>
        private readonly ICoreFileSystem _fileSystemProduction;

        /// <summary>
        /// The is disposed.
        /// </summary>
        private bool _isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreFileSystemIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreFileSystemIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
            ICoreAppSettings appSettingsProduction = CoreAppSettings.CreateFromAssemblyOrDefault(this.TestAssembly, null, CoreHostEnvironment.Production);
            this._fileSystemProduction = new CoreFileSystem(this.TestOperatingSystem, appSettingsProduction)
            {
                GlobalLogger = this.TestCaseLogger,
            };
        }

        /// <summary>
        /// Enum ValidatePropertyFlags.
        /// </summary>
        [Flags]
        public enum ValidatePropertyFlags
        {
            /// <summary>
            /// The no validation.
            /// </summary>
            NoValidation = 0,

            /// <summary>
            /// The not null.
            /// </summary>
            NotNull = 1 << 1,

            /// <summary>
            /// The not empty.
            /// </summary>
            NotEmpty = 1 << 2,

            /// <summary>
            /// The check folder exists.
            /// </summary>
            CheckFolderExists = 1 << 3,

            /// <summary>
            /// The check path exists.
            /// </summary>
            CheckPathExists = 1 << 4,

            /// <summary>
            /// The check folder writable.
            /// </summary>
            CheckFolderWritable = 1 << 5,

            /// <summary>
            /// The check test folder.
            /// </summary>
            CheckTestFolder = 1 << 7,

            /// <summary>
            /// Create folder.
            /// </summary>
            CreateFolder = 1 << 8,

            /// <summary>
            /// Check backslash for os.
            /// </summary>
            CheckBackslashForOS = 1 << 9,

            /// <summary>
            /// Not null or empty.
            /// </summary>
            NotNullEmpty = NotNull | NotEmpty,

            /// <summary>
            /// The folder exists.
            /// </summary>
            FolderExists = NotNullEmpty | CheckFolderExists | CheckBackslashForOS,

            /// <summary>
            /// The path exists.
            /// </summary>
            PathExists = NotNullEmpty | CheckPathExists | CheckBackslashForOS,

            /// <summary>
            /// The test folder exists.
            /// </summary>
            TestFolderExists = FolderExists | CheckTestFolder,

            /// <summary>
            /// The folder writable.
            /// </summary>
            FolderWritable = FolderExists | CheckFolderWritable,

            /// <summary>
            /// The test folder writable.
            /// </summary>
            TestFolderWritable = FolderWritable | CheckTestFolder,

            /// <summary>
            /// The create folder writable.
            /// </summary>
            CreateFolderWritable = FolderWritable | CreateFolder,

            /// <summary>
            /// The test create folder writable.
            /// </summary>
            TestCreateFolderWritable = CreateFolderWritable | CheckTestFolder,
        }

#pragma warning disable xUnit1013
        /// <summary>
        /// Validates a property of the file system based on the specified criteria.
        /// </summary>
        /// <param name="testOutputHelper">The test output helper used for logging test results.</param>
        /// <param name="testFileSystem">The file system instance being tested.</param>
        /// <param name="propertyName">The name of the property to validate.</param>
        /// <param name="fxProperty">A function that retrieves the value of the property to validate.</param>
        /// <param name="validatePropertyFlags">Flags specifying the validation criteria for the property.</param>
        /// <remarks>
        /// This method performs various checks on the property value, such as ensuring it is not null or empty,
        /// verifying its format based on the operating system, and checking its existence or writability.
        /// The results of the validation are logged using the provided <paramref name="testOutputHelper"/>.
        /// </remarks>
        [ExcludeFromCodeCoverage]
        public static void ValidateProperty(
            ICoreTestOutputHelper testOutputHelper,
            ICoreFileSystem testFileSystem,
            string propertyName,
            Func<string> fxProperty,
            ValidatePropertyFlags validatePropertyFlags)
        {
#pragma warning restore xUnit1013
            var property = fxProperty.Invoke();
            var exists = false;
            var sbResults = new StringBuilder();

            testFileSystem.GlobalLogger.IsNullLogger.Should().BeFalse();

            testOutputHelper.WriteLine();

            testOutputHelper.WriteLine($"[{propertyName}]\t[{validatePropertyFlags}]\t[{property ?? "[Null]"}]");

            if (validatePropertyFlags.HasFlag(ValidatePropertyFlags.NotNull))
            {
                property.Should().NotBeNull();
            }

            if (validatePropertyFlags.HasFlag(ValidatePropertyFlags.NotEmpty))
            {
                property.Should().NotBeEmpty();
            }

            // Return if property is null or empty
            if (!string.IsNullOrEmpty(property))
            {
                if (validatePropertyFlags.HasFlag(ValidatePropertyFlags.CheckBackslashForOS))
                {
                    if (testFileSystem.OperatingSystem.IsWindowsPlatform)
                    {
                        property.Should().NotContain($"/", "path contains forward slash on Windows");
                    }
                    else
                    {
                        property.Should().NotContain($"\\", "path contains back slash on Non-Windows");
                    }
                }

                if (validatePropertyFlags.HasFlag(ValidatePropertyFlags.CheckTestFolder))
                {
                    property.Should().Contain(testFileSystem.LocalUserAppDataFolderPath, "Missing Test artifacts in LocalUserAppDataFolderPath");
                }

                if (validatePropertyFlags.HasFlag(ValidatePropertyFlags.CheckFolderExists))
                {
                    exists = testFileSystem.FolderExists(property);

                    if (!exists && validatePropertyFlags.HasFlag(ValidatePropertyFlags.CreateFolder))
                    {
                        testFileSystem.CreateFolder(property);
                        exists = testFileSystem.FolderExists(property);
                    }

                    exists.Should().BeTrue($"folder {property} does not exist");
                }

                if (validatePropertyFlags.HasFlag(ValidatePropertyFlags.CheckPathExists))
                {
                    exists = testFileSystem.FileExists(property);
                    exists.Should().BeTrue($"file {property} does not exist");
                }

                sbResults.Append($"\t[Exists: {exists}]");

                if (validatePropertyFlags.HasFlag(ValidatePropertyFlags.CheckFolderWritable))
                {
                    var tempFileName = $"{Guid.NewGuid().ToStringNoDashes()}.tmp";
                    var tempFilePath = $"{property}{tempFileName}";

                    var folderDeletable = false;
                    bool folderWritable;

                    try
                    {
                        CoreFileSystemBase.WriteFileContentsAsync(tempFilePath, "Test").ConfigureAwait(false).GetAwaiter().GetResult();

                        folderWritable = testFileSystem.FileExists(tempFilePath);

                        if (folderWritable)
                        {
                            try
                            {
                                testFileSystem.DeleteFile(tempFilePath);
                                folderDeletable = !testFileSystem.FolderExists(tempFilePath);
                            }
                            catch (UnauthorizedAccessException)
                            {
                                folderDeletable = false;
                            }
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        folderWritable = false;
                        folderDeletable = false;
                    }
                    catch (IOException)
                    {
                        folderWritable = false;
                        folderDeletable = false;
                    }

                    sbResults.Append($"\t[Writable: {folderWritable}]\t[Deletable: {folderDeletable}]");
                }
            }

            testOutputHelper.WriteLine(sbResults.ToString());
        }

        [Fact]
        public void FileSystemIntegration_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Core, TraitTestType.Integration);
        }

        /// <summary>
        /// Defines the test method Ctor.
        /// </summary>
        [Fact]
        public void FileSystemIntegration_Ctor()
        {
            this.TestOutputHelper.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestOutputHelper>();
            this.TestFileSystem.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreFileSystem>();
        }

        /// <summary>
        /// Defines the test method FileSystemIntegration_SystemDirectorySeparator.
        /// </summary>
        [Fact]
        public void FileSystemIntegration_SystemDirectorySeparator()
        {
            this.TestFileSystem.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreFileSystem>();

            if (this.TestOperatingSystem.IsWindowsPlatform)
            {
                Path.DirectorySeparatorChar.Should().Be('\\');
                Path.AltDirectorySeparatorChar.Should().Be('/');
            }
            else
            {
                Path.DirectorySeparatorChar.Should().Be('/');
                Path.AltDirectorySeparatorChar.Should().Be('/');
            }
        }

        [Fact]
        public void FileSystemIntegration_TestFileSystem_LocalUserApp_OutputPaths()
        {
            this.TestFileSystem.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreFileSystem>();

            this.TestOutputHelper.WriteLine($"CurrentFolder:\t{this.TestFileSystem.CurrentFolder}");
            this.TestOutputHelper.WriteLine($"GetTempPath:\t{Path.GetTempPath()}");
            this.TestOutputHelper.WriteLine();

            this.TestOutputHelper.WriteLine($"GetLocalUserAppTempFolderPath:\t{this.TestFileSystem.GetLocalUserAppTempFolderPath()}");
            this.TestOutputHelper.WriteLine($"LocalUserAppTempFolderPath:\t{this.TestFileSystem.LocalUserAppTempFolderPath}");
            this.TestOutputHelper.WriteLine($"GetLocalUserAppTempFileName:\t{this.TestFileSystem.GetLocalUserAppTempFileName()}");
            this.TestOutputHelper.WriteLine();

            this.TestOutputHelper.WriteLine($"GetLocalUserAppDataFolderPath:\t{this.TestFileSystem.GetLocalUserAppDataFolderPath(null, Environment.SpecialFolderOption.None)}");
            this.TestOutputHelper.WriteLine($"LocalUserAppDataFolderPath:\t{this.TestFileSystem.LocalUserAppDataFolderPath}");
            this.TestOutputHelper.WriteLine();

            this.TestOutputHelper.WriteLine($"LocalUserAppTestArtifactsFolderPath:\t{this.TestFileSystem.LocalUserAppTestArtifactsFolderPath}");
            this.TestOutputHelper.WriteLine($"LocalUserAppDatabaseFolderPath:\t{this.TestFileSystem.LocalUserAppDatabaseFolderPath}");
            this.TestOutputHelper.WriteLine($"GetLocalUserAppDatabaseTempFolderPath:\t{this.TestFileSystem.GetLocalUserAppDatabaseTempFolderPath()}");
            this.TestOutputHelper.WriteLine($"LocalUserAppLogFolderPath:\t{this.TestFileSystem.LocalUserAppLogFolderPath}");
            this.TestOutputHelper.WriteLine($"LocalUserAppSettingsFolderPath:\t{this.TestFileSystem.LocalUserAppSettingsFolderPath}");
            this.TestOutputHelper.WriteLine($"LocalUserAppCertsFolderPath:\t{this.TestFileSystem.LocalUserAppCertsFolderPath}");
            this.TestOutputHelper.WriteLine();

            this.TestOutputHelper.WriteLine($"GetRoamingUserAppDataFolderPath:\t{this.TestFileSystem.GetRoamingUserAppDataFolderPath(null, Environment.SpecialFolderOption.None)}");
            this.TestOutputHelper.WriteLine($"RoamingUserAppDataFolderPath:\t{this.TestFileSystem.RoamingUserAppDataFolderPath}");
            this.TestOutputHelper.WriteLine();

            this.TestOutputHelper.WriteLine($"GetRoamingDefaultUserAppDataFolderPath:\t{this.TestFileSystem.GetRoamingDefaultUserAppDataFolderPath(null, Environment.SpecialFolderOption.None)}");
            this.TestOutputHelper.WriteLine($"RoamingDefaultUserAppDataFolderPath:\t{this.TestFileSystem.RoamingDefaultUserAppDataFolderPath}");
        }

        [Fact]
        public void FileSystemIntegration_TestFileSystem_App_OutputPaths()
        {
            this.TestFileSystem.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreFileSystem>();

            this.TestOutputHelper.WriteLine($"CurrentFolder:\t{this.TestFileSystem.CurrentFolder}");
            this.TestOutputHelper.WriteLine();
            this.TestOutputHelper.WriteLine($"AssemblyFolderPath:\t{this.TestFileSystem.AssemblyFolderPath}");
            this.TestOutputHelper.WriteLine($"ResourceFolderPath:\t{this.TestFileSystem.ResourceFolderPath}");
            this.TestOutputHelper.WriteLine($"EntryAssemblyPath:\t{this.TestFileSystem.EntryAssemblyPath}");
            this.TestOutputHelper.WriteLine($"ExecutingAssemblyPath:\t{this.TestFileSystem.ExecutingAssemblyPath}");
        }

        [Fact]
        public void FileSystemIntegration_ProductionFileSystem_LocalUserApp_OutputPaths()
        {
            this._fileSystemProduction.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreFileSystem>();

            this.TestOutputHelper.WriteLine($"CurrentFolder:\t{this._fileSystemProduction.CurrentFolder}");
            this.TestOutputHelper.WriteLine($"GetTempPath:\t{Path.GetTempPath()}");
            this.TestOutputHelper.WriteLine();

            this.TestOutputHelper.WriteLine($"GetLocalUserAppTempFolderPath:\t{this._fileSystemProduction.GetLocalUserAppTempFolderPath()}");
            this.TestOutputHelper.WriteLine($"LocalUserAppTempFolderPath:\t{this._fileSystemProduction.LocalUserAppTempFolderPath}");
            this.TestOutputHelper.WriteLine($"GetLocalUserAppTempFileName:\t{this._fileSystemProduction.GetLocalUserAppTempFileName()}");
            this.TestOutputHelper.WriteLine();

            this.TestOutputHelper.WriteLine($"GetLocalUserAppDataFolderPath:\t{this._fileSystemProduction.GetLocalUserAppDataFolderPath(null, Environment.SpecialFolderOption.None)}");
            this.TestOutputHelper.WriteLine($"LocalUserAppDataFolderPath:\t{this._fileSystemProduction.LocalUserAppDataFolderPath}");
            this.TestOutputHelper.WriteLine();

            this.TestOutputHelper.WriteLine($"LocalUserAppTestArtifactsFolderPath:\t{this._fileSystemProduction.LocalUserAppTestArtifactsFolderPath}");
            this.TestOutputHelper.WriteLine($"LocalUserAppDatabaseFolderPath:\t{this._fileSystemProduction.LocalUserAppDatabaseFolderPath}");
            this.TestOutputHelper.WriteLine($"GetLocalUserAppDatabaseTempFolderPath:\t{this._fileSystemProduction.GetLocalUserAppDatabaseTempFolderPath()}");
            this.TestOutputHelper.WriteLine($"LocalUserAppLogFolderPath:\t{this._fileSystemProduction.LocalUserAppLogFolderPath}");
            this.TestOutputHelper.WriteLine($"LocalUserAppSettingsFolderPath:\t{this._fileSystemProduction.LocalUserAppSettingsFolderPath}");
            this.TestOutputHelper.WriteLine($"LocalUserAppCertsFolderPath:\t{this._fileSystemProduction.LocalUserAppCertsFolderPath}");
            this.TestOutputHelper.WriteLine();

            this.TestOutputHelper.WriteLine($"GetRoamingUserAppDataFolderPath:\t{this._fileSystemProduction.GetRoamingUserAppDataFolderPath(null, Environment.SpecialFolderOption.None)}");
            this.TestOutputHelper.WriteLine($"RoamingUserAppDataFolderPath:\t{this._fileSystemProduction.RoamingUserAppDataFolderPath}");
            this.TestOutputHelper.WriteLine();

            this.TestOutputHelper.WriteLine($"GetRoamingDefaultUserAppDataFolderPath:\t{this._fileSystemProduction.GetRoamingDefaultUserAppDataFolderPath(null, Environment.SpecialFolderOption.None)}");
            this.TestOutputHelper.WriteLine($"RoamingDefaultUserAppDataFolderPath:\t{this._fileSystemProduction.RoamingDefaultUserAppDataFolderPath}");
        }

        [Fact]
        public void FileSystemIntegration_ProductionFileSystem_App_OutputPaths()
        {
            this._fileSystemProduction.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreFileSystem>();

            this.TestOutputHelper.WriteLine($"CurrentFolder:\t{this._fileSystemProduction.CurrentFolder}");
            this.TestOutputHelper.WriteLine();
            this.TestOutputHelper.WriteLine($"AssemblyFolderPath:\t{this._fileSystemProduction.AssemblyFolderPath}");
            this.TestOutputHelper.WriteLine($"ResourceFolderPath:\t{this._fileSystemProduction.ResourceFolderPath}");
            this.TestOutputHelper.WriteLine($"EntryAssemblyPath:\t{this._fileSystemProduction.EntryAssemblyPath}");
            this.TestOutputHelper.WriteLine($"ExecutingAssemblyPath:\t{this._fileSystemProduction.ExecutingAssemblyPath}");
            this.TestOutputHelper.WriteLine();
            this.TestOutputHelper.WriteLine($"DeviceID:\t{this.TestNetworkServices.LocalNetworkDevice.DeviceID}");
            this.TestOutputHelper.WriteLine($"SystemDeviceID:\t{this.TestNetworkingSystem.SystemDeviceID}");
            this.TestOutputHelper.WriteLine();
            this.TestOutputHelper.WriteLine($"UserID:\t{this.TestNetworkServices.LocalNetworkDevice.UserID}");
            this.TestOutputHelper.WriteLine($"SystemUserID:\t{this.TestNetworkingSystem.SystemUserID}");
        }

        /// <summary>
        /// Defines the test method FileSystemIntegration_SpecialFolders.
        /// </summary>
        /// <param name="specialFolder">The special folder.</param>
        [ExcludeFromCodeCoverage]
        [Theory(Skip = "DevOnly")]
        [InlineData(Environment.SpecialFolder.Desktop)]
        [InlineData(Environment.SpecialFolder.Programs)]
        [InlineData(Environment.SpecialFolder.MyDocuments)]

        // This is the same value as Environment.SpecialFolder.MyDocuments
        // [InlineData(Environment.SpecialFolder.Personal)]
        [InlineData(Environment.SpecialFolder.Favorites)]
        [InlineData(Environment.SpecialFolder.Startup)]
        [InlineData(Environment.SpecialFolder.Recent)]
        [InlineData(Environment.SpecialFolder.SendTo)]
        [InlineData(Environment.SpecialFolder.StartMenu)]
        [InlineData(Environment.SpecialFolder.MyMusic)]
        [InlineData(Environment.SpecialFolder.MyVideos)]
        [InlineData(Environment.SpecialFolder.DesktopDirectory)]
        [InlineData(Environment.SpecialFolder.MyComputer)]
        [InlineData(Environment.SpecialFolder.NetworkShortcuts)]
        [InlineData(Environment.SpecialFolder.Fonts)]
        [InlineData(Environment.SpecialFolder.Templates)]
        [InlineData(Environment.SpecialFolder.CommonStartMenu)]
        [InlineData(Environment.SpecialFolder.CommonPrograms)]
        [InlineData(Environment.SpecialFolder.CommonStartup)]
        [InlineData(Environment.SpecialFolder.CommonDesktopDirectory)]
        [InlineData(Environment.SpecialFolder.ApplicationData)]
        [InlineData(Environment.SpecialFolder.PrinterShortcuts)]
        [InlineData(Environment.SpecialFolder.LocalApplicationData)]
        [InlineData(Environment.SpecialFolder.InternetCache)]
        [InlineData(Environment.SpecialFolder.Cookies)]
        [InlineData(Environment.SpecialFolder.History)]
        [InlineData(Environment.SpecialFolder.CommonApplicationData)]
        [InlineData(Environment.SpecialFolder.Windows)]
        [InlineData(Environment.SpecialFolder.System)]
        [InlineData(Environment.SpecialFolder.ProgramFiles)]
        [InlineData(Environment.SpecialFolder.MyPictures)]
        [InlineData(Environment.SpecialFolder.UserProfile)]
        [InlineData(Environment.SpecialFolder.SystemX86)]
        [InlineData(Environment.SpecialFolder.ProgramFilesX86)]
        [InlineData(Environment.SpecialFolder.CommonProgramFiles)]
        [InlineData(Environment.SpecialFolder.CommonProgramFilesX86)]
        [InlineData(Environment.SpecialFolder.CommonTemplates)]
        [InlineData(Environment.SpecialFolder.CommonDocuments)]
        [InlineData(Environment.SpecialFolder.CommonAdminTools)]
        [InlineData(Environment.SpecialFolder.AdminTools)]
        [InlineData(Environment.SpecialFolder.CommonMusic)]
        [InlineData(Environment.SpecialFolder.CommonPictures)]
        [InlineData(Environment.SpecialFolder.CommonVideos)]
        [InlineData(Environment.SpecialFolder.Resources)]
        [InlineData(Environment.SpecialFolder.LocalizedResources)]
        [InlineData(Environment.SpecialFolder.CommonOemLinks)]
        [InlineData(Environment.SpecialFolder.CDBurning)]
        public void FileSystemIntegration_SpecialFolders(Environment.SpecialFolder specialFolder)
        {
            var specialFolderPath = this.TestFileSystem.GetSpecialFolderPath(specialFolder);
            specialFolderPath.Should().NotBeNull();

            this.TestOutputHelper.WriteLine($"[{specialFolder}]=\t[{specialFolderPath}]");

            if (!string.IsNullOrEmpty(specialFolderPath))
            {
                this.TestFileSystem.FolderExists(specialFolderPath).Should().BeTrue("Folder does not exist");
            }
        }

        /// <summary>
        /// Defines the test method FileSystemIntegration_OutputSpecialFolders.
        /// </summary>
        [ExcludeFromCodeCoverage]
        [Fact]
        public async Task FileSystemIntegration_OutputSpecialFolders()
        {
            foreach (var item in Enum.GetValues(typeof(Environment.SpecialFolder)))
            {
                if (item is Environment.SpecialFolder specialFolder)
                {
                    var folderExists = false;
                    var folderCreatable = false;
                    var folderWritable = false;
                    var folderDeletable = false;
                    var specialFolderPath = this.TestFileSystem.GetSpecialFolderPath(specialFolder);

                    if (!string.IsNullOrEmpty(specialFolderPath))
                    {
                        folderExists = folderCreatable = this.TestFileSystem.FolderExists(specialFolderPath);

                        if (!folderExists)
                        {
                            try
                            {
                                this.TestFileSystem.CreateFolder(specialFolderPath);
                                folderCreatable = folderExists = this.TestFileSystem.FolderExists(specialFolderPath);
                            }
                            catch (UnauthorizedAccessException)
                            {
                                folderCreatable = false;
                                folderExists = false;
                            }
                            catch (IOException)
                            {
                                folderCreatable = false;
                                folderExists = false;
                            }
                        }

                        if (folderExists)
                        {
                            var tempFileName = $"{Guid.NewGuid().ToStringNoDashes()}.tmp";
                            var tempFilePath = $"{specialFolderPath}{tempFileName}";

                            try
                            {
                                await CoreFileSystemBase.WriteFileContentsAsync(tempFilePath, "Test");

                                folderWritable = this.TestFileSystem.FileExists(tempFilePath);

                                if (folderWritable)
                                {
                                    try
                                    {
                                        this.TestFileSystem.DeleteFile(tempFilePath);
                                        folderDeletable = !this.TestFileSystem.FolderExists(tempFilePath);
                                    }
                                    catch (UnauthorizedAccessException)
                                    {
                                        folderDeletable = false;
                                    }
                                }
                            }
                            catch (UnauthorizedAccessException)
                            {
                                folderWritable = false;
                                folderDeletable = false;
                            }
                            catch (IOException)
                            {
                                folderWritable = false;
                                folderDeletable = false;
                            }
                        }
                    }

                    this.TestOutputHelper.WriteLine(
                        $"{specialFolder}\t[{specialFolderPath}]\t{folderExists}\t{folderCreatable}\t{folderWritable}\t{folderDeletable}");
                }
            }
        }

        /// <summary>
        /// Defines the test method FileSystemIntegration_OperatingSystem.
        /// </summary>
        [Fact]
        public void FileSystemIntegration_OperatingSystem()
        {
            this.TestFileSystem.OperatingSystem.Should().NotBeNull().And.Subject.Should()
                .BeAssignableTo<ICoreOperatingSystem>();
        }

        /// <summary>
        /// Defines the test method FileSystemIntegration_OperatingSystem_Loggable_AppleLog.
        /// </summary>
        [Fact]
        public void FileSystemIntegration_OperatingSystem_Loggable_AppleLog()
        {
            this.TestFileSystem.OperatingSystem.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreOperatingSystem>();
            this.TestFileSystem.OperatingSystem.IsLoggerProviderSupportedOnDevice(LoggerProviderFlags.AppleLog).Should().Be(this.TestFileSystem.OperatingSystem.IsAppleMobileOS);
        }

        /// <summary>
        /// Defines the test method FileSystemIntegration_AssemblyFolderPath.
        /// </summary>
        [Fact]
        public void FileSystemIntegration_AssemblyFolderPath()
        {
            this.ValidateProperty(this.TestFileSystem, nameof(this.TestFileSystem.AssemblyFolderPath), () => this.TestFileSystem.AssemblyFolderPath, ValidatePropertyFlags.FolderExists);
        }

        /// <summary>
        /// Defines the test method FileSystemIntegration_ResourceFolderPath.
        /// </summary>
        [Fact]
        public void FileSystemIntegration_ResourceFolderPath()
        {
            this.ValidateProperty(this.TestFileSystem, nameof(this.TestFileSystem.ResourceFolderPath), () => this.TestFileSystem.ResourceFolderPath, ValidatePropertyFlags.FolderExists);
        }

        /// <summary>
        /// Defines the test method FileSystemIntegration_ExecutingAssemblyPath.
        /// </summary>
        [Fact]
        public void FileSystemIntegration_ExecutingAssemblyPath()
        {
            this.ValidateProperty(this.TestFileSystem, nameof(this.TestFileSystem.ExecutingAssemblyPath), () => this.TestFileSystem.ExecutingAssemblyPath, ValidatePropertyFlags.CheckPathExists);
        }

        /// <summary>
        /// Defines the test method FileSystemIntegration_EntryAssemblyPath.
        /// </summary>
        [Fact]
        public void FileSystemIntegration_EntryAssemblyPath()
        {
            this.ValidateProperty(this.TestFileSystem, nameof(this.TestFileSystem.EntryAssemblyPath), () => this.TestFileSystem.EntryAssemblyPath!, ValidatePropertyFlags.CheckPathExists);
        }

        [Fact]
        public void FileSystemIntegration_LocalUserAppTempFolderPath()
        {
            this.ValidateProperty(this.TestFileSystem, nameof(this.TestFileSystem.LocalUserAppTempFolderPath), () => this.TestFileSystem.LocalUserAppTempFolderPath, ValidatePropertyFlags.FolderWritable);
        }

        [Fact]
        public void FileSystemIntegration_LocalUserAppDataFolderPath()
        {
            this.ValidateProperty(this.TestFileSystem, nameof(this.TestFileSystem.LocalUserAppDataFolderPath), () => this.TestFileSystem.LocalUserAppDataFolderPath, ValidatePropertyFlags.CreateFolderWritable);
            this.ValidateProperty(this._fileSystemProduction, nameof(this._fileSystemProduction.LocalUserAppDataFolderPath), () => this._fileSystemProduction.LocalUserAppDataFolderPath, ValidatePropertyFlags.CreateFolderWritable);

            this._fileSystemProduction.LocalUserAppDataFolderPath.Should().NotBe(this.TestFileSystem.LocalUserAppDataFolderPath, "Production LocalUserAppDataFolderPath should not equal test LocalUserAppDataFolderPath");

            // Validate production LocalUserAppDataFolderPath starts with Environment.SpecialFolder.LocalApplicationData (Windows) or Environment.SpecialFolder.ApplicationData (Non-Windows)
            if (this._fileSystemProduction.OperatingSystem.IsWindowsPlatform)
            {
                this._fileSystemProduction.LocalUserAppDataFolderPath.Should().StartWith(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Production LocalUserAppDataFolderPath should start with Environment.SpecialFolder.LocalApplicationData");
            }
            else
            {
                this._fileSystemProduction.LocalUserAppDataFolderPath.Should().StartWith(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Production LocalUserAppDataFolderPath should start with Environment.SpecialFolder.ApplicationData");
            }

            this.TestFileSystem.LocalUserAppDataFolderPath.Should().StartWith(Path.GetTempPath(), "Test LocalUserAppDataFolderPath should start with Path.GetTempPath()");
        }

        [Fact]
        public void FileSystemIntegration_RoamingUserAppDataFolderPath()
        {
            this.ValidateProperty(this.TestFileSystem, nameof(this.TestFileSystem.RoamingUserAppDataFolderPath), () => this.TestFileSystem.RoamingUserAppDataFolderPath, ValidatePropertyFlags.CreateFolderWritable);
            this.ValidateProperty(this._fileSystemProduction, nameof(this._fileSystemProduction.RoamingUserAppDataFolderPath), () => this._fileSystemProduction.RoamingUserAppDataFolderPath, ValidatePropertyFlags.CreateFolderWritable);

            this._fileSystemProduction.RoamingUserAppDataFolderPath.Should().NotBe(this.TestFileSystem.RoamingUserAppDataFolderPath, "Production RoamingUserAppDataFolderPath should not equal test RoamingUserAppDataFolderPath");

            // Validate production RoamingUserAppDataFolderPath starts with Environment.SpecialFolder.ApplicationData
            this._fileSystemProduction.RoamingUserAppDataFolderPath.Should().StartWith(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Production RoamingUserAppDataFolderPath should start with Environment.SpecialFolder.ApplicationData");

            this.TestFileSystem.RoamingUserAppDataFolderPath.Should().StartWith(Path.GetTempPath(), "Test RoamingUserAppDataFolderPath should start with Path.GetTempPath()");
        }

        [Fact]
        public void FileSystemIntegration_RoamingDefaultUserAppDataFolderPath()
        {
            this.ValidateProperty(this.TestFileSystem, nameof(this.TestFileSystem.RoamingDefaultUserAppDataFolderPath), () => this.TestFileSystem.RoamingDefaultUserAppDataFolderPath, ValidatePropertyFlags.CreateFolderWritable);
            this.ValidateProperty(this._fileSystemProduction, nameof(this._fileSystemProduction.RoamingDefaultUserAppDataFolderPath), () => this._fileSystemProduction.RoamingDefaultUserAppDataFolderPath, ValidatePropertyFlags.CreateFolderWritable);

            this._fileSystemProduction.RoamingDefaultUserAppDataFolderPath.Should().NotBe(this.TestFileSystem.RoamingDefaultUserAppDataFolderPath, "Production RoamingDefaultUserAppDataFolderPath should not equal test RoamingDefaultUserAppDataFolderPath");

            // Validate production RoamingDefaultUserAppDataFolderPath starts with Environment.SpecialFolder.CommonApplicationData
            if (this.TestOperatingSystem.IsWindowsPlatform)
            {
                this._fileSystemProduction.RoamingDefaultUserAppDataFolderPath.Should().StartWith(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Production RoamingDefaultUserAppDataFolderPath should start with Environment.SpecialFolder.CommonApplicationData");
            }
            else
            {
                this._fileSystemProduction.RoamingDefaultUserAppDataFolderPath.Should().StartWith(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Production RoamingDefaultUserAppDataFolderPath should start with Environment.SpecialFolder.ApplicationData");
            }

            this.TestFileSystem.RoamingDefaultUserAppDataFolderPath.Should().StartWith(Path.GetTempPath(), "Test RoamingDefaultUserAppDataFolderPath should start with Path.GetTempPath()");
        }

        [Fact]
        public void FileSystemIntegration_GetLocalUserAppDatabaseTempFolderPath()
        {
            this.ValidateProperty(this.TestFileSystem, nameof(this.TestFileSystem.GetLocalUserAppDatabaseTempFolderPath), this.TestFileSystem.GetLocalUserAppDatabaseTempFolderPath, ValidatePropertyFlags.CreateFolderWritable);
            this.ValidateProperty(this._fileSystemProduction, nameof(this._fileSystemProduction.GetLocalUserAppDatabaseTempFolderPath), this._fileSystemProduction.GetLocalUserAppDatabaseTempFolderPath, ValidatePropertyFlags.CreateFolderWritable);

            this._fileSystemProduction.GetLocalUserAppDatabaseTempFolderPath().Should().StartWith(this._fileSystemProduction.LocalUserAppTempFolderPath, "GetLocalUserAppDatabaseTempFolderPath should start with LocalUserAppTempFolderPath");
        }

        [Fact]
        public void FileSystemIntegration_LocalUserAppDatabaseFolderPath()
        {
            this.ValidateProperty(this.TestFileSystem, nameof(this.TestFileSystem.LocalUserAppDatabaseFolderPath), () => this.TestFileSystem.LocalUserAppDatabaseFolderPath, ValidatePropertyFlags.TestCreateFolderWritable);
            this.ValidateProperty(this._fileSystemProduction, nameof(this.TestFileSystem.LocalUserAppDatabaseFolderPath), () => this.TestFileSystem.LocalUserAppDatabaseFolderPath, ValidatePropertyFlags.CreateFolderWritable);

            this.TestFileSystem.LocalUserAppDatabaseFolderPath.Should().StartWith(this.TestFileSystem.LocalUserAppDataFolderPath, "LocalUserAppDatabaseFolderPath should start with LocalUserAppDataFolderPath");
            this.TestFileSystem.LocalUserAppDatabaseFolderPath.Should().NotBe(this._fileSystemProduction.LocalUserAppDatabaseFolderPath, "LocalUserAppDatabaseFolderPath should NOT equal LocalUserAppDatabaseFolderPath");
        }

        [Fact]
        public void FileSystemIntegration_LocalUserAppLogFolderPath()
        {
            this.ValidateProperty(this.TestFileSystem, nameof(this.TestFileSystem.LocalUserAppLogFolderPath), () => this.TestFileSystem.LocalUserAppLogFolderPath, ValidatePropertyFlags.TestCreateFolderWritable);
            this.ValidateProperty(this._fileSystemProduction, nameof(this.TestFileSystem.LocalUserAppLogFolderPath), () => this.TestFileSystem.LocalUserAppLogFolderPath, ValidatePropertyFlags.CreateFolderWritable);

            this.TestFileSystem.LocalUserAppLogFolderPath.Should().StartWith(this.TestFileSystem.LocalUserAppDataFolderPath, "LocalUserAppLogFolderPath should start with LocalUserAppDataFolderPath");
            this.TestFileSystem.LocalUserAppLogFolderPath.Should().NotBe(this._fileSystemProduction.LocalUserAppLogFolderPath, "LocalUserAppLogFolderPath should NOT equal LocalUserAppLogFolderPath");
        }

        [Fact]
        public void FileSystemIntegration_LocalUserAppSettingsFolderPath()
        {
            this.ValidateProperty(this.TestFileSystem, nameof(this.TestFileSystem.LocalUserAppSettingsFolderPath), () => this.TestFileSystem.LocalUserAppSettingsFolderPath, ValidatePropertyFlags.TestCreateFolderWritable);
            this.ValidateProperty(this._fileSystemProduction, nameof(this.TestFileSystem.LocalUserAppSettingsFolderPath), () => this.TestFileSystem.LocalUserAppSettingsFolderPath, ValidatePropertyFlags.CreateFolderWritable);

            this.TestFileSystem.LocalUserAppSettingsFolderPath.Should().StartWith(this.TestFileSystem.LocalUserAppDataFolderPath, "LocalUserAppSettingsFolderPath should start with LocalUserAppDataFolderPath");
            this.TestFileSystem.LocalUserAppSettingsFolderPath.Should().NotBe(this._fileSystemProduction.LocalUserAppSettingsFolderPath, "LocalUserAppSettingsFolderPath should NOT equal LocalUserAppSettingsFolderPath");
        }

        /// <summary>
        /// Defines the test method FileSystemIntegration_CreateNewFile_DeleteOnClose.
        /// </summary>
        [Fact]
        public void FileSystemIntegration_CreateNewFile_DeleteOnClose()
        {
            var tempFilePath = this.TestFileSystem.GetLocalUserAppTempFileName();
            tempFilePath.Should().NotBeNullOrEmpty();
            this.TestOutputHelper.WriteLine($"Writing file to {tempFilePath}");

            using (FileStream stream = this.TestFileSystem.CreateNewFileStream(tempFilePath, FileOptions.DeleteOnClose))
            {
                stream.Should().NotBeNull();
                stream.Name.Should().Be(tempFilePath);
                this.ValidateProperty(this.TestFileSystem, "GetLocalUserAppTempFileName", () => tempFilePath!, ValidatePropertyFlags.PathExists);
            }

            // Give the OS time to delete the file
            this.TestDelay(100, this.TestCaseLogger).Should().BeTrue();

            // We did not write to file so it should not be there anymore
            this.TestFileSystem.FileExists(tempFilePath).Should().BeFalse();
        }

        [Fact]
        public void FileSystemIntegration_CreateLocalUserAppTempFileStream()
        {
            var testBytes = new byte[] { 0x31, 0x32 };
            var testBytesRead = new byte[2];

            FileStream tempFileStream = this.TestFileSystem.CreateLocalUserAppTempFileStream();

            tempFileStream.Should().NotBeNull();

            var tempFileName = tempFileStream.Name;
            this.TestFileSystem.FileExists(tempFileName).Should().BeTrue();
            tempFileStream.Length.Should().Be(0);
            tempFileStream.Write(testBytes, 0, testBytes.Length);
            tempFileStream.Flush();
            tempFileStream.Seek(0, SeekOrigin.Begin);
            var bytesRead = tempFileStream.Read(testBytesRead, 0, testBytesRead.Length);
            bytesRead.Should().Be(testBytes.Length);
            testBytesRead.Should().BeEquivalentTo(testBytes);

            // Dispose file stream.
            tempFileStream.Dispose();

            // Wait for 100 milliseconds.
            this.TestDelay(100, this.TestCaseLogger);

            // Validate the file no longer exists
            this.TestFileSystem.FileExists(tempFileName).Should().BeFalse();
        }

        /// <summary>
        /// Defines the test method FileSystemIntegration_CurrentFolder.
        /// </summary>
        [Fact]
        public void FileSystemIntegration_CurrentFolder()
        {
            this.ValidateProperty(this.TestFileSystem, nameof(this.TestFileSystem.CurrentFolder), () => this.TestFileSystem.CurrentFolder, ValidatePropertyFlags.FolderExists);
        }

        /// <summary>
        /// Defines the test method FileSystemIntegration_GetSpecialFolderPath.
        /// </summary>
        [Fact]
        public void FileSystemIntegration_GetSpecialFolderPath()
        {
            this.TestFileSystem.GetSpecialFolderPath(Environment.SpecialFolder.MyDocuments).Should()
                .NotBeNullOrWhiteSpace();
        }

        /// <summary>
        /// Defines the test method FileSystemIntegration_ReadFileContentsAsync_Null.
        /// </summary>
        [Fact]
        public async Task FileSystemIntegration_ReadFileContentsAsync_Null()
        {
            Func<Task<string?>> fx = async () =>
                await this.TestFileSystem.ReadFileContentsAsync(null!);

            (await fx.Should().ThrowAsync<ArgumentNullException>()).And.ParamName.Should().Be("path");
        }

        /// <summary>
        /// Defines the test method FileSystemIntegration_GetFolderName.
        /// </summary>
        [Fact]
        public void FileSystemIntegration_GetFolderName()
        {
            var folderName = this.TestFileSystem.GetFolderName(typeof(CoreFileSystemIntegrationTests).Assembly.Location);
            folderName.Should().NotBeNullOrEmpty();
            this.TestFileSystem.FolderExists(folderName).Should().BeTrue();
        }

        [Fact]
        public void FileSystemIntegration_EnsureWriteableFolder()
        {
            CoreFileSystemBase.EnsureWriteableFolder(this.TestFileSystem.LocalUserAppLogFolderPath).Should().BeTrue();
        }

        /// <summary>
        /// Defines the test method FileSystemIntegration_GetFolderName_Null.
        /// </summary>
        [Fact]
        public void FileSystemIntegration_GetFolderName_Null()
        {
            Func<string> fx = () => this.TestFileSystem.GetFolderName(null!);
            fx.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("path");
        }

        /// <summary>
        /// Defines the test method FileSystemIntegration_DeleteFolder.
        /// </summary>
        [Fact]
        public void FileSystemIntegration_DeleteFolder()
        {
            var tempFolderPath = Path.Combine(this.TestFileSystem.LocalUserAppTestArtifactsFolderPath, Guid.NewGuid().ToStringNoDashes());

            this.TestFileSystem.FolderExists(tempFolderPath).Should().BeFalse();
            this.TestFileSystem.CreateFolder(tempFolderPath);
            this.TestFileSystem.FolderExists(tempFolderPath).Should().BeTrue();
            this.TestFileSystem.DeleteFolder(tempFolderPath);
            this.TestFileSystem.FolderExists(tempFolderPath).Should().BeFalse();
        }

        /// <summary>
        /// Defines the test method FileSystemIntegration_DeleteFolder_Null.
        /// </summary>
        [Fact]
        public void FileSystemIntegration_DeleteFolder_Null()
        {
            Action act = () => this.TestFileSystem.DeleteFolder(null!);
            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("path");
        }

        /// <summary>
        /// Defines the test method FileSystemIntegration_GetFileNameWithoutExtension.
        /// </summary>
        [Fact]
        public void FileSystemIntegration_GetFileNameWithoutExtension()
        {
            var fileNameWithoutExtension =
                this.TestFileSystem.GetFileNameWithoutExtension(typeof(CoreFileSystemIntegrationTests).Assembly.Location);
            fileNameWithoutExtension.Should().NotBeNullOrEmpty();

            fileNameWithoutExtension.Should().EndWith(".IntegrationTests");
        }

        /// <summary>
        /// Defines the test method FileSystemIntegration_GetFileNameWithoutExtension_Null.
        /// </summary>
        [Fact]
        public void FileSystemIntegration_GetFileNameWithoutExtension_Null()
        {
            Func<string> fx = () => this.TestFileSystem.GetFileNameWithoutExtension(null!);
            fx.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("path");
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!this._isDisposed)
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
                    this._isDisposed = true;
                }
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Validates the property.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="fxProperty">The fx property.</param>
        /// <param name="validatePropertyFlags">The validate property flags.</param>
        private void ValidateProperty(
            ICoreFileSystem fileSystem,
            string propertyName,
            Func<string> fxProperty,
            ValidatePropertyFlags validatePropertyFlags) => ValidateProperty(this.TestOutputHelper, fileSystem, propertyName, fxProperty, validatePropertyFlags);
    }
}
