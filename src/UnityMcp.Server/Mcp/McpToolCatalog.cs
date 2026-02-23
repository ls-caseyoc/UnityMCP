using System.Text.Json.Nodes;

namespace UnityMcp.Server.Mcp;

public sealed class McpToolCatalog
{
    private readonly Dictionary<string, McpToolDefinition> _byName;

    public McpToolCatalog()
    {
        var tools = new[]
        {
            new McpToolDefinition(
                "editor.getPlayModeState",
                "Returns the current Unity Editor play mode and compilation state.",
                EmptyObjectSchema()),
            new McpToolDefinition(
                "editor.getConsoleLogs",
                "Returns a bounded snapshot of recent Unity Editor console logs captured by UnityMCP.",
                new JsonObject
                {
                    ["type"] = "object",
                    ["additionalProperties"] = false,
                    ["properties"] = new JsonObject
                    {
                        ["maxResults"] = new JsonObject
                        {
                            ["type"] = "integer",
                            ["description"] = "Optional cap for returned log entries (1-500).",
                            ["minimum"] = 1,
                            ["maximum"] = 500
                        },
                        ["includeStackTrace"] = new JsonObject
                        {
                            ["type"] = "boolean",
                            ["description"] = "Whether to include stack traces in each entry (default false)."
                        },
                        ["contains"] = new JsonObject
                        {
                            ["type"] = "string",
                            ["description"] = "Optional case-insensitive substring filter applied to log messages."
                        },
                        ["levels"] = new JsonObject
                        {
                            ["type"] = "array",
                            ["description"] = "Optional log level filters (info, warning, error, assert, exception).",
                            ["items"] = new JsonObject
                            {
                                ["type"] = "string"
                            }
                        }
                    }
                }),
            new McpToolDefinition(
                "editor.consoleTail",
                "Returns log entries captured after a given sequence cursor.",
                new JsonObject
                {
                    ["type"] = "object",
                    ["additionalProperties"] = false,
                    ["required"] = new JsonArray("afterSequence"),
                    ["properties"] = new JsonObject
                    {
                        ["afterSequence"] = new JsonObject
                        {
                            ["type"] = "integer",
                            ["minimum"] = 0,
                            ["description"] = "Return entries with sequence greater than this cursor."
                        },
                        ["maxResults"] = new JsonObject
                        {
                            ["type"] = "integer",
                            ["description"] = "Optional cap for returned log entries (1-500).",
                            ["minimum"] = 1,
                            ["maximum"] = 500
                        },
                        ["includeStackTrace"] = new JsonObject
                        {
                            ["type"] = "boolean",
                            ["description"] = "Whether to include stack traces in each entry (default false)."
                        },
                        ["contains"] = new JsonObject
                        {
                            ["type"] = "string",
                            ["description"] = "Optional case-insensitive substring filter applied to log messages."
                        },
                        ["levels"] = new JsonObject
                        {
                            ["type"] = "array",
                            ["description"] = "Optional log level filters (info, warning, error, assert, exception).",
                            ["items"] = new JsonObject
                            {
                                ["type"] = "string"
                            }
                        }
                    }
                }),
            new McpToolDefinition(
                "editor.enterPlayMode",
                "Requests the Unity Editor to enter play mode.",
                EmptyObjectSchema()),
            new McpToolDefinition(
                "editor.exitPlayMode",
                "Requests the Unity Editor to exit play mode.",
                EmptyObjectSchema()),
            new McpToolDefinition(
                "scene.getActiveScene",
                "Returns metadata for the currently active Unity scene.",
                EmptyObjectSchema()),
            new McpToolDefinition(
                "scene.listOpenScenes",
                "Returns metadata for all currently open Unity scenes.",
                EmptyObjectSchema()),
            new McpToolDefinition(
                "scene.getSelection",
                "Returns metadata for the current Unity Editor selection.",
                EmptyObjectSchema()),
            new McpToolDefinition(
                "scene.selectObject",
                "Selects a single Unity object by instance id.",
                new JsonObject
                {
                    ["type"] = "object",
                    ["additionalProperties"] = false,
                    ["required"] = new JsonArray("instanceId"),
                    ["properties"] = new JsonObject
                    {
                        ["instanceId"] = new JsonObject
                        {
                            ["type"] = "integer",
                            ["description"] = "Unity instance id of the object to select."
                        }
                    }
                }),
            new McpToolDefinition(
                "scene.setSelection",
                "Replaces the Unity Editor selection with the specified object instance ids.",
                new JsonObject
                {
                    ["type"] = "object",
                    ["additionalProperties"] = false,
                    ["required"] = new JsonArray("instanceIds"),
                    ["properties"] = new JsonObject
                    {
                        ["instanceIds"] = new JsonObject
                        {
                            ["type"] = "array",
                            ["description"] = "Unity instance ids to set as the current selection (duplicates ignored).",
                            ["items"] = new JsonObject
                            {
                                ["type"] = "integer"
                            }
                        }
                    }
                }),
            new McpToolDefinition(
                "scene.createGameObject",
                "Creates a GameObject in the active scene and optionally sets its world position.",
                new JsonObject
                {
                    ["type"] = "object",
                    ["additionalProperties"] = false,
                    ["properties"] = new JsonObject
                    {
                        ["name"] = new JsonObject
                        {
                            ["type"] = "string",
                            ["description"] = "Optional GameObject name."
                        },
                        ["position"] = new JsonObject
                        {
                            ["type"] = "array",
                            ["description"] = "Optional world position [x,y,z].",
                            ["minItems"] = 3,
                            ["maxItems"] = 3,
                            ["items"] = new JsonObject
                            {
                                ["type"] = "number"
                            }
                        }
                    }
                }),
            new McpToolDefinition(
                "scene.findByTag",
                "Finds active GameObjects with the specified Unity tag.",
                new JsonObject
                {
                    ["type"] = "object",
                    ["additionalProperties"] = false,
                    ["required"] = new JsonArray("tag"),
                    ["properties"] = new JsonObject
                    {
                        ["tag"] = new JsonObject
                        {
                            ["type"] = "string",
                            ["description"] = "Unity tag to search for."
                        }
                    }
                }),
            new McpToolDefinition(
                "assets.find",
                "Searches Unity assets using AssetDatabase.FindAssets(query).",
                new JsonObject
                {
                    ["type"] = "object",
                    ["additionalProperties"] = false,
                    ["required"] = new JsonArray("query"),
                    ["properties"] = new JsonObject
                    {
                        ["query"] = new JsonObject
                        {
                            ["type"] = "string",
                            ["description"] = "Unity AssetDatabase.FindAssets query string."
                        },
                        ["maxResults"] = new JsonObject
                        {
                            ["type"] = "integer",
                            ["description"] = "Optional cap for returned results (1-500).",
                            ["minimum"] = 1,
                            ["maximum"] = 500
                        },
                        ["searchInFolders"] = new JsonObject
                        {
                            ["type"] = "array",
                            ["description"] = "Optional list of Unity folders under Assets/ used to scope the search.",
                            ["items"] = new JsonObject
                            {
                                ["type"] = "string"
                            }
                        },
                        ["types"] = new JsonObject
                        {
                            ["type"] = "array",
                            ["description"] = "Optional asset type filters appended as t:<type> tokens.",
                            ["items"] = new JsonObject
                            {
                                ["type"] = "string"
                            }
                        },
                        ["labels"] = new JsonObject
                        {
                            ["type"] = "array",
                            ["description"] = "Optional asset label filters appended as l:<label> tokens.",
                            ["items"] = new JsonObject
                            {
                                ["type"] = "string"
                            }
                        }
                    }
                }),
            new McpToolDefinition(
                "assets.import",
                "Imports an existing asset inside the Unity project's Assets folder.",
                new JsonObject
                {
                    ["type"] = "object",
                    ["additionalProperties"] = false,
                    ["required"] = new JsonArray("assetPath"),
                    ["properties"] = new JsonObject
                    {
                        ["assetPath"] = new JsonObject
                        {
                            ["type"] = "string",
                            ["description"] = "Project-relative asset path under Assets/."
                        }
                    }
                })
        };

        Tools = tools;
        _byName = tools.ToDictionary(tool => tool.Name, StringComparer.Ordinal);
    }

    public IReadOnlyList<McpToolDefinition> Tools { get; }

    public bool TryGet(string name, out McpToolDefinition definition)
    {
        return _byName.TryGetValue(name, out definition!);
    }

    private static JsonObject EmptyObjectSchema()
    {
        return new JsonObject
        {
            ["type"] = "object",
            ["additionalProperties"] = false
        };
    }
}

public sealed record McpToolDefinition(string Name, string Description, JsonObject InputSchema);
