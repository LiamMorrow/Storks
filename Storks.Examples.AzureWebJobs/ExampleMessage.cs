namespace Storks.Examples.AzureWebJobs
{
    public class ExampleMessage
    {
        public StoreBackedProperty<string> LargeStringMessage { get; set; }

        public StoreBackedProperty<byte[]> LargeByteMessage { get; set; }
        public StoreBackedProperty<Poco> PocoMessage { get; set; }
    }
    public class Poco
    {
        public string Data { get; set; }

        public int OtherData { get; set; }
    }
}