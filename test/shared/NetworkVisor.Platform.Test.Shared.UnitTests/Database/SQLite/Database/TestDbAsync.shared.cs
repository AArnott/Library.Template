// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="TestDbAsync.shared.cs" company="Network Visor">
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
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Networking.Interfaces;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite.Database
{
    public class TestDbAsync<T> : TestDbAsyncBase<T>
    {
        public TestDbAsync(ICoreFileSystem fileSystem, CoreSQLiteConnectionString connectionString, ICoreQueryLogger? queryLogger = null)
            : base(fileSystem, connectionString, queryLogger)
        {
        }

        public TestDbAsync(string databaseName, ICoreFileSystem fileSystem, bool storeDateTimeAsTicks = true, object? key = null, ICoreQueryLogger? queryLogger = null)
            : this(fileSystem, CoreSQLiteConnection.GetLocalUserAppDatabaseTempFilePath(fileSystem, databaseName), storeDateTimeAsTicks, key: key, queryLogger)
        {
        }

        public TestDbAsync(ICoreFileSystem fileSystem, bool storeDateTimeAsTicks = true, object? key = null, ICoreQueryLogger? queryLogger = null)
            : this(fileSystem, CoreSQLiteConnection.GetLocalUserAppDatabaseTempFilePath<T>(fileSystem), storeDateTimeAsTicks, key: key, queryLogger)
        {
        }

        public TestDbAsync(ICoreFileSystem fileSystem, string path, bool storeDateTimeAsTicks = true, object? key = null, ICoreQueryLogger? queryLogger = null)
            : this(fileSystem, new CoreSQLiteConnectionString(path, storeDateTimeAsTicks, key: key), queryLogger)
        {
        }

        public TestDbAsync(ICoreFileSystem fileSystem, string databasePath, CoreSQLiteOpenFlags openFlags, bool storeDateTimeAsTicks = true, ICoreQueryLogger? queryLogger = null)
            : this(fileSystem, new CoreSQLiteConnectionString(databasePath, openFlags, storeDateTimeAsTicks), queryLogger)
        {
        }
    }
}
