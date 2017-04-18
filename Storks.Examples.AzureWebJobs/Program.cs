using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Storks;
using Storks.AzureStorage;

namespace Storks.Examples.AzureWebJobs
{
    internal static class Program
    {
        /// <summary>
        /// This performs the startup necessary for running an Azure WebJob.  It also initializes the Storks controller for data operations
        /// </summary>
        private static void Main()
        {
            var config = new JobHostConfiguration();
            config.Queues.MaxPollingInterval = TimeSpan.FromSeconds(12);

            // These items should be added to App.Config, alternatively replace them with strings
            var htmlStorageConnectionString = ConfigurationManager.ConnectionStrings["PdfHtmlStorage"].ConnectionString;
            var htmlStorageContainer = ConfigurationManager.AppSettings["PdfHtmlStorageContainer"];

            // create a new communicator that utilizes Azure Storage as it's backing container
            var dataCommunicator = new AzureBlobStorageCommunicator(htmlStorageConnectionString, htmlStorageContainer);

            // create a storeController to handle data retrieval operations
            var storeController = new StoreBackedPropertyController(dataCommunicator);

            // This allows the queue handler to not have to worry about data retrieval operations
            storeController.AutoBindLoadedJson();

            if (config.IsDevelopment)
            {
                config.UseDevelopmentSettings();
            }

            var host = new JobHost(config);
            // The following code ensures that the WebJob will be running continuously
            host.RunAndBlock();
        }
    }
}