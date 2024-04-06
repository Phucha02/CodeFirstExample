using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Runtime.CompilerServices;
using Autofac;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Swashbuckle.AspNetCore.ReDoc;
using Swashbuckle.AspNetCore.SwaggerGen;
using TripleSix.Core.Appsettings;
using TripleSix.Core.Helpers;
using TripleSix.Core.Mappers;
using TripleSix.Core.Types;
using TripleSix.Core.Validation;
using TripleSix.Core.WebApi;

namespace CodeFirstExample.SwaggerReDoc
{
    public static class HelperSwagger
    {
        public static IApplicationBuilder UseReDocUIV2(this IApplicationBuilder app, SwaggerAppsetting setting)
        {
            SwaggerAppsetting setting2 = setting;
            if (!setting2.Enable)
            {
                return app;
            }

            app.UseSwagger();
            Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().First((Assembly x) => x.GetName().Name == "TripleSix.Core");
            string name = assembly.GetManifestResourceNames().First((string x) => x.EndsWith("ReDoc.html"));
            app.UseReDoc(delegate (ReDocOptions options)
            {
                options.RoutePrefix = setting2.Route;
                options.IndexStream = delegate
                {
                    Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().First((Assembly x) => x.GetName().Name == "TripleSix.Core");
                    string name = assembly.GetManifestResourceNames().First((string x) => x.EndsWith("ReDoc.html"));
                    return assembly.GetManifestResourceStream(name);
                };
            });
            return app;
        }

        public static IServiceCollection AddSwaggerV2(this IServiceCollection services, SwaggerAppsetting setting, Action<SwaggerGenOptions, SwaggerAppsetting>? setupAction = null)
        {
            SwaggerAppsetting setting2 = setting;
            Action<SwaggerGenOptions, SwaggerAppsetting> setupAction2 = setupAction;
            if (!setting2.Enable)
            {
                return services;
            }

            return services.AddSwaggerGen(delegate (SwaggerGenOptions options)
            {
                options.SwaggerDoc("openapi", new OpenApiInfo
                {
                    Title = setting2.Title,
                    Version = setting2.Version
                });
                options.AddSecurityDefinition("AccessToken", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.ApiKey,
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Description = "Nhập `Access Token` vào header để truy cập"
                });
                options.SwaggerGeneratorOptions.DescribeAllParametersInCamelCase = true;
                options.CustomSchemaIds((Type x) => x.FullName);
                options.EnableAnnotations();
                options.MapType<DateTime>(() => new OpenApiSchema
                {
                    Type = "integer",
                    Format = "int64"
                });
                options.MapType<DateTime?>(() => new OpenApiSchema
                {
                    Type = "integer",
                    Format = "int64",
                    Nullable = true
                });
                options.DocumentFilter<BaseDocumentFilter>(Array.Empty<object>());
                options.OperationFilter<DescribeOperationFilter>(Array.Empty<object>());
                setupAction2?.Invoke(options, setting2);
            });
        }

        internal static OpenApiSchema GenerateSwaggerSchema(this Type objectType, ISchemaGenerator schemaGenerator, SchemaRepository schemaRepository, PropertyInfo? propertyInfo = null, PropertyInfo? parentPropertyInfo = null, object? defaultValue = null, OpenApiSchema? baseSchema = null, bool generateDefaultValue = true)
        {
            OpenApiSchema openApiSchema = schemaGenerator.GenerateSchema(objectType, schemaRepository);
            Type propertyType = objectType.GetUnderlyingType();
            if (propertyType.IsAssignableTo<JToken>())
            {
                openApiSchema.Type = "object";
                openApiSchema.AdditionalProperties = null;
            }
            else if (!propertyType.IsAssignableTo<IFormFile>())
            {
                if (propertyType.IsEnum)
                {
                    openApiSchema.Type = "integer";
                    openApiSchema.Format = "int32";
                }
                else if (openApiSchema.Type == "array")
                {
                    Type type = (objectType.IsArray ? objectType.GetElementType() : objectType.GetGenericArguments()[0]);
                    openApiSchema.Items.Reference = null;
                    openApiSchema.Items = ((type == null) ? null : type.GenerateSwaggerSchema(schemaGenerator, schemaRepository, null, null, defaultValue, openApiSchema, generateDefaultValue));
                }
                else if (openApiSchema.Type == null)
                {
                    openApiSchema.Type = "object";
                    foreach (PropertyInfo item in from x in objectType.GetProperties()
                                                  orderby x.DeclaringType?.BaseTypesAndSelf().Count()
                                                  orderby x.GetCustomAttribute<JsonPropertyAttribute>(inherit: true)?.Order ?? 0
                                                  select x)
                    {
                        if (item.GetCustomAttribute<JsonIgnoreAttribute>(inherit: true) == null && item.GetCustomAttribute<SwaggerHideAttribute>(inherit: true) == null)
                        {
                            openApiSchema.Properties.Add(item.Name.ToCamelCase(), item.PropertyType.GenerateSwaggerSchema(schemaGenerator, schemaRepository, item, propertyInfo, (defaultValue == null) ? null : item.GetValue(defaultValue), openApiSchema, generateDefaultValue));
                        }
                    }
                }
            }

            openApiSchema.Reference = null;
            if (openApiSchema.Type != "object" && generateDefaultValue)
            {
                openApiSchema.Default = objectType.SwaggerValue(defaultValue);
            }

            openApiSchema.Nullable = objectType.IsNullableType();
            if (propertyType.IsEnum)
            {
                IEnumerable<string> enumerable = Enum.GetValues(propertyType).Cast<int>().Select(delegate (int value)
                {
                    string name = Enum.GetName(propertyType, value);
                    string description = EnumHelper.GetDescription(propertyType, value);
                    DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 3);
                    defaultInterpolatedStringHandler.AppendLiteral("<span>");
                    defaultInterpolatedStringHandler.AppendFormatted(value);
                    defaultInterpolatedStringHandler.AppendLiteral(" = ");
                    defaultInterpolatedStringHandler.AppendFormatted(name);
                    defaultInterpolatedStringHandler.AppendLiteral(" ");
                    defaultInterpolatedStringHandler.AppendFormatted(name.Equals(description, StringComparison.CurrentCultureIgnoreCase) ? string.Empty : ("(" + description + ")"));
                    defaultInterpolatedStringHandler.AppendLiteral("</span>");
                    return defaultInterpolatedStringHandler.ToStringAndClear();
                });
                if (enumerable.Any())
                {
                    openApiSchema.Description = openApiSchema.Description + "<br/><br/>" + string.Join("<br/>", enumerable);
                }
            }

            if (propertyInfo == null)
            {
                return openApiSchema;
            }

            if (propertyInfo.GetCustomAttribute<RequiredAttribute>() != null && baseSchema != null)
            {
                baseSchema!.Required.Add(propertyInfo!.Name.ToCamelCase());
                openApiSchema.Default = null;
            }

            string text = propertyInfo.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName.ToTitleCase();
            if (text == null)
            {
                if (propertyInfo!.DeclaringType?.IsAssignableToGenericType(typeof(IEntityQueryableDto<>)) ?? false)
                {
                    if (text == null)
                    {
                        Type? type2 = propertyInfo!.DeclaringType!.GetInterfaces().FirstOrDefault((Type x) => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEntityQueryableDto<>));
                        text = (((object)type2 == null) ? null : type2!.GenericTypeArguments[0].GetProperty(propertyInfo!.Name)?.GetCustomAttribute<CommentAttribute>()?.Comment);
                    }

                    if (text.IsNotNullOrEmpty())
                    {
                        text = "Lọc theo " + text;
                    }
                }
                else if (propertyInfo!.DeclaringType?.IsAssignableToGenericType(typeof(IElasticQueryableDto<>)) ?? false)
                {
                    Type? type3 = propertyInfo!.DeclaringType!.GetInterfaces().FirstOrDefault((Type x) => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IElasticQueryableDto<>));
                    Type obj = (((object)type3 != null) ? type3!.GenericTypeArguments[0] : null);
                    object obj2;
                    if ((object)obj == null)
                    {
                        obj2 = null;
                    }
                    else
                    {
                        Attribute? customAttribute = obj.GetCustomAttribute(typeof(MapFromEntityAttribute<>));
                        obj2 = ((customAttribute != null) ? customAttribute!.GetType().GetGenericArguments()[0] : null);
                    }

                    Type type4 = (Type)obj2;
                    if (text == null)
                    {
                        text = type4?.GetProperty(propertyInfo!.Name)?.GetCustomAttribute<CommentAttribute>()?.Comment;
                    }

                    if (text.IsNotNullOrEmpty())
                    {
                        text = "Lọc theo " + text;
                    }
                }
                else
                {
                    Type? declaringType = propertyInfo!.DeclaringType;
                    object obj3;
                    if ((object)declaringType == null)
                    {
                        obj3 = null;
                    }
                    else
                    {
                        Attribute? customAttribute2 = declaringType.GetCustomAttribute(typeof(MapFromEntityAttribute<>));
                        obj3 = ((customAttribute2 != null) ? customAttribute2!.GetType().GetGenericArguments()[0] : null);
                    }

                    if (obj3 == null)
                    {
                        Type? declaringType2 = propertyInfo!.DeclaringType;
                        if ((object)declaringType2 == null)
                        {
                            obj3 = null;
                        }
                        else
                        {
                            Attribute? customAttribute3 = declaringType2.GetCustomAttribute(typeof(MapToEntityAttribute<>));
                            obj3 = ((customAttribute3 != null) ? customAttribute3!.GetType().GetGenericArguments()[0] : null);
                        }
                    }

                    Type type5 = (Type)obj3;
                    if (type5 == null)
                    {
                        Type? declaringType3 = propertyInfo!.DeclaringType;
                        object obj4;
                        if ((object)declaringType3 == null)
                        {
                            obj4 = null;
                        }
                        else
                        {
                            Attribute? customAttribute4 = declaringType3.GetCustomAttribute(typeof(MapFromElasticDocumentAttribute<>));
                            obj4 = ((customAttribute4 != null) ? customAttribute4!.GetType().GetGenericArguments()[0] : null);
                        }

                        object obj5;
                        if (obj4 == null)
                        {
                            obj5 = null;
                        }
                        else
                        {
                            Attribute? customAttribute5 = ((MemberInfo)obj4).GetCustomAttribute(typeof(MapFromEntityAttribute<>));
                            obj5 = ((customAttribute5 != null) ? customAttribute5!.GetType().GetGenericArguments()[0] : null);
                        }

                        type5 = (Type)obj5;
                    }

                    if (text == null)
                    {
                        text = type5?.GetProperty(propertyInfo!.Name)?.GetCustomAttribute<CommentAttribute>()?.Comment;
                    }

                    if (text.IsNullOrEmpty())
                    {
                        Attribute? customAttribute6 = propertyInfo!.PropertyType.GetCustomAttribute(typeof(MapFromEntityAttribute<>));
                        object obj6 = ((customAttribute6 != null) ? customAttribute6!.GetType().GetGenericArguments()[0] : null);
                        if (obj6 == null)
                        {
                            Attribute? customAttribute7 = propertyInfo!.PropertyType.GetCustomAttribute(typeof(MapToEntityAttribute<>));
                            obj6 = ((customAttribute7 != null) ? customAttribute7!.GetType().GetGenericArguments()[0] : null);
                        }

                        Type type6 = (Type)obj6;
                        if (text == null)
                        {
                            text = type6?.GetCustomAttribute<CommentAttribute>()?.Comment;
                        }
                    }
                }
            }

            string text2 = propertyInfo.GetCustomAttribute<DescriptionAttribute>()?.Description.ToTitleCase();
            openApiSchema.Description = new string[2] { text, text2 }.Where((string x) => x.IsNotNullOrEmpty()).ToString("<br/>") + openApiSchema.Description;
            openApiSchema.MinLength = propertyInfo.GetCustomAttribute<MinLengthAttribute>()?.Length;
            openApiSchema.MaxLength = propertyInfo.GetCustomAttribute<MaxLengthAttribute>()?.Length;
            openApiSchema.Minimum = propertyInfo.GetCustomAttribute<MinValueAttribute>()?.Value;
            openApiSchema.Maximum = propertyInfo.GetCustomAttribute<MaxValueAttribute>()?.Value;
            List<Attribute> list = new List<Attribute>();
            RequiredAttribute customAttribute8 = propertyInfo.GetCustomAttribute<RequiredAttribute>();
            NotEmptyAttribute customAttribute9 = propertyInfo.GetCustomAttribute<NotEmptyAttribute>();
            if ((customAttribute8 != null && !customAttribute8.AllowEmptyStrings) || customAttribute9 != null)
            {
                list.Add(new NotEmptyAttribute());
            }

            NotNullAttribute customAttribute10 = propertyInfo.GetCustomAttribute<NotNullAttribute>();
            if (customAttribute10 != null)
            {
                list.Add(customAttribute10);
            }

            MustNoSpaceAttribute customAttribute11 = propertyInfo.GetCustomAttribute<MustNoSpaceAttribute>();
            if (customAttribute11 != null)
            {
                list.Add(customAttribute11);
            }

            MustLowerCaseAttribute customAttribute12 = propertyInfo.GetCustomAttribute<MustLowerCaseAttribute>();
            if (customAttribute12 != null)
            {
                list.Add(customAttribute12);
            }

            MustUpperCaseAttribute customAttribute13 = propertyInfo.GetCustomAttribute<MustUpperCaseAttribute>();
            if (customAttribute13 != null)
            {
                list.Add(customAttribute13);
            }

            MustTrimAttribute customAttribute14 = propertyInfo.GetCustomAttribute<MustTrimAttribute>();
            if (customAttribute14 != null)
            {
                list.Add(customAttribute14);
            }

            MustWordNumberAttribute customAttribute15 = propertyInfo.GetCustomAttribute<MustWordNumberAttribute>();
            if (customAttribute15 != null)
            {
                list.Add(customAttribute15);
            }

            MustNumberAttribute customAttribute16 = propertyInfo.GetCustomAttribute<MustNumberAttribute>();
            if (customAttribute16 != null)
            {
                list.Add(customAttribute16);
            }

            MustEmailAttribute customAttribute17 = propertyInfo.GetCustomAttribute<MustEmailAttribute>();
            if (customAttribute17 != null)
            {
                list.Add(customAttribute17);
            }

            MustPhoneAttribute customAttribute18 = propertyInfo.GetCustomAttribute<MustPhoneAttribute>();
            if (customAttribute18 != null)
            {
                list.Add(customAttribute18);
            }

            if (list.Any())
            {
                openApiSchema.Description = "<span class='sc-laZMeE dmLkmF'>Validators:</span> " + (from x in list
                                                                                                    select x.GetType().Name into x
                                                                                                    select x.Substring(0, x.Length - 9) into x
                                                                                                    select x.SplitCase().ToString(" ") into x
                                                                                                    select "`" + x + "`").ToString(" ") + "<br/>" + openApiSchema.Description;
            }

            return openApiSchema;
        }

        internal static IOpenApiAny? SwaggerValue(this Type type, object? value)
        {
            if (value == null)
            {
                return new OpenApiNull();
            }

            if (value is string)
            {
                return new OpenApiString((string)value);
            }

            if (value is bool)
            {
                return new OpenApiBoolean((bool)value);
            }

            if (value is byte)
            {
                return new OpenApiByte((byte)value);
            }

            if (value is int)
            {
                return new OpenApiInteger((int)value);
            }

            if (value is long)
            {
                return new OpenApiLong((long)value);
            }

            if (value is float)
            {
                return new OpenApiFloat((float)value);
            }

            if (value is double)
            {
                return new OpenApiDouble((double)value);
            }

            if (value is decimal)
            {
                return new OpenApiDouble(Convert.ToDouble(value));
            }

            if (value is DateTime)
            {
                return new OpenApiInteger((int)((DateTime)value).ToEpochTimestamp());
            }

            if (value is Enum)
            {
                return new OpenApiInteger((int)value);
            }

            if (type.IsArray)
            {
                OpenApiArray openApiArray = new OpenApiArray();
                Array array = value as Array;
                if (array == null)
                {
                    return openApiArray;
                }

                if (array.Length == 0)
                {
                    return openApiArray;
                }

                foreach (object item in array)
                {
                    openApiArray.Add(item.GetType().SwaggerValue(item));
                }

                if (!openApiArray.Where((IOpenApiAny x) => x != null).Any())
                {
                    return null;
                }

                return openApiArray;
            }

            if (type.IsAssignableTo<ICollection>())
            {
                OpenApiArray openApiArray2 = new OpenApiArray();
                ICollection collection = value as ICollection;
                if (collection == null)
                {
                    return openApiArray2;
                }

                if (collection.Count == 0)
                {
                    return openApiArray2;
                }

                foreach (object item2 in collection)
                {
                    openApiArray2.Add(item2.GetType().SwaggerValue(item2));
                }

                if (!openApiArray2.Where((IOpenApiAny x) => x != null).Any())
                {
                    return null;
                }

                return openApiArray2;
            }

            return null;
        }
    }
}
