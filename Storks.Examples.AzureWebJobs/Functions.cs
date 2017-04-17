using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;

namespace Storks.Examples.AzureWebJobs
{
    static class Functions
    {
        /// <summary>
        /// This function will be triggered by a message appearing in messageQueue.
        /// The message will already have the data in it due to 
        /// the <see cref="Storks.StorksHelpers.AutoBindLoadedJson(IStoreBackedPropertyController)"/> call in <see cref="Program.Main"/>
        /// </summary>
        /// <param name="message">The queue message.  The azure queue framework will automatically deserialize Json messages into your POCO, and storks will fill it with data</param>
        public static void ProcessQueueMessage([QueueTrigger("messageQueue")] ExampleMessage message)
        {
            var stringMessage = message.LargeStringMessage;
            var byteMessage = message.LargeByteMessage;
            var pocoMessage = message.PocoMessage;
            // All of those variables will be filled with the data they were given at queue time.  
            // Notice there is no need to set up a controller to populate the data as it is intercepted by Storks
        }

    }
}
