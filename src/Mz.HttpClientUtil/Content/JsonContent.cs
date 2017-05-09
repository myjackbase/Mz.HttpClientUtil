using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Mz.HttpClientUtil
{
    public class JsonContent : StringContent
    {
        public const string MediaType = "application/json";

        public JsonContent(string json)
            : base(json)
        {

        }

        public JsonContent(object obj, JsonSerializerSettings settings = null)
            : base(Serialize(obj, settings))
        {

        }

        private static string Serialize(object obj, JsonSerializerSettings settings)
        {
            return settings != null
                ? JsonConvert.SerializeObject(obj, settings)
                : JsonConvert.SerializeObject(obj);
        }
    }
}
