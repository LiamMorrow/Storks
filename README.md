![Storks Logo](https://raw.githubusercontent.com/LiamMorrow/Storks/master/stork.png)

[![Build status](https://ci.appveyor.com/api/projects/status/9vsuhammekxpyl1u?svg=true)](https://ci.appveyor.com/project/LiamMorrow/storks)
[![NuGet](https://img.shields.io/nuget/v/Storks.svg)](https://www.nuget.org/packages/Storks/)
# Storks
A .Net library for loading of data and separating properties into storage.  
Useful for large messages in queues with limited message sizes
## Installation
Get via nuget at:
#### Core
Contains base interfaces and default implementations using file and in memory data stores  
Install via the Package Manager Console  
```Install-Package Storks```
#### Azure Storage Implementation
Contains an implementation using Azure Blob storage as a data store  
Install via the Package Manager Console  
```Install-Package Storks.AzureStorage```
## Usage
A common use case of storks is passing large messages or files in an environment which limits message size.  
Imagine you have a web service that allows you to convert HTML to PDF using a tool like wkhtmltopdf.  Unfortunately, this tool requires the GDI library which many service providers limit access to on server environments. So you need to create a worker that reads from a queue to run the HTML->PDF conversion then reports back.  

You write it up but hit a snag.  Your queue messages are limited to 64kB! That will work for the majority of your html, but all it takes is one file that is too large to queue.  This is where storks comes in.  

By using a StoreBackedProperty, you can now effortlessly pass objects as large as you like into your queue!  
Your queue message object now becomes this:  
```
class HtmlToPdfMessage{
  public StoreBackedProperty<string> Html {get; set; } 
}
```
And your push to queue method becomes this:
```
public async Task PushHtmlToPdfMessage(string html){
   
   // Create a new data communicator.  Here we use a LocalFileDataCommunicator which stores message data on the local HDD
   // The communicator simply gets and retrieves byte data when given a unique ID.
   // The Storks.AzureStorage package implements a data communicator with Azure Blob storage being the backing store
   IStoreBackedPropertyDataCommunicator dataStore = new LocalFileDataCommunicator("path/to/directorystorage");
   
   // Create a new StoreBackedPropertyController.  Alternatively, we could use a DI framework to get it
    var controller = new StoreBackedPropertyController(dataStore);
    var storedData = await controller.StoreValueAsync(html);
    var message = new HtmlToPdfMessage{
      Html = storedData
    };
    
    // Then you'd do your usual message queue operations here with message
}
```
Then to dequeue simply do the reverse:

```
public async Task DequeueHtmlToPdf(HtmlToPdfMessage message){
   
   // Create a new data communicator.  Here we use a LocalFileDataCommunicator which stores message data on the local HDD
   // The communicator simply gets and retrieves byte data when given a unique ID.
   // The Storks.AzureStorage package implements a data communicator with Azure Blob storage being the backing store
   IStoreBackedPropertyDataCommunicator dataStore = new LocalFileDataCommunicator("path/to/directorystorage");
   
   // Create a new StoreBackedPropertyController.  Alternatively, we could use a DI framework to get it
    var controller = new StoreBackedPropertyController(dataStore);
    var retrievedData = await controller.GetValueAsync(message.Html);
    
    // Then you'd do your usual message queue operations here with retrievedData
}
```
#### Automatic Data Retrieval
Storks integrates with Json.Net to allow for automatic data retrieval when a message is deserialized with JsonConvert.Deserialize.  
This is the method which Azure Webjobs use to deserialize queued messages into POCOs.  
Simply call the method `AutoBindLoadedJson` on any `IStoreBackedPropertyController` to use that controller to deserialize messages.
An example can be found in the Storks.Examples.AzureWebJobs project

## Contributing
1. Fork it!
2. Create your feature branch: `git checkout -b my-new-feature`
3. Commit your changes: `git commit -am 'Add some feature'`
4. Push to the branch: `git push origin my-new-feature`
5. Submit a pull request
## License
Licensed under the MIT license, can be found in LICENSE file



## Special thanks:
Icons made by <a href="http://www.flaticon.com/authors/roundicons" title="Roundicons">Roundicons</a> from <a href="http://www.flaticon.com" title="Flaticon">www.flaticon.com</a> is licensed by <a href="http://creativecommons.org/licenses/by/3.0/" title="Creative Commons BY 3.0" target="_blank">CC 3.0 BY</a></div>

Thanks to Appveyor for providing free builds for Open source projects!
