// Assembly         : NetworkVisor.Platform.Test.Shared.Messaging.IntegrationTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="CoreProcessDiscoveredEntityCommandIntegrationTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NetworkVisor.Core.Device;
using NetworkVisor.Core.Entities;
using NetworkVisor.Core.Entities.Base;
using NetworkVisor.Core.Entities.Constants;
using NetworkVisor.Core.Entities.Database;
using NetworkVisor.Core.Entities.Extensions;
using NetworkVisor.Core.Entities.Hosts;
using NetworkVisor.Core.Entities.Networks.Addresses;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Messaging.Inbox;
using NetworkVisor.Core.Messaging.Queries.Entities;
using NetworkVisor.Core.Messaging.Queries.Entities.Base;
using NetworkVisor.Core.Messaging.Queries.Entities.NetworkAddress;
using NetworkVisor.Core.Networking.Extensions;
using NetworkVisor.Core.Networking.Ping;
using NetworkVisor.Core.Networking.Services.Discovery.Commands;
using NetworkVisor.Core.Networking.Services.Ping.Commands;
using NetworkVisor.Core.Networking.Types;
using NetworkVisor.Core.Serialization;
using NetworkVisor.Core.Test.Extensions;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.Messaging.Shared.IntegrationTests.Messaging.CommandProcessor.Base;
using Paramore.Brighter;
using Xunit;
using IDispatcher = Paramore.Brighter.ServiceActivator.IDispatcher;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Networking.Discovery
{
    /// <summary>
    /// Class CoreProcessDiscoveredEntityCommandIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CoreProcessDiscoveredEntityCommandIntegrationTests))]

    public class CoreProcessDiscoveredEntityCommandIntegrationTests : CoreCommandTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreProcessDiscoveredEntityCommandIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreProcessDiscoveredEntityCommandIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
            this.EntityDatabase = this.TestCaseServiceProvider.GetRequiredService<ICoreEntityDatabase>();
        }

        protected static string RandomHostName => $"{Guid.NewGuid():N}.testhost.local";

        protected ICoreEntityDatabase EntityDatabase { get; }

        [Fact]
        public void ProcessDiscoveredEntityCommand_CommandDispatchService_Ctor()
        {
            _ = this.TestCommandProcessor.ProducersConfiguration.Should().NotBeNull().And.Subject.Should()
                .BeAssignableTo<IAmProducersConfiguration>();

            _ = this.TestCommandProcessor.ServiceActivatorConsumerOptions.Should().NotBeNull().And.Subject.Should()
                .BeAssignableTo<IAmConsumerOptions>();

            _ = this.TestCommandProcessor.InboxConfiguration.Should().NotBeNull().And.Subject.Should()
                .BeAssignableTo<InboxConfiguration>();

            _ = this.TestCommandProcessor.SqliteInbox.Should().NotBeNull().And.Subject.Should()
                .BeAssignableTo<ICoreSqliteInbox>();
        }

        [Fact]
        public void ProcessDiscoveredEntityCommand_CommandDispatchService_IsRunning()
        {
            _ = this.TestCommandDispatchService.Should().NotBeNull();
            _ = this.TestCommandDispatchService!.IsRunning.Should().BeTrue();
        }

        [Fact]
        public void ProcessDiscoveredEntityCommand_CommandDispatchService_Dispatcher()
        {
            _ = this.TestCommandDispatchService.Should().NotBeNull();
            _ = this.TestCommandDispatchService!.Dispatcher.Should().NotBeNull().And.Subject.Should().BeAssignableTo<IDispatcher>();
        }

        [Fact]
        public async Task ProcessDiscoveredEntityCommand_Entity_Insert()
        {
            var discoveredEntity = new CoreEntity();
            _ = this.TestCommandProcessor.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestCommandProcessor>();

            var processDiscoveredEntityCommand = new ProcessDiscoveredEntityCommand(discoveredEntity);

            await this.TestCommandProcessor!.SendAsync(processDiscoveredEntityCommand);
            CoreEntity? updatedEntity = await this.ValidateProcessDiscoveredEntityCommandAsync(processDiscoveredEntityCommand, discoveredEntity);
            updatedEntity.Should().NotBeNull().And.Subject.Should().BeAssignableTo<CoreEntity>();
            updatedEntity.Should().BeEquivalentTo(discoveredEntity);
        }

        [Fact]
        public async Task ProcessDiscoveredEntityCommand_Entity_Update()
        {
            var discoveredEntity = new CoreEntity();
            _ = this.TestCommandProcessor.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestCommandProcessor>();

            // Insert the entity first to ensure it exists in the database
            (await this.EntityDatabase.InsertEntityAsync(discoveredEntity)).Should().Be(1);

            var processDiscoveredEntityCommand = new ProcessDiscoveredEntityCommand(discoveredEntity);

            await this.TestCommandProcessor!.SendAsync(processDiscoveredEntityCommand);
            CoreEntity? updatedEntity = await this.ValidateProcessDiscoveredEntityCommandAsync(processDiscoveredEntityCommand, discoveredEntity);
            updatedEntity.Should().NotBeNull().And.Subject.Should().BeAssignableTo<CoreEntity>();
            updatedEntity.EntityID.Should().Be(discoveredEntity.EntityID);
            updatedEntity.CreatedUtc.Should().Be(discoveredEntity.CreatedUtc);
            updatedEntity.ModifiedUtc.Should().BeAfter(discoveredEntity.ModifiedUtc);
            updatedEntity.Should().NotBeEquivalentTo(discoveredEntity);
        }

        [Fact]
        public async Task ProcessDiscoveredEntityCommand_HostEntity_Insert()
        {
            var discoveredHostEntity = new CoreHostEntity(CoreHostEntityType.DnsHost, RandomHostName);

            _ = this.TestCommandProcessor.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestCommandProcessor>();

            var processDiscoveredEntityCommand = new ProcessDiscoveredEntityCommand(discoveredHostEntity);

            await this.TestCommandProcessor!.SendAsync(processDiscoveredEntityCommand);
            CoreHostEntity? updatedEntity = await this.ValidateProcessDiscoveredEntityCommandAsync(processDiscoveredEntityCommand, discoveredHostEntity);
            updatedEntity.Should().NotBeNull().And.Subject.Should().BeAssignableTo<CoreHostEntity>();
            updatedEntity.Should().BeEquivalentTo(discoveredHostEntity);
            updatedEntity.HostName.Should().Be(discoveredHostEntity.HostName);
            updatedEntity.HostEntityType.Should().Be(discoveredHostEntity.HostEntityType);
        }

        [Fact]
        public async Task ProcessDiscoveredEntityCommand_HostEntity_Update()
        {
            var discoveredHostEntity = new CoreHostEntity(CoreHostEntityType.DnsHost, RandomHostName);

            _ = this.TestCommandProcessor.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestCommandProcessor>();

            // Insert the entity first to ensure it exists in the database
            (await this.EntityDatabase.InsertEntityAsync(discoveredHostEntity)).Should().Be(1);

            var processDiscoveredEntityCommand = new ProcessDiscoveredEntityCommand(discoveredHostEntity);

            await this.TestCommandProcessor!.SendAsync(processDiscoveredEntityCommand);

            CoreHostEntity? updatedEntity = await this.ValidateProcessDiscoveredEntityCommandAsync(processDiscoveredEntityCommand, discoveredHostEntity);

            updatedEntity.Should().NotBeNull().And.Subject.Should().BeAssignableTo<CoreHostEntity>();
            updatedEntity.EntityID.Should().Be(discoveredHostEntity.EntityID);
            updatedEntity.CreatedUtc.Should().Be(discoveredHostEntity.CreatedUtc);
            updatedEntity.ModifiedUtc.Should().BeAfter(discoveredHostEntity.ModifiedUtc);
            updatedEntity.HostName.Should().Be(discoveredHostEntity.HostName);
            updatedEntity.HostEntityType.Should().Be(discoveredHostEntity.HostEntityType);
            updatedEntity.Should().NotBeEquivalentTo(discoveredHostEntity);
            updatedEntity.EntityID.Should().Be(CoreEntityConstants.CreateHashedEntityID(discoveredHostEntity.HostName));
        }

        private async Task<TEntity?> ValidateProcessDiscoveredEntityCommandAsync<TEntity>(ProcessDiscoveredEntityCommand processDiscoveredEntityCommand, TEntity discoveredEntity)
            where TEntity : CoreEntity, new()
        {
            JsonSerializerOptions options = CoreDefaultJsonSerializerOptions.GetDefaultJsonSerializerOptions(CoreSerializationFormatFlags.JsonFormatted, this.TestCaseServiceProvider);
            string json = JsonSerializer.Serialize(discoveredEntity, typeof(TEntity), options);

            this.TestOutputHelper.WriteLine($"{$"Original {typeof(TEntity).GetDisplayShortName()}".CenterTitle()}\n{json}");
            processDiscoveredEntityCommand.MessageBody.UpdatedEntityID.Should().NotBeNull().And.Subject.Should()
                .NotBeEmpty();

            TEntity? updatedEntity = await this.EntityDatabase.GetEntityAsTypeAsync<TEntity>(processDiscoveredEntityCommand.MessageBody.UpdatedEntityID.Value, CoreEntityConstants.DefaultSnapshotID);
            updatedEntity.Should().NotBeNull();

            json = JsonSerializer.Serialize(updatedEntity, typeof(TEntity), options);
            this.TestOutputHelper.WriteLine($"{$"Updated {typeof(TEntity).GetDisplayShortName()}".CenterTitle()}\n{json}");
            this.TestOutputHelper.WriteLine($"Updated Entity ID: {processDiscoveredEntityCommand.MessageBody.UpdatedEntityID}");
            this.TestOutputHelper.WriteLine($"Updated ModifiedUtc: {processDiscoveredEntityCommand.MessageBody.UpdateModifiedUtc!.Value.ToUniversalTimeJsonFormat()}");

            return updatedEntity as TEntity;
        }
    }
}
