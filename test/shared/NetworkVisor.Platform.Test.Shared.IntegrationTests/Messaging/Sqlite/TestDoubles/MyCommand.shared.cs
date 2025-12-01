// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="MyCommand.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary>
//  Ported from the Brighter project: https://github.com/BrighterCommand/Brighter
//
//  The MIT License (MIT)
//  Copyright © 2014 Ian Cooper (ian_hammond_cooper@yahoo.co.uk)
// </summary>
using System;
using Paramore.Brighter;
using Command = Paramore.Brighter.Command;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Message.Sqlite.TestDoubles
{
    internal sealed class MyCommand : Command
    {
        public MyCommand()
            : base(Guid.NewGuid())
        {
        }

        public string Value { get; set; } = string.Empty;

        public bool WasCancelled { get; set; }

        public bool TaskCompleted { get; set; }
    }
}
