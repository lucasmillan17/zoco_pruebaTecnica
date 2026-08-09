using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json.Nodes;

namespace CMS.Api;

public sealed class StringEnumSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        var type = Nullable.GetUnderlyingType(context.Type) ?? context.Type;
        if (!type.IsEnum || schema is not OpenApiSchema s)
        {
            return;
        }

        var nombres = Enum.GetNames(type);
        s.Type = JsonSchemaType.String;
        s.Format = null;
        s.Enum = nombres.Select(n => (JsonNode)JsonValue.Create(n)).ToList();
        s.Example = JsonValue.Create(nombres.First());
    }
}
