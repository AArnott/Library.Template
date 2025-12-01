// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="TestProduct.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using NetworkVisor.Core.Database.Providers.SQLite.Attributes;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite.Database
{
    public class TestProduct
    {
        [AutoIncrement, PrimaryKey]
        public int Id { get; set; }

        public string? Name { get; set; }

        public decimal Price { get; set; }

        public uint TotalSales { get; set; }
    }
}
