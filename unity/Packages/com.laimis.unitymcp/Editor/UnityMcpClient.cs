#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityMcp.Editor
{

internal sealed class UnityMcpClient : IDisposable
{
    private static readonly object Sync = new();

    private static UnityMcpClient? _instance;

    private readonly SemaphoreSlim _sendLock = new(1, 1);
    private CancellationTokenSource? _lifetimeCts;
    private Task? _connectionLoopTask;
    private ClientWebSocket? _socket;
    private Uri? _configuredServerUri;

    public static UnityMcpClient Instance
    {
        get
        {
            lock (Sync)
            {
                return _instance ??= new UnityMcpClient();
            }
        }
    }

    public bool IsRunning => _connectionLoopTask is { IsCompleted: false };

    public void Start()
    {
        if (IsRunning)
        {
            return;
        }

        UnityMcpConsoleLogBuffer.EnsureInitialized();

        if (!UnityMcpSettings.TryGetServerUri(out var serverUri, out var serverUriError))
        {
            Debug.LogWarning($"[UnityMCP] Invalid server URI configuration: {serverUriError}");
            return;
        }

        _configuredServerUri = serverUri;
        _lifetimeCts = new CancellationTokenSource();
        _connectionLoopTask = Task.Run(() => ConnectionLoopAsync(_lifetimeCts.Token));
    }

    public void Stop()
    {
        try
        {
            _lifetimeCts?.Cancel();
        }
        catch
        {
            // Ignore cancellation races during domain reload/editor shutdown.
        }
    }

    public void Dispose()
    {
        Stop();
        _socket?.Dispose();
        _sendLock.Dispose();
        _lifetimeCts?.Dispose();
    }

    private async Task ConnectionLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            ClientWebSocket? socket = null;
            try
            {
                var serverUri = _configuredServerUri;
                if (serverUri == null)
                {
                    Debug.LogWarning("[UnityMCP] Server URI is not configured. Use Tools/Unity MCP/Settings.");
                    await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
                    continue;
                }

                socket = new ClientWebSocket();
                _socket = socket;

                await socket.ConnectAsync(serverUri, cancellationToken);
                Debug.Log($"[UnityMCP] Connected to {serverUri}.");

                await ReceiveLoopAsync(socket, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[UnityMCP] Connection loop error: {ex.Message}");
            }
            finally
            {
                try
                {
                    socket?.Dispose();
                }
                catch
                {
                    // Ignore cleanup failures.
                }

                if (ReferenceEquals(_socket, socket))
                {
                    _socket = null;
                }
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task ReceiveLoopAsync(ClientWebSocket socket, CancellationToken cancellationToken)
    {
        while (socket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
        {
            var message = await ReceiveTextAsync(socket, cancellationToken);
            if (message == null)
            {
                break;
            }

            UnityMcpMainThreadQueue.Enqueue(() => HandleMessageOnMainThread(message));
        }
    }

    private void HandleMessageOnMainThread(string message)
    {
        if (!UnityMcpProtocol.TryParse(message, out var document, out var parseError))
        {
            Debug.LogWarning($"[UnityMCP] Ignoring invalid JSON-RPC payload from server: {parseError}");
            return;
        }

        var root = document;

        if (!UnityMcpProtocol.TryGetId(root, out var idToken))
        {
            Debug.LogWarning("[UnityMCP] Ignoring JSON-RPC request without a valid id.");
            return;
        }

        if (!UnityMcpProtocol.TryGetMethod(root, out var method))
        {
            _ = SendAsync(UnityMcpProtocol.CreateError(idToken, -32600, "Missing JSON-RPC method."));
            return;
        }

        try
        {
            string response = method switch
            {
                "ping" => BuildPingResponse(idToken),
                "editor.getPlayModeState" => BuildPlayModeStateResponse(idToken),
                "editor.getConsoleLogs" => BuildGetConsoleLogsResponse(idToken, root),
                "editor.consoleTail" => BuildConsoleTailResponse(idToken, root),
                "editor.enterPlayMode" => BuildSetPlayModeResponse(idToken, shouldPlay: true),
                "editor.exitPlayMode" => BuildSetPlayModeResponse(idToken, shouldPlay: false),
                "scene.getActiveScene" => BuildGetActiveSceneResponse(idToken),
                "scene.listOpenScenes" => BuildListOpenScenesResponse(idToken),
                "scene.getSelection" => BuildGetSelectionResponse(idToken),
                "scene.selectObject" => BuildSelectObjectResponse(idToken, root),
                "scene.setSelection" => BuildSetSelectionResponse(idToken, root),
                "scene.createGameObject" => BuildCreateGameObjectResponse(idToken, root),
                "scene.findByTag" => BuildFindByTagResponse(idToken, root),
                "assets.find" => BuildFindAssetsResponse(idToken, root),
                "assets.import" => BuildImportAssetResponse(idToken, root),
                _ => UnityMcpProtocol.CreateError(idToken, -32601, $"Method '{method}' is not supported by UnityMCP MVP.")
            };

            _ = SendAsync(response);
        }
        catch (ArgumentException ex)
        {
            _ = SendAsync(UnityMcpProtocol.CreateError(idToken, -32602, ex.Message));
        }
        catch (Exception ex)
        {
            _ = SendAsync(UnityMcpProtocol.CreateError(idToken, -32603, ex.Message));
        }
    }

    private static string BuildPingResponse(JToken idToken)
    {
        var result = new
        {
            ok = true,
            source = "unity",
            unityVersion = Application.unityVersion
        };

        return UnityMcpProtocol.CreateResult(idToken, result);
    }

    private static string BuildPlayModeStateResponse(JToken idToken)
    {
        return UnityMcpProtocol.CreateResult(idToken, BuildEditorStateResult());
    }

    private static string BuildGetConsoleLogsResponse(JToken idToken, JObject root)
    {
        ParseConsoleQueryOptions(
            root,
            methodName: "editor.getConsoleLogs",
            defaultMaxResults: 100,
            defaultIncludeStackTrace: false,
            requireAfterSequence: false,
            out var maxResults,
            out var includeStackTrace,
            out _,
            out var levels,
            out var contains);

        var queryResult = UnityMcpConsoleLogBuffer.GetSnapshot(maxResults, includeStackTrace, levels, contains);
        return UnityMcpProtocol.CreateResult(idToken, CreateConsoleQueryResultPayload(queryResult, levels, contains));
    }

    private static string BuildConsoleTailResponse(JToken idToken, JObject root)
    {
        ParseConsoleQueryOptions(
            root,
            methodName: "editor.consoleTail",
            defaultMaxResults: 100,
            defaultIncludeStackTrace: false,
            requireAfterSequence: true,
            out var maxResults,
            out var includeStackTrace,
            out var afterSequence,
            out var levels,
            out var contains);

        var queryResult = UnityMcpConsoleLogBuffer.GetTail(afterSequence, maxResults, includeStackTrace, levels, contains);
        return UnityMcpProtocol.CreateResult(idToken, CreateConsoleQueryResultPayload(queryResult, levels, contains));
    }

    private static string BuildSetPlayModeResponse(JToken idToken, bool shouldPlay)
    {
        var changed = EditorApplication.isPlaying != shouldPlay;
        if (changed)
        {
            EditorApplication.isPlaying = shouldPlay;
        }

        var state = BuildEditorStateResult();
        var result = new
        {
            isPlaying = state.isPlaying,
            isPaused = state.isPaused,
            isCompiling = state.isCompiling,
            isPlayingOrWillChangePlaymode = state.isPlayingOrWillChangePlaymode,
            requestedState = shouldPlay ? "playing" : "editing",
            changed
        };

        return UnityMcpProtocol.CreateResult(idToken, result);
    }

    private static string BuildGetActiveSceneResponse(JToken idToken)
    {
        var activeScene = SceneManager.GetActiveScene();
        var result = CreateSceneSummary(activeScene, isActive: true);
        return UnityMcpProtocol.CreateResult(idToken, result);
    }

    private static string BuildListOpenScenesResponse(JToken idToken)
    {
        var activeScene = SceneManager.GetActiveScene();
        var activeHandle = activeScene.handle;
        var items = new List<object>();

        var sceneCount = SceneManager.sceneCount;
        for (var index = 0; index < sceneCount; index++)
        {
            var scene = SceneManager.GetSceneAt(index);
            items.Add(CreateSceneSummary(scene, isActive: scene.handle == activeHandle));
        }

        var result = new
        {
            count = items.Count,
            activeSceneHandle = activeHandle,
            items
        };

        return UnityMcpProtocol.CreateResult(idToken, result);
    }

    private static string BuildGetSelectionResponse(JToken idToken)
    {
        return UnityMcpProtocol.CreateResult(idToken, BuildSelectionSummaryResult());
    }

    private static string BuildSelectObjectResponse(JToken idToken, JObject root)
    {
        var paramsObject = RequireParamsObject(root, "scene.selectObject");
        var instanceId = ParseRequiredIntegerParameter(paramsObject, "instanceId");
        var targetObject = ResolveObjectByInstanceId(instanceId, "instanceId");

        Selection.activeObject = targetObject;
        Selection.objects = new[] { targetObject };

        return UnityMcpProtocol.CreateResult(idToken, BuildSelectionSummaryResult());
    }

    private static string BuildSetSelectionResponse(JToken idToken, JObject root)
    {
        var paramsObject = RequireParamsObject(root, "scene.setSelection");
        if (!paramsObject.TryGetValue("instanceIds", out var instanceIdsToken) || instanceIdsToken is not JArray instanceIdsArray)
        {
            throw new ArgumentException("Parameter 'instanceIds' is required and must be an array of integers.");
        }

        var resolvedObjects = new List<UnityEngine.Object>(instanceIdsArray.Count);
        var seen = new HashSet<int>();

        foreach (var item in instanceIdsArray)
        {
            if (item.Type != JTokenType.Integer)
            {
                throw new ArgumentException("Parameter 'instanceIds' must contain only integers.");
            }

            var instanceId = item.Value<int?>();
            if (!instanceId.HasValue)
            {
                throw new ArgumentException("Parameter 'instanceIds' must contain only integers.");
            }

            if (!seen.Add(instanceId.Value))
            {
                continue;
            }

            resolvedObjects.Add(ResolveObjectByInstanceId(instanceId.Value, "instanceIds"));
        }

        Selection.objects = resolvedObjects.ToArray();

        return UnityMcpProtocol.CreateResult(idToken, BuildSelectionSummaryResult());
    }

    private static string BuildCreateGameObjectResponse(JToken idToken, JObject root)
    {
        var name = "GameObject";
        Vector3? position = null;

        if (root.TryGetValue("params", out var paramsToken) && paramsToken.Type != JTokenType.Null)
        {
            if (paramsToken is not JObject paramsObject)
            {
                throw new ArgumentException("Method 'scene.createGameObject' expects params to be an object.");
            }

            if (paramsObject.TryGetValue("name", out var nameToken))
            {
                if (nameToken.Type != JTokenType.String)
                {
                    throw new ArgumentException("Parameter 'name' must be a string.");
                }

                var parsedName = nameToken.Value<string>();
                if (string.IsNullOrWhiteSpace(parsedName))
                {
                    throw new ArgumentException("Parameter 'name' cannot be empty.");
                }

                name = parsedName;
            }

            if (paramsObject.TryGetValue("position", out var positionToken))
            {
                position = ParsePosition(positionToken);
            }
        }

        var gameObject = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(gameObject, "UnityMCP Create GameObject");

        if (position.HasValue)
        {
            gameObject.transform.position = position.Value;
        }

        Selection.activeGameObject = gameObject;

        var activeScene = SceneManager.GetActiveScene();
        var currentPosition = gameObject.transform.position;
        var result = new
        {
            instanceId = gameObject.GetInstanceID(),
            name = gameObject.name,
            sceneName = activeScene.name,
            scenePath = activeScene.path,
            hierarchyPath = GetHierarchyPath(gameObject.transform),
            position = new[] { currentPosition.x, currentPosition.y, currentPosition.z }
        };

        return UnityMcpProtocol.CreateResult(idToken, result);
    }

    private static string BuildFindByTagResponse(JToken idToken, JObject root)
    {
        if (!root.TryGetValue("params", out var paramsToken) || paramsToken is not JObject paramsObject)
        {
            throw new ArgumentException("Method 'scene.findByTag' expects params to be an object.");
        }

        if (!paramsObject.TryGetValue("tag", out var tagToken) || tagToken.Type != JTokenType.String)
        {
            throw new ArgumentException("Parameter 'tag' is required and must be a string.");
        }

        var tag = tagToken.Value<string>();
        if (string.IsNullOrWhiteSpace(tag))
        {
            throw new ArgumentException("Parameter 'tag' cannot be empty.");
        }

        GameObject[] matches;
        try
        {
            matches = GameObject.FindGameObjectsWithTag(tag);
        }
        catch (UnityException ex)
        {
            throw new ArgumentException(ex.Message);
        }

        var items = new List<object>(matches.Length);
        foreach (var gameObject in matches)
        {
            var transform = gameObject.transform;
            var position = transform.position;
            var scene = gameObject.scene;

            items.Add(new
            {
                instanceId = gameObject.GetInstanceID(),
                name = gameObject.name,
                tag = gameObject.tag,
                activeSelf = gameObject.activeSelf,
                activeInHierarchy = gameObject.activeInHierarchy,
                sceneName = scene.name,
                scenePath = scene.path,
                hierarchyPath = GetHierarchyPath(transform),
                position = new[] { position.x, position.y, position.z }
            });
        }

        var result = new
        {
            tag,
            count = matches.Length,
            items
        };

        return UnityMcpProtocol.CreateResult(idToken, result);
    }

    private static string BuildImportAssetResponse(JToken idToken, JObject root)
    {
        if (!root.TryGetValue("params", out var paramsToken) || paramsToken is not JObject paramsObject)
        {
            throw new ArgumentException("Method 'assets.import' expects params to be an object.");
        }

        if (!paramsObject.TryGetValue("assetPath", out var assetPathToken) || assetPathToken.Type != JTokenType.String)
        {
            throw new ArgumentException("Parameter 'assetPath' is required and must be a string.");
        }

        var rawAssetPath = assetPathToken.Value<string>();
        var assetPath = NormalizeAndValidateAssetPath(rawAssetPath);

        var absoluteAssetPath = GetAbsoluteProjectPath(assetPath);
        var isFolder = Directory.Exists(absoluteAssetPath);
        var isFile = File.Exists(absoluteAssetPath);
        if (!isFolder && !isFile)
        {
            throw new ArgumentException($"Asset path '{assetPath}' does not exist in the Unity project.");
        }

        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        AssetDatabase.Refresh(ImportAssetOptions.Default);

        var guid = AssetDatabase.AssetPathToGUID(assetPath);
        if (string.IsNullOrWhiteSpace(guid))
        {
            throw new InvalidOperationException($"Unity did not return a GUID for imported asset '{assetPath}'.");
        }

        var mainAssetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
        var mainAsset = AssetDatabase.LoadMainAssetAtPath(assetPath);

        var result = new
        {
            assetPath,
            guid,
            isFolder,
            exists = true,
            mainAssetType = mainAssetType?.FullName,
            mainAssetName = mainAsset != null ? mainAsset.name : null,
            imported = true
        };

        return UnityMcpProtocol.CreateResult(idToken, result);
    }

    private static string BuildFindAssetsResponse(JToken idToken, JObject root)
    {
        if (!root.TryGetValue("params", out var paramsToken) || paramsToken is not JObject paramsObject)
        {
            throw new ArgumentException("Method 'assets.find' expects params to be an object.");
        }

        if (!paramsObject.TryGetValue("query", out var queryToken) || queryToken.Type != JTokenType.String)
        {
            throw new ArgumentException("Parameter 'query' is required and must be a string.");
        }

        var query = queryToken.Value<string>();
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new ArgumentException("Parameter 'query' cannot be empty.");
        }

        var maxResults = 100;
        if (paramsObject.TryGetValue("maxResults", out var maxResultsToken))
        {
            if (maxResultsToken.Type != JTokenType.Integer)
            {
                throw new ArgumentException("Parameter 'maxResults' must be an integer.");
            }

            var parsedMaxResults = maxResultsToken.Value<int?>();
            if (!parsedMaxResults.HasValue)
            {
                throw new ArgumentException("Parameter 'maxResults' must be an integer.");
            }

            if (parsedMaxResults.Value < 1 || parsedMaxResults.Value > 500)
            {
                throw new ArgumentException("Parameter 'maxResults' must be between 1 and 500.");
            }

            maxResults = parsedMaxResults.Value;
        }

        var searchInFolders = ParseOptionalStringArrayParameter(paramsObject, "searchInFolders");
        if (searchInFolders != null)
        {
            for (var index = 0; index < searchInFolders.Count; index++)
            {
                var normalizedFolder = NormalizeAndValidateAssetPath(searchInFolders[index]);
                if (!AssetDatabase.IsValidFolder(normalizedFolder))
                {
                    throw new ArgumentException($"Search folder '{normalizedFolder}' does not exist or is not a valid Unity folder.");
                }

                searchInFolders[index] = normalizedFolder;
            }
        }

        var types = ParseOptionalStringArrayParameter(paramsObject, "types");
        var labels = ParseOptionalStringArrayParameter(paramsObject, "labels");
        var effectiveQuery = BuildEffectiveAssetsFindQuery(query!, types, labels);

        var guids = searchInFolders is { Count: > 0 }
            ? AssetDatabase.FindAssets(effectiveQuery, searchInFolders.ToArray())
            : AssetDatabase.FindAssets(effectiveQuery);
        var takeCount = Math.Min(maxResults, guids.Length);
        var items = new List<object>(takeCount);
        for (var index = 0; index < takeCount; index++)
        {
            var guid = guids[index];
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var mainAssetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
            var mainAsset = AssetDatabase.LoadMainAssetAtPath(assetPath);

            items.Add(new
            {
                guid,
                assetPath,
                isFolder = AssetDatabase.IsValidFolder(assetPath),
                mainAssetType = mainAssetType != null ? mainAssetType.FullName : null,
                mainAssetName = mainAsset != null ? mainAsset.name : null
            });
        }

        var result = new
        {
            query,
            effectiveQuery,
            searchInFolders,
            types,
            labels,
            totalMatched = guids.Length,
            returnedCount = items.Count,
            maxResults,
            truncated = guids.Length > takeCount,
            items
        };

        return UnityMcpProtocol.CreateResult(idToken, result);
    }

    private static List<string>? ParseOptionalStringArrayParameter(JObject paramsObject, string parameterName)
    {
        if (!paramsObject.TryGetValue(parameterName, out var token))
        {
            return null;
        }

        if (token.Type != JTokenType.Array || token is not JArray array)
        {
            throw new ArgumentException($"Parameter '{parameterName}' must be an array of strings.");
        }

        var values = new List<string>(array.Count);
        foreach (var item in array)
        {
            if (item.Type != JTokenType.String)
            {
                throw new ArgumentException($"Parameter '{parameterName}' must contain only strings.");
            }

            var value = item.Value<string>();
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException($"Parameter '{parameterName}' cannot contain empty values.");
            }

            values.Add(value!.Trim());
        }

        return values;
    }

    private static string BuildEffectiveAssetsFindQuery(string query, List<string>? types, List<string>? labels)
    {
        var parts = new List<string> { query };

        if (types != null)
        {
            foreach (var type in types)
            {
                parts.Add($"t:{type}");
            }
        }

        if (labels != null)
        {
            foreach (var label in labels)
            {
                parts.Add($"l:{label}");
            }
        }

        return string.Join(" ", parts);
    }

    private static object CreateConsoleQueryResultPayload(
        UnityMcpConsoleLogBuffer.ConsoleLogQueryResult queryResult,
        IReadOnlyList<string>? levels,
        string? contains)
    {
        return new
        {
            bufferCapacity = queryResult.BufferCapacity,
            totalBuffered = queryResult.TotalBuffered,
            bufferStartSequence = queryResult.BufferStartSequence,
            latestSequence = queryResult.LatestSequence,
            afterSequence = queryResult.AfterSequence,
            nextAfterSequence = queryResult.NextAfterSequence,
            cursorBehindBuffer = queryResult.CursorBehindBuffer,
            returnedCount = queryResult.Items.Count,
            truncated = queryResult.Truncated,
            includeStackTrace = queryResult.IncludeStackTrace,
            levels = levels,
            contains,
            items = queryResult.Items
        };
    }

    private static void ParseConsoleQueryOptions(
        JObject root,
        string methodName,
        int defaultMaxResults,
        bool defaultIncludeStackTrace,
        bool requireAfterSequence,
        out int maxResults,
        out bool includeStackTrace,
        out long afterSequence,
        out List<string>? levels,
        out string? contains)
    {
        maxResults = defaultMaxResults;
        includeStackTrace = defaultIncludeStackTrace;
        afterSequence = 0;
        levels = null;
        contains = null;

        if (!root.TryGetValue("params", out var paramsToken) || paramsToken.Type == JTokenType.Null)
        {
            if (requireAfterSequence)
            {
                throw new ArgumentException($"Method '{methodName}' expects params to be an object.");
            }

            return;
        }

        if (paramsToken is not JObject paramsObject)
        {
            throw new ArgumentException($"Method '{methodName}' expects params to be an object.");
        }

        if (paramsObject.TryGetValue("maxResults", out var maxResultsToken))
        {
            if (maxResultsToken.Type != JTokenType.Integer)
            {
                throw new ArgumentException("Parameter 'maxResults' must be an integer.");
            }

            var parsedMaxResults = maxResultsToken.Value<int?>();
            if (!parsedMaxResults.HasValue || parsedMaxResults.Value < 1 || parsedMaxResults.Value > 500)
            {
                throw new ArgumentException("Parameter 'maxResults' must be between 1 and 500.");
            }

            maxResults = parsedMaxResults.Value;
        }

        if (paramsObject.TryGetValue("includeStackTrace", out var includeStackTraceToken))
        {
            if (includeStackTraceToken.Type != JTokenType.Boolean)
            {
                throw new ArgumentException("Parameter 'includeStackTrace' must be a boolean.");
            }

            var parsedIncludeStackTrace = includeStackTraceToken.Value<bool?>();
            if (!parsedIncludeStackTrace.HasValue)
            {
                throw new ArgumentException("Parameter 'includeStackTrace' must be a boolean.");
            }

            includeStackTrace = parsedIncludeStackTrace.Value;
        }

        var parsedLevels = ParseOptionalStringArrayParameter(paramsObject, "levels");
        if (parsedLevels != null)
        {
            levels = NormalizeConsoleLevels(parsedLevels);
        }

        if (paramsObject.TryGetValue("contains", out var containsToken))
        {
            if (containsToken.Type != JTokenType.String)
            {
                throw new ArgumentException("Parameter 'contains' must be a string.");
            }

            var parsedContains = containsToken.Value<string>();
            if (string.IsNullOrWhiteSpace(parsedContains))
            {
                throw new ArgumentException("Parameter 'contains' cannot be empty.");
            }

            contains = parsedContains!.Trim();
        }

        if (requireAfterSequence)
        {
            if (!paramsObject.TryGetValue("afterSequence", out var afterSequenceToken) || afterSequenceToken.Type != JTokenType.Integer)
            {
                throw new ArgumentException("Parameter 'afterSequence' is required and must be an integer.");
            }

            var parsedAfterSequence = afterSequenceToken.Value<long?>();
            if (!parsedAfterSequence.HasValue || parsedAfterSequence.Value < 0)
            {
                throw new ArgumentException("Parameter 'afterSequence' must be a non-negative integer.");
            }

            afterSequence = parsedAfterSequence.Value;
        }
    }

    private static List<string> NormalizeConsoleLevels(List<string> levels)
    {
        var normalized = new List<string>(levels.Count);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var level in levels)
        {
            string canonical = level.ToLowerInvariant() switch
            {
                "info" => "info",
                "log" => "info",
                "warning" => "warning",
                "warn" => "warning",
                "error" => "error",
                "assert" => "assert",
                "exception" => "exception",
                _ => throw new ArgumentException(
                    "Parameter 'levels' contains an unsupported value. Allowed values: info, warning, error, assert, exception.")
            };

            if (seen.Add(canonical))
            {
                normalized.Add(canonical);
            }
        }

        return normalized;
    }

    private static object BuildSelectionSummaryResult()
    {
        var selectedObjects = Selection.objects;
        var items = new List<object>(selectedObjects.Length);
        foreach (var selectedObject in selectedObjects)
        {
            if (selectedObject == null)
            {
                continue;
            }

            items.Add(CreateObjectSummary(selectedObject));
        }

        var activeObject = Selection.activeObject;
        object? activeObjectSummary = null;
        if (activeObject != null)
        {
            activeObjectSummary = CreateObjectSummary(activeObject);
        }

        var activeGameObject = Selection.activeGameObject;
        object? activeGameObjectSummary = null;
        if (activeGameObject != null)
        {
            activeGameObjectSummary = CreateObjectSummary(activeGameObject);
        }

        return new
        {
            count = items.Count,
            activeObject = activeObjectSummary,
            activeGameObject = activeGameObjectSummary,
            items
        };
    }

    private static JObject RequireParamsObject(JObject root, string methodName)
    {
        if (!root.TryGetValue("params", out var paramsToken) || paramsToken is not JObject paramsObject)
        {
            throw new ArgumentException($"Method '{methodName}' expects params to be an object.");
        }

        return paramsObject;
    }

    private static int ParseRequiredIntegerParameter(JObject paramsObject, string parameterName)
    {
        if (!paramsObject.TryGetValue(parameterName, out var token) || token.Type != JTokenType.Integer)
        {
            throw new ArgumentException($"Parameter '{parameterName}' is required and must be an integer.");
        }

        var value = token.Value<int?>();
        if (!value.HasValue)
        {
            throw new ArgumentException($"Parameter '{parameterName}' is required and must be an integer.");
        }

        return value.Value;
    }

    private static UnityEngine.Object ResolveObjectByInstanceId(int instanceId, string parameterName)
    {
        var resolved = TryResolveObjectByEntityId(instanceId) ?? ResolveObjectByLegacyInstanceId(instanceId);
        if (resolved == null)
        {
            throw new ArgumentException($"No Unity object found for instanceId {instanceId}.", parameterName);
        }

        return resolved;
    }

    private static UnityEngine.Object? TryResolveObjectByEntityId(int instanceId)
    {
        try
        {
            var editorUtilityType = typeof(EditorUtility);
            var intMethod = editorUtilityType.GetMethod(
                "EntityIdToObject",
                BindingFlags.Public | BindingFlags.Static,
                binder: null,
                types: new[] { typeof(int) },
                modifiers: null);

            if (intMethod != null)
            {
                return intMethod.Invoke(null, new object[] { instanceId }) as UnityEngine.Object;
            }

            var longMethod = editorUtilityType.GetMethod(
                "EntityIdToObject",
                BindingFlags.Public | BindingFlags.Static,
                binder: null,
                types: new[] { typeof(long) },
                modifiers: null);

            if (longMethod != null)
            {
                return longMethod.Invoke(null, new object[] { (long)instanceId }) as UnityEngine.Object;
            }
        }
        catch
        {
            // Fall back to the legacy API if the newer API is unavailable or throws.
        }

        return null;
    }

    private static UnityEngine.Object? ResolveObjectByLegacyInstanceId(int instanceId)
    {
#pragma warning disable CS0618 // Unity 6 deprecates InstanceIDToObject in favor of EntityIdToObject.
        return EditorUtility.InstanceIDToObject(instanceId);
#pragma warning restore CS0618
    }

    private static Vector3 ParsePosition(JToken positionToken)
    {
        if (positionToken is not JArray positionArray)
        {
            throw new ArgumentException("Parameter 'position' must be an array [x, y, z].");
        }

        var values = new float[3];
        var index = 0;
        foreach (var item in positionArray)
        {
            if (index >= values.Length)
            {
                throw new ArgumentException("Parameter 'position' must contain exactly 3 numeric values.");
            }

            if (item.Type != JTokenType.Integer && item.Type != JTokenType.Float)
            {
                throw new ArgumentException("Parameter 'position' must contain numeric values.");
            }

            var itemValue = item.Value<float?>();
            if (!itemValue.HasValue)
            {
                throw new ArgumentException("Parameter 'position' must contain numeric values.");
            }

            values[index] = itemValue.Value;
            index++;
        }

        if (index != 3)
        {
            throw new ArgumentException("Parameter 'position' must contain exactly 3 numeric values.");
        }

        return new Vector3(values[0], values[1], values[2]);
    }

    private static string GetHierarchyPath(Transform transform)
    {
        var names = new Stack<string>();
        var current = transform;

        while (current != null)
        {
            names.Push(current.name);
            current = current.parent;
        }

        return string.Join("/", names);
    }

    private static string NormalizeAndValidateAssetPath(string? rawAssetPath)
    {
        if (string.IsNullOrWhiteSpace(rawAssetPath))
        {
            throw new ArgumentException("Parameter 'assetPath' cannot be empty.");
        }

        var normalized = rawAssetPath!.Trim().Replace('\\', '/');
        if (Path.IsPathRooted(normalized) || normalized.StartsWith("/", StringComparison.Ordinal))
        {
            throw new ArgumentException("Parameter 'assetPath' must be a Unity project-relative path under 'Assets/'.");
        }

        if (!string.Equals(normalized, "Assets", StringComparison.Ordinal) &&
            !normalized.StartsWith("Assets/", StringComparison.Ordinal))
        {
            throw new ArgumentException("Parameter 'assetPath' must start with 'Assets/'.");
        }

        var segments = normalized.Split('/');
        foreach (var segment in segments)
        {
            if (string.IsNullOrWhiteSpace(segment))
            {
                throw new ArgumentException("Parameter 'assetPath' cannot contain empty path segments.");
            }

            if (string.Equals(segment, ".", StringComparison.Ordinal) ||
                string.Equals(segment, "..", StringComparison.Ordinal))
            {
                throw new ArgumentException("Parameter 'assetPath' cannot contain '.' or '..' path segments.");
            }
        }

        return normalized;
    }

    private static string GetAbsoluteProjectPath(string assetPath)
    {
        var projectRoot = Path.GetDirectoryName(Application.dataPath);
        if (string.IsNullOrWhiteSpace(projectRoot))
        {
            throw new InvalidOperationException("Unable to determine Unity project root path.");
        }

        var relativePath = assetPath.Replace('/', Path.DirectorySeparatorChar);
        return Path.Combine(projectRoot, relativePath);
    }

    private static object CreateSceneSummary(Scene scene, bool isActive)
    {
        if (!scene.IsValid())
        {
            return new
            {
                isValid = false,
                isLoaded = false,
                isActive,
                handle = scene.handle,
                buildIndex = scene.buildIndex,
                name = scene.name,
                path = scene.path,
                rootCount = 0
            };
        }

        return new
        {
            isValid = true,
            isLoaded = scene.isLoaded,
            isActive,
            handle = scene.handle,
            buildIndex = scene.buildIndex,
            name = scene.name,
            path = scene.path,
            rootCount = scene.rootCount
        };
    }

    private static object CreateObjectSummary(UnityEngine.Object unityObject)
    {
        var unityType = unityObject.GetType();
        var assetPath = AssetDatabase.GetAssetPath(unityObject);
        var isPersistent = EditorUtility.IsPersistent(unityObject);

        string? sceneName = null;
        string? scenePath = null;
        string? hierarchyPath = null;
        bool? activeSelf = null;
        bool? activeInHierarchy = null;
        string? componentType = null;

        if (unityObject is GameObject gameObject)
        {
            var scene = gameObject.scene;
            sceneName = scene.name;
            scenePath = scene.path;
            hierarchyPath = GetHierarchyPath(gameObject.transform);
            activeSelf = gameObject.activeSelf;
            activeInHierarchy = gameObject.activeInHierarchy;
        }
        else if (unityObject is Component component)
        {
            var ownerGameObject = component.gameObject;
            var scene = ownerGameObject.scene;
            sceneName = scene.name;
            scenePath = scene.path;
            hierarchyPath = GetHierarchyPath(component.transform);
            activeSelf = ownerGameObject.activeSelf;
            activeInHierarchy = ownerGameObject.activeInHierarchy;
            componentType = unityType.FullName;
        }

        return new
        {
            instanceId = unityObject.GetInstanceID(),
            name = unityObject.name,
            unityType = unityType.FullName,
            isPersistent,
            assetPath = string.IsNullOrWhiteSpace(assetPath) ? null : assetPath,
            sceneName,
            scenePath,
            hierarchyPath,
            activeSelf,
            activeInHierarchy,
            componentType
        };
    }

    private static EditorStateSnapshot BuildEditorStateResult()
    {
        return new EditorStateSnapshot
        {
            isPlaying = EditorApplication.isPlaying,
            isPaused = EditorApplication.isPaused,
            isCompiling = EditorApplication.isCompiling,
            isPlayingOrWillChangePlaymode = EditorApplication.isPlayingOrWillChangePlaymode
        };
    }

    private sealed class EditorStateSnapshot
    {
        public bool isPlaying { get; set; }
        public bool isPaused { get; set; }
        public bool isCompiling { get; set; }
        public bool isPlayingOrWillChangePlaymode { get; set; }
    }

    private async Task SendAsync(string payload)
    {
        var socket = _socket;
        if (socket == null || socket.State != WebSocketState.Open)
        {
            return;
        }

        await _sendLock.WaitAsync();
        try
        {
            if (socket.State != WebSocketState.Open)
            {
                return;
            }

            var bytes = Encoding.UTF8.GetBytes(payload);
            await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[UnityMCP] Send failed: {ex.Message}");
        }
        finally
        {
            _sendLock.Release();
        }
    }

    private static async Task<string?> ReceiveTextAsync(ClientWebSocket socket, CancellationToken cancellationToken)
    {
        var buffer = new byte[4096];
        var builder = new StringBuilder();

        while (socket.State == WebSocketState.Open)
        {
            var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                return null;
            }

            if (result.MessageType != WebSocketMessageType.Text)
            {
                continue;
            }

            builder.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
            if (result.EndOfMessage)
            {
                return builder.ToString();
            }
        }

        return null;
    }
}
}
