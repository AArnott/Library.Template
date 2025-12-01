// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 10-05-2024
// // ***********************************************************************
// <copyright file="CoreCommandTestCaseBase.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using NetworkVisor.Core.Messaging.Database;
using NetworkVisor.Core.Messaging.Tables;
using NetworkVisor.Core.Test.Fixtures;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Messaging.Shared.IntegrationTests.Messaging.CommandProcessor.Base
{
    /// <summary>
    /// Represents the base class for core test cases in the NetworkVisor platform.
    /// </summary>
    /// <remarks>
    /// This abstract class provides a foundational implementation for test cases, integrating
    /// with xUnit and offering various utilities and services for testing within the NetworkVisor platform.
    /// </remarks>
    public abstract class CoreCommandTestCaseBase : CoreEntityTestCaseBase, IClassFixture<CoreTestClassFixture>
    {
        public const string? SkipReason = "Test Commands Not Working";
        public const int DefaultShortDelayInMilliseconds = 2000;
        public const int DefaultLongDelayInMilliseconds = 20000;

        public static readonly string TestCommandName = "Steve";
        public static readonly string TestCommandNameResult = $"Hello {TestCommandName}, your IP address is:";

        private Lazy<ICoreTestCommandProcessor> lazyTestCommandProcessor;
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreCommandTestCaseBase"/> class.
        /// </summary>
        /// <param name="testClassFixture">The test class fixture.</param>
        /// <remarks>
        ///     TestClassFixture is shared across test cases within the same test class.
        /// </remarks>
        protected CoreCommandTestCaseBase(ICoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
            this.lazyTestCommandProcessor = new Lazy<ICoreTestCommandProcessor>(() => new CoreTestCommandProcessor(this.TestCaseServiceProvider, this.CommandProcessor.WrappedCommandProcessor));
        }

        /// <summary>
        /// Gets the instance of <see cref="ICoreTestCommandProcessor"/> used for handling test-related commands
        /// within the NetworkVisor platform.
        /// </summary>
        /// <remarks>
        /// This property lazily initializes the command processor, providing access to messaging database operations
        /// and configurations essential for integration tests.
        /// </remarks>
        /// <value>
        /// An instance of <see cref="ICoreTestCommandProcessor"/> that facilitates test command processing.
        /// </value>
        public ICoreTestCommandProcessor TestCommandProcessor => this.lazyTestCommandProcessor.Value;

        /// <summary>
        /// Gets the messaging database used for testing within the NetworkVisor platform.
        /// </summary>
        /// <remarks>
        /// This property provides access to the <see cref="ICoreMessagingDatabase"/> instance
        /// associated with the test command processor. It enables interaction with the messaging-related
        /// database tables, such as inbox, outbox, and queue, for integration testing purposes.
        /// </remarks>
        /// <value>
        /// An instance of <see cref="ICoreMessagingDatabase"/> representing the messaging database.
        /// </value>
        public ICoreMessagingDatabase TestMessagingDatabase => this.TestCommandProcessor.MessagingDatabase;

        /// <summary>
        /// Gets the file path of the messaging database used in the test environment.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> representing the file path of the messaging database.
        /// </value>
        /// <remarks>
        /// This property provides access to the database path of the messaging database
        /// utilized by the test command processor in the integration tests.
        /// </remarks>
        public string TestMessagingDatabasePath => this.TestCommandProcessor.MessagingDatabase.DatabasePath;

        /// <summary>
        /// Delays execution for a specified duration and validates the test output.
        /// </summary>
        /// <param name="millisecondsDelay">
        /// The delay duration in milliseconds. Defaults to <see cref="DefaultShortDelayInMilliseconds"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the test output contains the expected result specified by <see cref="TestCommandNameResult"/>; otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// This method introduces a delay in the test execution using TestDelay and then checks if the test output
        /// contains the expected result. The delay duration can be customized by providing a value for <paramref name="millisecondsDelay"/>.
        /// </remarks>
        public bool DelayAndValidateTestOutput(int millisecondsDelay = DefaultShortDelayInMilliseconds)
        {
            if (millisecondsDelay > 0)
            {
                _ = this.TestDelay(millisecondsDelay, this.TestCaseLogger);
            }

            return this.TestOutputHelper.Output.Contains(TestCommandNameResult);
        }

        protected override void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                try
                {
                    if (disposing)
                    {
                        // Paramore.Brighter.CommandProcessor.ClearServiceBus();
                        this.TestCommandProcessor.MessagingDatabase.InboxTable.DeleteAllAsync().GetAwaiter().GetResult();
                        this.TestCommandProcessor.MessagingDatabase.OutboxTable.DeleteAllAsync().GetAwaiter().GetResult();
                    }
                }
                finally
                {
                    this.disposedValue = true;
                    base.Dispose(disposing);
                }
            }
        }
    }
}
