// Copyright (c) Liam Morrow.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Storks.Encoders;

namespace Storks.Tests
{
    [TestClass]
    public class StoreBackedPropertyControllerTests
    {
        /// <summary>
        /// This test verifies that a custom encoder is used when
        /// one is set with <see cref="IStoreBackedPropertyController.RegisterEncoder{T}(IStoreBackedPropertyEncoder{T})"/>
        /// </summary>
        [TestMethod]
        public void TestCustomEncoderRegistering()
        {
            var controller = GetDefaultController();
            controller.RegisterEncoder(new TestPocoEncoder());
            Assert.IsInstanceOfType(controller.GetEncoder<TestPoco>(), typeof(TestPocoEncoder), "Expected encoder to be one that was set");
        }

        /// <summary>
        /// This test verifies that an exception will be thrown if the controller has no DataCommunicator set
        /// </summary>
        [TestMethod]
        public void TestNoDataCommunicatorThrowsException()
        {
            // Make a new controller with no DataCommunicator
            var controller = GetDefaultController(x => x.DataCommunicator = null);

            // Attempt to get data when there is no DataCommunicator set
            Assert.ThrowsExceptionAsync<InvalidOperationException>(() =>
                controller.GetValueAsync(new StoreBackedProperty<string>("123")));
        }

        /// <summary>
        /// This test verifies that a <see cref="StoreBackedPropertyController"/> cannot be used if it has already been disposed
        /// </summary>
        [TestMethod]
        public void TestNoDisposedUsage()
        {
            var controller = GetDefaultController();
            controller.Dispose();
            Assert.ThrowsException<ObjectDisposedException>(controller.GetEncoder<string>);
        }

        /// <summary>
        /// This test verifies that if <see cref="StoreBackedPropertyController.FallbackToBsonEncoder"/> is true,
        /// the controller will use the <see cref="BsonStoreBackedPropertyEncoder{T}"/> to encode the data when no encoder is registered for that type
        /// </summary>
        [TestMethod]
        public void TestFallbackToBson()
        {
            var controller = GetDefaultController(x => x.FallbackToBsonEncoder = true);
            Assert.IsInstanceOfType(controller.GetEncoder<TestPoco>(), typeof(BsonStoreBackedPropertyEncoder<TestPoco>), "BSON encoder not returned for unknown type");
        }

        /// <summary>
        /// This test verifies that if <see cref="StoreBackedPropertyController.FallbackToBsonEncoder"/> is false,
        /// An <see cref="InvalidOperationException"/> is thrown when no encoder is registered for that type
        /// </summary>
        [TestMethod]
        public void TestNoFallbackToBson()
        {
            var controller = GetDefaultController(x => x.FallbackToBsonEncoder = false);

            Assert.ThrowsException<InvalidOperationException>(() => controller.GetEncoder<TestPoco>());
        }

        private IStoreBackedPropertyController GetDefaultController(Action<StoreBackedPropertyController> customSettings = null)
        {
            var communicator = new InMemoryDataCommunicator();
            var controller = new StoreBackedPropertyController(communicator);
            customSettings?.Invoke(controller);
            return controller;
        }

        private class TestPoco
        {
            public int IntVal { get; set; }
            public string Name { get; set; }
        }

        /// <summary>
        /// A custom encoder based on the <see cref="JsonStoreBackedPropertyEncoder{T}"/>
        /// </summary>
        private class TestPocoEncoder : JsonStoreBackedPropertyEncoder<TestPoco>
        {
        }
    }
}