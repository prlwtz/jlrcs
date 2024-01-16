using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Threading.Tasks;

namespace jlr_sample
{
    public static class JsonEx
    {
        public static void AddRange(this JsonArray jsonArray, IEnumerable<JsonNode> values)
        {
            foreach (JsonNode value in values)
            {
                // we have to create copies to prevent the "Node already has a parent" Exception
                jsonArray.Add(value!.Copy());
            }
        }

        public static void AddRange(this JsonObject jsonObject, IEnumerable<KeyValuePair<string, JsonNode?>> properties)
        {
            // existing values will be replaced by the new contents
            foreach (KeyValuePair<string, JsonNode?> kvp in properties)
            {
                // we have to create copies to prevent the "Node already has a parent" Exception
                jsonObject[kvp.Key] = kvp.Value!.Copy();
            }
        }

        public static JsonObject? Copy(this JsonObject jsonObject)
        {
            // We know this can be relatively expensive with complex content, but let's keep the code simple for now
            return JsonSerializer.Deserialize<JsonObject>(JsonSerializer.Serialize(jsonObject));
        }

        public static JsonNode? Copy(this JsonNode jsonNode)
        {
            // We know this can be relatively expensive with complex content, but let's keep the code simple for now
            return JsonSerializer.Deserialize<JsonNode>(JsonSerializer.Serialize(jsonNode));
        }

        public static JsonObject? Create(this JsonNode jsonNode, object? anonymous)
        {
            // by the lack of constructor extenstions, we just extend Create on an object
            return JsonSerializer.Deserialize<JsonNode>(JsonSerializer.Serialize(anonymous!)!)! as JsonObject;
        }
    }

}
