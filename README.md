# UnityMCP (Codex <-> Unity Editor MCP Bridge)

MVP foundation for a local MCP relay server and Unity Editor package that let Codex issue JSON-RPC commands to the Unity Editor.

## Current MVP
- `.NET / C#` WebSocket relay server
- MCP HTTP endpoint for Codex (`/mcp`)
- Unity Editor package (UPM local package workflow)
- JSON-RPC 2.0 request/response forwarding
- Main-thread Unity command dispatch (via `EditorApplication.update`)
- Implemented methods:
  - MCP: `initialize`, `notifications/initialized`, `ping`
  - MCP: `tools/list`, `tools/call`
  - MCP: `prompts/list` (compatibility, empty)
  - MCP: `resources/list`, `resources/templates/list`, `resources/read`
  - `ping`
  - `editor.getPlayModeState`
  - `editor.getConsoleLogs`
  - `editor.consoleTail`
  - `editor.enterPlayMode`
  - `editor.exitPlayMode`
  - `scene.getActiveScene`
  - `scene.listOpenScenes`
  - `scene.getSelection`
  - `scene.selectObject`
  - `scene.selectByPath`
  - `scene.findByPath`
  - `camera.getSettings`
  - `camera.setSettings`
  - `light.getSettings`
  - `light.setSettings`
  - `rigidbody.getSettings`
  - `rigidbody.setSettings`
  - `collider.getSettings`
  - `collider.setSettings`
  - `boxCollider.getSettings`
  - `boxCollider.setSettings`
  - `sphereCollider.getSettings`
  - `sphereCollider.setSettings`
  - `capsuleCollider.getSettings`
  - `capsuleCollider.setSettings`
  - `meshCollider.getSettings`
  - `meshCollider.setSettings`
  - `scene.getComponents`
  - `scene.destroyObject`
  - `scene.getComponentProperties`
  - `scene.setComponentProperties`
  - `scene.setTransform`
  - `scene.addComponent`
  - `scene.setSelection`
  - `scene.pingObject`
  - `scene.frameSelection`
  - `scene.frameObject`
  - `scene.createGameObject`
  - `scene.setParent`
  - `scene.duplicateObject`
  - `scene.renameObject`
  - `scene.setActive`
  - `prefab.instantiate`
  - `prefab.getSource`
  - `prefab.applyOverrides`
  - `prefab.revertOverrides`
  - `scene.findByTag`
  - `assets.find`
  - `assets.import`
  - `assets.ping`
  - `assets.reveal`

## Repository Layout
- `src/UnityMcp.Server` - MCP relay server
- `tests/UnityMcp.Server.Tests` - xUnit unit tests
- `unity/Packages/com.laimis.unitymcp` - Unity UPM package (Editor bridge)
- `docs/protocol.md` - MVP protocol/method notes

## Unity Version Target
- Primary target: Unity `6.3 LTS (6000.3.9f1)`

## Package-Only Install (Unity Project)
If you only want the Unity Editor bridge package in your Unity project, you do not need to move this whole repository into the Unity project.

Use one of these UPM methods:

### Option A. Local path package (recommended during development)
Edit your Unity project's `Packages/manifest.json` and add:

```json
{
  "dependencies": {
    "com.laimis.unitymcp": "file:./UnityMCP/unity/Packages/com.laimis.unitymcp"
  }
}
```

### Option B. Add package from disk (Unity Editor UI)
- Open Unity `Package Manager`
- Click `+`
- Select `Add package from disk...`
- Choose `./UnityMCP/unity/Packages/com.laimis.unitymcp/package.json`

Notes:
- Unity will also install the package dependency `com.unity.nuget.newtonsoft-json` automatically.
- This adds only the Unity package (`unity/Packages/com.laimis.unitymcp`), not the MCP server project.
- You still run the MCP server separately from this repo (`src/UnityMcp.Server`).

## Separate-Root Setup (Recommended)
Keep this repository and your Unity game project in separate root folders.

### 1. Add the local package to your Unity project
Edit your Unity project's `Packages/manifest.json` and add:

```json
{
  "dependencies": {
    "com.laimis.unitymcp": "file:./UnityMCP/unity/Packages/com.laimis.unitymcp"
  }
}
```

Alternative in Unity Editor:
- Package Manager -> `+` -> `Add package from disk...`
- Select `./UnityMCP/unity/Packages/com.laimis.unitymcp/package.json`

### 2. Run the server
```bash
dotnet run --project ./UnityMCP/src/UnityMcp.Server
```

The server listens on:
- `http://127.0.0.1:5001`
- `ws://127.0.0.1:5001/ws/cli`
- `ws://127.0.0.1:5001/ws/unity`

### 2.1 Codex `config.toml` (MCP server registration)
This server now exposes a Codex-compatible MCP HTTP endpoint at `http://127.0.0.1:5001/mcp`.

Codex registers MCP servers in `~/.codex/config.toml` using entries like:

```toml
[mcp_servers.someName]
url = "http://127.0.0.1:PORT/mcp"
```

Use this entry (example):

```toml
[mcp_servers.unitymcp]
url = "http://127.0.0.1:5001/mcp"
```

Notes:
- `/mcp` is the Codex MCP endpoint.
- `/ws/unity` is used by the Unity Editor package bridge.
- `/ws/cli` remains available for direct relay debugging but is not required for Codex MCP usage.

After editing `~/.codex/config.toml`, restart Codex so it reloads MCP server registrations.

### 3. Open the Unity project
The package auto-starts the Unity bridge on editor load and attempts to connect to `/ws/unity`.

Menu items:
- `Tools/Unity MCP/Connect`
- `Tools/Unity MCP/Disconnect`
- `Tools/Unity MCP/Settings`

### 4. Configure the Unity bridge endpoint (optional)
- Open `Tools/Unity MCP/Settings`
- Set the WebSocket endpoint (must be `ws://` or `wss://`)
- Click `Save`
- If already connected, use `Disconnect` then `Connect` to apply immediately

Default endpoint:
- `ws://127.0.0.1:5001/ws/unity`

## Manual Verification (MVP)
1. Start the server.
2. Open Unity and confirm the console logs `[UnityMCP] Connected`.
3. Verify MCP endpoint with HTTP JSON-RPC:
   - `initialize`
   - `prompts/list`
   - `resources/list`
   - `resources/templates/list`
   - `resources/read` (for example `unitymcp://server/info`)
   - `tools/list`
   - `tools/call` (for example `editor.getPlayModeState`)
4. Optionally verify direct relay WebSocket JSON-RPC on `/ws/cli` for debugging:
   - `ping`
   - `editor.getPlayModeState`
   - `editor.getConsoleLogs`
   - `editor.consoleTail`
   - `editor.enterPlayMode`
   - `editor.exitPlayMode`
   - `scene.getActiveScene`
   - `scene.listOpenScenes`
   - `scene.getSelection`
   - `scene.selectObject`
   - `scene.selectByPath`
   - `scene.findByPath`
   - `camera.getSettings`
   - `camera.setSettings`
   - `light.getSettings`
   - `light.setSettings`
   - `rigidbody.getSettings`
   - `rigidbody.setSettings`
   - `collider.getSettings`
   - `collider.setSettings`
   - `boxCollider.getSettings`
   - `boxCollider.setSettings`
   - `sphereCollider.getSettings`
   - `sphereCollider.setSettings`
   - `capsuleCollider.getSettings`
   - `capsuleCollider.setSettings`
   - `meshCollider.getSettings`
   - `meshCollider.setSettings`
   - `scene.getComponents`
   - `scene.destroyObject`
   - `scene.getComponentProperties`
   - `scene.setComponentProperties`
   - `scene.setTransform`
   - `scene.addComponent`
   - `scene.setSelection`
   - `scene.pingObject`
   - `scene.frameSelection`
   - `scene.frameObject`
   - `scene.createGameObject`
   - `scene.setParent`
   - `scene.duplicateObject`
   - `scene.renameObject`
   - `scene.setActive`
   - `prefab.instantiate`
   - `prefab.getSource`
   - `prefab.applyOverrides`
   - `prefab.revertOverrides`
   - `scene.findByTag`
   - `assets.find`
   - `assets.import`
   - `assets.ping`
   - `assets.reveal`
5. Verify a JSON-RPC response is returned.

Selection tool note:
- `scene.selectObject` / `scene.selectByPath` / `scene.setSelection` support optional `ping` and `focus` booleans to highlight and frame the selection in the Unity Editor.
- `scene.selectByPath` also supports optional `scenePath` for disambiguating duplicate hierarchy paths across open scenes.
- `scene.pingObject` pings/highlights an object by `instanceId` without changing selection.
- `scene.frameSelection` frames the current selection in the Scene view (best effort).
- `scene.frameObject` frames a specific scene object by `instanceId` (best effort) while preserving the previous selection when possible.
- `scene.getComponents` / `scene.getComponentProperties` / `scene.setComponentProperties` / `scene.setTransform` / `scene.addComponent` enable basic component inspection and safe scene-object edits from MCP.
- `scene.setParent` / `scene.duplicateObject` / `scene.renameObject` / `scene.setActive` add Undo-aware hierarchy editing and active-state workflows for scene objects.
- `scene.destroyObject` deletes a scene `GameObject` or `Component` with Undo support (destroying `Transform` directly is rejected).
- `prefab.instantiate` creates prefab instances by asset path, and `prefab.getSource` returns prefab source metadata for scene instances.
- `prefab.applyOverrides` / `prefab.revertOverrides` support deterministic `instanceRoot`, `object`, and `component` scopes; unsupported target/scope combinations fail explicitly instead of widening scope.
- Component property read/write uses a constrained serialized-property MVP (common simple types only; unsupported/non-editable properties fail clearly).
- `camera.*` / `light.*` / `rigidbody.*` / `collider.*` provide direct API convenience wrappers (`getSettings` / `setSettings`) so you do not need serialized `m_*` field names for common workflows.
- 2D physics convenience wrappers are also available:
  - `rigidbody2D.*`, `collider2D.*`
  - `boxCollider2D.*`, `circleCollider2D.*`, `capsuleCollider2D.*`
  - `polygonCollider2D.*`, `edgeCollider2D.*`, `compositeCollider2D.*`
- 2D subtype wrappers use conservative MVP write support for complex-shape colliders:
  - `polygonCollider2D.setSettings` currently supports base `Collider2D` fields only
  - `edgeCollider2D.setSettings` adds `edgeRadius`
  - `compositeCollider2D.setSettings` adds `geometryType` / `generationType` (safe subset)
- Common 2D joint convenience wrappers are available:
  - `hingeJoint2D.*`, `springJoint2D.*`, `distanceJoint2D.*`, `fixedJoint2D.*`
  - `sliderJoint2D.*`, `wheelJoint2D.*`, `targetJoint2D.*`
- `connectedBodyInstanceId` is supported on anchored Joint2D setters (`hingeJoint2D.*`, `springJoint2D.*`, `distanceJoint2D.*`, `fixedJoint2D.*`, `sliderJoint2D.*`, `wheelJoint2D.*`).
- `targetJoint2D.*` does not expose `connectedBodyInstanceId` because `TargetJoint2D` is target-driven rather than body-linked in the same way as the anchored joint families.
- Common 3D joint convenience wrappers are available:
  - `hingeJoint.*`, `springJoint.*`, `fixedJoint.*`, `characterJoint.*`, `configurableJoint.*`
- Anchored `Joint2D` setters and the supported 3D joint setters accept `connectedBodyInstanceId` as either a matching rigidbody/component target or `null` to clear the connection.
- Those same anchored-joint setters also accept `connectedAnchorMode` with `preserve`, `auto`, `zero`, and `matchAnchor` helper behavior.
- `characterJoint.*` exposes shared `Joint` fields plus swing axis, projection/preprocessing flags, and practical twist/swing spring-limit editing.
- `configurableJoint.*` now exposes a practical editing subset:
  - shared joint fields plus `secondaryAxis`
  - `configuredInWorldSpace` and `swapBodies`
  - linear/angular motion modes
  - linear/angular limits
  - target position and target velocities
  - `rotationDriveMode` plus x/y/z/angular/slerp drives
  - projection mode, distance, and angle
  - `targetRotation` helpers remain intentionally out of scope
- `collider.setSettings` currently supports BoxCollider-specific `center` / `size` fields in addition to base collider fields.
- Subtype collider wrappers are available for `BoxCollider`, `SphereCollider`, `CapsuleCollider`, and `MeshCollider`:
  - `boxCollider.*`, `sphereCollider.*`, `capsuleCollider.*`, `meshCollider.*`
- `meshCollider.setSettings` intentionally uses a safe subset in the MVP (no `sharedMesh` assignment yet).
- `assets.ping` / `assets.reveal` navigate the Project window to an asset path (`assets.reveal` also focuses Project window and selects the asset).

Example MCP `initialize` request:
```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "method": "initialize",
  "params": {
    "protocolVersion": "2025-06-18",
    "capabilities": {},
    "clientInfo": {
      "name": "manual-test",
      "version": "1.0"
    }
  }
}
```

Example MCP `tools/call` request:
```json
{
  "jsonrpc": "2.0",
  "id": 2,
  "method": "tools/call",
  "params": {
    "name": "editor.getPlayModeState",
    "arguments": {}
  }
}
```

Example MCP `resources/read` request:
```json
{
  "jsonrpc": "2.0",
  "id": 3,
  "method": "resources/read",
  "params": {
    "uri": "unitymcp://server/info"
  }
}
```

Current MCP resource URIs:
- `unitymcp://server/info`
- `unitymcp://unity/connection`
- `unitymcp://editor/playmode-state`
- `unitymcp://editor/console-logs`
- `unitymcp://scene/active`
- `unitymcp://scene/open-scenes`
- `unitymcp://scene/selection`
- `unitymcp://scene/selection/active`

Current MCP resource templates:
- `unitymcp://scene/find-by-tag/{tag}`
- `unitymcp://assets/find/{query}`
- `unitymcp://editor/console-tail/{afterSequence}`
- `unitymcp://scene/selection/index/{index}`

Resource query parameter support (MVP):
- `unitymcp://editor/console-logs?maxResults=20&includeStackTrace=true`
- `unitymcp://editor/console-logs?level=warning&level=error`
- `unitymcp://editor/console-logs?contains=MissingReference`
- `unitymcp://editor/console-tail/125?maxResults=10&includeStackTrace=false`
- `unitymcp://editor/console-tail/125?level=error&includeStackTrace=true`
- `unitymcp://editor/console-tail/125?contains=NullReference`
- `unitymcp://assets/find/Player?maxResults=25&folder=Assets/Prefabs&type=Prefab&label=Gameplay`
- `unitymcp://scene/selection/index/0`

Example direct relay `/ws/cli` request:
```json
{
  "jsonrpc": "2.0",
  "id": 2,
  "method": "scene.createGameObject",
  "params": {
    "name": "EnemySpawnPoint",
    "position": [0, 1.5, 0]
  }
}
```

## Development Notes
- Local-only MVP (no auth/TLS yet)
- Single Unity connection
- Domain reloads can interrupt the connection; the package reconnect loop handles recovery attempts
