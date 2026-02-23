using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using UnityMcp.Server.Protocol;

namespace UnityMcp.Server.Mcp;

public sealed class McpRequestHandler
{
    private const string DefaultProtocolVersion = "2025-06-18";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = null,
        WriteIndented = false
    };

    private readonly McpToolCatalog _toolCatalog;
    private readonly IUnityJsonRpcForwarder _unityForwarder;
    private readonly IUnityConnectionStatusProvider _unityConnectionStatusProvider;
    private readonly ILogger<McpRequestHandler> _logger;
    private long _nextUnityRequestId;

    public McpRequestHandler(
        McpToolCatalog toolCatalog,
        IUnityJsonRpcForwarder unityForwarder,
        IUnityConnectionStatusProvider unityConnectionStatusProvider,
        ILogger<McpRequestHandler> logger)
    {
        _toolCatalog = toolCatalog;
        _unityForwarder = unityForwarder;
        _unityConnectionStatusProvider = unityConnectionStatusProvider;
        _logger = logger;
    }

    public async Task<McpHttpResponse> HandlePostAsync(string requestBody, CancellationToken cancellationToken)
    {
        if (!JsonRpcProtocol.TryParse(requestBody, out var document, out var parseError))
        {
            return Json(200, JsonRpcProtocol.CreateError(
                idNode: null,
                code: JsonRpcErrorCodes.ParseError,
                message: $"Invalid JSON: {parseError}"));
        }

        using var parsedDocument = document!;
        var root = parsedDocument.RootElement;

        if (root.ValueKind != JsonValueKind.Object)
        {
            return Json(200, JsonRpcProtocol.CreateError(
                idNode: null,
                code: JsonRpcErrorCodes.InvalidRequest,
                message: "JSON-RPC payload must be an object."));
        }

        if (!JsonRpcProtocol.TryGetMethod(root, out var method) || string.IsNullOrWhiteSpace(method))
        {
            return Json(200, JsonRpcProtocol.CreateError(
                idNode: null,
                code: JsonRpcErrorCodes.InvalidRequest,
                message: "JSON-RPC request must include a method name."));
        }

        var hasIdProperty = root.TryGetProperty("id", out _);
        JsonNode? requestIdNode = null;
        string? requestIdKey = null;

        if (hasIdProperty)
        {
            if (!JsonRpcProtocol.TryGetId(root, out requestIdNode, out requestIdKey) || string.IsNullOrWhiteSpace(requestIdKey))
            {
                return Json(200, JsonRpcProtocol.CreateError(
                    idNode: null,
                    code: JsonRpcErrorCodes.InvalidRequest,
                    message: "JSON-RPC request id must be a string, number, or null."));
            }
        }

        var hasParams = root.TryGetProperty("params", out var paramsElement);
        var isNotification = !hasIdProperty;

        try
        {
            switch (method)
            {
                case "initialize":
                    if (isNotification)
                    {
                        return Json(200, JsonRpcProtocol.CreateError(
                            idNode: null,
                            code: JsonRpcErrorCodes.InvalidRequest,
                            message: "initialize must include an id."));
                    }

                    return Json(200, JsonRpcProtocol.CreateResult(requestIdNode, CreateInitializeResult(hasParams ? paramsElement : default)));

                case "notifications/initialized":
                    return isNotification
                        ? Accepted()
                        : Json(200, JsonRpcProtocol.CreateResult(requestIdNode, new JsonObject()));

                case "ping":
                    return isNotification
                        ? Accepted()
                        : Json(200, JsonRpcProtocol.CreateResult(requestIdNode, new JsonObject()));

                case "tools/list":
                    if (isNotification)
                    {
                        return Accepted();
                    }

                    return Json(200, JsonRpcProtocol.CreateResult(requestIdNode, CreateToolsListResult()));

                case "prompts/list":
                    if (isNotification)
                    {
                        return Accepted();
                    }

                    return Json(200, JsonRpcProtocol.CreateResult(requestIdNode, CreatePromptsListResult()));

                case "resources/list":
                    if (isNotification)
                    {
                        return Accepted();
                    }

                    return Json(200, JsonRpcProtocol.CreateResult(requestIdNode, CreateResourcesListResult()));

                case "resources/templates/list":
                    if (isNotification)
                    {
                        return Accepted();
                    }

                    return Json(200, JsonRpcProtocol.CreateResult(requestIdNode, CreateResourceTemplatesListResult()));

                case "resources/read":
                    if (isNotification)
                    {
                        return Accepted();
                    }

                    return await HandleResourcesReadAsync(requestIdNode, hasParams ? paramsElement : default, cancellationToken);

                case "tools/call":
                    if (isNotification)
                    {
                        return Accepted();
                    }

                    return await HandleToolsCallAsync(requestIdNode, hasParams ? paramsElement : default, cancellationToken);

                default:
                    if (isNotification)
                    {
                        _logger.LogDebug("Ignoring unsupported JSON-RPC notification method {Method}.", method);
                        return Accepted();
                    }

                    return Json(200, JsonRpcProtocol.CreateError(
                        idNode: requestIdNode,
                        code: JsonRpcErrorCodes.MethodNotFound,
                        message: $"Method '{method}' is not supported."));
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled MCP request error. Method={Method}", method);
            return Json(200, JsonRpcProtocol.CreateError(
                idNode: requestIdNode,
                code: JsonRpcErrorCodes.InternalError,
                message: "Internal server error."));
        }
    }

    private async Task<McpHttpResponse> HandleToolsCallAsync(JsonNode? requestIdNode, JsonElement paramsElement, CancellationToken cancellationToken)
    {
        if (paramsElement.ValueKind != JsonValueKind.Object)
        {
            return Json(200, JsonRpcProtocol.CreateError(
                requestIdNode,
                JsonRpcErrorCodes.InvalidParams,
                "tools/call params must be an object."));
        }

        if (!paramsElement.TryGetProperty("name", out var nameElement) || nameElement.ValueKind != JsonValueKind.String)
        {
            return Json(200, JsonRpcProtocol.CreateError(
                requestIdNode,
                JsonRpcErrorCodes.InvalidParams,
                "tools/call params.name must be a string."));
        }

        var toolName = nameElement.GetString();
        if (string.IsNullOrWhiteSpace(toolName))
        {
            return Json(200, JsonRpcProtocol.CreateError(
                requestIdNode,
                JsonRpcErrorCodes.InvalidParams,
                "tools/call params.name cannot be empty."));
        }

        if (!_toolCatalog.TryGet(toolName, out _))
        {
            return Json(200, JsonRpcProtocol.CreateResult(
                requestIdNode,
                CreateToolResult(
                    isError: true,
                    text: $"Unknown tool '{toolName}'.",
                    structuredContent: new JsonObject
                    {
                        ["tool"] = toolName,
                        ["errorType"] = "unknown_tool"
                    })));
        }

        JsonNode? argumentsNode = null;
        if (paramsElement.TryGetProperty("arguments", out var argumentsElement))
        {
            if (argumentsElement.ValueKind is not JsonValueKind.Object and not JsonValueKind.Null)
            {
                return Json(200, JsonRpcProtocol.CreateError(
                    requestIdNode,
                    JsonRpcErrorCodes.InvalidParams,
                    "tools/call params.arguments must be an object or null."));
            }

            if (argumentsElement.ValueKind != JsonValueKind.Null)
            {
                argumentsNode = JsonNode.Parse(argumentsElement.GetRawText());
            }
        }

        var unityRequestId = $"mcp-{Interlocked.Increment(ref _nextUnityRequestId)}";
        var unityRequest = new JsonObject
        {
            ["jsonrpc"] = "2.0",
            ["id"] = unityRequestId,
            ["method"] = toolName
        };

        if (argumentsNode is not null)
        {
            unityRequest["params"] = argumentsNode.DeepClone();
        }

        try
        {
            var unityResponseJson = await _unityForwarder.ForwardAsync(
                unityRequest.ToJsonString(SerializerOptions),
                requestIdKey: $"s:{unityRequestId}",
                cancellationToken);

            var toolResult = CreateToolResultFromUnityResponse(toolName, unityResponseJson);
            return Json(200, JsonRpcProtocol.CreateResult(requestIdNode, toolResult));
        }
        catch (InvalidOperationException ex)
        {
            return Json(200, JsonRpcProtocol.CreateResult(
                requestIdNode,
                CreateToolResult(
                    isError: true,
                    text: ex.Message,
                    structuredContent: new JsonObject
                    {
                        ["tool"] = toolName,
                        ["errorType"] = "relay_error"
                    })));
        }
        catch (TimeoutException)
        {
            return Json(200, JsonRpcProtocol.CreateResult(
                requestIdNode,
                CreateToolResult(
                    isError: true,
                    text: "Timed out waiting for Unity response.",
                    structuredContent: new JsonObject
                    {
                        ["tool"] = toolName,
                        ["errorType"] = "timeout"
                    })));
        }
    }

    private JsonObject CreateInitializeResult(JsonElement paramsElement)
    {
        var protocolVersion = DefaultProtocolVersion;

        if (paramsElement.ValueKind == JsonValueKind.Object &&
            paramsElement.TryGetProperty("protocolVersion", out var protocolVersionElement) &&
            protocolVersionElement.ValueKind == JsonValueKind.String)
        {
            var requested = protocolVersionElement.GetString();
            if (!string.IsNullOrWhiteSpace(requested))
            {
                protocolVersion = requested;
            }
        }

        return new JsonObject
        {
            ["protocolVersion"] = protocolVersion,
            ["capabilities"] = new JsonObject
            {
                ["tools"] = new JsonObject
                {
                    ["listChanged"] = false
                },
                ["prompts"] = new JsonObject
                {
                    ["listChanged"] = false
                },
                ["resources"] = new JsonObject
                {
                    ["subscribe"] = false,
                    ["listChanged"] = false
                }
            },
            ["serverInfo"] = new JsonObject
            {
                ["name"] = "UnityMCP Server",
                ["version"] = "0.1.0"
            },
            ["instructions"] = "Use the Unity tools to inspect and control the connected Unity Editor instance."
        };
    }

    private JsonObject CreateToolsListResult()
    {
        var tools = new JsonArray();

        foreach (var tool in _toolCatalog.Tools)
        {
            tools.Add(new JsonObject
            {
                ["name"] = tool.Name,
                ["description"] = tool.Description,
                ["inputSchema"] = tool.InputSchema.DeepClone()
            });
        }

        return new JsonObject
        {
            ["tools"] = tools
        };
    }

    private static JsonObject CreatePromptsListResult()
    {
        return new JsonObject
        {
            ["prompts"] = new JsonArray()
        };
    }

    private JsonObject CreateResourcesListResult()
    {
        var resources = new JsonArray
        {
            new JsonObject
            {
                ["uri"] = "unitymcp://server/info",
                ["name"] = "Server Info",
                ["description"] = "UnityMCP server info and MCP feature summary.",
                ["mimeType"] = "application/json"
            },
            new JsonObject
            {
                ["uri"] = "unitymcp://unity/connection",
                ["name"] = "Unity Connection",
                ["description"] = "Current Unity bridge connection status.",
                ["mimeType"] = "application/json"
            },
            new JsonObject
            {
                ["uri"] = "unitymcp://editor/playmode-state",
                ["name"] = "Editor Play Mode State",
                ["description"] = "Current Unity Editor play mode and compilation state.",
                ["mimeType"] = "application/json"
            },
            new JsonObject
            {
                ["uri"] = "unitymcp://editor/console-logs",
                ["name"] = "Editor Console Logs",
                ["description"] = "Bounded snapshot of recent Unity Editor console logs (default options).",
                ["mimeType"] = "application/json"
            },
            new JsonObject
            {
                ["uri"] = "unitymcp://scene/active",
                ["name"] = "Active Scene",
                ["description"] = "Metadata for the currently active Unity scene.",
                ["mimeType"] = "application/json"
            },
            new JsonObject
            {
                ["uri"] = "unitymcp://scene/open-scenes",
                ["name"] = "Open Scenes",
                ["description"] = "Metadata for all currently open Unity scenes.",
                ["mimeType"] = "application/json"
            },
            new JsonObject
            {
                ["uri"] = "unitymcp://scene/selection",
                ["name"] = "Scene Selection",
                ["description"] = "Current Unity Editor selection metadata.",
                ["mimeType"] = "application/json"
            },
            new JsonObject
            {
                ["uri"] = "unitymcp://scene/selection/active",
                ["name"] = "Active Selection",
                ["description"] = "Active Unity Editor selection object (or null).",
                ["mimeType"] = "application/json"
            }
        };

        return new JsonObject
        {
            ["resources"] = resources
        };
    }

    private static JsonObject CreateResourceTemplatesListResult()
    {
        var templates = new JsonArray
        {
            new JsonObject
            {
                ["uriTemplate"] = "unitymcp://scene/find-by-tag/{tag}",
                ["name"] = "Scene Find By Tag",
                ["description"] = "Returns active GameObjects with the specified Unity tag.",
                ["mimeType"] = "application/json"
            },
            new JsonObject
            {
                ["uriTemplate"] = "unitymcp://assets/find/{query}",
                ["name"] = "Assets Find",
                ["description"] = "Searches Unity assets using AssetDatabase.FindAssets(query).",
                ["mimeType"] = "application/json"
            },
            new JsonObject
            {
                ["uriTemplate"] = "unitymcp://editor/console-tail/{afterSequence}",
                ["name"] = "Editor Console Tail",
                ["description"] = "Returns Unity console log entries after a sequence cursor (default options).",
                ["mimeType"] = "application/json"
            },
            new JsonObject
            {
                ["uriTemplate"] = "unitymcp://scene/selection/index/{index}",
                ["name"] = "Scene Selection Item By Index",
                ["description"] = "Returns a single selection item by 0-based index from the current Unity selection.",
                ["mimeType"] = "application/json"
            }
        };

        return new JsonObject
        {
            ["resourceTemplates"] = templates
        };
    }

    private async Task<McpHttpResponse> HandleResourcesReadAsync(JsonNode? requestIdNode, JsonElement paramsElement, CancellationToken cancellationToken)
    {
        if (paramsElement.ValueKind != JsonValueKind.Object)
        {
            return Json(200, JsonRpcProtocol.CreateError(
                requestIdNode,
                JsonRpcErrorCodes.InvalidParams,
                "resources/read params must be an object."));
        }

        if (!paramsElement.TryGetProperty("uri", out var uriElement) || uriElement.ValueKind != JsonValueKind.String)
        {
            return Json(200, JsonRpcProtocol.CreateError(
                requestIdNode,
                JsonRpcErrorCodes.InvalidParams,
                "resources/read params.uri must be a string."));
        }

        var uriText = uriElement.GetString();
        if (string.IsNullOrWhiteSpace(uriText))
        {
            return Json(200, JsonRpcProtocol.CreateError(
                requestIdNode,
                JsonRpcErrorCodes.InvalidParams,
                "resources/read params.uri cannot be empty."));
        }

        if (!Uri.TryCreate(uriText, UriKind.Absolute, out var uri) || !string.Equals(uri.Scheme, "unitymcp", StringComparison.OrdinalIgnoreCase))
        {
            return Json(200, JsonRpcProtocol.CreateError(
                requestIdNode,
                JsonRpcErrorCodes.InvalidParams,
                "resources/read params.uri must be a valid unitymcp:// URI."));
        }

        try
        {
            var result = await CreateResourcesReadResultAsync(uri, cancellationToken);
            return Json(200, JsonRpcProtocol.CreateResult(requestIdNode, result));
        }
        catch (ArgumentException ex)
        {
            return Json(200, JsonRpcProtocol.CreateError(
                requestIdNode,
                JsonRpcErrorCodes.InvalidParams,
                ex.Message,
                CreateResourceInvalidParamsData(uriText, ex)));
        }
        catch (InvalidOperationException ex)
        {
            return Json(200, JsonRpcProtocol.CreateError(requestIdNode, JsonRpcErrorCodes.UnityNotConnected, ex.Message));
        }
        catch (TimeoutException)
        {
            return Json(200, JsonRpcProtocol.CreateError(requestIdNode, JsonRpcErrorCodes.UnityTimeout, "Timed out waiting for Unity response."));
        }
    }

    private async Task<JsonObject> CreateResourcesReadResultAsync(Uri uri, CancellationToken cancellationToken)
    {
        var host = uri.Host;
        var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        JsonNode? payloadNode;
        if (string.Equals(host, "server", StringComparison.OrdinalIgnoreCase) &&
            segments.Length == 1 &&
            string.Equals(segments[0], "info", StringComparison.OrdinalIgnoreCase))
        {
            payloadNode = new JsonObject
            {
                ["name"] = "UnityMCP Server",
                ["version"] = "0.1.0",
                ["transport"] = "MCP HTTP + Unity WebSocket bridge",
                ["mcpEndpoint"] = "/mcp",
                ["unityEndpoint"] = "/ws/unity",
                ["unityConnected"] = _unityConnectionStatusProvider.IsUnityConnected
            };
        }
        else if (string.Equals(host, "unity", StringComparison.OrdinalIgnoreCase) &&
                 segments.Length == 1 &&
                 string.Equals(segments[0], "connection", StringComparison.OrdinalIgnoreCase))
        {
            payloadNode = new JsonObject
            {
                ["connected"] = _unityConnectionStatusProvider.IsUnityConnected,
                ["transport"] = "WebSocket"
            };
        }
        else if (string.Equals(host, "editor", StringComparison.OrdinalIgnoreCase) &&
                 segments.Length == 1 &&
                 string.Equals(segments[0], "playmode-state", StringComparison.OrdinalIgnoreCase))
        {
            payloadNode = await CallUnityMethodResultAsync("editor.getPlayModeState", argumentsNode: null, cancellationToken);
        }
        else if (string.Equals(host, "editor", StringComparison.OrdinalIgnoreCase) &&
                 segments.Length == 1 &&
                 string.Equals(segments[0], "console-logs", StringComparison.OrdinalIgnoreCase))
        {
            var argumentsNode = CreateConsoleResourceArguments(ParseResourceQueryParameters(uri));
            payloadNode = await CallUnityMethodResultAsync("editor.getConsoleLogs", argumentsNode, cancellationToken);
        }
        else if (string.Equals(host, "editor", StringComparison.OrdinalIgnoreCase) &&
                 segments.Length == 2 &&
                 string.Equals(segments[0], "console-tail", StringComparison.OrdinalIgnoreCase))
        {
            var afterSequenceText = Uri.UnescapeDataString(segments[1]);
            var queryParameters = ParseResourceQueryParameters(uri);
            var argumentsNode = CreateConsoleTailResourceArguments(queryParameters, afterSequenceText);
            payloadNode = await CallUnityMethodResultAsync("editor.consoleTail", argumentsNode, cancellationToken);
        }
        else if (string.Equals(host, "scene", StringComparison.OrdinalIgnoreCase) &&
                 segments.Length == 1 &&
                 string.Equals(segments[0], "active", StringComparison.OrdinalIgnoreCase))
        {
            payloadNode = await CallUnityMethodResultAsync("scene.getActiveScene", argumentsNode: null, cancellationToken);
        }
        else if (string.Equals(host, "scene", StringComparison.OrdinalIgnoreCase) &&
                 segments.Length == 1 &&
                 string.Equals(segments[0], "open-scenes", StringComparison.OrdinalIgnoreCase))
        {
            payloadNode = await CallUnityMethodResultAsync("scene.listOpenScenes", argumentsNode: null, cancellationToken);
        }
        else if (string.Equals(host, "scene", StringComparison.OrdinalIgnoreCase) &&
                 segments.Length == 2 &&
                 string.Equals(segments[0], "selection", StringComparison.OrdinalIgnoreCase) &&
                 string.Equals(segments[1], "active", StringComparison.OrdinalIgnoreCase))
        {
            var selectionNode = await CallUnityMethodResultAsync("scene.getSelection", argumentsNode: null, cancellationToken);
            payloadNode = ProjectSelectionActiveResource(selectionNode);
        }
        else if (string.Equals(host, "scene", StringComparison.OrdinalIgnoreCase) &&
                 segments.Length == 3 &&
                 string.Equals(segments[0], "selection", StringComparison.OrdinalIgnoreCase) &&
                 string.Equals(segments[1], "index", StringComparison.OrdinalIgnoreCase))
        {
            var selectionNode = await CallUnityMethodResultAsync("scene.getSelection", argumentsNode: null, cancellationToken);
            payloadNode = ProjectSelectionIndexResource(selectionNode, Uri.UnescapeDataString(segments[2]));
        }
        else if (string.Equals(host, "scene", StringComparison.OrdinalIgnoreCase) &&
                 segments.Length == 1 &&
                 string.Equals(segments[0], "selection", StringComparison.OrdinalIgnoreCase))
        {
            payloadNode = await CallUnityMethodResultAsync("scene.getSelection", argumentsNode: null, cancellationToken);
        }
        else if (string.Equals(host, "scene", StringComparison.OrdinalIgnoreCase) &&
                 segments.Length == 2 &&
                 string.Equals(segments[0], "find-by-tag", StringComparison.OrdinalIgnoreCase))
        {
            var tag = Uri.UnescapeDataString(segments[1]);
            if (string.IsNullOrWhiteSpace(tag))
            {
                throw new ArgumentException("Resource tag segment cannot be empty.");
            }

            payloadNode = await CallUnityMethodResultAsync(
                "scene.findByTag",
                new JsonObject
                {
                    ["tag"] = tag
                },
                cancellationToken);
        }
        else if (string.Equals(host, "assets", StringComparison.OrdinalIgnoreCase) &&
                 segments.Length == 2 &&
                 string.Equals(segments[0], "find", StringComparison.OrdinalIgnoreCase))
        {
            var query = Uri.UnescapeDataString(segments[1]);
            if (string.IsNullOrWhiteSpace(query))
            {
                throw new ArgumentException("Asset query segment cannot be empty.", "query");
            }

            var argumentsNode = CreateAssetsFindResourceArguments(query, ParseResourceQueryParameters(uri));
            payloadNode = await CallUnityMethodResultAsync("assets.find", argumentsNode, cancellationToken);
        }
        else
        {
            throw new ArgumentException($"Unsupported resource URI '{uri}'.");
        }

        var payloadJson = payloadNode?.ToJsonString(SerializerOptions) ?? "null";
        return new JsonObject
        {
            ["contents"] = new JsonArray
            {
                new JsonObject
                {
                    ["uri"] = uri.ToString(),
                    ["mimeType"] = "application/json",
                    ["text"] = payloadJson
                }
            }
        };
    }

    private static Dictionary<string, StringValues> ParseResourceQueryParameters(Uri uri)
    {
        return QueryHelpers.ParseQuery(uri.Query);
    }

    private static JsonObject? CreateConsoleResourceArguments(IReadOnlyDictionary<string, StringValues> queryParameters)
    {
        ValidateAllowedQueryKeys(queryParameters, "maxResults", "includeStackTrace", "contains", "level");

        JsonObject? argumentsNode = null;
        AddConsoleQueryArguments(ref argumentsNode, queryParameters);
        return argumentsNode;
    }

    private static JsonObject CreateConsoleTailResourceArguments(IReadOnlyDictionary<string, StringValues> queryParameters, string afterSequenceText)
    {
        ValidateAllowedQueryKeys(queryParameters, "maxResults", "includeStackTrace", "contains", "level");

        if (!long.TryParse(afterSequenceText, out var afterSequence) || afterSequence < 0)
        {
            throw new ArgumentException("Console tail sequence segment must be a non-negative integer.", "afterSequence");
        }

        var argumentsNode = new JsonObject
        {
            ["afterSequence"] = afterSequence
        };

        JsonObject? optionalArguments = argumentsNode;
        AddConsoleQueryArguments(ref optionalArguments, queryParameters);
        return argumentsNode;
    }

    private static JsonObject CreateAssetsFindResourceArguments(string query, IReadOnlyDictionary<string, StringValues> queryParameters)
    {
        ValidateAllowedQueryKeys(queryParameters, "maxResults", "folder", "type", "label");

        var argumentsNode = new JsonObject
        {
            ["query"] = query
        };

        if (TryGetSingleQueryParameter(queryParameters, "maxResults", out var maxResultsText))
        {
            argumentsNode["maxResults"] = ParseQueryIntInRange(maxResultsText!, "maxResults", min: 1, max: 500);
        }

        AddRepeatedStringQueryParameter(argumentsNode, queryParameters, queryKey: "folder", argumentName: "searchInFolders");
        AddRepeatedStringQueryParameter(argumentsNode, queryParameters, queryKey: "type", argumentName: "types");
        AddRepeatedStringQueryParameter(argumentsNode, queryParameters, queryKey: "label", argumentName: "labels");

        return argumentsNode;
    }

    private static void ValidateAllowedQueryKeys(IReadOnlyDictionary<string, StringValues> queryParameters, params string[] allowedKeys)
    {
        if (queryParameters.Count == 0)
        {
            return;
        }

        var allowed = new HashSet<string>(allowedKeys, StringComparer.OrdinalIgnoreCase);
        foreach (var key in queryParameters.Keys)
        {
            if (!allowed.Contains(key))
            {
                throw new ArgumentException($"Unsupported resource query parameter '{key}'.", key);
            }
        }
    }

    private static void AddConsoleQueryArguments(ref JsonObject? argumentsNode, IReadOnlyDictionary<string, StringValues> queryParameters)
    {
        if (TryGetSingleQueryParameter(queryParameters, "maxResults", out var maxResultsText))
        {
            argumentsNode ??= new JsonObject();
            argumentsNode["maxResults"] = ParseQueryIntInRange(maxResultsText!, "maxResults", min: 1, max: 500);
        }

        if (TryGetSingleQueryParameter(queryParameters, "includeStackTrace", out var includeStackTraceText))
        {
            argumentsNode ??= new JsonObject();
            argumentsNode["includeStackTrace"] = ParseQueryBoolean(includeStackTraceText!, "includeStackTrace");
        }

        if (TryGetSingleQueryParameter(queryParameters, "contains", out var containsText))
        {
            argumentsNode ??= new JsonObject();
            argumentsNode["contains"] = containsText;
        }

        AddRepeatedConsoleLevelQueryArgument(ref argumentsNode, queryParameters);
    }

    private static bool TryGetSingleQueryParameter(IReadOnlyDictionary<string, StringValues> queryParameters, string key, out string? value)
    {
        value = null;
        if (!queryParameters.TryGetValue(key, out var values))
        {
            return false;
        }

        if (values.Count != 1)
        {
            throw new ArgumentException($"Query parameter '{key}' must be provided at most once.", key);
        }

        value = values[0];
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"Query parameter '{key}' cannot be empty.", key);
        }

        return true;
    }

    private static int ParseQueryIntInRange(string value, string parameterName, int min, int max)
    {
        if (!int.TryParse(value, out var parsed))
        {
            throw new ArgumentException($"Query parameter '{parameterName}' must be an integer.", parameterName);
        }

        if (parsed < min || parsed > max)
        {
            throw new ArgumentException($"Query parameter '{parameterName}' must be between {min} and {max}.", parameterName);
        }

        return parsed;
    }

    private static bool ParseQueryBoolean(string value, string parameterName)
    {
        if (!bool.TryParse(value, out var parsed))
        {
            throw new ArgumentException($"Query parameter '{parameterName}' must be a boolean ('true' or 'false').", parameterName);
        }

        return parsed;
    }

    private static void AddRepeatedStringQueryParameter(
        JsonObject argumentsNode,
        IReadOnlyDictionary<string, StringValues> queryParameters,
        string queryKey,
        string argumentName)
    {
        if (!queryParameters.TryGetValue(queryKey, out var values))
        {
            return;
        }

        var items = new JsonArray();
        foreach (var value in values)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException($"Query parameter '{queryKey}' cannot contain empty values.", queryKey);
            }

            items.Add(value);
        }

        if (items.Count > 0)
        {
            argumentsNode[argumentName] = items;
        }
    }

    private static void AddRepeatedConsoleLevelQueryArgument(ref JsonObject? argumentsNode, IReadOnlyDictionary<string, StringValues> queryParameters)
    {
        if (!queryParameters.TryGetValue("level", out var values))
        {
            return;
        }

        var items = new JsonArray();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var value in values)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Query parameter 'level' cannot contain empty values.", "level");
            }

            var normalized = NormalizeConsoleLevel(value);
            if (seen.Add(normalized))
            {
                items.Add(normalized);
            }
        }

        if (items.Count > 0)
        {
            argumentsNode ??= new JsonObject();
            argumentsNode["levels"] = items;
        }
    }

    private static string NormalizeConsoleLevel(string value)
    {
        return value.Trim().ToLowerInvariant() switch
        {
            "info" => "info",
            "log" => "info",
            "warning" => "warning",
            "warn" => "warning",
            "error" => "error",
            "assert" => "assert",
            "exception" => "exception",
            _ => throw new ArgumentException(
                "Query parameter 'level' contains an unsupported value. Allowed values: info, warning, error, assert, exception.",
                "level")
        };
    }

    private static JsonObject CreateResourceInvalidParamsData(string? resourceUri, ArgumentException ex)
    {
        var data = new JsonObject
        {
            ["source"] = "resources/read"
        };

        if (!string.IsNullOrWhiteSpace(resourceUri))
        {
            data["resourceUri"] = resourceUri;
        }

        if (!string.IsNullOrWhiteSpace(ex.ParamName))
        {
            data["parameter"] = ex.ParamName;
        }

        return data;
    }

    private static JsonNode ProjectSelectionActiveResource(JsonNode selectionNode)
    {
        var selectionObject = selectionNode as JsonObject
            ?? throw new InvalidOperationException("Unity selection response was not an object.");

        if (!selectionObject.TryGetPropertyValue("activeObject", out var activeObjectNode))
        {
            throw new InvalidOperationException("Unity selection response did not include activeObject.");
        }

        return activeObjectNode?.DeepClone() ?? JsonValue.Create((string?)null)!;
    }

    private static JsonNode ProjectSelectionIndexResource(JsonNode selectionNode, string indexText)
    {
        if (!int.TryParse(indexText, out var index) || index < 0)
        {
            throw new ArgumentException("Selection index segment must be a non-negative integer.", "index");
        }

        var selectionObject = selectionNode as JsonObject
            ?? throw new InvalidOperationException("Unity selection response was not an object.");

        if (!selectionObject.TryGetPropertyValue("items", out var itemsNode) || itemsNode is not JsonArray itemsArray)
        {
            throw new InvalidOperationException("Unity selection response did not include items array.");
        }

        if (index >= itemsArray.Count)
        {
            throw new ArgumentException($"Selection index {index} is out of range for {itemsArray.Count} items.", "index");
        }

        var itemNode = itemsArray[index];
        return itemNode?.DeepClone() ?? JsonValue.Create((string?)null)!;
    }

    private async Task<JsonNode> CallUnityMethodResultAsync(string methodName, JsonNode? argumentsNode, CancellationToken cancellationToken)
    {
        var unityRequestId = $"mcp-{Interlocked.Increment(ref _nextUnityRequestId)}";
        var unityRequest = new JsonObject
        {
            ["jsonrpc"] = "2.0",
            ["id"] = unityRequestId,
            ["method"] = methodName
        };

        if (argumentsNode is not null)
        {
            unityRequest["params"] = argumentsNode.DeepClone();
        }

        var unityResponseJson = await _unityForwarder.ForwardAsync(
            unityRequest.ToJsonString(SerializerOptions),
            requestIdKey: $"s:{unityRequestId}",
            cancellationToken);

        if (!JsonRpcProtocol.TryParse(unityResponseJson, out var unityDocument, out var parseError))
        {
            throw new InvalidOperationException($"Unity returned invalid JSON: {parseError}");
        }

        using var parsedUnityDocument = unityDocument!;
        var root = parsedUnityDocument.RootElement;
        if (root.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException("Unity returned a non-object JSON-RPC payload.");
        }

        if (root.TryGetProperty("error", out var errorElement))
        {
            throw new InvalidOperationException($"Unity method '{methodName}' failed: {errorElement.GetRawText()}");
        }

        if (!root.TryGetProperty("result", out var resultElement))
        {
            throw new InvalidOperationException($"Unity method '{methodName}' response did not contain result.");
        }

        return JsonNode.Parse(resultElement.GetRawText()) ?? JsonValue.Create((string?)null)!;
    }

    private JsonObject CreateToolResultFromUnityResponse(string toolName, string unityResponseJson)
    {
        if (!JsonRpcProtocol.TryParse(unityResponseJson, out var unityDocument, out var parseError))
        {
            return CreateToolResult(
                isError: true,
                text: $"Unity returned invalid JSON: {parseError}",
                structuredContent: new JsonObject
                {
                    ["tool"] = toolName,
                    ["errorType"] = "invalid_unity_response"
                });
        }

        using var parsedUnityDocument = unityDocument!;
        var root = parsedUnityDocument.RootElement;
        if (root.ValueKind != JsonValueKind.Object)
        {
            return CreateToolResult(
                isError: true,
                text: "Unity returned a non-object JSON-RPC payload.",
                structuredContent: new JsonObject
                {
                    ["tool"] = toolName,
                    ["errorType"] = "invalid_unity_response"
                });
        }

        if (root.TryGetProperty("error", out var errorElement))
        {
            var errorNode = JsonNode.Parse(errorElement.GetRawText());
            return CreateToolResult(
                isError: true,
                text: $"Unity tool call failed: {errorElement.GetRawText()}",
                structuredContent: new JsonObject
                {
                    ["tool"] = toolName,
                    ["jsonRpcError"] = errorNode
                });
        }

        if (!root.TryGetProperty("result", out var resultElement))
        {
            return CreateToolResult(
                isError: true,
                text: "Unity response did not contain a result.",
                structuredContent: new JsonObject
                {
                    ["tool"] = toolName,
                    ["errorType"] = "missing_result"
                });
        }

        var resultNode = JsonNode.Parse(resultElement.GetRawText());
        JsonObject structuredContent;
        if (resultNode is JsonObject resultObject)
        {
            structuredContent = (JsonObject)resultObject.DeepClone();
        }
        else
        {
            structuredContent = new JsonObject
            {
                ["value"] = resultNode
            };
        }

        var text = resultNode?.ToJsonString(SerializerOptions) ?? "null";
        return CreateToolResult(isError: false, text: text, structuredContent: structuredContent);
    }

    private static JsonObject CreateToolResult(bool isError, string text, JsonObject? structuredContent)
    {
        var result = new JsonObject
        {
            ["content"] = new JsonArray
            {
                new JsonObject
                {
                    ["type"] = "text",
                    ["text"] = text
                }
            },
            ["isError"] = isError
        };

        if (structuredContent is not null)
        {
            result["structuredContent"] = structuredContent;
        }

        return result;
    }

    private static McpHttpResponse Json(int statusCode, string body)
    {
        return new McpHttpResponse(statusCode, body);
    }

    private static McpHttpResponse Accepted()
    {
        return new McpHttpResponse(StatusCodes.Status202Accepted, null);
    }
}
