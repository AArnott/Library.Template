// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-27-2020
// // ***********************************************************************
// <copyright file="CoreFirewallIntegrationTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary>Core Firewall integration tests.</summary>
using FluentAssertions;
using NetworkVisor.Core.CoreApp.Settings;
using NetworkVisor.Core.Networking.Firewall;
using NetworkVisor.Core.Networking.Firewall.Addresses;
using NetworkVisor.Core.Networking.Types;
using NetworkVisor.Core.Test.Extensions;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Networking.Firewall
{
    /// <summary>
    /// Class CoreFirewallIntegrationTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreFirewallIntegrationTests))]

    public class CoreFirewallIntegrationTests : CoreTestCaseBase
    {
        private const string? SkipReason = "Local Dev Environment Only";

        private static int rulesBase = 20000;
        private readonly string rulesPrefix = $"_CFW_TEST_{Guid.NewGuid():N}_";
        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreFirewallIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreFirewallIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        public static bool RunLocalFirewallTests => CoreAppConstants.IsLocalDevEnvironment;

        [Fact(SkipUnless = nameof(RunLocalFirewallTests), Skip = SkipReason)]
        public void NetworkFirewall_IsServiceSupported_ReadOnly()
        {
            this.TestOutputHelper.WriteLine(
                !this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.LocalFirewallReadOnly)
                    ? $"{CoreNetworkServiceTypes.LocalFirewallReadOnly} is NOT available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})"
                    : $"{CoreNetworkServiceTypes.LocalFirewallReadOnly} is available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");
        }

        [Fact(SkipUnless = nameof(RunLocalFirewallTests), Skip = SkipReason)]
        public void NetworkFirewall_IsServiceSupported_ReadWrite()
        {
            this.TestOutputHelper.WriteLine(
                !this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.LocalFirewallReadWrite)
                    ? $"{CoreNetworkServiceTypes.LocalFirewallReadWrite} is NOT available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})"
                    : $"{CoreNetworkServiceTypes.LocalFirewallReadWrite} is available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");
        }

        [Fact(SkipUnless = nameof(RunLocalFirewallTests), Skip = SkipReason)]
        public void NetworkFirewall_IsServiceRunning()
        {
            this.TestOutputHelper.WriteLine($"Firewall Service Running: {this.TestNetworkFirewall.IsServiceRunning}");
        }

        [Fact(SkipUnless = nameof(RunLocalFirewallTests), Skip = SkipReason)]
        public void NetworkFirewall_IsReadOnly()
        {
            this.TestOutputHelper.WriteLine($"Firewall IsReadOnly: {this.TestNetworkFirewall.IsReadOnly}");
            this.TestNetworkFirewall.IsReadOnly.Should().NotBe(this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.LocalFirewallReadWrite));
        }

        [Fact(SkipUnless = nameof(RunLocalFirewallTests), Skip = SkipReason)]
        public void NetworkFirewall_APIVersion()
        {
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.LocalFirewallReadOnly))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.LocalFirewallReadOnly} is not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");
                this.TestNetworkFirewall.APIVersion.Should().Be(0);
                return;
            }

            this.TestOutputHelper.WriteLine($"Firewall API Version: {this.TestNetworkFirewall.APIVersion}");
            this.TestNetworkFirewall.APIVersion.Should().BeGreaterThanOrEqualTo(6);
        }

        [Fact(SkipUnless = nameof(RunLocalFirewallTests), Skip = SkipReason)]
        public void NetworkFirewall_IsProfileActive()
        {
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.LocalFirewallReadOnly))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.LocalFirewallReadOnly} is not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");
                this.TestNetworkFirewall.GetActiveProfile().Should().BeNull();

                return;
            }

            this.TestOutputHelper.WriteLine($"Active Profile: {this.TestNetworkFirewall.GetActiveProfile()!.Type}");
            this.TestOutputHelper.WriteLine($"Enabled:        {this.TestNetworkFirewall.GetActiveProfile()!.IsActive}");
            this.TestNetworkFirewall.GetActiveProfile()!.IsActive.Should().Be(this.TestNetworkFirewall.IsServiceRunning);
        }

        [Fact(SkipUnless = nameof(RunLocalFirewallTests), Skip = SkipReason)]
        public void NetworkFirewall_ListAllRules()
        {
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.LocalFirewallReadOnly))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.LocalFirewallReadOnly} is not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");
                this.TestNetworkFirewall.Rules.Should().BeEmpty();
                return;
            }

            foreach (ICoreFirewallRule? rule in this.TestNetworkFirewall.Rules)
            {
                this.OutputFirewallRule(rule);
            }
        }

        [Fact(SkipUnless = nameof(RunLocalFirewallTests), Skip = SkipReason)]
        public void NetworkFirewall_IsMulticastInboundUdpPortOpen()
        {
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.LocalFirewallReadOnly))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.LocalFirewallReadOnly} is not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");
                return;
            }

            this.TestOutputHelper.WriteLine(this.TestFileSystem.EntryAssemblyPath);
            this.TestOutputHelper.WriteLine();

            var allRules = this.TestNetworkFirewall.FindFirewallRules(r => r.IsEnable && r.Protocol == CoreFirewallProtocol.UDP && r.Direction == CoreFirewallDirection.Inbound && r.LocalPorts.Contains<ushort>(5353)).ToList();

            foreach (ICoreFirewallRule? rule in allRules)
            {
                this.OutputFirewallRule(rule);
            }
        }

        [Fact(SkipUnless = nameof(RunLocalFirewallTests), Skip = SkipReason)]
        public void NetworkFirewall_CreateDirectMultiProfileApplicationRule()
        {
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.LocalFirewallReadOnly))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.LocalFirewallReadOnly} updates are not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");
                return;
            }

            string ruleName = this.rulesPrefix + Guid.NewGuid().ToString("N");
            using FileStream tempFileStream = this.TestFileSystem.CreateLocalUserAppTempFileStream();
            tempFileStream.Should().NotBeNull();
            this.TestNetworkFirewall.Should().NotBeNull();

            ICoreFirewallRule? rule = this.TestNetworkFirewall.CreateApplicationRule(
                CoreFirewallProfiles.Domain | CoreFirewallProfiles.Private,
                ruleName,
                tempFileStream.Name);

            rule.Should().NotBeNull();

            if (this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.LocalFirewallReadWrite))
            {
                this.TestNetworkFirewall.AddFirewallRule(rule!).Should().BeTrue();

                ICoreFirewallRule? checkRule = this.TestNetworkFirewall.FindFirstOrDefaultFirewallRule(firewallRule => firewallRule.Name == ruleName);
                checkRule.Should().NotBeNull();
                checkRule!.ApplicationName.ToLower().Should().Be(tempFileStream.Name.ToLower());
                checkRule.Profiles.Should().Be(CoreFirewallProfiles.Domain | CoreFirewallProfiles.Private);

                this.TestNetworkFirewall.RemoveFirewallRule(rule!).Should().BeTrue();

                checkRule = this.TestNetworkFirewall.FindFirstOrDefaultFirewallRule(firewallRule => firewallRule.Name == ruleName);
                checkRule.Should().BeNull();
            }
            else
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.LocalFirewallReadWrite} updates are not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");
            }
        }

        [Fact(SkipUnless = nameof(RunLocalFirewallTests), Skip = SkipReason)]
        public void NetworkFirewall_CreateDirectMultiProfilePortRule()
        {
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.LocalFirewallReadOnly))
            {
                this.TestOutputHelper.WriteLine(
                    $"{CoreNetworkServiceTypes.LocalFirewallReadOnly} updates are not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");
                return;
            }

            if (this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.LocalFirewallReadWrite))
            {
                string ruleName = this.rulesPrefix + Guid.NewGuid().ToString("N");
                ushort portNumber = this.GetRandomPort();
                this.TestNetworkFirewall.Should().NotBeNull();

                ICoreFirewallRule? rule = this.TestNetworkFirewall.CreatePortRule(
                    CoreFirewallProfiles.Domain | CoreFirewallProfiles.Private,
                    ruleName,
                    portNumber);

                rule.Should().NotBeNull();

                this.TestNetworkFirewall.AddFirewallRule(rule!).Should().BeTrue();

                ICoreFirewallRule? checkRule =
                    this.TestNetworkFirewall.FindFirstOrDefaultFirewallRule(firewallRule =>
                        firewallRule.Name == ruleName);
                checkRule.Should().NotBeNull();
                checkRule!.LocalPorts.Contains(portNumber).Should().BeTrue();
                checkRule.Profiles.Should().Be(CoreFirewallProfiles.Domain | CoreFirewallProfiles.Private);

                this.TestNetworkFirewall.RemoveFirewallRule(rule!).Should().BeTrue();

                checkRule = this.TestNetworkFirewall.FindFirstOrDefaultFirewallRule(firewallRule =>
                    firewallRule.Name == ruleName);
                checkRule.Should().BeNull();
            }
            else
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.LocalFirewallReadWrite} updates are not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");
            }
        }

        [Fact(SkipUnless = nameof(RunLocalFirewallTests), Skip = SkipReason)]
        public void NetworkFirewall_CreateIndirectApplicationRule()
        {
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.LocalFirewallReadOnly))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.LocalFirewallReadOnly} updates are not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");
                return;
            }

            if (this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.LocalFirewallReadWrite))
            {
                string ruleName1 = this.rulesPrefix + Guid.NewGuid().ToString("N");
                string ruleName2 = this.rulesPrefix + Guid.NewGuid().ToString("N");
                string ruleName3 = this.rulesPrefix + Guid.NewGuid().ToString("N");

                using FileStream tempFileStream1 = this.TestFileSystem.CreateLocalUserAppTempFileStream();
                using FileStream tempFileStream2 = this.TestFileSystem.CreateLocalUserAppTempFileStream();
                using FileStream tempFileStream3 = this.TestFileSystem.CreateLocalUserAppTempFileStream();

                ICoreFirewallRule? rule1 = this.TestNetworkFirewall.CreateApplicationRule(
                    CoreFirewallProfiles.Private,
                    ruleName1,
                    tempFileStream1.Name);

                ICoreFirewallRule? rule2 = this.TestNetworkFirewall.CreateApplicationRule(
                    CoreFirewallProfiles.Private,
                    ruleName2,
                    CoreFirewallAction.Allow,
                    tempFileStream2.Name);

                ICoreFirewallRule? rule3 = this.TestNetworkFirewall.CreateApplicationRule(
                    CoreFirewallProfiles.Private,
                    ruleName3,
                    CoreFirewallAction.Allow,
                    [],
                    CoreFirewallProtocol.Any,
                    tempFileStream3.Name);

                rule1.Should().NotBeNull();
                rule2.Should().NotBeNull();
                rule3.Should().NotBeNull();

                this.TestNetworkFirewall.AddFirewallRule(rule1!).Should().BeTrue();
                this.TestNetworkFirewall.AddFirewallRule(rule2!).Should().BeTrue();
                this.TestNetworkFirewall.AddFirewallRule(rule3!).Should().BeTrue();

                var rules = this.TestNetworkFirewall.Rules.ToList();

                ICoreFirewallRule? checkRule = rules.FirstOrDefault(firewallRule => firewallRule.Name == ruleName1);
                checkRule.Should().NotBeNull();
                checkRule!.ApplicationName.ToLower().Should().Be(tempFileStream1.Name.ToLower());
                checkRule.Profiles.Should().Be(CoreFirewallProfiles.Private);
                checkRule.Action.Should().Be(CoreFirewallAction.Allow);
                checkRule.Protocol.Should().Be(CoreFirewallProtocol.Any);

                checkRule = rules.FirstOrDefault(firewallRule => firewallRule.Name == ruleName2);
                checkRule.Should().NotBeNull();
                checkRule!.ApplicationName.ToLower().Should().Be(tempFileStream2.Name.ToLower());
                checkRule.Profiles.Should().Be(CoreFirewallProfiles.Private);
                checkRule.Action.Should().Be(CoreFirewallAction.Allow);
                checkRule.Protocol.Should().Be(CoreFirewallProtocol.Any);

                checkRule = rules.FirstOrDefault(firewallRule => firewallRule.Name == ruleName3);
                checkRule.Should().NotBeNull();
                checkRule!.ApplicationName.ToLower().Should().Be(tempFileStream3.Name.ToLower());
                checkRule.Profiles.Should().Be(CoreFirewallProfiles.Private);
                checkRule.Action.Should().Be(CoreFirewallAction.Allow);
                checkRule.Protocol.Should().Be(CoreFirewallProtocol.Any);

                this.TestNetworkFirewall.RemoveFirewallRule(rule1!).Should().BeTrue();
                this.TestNetworkFirewall.RemoveFirewallRule(rule2!).Should().BeTrue();
                this.TestNetworkFirewall.RemoveFirewallRule(rule3!).Should().BeTrue();

                checkRule = this.TestNetworkFirewall.FindFirstOrDefaultFirewallRule(firewallRule =>
                    firewallRule.Name == ruleName1);
                checkRule.Should().BeNull();
                checkRule = this.TestNetworkFirewall.FindFirstOrDefaultFirewallRule(firewallRule =>
                    firewallRule.Name == ruleName2);
                checkRule.Should().BeNull();
                checkRule = this.TestNetworkFirewall.FindFirstOrDefaultFirewallRule(firewallRule =>
                    firewallRule.Name == ruleName3);
                checkRule.Should().BeNull();
            }
            else
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.LocalFirewallReadWrite} updates are not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");
            }
        }

        [Fact(SkipUnless = nameof(RunLocalFirewallTests), Skip = SkipReason)]
        public void NetworkFirewall_CreateIndirectPortRule()
        {
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.LocalFirewallReadOnly))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.LocalFirewallReadOnly} updates are not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");
                return;
            }

            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.LocalFirewallReadWrite))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.LocalFirewallReadWrite} updates are not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");
                return;
            }

            string ruleName1 = this.rulesPrefix + Guid.NewGuid().ToString("N");
            string ruleName2 = this.rulesPrefix + Guid.NewGuid().ToString("N");
            string ruleName3 = this.rulesPrefix + Guid.NewGuid().ToString("N");
            string ruleName4 = this.rulesPrefix + Guid.NewGuid().ToString("N");

            ushort portNumber1 = this.GetRandomPort();
            ushort portNumber2 = this.GetRandomPort();
            ushort portNumber3 = this.GetRandomPort();
            ushort portNumber4 = this.GetRandomPort();

            ICoreFirewallRule? rule1 = this.TestNetworkFirewall.CreatePortRule(
                    CoreFirewallProfiles.Private,
                    ruleName1,
                    portNumber1);
            ICoreFirewallRule? rule2 = this.TestNetworkFirewall.CreatePortRule(
                    CoreFirewallProfiles.Private,
                    ruleName2,
                    CoreFirewallAction.Allow,
                    portNumber2);
            ICoreFirewallRule? rule3 = this.TestNetworkFirewall.CreatePortRule(
                    CoreFirewallProfiles.Private,
                    ruleName3,
                    CoreFirewallAction.Allow,
                    portNumber3,
                    CoreFirewallProtocol.TCP);
            ICoreFirewallRule? rule4 = this.TestNetworkFirewall.CreatePortRule(
                    CoreFirewallProfiles.Domain,
                    ruleName4,
                    CoreFirewallAction.Allow,
                    portNumber4,
                    CoreFirewallProtocol.UDP);

            rule1.Should().NotBeNull();
            rule2.Should().NotBeNull();
            rule3.Should().NotBeNull();
            rule4.Should().NotBeNull();

            this.TestNetworkFirewall.AddFirewallRule(rule1!).Should().BeTrue();
            this.TestNetworkFirewall.AddFirewallRule(rule2!).Should().BeTrue();
            this.TestNetworkFirewall.AddFirewallRule(rule3!).Should().BeTrue();
            this.TestNetworkFirewall.AddFirewallRule(rule4!).Should().BeTrue();

            var rules = this.TestNetworkFirewall.Rules.ToList();

            ICoreFirewallRule? checkRule = rules.FirstOrDefault(firewallRule => firewallRule.Name == ruleName1);
            checkRule.Should().NotBeNull();
            checkRule!.LocalPorts.Should().BeEquivalentTo(new[] { portNumber1 });
            checkRule.Profiles.Should().Be(CoreFirewallProfiles.Private);
            checkRule.Action.Should().Be(CoreFirewallAction.Allow);
            checkRule.Protocol.Should().Be(CoreFirewallProtocol.TCP);

            checkRule = rules.FirstOrDefault(firewallRule => firewallRule.Name == ruleName2);
            checkRule.Should().NotBeNull();
            checkRule!.LocalPorts.Should().BeEquivalentTo(new[] { portNumber2 });
            checkRule.Profiles.Should().Be(CoreFirewallProfiles.Private);
            checkRule.Action.Should().Be(CoreFirewallAction.Allow);
            checkRule.Protocol.Should().Be(CoreFirewallProtocol.TCP);

            checkRule = rules.FirstOrDefault(firewallRule => firewallRule.Name == ruleName3);
            checkRule.Should().NotBeNull();
            checkRule!.LocalPorts.Should().BeEquivalentTo(new[] { portNumber3 });
            checkRule.Profiles.Should().Be(CoreFirewallProfiles.Private);
            checkRule.Action.Should().Be(CoreFirewallAction.Allow);
            checkRule.Protocol.Should().Be(CoreFirewallProtocol.TCP);

            checkRule = rules.FirstOrDefault(firewallRule => firewallRule.Name == ruleName4);
            checkRule.Should().NotBeNull();
            checkRule!.LocalPorts.Should().BeEquivalentTo(new[] { portNumber4 });
            checkRule.Profiles.Should().Be(CoreFirewallProfiles.Domain);
            checkRule.Action.Should().Be(CoreFirewallAction.Allow);
            checkRule.Protocol.Should().Be(CoreFirewallProtocol.UDP);

            this.TestNetworkFirewall.RemoveFirewallRule(rule1!).Should().BeTrue();
            this.TestNetworkFirewall.RemoveFirewallRule(rule2!).Should().BeTrue();
            this.TestNetworkFirewall.RemoveFirewallRule(rule3!).Should().BeTrue();
            this.TestNetworkFirewall.RemoveFirewallRule(rule4!).Should().BeTrue();

            checkRule = this.TestNetworkFirewall.FindFirstOrDefaultFirewallRule(firewallRule =>
                    firewallRule.Name == ruleName1);
            checkRule.Should().BeNull();
            checkRule = this.TestNetworkFirewall.FindFirstOrDefaultFirewallRule(firewallRule =>
                    firewallRule.Name == ruleName2);
            checkRule.Should().BeNull();
            checkRule = this.TestNetworkFirewall.FindFirstOrDefaultFirewallRule(firewallRule =>
                    firewallRule.Name == ruleName3);
            checkRule.Should().BeNull();
            checkRule = this.TestNetworkFirewall.FindFirstOrDefaultFirewallRule(firewallRule =>
                    firewallRule.Name == ruleName4);
            checkRule.Should().BeNull();
        }

        [Fact(SkipUnless = nameof(RunLocalFirewallTests), Skip = SkipReason)]
        public void NetworkFirewall_IsInBoundPortOpen_RandomPort()
        {
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.LocalFirewallReadOnly))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.LocalFirewallReadOnly} is not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");
                this.TestNetworkFirewall.GetActiveProfile().Should().BeNull();

                return;
            }

            ushort portNumber = this.GetRandomPort();

            this.TestNetworkFirewall.IsInBoundPortOpen(portNumber, CoreFirewallProtocol.UDP).Should().BeFalse();
        }

        [Fact(SkipUnless = nameof(RunLocalFirewallTests), Skip = SkipReason)]
        public void NetworkFirewall_IsInBoundPortOpen_Existing()
        {
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.LocalFirewallReadOnly))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.LocalFirewallReadOnly} is not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");
                this.TestNetworkFirewall.GetActiveProfile().Should().BeNull();

                return;
            }

            string ruleName1 = this.rulesPrefix + Guid.NewGuid().ToString("N");
            ushort portNumber = this.GetRandomPort();

            this.TestNetworkFirewall.IsInBoundPortOpen(portNumber, CoreFirewallProtocol.UDP).Should().BeFalse();

            if (this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.LocalFirewallReadWrite))
            {
                ICoreFirewallRule? rule1 = this.TestNetworkFirewall.CreatePortRule(
                    CoreFirewallProfiles.Private,
                    ruleName1,
                    CoreFirewallAction.Allow,
                    portNumber,
                    CoreFirewallProtocol.UDP);

                rule1.Should().NotBeNull();
                rule1!.Direction = CoreFirewallDirection.Inbound;

                this.TestNetworkFirewall.AddFirewallRule(rule1).Should().BeTrue();

                this.TestNetworkFirewall.IsInBoundPortOpen(portNumber, CoreFirewallProtocol.UDP).Should().BeTrue();

                this.TestNetworkFirewall.RemoveFirewallRule(rule1).Should().BeTrue();
            }
            else
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.LocalFirewallReadWrite} updates are not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");
            }
        }

        [Fact(SkipUnless = nameof(RunLocalFirewallTests), Skip = SkipReason)]
        public void NetworkFirewall_IsInBoundPortOpen_MaxPort()
        {
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.LocalFirewallReadOnly))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.LocalFirewallReadOnly} is not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");
                this.TestNetworkFirewall.GetActiveProfile().Should().BeNull();

                return;
            }

            this.TestNetworkFirewall.IsInBoundPortOpen(ushort.MaxValue, CoreFirewallProtocol.IGMP).Should().BeFalse();
        }

        [Fact(SkipUnless = nameof(RunLocalFirewallTests), Skip = SkipReason)]
        public void NetworkFirewall_IsOutBoundPortOpen_RandomPort()
        {
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.LocalFirewallReadOnly))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.LocalFirewallReadOnly} is not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");
                this.TestNetworkFirewall.GetActiveProfile().Should().BeNull();

                return;
            }

            ushort portNumber = this.GetRandomPort();

            this.TestNetworkFirewall.IsOutBoundPortOpen(portNumber, CoreFirewallProtocol.UDP).Should().BeFalse();
        }

        [Fact(SkipUnless = nameof(RunLocalFirewallTests), Skip = SkipReason)]
        public void NetworkFirewall_IsOutBoundPortOpen_Existing()
        {
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.LocalFirewallReadOnly))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.LocalFirewallReadOnly} is not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");
                this.TestNetworkFirewall.GetActiveProfile().Should().BeNull();

                return;
            }

            string ruleName1 = this.rulesPrefix + Guid.NewGuid().ToString("N");
            ushort portNumber = this.GetRandomPort();

            this.TestNetworkFirewall.IsOutBoundPortOpen(portNumber, CoreFirewallProtocol.UDP).Should().BeFalse();

            if (this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.LocalFirewallReadWrite))
            {
                ICoreFirewallRule? rule1 = this.TestNetworkFirewall.CreatePortRule(
                    CoreFirewallProfiles.Private,
                    ruleName1,
                    CoreFirewallAction.Allow,
                    portNumber,
                    CoreFirewallProtocol.UDP);

                rule1.Should().NotBeNull();
                rule1!.Direction = CoreFirewallDirection.Outbound;

                this.TestNetworkFirewall.AddFirewallRule(rule1).Should().BeTrue();

                this.TestNetworkFirewall.IsOutBoundPortOpen(portNumber, CoreFirewallProtocol.UDP).Should().BeTrue();

                this.TestNetworkFirewall.RemoveFirewallRule(rule1).Should().BeTrue();
            }
            else
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.LocalFirewallReadWrite} updates are not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");
            }
        }

        [Fact(SkipUnless = nameof(RunLocalFirewallTests), Skip = SkipReason)]
        public void NetworkFirewall_IsInBoundPortOpenForRunningApplication_Existing()
        {
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.LocalFirewallReadOnly))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.LocalFirewallReadOnly} is not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");
                this.TestNetworkFirewall.GetActiveProfile().Should().BeNull();

                return;
            }

            string ruleName1 = this.rulesPrefix + Guid.NewGuid().ToString("N");
            ushort portNumber = this.GetRandomPort();

            this.TestOutputHelper.WriteLine($"Running Application Path: {this.TestNetworkFirewall.RunningApplicationPath}");

            // Checking without allowing any port
            this.TestNetworkFirewall.IsInBoundPortOpenForRunningApplication(portNumber, CoreFirewallProtocol.UDP, false).Should().BeFalse();

            if (this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.LocalFirewallReadWrite))
            {
                ICoreFirewallRule? rule1 = this.TestNetworkFirewall.CreateApplicationRule(
                    CoreFirewallProfiles.Private,
                    ruleName1,
                    CoreFirewallAction.Allow,
                    [portNumber],
                    CoreFirewallProtocol.UDP);

                rule1.Should().NotBeNull();
                rule1!.Direction = CoreFirewallDirection.Inbound;

                this.TestNetworkFirewall.AddFirewallRule(rule1).Should().BeTrue();

                this.TestNetworkFirewall.IsInBoundPortOpenForRunningApplication(portNumber, CoreFirewallProtocol.UDP, false)
                    .Should().BeTrue();

                this.TestNetworkFirewall.RemoveFirewallRule(rule1).Should().BeTrue();
            }
            else
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.LocalFirewallReadWrite} updates are not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");
            }
        }

        [Fact(SkipUnless = nameof(RunLocalFirewallTests), Skip = SkipReason)]
        public void NetworkFirewall_IsOutBoundPortOpen_MaxPort()
        {
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.LocalFirewallReadOnly))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.LocalFirewallReadOnly} is not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");
                this.TestNetworkFirewall.GetActiveProfile().Should().BeNull();

                return;
            }

            this.TestNetworkFirewall.IsOutBoundPortOpen(ushort.MaxValue, CoreFirewallProtocol.IGMP).Should().BeFalse();
        }

        [Fact(SkipUnless = nameof(RunLocalFirewallTests), Skip = SkipReason)]
        public void NetworkFirewall_Profiles()
        {
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.LocalFirewallReadOnly))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.LocalFirewallReadOnly} is not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");
                this.TestNetworkFirewall.Profiles.Should().BeEmpty();
                return;
            }

            CoreFirewallProfiles[] profiles = this.TestNetworkFirewall.Profiles.Select(profile => profile.Type).ToArray();
            profiles.Should().NotBeNull();

            profiles.Length.Should().Be(3);
            profiles.All(type => new[] { CoreFirewallProfiles.Domain, CoreFirewallProfiles.Public, CoreFirewallProfiles.Private }.Contains(type)).Should().BeTrue();
            this.TestNetworkFirewall.GetProfile(CoreFirewallProfiles.Domain).Should().NotBeNull();
            this.TestNetworkFirewall.GetProfile(CoreFirewallProfiles.Private).Should().NotBeNull();
            this.TestNetworkFirewall.GetProfile(CoreFirewallProfiles.Public).Should().NotBeNull();
        }

        protected override void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                try
                {
                    if (disposing)
                    {
                        if (this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.LocalFirewallReadWrite))
                        {
                            IReadOnlyCollection<ICoreFirewallRule> rulesToBeDeleted = this.TestNetworkFirewall.FindFirewallRules(rule => rule.Name.StartsWith(this.rulesPrefix));

                            foreach (ICoreFirewallRule? firewallRule in rulesToBeDeleted)
                            {
                                this.TestNetworkFirewall.RemoveFirewallRule(firewallRule);
                            }
                        }
                    }
                }
                finally
                {
                    this.disposedValue = true;
                }
            }

            base.Dispose(disposing);
        }

        private ushort GetRandomPort()
        {
            return (ushort)(Interlocked.Increment(ref rulesBase) + (DateTime.UtcNow.Ticks % 10000));
        }

        private void OutputFirewallRule(ICoreFirewallRule? rule)
        {
            rule.Should().NotBeNull();

            this.TestOutputHelper.WriteLine($"Name: {rule!.Name}");
            this.TestOutputHelper.WriteLine($"Action: {rule.Action}");
            this.TestOutputHelper.WriteLine($"ApplicationName: [{rule.ApplicationName}]");
            this.TestOutputHelper.WriteLine($"Direction: {rule.Direction}");
            this.TestOutputHelper.WriteLine($"FriendlyName: {rule.FriendlyName}");
            this.TestOutputHelper.WriteLine($"IsEnable: {rule.IsEnable}");
            this.TestOutputHelper.WriteLine($"LocalAddresses: [{string.Join<ICoreFirewallAddress>(",", rule.LocalAddresses)}]");
            this.TestOutputHelper.WriteLine($"LocalPorts: [{string.Join(",", rule.LocalPorts)}]");
            this.TestOutputHelper.WriteLine($"LocalPortType: {rule.LocalPortType}");
            this.TestOutputHelper.WriteLine($"Profiles: [{rule.Profiles}]");
            this.TestOutputHelper.WriteLine($"Protocol: {rule.Protocol}");
            this.TestOutputHelper.WriteLine($"RemoteAddresses: [{string.Join<ICoreFirewallAddress>(",", rule.RemoteAddresses)}]");
            this.TestOutputHelper.WriteLine($"RemotePorts: [{string.Join(",", rule.RemotePorts)}]");
            this.TestOutputHelper.WriteLine($"Scope: {rule.Scope}");
            this.TestOutputHelper.WriteLine($"ServiceName: {rule.ServiceName}");

            this.TestOutputHelper.WriteLine();
        }
    }
}
