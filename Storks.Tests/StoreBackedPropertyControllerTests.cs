// Copyright (c) Liam Morrow.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Storks.Tests
{
    [TestClass]
    public class StoreBackedPropertyControllerTests
    {
        [TestMethod]
        public async Task TestFallbackToBson()
        {
            var propertyId = Guid.NewGuid().ToString();
            var communicator = new InMemoryDataCommunicator();
            var controller = new StoreBackedPropertyController(communicator)
            {
                FallbackToBsonEncoder = true
            };
            var testPocoInput = new TestPoco
            {
                Name = "JellyBean",
                IntVal = 5454
            };
            var propertyPointer = await controller.StoreValueAsync(testPocoInput).ConfigureAwait(false);

            var controllerRetrieveOutput = await controller.GetValueAsync(propertyPointer).ConfigureAwait(false);

            Assert.AreEqual(testPocoInput.Name, controllerRetrieveOutput.Name);
            Assert.AreEqual(testPocoInput.IntVal, controllerRetrieveOutput.IntVal);
        }

        [TestMethod]
        public async Task TestNoFallbackToBson()
        {
            var propertyId = Guid.NewGuid().ToString();
            var communicator = new InMemoryDataCommunicator();
            var controller = new StoreBackedPropertyController(communicator)
            {
                FallbackToBsonEncoder = false
            };
            var testPocoInput = new TestPoco
            {
                Name = "JellyBean",
                IntVal = 3
            };
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => controller.StoreValueAsync(testPocoInput))
                .ConfigureAwait(false);
        }

        private class TestPoco
        {
            public int IntVal { get; set; }
            public string Name { get; set; }
        }
    }
}