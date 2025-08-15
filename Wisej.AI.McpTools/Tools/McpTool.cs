///////////////////////////////////////////////////////////////////////////////
//
// (C) 2025 ICE TEA GROUP LLC - ALL RIGHTS RESERVED
//
// 
//
// ALL INFORMATION CONTAINED HEREIN IS, AND REMAINS
// THE PROPERTY OF ICE TEA GROUP LLC AND ITS SUPPLIERS, IF ANY.
// THE INTELLECTUAL PROPERTY AND TECHNICAL CONCEPTS CONTAINED
// HEREIN ARE PROPRIETARY TO ICE TEA GROUP LLC AND ITS SUPPLIERS
// AND MAY BE COVERED BY U.S. AND FOREIGN PATENTS, PATENT IN PROCESS, AND
// ARE PROTECTED BY TRADE SECRET OR COPYRIGHT LAW.
//
// DISSEMINATION OF THIS INFORMATION OR REPRODUCTION OF THIS MATERIAL
// IS STRICTLY FORBIDDEN UNLESS PRIOR WRITTEN PERMISSION IS OBTAINED
// FROM ICE TEA GROUP LLC.
//
///////////////////////////////////////////////////////////////////////////////

using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Wisej.Core;

namespace Wisej.AI
{
	/// <summary>
	/// Represents a tool that integrates with an MCP client, providing functionality to invoke
	/// operations and retrieve parameter schemas.
	/// </summary>
	internal class McpTool : SmartTool
	{
		McpClientTool _tool;
		dynamic _parametersSchema;

		/// <summary>
		/// Initializes a new instance of the <see cref="McpTool"/> class with the specified MCP client tool.
		/// </summary>
		/// <param name="tool">The MCP client tool to integrate with.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="tool"/> is null.</exception>
		public McpTool(McpClientTool tool)
		{
			if (tool == null)
				throw new ArgumentNullException(nameof(tool));

			_tool = tool;
			_parametersSchema = GetParametersSchema(tool);

			this.Name = tool.Name;
			this.Description = tool.Description;
			this.Parameters = GetParameters(tool);
		}

		//
		private Parameter[] GetParameters(McpClientTool tool)
		{
			var jsonSchema = tool.JsonSchema;
			var requiredParameterNames = new string[0];

			if (jsonSchema.TryGetProperty("required", out JsonElement required))
			{
				requiredParameterNames = required.EnumerateArray().Select(n => n.GetString()).ToArray();
			}

			var parameters = new List<Parameter>();
			if (jsonSchema.TryGetProperty("properties", out JsonElement properties))
			{
				foreach (var param in properties.EnumerateObject())
				{
					parameters.Add(
						new Parameter
						{
							Name = param.Name,
							DefaultValue = GetDefaultValue(param),
							ParameterType = GetParameterType(param),
							Required = requiredParameterNames.Contains(param.Name),
							TypeName = param.Value.TryGetProperty("type", out JsonElement type) ? type.GetString() : null,
							Description = param.Value.TryGetProperty("description", out JsonElement description) ? description.GetString() : null,
						});
				}
			}


			return parameters.ToArray();
		}

		//
		private object GetDefaultValue(JsonProperty param)
		{
			if (param.Value.TryGetProperty("default", out JsonElement defaultValue))
			{
				switch (defaultValue.ValueKind)
				{
					case JsonValueKind.String:
						return defaultValue.GetString();

					case JsonValueKind.Number:
						return defaultValue.GetDouble();

					case JsonValueKind.False:
						return false;

					case JsonValueKind.True:
						return true;

					default:
						return defaultValue.GetRawText();
				}
			}

			return null;
		}

		//
		private Type GetParameterType(JsonProperty param)
		{
			if (param.Value.TryGetProperty("type", out JsonElement type))
			{
				switch (type.GetString())
				{
					case "string":
						return typeof(string);
					case "number":
						return typeof(double);
					case "array":
						return typeof(Array);
					default:
						return typeof(object);
				}
			}

			return typeof(string);
		}

		//
		private dynamic GetParametersSchema(McpClientTool tool)
		{
			dynamic schema = new DynamicObject();
			schema.type = "object";

			var jsonSchema = tool.JsonSchema;
			if (jsonSchema.TryGetProperty("properties", out JsonElement properties))
				schema.properties = JSON.Parse(properties.ToString());

			return schema;
		}

		/// <inheritdoc/>
		public override async Task<object> InvokeAsync(ToolContext context)
		{
			var parameters = this.Parameters;
			var args = new AIFunctionArguments();

			if (parameters.Length > 0)
			{
				foreach (var p in parameters)
				{
					if (context.Arguments.TryGetValue(p.Name, out object value))
					{
						args.Add(p.Name, ConvertValue(value, p.ParameterType));
					}
					else if (!p.Required)
					{
						args.Add(p.Name, p.DefaultValue);
					}
					else
					{
						args.Add(p.Name, Type.Missing);
					}
				}
			}

			var task = _tool.InvokeAsync(args);
			await task;
			return JSON.Parse(task.Result.ToString());
		}

		/// <inheritdoc/>
		public override dynamic GetParametersSchema()
			=> _parametersSchema;
	}
}
