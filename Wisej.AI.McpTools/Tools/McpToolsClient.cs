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

using ModelContextProtocol.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Wisej.AI.Tools;
using static Wisej.AI.SmartTool;

namespace Wisej.AI
{
	/// <summary>
	/// Provides a client for accessing and managing tools from an MCP (Modular Command Platform) service.
	/// </summary>
	/// <remarks>
	/// The <see cref="McpToolsClient"/> class allows you to connect to an MCP service using various constructors, 
	/// retrieve available tools, and access them through the <see cref="Tools"/> property. 
	/// It supports initialization via URL, URI, or a custom transport.
	/// </remarks>
	[ApiCategory("Tools")]
	public class McpToolsClient : IToolsProvider
	{
		private ToolCollection _tools;

		/// <summary>
		/// Initializes a new instance of the <see cref="McpToolsClient"/> class using the specified service URL.
		/// </summary>
		/// <param name="url">The URL of the MCP service endpoint.</param>
		public McpToolsClient(string url)
			: this("", "", new Uri(url))
		{ }

		/// <summary>
		/// Initializes a new instance of the <see cref="McpToolsClient"/> class with a name, description, and service URL.
		/// </summary>
		/// <param name="name">The namespace or logical name for the tools.</param>
		/// <param name="description">A description for the tool namespace.</param>
		/// <param name="url">The URL of the MCP service endpoint.</param>

		public McpToolsClient(string name, string description, string url)
			: this(
				name,
				description,
				new Uri(url))
		{ }

		/// <summary>
		/// Initializes a new instance of the <see cref="McpToolsClient"/> class using the specified service URI.
		/// </summary>
		/// <param name="uri">The URI of the MCP service endpoint.</param>

		public McpToolsClient(Uri uri)
			: this("", "", uri)
		{ }

		/// <summary>
		/// Initializes a new instance of the <see cref="McpToolsClient"/> class with a name, description, and service URI.
		/// </summary>
		/// <param name="name">The namespace or logical name for the tools.</param>
		/// <param name="description">A description for the tool namespace.</param>
		/// <param name="uri">The URI of the MCP service endpoint.</param>
		public McpToolsClient(string name, string description, Uri uri)
			: this(
				name,
				description,
				new SseClientTransport(new SseClientTransportOptions
				{
					Endpoint = uri
				}))
		{ }

		/// <summary>
		/// Initializes a new instance of the <see cref="McpToolsClient"/> class using a custom client transport.
		/// </summary>
		/// <param name="clientTransport">The transport mechanism for communicating with the MCP service.</param>

		public McpToolsClient(IClientTransport clientTransport)
			: this("", "", clientTransport)
		{ }

		/// <summary>
		/// Initializes a new instance of the <see cref="McpToolsClient"/> class with a name, description, and custom client transport.
		/// </summary>
		/// <param name="name">The namespace or logical name for the tools.</param>
		/// <param name="description">A description for the tool namespace.</param>
		/// <param name="clientTransport">The transport mechanism for communicating with the MCP service.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="clientTransport"/> is <c>null</c>.</exception>
		/// <remarks>
		/// This constructor allows you to fully customize the client, including the transport, name, and description.
		/// </remarks>
		public McpToolsClient(string name, string description, IClientTransport clientTransport)
		{
			if (clientTransport == null)
				throw new ArgumentNullException(nameof(clientTransport));

			var client = McpClientFactory.CreateAsync(clientTransport).Result;
			var tools = client.ListToolsAsync().Result;
			ImportTools(name, description, tools);
		}

		/// <summary>
		/// Gets a value indicating whether any tools are available in this client.
		/// </summary>
		public bool HasTools
			=> _tools?.Count > 0;

		/// <summary>
		/// Gets the collection of tools imported from the MCP service.
		/// </summary>
		public ToolCollection Tools
			=> _tools;

		//
		private void ImportTools(string name, string description, IList<McpClientTool> tools)
		{
			_tools = new ToolCollection();

			description = SmartPrompt.ResolvePrompt(description);

			foreach (var tool in tools)
			{
				var smartTool = new McpTool(tool);
				smartTool.Namespace = name;
				smartTool.NamespaceDescription = description;

				_tools.Add(smartTool.FullName, smartTool);
			}
		}
	}
}
