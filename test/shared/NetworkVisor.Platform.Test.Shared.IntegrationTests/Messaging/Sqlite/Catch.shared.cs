// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="Catch.shared.cs" company="Network Visor">
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
using System.Diagnostics;
using System.Threading.Tasks;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Message.Sqlite
{
    [DebuggerStepThrough]
    public static class Catch
    {
        public static Exception? Exception(Action action)
        {
            Exception? exception = null;

            try
            {
                action();
            }
            catch (Exception e)
            {
                exception = e;
            }

            return exception;
        }

        public static async Task<Exception?> ExceptionAsync(Func<Task> action)
        {
            Exception? exception = null;

            try
            {
                await action();
            }
            catch (Exception e)
            {
                exception = e;
            }

            return exception;
        }
    }
}
