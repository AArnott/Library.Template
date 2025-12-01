// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 05-12-2020
// ***********************************************************************
// <copyright file="CoreTestObservableUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using NetworkVisor.Core.CoreSystem;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Test.Extensions;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Test
{
    /// <summary>
    /// Class CoreTestObservableUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreTestObservableUnitTests))]

    public class CoreTestObservableUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreTestObservableUnitTests" /> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreTestObservableUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        /// <summary>
        /// Defines the test method Ctor.
        /// </summary>
        [Fact]
        public void Ctor()
        {
            this.TestOutputHelper.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestOutputHelper>();
            this.TestFileSystem.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreFileSystem>();
        }

        /// <summary>
        /// Defines the test method TestObservable_TestLoggerObserver_SubscribeToConsole.
        /// </summary>
        [Fact]
        public void TestObservable_TestLoggerObserver_SubscribeToConsole()
        {
            using var testObservable = new TestObservable<int>();

            using (testObservable.SubscribeTestConsole(this.TestOutputHelper))
            {
                testObservable.SendOnNext(1);
            }
        }
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

        /// <summary>
        /// Defines the test method TestObservable_TestLoggerObserver_SubscribeToConsole_Observable_Null.
        /// </summary>
        [Fact]
        public void TestObservable_TestLoggerObserver_SubscribeToConsole_Observable_Null()
        {
            Func<IDisposable> fx = () => ((TestObservable<int>)null).SubscribeTestConsole(this.TestOutputHelper);
            fx.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("observable");
        }

        /// <summary>
        /// Defines the test method TestObservable_TestLoggerObserver_SubscribeToConsole_TestOutputHelper_Null.
        /// </summary>
        [Fact]
        public void TestObservable_TestLoggerObserver_SubscribeToConsole_TestOutputHelper_Null()
        {
            using var testObservable = new TestObservable<int>();

            Func<IDisposable> fx = () => testObservable.SubscribeTestConsole(null);
            fx.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("testOutputHelper");
        }

        /// <summary>
        /// Defines the test method TestObservable_TestLoggerObserver_SubscribeTestLogger_Observable_Null.
        /// </summary>
        [Fact]
        public void TestObservable_TestLoggerObserver_SubscribeTestLogger_Observable_Null()
        {
            Func<IDisposable> fx = () => ((TestObservable<int>)null).SubscribeTestLogger(null, "SubscribeTestLogger");
            fx.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("observable");
        }

        /// <summary>
        /// Defines the test method TestObservable_TestLoggerObserver_SubscribeTestLogger_Logger_Null.
        /// </summary>
        [Fact]
        public void TestObservable_TestLoggerObserver_SubscribeTestLogger_Logger_Null()
        {
            using var testObservable = new TestObservable<int>();

            Func<IDisposable> fx = () => testObservable.SubscribeTestLogger(null, "SubscribeTestLogger");
            fx.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }

        /// <summary>
        /// Class TestObservable.
        /// Implements the <see cref="IObservable{T}" />
        /// Implements the <see cref="IDisposable" />.
        /// </summary>
        /// <typeparam name="T">Type of observable.</typeparam>
        /// <seealso cref="IObservable{T}" />
        /// <seealso cref="IDisposable" />
        [ExcludeFromCodeCoverage]
        private class TestObservable<T> : IObservable<T>, IDisposable
        {
            /// <summary>
            /// The observer.
            /// </summary>
            private IObserver<T>? observer;

            /// <summary>
            /// Notifies the provider that an observer is to receive notifications.
            /// </summary>
            /// <param name="observer">The object that is to receive notifications.</param>
            /// <returns>A reference to an interface that allows observers to stop receiving notifications before the provider has finished sending them.</returns>
            public IDisposable Subscribe(IObserver<T> observer)
            {
                this.observer = observer;

                return this;
            }

            /// <summary>
            /// Sends the on next.
            /// </summary>
            /// <param name="item">The item.</param>
            public void SendOnNext(T item)
            {
                this.observer?.OnNext(item);
            }

            /// <summary>
            /// Sends the on error.
            /// </summary>
            /// <param name="error">The error.</param>
            public void SendOnError(Exception error)
            {
                this.observer?.OnError(error);
            }

            /// <summary>
            /// Sends the on completed.
            /// </summary>
            public void SendOnCompleted()
            {
                this.observer?.OnCompleted();
            }

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                this.observer?.OnCompleted();
                this.observer = null;
            }
        }
    }
}
