// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-26-2020
// // ***********************************************************************
// <copyright file="CoreEncryptionBouncyCastleUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using FluentAssertions;
using NetworkVisor.Core.Encryption;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Org.BouncyCastle.Crypto;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Encryption
{
    /// <summary>
    /// Class CoreEncryptionBouncyCastleUnitTests.
    /// </summary>
    [PlatformTrait(typeof(CoreEncryptionBouncyCastleUnitTests))]

    public class CoreEncryptionBouncyCastleUnitTests : CoreTestCaseBase
    {
        private static readonly string PlainText = "This is just some *plain* text & over 50+ characters!";
        private static readonly byte[] DefaultKey = [132, 19, 13, 129, 1, 135, 31, 237, 242, 183, 80, 235, 21, 84, 21, 210, 140, 231, 99, 144, 75, 186, 3, 76, 61, 80, 251, 238, 237, 23, 120, 167];
        private static readonly byte[] BadKey = [13, 19, 13, 129, 1, 135, 31, 237, 242, 183, 80, 235, 21, 84, 21, 210, 140, 231, 99, 144, 75, 186, 3, 76, 61, 80, 251, 238, 237, 23, 120, 167];
        private static readonly string DefaultPassword = "1@M@STR0N6!PWD#";
        private static readonly byte[] DefaultNonSecretPayload = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9];

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreEncryptionBouncyCastleUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreEncryptionBouncyCastleUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void EncryptionBouncyCastle_SimpleEncrypt_RandomKey()
        {
            var key = CoreEncryptionBouncyCastle.NewKey();

            (string? EncryptedText, Exception? Exception)? encryptedTextWithException = CoreEncryptionBouncyCastle.SimpleEncrypt(PlainText, key);
            encryptedTextWithException.Should().NotBeNull();
            encryptedTextWithException!.Value.Exception.Should().BeNull();
            encryptedTextWithException.Value.EncryptedText.Should().NotBeNull();

            this.TestOutputHelper.WriteLine($"Encrypted Text: {encryptedTextWithException!.Value.EncryptedText}");

            (string? PlainText, Exception? Exception)? plainTextTestWithException = CoreEncryptionBouncyCastle.SimpleDecrypt(encryptedTextWithException.Value.EncryptedText!, key);
            plainTextTestWithException.Should().NotBeNull();

            this.TestOutputHelper.WriteLine($"Decrypted Text: {plainTextTestWithException!.Value.PlainText}");

            plainTextTestWithException.Value.Exception.Should().BeNull();
            plainTextTestWithException.Value.PlainText.Should().NotBeNullOrEmpty();
            plainTextTestWithException.Value.PlainText.Should().Be(PlainText);
        }

        [Fact]
        public void EncryptionBouncyCastle_SimpleEncrypt_DefaultKey()
        {
            var key = CoreEncryptionBouncyCastle.NewKey();

            (string? EncryptedText, Exception? Exception)? encryptedTextWithException = CoreEncryptionBouncyCastle.SimpleEncrypt(PlainText, DefaultKey);
            encryptedTextWithException.Should().NotBeNull();
            encryptedTextWithException!.Value.EncryptedText.Should().NotBeNullOrEmpty();

            this.TestOutputHelper.WriteLine($"Encrypted Text: {encryptedTextWithException.Value.EncryptedText}");

            (string? PlainText, Exception? Exception)? plainTextTestWithException = CoreEncryptionBouncyCastle.SimpleDecrypt(encryptedTextWithException.Value.EncryptedText!, DefaultKey);
            plainTextTestWithException.Should().NotBeNull();
            plainTextTestWithException!.Value.Exception.Should().BeNull();
            plainTextTestWithException!.Value.PlainText.Should().NotBeNullOrEmpty();

            this.TestOutputHelper.WriteLine($"Decrypted Text: {plainTextTestWithException.Value.PlainText}");

            plainTextTestWithException.Value.PlainText.Should().NotBeNullOrEmpty();
            plainTextTestWithException.Value.PlainText.Should().Be(PlainText);
        }

        [Fact]
        public void EncryptionBouncyCastle_SimpleEncrypt_BadKey()
        {
            var key = CoreEncryptionBouncyCastle.NewKey();

            (string? EncryptedText, Exception? Exception)? encryptedTextWithException = CoreEncryptionBouncyCastle.SimpleEncrypt(PlainText, DefaultKey);
            encryptedTextWithException.Should().NotBeNull();
            encryptedTextWithException!.Value.EncryptedText.Should().NotBeNullOrEmpty();

            this.TestOutputHelper.WriteLine($"Encrypted Text: {encryptedTextWithException.Value.EncryptedText}");

            (string? PlainText, Exception? Exception)? plainTextTestWithException = CoreEncryptionBouncyCastle.SimpleDecrypt(encryptedTextWithException.Value.EncryptedText!, BadKey);
            plainTextTestWithException.Should().NotBeNull();
            plainTextTestWithException!.Value.Exception.Should().BeOfType<InvalidCipherTextException>();
            plainTextTestWithException!.Value.PlainText.Should().BeNull();
        }

        [Fact]
        public void EncryptionBouncyCastle_SimpleEncrypt_DefaultKey_NonSecretPayload()
        {
            var key = CoreEncryptionBouncyCastle.NewKey();

            (string? EncryptedText, Exception? Exception)? encryptedTextWithException = CoreEncryptionBouncyCastle.SimpleEncrypt(PlainText, DefaultKey, DefaultNonSecretPayload);
            encryptedTextWithException.Should().NotBeNull();
            encryptedTextWithException!.Value.EncryptedText.Should().NotBeNullOrEmpty();

            this.TestOutputHelper.WriteLine($"Encrypted Text: {encryptedTextWithException.Value.EncryptedText}");

            (string? PlainText, Exception? Exception)? plainTextTestWithException = CoreEncryptionBouncyCastle.SimpleDecrypt(encryptedTextWithException.Value.EncryptedText!, DefaultKey, DefaultNonSecretPayload.Length);
            plainTextTestWithException.Should().NotBeNull();
            plainTextTestWithException!.Value.Exception.Should().BeNull();
            plainTextTestWithException!.Value.PlainText.Should().NotBeNullOrEmpty();

            this.TestOutputHelper.WriteLine($"Decrypted Text: {plainTextTestWithException.Value.PlainText}");

            plainTextTestWithException.Value.PlainText.Should().NotBeNullOrEmpty();
            plainTextTestWithException.Value.PlainText.Should().Be(PlainText);
        }

        [Fact]
        public void EncryptionBouncyCastle_SimpleEncrypt_DefaultKey_EncryptTwice()
        {
            var key = CoreEncryptionBouncyCastle.NewKey();

            (string? EncryptedText, Exception? Exception)? encryptedTextWithException = CoreEncryptionBouncyCastle.SimpleEncrypt(PlainText, DefaultKey);
            encryptedTextWithException.Should().NotBeNull();
            encryptedTextWithException!.Value.EncryptedText.Should().NotBeNullOrEmpty();

            this.TestOutputHelper.WriteLine($"Encrypted Text: {encryptedTextWithException.Value.EncryptedText}");

            (string? PlainText, Exception? Exception)? plainTextTestWithException = CoreEncryptionBouncyCastle.SimpleDecrypt(encryptedTextWithException!.Value.EncryptedText!, DefaultKey);
            plainTextTestWithException.Should().NotBeNull();
            plainTextTestWithException!.Value.Exception.Should().BeNull();
            plainTextTestWithException!.Value.PlainText.Should().NotBeNullOrEmpty();

            this.TestOutputHelper.WriteLine($"Decrypted Text: {plainTextTestWithException.Value.PlainText}");

            plainTextTestWithException.Value.PlainText.Should().NotBeNullOrEmpty();
            plainTextTestWithException.Value.PlainText.Should().Be(PlainText);

            (string? EncryptedText, Exception? Exception)? encryptedTextWithException2 = CoreEncryptionBouncyCastle.SimpleEncrypt(PlainText, DefaultKey);
            encryptedTextWithException2.Should().NotBeNull();
            encryptedTextWithException2!.Value.Exception.Should().BeNull();
            encryptedTextWithException2!.Value.EncryptedText.Should().NotBeNullOrEmpty();

            encryptedTextWithException2.Value.EncryptedText.Should().NotBe(encryptedTextWithException.Value.EncryptedText);

            this.TestOutputHelper.WriteLine();

            this.TestOutputHelper.WriteLine($"2nd Encrypted Text: {encryptedTextWithException2.Value.EncryptedText}");

            plainTextTestWithException = CoreEncryptionBouncyCastle.SimpleDecrypt(encryptedTextWithException2.Value.EncryptedText!, DefaultKey);
            plainTextTestWithException.Should().NotBeNull();

            this.TestOutputHelper.WriteLine($"2nd Decrypted Text: {plainTextTestWithException!.Value.PlainText}");

            plainTextTestWithException!.Value.Exception.Should().BeNull();
            plainTextTestWithException!.Value.PlainText.Should().NotBeNullOrEmpty();
            plainTextTestWithException!.Value.PlainText.Should().Be(PlainText);
        }

        [Fact]
        public void EncryptionBouncyCastle_SimpleEncrypt_PlainText_Null()
        {
            var key = CoreEncryptionBouncyCastle.NewKey();

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Action act = () => CoreEncryptionBouncyCastle.SimpleEncrypt((string)null, key);

            act.Should().Throw<ArgumentException>().WithParameterName("secretMessage");

            act = () => CoreEncryptionBouncyCastle.SimpleDecrypt((string)null, key);

            act.Should().Throw<ArgumentException>().WithParameterName("encryptedMessage");

#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        }

        [Fact]
        public void EncryptionBouncyCastle_SimpleEncrypt_PlainText_Empty()
        {
            var key = CoreEncryptionBouncyCastle.NewKey();

            Action act = () => CoreEncryptionBouncyCastle.SimpleEncrypt(string.Empty, key);

            act.Should().Throw<ArgumentException>().WithParameterName("secretMessage");
        }

        [Fact]
        public void EncryptionBouncyCastle_SimpleEncrypt_WithPassword()
        {
            var key = CoreEncryptionBouncyCastle.NewKey();

            (string? EncryptedText, Exception? Exception)? encryptedTextWithException = CoreEncryptionBouncyCastle.SimpleEncryptWithPassword(PlainText, DefaultPassword);
            encryptedTextWithException.Should().NotBeNull();
            encryptedTextWithException!.Value.Exception.Should().BeNull();
            encryptedTextWithException.Value.EncryptedText.Should().NotBeNullOrEmpty();

            this.TestOutputHelper.WriteLine($"Encrypted Text: {encryptedTextWithException.Value.EncryptedText}");

            (string? PlainText, Exception? Exception)? plainTextTestWithException = CoreEncryptionBouncyCastle.SimpleDecryptWithPassword(encryptedTextWithException.Value.EncryptedText!, DefaultPassword);
            plainTextTestWithException.Should().NotBeNull();
            plainTextTestWithException!.Value.Exception.Should().BeNull();
            plainTextTestWithException.Value.PlainText.Should().NotBeNull();

            this.TestOutputHelper.WriteLine($"Decrypted Text: {plainTextTestWithException.Value.PlainText}");

            plainTextTestWithException.Value.PlainText.Should().NotBeNullOrEmpty();
            plainTextTestWithException.Value.PlainText.Should().Be(PlainText);
        }

        [Fact]
        public void EncryptionBouncyCastle_SimpleEncrypt_WithPassword_NonSecretPayload()
        {
            var key = CoreEncryptionBouncyCastle.NewKey();

            (string? EncryptedText, Exception? Exception)? encryptedTextWithException = CoreEncryptionBouncyCastle.SimpleEncryptWithPassword(PlainText, DefaultPassword, DefaultNonSecretPayload);
            encryptedTextWithException.Should().NotBeNull();
            encryptedTextWithException!.Value.EncryptedText.Should().NotBeNullOrEmpty();

            this.TestOutputHelper.WriteLine($"Encrypted Text: {encryptedTextWithException.Value.EncryptedText}");

            (string? PlainText, Exception? Exception)? plainTextWithException = CoreEncryptionBouncyCastle.SimpleDecryptWithPassword(encryptedTextWithException.Value.EncryptedText!, DefaultPassword, DefaultNonSecretPayload.Length);
            plainTextWithException.Should().NotBeNull();
            plainTextWithException!.Value.Exception.Should().BeNull();
            plainTextWithException!.Value.PlainText.Should().NotBeNullOrEmpty();

            this.TestOutputHelper.WriteLine($"Decrypted Text: {plainTextWithException.Value.PlainText}");

            plainTextWithException.Value.PlainText.Should().NotBeNullOrEmpty();
            plainTextWithException.Value.PlainText.Should().Be(PlainText);
        }

        [Fact]
        public void EncryptionBouncyCastle_SimpleEncrypt_WithPassword_EncryptTwice()
        {
            var key = CoreEncryptionBouncyCastle.NewKey();

            (string? EncryptedText, Exception? Exception)? encryptedTextWithException = CoreEncryptionBouncyCastle.SimpleEncryptWithPassword(PlainText, DefaultPassword);
            encryptedTextWithException.Should().NotBeNull();
            encryptedTextWithException!.Value.EncryptedText.Should().NotBeNullOrEmpty();

            this.TestOutputHelper.WriteLine($"Encrypted Text: {encryptedTextWithException.Value.EncryptedText}");

            (string? PlainText, Exception? Exception)? plainTextTestWithException = CoreEncryptionBouncyCastle.SimpleDecryptWithPassword(encryptedTextWithException.Value.EncryptedText!, DefaultPassword);
            plainTextTestWithException.Should().NotBeNull();
            plainTextTestWithException!.Value.Exception.Should().BeNull();
            plainTextTestWithException!.Value.PlainText.Should().NotBeNullOrEmpty();

            this.TestOutputHelper.WriteLine($"Decrypted Text: {plainTextTestWithException.Value.PlainText}");

            plainTextTestWithException.Value.PlainText.Should().NotBeNullOrEmpty();
            plainTextTestWithException.Value.PlainText.Should().Be(PlainText);

            (string? EncryptedText, Exception? Exception)? encryptedText2WithException = CoreEncryptionBouncyCastle.SimpleEncryptWithPassword(PlainText, DefaultPassword);
            encryptedText2WithException.Should().NotBeNull();
            encryptedText2WithException!.Value.Exception.Should().BeNull();
            encryptedText2WithException!.Value.EncryptedText.Should().NotBeNullOrEmpty();

            encryptedText2WithException.Should().NotBe(encryptedText2WithException.Value.EncryptedText);

            this.TestOutputHelper.WriteLine();

            this.TestOutputHelper.WriteLine($"2nd Encrypted Text: {encryptedText2WithException.Value.EncryptedText}");

            plainTextTestWithException = CoreEncryptionBouncyCastle.SimpleDecryptWithPassword(encryptedText2WithException!.Value.EncryptedText!, DefaultPassword);
            plainTextTestWithException.Should().NotBeNull();
            plainTextTestWithException!.Value.Exception.Should().BeNull();
            plainTextTestWithException!.Value.PlainText.Should().NotBeNullOrEmpty();

            this.TestOutputHelper.WriteLine($"Decrypted Text: {plainTextTestWithException.Value.PlainText}");

            plainTextTestWithException.Value.PlainText.Should().NotBeNullOrEmpty();
            plainTextTestWithException.Value.PlainText.Should().Be(PlainText);
        }

        [Fact]
        public void EncryptionBouncyCastle_SimpleEncrypt_PlainText_WithPassword_Null()
        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Action act = () => CoreEncryptionBouncyCastle.SimpleEncryptWithPassword((string)null, DefaultPassword);

            act.Should().Throw<ArgumentException>().WithParameterName("secretMessage");

            act = () => CoreEncryptionBouncyCastle.SimpleDecryptWithPassword((string)null, DefaultPassword);

            act.Should().Throw<ArgumentException>().WithParameterName("encryptedMessage");

#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        }

        [Fact]
        public void EncryptionBouncyCastle_SimpleEncrypt_PlainText_WithPassword_Empty()
        {
            var key = CoreEncryptionBouncyCastle.NewKey();

            Action act = () => CoreEncryptionBouncyCastle.SimpleEncryptWithPassword(string.Empty, DefaultPassword);

            act.Should().Throw<ArgumentException>().WithParameterName("secretMessage");
        }
    }
}
