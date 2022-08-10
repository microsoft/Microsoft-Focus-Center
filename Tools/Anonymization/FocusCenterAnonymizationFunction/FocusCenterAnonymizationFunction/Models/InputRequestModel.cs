using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnonymizationFunctionApp
{
    /// <summary>
    ///Appsetting json convert to c# class
    /// </summary>
    public class InputRequestModel
    {
        [JsonProperty("entities")]
        public InputEntityData[] Entities { get; set; }
    }

    public class InputEntityData
    {
        [JsonProperty("entityName")]
        public string EntityName { get; set; }

        [JsonProperty("entityIdName")]
        public string EntityIdName { get; set; }

        [JsonProperty("filter")]
        public string Filter { get; set; }
        [JsonProperty("attributes")]
        public Attributes[] Attributes { get; set; }
    }

    public class Attributes
    {
        public string Name { get; set; }
        public bool Counter { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public string Limits { get; set; }
    }
}
