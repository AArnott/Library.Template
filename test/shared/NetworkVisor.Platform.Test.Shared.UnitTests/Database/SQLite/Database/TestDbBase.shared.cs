// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="TestDbBase.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using Microsoft.Extensions.Logging;
using NetworkVisor.Core.CoreSystem;
using NetworkVisor.Core.Database.Providers.SQLite.Connections;
using NetworkVisor.Core.Database.Providers.SQLite.Logging;
using NetworkVisor.Core.Database.Providers.SQLite.Types;
using NetworkVisor.Core.Networking.Interfaces;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite.Database
{
    public abstract class TestDbBase<T> : CoreSQLiteConnection
    {
        private bool disposedValue;

        protected TestDbBase(ICoreFileSystem fileSystem, CoreSQLiteConnectionString connectionString, bool wal = true, ICoreQueryLogger? queryLogger = null)
            : base(fileSystem, connectionString, queryLogger ?? new CoreQueryLogger(fileSystem.GlobalLogger))
        {
            if (wal)
            {
                this.EnableWriteAheadLogging();
            }
        }

        protected TestDbBase(string databaseName, ICoreFileSystem fileSystem, bool storeDateTimeAsTicks = true, object? key = null, bool wal = true, ICoreQueryLogger? queryLogger = null)
            : this(fileSystem, CoreSQLiteConnection.GetLocalUserAppDatabaseTempFilePath(fileSystem, databaseName), storeDateTimeAsTicks, key: key, queryLogger: queryLogger)
        {
        }

        protected TestDbBase(ICoreFileSystem fileSystem, bool storeDateTimeAsTicks = true, object? key = null, bool wal = true, ICoreQueryLogger? queryLogger = null)
            : this(fileSystem, CoreSQLiteConnection.GetLocalUserAppDatabaseTempFilePath<T>(fileSystem), storeDateTimeAsTicks, key: key, queryLogger: queryLogger)
        {
        }

        protected TestDbBase(ICoreFileSystem fileSystem, string path, bool storeDateTimeAsTicks = true, object? key = null, bool wal = true, ICoreQueryLogger? queryLogger = null)
            : this(fileSystem, new CoreSQLiteConnectionString(path, storeDateTimeAsTicks, key: key), queryLogger: queryLogger)
        {
        }

        protected TestDbBase(ICoreFileSystem fileSystem, string databasePath, CoreSQLiteOpenFlags openFlags, bool storeDateTimeAsTicks = true, ICoreQueryLogger? queryLogger = null)
            : this(fileSystem, new CoreSQLiteConnectionString(databasePath, openFlags, storeDateTimeAsTicks), queryLogger: queryLogger)
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether the database file should be deleted
        /// when the connection is closed and disposed.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the database file should be deleted upon closure; otherwise, <see langword="false"/>.
        /// The default value is <see langword="true"/>.
        /// </value>
        /// <remarks>
        /// When this property is set to <see langword="true"/>, the database file will be removed from the file system
        /// during the disposal process, provided the file exists and the database path is valid.
        /// </remarks>
        public bool CleanupDatabaseOnClose { get; set; } = true;

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            try
            {
                if (disposing)
                {
                    if (!this.disposedValue)
                    {
                        string databaseFilePath = this.DatabasePath;

                        if (this.CleanupDatabaseOnClose && !string.IsNullOrEmpty(databaseFilePath))
                        {
                            this.FileSystem.WaitToDeleteLockedFile(databaseFilePath);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Failed to delete temporary database folder {DatabaseTempFolderPath}", this.FileSystem.GetLocalUserAppDatabaseTempFolderPath());
            }
            finally
            {
                this.disposedValue = true;
            }
        }
    }
}
