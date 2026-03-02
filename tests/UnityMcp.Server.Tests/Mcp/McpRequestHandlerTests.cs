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
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "scene.selectByPath");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "scene.findByPath");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "camera.getSettings");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "camera.setSettings");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "light.getSettings");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "light.setSettings");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "rigidbody.getSettings");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "rigidbody.setSettings");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "rigidbody2D.getSettings");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "rigidbody2D.setSettings");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "collider.getSettings");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "collider.setSettings");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "collider2D.getSettings");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "collider2D.setSettings");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "boxCollider.getSettings");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "boxCollider.setSettings");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "boxCollider2D.getSettings");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "boxCollider2D.setSettings");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "sphereCollider.getSettings");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "sphereCollider.setSettings");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "circleCollider2D.getSettings");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "circleCollider2D.setSettings");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "capsuleCollider.getSettings");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "capsuleCollider.setSettings");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "capsuleCollider2D.getSettings");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "capsuleCollider2D.setSettings");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "meshCollider.getSettings");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "meshCollider.setSettings");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "polygonCollider2D.getSettings");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "polygonCollider2D.setSettings");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "edgeCollider2D.getSettings");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "edgeCollider2D.setSettings");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "compositeCollider2D.getSettings");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "compositeCollider2D.setSettings");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "hingeJoint2D.getSettings");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "hingeJoint2D.setSettings");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "springJoint2D.getSettings");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "springJoint2D.setSettings");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "distanceJoint2D.getSettings");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "distanceJoint2D.setSettings");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "fixedJoint2D.getSettings");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "fixedJoint2D.setSettings");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "sliderJoint2D.getSettings");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "sliderJoint2D.setSettings");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "wheelJoint2D.getSettings");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "wheelJoint2D.setSettings");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "targetJoint2D.getSettings");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "targetJoint2D.setSettings");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "hingeJoint.getSettings");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "hingeJoint.setSettings");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "springJoint.getSettings");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "springJoint.setSettings");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "fixedJoint.getSettings");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "fixedJoint.setSettings");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "characterJoint.getSettings");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "characterJoint.setSettings");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "configurableJoint.getSettings");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "configurableJoint.setSettings");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "scene.getComponents");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "scene.destroyObject");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "scene.getComponentProperties");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "scene.setComponentProperties");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "scene.setTransform");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "scene.addComponent");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "scene.setSelection");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "scene.pingObject");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "scene.frameSelection");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "scene.frameObject");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "scene.createGameObject");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "scene.setParent");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "scene.duplicateObject");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "scene.renameObject");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "scene.setActive");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "prefab.instantiate");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "prefab.getSource");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "prefab.applyOverrides");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "prefab.revertOverrides");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "assets.find");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "assets.import");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "assets.ping");
        Assert.Contains(tools.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "assets.reveal");
    }

    [Fact]
    public async Task HandlePostAsync_ReturnsJointToolSchemas_WhenRequested()
    {
        // Arrange
        var handler = CreateHandler((_, _, _) => throw new InvalidOperationException("Relay should not be called."));
        const string requestJson = """{"jsonrpc":"2.0","id":"list-schema-1","method":"tools/list"}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        using var document = JsonDocument.Parse(response.Body!);
        var tools = document.RootElement.GetProperty("result").GetProperty("tools");

        var characterJointSetSettings = tools.EnumerateArray().First(tool => tool.GetProperty("name").GetString() == "characterJoint.setSettings");
        var characterJointProperties = characterJointSetSettings.GetProperty("inputSchema").GetProperty("properties");
        Assert.True(characterJointProperties.TryGetProperty("connectedAnchorMode", out var characterConnectedAnchorMode));
        Assert.Equal(JsonValueKind.Array, characterConnectedAnchorMode.GetProperty("enum").ValueKind);
        Assert.True(characterJointProperties.TryGetProperty("connectedBodyInstanceId", out var characterConnectedBodyInstanceId));
        var characterConnectedBodyTypes = characterConnectedBodyInstanceId.GetProperty("type").EnumerateArray().Select(item => item.GetString()).ToArray();
        Assert.Contains("integer", characterConnectedBodyTypes);
        Assert.Contains("null", characterConnectedBodyTypes);
        Assert.True(characterJointProperties.TryGetProperty("twistLimitSpring", out _));
        Assert.True(characterJointProperties.TryGetProperty("swing1Limit", out _));

        var configurableJointSetSettings = tools.EnumerateArray().First(tool => tool.GetProperty("name").GetString() == "configurableJoint.setSettings");
        var configurableProperties = configurableJointSetSettings.GetProperty("inputSchema").GetProperty("properties");
        Assert.True(configurableProperties.TryGetProperty("linearLimit", out var linearLimit));
        Assert.Equal("object", linearLimit.GetProperty("type").GetString());
        Assert.True(configurableProperties.TryGetProperty("xDrive", out var xDrive));
        Assert.Equal("object", xDrive.GetProperty("type").GetString());
        Assert.True(configurableProperties.TryGetProperty("projectionMode", out _));
        Assert.True(configurableProperties.TryGetProperty("connectedAnchorMode", out _));
    }

    [Fact]
    public async Task HandlePostAsync_ReturnsHierarchyAndPrefabToolSchemas_WhenRequested()
    {
        // Arrange
        var handler = CreateHandler((_, _, _) => throw new InvalidOperationException("Relay should not be called."));
        const string requestJson = """{"jsonrpc":"2.0","id":"list-schema-2","method":"tools/list"}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        using var document = JsonDocument.Parse(response.Body!);
        var tools = document.RootElement.GetProperty("result").GetProperty("tools");

        var setParent = tools.EnumerateArray().First(tool => tool.GetProperty("name").GetString() == "scene.setParent");
        var setParentProperties = setParent.GetProperty("inputSchema").GetProperty("properties");
        var parentInstanceIdTypes = setParentProperties.GetProperty("parentInstanceId").GetProperty("type").EnumerateArray().Select(item => item.GetString()).ToArray();
        Assert.Contains("integer", parentInstanceIdTypes);
        Assert.Contains("null", parentInstanceIdTypes);
        Assert.Equal("boolean", setParentProperties.GetProperty("keepWorldTransform").GetProperty("type").GetString());

        var instantiatePrefab = tools.EnumerateArray().First(tool => tool.GetProperty("name").GetString() == "prefab.instantiate");
        var instantiateProperties = instantiatePrefab.GetProperty("inputSchema").GetProperty("properties");
        Assert.Equal("string", instantiateProperties.GetProperty("assetPath").GetProperty("type").GetString());
        Assert.Equal("boolean", instantiateProperties.GetProperty("select").GetProperty("type").GetString());
        Assert.Equal(3, instantiateProperties.GetProperty("rotationEuler").GetProperty("minItems").GetInt32());

        var applyOverrides = tools.EnumerateArray().First(tool => tool.GetProperty("name").GetString() == "prefab.applyOverrides");
        var applyProperties = applyOverrides.GetProperty("inputSchema").GetProperty("properties");
        var scopeValues = applyProperties.GetProperty("scope").GetProperty("enum").EnumerateArray().Select(item => item.GetString()).ToArray();
        Assert.Equal(new[] { "instanceRoot", "object", "component" }, scopeValues);
        Assert.Equal("integer", applyProperties.GetProperty("componentInstanceId").GetProperty("type").GetString());
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
    public async Task HandlePostAsync_ForwardsPingAndFocus_WhenToolCallTargetsSceneSelectObject()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"count":1,"items":[{"instanceId":45458}]}}""");
        });

        const string requestJson =
            """{"jsonrpc":"2.0","id":"sel-1b","method":"tools/call","params":{"name":"scene.selectObject","arguments":{"instanceId":45458,"ping":true,"focus":true}}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using var forwarded = JsonDocument.Parse(forwardedRequestJson!);
        var forwardedParams = forwarded.RootElement.GetProperty("params");
        Assert.Equal("scene.selectObject", forwarded.RootElement.GetProperty("method").GetString());
        Assert.Equal(45458, forwardedParams.GetProperty("instanceId").GetInt32());
        Assert.True(forwardedParams.GetProperty("ping").GetBoolean());
        Assert.True(forwardedParams.GetProperty("focus").GetBoolean());
    }

    [Fact]
    public async Task HandlePostAsync_ForwardsPathAndPresentationFlags_WhenToolCallTargetsSceneSelectByPath()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"count":1,"items":[{"instanceId":45444}]}}""");
        });

        const string requestJson =
            """{"jsonrpc":"2.0","id":"sel-path-1","method":"tools/call","params":{"name":"scene.selectByPath","arguments":{"path":"Cube/Main Camera","ping":true,"focus":true}}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using var forwarded = JsonDocument.Parse(forwardedRequestJson!);
        var forwardedParams = forwarded.RootElement.GetProperty("params");
        Assert.Equal("scene.selectByPath", forwarded.RootElement.GetProperty("method").GetString());
        Assert.Equal("Cube/Main Camera", forwardedParams.GetProperty("path").GetString());
        Assert.True(forwardedParams.GetProperty("ping").GetBoolean());
        Assert.True(forwardedParams.GetProperty("focus").GetBoolean());
    }

    [Fact]
    public async Task HandlePostAsync_ForwardsScenePathDisambiguation_WhenToolCallTargetsSceneSelectByPath()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"count":1,"items":[{"instanceId":45444}]}}""");
        });

        const string requestJson =
            """{"jsonrpc":"2.0","id":"sel-path-2","method":"tools/call","params":{"name":"scene.selectByPath","arguments":{"path":"Cube/Main Camera","scenePath":"Assets/_Game/Scenes/TestScene.unity"}}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using var forwarded = JsonDocument.Parse(forwardedRequestJson!);
        var forwardedParams = forwarded.RootElement.GetProperty("params");
        Assert.Equal("scene.selectByPath", forwarded.RootElement.GetProperty("method").GetString());
        Assert.Equal("Cube/Main Camera", forwardedParams.GetProperty("path").GetString());
        Assert.Equal("Assets/_Game/Scenes/TestScene.unity", forwardedParams.GetProperty("scenePath").GetString());
    }

    [Fact]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenToolCallTargetsSceneFindByPath()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"path":"Cube/Main Camera","count":1,"items":[{"instanceId":45444}]}}""");
        });

        const string requestJson =
            """{"jsonrpc":"2.0","id":"find-path-1","method":"tools/call","params":{"name":"scene.findByPath","arguments":{"path":"Cube/Main Camera","scenePath":"Assets/_Game/Scenes/TestScene.unity"}}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using (var forwarded = JsonDocument.Parse(forwardedRequestJson!))
        {
            Assert.Equal("scene.findByPath", forwarded.RootElement.GetProperty("method").GetString());
            Assert.Equal("Cube/Main Camera", forwarded.RootElement.GetProperty("params").GetProperty("path").GetString());
            Assert.Equal("Assets/_Game/Scenes/TestScene.unity", forwarded.RootElement.GetProperty("params").GetProperty("scenePath").GetString());
        }

        using var document = JsonDocument.Parse(response.Body!);
        Assert.False(document.RootElement.GetProperty("result").GetProperty("isError").GetBoolean());
    }

    [Fact]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenToolCallTargetsCameraGetSettings()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"settings":{"fieldOfView":60}}}""");
        });

        const string requestJson =
            """{"jsonrpc":"2.0","id":"camera-get-1","method":"tools/call","params":{"name":"camera.getSettings","arguments":{"instanceId":45444}}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using (var forwarded = JsonDocument.Parse(forwardedRequestJson!))
        {
            Assert.Equal("camera.getSettings", forwarded.RootElement.GetProperty("method").GetString());
            Assert.Equal(45444, forwarded.RootElement.GetProperty("params").GetProperty("instanceId").GetInt32());
        }

        using var document = JsonDocument.Parse(response.Body!);
        Assert.False(document.RootElement.GetProperty("result").GetProperty("isError").GetBoolean());
    }

    [Fact]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenToolCallTargetsCameraSetSettings()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"applied":{"fieldOfView":true}}}""");
        });

        const string requestJson =
            """{"jsonrpc":"2.0","id":"camera-set-1","method":"tools/call","params":{"name":"camera.setSettings","arguments":{"instanceId":45444,"fieldOfView":55,"nearClipPlane":0.2}}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using (var forwarded = JsonDocument.Parse(forwardedRequestJson!))
        {
            Assert.Equal("camera.setSettings", forwarded.RootElement.GetProperty("method").GetString());
            var forwardedParams = forwarded.RootElement.GetProperty("params");
            Assert.Equal(45444, forwardedParams.GetProperty("instanceId").GetInt32());
            Assert.Equal(55, forwardedParams.GetProperty("fieldOfView").GetInt32());
            Assert.Equal(0.2, forwardedParams.GetProperty("nearClipPlane").GetDouble(), 3);
        }

        using var document = JsonDocument.Parse(response.Body!);
        Assert.False(document.RootElement.GetProperty("result").GetProperty("isError").GetBoolean());
    }

    [Fact]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenToolCallTargetsLightGetSettings()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"settings":{"intensity":1.0}}}""");
        });

        const string requestJson =
            """{"jsonrpc":"2.0","id":"light-get-1","method":"tools/call","params":{"name":"light.getSettings","arguments":{"instanceId":45444}}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using (var forwarded = JsonDocument.Parse(forwardedRequestJson!))
        {
            Assert.Equal("light.getSettings", forwarded.RootElement.GetProperty("method").GetString());
            Assert.Equal(45444, forwarded.RootElement.GetProperty("params").GetProperty("instanceId").GetInt32());
        }

        using var document = JsonDocument.Parse(response.Body!);
        Assert.False(document.RootElement.GetProperty("result").GetProperty("isError").GetBoolean());
    }

    [Fact]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenToolCallTargetsLightSetSettings()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"applied":{"intensity":true}}}""");
        });

        const string requestJson =
            """{"jsonrpc":"2.0","id":"light-set-1","method":"tools/call","params":{"name":"light.setSettings","arguments":{"instanceId":45444,"intensity":2.5,"range":15}}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using (var forwarded = JsonDocument.Parse(forwardedRequestJson!))
        {
            Assert.Equal("light.setSettings", forwarded.RootElement.GetProperty("method").GetString());
            var forwardedParams = forwarded.RootElement.GetProperty("params");
            Assert.Equal(45444, forwardedParams.GetProperty("instanceId").GetInt32());
            Assert.Equal(2.5, forwardedParams.GetProperty("intensity").GetDouble(), 3);
            Assert.Equal(15, forwardedParams.GetProperty("range").GetInt32());
        }

        using var document = JsonDocument.Parse(response.Body!);
        Assert.False(document.RootElement.GetProperty("result").GetProperty("isError").GetBoolean());
    }

    [Fact]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenToolCallTargetsRigidbodyGetSettings()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"settings":{"mass":1.0}}}""");
        });

        const string requestJson =
            """{"jsonrpc":"2.0","id":"rb-get-1","method":"tools/call","params":{"name":"rigidbody.getSettings","arguments":{"instanceId":45444}}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using (var forwarded = JsonDocument.Parse(forwardedRequestJson!))
        {
            Assert.Equal("rigidbody.getSettings", forwarded.RootElement.GetProperty("method").GetString());
            Assert.Equal(45444, forwarded.RootElement.GetProperty("params").GetProperty("instanceId").GetInt32());
        }

        using var document = JsonDocument.Parse(response.Body!);
        Assert.False(document.RootElement.GetProperty("result").GetProperty("isError").GetBoolean());
    }

    [Fact]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenToolCallTargetsRigidbodySetSettings()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"applied":{"isKinematic":true}}}""");
        });

        const string requestJson =
            """{"jsonrpc":"2.0","id":"rb-set-1","method":"tools/call","params":{"name":"rigidbody.setSettings","arguments":{"instanceId":45444,"isKinematic":true,"useGravity":false}}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using (var forwarded = JsonDocument.Parse(forwardedRequestJson!))
        {
            Assert.Equal("rigidbody.setSettings", forwarded.RootElement.GetProperty("method").GetString());
            var forwardedParams = forwarded.RootElement.GetProperty("params");
            Assert.Equal(45444, forwardedParams.GetProperty("instanceId").GetInt32());
            Assert.True(forwardedParams.GetProperty("isKinematic").GetBoolean());
            Assert.False(forwardedParams.GetProperty("useGravity").GetBoolean());
        }

        using var document = JsonDocument.Parse(response.Body!);
        Assert.False(document.RootElement.GetProperty("result").GetProperty("isError").GetBoolean());
    }

    [Fact]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenToolCallTargetsColliderGetSettings()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"settings":{"isTrigger":false}}}""");
        });

        const string requestJson =
            """{"jsonrpc":"2.0","id":"col-get-1","method":"tools/call","params":{"name":"collider.getSettings","arguments":{"instanceId":45444}}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using (var forwarded = JsonDocument.Parse(forwardedRequestJson!))
        {
            Assert.Equal("collider.getSettings", forwarded.RootElement.GetProperty("method").GetString());
            Assert.Equal(45444, forwarded.RootElement.GetProperty("params").GetProperty("instanceId").GetInt32());
        }

        using var document = JsonDocument.Parse(response.Body!);
        Assert.False(document.RootElement.GetProperty("result").GetProperty("isError").GetBoolean());
    }

    [Fact]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenToolCallTargetsColliderSetSettings()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"applied":{"isTrigger":true}}}""");
        });

        const string requestJson =
            """{"jsonrpc":"2.0","id":"col-set-1","method":"tools/call","params":{"name":"collider.setSettings","arguments":{"instanceId":45444,"isTrigger":true}}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using (var forwarded = JsonDocument.Parse(forwardedRequestJson!))
        {
            Assert.Equal("collider.setSettings", forwarded.RootElement.GetProperty("method").GetString());
            var forwardedParams = forwarded.RootElement.GetProperty("params");
            Assert.Equal(45444, forwardedParams.GetProperty("instanceId").GetInt32());
            Assert.True(forwardedParams.GetProperty("isTrigger").GetBoolean());
        }

        using var document = JsonDocument.Parse(response.Body!);
        Assert.False(document.RootElement.GetProperty("result").GetProperty("isError").GetBoolean());
    }

    [Fact]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenToolCallTargetsBoxColliderGetSettings()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"settings":{"subtype":{"kind":"BoxCollider"}}}}""");
        });

        const string requestJson =
            """{"jsonrpc":"2.0","id":"box-get-1","method":"tools/call","params":{"name":"boxCollider.getSettings","arguments":{"instanceId":45444}}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using (var forwarded = JsonDocument.Parse(forwardedRequestJson!))
        {
            Assert.Equal("boxCollider.getSettings", forwarded.RootElement.GetProperty("method").GetString());
            Assert.Equal(45444, forwarded.RootElement.GetProperty("params").GetProperty("instanceId").GetInt32());
        }

        using var document = JsonDocument.Parse(response.Body!);
        Assert.False(document.RootElement.GetProperty("result").GetProperty("isError").GetBoolean());
    }

    [Fact]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenToolCallTargetsBoxColliderSetSettings()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"applied":{"center":true,"size":true}}}""");
        });

        const string requestJson =
            """{"jsonrpc":"2.0","id":"box-set-1","method":"tools/call","params":{"name":"boxCollider.setSettings","arguments":{"instanceId":45444,"center":[0,0.5,0],"size":[1,2,1]}}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using (var forwarded = JsonDocument.Parse(forwardedRequestJson!))
        {
            Assert.Equal("boxCollider.setSettings", forwarded.RootElement.GetProperty("method").GetString());
            var forwardedParams = forwarded.RootElement.GetProperty("params");
            Assert.Equal(45444, forwardedParams.GetProperty("instanceId").GetInt32());
            Assert.Equal(3, forwardedParams.GetProperty("center").GetArrayLength());
            Assert.Equal(3, forwardedParams.GetProperty("size").GetArrayLength());
        }

        using var document = JsonDocument.Parse(response.Body!);
        Assert.False(document.RootElement.GetProperty("result").GetProperty("isError").GetBoolean());
    }

    [Fact]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenToolCallTargetsSphereColliderGetSettings()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"settings":{"subtype":{"kind":"SphereCollider"}}}}""");
        });

        const string requestJson =
            """{"jsonrpc":"2.0","id":"sphere-get-1","method":"tools/call","params":{"name":"sphereCollider.getSettings","arguments":{"instanceId":45444}}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using (var forwarded = JsonDocument.Parse(forwardedRequestJson!))
        {
            Assert.Equal("sphereCollider.getSettings", forwarded.RootElement.GetProperty("method").GetString());
            Assert.Equal(45444, forwarded.RootElement.GetProperty("params").GetProperty("instanceId").GetInt32());
        }

        using var document = JsonDocument.Parse(response.Body!);
        Assert.False(document.RootElement.GetProperty("result").GetProperty("isError").GetBoolean());
    }

    [Fact]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenToolCallTargetsSphereColliderSetSettings()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"applied":{"radius":true}}}""");
        });

        const string requestJson =
            """{"jsonrpc":"2.0","id":"sphere-set-1","method":"tools/call","params":{"name":"sphereCollider.setSettings","arguments":{"instanceId":45444,"radius":1.5}}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using (var forwarded = JsonDocument.Parse(forwardedRequestJson!))
        {
            Assert.Equal("sphereCollider.setSettings", forwarded.RootElement.GetProperty("method").GetString());
            var forwardedParams = forwarded.RootElement.GetProperty("params");
            Assert.Equal(45444, forwardedParams.GetProperty("instanceId").GetInt32());
            Assert.Equal(1.5, forwardedParams.GetProperty("radius").GetDouble(), 3);
        }

        using var document = JsonDocument.Parse(response.Body!);
        Assert.False(document.RootElement.GetProperty("result").GetProperty("isError").GetBoolean());
    }

    [Fact]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenToolCallTargetsCapsuleColliderGetSettings()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"settings":{"subtype":{"kind":"CapsuleCollider"}}}}""");
        });

        const string requestJson =
            """{"jsonrpc":"2.0","id":"capsule-get-1","method":"tools/call","params":{"name":"capsuleCollider.getSettings","arguments":{"instanceId":45444}}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using (var forwarded = JsonDocument.Parse(forwardedRequestJson!))
        {
            Assert.Equal("capsuleCollider.getSettings", forwarded.RootElement.GetProperty("method").GetString());
            Assert.Equal(45444, forwarded.RootElement.GetProperty("params").GetProperty("instanceId").GetInt32());
        }

        using var document = JsonDocument.Parse(response.Body!);
        Assert.False(document.RootElement.GetProperty("result").GetProperty("isError").GetBoolean());
    }

    [Fact]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenToolCallTargetsCapsuleColliderSetSettings()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"applied":{"radius":true,"height":true,"direction":true}}}""");
        });

        const string requestJson =
            """{"jsonrpc":"2.0","id":"capsule-set-1","method":"tools/call","params":{"name":"capsuleCollider.setSettings","arguments":{"instanceId":45444,"radius":0.75,"height":2.0,"direction":"Z"}}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using (var forwarded = JsonDocument.Parse(forwardedRequestJson!))
        {
            Assert.Equal("capsuleCollider.setSettings", forwarded.RootElement.GetProperty("method").GetString());
            var forwardedParams = forwarded.RootElement.GetProperty("params");
            Assert.Equal(45444, forwardedParams.GetProperty("instanceId").GetInt32());
            Assert.Equal(0.75, forwardedParams.GetProperty("radius").GetDouble(), 3);
            Assert.Equal(2.0, forwardedParams.GetProperty("height").GetDouble(), 3);
            Assert.Equal("Z", forwardedParams.GetProperty("direction").GetString());
        }

        using var document = JsonDocument.Parse(response.Body!);
        Assert.False(document.RootElement.GetProperty("result").GetProperty("isError").GetBoolean());
    }

    [Fact]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenToolCallTargetsMeshColliderGetSettings()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"settings":{"subtype":{"kind":"MeshCollider"}}}}""");
        });

        const string requestJson =
            """{"jsonrpc":"2.0","id":"mesh-get-1","method":"tools/call","params":{"name":"meshCollider.getSettings","arguments":{"instanceId":45444}}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using (var forwarded = JsonDocument.Parse(forwardedRequestJson!))
        {
            Assert.Equal("meshCollider.getSettings", forwarded.RootElement.GetProperty("method").GetString());
            Assert.Equal(45444, forwarded.RootElement.GetProperty("params").GetProperty("instanceId").GetInt32());
        }

        using var document = JsonDocument.Parse(response.Body!);
        Assert.False(document.RootElement.GetProperty("result").GetProperty("isError").GetBoolean());
    }

    [Fact]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenToolCallTargetsMeshColliderSetSettings()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"applied":{"convex":true}}}""");
        });

        const string requestJson =
            """{"jsonrpc":"2.0","id":"mesh-set-1","method":"tools/call","params":{"name":"meshCollider.setSettings","arguments":{"instanceId":45444,"convex":true}}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using (var forwarded = JsonDocument.Parse(forwardedRequestJson!))
        {
            Assert.Equal("meshCollider.setSettings", forwarded.RootElement.GetProperty("method").GetString());
            var forwardedParams = forwarded.RootElement.GetProperty("params");
            Assert.Equal(45444, forwardedParams.GetProperty("instanceId").GetInt32());
            Assert.True(forwardedParams.GetProperty("convex").GetBoolean());
        }

        using var document = JsonDocument.Parse(response.Body!);
        Assert.False(document.RootElement.GetProperty("result").GetProperty("isError").GetBoolean());
    }

    [Theory]
    [InlineData("""{"jsonrpc":"2.0","id":"rb2d-get-1","method":"tools/call","params":{"name":"rigidbody2D.getSettings","arguments":{"instanceId":45444}}}""", "rigidbody2D.getSettings")]
    [InlineData("""{"jsonrpc":"2.0","id":"col2d-get-1","method":"tools/call","params":{"name":"collider2D.getSettings","arguments":{"instanceId":45444}}}""", "collider2D.getSettings")]
    [InlineData("""{"jsonrpc":"2.0","id":"box2d-get-1","method":"tools/call","params":{"name":"boxCollider2D.getSettings","arguments":{"instanceId":45444}}}""", "boxCollider2D.getSettings")]
    [InlineData("""{"jsonrpc":"2.0","id":"circle2d-get-1","method":"tools/call","params":{"name":"circleCollider2D.getSettings","arguments":{"instanceId":45444}}}""", "circleCollider2D.getSettings")]
    [InlineData("""{"jsonrpc":"2.0","id":"capsule2d-get-1","method":"tools/call","params":{"name":"capsuleCollider2D.getSettings","arguments":{"instanceId":45444}}}""", "capsuleCollider2D.getSettings")]
    [InlineData("""{"jsonrpc":"2.0","id":"poly2d-get-1","method":"tools/call","params":{"name":"polygonCollider2D.getSettings","arguments":{"instanceId":45444}}}""", "polygonCollider2D.getSettings")]
    [InlineData("""{"jsonrpc":"2.0","id":"edge2d-get-1","method":"tools/call","params":{"name":"edgeCollider2D.getSettings","arguments":{"instanceId":45444}}}""", "edgeCollider2D.getSettings")]
    [InlineData("""{"jsonrpc":"2.0","id":"comp2d-get-1","method":"tools/call","params":{"name":"compositeCollider2D.getSettings","arguments":{"instanceId":45444}}}""", "compositeCollider2D.getSettings")]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenToolCallTargets2DPhysicsGetSettings(string requestJson, string expectedMethod)
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJsonValue, _, _) =>
        {
            forwardedRequestJson = requestJsonValue;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"settings":{"ok":true}}}""");
        });

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using (var forwarded = JsonDocument.Parse(forwardedRequestJson!))
        {
            Assert.Equal(expectedMethod, forwarded.RootElement.GetProperty("method").GetString());
            Assert.Equal(45444, forwarded.RootElement.GetProperty("params").GetProperty("instanceId").GetInt32());
        }

        using var document = JsonDocument.Parse(response.Body!);
        Assert.False(document.RootElement.GetProperty("result").GetProperty("isError").GetBoolean());
    }

    [Theory]
    [InlineData("""{"jsonrpc":"2.0","id":"rb2d-set-1","method":"tools/call","params":{"name":"rigidbody2D.setSettings","arguments":{"instanceId":45444,"simulated":true}}}""", "rigidbody2D.setSettings")]
    [InlineData("""{"jsonrpc":"2.0","id":"col2d-set-1","method":"tools/call","params":{"name":"collider2D.setSettings","arguments":{"instanceId":45444,"isTrigger":true}}}""", "collider2D.setSettings")]
    [InlineData("""{"jsonrpc":"2.0","id":"box2d-set-1","method":"tools/call","params":{"name":"boxCollider2D.setSettings","arguments":{"instanceId":45444,"edgeRadius":0.1}}}""", "boxCollider2D.setSettings")]
    [InlineData("""{"jsonrpc":"2.0","id":"circle2d-set-1","method":"tools/call","params":{"name":"circleCollider2D.setSettings","arguments":{"instanceId":45444,"radius":1.25}}}""", "circleCollider2D.setSettings")]
    [InlineData("""{"jsonrpc":"2.0","id":"capsule2d-set-1","method":"tools/call","params":{"name":"capsuleCollider2D.setSettings","arguments":{"instanceId":45444,"direction":"Vertical"}}}""", "capsuleCollider2D.setSettings")]
    [InlineData("""{"jsonrpc":"2.0","id":"poly2d-set-1","method":"tools/call","params":{"name":"polygonCollider2D.setSettings","arguments":{"instanceId":45444,"usedByEffector":false}}}""", "polygonCollider2D.setSettings")]
    [InlineData("""{"jsonrpc":"2.0","id":"edge2d-set-1","method":"tools/call","params":{"name":"edgeCollider2D.setSettings","arguments":{"instanceId":45444,"edgeRadius":0.05}}}""", "edgeCollider2D.setSettings")]
    [InlineData("""{"jsonrpc":"2.0","id":"comp2d-set-1","method":"tools/call","params":{"name":"compositeCollider2D.setSettings","arguments":{"instanceId":45444,"geometryType":"Polygons"}}}""", "compositeCollider2D.setSettings")]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenToolCallTargets2DPhysicsSetSettings(string requestJson, string expectedMethod)
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJsonValue, _, _) =>
        {
            forwardedRequestJson = requestJsonValue;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"applied":{"ok":true}}}""");
        });

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using (var forwarded = JsonDocument.Parse(forwardedRequestJson!))
        {
            Assert.Equal(expectedMethod, forwarded.RootElement.GetProperty("method").GetString());
            Assert.Equal(45444, forwarded.RootElement.GetProperty("params").GetProperty("instanceId").GetInt32());
        }

        using var document = JsonDocument.Parse(response.Body!);
        Assert.False(document.RootElement.GetProperty("result").GetProperty("isError").GetBoolean());
    }

    [Fact]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenToolCallTargetsBoxCollider2DSetSettings_WithVector2Fields()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"applied":{"size":true,"offset":true,"edgeRadius":true}}}""");
        });

        const string requestJson =
            """{"jsonrpc":"2.0","id":"box2d-set-detailed-1","method":"tools/call","params":{"name":"boxCollider2D.setSettings","arguments":{"instanceId":45444,"offset":[0.25,0.5],"size":[2,3],"edgeRadius":0.1}}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using (var forwarded = JsonDocument.Parse(forwardedRequestJson!))
        {
            Assert.Equal("boxCollider2D.setSettings", forwarded.RootElement.GetProperty("method").GetString());
            var forwardedParams = forwarded.RootElement.GetProperty("params");
            Assert.Equal(2, forwardedParams.GetProperty("offset").GetArrayLength());
            Assert.Equal(2, forwardedParams.GetProperty("size").GetArrayLength());
            Assert.Equal(0.1, forwardedParams.GetProperty("edgeRadius").GetDouble(), 3);
        }

        using var document = JsonDocument.Parse(response.Body!);
        Assert.False(document.RootElement.GetProperty("result").GetProperty("isError").GetBoolean());
    }

    [Fact]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenToolCallTargetsCompositeCollider2DSetSettings_WithEnumFields()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"applied":{"geometryType":true,"generationType":true}}}""");
        });

        const string requestJson =
            """{"jsonrpc":"2.0","id":"comp2d-set-detailed-1","method":"tools/call","params":{"name":"compositeCollider2D.setSettings","arguments":{"instanceId":45444,"geometryType":"Outlines","generationType":"Manual"}}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using (var forwarded = JsonDocument.Parse(forwardedRequestJson!))
        {
            Assert.Equal("compositeCollider2D.setSettings", forwarded.RootElement.GetProperty("method").GetString());
            var forwardedParams = forwarded.RootElement.GetProperty("params");
            Assert.Equal("Outlines", forwardedParams.GetProperty("geometryType").GetString());
            Assert.Equal("Manual", forwardedParams.GetProperty("generationType").GetString());
        }

        using var document = JsonDocument.Parse(response.Body!);
        Assert.False(document.RootElement.GetProperty("result").GetProperty("isError").GetBoolean());
    }

    [Theory]
    [InlineData("""{"jsonrpc":"2.0","id":"hinge2d-get-1","method":"tools/call","params":{"name":"hingeJoint2D.getSettings","arguments":{"instanceId":45444}}}""", "hingeJoint2D.getSettings")]
    [InlineData("""{"jsonrpc":"2.0","id":"spring2d-get-1","method":"tools/call","params":{"name":"springJoint2D.getSettings","arguments":{"instanceId":45444}}}""", "springJoint2D.getSettings")]
    [InlineData("""{"jsonrpc":"2.0","id":"distance2d-get-1","method":"tools/call","params":{"name":"distanceJoint2D.getSettings","arguments":{"instanceId":45444}}}""", "distanceJoint2D.getSettings")]
    [InlineData("""{"jsonrpc":"2.0","id":"fixed2d-get-1","method":"tools/call","params":{"name":"fixedJoint2D.getSettings","arguments":{"instanceId":45444}}}""", "fixedJoint2D.getSettings")]
    [InlineData("""{"jsonrpc":"2.0","id":"slider2d-get-1","method":"tools/call","params":{"name":"sliderJoint2D.getSettings","arguments":{"instanceId":45444}}}""", "sliderJoint2D.getSettings")]
    [InlineData("""{"jsonrpc":"2.0","id":"wheel2d-get-1","method":"tools/call","params":{"name":"wheelJoint2D.getSettings","arguments":{"instanceId":45444}}}""", "wheelJoint2D.getSettings")]
    [InlineData("""{"jsonrpc":"2.0","id":"target2d-get-1","method":"tools/call","params":{"name":"targetJoint2D.getSettings","arguments":{"instanceId":45444}}}""", "targetJoint2D.getSettings")]
    [InlineData("""{"jsonrpc":"2.0","id":"hinge3d-get-1","method":"tools/call","params":{"name":"hingeJoint.getSettings","arguments":{"instanceId":45444}}}""", "hingeJoint.getSettings")]
    [InlineData("""{"jsonrpc":"2.0","id":"spring3d-get-1","method":"tools/call","params":{"name":"springJoint.getSettings","arguments":{"instanceId":45444}}}""", "springJoint.getSettings")]
    [InlineData("""{"jsonrpc":"2.0","id":"fixed3d-get-1","method":"tools/call","params":{"name":"fixedJoint.getSettings","arguments":{"instanceId":45444}}}""", "fixedJoint.getSettings")]
    [InlineData("""{"jsonrpc":"2.0","id":"character3d-get-1","method":"tools/call","params":{"name":"characterJoint.getSettings","arguments":{"instanceId":45444}}}""", "characterJoint.getSettings")]
    [InlineData("""{"jsonrpc":"2.0","id":"config3d-get-1","method":"tools/call","params":{"name":"configurableJoint.getSettings","arguments":{"instanceId":45444}}}""", "configurableJoint.getSettings")]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenToolCallTargetsJoint2DGetSettings(string requestJson, string expectedMethod)
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJsonValue, _, _) =>
        {
            forwardedRequestJson = requestJsonValue;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"settings":{"ok":true}}}""");
        });

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using (var forwarded = JsonDocument.Parse(forwardedRequestJson!))
        {
            Assert.Equal(expectedMethod, forwarded.RootElement.GetProperty("method").GetString());
            Assert.Equal(45444, forwarded.RootElement.GetProperty("params").GetProperty("instanceId").GetInt32());
        }

        using var document = JsonDocument.Parse(response.Body!);
        Assert.False(document.RootElement.GetProperty("result").GetProperty("isError").GetBoolean());
    }

    [Theory]
    [InlineData("""{"jsonrpc":"2.0","id":"hinge2d-set-1","method":"tools/call","params":{"name":"hingeJoint2D.setSettings","arguments":{"instanceId":45444,"useMotor":true}}}""", "hingeJoint2D.setSettings")]
    [InlineData("""{"jsonrpc":"2.0","id":"spring2d-set-1","method":"tools/call","params":{"name":"springJoint2D.setSettings","arguments":{"instanceId":45444,"distance":2.0}}}""", "springJoint2D.setSettings")]
    [InlineData("""{"jsonrpc":"2.0","id":"distance2d-set-1","method":"tools/call","params":{"name":"distanceJoint2D.setSettings","arguments":{"instanceId":45444,"maxDistanceOnly":true}}}""", "distanceJoint2D.setSettings")]
    [InlineData("""{"jsonrpc":"2.0","id":"fixed2d-set-1","method":"tools/call","params":{"name":"fixedJoint2D.setSettings","arguments":{"instanceId":45444,"frequency":5.0}}}""", "fixedJoint2D.setSettings")]
    [InlineData("""{"jsonrpc":"2.0","id":"slider2d-set-1","method":"tools/call","params":{"name":"sliderJoint2D.setSettings","arguments":{"instanceId":45444,"useLimits":true}}}""", "sliderJoint2D.setSettings")]
    [InlineData("""{"jsonrpc":"2.0","id":"wheel2d-set-1","method":"tools/call","params":{"name":"wheelJoint2D.setSettings","arguments":{"instanceId":45444,"useMotor":true}}}""", "wheelJoint2D.setSettings")]
    [InlineData("""{"jsonrpc":"2.0","id":"target2d-set-1","method":"tools/call","params":{"name":"targetJoint2D.setSettings","arguments":{"instanceId":45444,"maxForce":10.0}}}""", "targetJoint2D.setSettings")]
    [InlineData("""{"jsonrpc":"2.0","id":"hinge3d-set-1","method":"tools/call","params":{"name":"hingeJoint.setSettings","arguments":{"instanceId":45444,"useMotor":true}}}""", "hingeJoint.setSettings")]
    [InlineData("""{"jsonrpc":"2.0","id":"spring3d-set-1","method":"tools/call","params":{"name":"springJoint.setSettings","arguments":{"instanceId":45444,"spring":10.0}}}""", "springJoint.setSettings")]
    [InlineData("""{"jsonrpc":"2.0","id":"fixed3d-set-1","method":"tools/call","params":{"name":"fixedJoint.setSettings","arguments":{"instanceId":45444,"breakForce":5.0}}}""", "fixedJoint.setSettings")]
    [InlineData("""{"jsonrpc":"2.0","id":"character3d-set-1","method":"tools/call","params":{"name":"characterJoint.setSettings","arguments":{"instanceId":45444,"enableProjection":true}}}""", "characterJoint.setSettings")]
    [InlineData("""{"jsonrpc":"2.0","id":"config3d-set-1","method":"tools/call","params":{"name":"configurableJoint.setSettings","arguments":{"instanceId":45444,"xMotion":"Locked"}}}""", "configurableJoint.setSettings")]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenToolCallTargetsJoint2DSetSettings(string requestJson, string expectedMethod)
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJsonValue, _, _) =>
        {
            forwardedRequestJson = requestJsonValue;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"applied":{"ok":true}}}""");
        });

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using (var forwarded = JsonDocument.Parse(forwardedRequestJson!))
        {
            Assert.Equal(expectedMethod, forwarded.RootElement.GetProperty("method").GetString());
            Assert.Equal(45444, forwarded.RootElement.GetProperty("params").GetProperty("instanceId").GetInt32());
        }

        using var document = JsonDocument.Parse(response.Body!);
        Assert.False(document.RootElement.GetProperty("result").GetProperty("isError").GetBoolean());
    }

    [Fact]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenToolCallTargetsHingeJoint2DSetSettings_WithMotorAndLimits()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"applied":{"useMotor":true,"motorSpeed":true,"maxMotorTorque":true,"useLimits":true,"lowerAngle":true,"upperAngle":true}}}""");
        });

        const string requestJson =
            """{"jsonrpc":"2.0","id":"hinge2d-set-detailed-1","method":"tools/call","params":{"name":"hingeJoint2D.setSettings","arguments":{"instanceId":45444,"useMotor":true,"motorSpeed":120.0,"maxMotorTorque":15.0,"useLimits":true,"lowerAngle":-30.0,"upperAngle":45.0}}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using (var forwarded = JsonDocument.Parse(forwardedRequestJson!))
        {
            Assert.Equal("hingeJoint2D.setSettings", forwarded.RootElement.GetProperty("method").GetString());
            var forwardedParams = forwarded.RootElement.GetProperty("params");
            Assert.True(forwardedParams.GetProperty("useMotor").GetBoolean());
            Assert.Equal(120.0, forwardedParams.GetProperty("motorSpeed").GetDouble(), 3);
            Assert.Equal(15.0, forwardedParams.GetProperty("maxMotorTorque").GetDouble(), 3);
            Assert.True(forwardedParams.GetProperty("useLimits").GetBoolean());
            Assert.Equal(-30.0, forwardedParams.GetProperty("lowerAngle").GetDouble(), 3);
            Assert.Equal(45.0, forwardedParams.GetProperty("upperAngle").GetDouble(), 3);
        }

        using var document = JsonDocument.Parse(response.Body!);
        Assert.False(document.RootElement.GetProperty("result").GetProperty("isError").GetBoolean());
    }

    [Fact]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenToolCallTargetsJoint2DSetSettings_WithConnectedBodyInstanceId()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"applied":{"connectedBodyInstanceId":true}}}""");
        });

        const string requestJson =
            """{"jsonrpc":"2.0","id":"spring2d-set-connected-1","method":"tools/call","params":{"name":"springJoint2D.setSettings","arguments":{"instanceId":45444,"connectedBodyInstanceId":45555}}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using (var forwarded = JsonDocument.Parse(forwardedRequestJson!))
        {
            Assert.Equal("springJoint2D.setSettings", forwarded.RootElement.GetProperty("method").GetString());
            var forwardedParams = forwarded.RootElement.GetProperty("params");
            Assert.Equal(45444, forwardedParams.GetProperty("instanceId").GetInt32());
            Assert.Equal(45555, forwardedParams.GetProperty("connectedBodyInstanceId").GetInt32());
        }

        using var document = JsonDocument.Parse(response.Body!);
        Assert.False(document.RootElement.GetProperty("result").GetProperty("isError").GetBoolean());
    }

    [Fact]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenToolCallClearsConnectedBodyAndUsesConnectedAnchorMode()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"applied":{"connectedBodyInstanceId":null,"connectedAnchorMode":"zero"}}}""");
        });

        const string requestJson =
            """{"jsonrpc":"2.0","id":"fixed3d-set-connected-clear-1","method":"tools/call","params":{"name":"fixedJoint.setSettings","arguments":{"instanceId":45444,"connectedBodyInstanceId":null,"connectedAnchorMode":"zero"}}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using (var forwarded = JsonDocument.Parse(forwardedRequestJson!))
        {
            Assert.Equal("fixedJoint.setSettings", forwarded.RootElement.GetProperty("method").GetString());
            var forwardedParams = forwarded.RootElement.GetProperty("params");
            Assert.Equal(45444, forwardedParams.GetProperty("instanceId").GetInt32());
            Assert.Equal(JsonValueKind.Null, forwardedParams.GetProperty("connectedBodyInstanceId").ValueKind);
            Assert.Equal("zero", forwardedParams.GetProperty("connectedAnchorMode").GetString());
        }

        using var document = JsonDocument.Parse(response.Body!);
        Assert.False(document.RootElement.GetProperty("result").GetProperty("isError").GetBoolean());
    }

    [Fact]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenToolCallTargetsExpandedConfigurableJointSettings()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"applied":{"linearLimit":true,"xDrive":true,"projectionMode":true}}}""");
        });

        const string requestJson =
            """{"jsonrpc":"2.0","id":"config3d-set-detailed-1","method":"tools/call","params":{"name":"configurableJoint.setSettings","arguments":{"instanceId":45444,"linearLimit":{"limit":1.5,"bounciness":0.25,"contactDistance":0.05},"targetPosition":[1.0,2.0,3.0],"rotationDriveMode":"Slerp","xDrive":{"positionSpring":5.0,"positionDamper":1.5,"maximumForce":10.0},"projectionMode":"PositionAndRotation","projectionDistance":0.1,"projectionAngle":15.0}}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using (var forwarded = JsonDocument.Parse(forwardedRequestJson!))
        {
            Assert.Equal("configurableJoint.setSettings", forwarded.RootElement.GetProperty("method").GetString());
            var forwardedParams = forwarded.RootElement.GetProperty("params");
            Assert.Equal(45444, forwardedParams.GetProperty("instanceId").GetInt32());
            Assert.Equal(1.5, forwardedParams.GetProperty("linearLimit").GetProperty("limit").GetDouble(), 3);
            Assert.Equal(0.25, forwardedParams.GetProperty("linearLimit").GetProperty("bounciness").GetDouble(), 3);
            Assert.Equal(0.05, forwardedParams.GetProperty("linearLimit").GetProperty("contactDistance").GetDouble(), 3);
            Assert.Equal("Slerp", forwardedParams.GetProperty("rotationDriveMode").GetString());
            Assert.Equal(5.0, forwardedParams.GetProperty("xDrive").GetProperty("positionSpring").GetDouble(), 3);
            Assert.Equal(0.1, forwardedParams.GetProperty("projectionDistance").GetDouble(), 3);
            Assert.Equal(15.0, forwardedParams.GetProperty("projectionAngle").GetDouble(), 3);
        }

        using var document = JsonDocument.Parse(response.Body!);
        Assert.False(document.RootElement.GetProperty("result").GetProperty("isError").GetBoolean());
    }

    [Fact]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenToolCallTargetsSceneGetComponents()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"componentCount":3,"items":[]}}""");
        });

        const string requestJson =
            """{"jsonrpc":"2.0","id":"components-1","method":"tools/call","params":{"name":"scene.getComponents","arguments":{"instanceId":45444}}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using (var forwarded = JsonDocument.Parse(forwardedRequestJson!))
        {
            Assert.Equal("scene.getComponents", forwarded.RootElement.GetProperty("method").GetString());
            Assert.Equal(45444, forwarded.RootElement.GetProperty("params").GetProperty("instanceId").GetInt32());
        }

        using var document = JsonDocument.Parse(response.Body!);
        Assert.False(document.RootElement.GetProperty("result").GetProperty("isError").GetBoolean());
    }

    [Fact]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenToolCallTargetsSceneDestroyObject()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"destroyed":true,"destroyedKind":"gameObject"}}""");
        });

        const string requestJson =
            """{"jsonrpc":"2.0","id":"destroy-1","method":"tools/call","params":{"name":"scene.destroyObject","arguments":{"instanceId":45444}}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using (var forwarded = JsonDocument.Parse(forwardedRequestJson!))
        {
            Assert.Equal("scene.destroyObject", forwarded.RootElement.GetProperty("method").GetString());
            Assert.Equal(45444, forwarded.RootElement.GetProperty("params").GetProperty("instanceId").GetInt32());
        }

        using var document = JsonDocument.Parse(response.Body!);
        Assert.False(document.RootElement.GetProperty("result").GetProperty("isError").GetBoolean());
    }

    [Fact]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenToolCallTargetsSceneGetComponentProperties()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"component":{"instanceId":10},"propertyCount":1,"properties":{"m_Enabled":true}}}""");
        });

        const string requestJson =
            """{"jsonrpc":"2.0","id":"comp-props-1","method":"tools/call","params":{"name":"scene.getComponentProperties","arguments":{"componentInstanceId":45448}}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using (var forwarded = JsonDocument.Parse(forwardedRequestJson!))
        {
            Assert.Equal("scene.getComponentProperties", forwarded.RootElement.GetProperty("method").GetString());
            Assert.Equal(45448, forwarded.RootElement.GetProperty("params").GetProperty("componentInstanceId").GetInt32());
        }

        using var document = JsonDocument.Parse(response.Body!);
        Assert.False(document.RootElement.GetProperty("result").GetProperty("isError").GetBoolean());
    }

    [Fact]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenToolCallTargetsSceneSetComponentProperties()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"appliedCount":1,"updated":["m_Enabled"]}}""");
        });

        const string requestJson =
            """{"jsonrpc":"2.0","id":"comp-props-2","method":"tools/call","params":{"name":"scene.setComponentProperties","arguments":{"componentInstanceId":45448,"properties":{"m_Enabled":false}}}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using (var forwarded = JsonDocument.Parse(forwardedRequestJson!))
        {
            Assert.Equal("scene.setComponentProperties", forwarded.RootElement.GetProperty("method").GetString());
            var forwardedParams = forwarded.RootElement.GetProperty("params");
            Assert.Equal(45448, forwardedParams.GetProperty("componentInstanceId").GetInt32());
            Assert.False(forwardedParams.GetProperty("properties").GetProperty("m_Enabled").GetBoolean());
        }

        using var document = JsonDocument.Parse(response.Body!);
        Assert.False(document.RootElement.GetProperty("result").GetProperty("isError").GetBoolean());
    }

    [Fact]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenToolCallTargetsSceneSetTransform()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"instanceId":45444,"applied":{"position":true}}}""");
        });

        const string requestJson =
            """{"jsonrpc":"2.0","id":"transform-1","method":"tools/call","params":{"name":"scene.setTransform","arguments":{"instanceId":45444,"position":[1,2,3],"localScale":[1,1,1]}}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using (var forwarded = JsonDocument.Parse(forwardedRequestJson!))
        {
            Assert.Equal("scene.setTransform", forwarded.RootElement.GetProperty("method").GetString());
            var forwardedParams = forwarded.RootElement.GetProperty("params");
            Assert.Equal(45444, forwardedParams.GetProperty("instanceId").GetInt32());
            Assert.Equal(3, forwardedParams.GetProperty("position").GetArrayLength());
            Assert.Equal(3, forwardedParams.GetProperty("localScale").GetArrayLength());
        }

        using var document = JsonDocument.Parse(response.Body!);
        Assert.False(document.RootElement.GetProperty("result").GetProperty("isError").GetBoolean());
    }

    [Fact]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenToolCallTargetsSceneAddComponent()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"componentCount":4}}""");
        });

        const string requestJson =
            """{"jsonrpc":"2.0","id":"add-component-1","method":"tools/call","params":{"name":"scene.addComponent","arguments":{"instanceId":45444,"typeName":"BoxCollider"}}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using (var forwarded = JsonDocument.Parse(forwardedRequestJson!))
        {
            Assert.Equal("scene.addComponent", forwarded.RootElement.GetProperty("method").GetString());
            var forwardedParams = forwarded.RootElement.GetProperty("params");
            Assert.Equal(45444, forwardedParams.GetProperty("instanceId").GetInt32());
            Assert.Equal("BoxCollider", forwardedParams.GetProperty("typeName").GetString());
        }

        using var document = JsonDocument.Parse(response.Body!);
        Assert.False(document.RootElement.GetProperty("result").GetProperty("isError").GetBoolean());
    }

    [Fact]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenToolCallTargetsSceneSetParent()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"keepWorldTransform":true,"applied":{"reparented":true}}}""");
        });

        const string requestJson =
            """{"jsonrpc":"2.0","id":"set-parent-1","method":"tools/call","params":{"name":"scene.setParent","arguments":{"instanceId":45444,"parentInstanceId":null,"keepWorldTransform":true,"ping":true,"focus":false}}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using var forwarded = JsonDocument.Parse(forwardedRequestJson!);
        var forwardedParams = forwarded.RootElement.GetProperty("params");
        Assert.Equal("scene.setParent", forwarded.RootElement.GetProperty("method").GetString());
        Assert.Equal(45444, forwardedParams.GetProperty("instanceId").GetInt32());
        Assert.Equal(JsonValueKind.Null, forwardedParams.GetProperty("parentInstanceId").ValueKind);
        Assert.True(forwardedParams.GetProperty("keepWorldTransform").GetBoolean());
        Assert.True(forwardedParams.GetProperty("ping").GetBoolean());
        Assert.False(forwardedParams.GetProperty("focus").GetBoolean());
    }

    [Fact]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenToolCallTargetsSceneDuplicateObject()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"applied":{"selected":true}}}""");
        });

        const string requestJson =
            """{"jsonrpc":"2.0","id":"duplicate-1","method":"tools/call","params":{"name":"scene.duplicateObject","arguments":{"instanceId":45444,"select":false,"ping":true,"focus":true}}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using var forwarded = JsonDocument.Parse(forwardedRequestJson!);
        var forwardedParams = forwarded.RootElement.GetProperty("params");
        Assert.Equal("scene.duplicateObject", forwarded.RootElement.GetProperty("method").GetString());
        Assert.Equal(45444, forwardedParams.GetProperty("instanceId").GetInt32());
        Assert.False(forwardedParams.GetProperty("select").GetBoolean());
        Assert.True(forwardedParams.GetProperty("ping").GetBoolean());
        Assert.True(forwardedParams.GetProperty("focus").GetBoolean());
    }

    [Fact]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenToolCallTargetsSceneRenameObject()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"previousName":"Old","currentName":"New"}}""");
        });

        const string requestJson =
            """{"jsonrpc":"2.0","id":"rename-1","method":"tools/call","params":{"name":"scene.renameObject","arguments":{"instanceId":45444,"name":"Enemy Clone"}}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using var forwarded = JsonDocument.Parse(forwardedRequestJson!);
        var forwardedParams = forwarded.RootElement.GetProperty("params");
        Assert.Equal("scene.renameObject", forwarded.RootElement.GetProperty("method").GetString());
        Assert.Equal(45444, forwardedParams.GetProperty("instanceId").GetInt32());
        Assert.Equal("Enemy Clone", forwardedParams.GetProperty("name").GetString());
    }

    [Fact]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenToolCallTargetsSceneSetActive()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"activeSelf":false,"activeInHierarchy":false}}""");
        });

        const string requestJson =
            """{"jsonrpc":"2.0","id":"active-1","method":"tools/call","params":{"name":"scene.setActive","arguments":{"instanceId":45444,"active":false}}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using var forwarded = JsonDocument.Parse(forwardedRequestJson!);
        var forwardedParams = forwarded.RootElement.GetProperty("params");
        Assert.Equal("scene.setActive", forwarded.RootElement.GetProperty("method").GetString());
        Assert.Equal(45444, forwardedParams.GetProperty("instanceId").GetInt32());
        Assert.False(forwardedParams.GetProperty("active").GetBoolean());
    }

    [Fact]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenToolCallTargetsPrefabInstantiate()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"instance":{"instanceId":7001},"prefabSource":{"assetPath":"Assets/Prefabs/Test.prefab"}}}""");
        });

        const string requestJson =
            """{"jsonrpc":"2.0","id":"prefab-instantiate-1","method":"tools/call","params":{"name":"prefab.instantiate","arguments":{"assetPath":"Assets/Prefabs/Test.prefab","parentInstanceId":45444,"position":[1,2,3],"rotationEuler":[0,90,0],"select":true,"ping":true,"focus":false}}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using var forwarded = JsonDocument.Parse(forwardedRequestJson!);
        var forwardedParams = forwarded.RootElement.GetProperty("params");
        Assert.Equal("prefab.instantiate", forwarded.RootElement.GetProperty("method").GetString());
        Assert.Equal("Assets/Prefabs/Test.prefab", forwardedParams.GetProperty("assetPath").GetString());
        Assert.Equal(45444, forwardedParams.GetProperty("parentInstanceId").GetInt32());
        Assert.Equal(3, forwardedParams.GetProperty("position").GetArrayLength());
        Assert.Equal(3, forwardedParams.GetProperty("rotationEuler").GetArrayLength());
        Assert.True(forwardedParams.GetProperty("select").GetBoolean());
        Assert.True(forwardedParams.GetProperty("ping").GetBoolean());
        Assert.False(forwardedParams.GetProperty("focus").GetBoolean());
    }

    [Fact]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenToolCallTargetsPrefabGetSource()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"prefabInstanceStatus":"Connected","sourceAsset":{"assetPath":"Assets/Prefabs/Test.prefab"}}}""");
        });

        const string requestJson =
            """{"jsonrpc":"2.0","id":"prefab-source-1","method":"tools/call","params":{"name":"prefab.getSource","arguments":{"instanceId":45444}}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using var forwarded = JsonDocument.Parse(forwardedRequestJson!);
        Assert.Equal("prefab.getSource", forwarded.RootElement.GetProperty("method").GetString());
        Assert.Equal(45444, forwarded.RootElement.GetProperty("params").GetProperty("instanceId").GetInt32());
    }

    [Theory]
    [InlineData("prefab.applyOverrides", "apply-overrides-1", "instanceRoot")]
    [InlineData("prefab.revertOverrides", "revert-overrides-1", "instanceRoot")]
    [InlineData("prefab.applyOverrides", "apply-overrides-2", "object")]
    [InlineData("prefab.revertOverrides", "revert-overrides-2", "object")]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenToolCallTargetsPrefabNonComponentOverrideScope(
        string toolName,
        string requestId,
        string scope)
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult($"{{\"jsonrpc\":\"2.0\",\"id\":\"mcp-1\",\"result\":{{\"scope\":\"{scope}\",\"applied\":{{\"scope\":\"{scope}\"}}}}}}");
        });

        var requestJson =
            $"{{\"jsonrpc\":\"2.0\",\"id\":\"{requestId}\",\"method\":\"tools/call\",\"params\":{{\"name\":\"{toolName}\",\"arguments\":{{\"instanceId\":45444,\"scope\":\"{scope}\"}}}}}}";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using var forwarded = JsonDocument.Parse(forwardedRequestJson!);
        var forwardedParams = forwarded.RootElement.GetProperty("params");
        Assert.Equal(toolName, forwarded.RootElement.GetProperty("method").GetString());
        Assert.Equal(45444, forwardedParams.GetProperty("instanceId").GetInt32());
        Assert.Equal(scope, forwardedParams.GetProperty("scope").GetString());
    }

    [Theory]
    [InlineData("prefab.applyOverrides", "apply-overrides-3")]
    [InlineData("prefab.revertOverrides", "revert-overrides-3")]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenToolCallTargetsPrefabComponentOverrideScope(
        string toolName,
        string requestId)
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"scope":"component","applied":{"scope":"component","componentInstanceId":45445}}}""");
        });

        var requestJson =
            $"{{\"jsonrpc\":\"2.0\",\"id\":\"{requestId}\",\"method\":\"tools/call\",\"params\":{{\"name\":\"{toolName}\",\"arguments\":{{\"instanceId\":45444,\"scope\":\"component\",\"componentInstanceId\":45445}}}}}}";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using var forwarded = JsonDocument.Parse(forwardedRequestJson!);
        var forwardedParams = forwarded.RootElement.GetProperty("params");
        Assert.Equal(toolName, forwarded.RootElement.GetProperty("method").GetString());
        Assert.Equal(45444, forwardedParams.GetProperty("instanceId").GetInt32());
        Assert.Equal("component", forwardedParams.GetProperty("scope").GetString());
        Assert.Equal(45445, forwardedParams.GetProperty("componentInstanceId").GetInt32());
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
    public async Task HandlePostAsync_ForwardsPingAndFocus_WhenToolCallTargetsSceneSetSelection()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"count":2,"items":[{"instanceId":1},{"instanceId":2}]}}""");
        });

        const string requestJson =
            """{"jsonrpc":"2.0","id":"sel-2b","method":"tools/call","params":{"name":"scene.setSelection","arguments":{"instanceIds":[1,2],"ping":true,"focus":false}}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using var forwarded = JsonDocument.Parse(forwardedRequestJson!);
        var forwardedParams = forwarded.RootElement.GetProperty("params");
        Assert.Equal("scene.setSelection", forwarded.RootElement.GetProperty("method").GetString());
        Assert.True(forwardedParams.GetProperty("ping").GetBoolean());
        Assert.False(forwardedParams.GetProperty("focus").GetBoolean());
    }

    [Fact]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenToolCallTargetsScenePingObject()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"pinged":true,"instanceId":45444}}""");
        });

        const string requestJson =
            """{"jsonrpc":"2.0","id":"ping-1","method":"tools/call","params":{"name":"scene.pingObject","arguments":{"instanceId":45444}}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using (var forwarded = JsonDocument.Parse(forwardedRequestJson!))
        {
            Assert.Equal("scene.pingObject", forwarded.RootElement.GetProperty("method").GetString());
            Assert.Equal(45444, forwarded.RootElement.GetProperty("params").GetProperty("instanceId").GetInt32());
        }

        using var document = JsonDocument.Parse(response.Body!);
        Assert.False(document.RootElement.GetProperty("result").GetProperty("isError").GetBoolean());
    }

    [Fact]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenToolCallTargetsSceneFrameSelection()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"framed":true,"selectionCount":1}}""");
        });

        const string requestJson =
            """{"jsonrpc":"2.0","id":"frame-1","method":"tools/call","params":{"name":"scene.frameSelection","arguments":{}}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using (var forwarded = JsonDocument.Parse(forwardedRequestJson!))
        {
            Assert.Equal("scene.frameSelection", forwarded.RootElement.GetProperty("method").GetString());
            Assert.Equal(JsonValueKind.Object, forwarded.RootElement.GetProperty("params").ValueKind);
        }

        using var document = JsonDocument.Parse(response.Body!);
        Assert.False(document.RootElement.GetProperty("result").GetProperty("isError").GetBoolean());
    }

    [Fact]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenToolCallTargetsSceneFrameObject()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"framed":true,"instanceId":45444}}""");
        });

        const string requestJson =
            """{"jsonrpc":"2.0","id":"frame-2","method":"tools/call","params":{"name":"scene.frameObject","arguments":{"instanceId":45444}}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using (var forwarded = JsonDocument.Parse(forwardedRequestJson!))
        {
            Assert.Equal("scene.frameObject", forwarded.RootElement.GetProperty("method").GetString());
            Assert.Equal(45444, forwarded.RootElement.GetProperty("params").GetProperty("instanceId").GetInt32());
        }

        using var document = JsonDocument.Parse(response.Body!);
        Assert.False(document.RootElement.GetProperty("result").GetProperty("isError").GetBoolean());
    }

    [Fact]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenToolCallTargetsAssetsPing()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"pinged":true,"assetPath":"Assets/Test.asset"}}""");
        });

        const string requestJson =
            """{"jsonrpc":"2.0","id":"asset-ping-1","method":"tools/call","params":{"name":"assets.ping","arguments":{"assetPath":"Assets/Test.asset"}}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using (var forwarded = JsonDocument.Parse(forwardedRequestJson!))
        {
            Assert.Equal("assets.ping", forwarded.RootElement.GetProperty("method").GetString());
            Assert.Equal("Assets/Test.asset", forwarded.RootElement.GetProperty("params").GetProperty("assetPath").GetString());
        }

        using var document = JsonDocument.Parse(response.Body!);
        Assert.False(document.RootElement.GetProperty("result").GetProperty("isError").GetBoolean());
    }

    [Fact]
    public async Task HandlePostAsync_ForwardsUnityRequest_WhenToolCallTargetsAssetsReveal()
    {
        // Arrange
        string? forwardedRequestJson = null;
        var handler = CreateHandler((requestJson, _, _) =>
        {
            forwardedRequestJson = requestJson;
            return Task.FromResult("""{"jsonrpc":"2.0","id":"mcp-1","result":{"revealed":true,"assetPath":"Assets/Test.asset"}}""");
        });

        const string requestJson =
            """{"jsonrpc":"2.0","id":"asset-reveal-1","method":"tools/call","params":{"name":"assets.reveal","arguments":{"assetPath":"Assets/Test.asset"}}}""";

        // Act
        var response = await handler.HandlePostAsync(requestJson, CancellationToken.None);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(forwardedRequestJson);
        using (var forwarded = JsonDocument.Parse(forwardedRequestJson!))
        {
            Assert.Equal("assets.reveal", forwarded.RootElement.GetProperty("method").GetString());
            Assert.Equal("Assets/Test.asset", forwarded.RootElement.GetProperty("params").GetProperty("assetPath").GetString());
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
