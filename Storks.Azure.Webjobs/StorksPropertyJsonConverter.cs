using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Storks.Encoders;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Storks.Azure.Webjobs
{
    public class StorksPropertyJsonConverter : JsonConverter
    {
        private readonly IStoreBackedPropertyController _controller;

        public StorksPropertyJsonConverter(IStoreBackedPropertyController controller)
        {
            this._controller = controller;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Storks.StoreBackedProperty<>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jobject = JObject.Load(reader);
            JToken jtoken;
            if (jobject == null || !jobject.HasValues || !jobject.TryGetValue("Id", out jtoken))
                return (object)null;
            string id = jtoken.Value<string>();
            dynamic task = typeof(IStoreBackedPropertyController).GetMethod("GetValueAsync")
                .MakeGenericMethod(objectType.GenericTypeArguments[0])
                .Invoke(_controller, new[] { new StoreBackedProperty(objectType.GenericParameterAttributes()[0], id) });
            var result = task.Result;
            return Activator.CreateInstance(objectType, id, result);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
        }

        private class StoreBackedProperty : IStoreBackedProperty
        {
            public Type PropertyType { get; }

            public string Id { get; }

            public StoreBackedProperty(Type objectType, string id)
            {
                this.PropertyType = objectType;
                this.Id = id;
            }
        }

        public class DataRetriever : IStoreBackedPropertyDataCommunicator
        {
            public async Task<byte[]> GetDataAsync(string id)
            {
                await Task.Delay(300);
                return new StringStoreBackedPropertyEncoder().Encode("Hello world");
            }

            public Task StoreDataAsync(string id, byte[] data)
            {
                throw new NotImplementedException();
            }
        }
    }
}
