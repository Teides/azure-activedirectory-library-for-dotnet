﻿//----------------------------------------------------------------------
//
// Copyright (c) Microsoft Corporation.
// All rights reserved.
//
// This code is licensed under the MIT License.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files(the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions :
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//------------------------------------------------------------------------------

using System.IO;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.ADAL.Common.Unit;
using Test.ADAL.NET.Common;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal;
using Microsoft.Identity.Test.Common.Core.Mocks;
using Microsoft.Identity.Core;
using Microsoft.Identity.Core.Cache;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.Cache;
using Test.ADAL.NET.Common.Mocks;
using System;

namespace Test.ADAL.NET.Unit
{
    [TestClass]
    [DeploymentItem("Resources\\oldcache.serialized")]
    public class TokenCacheUnitTests
    {
        [TestInitialize]
        public void Initialize()
        {
            ModuleInitializer.ForceModuleInitializationTestOnly();
            InstanceDiscovery.InstanceCache.Clear();
        }

        [TestMethod]
        [Description("Test to store in default token cache")]
        [TestCategory("AdalDotNetUnit")]
        public void DefaultTokenCacheTest()
        {
            TokenCacheTests.DefaultTokenCacheTest();
        }

        [TestMethod]
        [TestCategory("Regression")] // regression for https://github.com/AzureAD/azure-activedirectory-library-for-dotnet/issues/1463
        public void MrrtTest()
        {
            // Arrange
            const string ClientId = "ClientId";
            string Authority = AdalTestConstants.DefaultAuthorityCommonTenant;
            const string Resource1 = "R1";
            const string Resource2 = "R2";
            const string UniqueId = "uniqueId";
            const string DisplayableId = "displayId";

            var requestContext = new RequestContext(ClientId, new TestLogger());
            var tokenCache = new TokenCache();
            var cacheDictionary = tokenCache._tokenCacheDictionary;

            AdalResultWrapper entryForResource1 = TokenCacheTests.CreateCacheValue(UniqueId, DisplayableId, false);
            CacheQueryData cacheQueryForResource2 = new CacheQueryData()
            {
                Authority = Authority,
                Resource = Resource2,
                ClientId = ClientId,
                SubjectType = TokenSubjectType.User,
                UniqueId = UniqueId,
                DisplayableId = DisplayableId
            };

            CacheQueryData cacheQueryForResource1 = new CacheQueryData()
            {
                Authority = Authority,
                Resource = Resource1,
                ClientId = ClientId,
                SubjectType = TokenSubjectType.User,
                UniqueId = UniqueId,
                DisplayableId = DisplayableId
            };

            // Act 

            // 1. Store the AT and IdT (but not the RT as it's on the broker) 
            tokenCache.StoreToCacheCommon(entryForResource1, Authority, Resource1, ClientId, TokenSubjectType.User, requestContext);

            // 2. Rquest an AT for Resource2 from the cache -> should fail, because we don't have the MRRT (the broker has it)
            AdalResultWrapper resultForResource2Query = tokenCache.LoadFromCacheCommon(cacheQueryForResource2, requestContext);

            // 3. Request an AT for Resource1 from the cache -> should succed (it used to fail because step 2 would delete the token)
            AdalResultWrapper resultForResource1Query = tokenCache.LoadFromCacheCommon(cacheQueryForResource1, requestContext);

            // Assert
            Assert.IsNull(resultForResource2Query, "No result should be returned from the cache for Resource2");
            Assert.IsNotNull(resultForResource1Query, "An AT is present in the cache for Resource1");
        }

#if !NET_CORE // netcore doesn't support interactive
        [TestMethod]
        [TestCategory("AdalDotNetUnit")]
        public async Task TestUniqueIdDisplayableIdLookupAsync()
        {
            await TokenCacheTests.TestUniqueIdDisplayableIdLookupAsync().ConfigureAwait(false);
        }

        [TestMethod]
        [Description("Test for TokenCache")]
        [TestCategory("AdalDotNetUnit")]
        public async Task TokenCacheKeyTestAsync()
        {
            await TokenCacheTests.TokenCacheKeyTestAsync(new PlatformParameters(PromptBehavior.Auto))
                .ConfigureAwait(false);
        }
#endif

        [TestMethod]
        [Description("Test for Token Cache Operations")]
        [TestCategory("AdalDotNetUnit")]
        public async Task TokenCacheOperationsTestAsync()
        {
            await TokenCacheTests.TokenCacheOperationsTestAsync().ConfigureAwait(false);
        }

        [TestMethod]
        [Description("Test for Token Cache Cross-Tenant operations")]
        [TestCategory("AdalDotNetUnit")]
        public void TokenCacheCrossTenantOperationsTest()
        {
            TokenCacheTests.TokenCacheCrossTenantOperationsTest();
        }

        [TestMethod]
        [Description("Test for Token Cache Capacity")]
        [TestCategory("AdalDotNetUnit")]
        public void TokenCacheCapacityTest()
        {
            TokenCacheTests.TokenCacheCapacityTest();
        }

        [TestMethod]
        [Description("Test for Multiple User tokens found, hash fallback test")]
        [TestCategory("AdalDotNetUnit")]
        public async Task MultipleUserAssertionHashTestAsync()
        {
            await TokenCacheTests.MultipleUserAssertionHashTestAsync().ConfigureAwait(false);
        }

        [TestMethod]
        [Description("Test for Token Cache Serialization")]
        [TestCategory("AdalDotNetUnit")]
        public void TokenCacheSerializationTest()
        {
            TokenCacheTests.TokenCacheSerializationTest();
        }

        [TestMethod]
        [Description("Test for Token Cache backwasrd compatiblity where new attribute is added in AdalResultWrapper")]
        [TestCategory("AdalDotNetUnit")]
        public void TokenCacheBackCompatTest()
        {
            TokenCacheTests.TokenCacheBackCompatTest(File.ReadAllBytes("oldcache.serialized"));
        }

        [TestMethod]
        [Description("Positive Test for Parallel stores on cache")]
        [TestCategory("AdalDotNet.Unit")]
        public void ParallelStoreTest()
        {
            TokenCacheTests.ParallelStorePositiveTest(File.ReadAllBytes("oldcache.serialized"));
        }

        [TestMethod]
        [Description("Test to ensure the token cache doesn't throw an exception when cleared")]
        [TestCategory("AdalDotNet.Unit")]
        public void TokenCacheClearTest()
        {
            TokenCacheTests.TokenCacheClearTest(File.ReadAllBytes("oldcache.serialized"));
        }

        [TestMethod]
        [Description("Test to ensure a null IdToken is handled correctly")]
        [TestCategory("AdalDotNet.Unit")]
        public void NullIdTokenCacheTest()
        {
            // arrange & act
            TokenCache tokenCache = new TokenCache();

            var result = CreateAdalResultWithIdToken(false);

            WriteMsalRefreshTokenValues(result, tokenCache.TokenCacheAccessor);

            // assert
            // no IdToken present, CacheFallbackOperations exits early
            Assert.AreEqual(tokenCache.TokenCacheAccessor.AccountCount, 0);
            Assert.AreEqual(tokenCache.TokenCacheAccessor.RefreshTokenCount, 0);
            Assert.IsNull(result.Result.UserInfo);
        }

        [TestMethod]
        [Description("Test to ensure WriteMsalRefreshToken handles presence of IdToken correctly")]
        [TestCategory("AdalDotNet.Unit")]
        public void IdTokenReturnedCacheTest()
        {
            // arrange
            TokenCache tokenCache = new TokenCache();

            var result = CreateAdalResultWithIdToken(true);

            // act
            WriteMsalRefreshTokenValues(result, tokenCache.TokenCacheAccessor);

            // assert
            // IdToken present
            Assert.AreEqual(tokenCache.TokenCacheAccessor.AccountCount, 1);
            Assert.AreEqual(tokenCache.TokenCacheAccessor.RefreshTokenCount, 1);
            Assert.AreEqual(AdalTestConstants.DefaultDisplayableId, result.Result.UserInfo.DisplayableId);
            Assert.AreEqual(AdalTestConstants.DefaultUniqueId, result.Result.UserInfo.UniqueId);
        }

        private AdalResultWrapper CreateAdalResultWithIdToken(bool withIdToken)
        {
            var result = new AdalResultWrapper
            {
                RefreshToken = "some-rt",
                RawClientInfo = MockHelpers.CreateClientInfo(AdalTestConstants.DefaultUniqueId, AdalTestConstants.DefaultUniqueTenantIdentifier),
                ResourceInResponse = AdalTestConstants.DefaultResource
            };

            if (withIdToken)
            {
                result.Result = new AdalResult("Bearer", "some-access-token", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow + TimeSpan.FromMinutes(180))
                {
                    UserInfo = new AdalUserInfo()
                    {
                        DisplayableId = AdalTestConstants.DefaultDisplayableId,
                        UniqueId = AdalTestConstants.DefaultUniqueId
                    },
                    IdToken = MockHelpers.CreateAdalIdToken(
                                AdalTestConstants.DefaultUniqueId,
                                AdalTestConstants.DefaultDisplayableId)
                };
            }
            else
            {
                result.Result = new AdalResult("Bearer", "some-access-token", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow + TimeSpan.FromMinutes(180));
            }

            return result;
        }

        private void WriteMsalRefreshTokenValues(AdalResultWrapper result, ITokenCacheAccessor tokenCacheAccessor)
        {
            CacheFallbackOperations.WriteMsalRefreshToken(
                tokenCacheAccessor,
                result,
                AdalTestConstants.DefaultAuthorityCommonTenant,
                AdalTestConstants.DefaultClientId,
                result.Result.UserInfo?.DisplayableId,
                result.Result.UserInfo?.GivenName,
                result.Result.UserInfo?.FamilyName,
                result.Result.UserInfo?.UniqueId);
        }
    }
}