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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Identity.Core.WsTrust;
using System.IO;
using System.Xml.Linq;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Test.ADAL.NET.Unit
{
    [TestClass]
    [DeploymentItem("Resources\\WsTrustResponse.xml")]
    public class WsTrustTests
    {
        [TestInitialize]
        public void Initialize()
        {
            ModuleInitializer.ForceModuleInitializationTestOnly();
            InstanceDiscovery.InstanceCache.Clear();
        }

        [TestMethod]
        [TestCategory("WsTrustTests")]
        public void TestCreateFromResponseDocument_WhenInputContainsWhitespace_ShouldPreserveWhitespace()
        {
            string sample = File.ReadAllText(
                Microsoft.Identity.Core.Unit.ResourceHelper.GetTestResourceRelativePath("WsTrustResponse.xml"));
            string characteristic = "\n        <saml:Assertion";
            StringAssert.Contains(sample, characteristic);
            WsTrustResponse resp = WsTrustResponse.CreateFromResponseDocument(
                XDocument.Parse(sample, LoadOptions.PreserveWhitespace), WsTrustVersion.WsTrust2005);
            StringAssert.Contains(resp.Token, characteristic);
        }

        [TestMethod]
        [TestCategory("WsTrustTests")]
        public void TestCreateFromResponse_WhenInputContainsWhitespace_ShouldPreserveWhitespace()
        {
            string sample = File.ReadAllText(
                Microsoft.Identity.Core.Unit.ResourceHelper.GetTestResourceRelativePath("WsTrustResponse.xml"));
            string characteristic = "\n        <saml:Assertion";
            StringAssert.Contains(sample, characteristic);
            WsTrustResponse resp = WsTrustResponse.CreateFromResponse(sample, WsTrustVersion.WsTrust2005);
            StringAssert.Contains(resp.Token, characteristic);
        }
    }
}
