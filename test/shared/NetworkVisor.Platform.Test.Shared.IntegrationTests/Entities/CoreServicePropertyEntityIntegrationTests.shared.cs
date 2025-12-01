// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// ***********************************************************************
// <copyright file="CoreServicePropertyEntityIntegrationTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// ***********************************************************************
// <summary>
// </summary>

using FluentAssertions;
using NetworkVisor.Core.Entities;
using NetworkVisor.Core.Entities.Base;
using NetworkVisor.Core.Entities.Constants;
using NetworkVisor.Core.Entities.Extensions;
using NetworkVisor.Core.Entities.Metadata.Base;
using NetworkVisor.Core.Entities.Services.Properties;
using NetworkVisor.Core.Metadata.Extensions;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Messaging.Shared.IntegrationTests.Entities
{
    /// <summary>
    /// Class CoreMetadataEntityIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CoreServicePropertyEntityIntegrationTests))]
    public class CoreServicePropertyEntityIntegrationTests : CoreEntityTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreServicePropertyEntityIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">
        /// The test class fixture that provides shared context and dependencies for the test class.
        /// </param>
        public CoreServicePropertyEntityIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public async Task ServicePropertyEntityIntegration_InsertServicePropertyEntityAsync()
        {
            var servicePropertyEntity = new CoreServicePropertyEntity("txtvers", CoreServicePropertyType.TxtVers);
            this.ValidateAndOutputEntity<CoreMetadataEntity>(servicePropertyEntity, CoreEntityType.MetadataV1, CoreEntityConstants.DefaultEntityScore, CoreServicePropertyEntity.CreateLookupKey("txtvers", CoreServicePropertyType.TxtVers));
            _ = (await this.TestMetadataEntityDatabase.InsertMetadataEntityAsync(servicePropertyEntity)).Should().Be(1);
            _ = (await this.TestMetadataEntityDatabase.SQLiteAsyncConnection.Table<CoreMetadataEntity>().CountAsync()).Should().Be(1);
        }
    }
}
