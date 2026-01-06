using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Carmasters.Core.Application.Dapper
{
    
    public class JsonNodeTypeHandler : SqlMapper.TypeHandler<JsonNode>
    {
        public override void SetValue(IDbDataParameter parameter, JsonNode value)
        {
            parameter.Value = JsonSerializer.Serialize(value);
        }

        public override JsonNode Parse(object value)
        {
            if (value is string json)
            {
                return JsonSerializer.Deserialize<JsonNode>(json);
            }

            return default;
        }
    }
}
