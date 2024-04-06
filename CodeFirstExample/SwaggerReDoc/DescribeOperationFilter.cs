using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using TripleSix.Core.Helpers;
using TripleSix.Core.Types;
using TripleSix.Core.WebApi;

namespace CodeFirstExample.SwaggerReDoc
{
    public class DescribeOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            ActionDescriptor actionDescriptor = context.ApiDescription.ActionDescriptor;
            ControllerActionDescriptor controllerDescriptor = actionDescriptor as ControllerActionDescriptor;
            if (controllerDescriptor == null)
            {
                return;
            }

            Type baseType = controllerDescriptor.ControllerTypeInfo.BaseType;
            if ((object)baseType == null)
            {
                return;
            }

            MethodInfo methodInfo = controllerDescriptor.MethodInfo;
            if (methodInfo == null)
            {
                return;
            }

            operation.Parameters.Clear();
            operation.RequestBody = new OpenApiRequestBody();
            foreach (ApiParameterDescription parameterDescription in context.ApiDescription.ParameterDescriptions)
            {
                if (parameterDescription.Type == null)
                {
                    continue;
                }

                string text = parameterDescription.Source.DisplayName;
                if (text == "ModelBinding")
                {
                    text = "Query";
                }

                if (text == "Body")
                {
                    if (!operation.RequestBody.Content.ContainsKey("application/json"))
                    {
                        operation.RequestBody.Content.Add("application/json", new OpenApiMediaType());
                    }

                    operation.RequestBody.Content["application/json"].Schema = HelperSwagger.GenerateSwaggerSchema(defaultValue: Activator.CreateInstance(parameterDescription.Type), objectType: parameterDescription.Type, schemaGenerator: context.SchemaGenerator, schemaRepository: context.SchemaRepository);
                    continue;
                }

                if (text == "FormFile")
                {
                    if (!operation.RequestBody.Content.ContainsKey("multipart/form-data"))
                    {
                        operation.RequestBody.Content.Add("multipart/form-data", new OpenApiMediaType());
                    }

                    operation.RequestBody.Content["multipart/form-data"].Schema = parameterDescription.ParameterDescriptor.ParameterType.GenerateSwaggerSchema(context.SchemaGenerator, context.SchemaRepository, null, null, null, null, generateDefaultValue: false);
                    continue;
                }

                OpenApiParameter openApiParameter = new OpenApiParameter();
                switch (text)
                {
                    case "Path":
                        openApiParameter.In = ParameterLocation.Path;
                        break;
                    case "Query":
                        openApiParameter.In = ParameterLocation.Query;
                        break;
                    case "Header":
                        openApiParameter.In = ParameterLocation.Header;
                        break;
                }

                Type containerType = parameterDescription.ModelMetadata.ContainerType;
                if (!(containerType == null))
                {
                    object obj = Activator.CreateInstance(containerType);
                    PropertyInfo propertyInfo = parameterDescription.PropertyInfo();
                    PropertyInfo parentPropertyInfo = null;
                    if (parameterDescription.Name.Contains('.'))
                    {
                        parentPropertyInfo = parameterDescription.ParameterDescriptor.ParameterType.GetProperty(parameterDescription.Name.Split(".")[0]);
                    }

                    openApiParameter.Schema = parameterDescription.Type.GenerateSwaggerSchema(context.SchemaGenerator, context.SchemaRepository, propertyInfo, parentPropertyInfo, propertyInfo.GetValue(obj));
                    openApiParameter.Name = (from x in parameterDescription.Name.Split(".")
                                             select x.ToCamelCase()).ToString(".");
                    openApiParameter.Required = openApiParameter.In == ParameterLocation.Path || (propertyInfo?.GetCustomAttribute<RequiredAttribute>() != null && openApiParameter.Schema.Default is OpenApiNull);
                    operation.Parameters.Add(openApiParameter);
                }
            }

            if (operation.RequestBody.Content.Count == 0)
            {
                operation.RequestBody = null;
            }

            Type returnType = methodInfo.ReturnType;
            returnType = ((returnType != null && returnType.IsSubclassOfOpenGeneric(typeof(Task<>))) ? returnType.GetGenericArguments()[0] : typeof(SuccessResult));
            OpenApiMediaType openApiMediaType = new OpenApiMediaType();
            openApiMediaType.Schema = returnType.GenerateSwaggerSchema(context.SchemaGenerator, context.SchemaRepository, null, null, null, null, generateDefaultValue: false);
            OpenApiResponse openApiResponse = new OpenApiResponse
            {
                Description = "Success"
            };
            openApiResponse.Content.Add("application/json", openApiMediaType);
            operation.Responses["200"] = openApiResponse;
            if ((methodInfo.GetCustomAttribute<AuthorizeAttribute>(inherit: true) ?? controllerDescriptor.ControllerTypeInfo.GetCustomAttribute<AuthorizeAttribute>(inherit: true)) != null)
            {
                HashSet<string> hashSet = new HashSet<string>();
                List<RequireScope> list = methodInfo.GetCustomAttributes<RequireScope>(inherit: true).ToList();
                list.AddRange(controllerDescriptor.ControllerTypeInfo.GetCustomAttributes<RequireScope>(inherit: true));
                foreach (RequireScope item in list)
                {
                    object[]? arguments = item.Arguments;
                    string text2 = ((arguments != null) ? arguments[0].ToString() : null);
                    if (!text2.IsNullOrEmpty())
                    {
                        hashSet.Add(text2);
                    }
                }

                foreach (object item2 in methodInfo.GetCustomAttributes(typeof(RequireScope<>), inherit: true).ToList())
                {
                    TypeFilterAttribute obj2 = item2 as TypeFilterAttribute;
                    object obj3;
                    if (obj2 == null)
                    {
                        obj3 = null;
                    }
                    else
                    {
                        object[]? arguments2 = obj2.Arguments;
                        obj3 = ((arguments2 != null) ? arguments2[0] : null);
                    }

                    string text3 = obj3 as string;
                    if (!text3.IsNullOrEmpty())
                    {
                        IScopeTransformer scopeTransformer = Activator.CreateInstance(item2.GetType().GetGenericArguments()[0]) as IScopeTransformer;
                        if (scopeTransformer != null)
                        {
                            hashSet.Add(scopeTransformer.Transform(text3, controllerDescriptor));
                        }
                    }
                }

                List<RequireAnyScope> list2 = methodInfo.GetCustomAttributes<RequireAnyScope>(inherit: true).ToList();
                list2.AddRange(controllerDescriptor.ControllerTypeInfo.GetCustomAttributes<RequireAnyScope>(inherit: true));
                foreach (RequireAnyScope item3 in list2)
                {
                    IEnumerable<string> enumerable = item3.Arguments?.SelectMany((object x) => (string[])x);
                    if (!enumerable.IsNullOrEmpty())
                    {
                        if (enumerable.Count() == 1)
                        {
                            hashSet.Add(enumerable.First());
                        }
                        else
                        {
                            hashSet.Add("[" + enumerable.ToString(", ") + "]");
                        }
                    }
                }

                foreach (object item4 in methodInfo.GetCustomAttributes(typeof(RequireAnyScope<>), inherit: true).ToList())
                {
                    IEnumerable<string> enumerable2 = (item4 as TypeFilterAttribute)?.Arguments?.SelectMany((object x) => (string[])x);
                    if (enumerable2.IsNullOrEmpty())
                    {
                        continue;
                    }

                    object obj4 = Activator.CreateInstance(item4.GetType().GetGenericArguments()[0]);
                    IScopeTransformer transformer = obj4 as IScopeTransformer;
                    if (transformer != null)
                    {
                        IEnumerable<string> enumerable3 = enumerable2.Select((string inputScope) => transformer.Transform(inputScope, controllerDescriptor));
                        if (enumerable3.Count() == 1)
                        {
                            hashSet.Add(enumerable3.First());
                        }
                        else
                        {
                            hashSet.Add("[" + enumerable3.ToString(", ") + "]");
                        }
                    }
                }

                if (controllerDescriptor.ControllerTypeInfo.IsAssignableToGenericType(typeof(IControllerEndpoint<,>)))
                {
                    Type[] genericArguments = controllerDescriptor.ControllerTypeInfo.GetGenericArguments(typeof(IControllerEndpoint<,>));
                    IControllerEndpointAttribute controllerEndpointAttribute = genericArguments[0].GetCustomAttribute(genericArguments[1]) as IControllerEndpointAttribute;
                    if (controllerEndpointAttribute != null && controllerEndpointAttribute.RequiredAnyScopes.IsNotNullOrEmpty())
                    {
                        if (controllerEndpointAttribute.RequiredAnyScopes.Length == 1)
                        {
                            hashSet.Add(controllerEndpointAttribute.RequiredAnyScopes.First());
                        }
                        else
                        {
                            hashSet.Add("[" + controllerEndpointAttribute.RequiredAnyScopes.ToString(", ") + "]");
                        }
                    }
                }

                operation.Security.Add(new OpenApiSecurityRequirement {
                {
                    new OpenApiSecurityScheme
                    {
                        Name = "Bearer",
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "AccessToken"
                        }
                    },
                    hashSet.ToList()
                } });
            }

            string text4 = baseType.GetCustomAttribute<SwaggerTagGroupAttribute>()?.Name;
            if (text4.IsNullOrEmpty())
            {
                text4 = baseType.Name;
                if (text4.EndsWith("Controller"))
                {
                    string text5 = text4;
                    text4 = text5.Substring(0, text5.Length - 10);
                }
            }

            if (operation.Tags.IsNullOrEmpty())
            {
                OpenApiTag openApiTag = new OpenApiTag();
                openApiTag.Name = text4 + openApiTag.Name;
                openApiTag.Extensions.Add("x-tagGroup", new OpenApiString(text4));
                operation.Tags = new List<OpenApiTag> { openApiTag };
            }
            else
            {
                foreach (OpenApiTag tag in operation.Tags)
                {
                    tag.Name = text4 + tag.Name;
                    tag.Extensions.Add("x-tagGroup", new OpenApiString(text4));
                }
            }

            operation.OperationId = text4 + controllerDescriptor.ControllerName + controllerDescriptor.ActionName;
            context.SchemaRepository.Schemas.Clear();
        }
    }
}
