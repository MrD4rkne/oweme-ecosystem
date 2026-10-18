using System.CodeDom.Compiler;
using System.Reflection;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace OweMe.Api.Description;

/// <summary>
/// Describes StronglyTypedId guid structs as <c>string/uuid</c> instead of an empty object schema.
/// </summary>
public sealed class StronglyTypedIdSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        // [StronglyTypedId] itself is compiled away, but the generator stamps the struct with GeneratedCode
        if (context.JsonTypeInfo.Type.GetCustomAttribute<GeneratedCodeAttribute>()?.Tool == "StronglyTypedId")
        {
            schema.Type = JsonSchemaType.String;
            schema.Format = "uuid";
            schema.Properties?.Clear();
        }

        return Task.CompletedTask;
    }
}