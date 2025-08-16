# Wisej.AI.McpTools

Welcome to the Wisej.AI.McpTools repository! This library provides a powerful tool for Wisej.AI, allowing you to seamlessly import and utilize remote Model Context Protocol (MCP) tools.

## Usage

Here is an example of how to utilize Wisej.AI.McpTools in your application:

```csharp
this.smartPrompt
    .UseTools(new McpToolsClient(
        "Fetch",
        "A Model Context Protocol server that provides web content fetching capabilities.",
        "https://remote.mcpservers.org/fetch"));
```

In this example, the `McpToolsClient` is used to connect to a remote MCP server with the ability to fetch web content and convert it for use with LLMs.

## Requirements

- Must be a Wisej.NET technology partner with access to Wisej.AI
