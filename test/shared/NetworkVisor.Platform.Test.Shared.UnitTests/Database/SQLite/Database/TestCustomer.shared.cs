// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="TestCustomer.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using NetworkVisor.Core.Database.Providers.SQLite.Attributes;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite.Database
{
    public class TestCustomer
    {
        [AutoIncrement, PrimaryKey]
        public int Id { get; set; }

        [MaxLength(64)]
        public string? FirstName { get; set; }

        [MaxLength(64)]
        public string? LastName { get; set; }

        [MaxLength(64), Indexed]
        public string? Email { get; set; }

        [MaxLength(64), Indexed]
        public string? Address { get; set; }

        [MaxLength(64), Indexed]
        public string? Country { get; set; }
    }
}
