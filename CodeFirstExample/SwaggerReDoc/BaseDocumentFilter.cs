using Elastic.Transport.Extensions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using System.Text.RegularExpressions;
using TripleSix.Core.Helpers;
using TripleSix.Core.WebApi;

namespace CodeFirstExample.SwaggerReDoc
{
    public class BaseDocumentFilter : IDocumentFilter
    {
        private class TagGroupItem
        {
            public string Name { get; set; }

            public int OrderIndex { get; set; }

            public string? Description { get; set; }

            public List<TagItem> Tags { get; set; }
        }

        private class TagItem
        {
            public string Name { get; set; }

            public string? Description { get; set; }
        }

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            List<TagGroupItem> list = new List<TagGroupItem>();
            string key;
            OpenApiPathItem value;
            OperationType key2;
            OpenApiOperation value2;
            foreach (KeyValuePair<string, OpenApiPathItem> path in swaggerDoc.Paths)
            {
                path.Deconstruct(out key, out value);
                string apiPath = key;
                foreach (KeyValuePair<OperationType, OpenApiOperation> operation in value.Operations)
                {
                    operation.Deconstruct(out key2, out value2);
                    OperationType apiMethod = key2;
                    OpenApiOperation openApiOperation = value2;
                    ControllerActionDescriptor controllerActionDescriptor = context.ApiDescriptions.First(delegate (ApiDescription x)
                    {
                        if (x.HttpMethod == apiMethod.GetStringValue().ToUpper())
                        {
                            string? relativePath = x.RelativePath;
                            string text = apiPath;
                            return relativePath == text.Substring(1, text.Length - 1);
                        }

                        return false;
                    }).ActionDescriptor as ControllerActionDescriptor;
                    if (controllerActionDescriptor == null)
                    {
                        continue;
                    }

                    TypeInfo controllerTypeInfo = controllerActionDescriptor.ControllerTypeInfo;
                    Type baseType = controllerTypeInfo.BaseType;
                    if ((object)baseType == null)
                    {
                        continue;
                    }

                    SwaggerTagGroupAttribute customAttribute = baseType.GetCustomAttribute<SwaggerTagGroupAttribute>();
                    SwaggerTagAttribute customAttribute2 = controllerTypeInfo.GetCustomAttribute<SwaggerTagAttribute>();
                    if (customAttribute2 == null && controllerTypeInfo.IsAssignableToGenericType(typeof(IControllerEndpoint<,>)))
                    {
                        customAttribute2 = controllerTypeInfo.GetGenericArguments(typeof(IControllerEndpoint<,>))[0].GetCustomAttribute<SwaggerTagAttribute>();
                    }

                    foreach (OpenApiTag tag in openApiOperation.Tags)
                    {
                        string groupName = ((OpenApiString)tag.Extensions.First((KeyValuePair<string, IOpenApiExtension> x) => x.Key == "x-tagGroup").Value).Value;
                        TagGroupItem tagGroupItem = list.FirstOrDefault((TagGroupItem x) => x.Name == groupName);
                        if (tagGroupItem == null)
                        {
                            tagGroupItem = new TagGroupItem
                            {
                                Name = groupName,
                                OrderIndex = (customAttribute?.OrderIndex ?? 0),
                                Description = customAttribute?.Description,
                                Tags = new List<TagItem>()
                            };
                            list.Add(tagGroupItem);
                        }

                        if (!tagGroupItem.Tags.Any((TagItem x) => x.Name == tag.Name))
                        {
                            tagGroupItem.Tags.Add(new TagItem
                            {
                                Name = tag.Name,
                                Description = customAttribute2?.Description
                            });
                        }
                    }
                }
            }

            swaggerDoc.Tags.Clear();
            foreach (TagItem item in list.SelectMany((TagGroupItem x) => x.Tags))
            {
                swaggerDoc.Tags.Add(new OpenApiTag
                {
                    Name = item.Name,
                    Description = (item.Description?.ToTitleCase() ?? null),
                    Extensions = new Dictionary<string, IOpenApiExtension> {
                    {
                        "x-displayName",
                        new OpenApiString(item.Description ?? item.Name)
                    } }
                });
            }

            OpenApiArray openApiArray = new OpenApiArray();
            swaggerDoc.Extensions.Add("x-tagGroups", openApiArray);
            foreach (TagGroupItem item2 in list.OrderBy((TagGroupItem x) => x.OrderIndex))
            {
                OpenApiArray openApiArray2 = new OpenApiArray();
                openApiArray2.AddRange(item2.Tags.Select((TagItem x) => new OpenApiString(x.Name)));
                openApiArray.Add(new OpenApiObject
                {
                    ["name"] = new OpenApiString(item2.Name),
                    ["tags"] = openApiArray2
                });
            }

            foreach (KeyValuePair<string, OpenApiPathItem> path2 in swaggerDoc.Paths)
            {
                path2.Deconstruct(out key, out value);
                foreach (KeyValuePair<OperationType, OpenApiOperation> operation2 in value.Operations)
                {
                    operation2.Deconstruct(out key2, out value2);
                    OpenApiOperation openApiOperation2 = value2;
                    if (openApiOperation2.Summary != null)
                    {
                        string tagName = openApiOperation2.Tags[0].Name;
                        openApiOperation2.Summary = Regex.Replace(openApiOperation2.Summary, "\\[controller\\]", swaggerDoc.Tags.FirstOrDefault((OpenApiTag x) => x.Name == tagName)?.Description ?? tagName);
                    }
                }
            }
        }
    }
}
