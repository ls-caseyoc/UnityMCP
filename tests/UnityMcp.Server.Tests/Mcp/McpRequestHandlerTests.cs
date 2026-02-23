using Microsoft.Extensions.Logging.Abstractions;

namespace UnityMcp.Server.Tests.Mcp;

public sealed class McpRequestHandlerTests
{
    [Fact]
    public async Task HandlePostAsync_ReturnsInitializeResult_WhenRequestIsValid()
    {
        // Arrange
        var handler = CreateHandler((_, _, _) => throw new InvalidOperationException("Relay should not be called."));
        const string requestJson =
            """{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2025-06-18","capabilities":{},"clientInfo":{"name":"codex","version":"1.0"}}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(response.Body);

        using var document = JsonDocument.Parse(response.Body!);
        var result = document.RootElement.GetProperty("result");
        Assert.Equal("2025-06-18", result.GetProperty("protocolVersion").GetString());
        Assert.Equal("UnityMCP Server", result.GetProperty("serverInfo").GetProperty("name").GetString());
        Assert.True(result.GetProperty("capabilities").TryGetProperty("tools", out _));
    }

    [Fact]
    public async Task HandlePostAsync_ReturnsAccepted_WhenInitializedNotificationHasNoId()
    {
        // Arrange
        var handler = CreateHandler((_, _, _) => throw new InvalidOperationException("Relay should not be called."));
        const string requestJson = """{"jsonrpc":"2.0","method":"notifications/initialized","params":{}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(202, response.StatusCode);
        Assert.Null(response.Body);
    }

    [Fact]
    public async Task HandlePostAsync_ReturnsToolsList_WhenRequested()
    {
        // Arrange
        var handler = CreateHandler((_, _, _) => throw new InvalidOperationException("Relay should not be called."));
        const string requestJson = """{"jsonrpc":"2.0","id":"list-1","method":"tools/list"}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        using var document = JsonDocument.Parse(response.Body!);
        var tools = document.RootElement.GetProperty("result").GetProperty("tools");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "editor.getConsoleLogs");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "editor.consoleTail");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "scene.getActiveScene");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "scene.listOpenScenes");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "scene.getSelection");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "scene.selectObject");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "scene.setSelection");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "scene.createGameObject");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "assets.find");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "assets.import");
    }

    [Fact]
    public async Task HandlePostAsync_ReturnsPromptsList_WhenRequested()
    {
        // Arrange
        var handler = CreateHandler((_, _, _) => throw new InvalidOperationException("Relay should not be called."));
        const string requestJson = """{"jsonrpc":"2.0","id":"prompts-1","method":"prompts/list"}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        using var document = JsonDocument.Parse(response.Body!);
        var prompts = document.RootElement.GetProperty("result").GetProperty("prompts");
        Assert.Equal(JsonValueKind.Array, prompts.ValueKind);
        Assert.Empty(prompts.EnumerateArray());
    }

    [Fact]
    public async Task HandlePostAsync_ReturnsResourcesList_WhenRequested()
    {
        // Arrange
        var handler = CreateHandler((_, _, _) => throw new InvalidOperationException("Relay should not be called."));
        const string requestJson = """{"jsonrpc":"2.0","id":"res-1","method":"resources/list"}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        using var document = JsonDocument.Parse(response.Body!);
        var resources = document.RootElement.GetProperty("result").GetProperty("resources");
        Assert.Equal(JsonValueKind.Array, resources.ValueKind);
        Assert.Contains(resources.EnumerateArray(), item => item.GetProperty("uri").GetString() == "unitymcp://server/info");
        Assert.Contains(resources.EnumerateArray(), item => item.GetProperty("uri").GetString() == "unitymcp://editor/playmode-state");
        Assert.Contains(resources.EnumerateArray(), item => item.GetProperty("uri").GetString() == "unitymcp://editor/console-logs");
        Assert.Contains(resources.EnumerateArray(), item => item.GetProperty("uri").GetString() == "unitymcp://scene/active");
        Assert.Contains(resources.EnumerateArray(), item => item.GetProperty("uri").GetString() == "unitymcp://scene/open-scenes");
        Assert.Contains(resources.EnumerateArray(), item => item.GetProperty("uri").GetString() == "unitymcp://scene/selection");
        Assert.Contains(resources.EnumerateArray(), item => item.GetProperty("uri").GetString() == "unitymcp://scene/selection/active");
    }

    [Fact]
    public async Task HandlePostAsync_ReturnsResourceTemplatesList_WhenRequested()
    {
        // Arrange
        var handler = CreateHandler((_, _, _) => throw new InvalidOperationException("Relay should not be called."));
        const string requestJson = """{"jsonrpc":"2.0","id":"tpl-1","method":"resources/templates/list"}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        using var document = JsonDocument.Parse(response.Body!);
        var templates = document.RootElement.GetProperty("result").GetProperty("resourceTemplates");
        Assert.Equal(JsonValueKind.Array, templates.ValueKind);
        Assert.Contains(templates.EnumerateArray(), item => item.GetProperty("uriTemplate").GetString() == "unitymcp://scene/find-by-tag/{tag}");
        Assert.Contains(templates.EnumerateArray(), item => item.GetProperty("uriTemplate").GetString() == "unitymcp://assets/find/{query}");
        Assert.Contains(templates.EnumerateArray(), item => item.GetProperty("uriTemplate").GetString() == "unitymcp://editor/console-tail/{afterSequence}");
        Assert.Contains(templates.EnumerateArray(), item => item.GetProperty("uriTemplate").GetString() == "unitymcp://scene/selection/index/{index}");
    }

    [Fact]
    public async Task HandlePostAsync_ReturnsServerInfoResource_WhenResourceReadTargetsServerInfo()
    {
        // Arrange
        var handler = CreateHandler((_, _, _) => throw new InvalidOperationException("Relay should not be called."));
        const string requestJson = """{"jsonrpc":"2.0","id":"read-1","method":"resources/read","params":{"uri":"unitymcp://server/info"}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        using var document = JsonDocument.Parse(response.Body!);
        var contents = document.RootElement.GetProperty("result").GetProperty("contents");
        var item = contents[0];
        Assert.Equal("unitymcp://server/info", item.GetProperty("uri").GetString());
        Assert.Equal("application/json", item.GetProperty("mimeType").GetString());
        var text = item.GetProperty("text").GetString();
        Assert.Contains("UnityMCP Server", text);
    }

    [Fact]
    public async Task HandlePostAsync_ReturnsUnityConnectionResource_WhenResourceReadTargetsUnityConnection()
    {
        // Arrange
        var handler = CreateHandler(
            (_, _, _) => throw new InvalidOperationException("Relay should not be called."),
            isUnityConnected: true);
        const string requestJson = """{"jsonrpc":"2.0","id":"read-2","method":"resources/read","params":{"uri":"unitymcp://unity/connection"}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        using var document = JsonDocument.Parse(response.Body!);
        var text = document.RootElement.GetProperty("result").GetProperty("contents")[0].GetProperty("text").GetString();
        Assert.Contains(@"""connected"":true", text);
    }

    [Fact]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenResourceReadTargetsSceneFindByTagTemplate()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"tag":"Enemy","count":1,"items":[{"name":"EnemyA"}]}}""");
        });
        const string requestJson = """{"jsonrpc":"2.0","id":"read-3","method":"resources/read","params":{"uri":"unitymcp://scene/find-by-tag/Enemy"}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using (var forwarded = JsonDocument.Parse(forwardedRequestJson!))
        {
            Assert.Equal("scene.findByTag", forwarded.RootElement.GetProperty("method").GetString());
            Assert.Equal("Enemy", forwarded.RootElement.GetProperty("params").GetProperty("tag").GetString());
        }

        using var document = JsonDocument.Parse(response.Body!);
        var text = document.RootElement.GetProperty("result").GetProperty("contents")[0].GetProperty("text").GetString();
        Assert.Contains(@"""count"":1", text);
    }

    [Fact]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenResourceReadTargetsActiveScene()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"name":"TestScene","isActive":true}}""");
        });
        const string requestJson = """{"jsonrpc":"2.0","id":"read-4","method":"resources/read","params":{"uri":"unitymcp://scene/active"}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using (var forwarded = JsonDocument.Parse(forwardedRequestJson!))
        {
            Assert.Equal("scene.getActiveScene", forwarded.RootElement.GetProperty("method").GetString());
        }

        using var document = JsonDocument.Parse(response.Body!);
        var text = document.RootElement.GetProperty("result").GetProperty("contents")[0].GetProperty("text").GetString();
        Assert.Contains("TestScene", text);
    }

    [Fact]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenResourceReadTargetsEditorConsoleLogs()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"latestSequence":42,"returnedCount":1,"items":[{"sequence":42}]}}""");
        });
        const string requestJson = """{"jsonrpc":"2.0","id":"read-console-1","method":"resources/read","params":{"uri":"unitymcp://editor/console-logs"}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using (var forwarded = JsonDocument.Parse(forwardedRequestJson!))
        {
            Assert.Equal("editor.getConsoleLogs", forwarded.RootElement.GetProperty("method").GetString());
            Assert.False(forwarded.RootElement.TryGetProperty("params", out _));
        }

        using var document = JsonDocument.Parse(response.Body!);
        var text = document.RootElement.GetProperty("result").GetProperty("contents")[0].GetProperty("text").GetString();
        Assert.Contains(@"""latestSequence"":42", text);
    }

    [Fact]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenResourceReadTargetsEditorConsoleLogsWithQueryParameters()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"latestSequence":50,"returnedCount":2,"items":[{"sequence":49},{"sequence":50}]}}""");
        });
        const string requestJson =
            """{"jsonrpc":"2.0","id":"read-console-1b","method":"resources/read","params":{"uri":"unitymcp://editor/console-logs?maxResults=20&includeStackTrace=true"}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using (var forwarded = JsonDocument.Parse(forwardedRequestJson!))
        {
            Assert.Equal("editor.getConsoleLogs", forwarded.RootElement.GetProperty("method").GetString());
            var forwardedParams = forwarded.RootElement.GetProperty("params");
            Assert.Equal(20, forwardedParams.GetProperty("maxResults").GetInt32());
            Assert.True(forwardedParams.GetProperty("includeStackTrace").GetBoolean());
        }
    }

    [Fact]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenResourceReadTargetsEditorConsoleLogsWithLevelFilters()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"returnedCount":1,"items":[{"sequence":5,"level":"warning"}]}}""");
        });
        const string requestJson =
            """{"jsonrpc":"2.0","id":"read-console-1c","method":"resources/read","params":{"uri":"unitymcp://editor/console-logs?level=warning&level=error"}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using var forwarded = JsonDocument.Parse(forwardedRequestJson!);
        var levels = forwarded.RootElement.GetProperty("params").GetProperty("levels");
        Assert.Equal(2, levels.GetArrayLength());
        Assert.Equal("warning", levels[0].GetString());
        Assert.Equal("error", levels[1].GetString());
    }

    [Fact]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenResourceReadTargetsEditorConsoleTailTemplate()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"afterSequence":4,"nextAfterSequence":6,"returnedCount":2,"items":[{"sequence":5},{"sequence":6}]}}""");
        });
        const string requestJson = """{"jsonrpc":"2.0","id":"read-console-2","method":"resources/read","params":{"uri":"unitymcp://editor/console-tail/4"}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using (var forwarded = JsonDocument.Parse(forwardedRequestJson!))
        {
            Assert.Equal("editor.consoleTail", forwarded.RootElement.GetProperty("method").GetString());
            Assert.Equal(4, forwarded.RootElement.GetProperty("params").GetProperty("afterSequence").GetInt32());
        }

        using var document = JsonDocument.Parse(response.Body!);
        var text = document.RootElement.GetProperty("result").GetProperty("contents")[0].GetProperty("text").GetString();
        Assert.Contains(@"""nextAfterSequence"":6", text);
    }

    [Fact]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenResourceReadTargetsEditorConsoleLogsWithContainsFilter()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"returnedCount":1,"items":[{"sequence":9,"message":"MissingReferenceException"}]}}""");
        });
        const string requestJson =
            """{"jsonrpc":"2.0","id":"read-console-1d","method":"resources/read","params":{"uri":"unitymcp://editor/console-logs?contains=MissingReference"}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using var forwarded = JsonDocument.Parse(forwardedRequestJson!);
        Assert.Equal("MissingReference", forwarded.RootElement.GetProperty("params").GetProperty("contains").GetString());
    }

    [Fact]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenResourceReadTargetsEditorConsoleTailTemplateWithQueryParameters()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"afterSequence":125,"nextAfterSequence":127,"returnedCount":2,"items":[{"sequence":126},{"sequence":127}]}}""");
        });
        const string requestJson =
            """{"jsonrpc":"2.0","id":"read-console-2b","method":"resources/read","params":{"uri":"unitymcp://editor/console-tail/125?maxResults=10&includeStackTrace=false"}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using (var forwarded = JsonDocument.Parse(forwardedRequestJson!))
        {
            Assert.Equal("editor.consoleTail", forwarded.RootElement.GetProperty("method").GetString());
            var forwardedParams = forwarded.RootElement.GetProperty("params");
            Assert.Equal(125, forwardedParams.GetProperty("afterSequence").GetInt32());
            Assert.Equal(10, forwardedParams.GetProperty("maxResults").GetInt32());
            Assert.False(forwardedParams.GetProperty("includeStackTrace").GetBoolean());
        }
    }

    [Fact]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenResourceReadTargetsEditorConsoleTailTemplateWithLevelFilter()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"afterSequence":1,"nextAfterSequence":2,"returnedCount":1,"items":[{"sequence":2,"level":"error"}]}}""");
        });
        const string requestJson =
            """{"jsonrpc":"2.0","id":"read-console-2c","method":"resources/read","params":{"uri":"unitymcp://editor/console-tail/1?level=error"}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using var forwarded = JsonDocument.Parse(forwardedRequestJson!);
        var forwardedParams = forwarded.RootElement.GetProperty("params");
        Assert.Equal("editor.consoleTail", forwarded.RootElement.GetProperty("method").GetString());
        Assert.Equal("error", forwardedParams.GetProperty("levels")[0].GetString());
    }

    [Fact]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenResourceReadTargetsEditorConsoleTailTemplateWithContainsFilter()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"afterSequence":10,"nextAfterSequence":11,"returnedCount":1,"items":[{"sequence":11,"message":"NullReferenceException"}]}}""");
        });
        const string requestJson =
            """{"jsonrpc":"2.0","id":"read-console-2d","method":"resources/read","params":{"uri":"unitymcp://editor/console-tail/10?contains=NullReference"}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using var forwarded = JsonDocument.Parse(forwardedRequestJson!);
        var forwardedParams = forwarded.RootElement.GetProperty("params");
        Assert.Equal("editor.consoleTail", forwarded.RootElement.GetProperty("method").GetString());
        Assert.Equal("NullReference", forwardedParams.GetProperty("contains").GetString());
    }

    [Fact]
    public async Task HandlePostAsync_ReturnsStructuredInvalidParamsError_WhenConsoleTailCursorIsInvalid()
    {
        // Arrange
        var handler = CreateHandler((_, _, _) => throw new InvalidOperationException("Relay should not be called."));
        const string requestJson =
            """{"jsonrpc":"2.0","id":"read-console-invalid","method":"resources/read","params":{"uri":"unitymcp://editor/console-tail/not-a-number"}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        using var document = JsonDocument.Parse(response.Body!);
        var error = document.RootElement.GetProperty("error");
        Assert.Equal(-32602, error.GetProperty("code").GetInt32());
        Assert.Contains("afterSequence", error.GetProperty("message").GetString());
        var data = error.GetProperty("data");
        Assert.Equal("resources/read", data.GetProperty("source").GetString());
        Assert.Equal("unitymcp://editor/console-tail/not-a-number", data.GetProperty("resourceUri").GetString());
        Assert.Equal("afterSequence", data.GetProperty("parameter").GetString());
    }

    [Fact]
    public async Task HandlePostAsync_ReturnsStructuredInvalidParamsError_WhenConsoleQueryContainsDuplicateContains()
    {
        // Arrange
        var handler = CreateHandler((_, _, _) => throw new InvalidOperationException("Relay should not be called."));
        const string requestJson =
            """{"jsonrpc":"2.0","id":"read-console-invalid-dup-contains","method":"resources/read","params":{"uri":"unitymcp://editor/console-logs?contains=a&contains=b"}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        using var document = JsonDocument.Parse(response.Body!);
        var error = document.RootElement.GetProperty("error");
        Assert.Equal(-32602, error.GetProperty("code").GetInt32());
        Assert.Equal("contains", error.GetProperty("data").GetProperty("parameter").GetString());
    }

    [Fact]
    public async Task HandlePostAsync_ReturnsStructuredInvalidParamsError_WhenConsoleQueryContainsDuplicateMaxResults()
    {
        // Arrange
        var handler = CreateHandler((_, _, _) => throw new InvalidOperationException("Relay should not be called."));
        const string requestJson =
            """{"jsonrpc":"2.0","id":"read-console-invalid-dup","method":"resources/read","params":{"uri":"unitymcp://editor/console-logs?maxResults=10&maxResults=20"}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        using var document = JsonDocument.Parse(response.Body!);
        var error = document.RootElement.GetProperty("error");
        Assert.Equal(-32602, error.GetProperty("code").GetInt32());
        var data = error.GetProperty("data");
        Assert.Equal("maxResults", data.GetProperty("parameter").GetString());
    }

    [Fact]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenResourceReadTargetsOpenScenes()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"count":2,"items":[{"name":"A"},{"name":"B"}]}}""");
        });
        const string requestJson = """{"jsonrpc":"2.0","id":"read-5","method":"resources/read","params":{"uri":"unitymcp://scene/open-scenes"}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using (var forwarded = JsonDocument.Parse(forwardedRequestJson!))
        {
            Assert.Equal("scene.listOpenScenes", forwarded.RootElement.GetProperty("method").GetString());
        }

        using var document = JsonDocument.Parse(response.Body!);
        var text = document.RootElement.GetProperty("result").GetProperty("contents")[0].GetProperty("text").GetString();
        Assert.Contains(@"""count"":2", text);
    }

    [Fact]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenResourceReadTargetsSelection()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"count":1,"items":[{"name":"Cube"}]}}""");
        });
        const string requestJson = """{"jsonrpc":"2.0","id":"read-6","method":"resources/read","params":{"uri":"unitymcp://scene/selection"}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using (var forwarded = JsonDocument.Parse(forwardedRequestJson!))
        {
            Assert.Equal("scene.getSelection", forwarded.RootElement.GetProperty("method").GetString());
        }

        using var document = JsonDocument.Parse(response.Body!);
        var text = document.RootElement.GetProperty("result").GetProperty("contents")[0].GetProperty("text").GetString();
        Assert.Contains("Cube", text);
    }

    [Fact]
    public async Task HandlePostAsync_ProjectsActiveSelection_WhenResourceReadTargetsSelectionActive()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"count":1,"activeObject":{"name":"Cube","instanceId":1},"activeGameObject":{"name":"Cube"},"items":[{"name":"Cube","instanceId":1}]}}""");
        });
        const string requestJson = """{"jsonrpc":"2.0","id":"read-sel-active","method":"resources/read","params":{"uri":"unitymcp://scene/selection/active"}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using (var forwarded = JsonDocument.Parse(forwardedRequestJson!))
        {
            Assert.Equal("scene.getSelection", forwarded.RootElement.GetProperty("method").GetString());
        }

        using var document = JsonDocument.Parse(response.Body!);
        var text = document.RootElement.GetProperty("result").GetProperty("contents")[0].GetProperty("text").GetString();
        Assert.Contains(@"""name"":""Cube""", text);
        Assert.DoesNotContain(@"""items"":", text);
    }

    [Fact]
    public async Task HandlePostAsync_ReturnsNullPayload_WhenResourceReadTargetsSelectionActiveAndNoSelectionExists()
    {
        // Arrange
        var handler = CreateHandler((_, _, _) =>
            Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"count":0,"activeObject":null,"activeGameObject":null,"items":[]}}"""));
        const string requestJson = """{"jsonrpc":"2.0","id":"read-sel-active-null","method":"resources/read","params":{"uri":"unitymcp://scene/selection/active"}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        using var document = JsonDocument.Parse(response.Body!);
        var text = document.RootElement.GetProperty("result").GetProperty("contents")[0].GetProperty("text").GetString();
        Assert.Equal("null", text);
    }

    [Fact]
    public async Task HandlePostAsync_ProjectsSelectionItemByIndex_WhenResourceReadTargetsSelectionIndexTemplate()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"count":2,"activeObject":{"name":"Cube"},"items":[{"name":"Cube"},{"name":"Light"}]}}""");
        });
        const string requestJson = """{"jsonrpc":"2.0","id":"read-sel-index","method":"resources/read","params":{"uri":"unitymcp://scene/selection/index/1"}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using (var forwarded = JsonDocument.Parse(forwardedRequestJson!))
        {
            Assert.Equal("scene.getSelection", forwarded.RootElement.GetProperty("method").GetString());
        }

        using var document = JsonDocument.Parse(response.Body!);
        var text = document.RootElement.GetProperty("result").GetProperty("contents")[0].GetProperty("text").GetString();
        Assert.Contains("Light", text);
        Assert.DoesNotContain(@"""items"":", text);
    }

    [Fact]
    public async Task HandlePostAsync_ReturnsStructuredInvalidParamsError_WhenSelectionIndexIsOutOfRange()
    {
        // Arrange
        var handler = CreateHandler((_, _, _) =>
            Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"count":1,"activeObject":{"name":"Cube"},"items":[{"name":"Cube"}]}}"""));
        const string requestJson = """{"jsonrpc":"2.0","id":"read-sel-index-invalid","method":"resources/read","params":{"uri":"unitymcp://scene/selection/index/4"}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        using var document = JsonDocument.Parse(response.Body!);
        var error = document.RootElement.GetProperty("error");
        Assert.Equal(-32602, error.GetProperty("code").GetInt32());
        var data = error.GetProperty("data");
        Assert.Equal("index", data.GetProperty("parameter").GetString());
        Assert.Equal("unitymcp://scene/selection/index/4", data.GetProperty("resourceUri").GetString());
    }

    [Fact]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenResourceReadTargetsAssetsFindTemplate()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"query":"Player t:Prefab","totalMatched":1,"returnedCount":1,"items":[{"assetPath":"Assets/Prefabs/Player.prefab"}]}}""");
        });
        const string requestJson = """{"jsonrpc":"2.0","id":"read-7","method":"resources/read","params":{"uri":"unitymcp://assets/find/Player%20t%3APrefab"}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using (var forwarded = JsonDocument.Parse(forwardedRequestJson!))
        {
            Assert.Equal("assets.find", forwarded.RootElement.GetProperty("method").GetString());
            Assert.Equal("Player t:Prefab", forwarded.RootElement.GetProperty("params").GetProperty("query").GetString());
        }

        using var document = JsonDocument.Parse(response.Body!);
        var text = document.RootElement.GetProperty("result").GetProperty("contents")[0].GetProperty("text").GetString();
        Assert.Contains("Player.prefab", text);
    }

    [Fact]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenResourceReadTargetsAssetsFindTemplateWithQueryParameters()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"query":"Player","effectiveQuery":"Player t:Prefab l:Gameplay","returnedCount":0,"items":[]}}""");
        });
        const string requestJson =
            """{"jsonrpc":"2.0","id":"read-7b","method":"resources/read","params":{"uri":"unitymcp://assets/find/Player?maxResults=5&folder=Assets/Prefabs&type=Prefab&label=Gameplay"}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using (var forwarded = JsonDocument.Parse(forwardedRequestJson!))
        {
            Assert.Equal("assets.find", forwarded.RootElement.GetProperty("method").GetString());
            var forwardedParams = forwarded.RootElement.GetProperty("params");
            Assert.Equal("Player", forwardedParams.GetProperty("query").GetString());
            Assert.Equal(5, forwardedParams.GetProperty("maxResults").GetInt32());
            Assert.Equal("Assets/Prefabs", forwardedParams.GetProperty("searchInFolders")[0].GetString());
            Assert.Equal("Prefab", forwardedParams.GetProperty("types")[0].GetString());
            Assert.Equal("Gameplay", forwardedParams.GetProperty("labels")[0].GetString());
        }
    }

    [Fact]
    public async Task HandlePostAsync_ForwardsUnityRequestAndWrapsSuccess_WhenToolCallSucceeds()
    {
        // Arrange
        string? forwardedRequestJson = null;
        string? forwardedRequestIdKey = null;
        var handler = CreateHandler((requestJson, requestIdKey, _) =>
        {
            forwardedRequestJson = requestJson;
            forwardedRequestIdKey = requestIdKey;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"success":true,"instanceId":123,"name":"Enemy"}}""");
        });

        const string requestJson =
            """{"jsonrpc":"2.0","id":2,"method":"tools/call","params":{"name":"scene.createGameObject","arguments":{"name":"Enemy","position":[0,1,0]}}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        Assert.Equal("s:mcp-1", forwardedRequestIdKey);

        using (var forwarded = JsonDocument.Parse(forwardedRequestJson!))
        {
            Assert.Equal("scene.createGameObject", forwarded.RootElement.GetProperty("method").GetString());
            Assert.Equal("Enemy", forwarded.RootElement.GetProperty("params").GetProperty("name").GetString());
        }

        using var document = JsonDocument.Parse(response.Body!);
        var toolResult = document.RootElement.GetProperty("result");
        Assert.False(toolResult.GetProperty("isError").GetBoolean());
        Assert.Equal(123, toolResult.GetProperty("structuredContent").GetProperty("instanceId").GetInt32());
    }

    [Fact]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenToolCallTargetsSceneSelectObject()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"count":1,"activeObject":{"instanceId":45458,"name":"Cube"},"activeGameObject":{"instanceId":45458,"name":"Cube"},"items":[{"instanceId":45458,"name":"Cube"}]}}""");
        });

        const string requestJson =
            """{"jsonrpc":"2.0","id":"sel-1","method":"tools/call","params":{"name":"scene.selectObject","arguments":{"instanceId":45458}}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using (var forwarded = JsonDocument.Parse(forwardedRequestJson!))
        {
            Assert.Equal("scene.selectObject", forwarded.RootElement.GetProperty("method").GetString());
            Assert.Equal(45458, forwarded.RootElement.GetProperty("params").GetProperty("instanceId").GetInt32());
        }

        using var document = JsonDocument.Parse(response.Body!);
        Assert.False(document.RootElement.GetProperty("result").GetProperty("isError").GetBoolean());
    }

    [Fact]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenToolCallTargetsSceneSetSelection()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"count":2,"items":[{"instanceId":1},{"instanceId":2}]}}""");
        });

        const string requestJson =
            """{"jsonrpc":"2.0","id":"sel-2","method":"tools/call","params":{"name":"scene.setSelection","arguments":{"instanceIds":[1,2]}}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using (var forwarded = JsonDocument.Parse(forwardedRequestJson!))
        {
            Assert.Equal("scene.setSelection", forwarded.RootElement.GetProperty("method").GetString());
            var instanceIds = forwarded.RootElement.GetProperty("params").GetProperty("instanceIds");
            Assert.Equal(2, instanceIds.GetArrayLength());
            Assert.Equal(1, instanceIds[0].GetInt32());
            Assert.Equal(2, instanceIds[1].GetInt32());
        }

        using var document = JsonDocument.Parse(response.Body!);
        Assert.False(document.RootElement.GetProperty("result").GetProperty("isError").GetBoolean());
    }

    [Fact]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenToolCallTargetsEditorGetConsoleLogs()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"latestSequence":42,"returnedCount":1,"items":[{"sequence":42,"level":"warning","message":"Test warning"}]}}""");
        });

        const string requestJson =
            """{"jsonrpc":"2.0","id":"console-1","method":"tools/call","params":{"name":"editor.getConsoleLogs","arguments":{"maxResults":25,"includeStackTrace":false}}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);

        using (var forwarded = JsonDocument.Parse(forwardedRequestJson!))
        {
            Assert.Equal("editor.getConsoleLogs", forwarded.RootElement.GetProperty("method").GetString());
            Assert.Equal(25, forwarded.RootElement.GetProperty("params").GetProperty("maxResults").GetInt32());
            Assert.False(forwarded.RootElement.GetProperty("params").GetProperty("includeStackTrace").GetBoolean());
        }

        using var document = JsonDocument.Parse(response.Body!);
        var toolResult = document.RootElement.GetProperty("result");
        Assert.False(toolResult.GetProperty("isError").GetBoolean());
        Assert.Equal(42, toolResult.GetProperty("structuredContent").GetProperty("latestSequence").GetInt32());
    }

    [Fact]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenToolCallTargetsEditorConsoleTail()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"afterSequence":42,"nextAfterSequence":45,"returnedCount":3,"items":[{"sequence":43},{"sequence":44},{"sequence":45}]}}""");
        });

        const string requestJson =
            """{"jsonrpc":"2.0","id":"console-2","method":"tools/call","params":{"name":"editor.consoleTail","arguments":{"afterSequence":42,"maxResults":50}}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);

        using (var forwarded = JsonDocument.Parse(forwardedRequestJson!))
        {
            Assert.Equal("editor.consoleTail", forwarded.RootElement.GetProperty("method").GetString());
            Assert.Equal(42, forwarded.RootElement.GetProperty("params").GetProperty("afterSequence").GetInt32());
            Assert.Equal(50, forwarded.RootElement.GetProperty("params").GetProperty("maxResults").GetInt32());
        }

        using var document = JsonDocument.Parse(response.Body!);
        var toolResult = document.RootElement.GetProperty("result");
        Assert.False(toolResult.GetProperty("isError").GetBoolean());
        Assert.Equal(45, toolResult.GetProperty("structuredContent").GetProperty("nextAfterSequence").GetInt32());
    }

    [Fact]
    public async Task HandlePostAsync_ReturnsToolErrorResult_WhenUnityReturnsJsonRpcError()
    {
        // Arrange
        var handler = CreateHandler((_, _, _) =>
            Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","error":{"code":-32001,"message":"Unity is not connected."}}"""));

        const string requestJson =
            """{"jsonrpc":"2.0","id":3,"method":"tools/call","params":{"name":"editor.getPlayModeState","arguments":{}}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        using var document = JsonDocument.Parse(response.Body!);
        var toolResult = document.RootElement.GetProperty("result");
        Assert.True(toolResult.GetProperty("isError").GetBoolean());
        Assert.Equal(-32001, toolResult.GetProperty("structuredContent").GetProperty("jsonRpcError").GetProperty("code").GetInt32());
    }

    private static McpRequestHandler CreateHandler(
        Func<string, string, CancellationToken, Task<string>> forwardAsync,
        bool isUnityConnected = false)
    {
        return new McpRequestHandler(
            new McpToolCatalog(),
            new FakeUnityJsonRpcForwarder(forwardAsync),
            new FakeUnityConnectionStatusProvider(isUnityConnected),
            NullLogger<McpRequestHandler>.Instance);
    }

    private sealed class FakeUnityJsonRpcForwarder : IUnityJsonRpcForwarder
    {
        private readonly Func<string, string, CancellationToken, Task<string>> _forwardAsync;

        public FakeUnityJsonRpcForwarder(Func<string, string, CancellationToken, Task<string>> forwardAsync)
        {
            _forwardAsync = forwardAsync;
        }

        public Task<string> ForwardAsync(string requestJson, string requestIdKey, CancellationToken cancellationToken)
        {
            return _forwardAsync(requestJson, requestIdKey, cancellationToken);
        }
    }

    private sealed class FakeUnityConnectionStatusProvider : IUnityConnectionStatusProvider
    {
        public FakeUnityConnectionStatusProvider(bool isUnityConnected)
        {
            IsUnityConnected = isUnityConnected;
        }

        public bool IsUnityConnected { get; }
    }
}
