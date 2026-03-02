# Unity MCP Protocol (MVP)

## Transport
- WebSocket (text frames)
- JSON-RPC 2.0 message envelopes

## Endpoints
- CLI client -> `ws://127.0.0.1:5001/ws/cli`
- Unity Editor plugin -> `ws://127.0.0.1:5001/ws/unity` (default; configurable in Unity via `Tools/Unity MCP/Settings`)

## Server-handled Methods
- `ping`
  - Returns server heartbeat metadata without contacting Unity.

## Unity-handled Methods (MVP)
- `ping`
  - Returns Unity heartbeat metadata (`unityVersion`).
- `editor.getPlayModeState`
  - Returns editor state:
    - `isPlaying`
    - `isPaused`
    - `isCompiling`
    - `isPlayingOrWillChangePlaymode`
- `editor.getConsoleLogs`
  - Returns a bounded snapshot of recent Unity logs captured by UnityMCP (in-memory buffer).
  - Params:
    - `maxResults` (optional, integer, `1..500`, default `100`)
    - `includeStackTrace` (optional, boolean, default `false`)
    - `contains` (optional, case-insensitive substring filter applied to log messages)
    - `levels` (optional, string array filter; allowed values: `info`, `warning`, `error`, `assert`, `exception`; aliases `log`->`info`, `warn`->`warning`)
  - Returns:
    - `bufferCapacity`
    - `totalBuffered`
    - `bufferStartSequence`
    - `latestSequence`
    - `nextAfterSequence`
    - `returnedCount`
    - `truncated`
    - `contains`
    - `items[]` (log entries with `sequence`, `timestampUtc`, `logType`, `level`, `message`, optional `stackTrace`)
- `editor.consoleTail`
  - Returns captured log entries after a given sequence cursor (poll-based tail).
  - Params:
    - `afterSequence` (required, non-negative integer)
    - `maxResults` (optional, integer, `1..500`, default `100`)
    - `includeStackTrace` (optional, boolean, default `false`)
    - `contains` (optional, case-insensitive substring filter applied to log messages)
    - `levels` (optional, string array filter; allowed values: `info`, `warning`, `error`, `assert`, `exception`; aliases `log`->`info`, `warn`->`warning`)
  - Returns:
    - `afterSequence`
    - `nextAfterSequence`
    - `cursorBehindBuffer`
    - `returnedCount`
    - `truncated`
    - `items[]` (log entries)
- `editor.enterPlayMode`
  - Requests transition into play mode.
  - Returns immediate editor state and whether a change was requested (`changed`).
- `editor.exitPlayMode`
  - Requests transition back to edit mode.
  - Returns immediate editor state and whether a change was requested (`changed`).
- `scene.createGameObject`
  - Creates a `GameObject` in the active scene.
  - Params:
    - `name` (optional, string)
    - `position` (optional, number array `[x, y, z]`)
  - Returns:
    - `instanceId`
    - `name`
    - `sceneName`
    - `scenePath`
    - `hierarchyPath`
    - `position`
- `scene.setParent`
  - Reparents a scene object under another scene object, or unparents it to the scene root.
  - Params:
    - `instanceId` (required, integer; `GameObject` or `Component`, resolved to owner `GameObject`)
    - `parentInstanceId` (optional, integer or `null`; `null` or omitted unparents to the scene root)
    - `keepWorldTransform` (optional, boolean, default `true`)
    - `ping` (optional, boolean)
    - `focus` (optional, boolean)
  - Notes:
    - Rejects self-parenting, cyclic parenting, and cross-scene parenting.
    - Selects the moved object after success.
  - Returns:
    - `target`
    - `parent`
    - `keepWorldTransform`
    - `selection`
    - `applied`
- `scene.duplicateObject`
  - Duplicates a scene object using Unity Undo-aware scene duplication semantics.
  - Params:
    - `instanceId` (required, integer; `GameObject` or `Component`, resolved to owner `GameObject`)
    - `select` (optional, boolean, default `true`)
    - `ping` (optional, boolean)
    - `focus` (optional, boolean)
  - Returns:
    - `source`
    - `duplicate`
    - `selection`
    - `applied`
- `scene.renameObject`
  - Renames a scene `GameObject`.
  - Params:
    - `instanceId` (required, integer; `GameObject` or `Component`, resolved to owner `GameObject`)
    - `name` (required, non-empty string)
  - Returns:
    - `target`
    - `previousName`
    - `currentName`
    - `applied`
- `scene.setActive`
  - Toggles active state of a scene object.
  - Params:
    - `instanceId` (required, integer; `GameObject` or `Component`, resolved to owner `GameObject`)
    - `active` (required, boolean)
  - Returns:
    - `target`
    - `activeSelf`
    - `activeInHierarchy`
    - `applied`
- `scene.getActiveScene`
  - Returns metadata for the currently active scene.
  - Returns:
    - `isValid`
    - `isLoaded`
    - `isActive`
    - `handle`
    - `buildIndex`
    - `name`
    - `path`
    - `rootCount`
- `scene.listOpenScenes`
  - Returns metadata for all currently open scenes.
  - Returns:
    - `count`
    - `activeSceneHandle`
    - `items[]` (scene summaries)
- `scene.getSelection`
  - Returns metadata for the current Unity Editor selection (`Selection.objects` / active selection).
  - Returns:
    - `count`
    - `activeObject`
    - `activeGameObject`
    - `items[]` (selection object summaries)
- `scene.selectObject`
  - Selects a single Unity object by `instanceId`.
  - Params:
    - `instanceId` (required, integer)
    - `ping` (optional, boolean)
    - `focus` (optional, boolean)
  - Returns:
    - same payload shape as `scene.getSelection`
- `scene.selectByPath`
  - Selects a single Unity scene object by hierarchy path (same format as returned `hierarchyPath`).
  - Params:
    - `path` (required, string; example `Cube/Main Camera`)
    - `scenePath` (optional, string; Unity scene path for disambiguation)
    - `ping` (optional, boolean)
    - `focus` (optional, boolean)
  - Returns:
    - same payload shape as `scene.getSelection`
- `scene.findByPath`
  - Finds Unity scene objects by hierarchy path without changing selection.
  - Params:
    - `path` (required, string; example `Cube/Main Camera`)
    - `scenePath` (optional, string; Unity scene path for scoping/disambiguation)
  - Returns:
    - `path`
    - `scenePath`
    - `count`
    - `items[]` (object summaries)
- `camera.getSettings`
  - Returns common Camera settings for a `Camera` component target (or a `GameObject` with a single Camera).
  - Params:
    - `instanceId` (required, integer)
  - Returns:
    - `target`
    - `component`
    - `settings`
- `camera.setSettings`
  - Mutates common Camera settings using direct Unity `Camera` APIs.
  - Params:
    - `instanceId` (required, integer)
    - optional: `enabled`, `orthographic`, `fieldOfView`, `orthographicSize`, `nearClipPlane`, `farClipPlane`, `depth`, `clearFlags`, `backgroundColor`
  - Returns:
    - `target`
    - `component`
    - `settings`
    - `applied`
- `light.getSettings`
  - Returns common Light settings for a `Light` component target (or a `GameObject` with a single Light).
  - Params:
    - `instanceId` (required, integer)
  - Returns:
    - `target`
    - `component`
    - `settings`
- `light.setSettings`
  - Mutates common Light settings using direct Unity `Light` APIs.
  - Params:
    - `instanceId` (required, integer)
    - optional: `enabled`, `type`, `color`, `intensity`, `range`, `spotAngle`, `shadows`
  - Notes:
    - `spotAngle` is only valid for Spot lights.
  - Returns:
    - `target`
    - `component`
    - `settings`
    - `applied`
- `rigidbody.getSettings`
  - Returns common 3D `Rigidbody` settings for a `Rigidbody` component target (or a `GameObject` with a single `Rigidbody`).
  - Params:
    - `instanceId` (required, integer)
  - Returns:
    - `target`
    - `component`
    - `settings`
- `rigidbody.setSettings`
  - Mutates common 3D `Rigidbody` settings using direct Unity `Rigidbody` APIs.
  - Params:
    - `instanceId` (required, integer)
    - optional: `mass`, `useGravity`, `isKinematic`, `detectCollisions`, `constraints`, `interpolation`, `collisionDetectionMode`
  - Returns:
    - `target`
    - `component`
    - `settings`
    - `applied`
- `collider.getSettings`
  - Returns common `Collider` settings for a `Collider` component target (or a `GameObject` with a single `Collider`).
  - Params:
    - `instanceId` (required, integer)
  - Returns:
    - `target`
    - `component`
    - `settings`
- `collider.setSettings`
  - Mutates common `Collider` settings (with BoxCollider-specific `center` / `size` in the MVP).
  - Params:
    - `instanceId` (required, integer)
    - optional: `enabled`, `isTrigger`, `contactOffset`, `center`, `size`
  - Notes:
    - `center` and `size` are only supported for `BoxCollider` in the MVP.
  - Returns:
    - `target`
    - `component`
    - `settings`
    - `applied`
- `boxCollider.getSettings` / `boxCollider.setSettings`
  - Typed convenience wrappers for `BoxCollider`.
  - `setSettings` supports base collider fields plus `center` and `size`.
- `sphereCollider.getSettings` / `sphereCollider.setSettings`
  - Typed convenience wrappers for `SphereCollider`.
  - `setSettings` supports base collider fields plus `center` and `radius`.
- `capsuleCollider.getSettings` / `capsuleCollider.setSettings`
  - Typed convenience wrappers for `CapsuleCollider`.
  - `setSettings` supports base collider fields plus `center`, `radius`, `height`, and `direction` (`X`/`Y`/`Z` or `0`/`1`/`2`).
- `meshCollider.getSettings` / `meshCollider.setSettings`
  - Typed convenience wrappers for `MeshCollider`.
  - `setSettings` uses a safe subset in the MVP:
    - base collider fields
    - `convex`
    - `cookingOptions`
  - `sharedMesh` assignment is intentionally out of scope for this slice.
- `rigidbody2D.getSettings` / `rigidbody2D.setSettings`
  - Typed convenience wrappers for `Rigidbody2D`.
  - `setSettings` supports common 2D body fields (for example `bodyType`, `simulated`, `mass`, `gravityScale`, `constraints`, `interpolation`, `collisionDetectionMode`, `sleepMode`).
- `collider2D.getSettings` / `collider2D.setSettings`
  - Typed convenience wrappers for base `Collider2D`.
  - `setSettings` supports base 2D collider fields:
    - `enabled`
    - `isTrigger`
    - `usedByEffector`
    - `offset`
    - `density`
- `boxCollider2D.getSettings` / `boxCollider2D.setSettings`
  - Typed convenience wrappers for `BoxCollider2D`.
  - `setSettings` supports base `Collider2D` fields plus `size` and `edgeRadius`.
- `circleCollider2D.getSettings` / `circleCollider2D.setSettings`
  - Typed convenience wrappers for `CircleCollider2D`.
  - `setSettings` supports base `Collider2D` fields plus `radius`.
- `capsuleCollider2D.getSettings` / `capsuleCollider2D.setSettings`
  - Typed convenience wrappers for `CapsuleCollider2D`.
  - `setSettings` supports base `Collider2D` fields plus `size` and `direction`.
- `polygonCollider2D.getSettings` / `polygonCollider2D.setSettings`
  - Typed convenience wrappers for `PolygonCollider2D`.
  - `setSettings` uses a safe subset in the MVP (base `Collider2D` fields only; no path/points editing).
- `edgeCollider2D.getSettings` / `edgeCollider2D.setSettings`
  - Typed convenience wrappers for `EdgeCollider2D`.
  - `setSettings` supports base `Collider2D` fields plus `edgeRadius`.
- `compositeCollider2D.getSettings` / `compositeCollider2D.setSettings`
  - Typed convenience wrappers for `CompositeCollider2D`.
  - `setSettings` uses a safe subset in the MVP:
    - base `Collider2D` fields
    - `geometryType`
    - `generationType`
- `hingeJoint2D.getSettings` / `hingeJoint2D.setSettings`
  - Typed convenience wrappers for `HingeJoint2D`.
  - `setSettings` supports base `Joint2D` fields plus `connectedBodyInstanceId`, `connectedAnchorMode`, `useConnectedAnchor`, `useMotor`, `motorSpeed`, `maxMotorTorque`, `useLimits`, `lowerAngle`, and `upperAngle`.
- `springJoint2D.getSettings` / `springJoint2D.setSettings`
  - Typed convenience wrappers for `SpringJoint2D`.
  - `setSettings` supports base `Joint2D` fields plus `connectedBodyInstanceId`, `connectedAnchorMode`, `autoConfigureDistance`, `distance`, `dampingRatio`, and `frequency`.
- `distanceJoint2D.getSettings` / `distanceJoint2D.setSettings`
  - Typed convenience wrappers for `DistanceJoint2D`.
  - `setSettings` supports base `Joint2D` fields plus `connectedBodyInstanceId`, `connectedAnchorMode`, `autoConfigureDistance`, `distance`, and `maxDistanceOnly`.
- `fixedJoint2D.getSettings` / `fixedJoint2D.setSettings`
  - Typed convenience wrappers for `FixedJoint2D`.
  - `setSettings` supports base `Joint2D` fields plus `dampingRatio`, `frequency`, `connectedBodyInstanceId`, and `connectedAnchorMode`.
- `sliderJoint2D.getSettings` / `sliderJoint2D.setSettings`
  - Typed convenience wrappers for `SliderJoint2D`.
  - `setSettings` supports base `Joint2D` fields plus `connectedBodyInstanceId`, `connectedAnchorMode`, `autoConfigureAngle`, `angle`, `useMotor`, `motorSpeed`, `maxMotorTorque`, `useLimits`, `lowerTranslation`, and `upperTranslation`.
- `wheelJoint2D.getSettings` / `wheelJoint2D.setSettings`
  - Typed convenience wrappers for `WheelJoint2D`.
  - `setSettings` supports base `Joint2D` fields plus `connectedBodyInstanceId`, `connectedAnchorMode`, `useMotor`, `motorSpeed`, `maxMotorTorque`, `suspensionDampingRatio`, `suspensionFrequency`, and `suspensionAngle`.
- `targetJoint2D.getSettings` / `targetJoint2D.setSettings`
  - Typed convenience wrappers for `TargetJoint2D`.
  - `setSettings` supports `anchor`, `autoConfigureTarget`, `target`, `maxForce`, `dampingRatio`, and `frequency`.
- `hingeJoint.getSettings` / `hingeJoint.setSettings`
  - Typed convenience wrappers for `HingeJoint`.
  - `setSettings` supports shared `Joint` fields plus `connectedBodyInstanceId`, `connectedAnchorMode`, spring, motor, and limit controls.
- `springJoint.getSettings` / `springJoint.setSettings`
  - Typed convenience wrappers for `SpringJoint`.
  - `setSettings` supports shared `Joint` fields plus `connectedBodyInstanceId`, `connectedAnchorMode`, `spring`, `damper`, `minDistance`, `maxDistance`, and `tolerance`.
- `fixedJoint.getSettings` / `fixedJoint.setSettings`
  - Typed convenience wrappers for `FixedJoint`.
  - `setSettings` supports the shared `Joint` field subset plus `connectedBodyInstanceId` and `connectedAnchorMode`.
- `characterJoint.getSettings` / `characterJoint.setSettings`
  - Typed convenience wrappers for `CharacterJoint`.
  - `setSettings` supports shared `Joint` fields plus `connectedBodyInstanceId`, `connectedAnchorMode`, `swingAxis`, `enableProjection`, `enablePreprocessing`, twist-limit springs, swing-limit springs, and twist/swing limits.
- `configurableJoint.getSettings` / `configurableJoint.setSettings`
  - Typed convenience wrappers for a practical `ConfigurableJoint` editing subset.
  - `setSettings` supports shared `Joint` fields plus `connectedBodyInstanceId`, `connectedAnchorMode`, `secondaryAxis`, `configuredInWorldSpace`, `swapBodies`, linear/angular motion modes, linear/angular limits, target position/velocities, `rotationDriveMode`, x/y/z/angular/slerp drives, and projection settings.
- For anchored `Joint2D` setters and supported 3D joint setters:
  - `connectedBodyInstanceId` accepts a matching rigidbody component id, a `GameObject` with exactly one matching rigidbody, or `null` to clear `connectedBody`.
  - `connectedAnchorMode` accepts `preserve`, `auto`, `zero`, or `matchAnchor`.
  - Setter responses include connection-helper readback in `applied`:
    - `connectedBodyInstanceId`
    - `connectedAnchor`
    - `connectedAnchorMode`
    - `autoConfigureConnectedAnchor`
- `scene.getComponents`
  - Returns component metadata for a target `GameObject` (or a `Component` target's owner `GameObject`).
  - Params:
    - `instanceId` (required, integer)
  - Returns:
    - `target`
    - `componentCount`
    - `missingComponentCount`
    - `items[]` (component summaries)
- `scene.destroyObject`
  - Destroys a scene `GameObject` or `Component` by `instanceId` using Unity Undo.
  - Params:
    - `instanceId` (required, integer)
  - Notes:
    - `Transform` component targets are rejected (destroy the `GameObject` instead).
    - Scene objects/components only (asset/prefab targets are rejected).
  - Returns:
    - `destroyed`
    - `destroyedKind` (`gameObject` or `component`)
    - `destroyedInstanceId`
    - `target` (pre-destroy object summary)
- `scene.getComponentProperties`
  - Reads a constrained set of serialized properties for a `Component` by `instanceId`.
  - Params:
    - `componentInstanceId` (required, integer; must reference a `Component`)
  - Returns:
    - `component`
    - `target`
    - `visiblePropertyCount`
    - `propertyCount` (supported/readable)
    - `unsupportedPropertyCount`
    - `properties` (property-path/value map)
    - `unsupportedProperties[]`
- `scene.setComponentProperties`
  - Writes a constrained set of serialized `Component` properties by property path.
  - Params:
    - `componentInstanceId` (required, integer; must reference a `Component`)
    - `properties` (required, object; property-path/value map)
  - Notes:
    - Rejects `m_Script`, non-editable properties, and unsupported property types.
    - Uses Unity Undo (`Undo.RecordObject`) before applying serialized changes.
  - Returns:
    - `component`
    - `target`
    - `appliedModifiedProperties`
    - `appliedCount`
    - `updated[]`
- `scene.setTransform`
  - Mutates basic transform properties on a `GameObject`/`Component` target.
  - Params:
    - `instanceId` (required, integer)
    - `position` (optional `[x,y,z]`, world-space)
    - `localPosition` (optional `[x,y,z]`)
    - `rotationEuler` (optional `[x,y,z]`, world-space euler)
    - `localRotationEuler` (optional `[x,y,z]`)
    - `localScale` (optional `[x,y,z]`)
  - Notes:
    - At least one transform field is required.
    - `position` and `localPosition` cannot be set together.
    - `rotationEuler` and `localRotationEuler` cannot be set together.
  - Returns:
    - `instanceId`
    - `target`
    - `transform` (world/local transform snapshot)
    - `applied`
- `scene.addComponent`
  - Adds a component to a `GameObject` (or a `Component` target's owner `GameObject`) by type name.
  - Params:
    - `instanceId` (required, integer)
    - `typeName` (required, string; short/full/assembly-qualified)
  - Returns:
    - `target`
    - `addedComponent`
    - `componentCount`
- `prefab.instantiate`
  - Instantiates a prefab asset into the active scene.
  - Params:
    - `assetPath` (required, string; prefab asset path under `Assets/`)
    - `parentInstanceId` (optional, integer or `null`)
    - `position` (optional `[x,y,z]`, world-space)
    - `rotationEuler` (optional `[x,y,z]`, world-space euler)
    - `select` (optional, boolean, default `true`)
    - `ping` (optional, boolean)
    - `focus` (optional, boolean)
  - Notes:
    - Parent targets must be scene objects in the active loaded scene.
  - Returns:
    - `instance`
    - `prefabSource`
    - `selection`
    - `applied`
- `prefab.getSource`
  - Resolves prefab source metadata for a prefab instance object in a scene.
  - Params:
    - `instanceId` (required, integer)
  - Returns:
    - `target`
    - `prefabInstanceStatus`
    - `prefabAssetType`
    - `instanceRoot`
    - `sourceAsset`
    - `nearestPrefabInstanceRoot`
    - `isOutermostPrefabInstanceRoot`
- `prefab.applyOverrides`
  - Applies prefab overrides from a scene prefab instance back to the source prefab asset.
  - Params:
    - `instanceId` (required, integer)
    - `scope` (optional, enum string: `instanceRoot`, `object`, `component`; default `instanceRoot`)
    - `componentInstanceId` (optional, integer; required for `component` scope unless `instanceId` already resolves to a `Component`)
  - Returns:
    - `target`
    - `scope`
    - `prefabSource`
    - `applied`
- `prefab.revertOverrides`
  - Reverts prefab overrides on a scene prefab instance.
  - Params:
    - same as `prefab.applyOverrides`
  - Returns:
    - `target`
    - `scope`
    - `prefabSource`
    - `applied`
- `scene.setSelection`
  - Replaces the current Unity Editor selection with the specified `instanceId`s.
  - Params:
    - `instanceIds` (required, integer array; duplicates ignored)
    - `ping` (optional, boolean)
    - `focus` (optional, boolean)
  - Returns:
    - same payload shape as `scene.getSelection`
- `scene.pingObject`
  - Pings/highlights a Unity object in the Editor without changing selection.
  - Params:
    - `instanceId` (required, integer)
  - Returns:
    - `pinged`
    - `instanceId`
    - `target` (object summary)
- `scene.frameSelection`
  - Best-effort frames the current selection in the Scene view.
  - Returns:
    - `framed`
    - `selectionCount`
    - `hasSceneSelection`
    - `sceneViewAvailable`
    - `activeObject`
- `scene.frameObject`
  - Best-effort frames a specific Unity scene object in the Scene view by `instanceId`.
  - Params:
    - `instanceId` (required, integer)
  - Returns:
    - `framed`
    - `selectionPreserved`
    - `sceneViewAvailable`
    - `hasSceneTarget`
    - `instanceId`
    - `target` (object summary)
- `scene.findByTag`
  - Finds active loaded `GameObject`s matching a tag.
  - Params:
    - `tag` (required, string)
  - Returns:
    - `tag`
    - `count`
    - `items[]` (object summaries)
- `assets.import`
  - Imports/reimports an existing Unity project asset or folder.
  - Params:
    - `assetPath` (required, string, must be project-relative under `Assets/`)
  - Returns:
    - `assetPath`
    - `guid`
    - `isFolder`
    - `exists`
    - `mainAssetType`
    - `mainAssetName`
    - `imported`
- `assets.ping`
  - Pings/highlights an existing asset in the Project window.
  - Params:
    - `assetPath` (required, string, must be project-relative under `Assets/`)
  - Returns:
    - `pinged`
    - `assetPath`
    - `guid`
    - `isFolder`
    - `target`
- `assets.reveal`
  - Focuses the Project window, selects the asset, and pings it.
  - Params:
    - `assetPath` (required, string, must be project-relative under `Assets/`)
  - Returns:
    - `revealed`
    - `focusedProjectWindow`
    - `assetPath`
    - `guid`
    - `isFolder`
    - `target`
- `assets.find`
  - Searches Unity assets using `AssetDatabase.FindAssets(query)`.
  - Params:
    - `query` (required, string)
    - `maxResults` (optional, integer, `1..500`, default `100`)
    - `searchInFolders` (optional, string array, Unity folders under `Assets/`)
    - `types` (optional, string array, appended as `t:<type>` filters)
    - `labels` (optional, string array, appended as `l:<label>` filters)
  - Returns:
    - `query`
    - `effectiveQuery`
    - `searchInFolders`
    - `types`
    - `labels`
    - `totalMatched`
    - `returnedCount`
    - `maxResults`
    - `truncated`
    - `items[]` (asset summaries)
      - each item additionally includes:
        - `assetImporterType`
        - `labels`
        - `fileExtension`

## Request Example
```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "method": "editor.getPlayModeState"
}
```

## Success Response Example
```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "result": {
    "isPlaying": false,
    "isPaused": false,
    "isCompiling": false
  }
}
```

## `scene.createGameObject` Example
Request:
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

`scene.frameSelection` success response (example):
```json
{
  "jsonrpc": "2.0",
  "id": 2,
  "result": {
    "instanceId": 12345,
    "name": "EnemySpawnPoint",
    "sceneName": "SampleScene",
    "scenePath": "Assets/Scenes/SampleScene.unity",
    "hierarchyPath": "EnemySpawnPoint",
    "position": [0, 1.5, 0]
  }
}
```

## `scene.setParent` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 21,
  "method": "scene.setParent",
  "params": {
    "instanceId": 45501,
    "parentInstanceId": 45458,
    "keepWorldTransform": true,
    "ping": true
  }
}
```

## `scene.duplicateObject` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 22,
  "method": "scene.duplicateObject",
  "params": {
    "instanceId": 45458,
    "select": true
  }
}
```

## `scene.renameObject` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 23,
  "method": "scene.renameObject",
  "params": {
    "instanceId": 45458,
    "name": "Enemy Spawn Root"
  }
}
```

## `scene.setActive` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 24,
  "method": "scene.setActive",
  "params": {
    "instanceId": 45458,
    "active": false
  }
}
```

## `prefab.instantiate` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 25,
  "method": "prefab.instantiate",
  "params": {
    "assetPath": "Assets/Prefabs/Enemy.prefab",
    "parentInstanceId": 45458,
    "position": [0, 1, 0],
    "rotationEuler": [0, 180, 0],
    "select": true
  }
}
```

## `prefab.getSource` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 26,
  "method": "prefab.getSource",
  "params": {
    "instanceId": 45520
  }
}
```

Success response (example):
```json
{
  "jsonrpc": "2.0",
  "id": 26,
  "result": {
    "target": {
      "instanceId": 45520,
      "name": "Enemy",
      "unityType": "UnityEngine.GameObject"
    },
    "prefabInstanceStatus": "Connected",
    "prefabAssetType": "Regular",
    "instanceRoot": {
      "instanceId": 45520,
      "name": "Enemy",
      "unityType": "UnityEngine.GameObject"
    },
    "sourceAsset": {
      "name": "Enemy",
      "assetPath": "Assets/Prefabs/Enemy.prefab",
      "guid": "0123456789abcdef0123456789abcdef",
      "prefabInstanceStatus": "Connected",
      "prefabAssetType": "Regular"
    },
    "nearestPrefabInstanceRoot": {
      "instanceId": 45520,
      "name": "Enemy",
      "unityType": "UnityEngine.GameObject"
    },
    "isOutermostPrefabInstanceRoot": true
  }
}
```

## `prefab.applyOverrides` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 27,
  "method": "prefab.applyOverrides",
  "params": {
    "instanceId": 45520,
    "scope": "component",
    "componentInstanceId": 45521
  }
}
```

## `prefab.revertOverrides` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 28,
  "method": "prefab.revertOverrides",
  "params": {
    "instanceId": 45520,
    "scope": "instanceRoot"
  }
}
```

## `editor.enterPlayMode` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 4,
  "method": "editor.enterPlayMode"
}
```

`scene.selectObject` / `scene.setSelection` success response (example):
```json
{
  "jsonrpc": "2.0",
  "id": 4,
  "result": {
    "isPlaying": false,
    "isPaused": false,
    "isCompiling": false,
    "isPlayingOrWillChangePlaymode": true,
    "requestedState": "playing",
    "changed": true
  }
}
```

## `editor.getConsoleLogs` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 11,
  "method": "editor.getConsoleLogs",
  "params": {
    "maxResults": 50,
    "includeStackTrace": false,
    "contains": "Reference",
    "levels": ["warning", "error"]
  }
}
```

Success response (example):
```json
{
  "jsonrpc": "2.0",
  "id": 11,
  "result": {
    "bufferCapacity": 2000,
    "totalBuffered": 12,
    "bufferStartSequence": 31,
    "latestSequence": 42,
    "afterSequence": null,
    "nextAfterSequence": 42,
    "cursorBehindBuffer": false,
    "returnedCount": 12,
    "truncated": false,
    "includeStackTrace": false,
    "contains": "Reference",
    "levels": ["warning", "error"],
    "items": [
      {
        "sequence": 42,
        "timestampUtc": "2026-02-23T21:55:00.0000000+00:00",
        "logType": "Warning",
        "level": "warning",
        "message": "Sample warning from Unity",
        "stackTrace": null
      }
    ]
  }
}
```

## `editor.consoleTail` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 12,
  "method": "editor.consoleTail",
  "params": {
    "afterSequence": 42,
    "maxResults": 20
  }
}
```

Success response (example):
```json
{
  "jsonrpc": "2.0",
  "id": 12,
  "result": {
    "bufferCapacity": 2000,
    "totalBuffered": 14,
    "bufferStartSequence": 31,
    "latestSequence": 44,
    "afterSequence": 42,
    "nextAfterSequence": 44,
    "cursorBehindBuffer": false,
    "returnedCount": 2,
    "truncated": false,
    "includeStackTrace": false,
    "items": [
      {
        "sequence": 43,
        "timestampUtc": "2026-02-23T21:55:05.0000000+00:00",
        "logType": "Log",
        "level": "info",
        "message": "Recompiling scripts...",
        "stackTrace": null
      },
      {
        "sequence": 44,
        "timestampUtc": "2026-02-23T21:55:06.0000000+00:00",
        "logType": "Error",
        "level": "error",
        "message": "MissingReferenceException...",
        "stackTrace": null
      }
    ]
  }
}
```

## `editor.exitPlayMode` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 5,
  "method": "editor.exitPlayMode"
}
```

## `scene.findByTag` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 3,
  "method": "scene.findByTag",
  "params": {
    "tag": "Enemy"
  }
}
```

## `scene.getActiveScene` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 7,
  "method": "scene.getActiveScene"
}
```

Success response (example):
```json
{
  "jsonrpc": "2.0",
  "id": 7,
  "result": {
    "isValid": true,
    "isLoaded": true,
    "isActive": true,
    "handle": 123,
    "buildIndex": 0,
    "name": "TestScene",
    "path": "Assets/Scenes/TestScene.unity",
    "rootCount": 4
  }
}
```

## `scene.listOpenScenes` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 8,
  "method": "scene.listOpenScenes"
}
```

Success response (example):
```json
{
  "jsonrpc": "2.0",
  "id": 8,
  "result": {
    "count": 2,
    "activeSceneHandle": 123,
    "items": [
      {
        "isValid": true,
        "isLoaded": true,
        "isActive": true,
        "handle": 123,
        "buildIndex": 0,
        "name": "TestScene",
        "path": "Assets/Scenes/TestScene.unity",
        "rootCount": 10
      },
      {
        "isValid": true,
        "isLoaded": true,
        "isActive": false,
        "handle": 124,
        "buildIndex": 1,
        "name": "UI",
        "path": "Assets/Scenes/UI.unity",
        "rootCount": 3
      }
    ]
  }
}
```

## `scene.getSelection` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 9,
  "method": "scene.getSelection"
}
```

## `scene.selectObject` Example
Optional params:
- `ping` (`boolean`): highlights the selected object in the Unity Editor.
- `focus` (`boolean`): best-effort frames the selection in the Scene view.

Request:
```json
{
  "jsonrpc": "2.0",
  "id": 13,
  "method": "scene.selectObject",
  "params": {
    "instanceId": 45458,
    "ping": true,
    "focus": true
  }
}
```

## `scene.selectByPath` Example
Optional params:
- `scenePath` (`string`): disambiguates duplicate hierarchy paths across open scenes.
- `ping` (`boolean`): highlights the selected object in the Unity Editor.
- `focus` (`boolean`): best-effort frames the selection in the Scene view.

Request:
```json
{
  "jsonrpc": "2.0",
  "id": 13,
  "method": "scene.selectByPath",
  "params": {
    "path": "Cube/Main Camera",
    "scenePath": "Assets/_Game/Scenes/TestScene.unity",
    "ping": true,
    "focus": true
  }
}
```

## `scene.findByPath` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 18,
  "method": "scene.findByPath",
  "params": {
    "path": "Cube/Main Camera",
    "scenePath": "Assets/_Game/Scenes/TestScene.unity"
  }
}
```

## `camera.getSettings` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 19,
  "method": "camera.getSettings",
  "params": {
    "instanceId": 45444
  }
}
```

## `camera.setSettings` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 20,
  "method": "camera.setSettings",
  "params": {
    "instanceId": 45444,
    "fieldOfView": 55,
    "nearClipPlane": 0.2
  }
}
```

## `light.getSettings` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 20,
  "method": "light.getSettings",
  "params": {
    "instanceId": 60001
  }
}
```

## `light.setSettings` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 20,
  "method": "light.setSettings",
  "params": {
    "instanceId": 60001,
    "intensity": 2.5,
    "range": 15
  }
}
```

## `rigidbody.getSettings` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 20,
  "method": "rigidbody.getSettings",
  "params": {
    "instanceId": 70001
  }
}
```

## `rigidbody.setSettings` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 20,
  "method": "rigidbody.setSettings",
  "params": {
    "instanceId": 70001,
    "isKinematic": true,
    "useGravity": false
  }
}
```

## `collider.getSettings` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 20,
  "method": "collider.getSettings",
  "params": {
    "instanceId": 70002
  }
}
```

## `collider.setSettings` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 20,
  "method": "collider.setSettings",
  "params": {
    "instanceId": 70002,
    "isTrigger": true,
    "center": [0, 0.5, 0],
    "size": [1, 2, 1]
  }
}
```

## `boxCollider.getSettings` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 20,
  "method": "boxCollider.getSettings",
  "params": {
    "instanceId": 70002
  }
}
```

## `boxCollider.setSettings` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 20,
  "method": "boxCollider.setSettings",
  "params": {
    "instanceId": 70002,
    "center": [0, 0.5, 0],
    "size": [1, 2, 1]
  }
}
```

## `sphereCollider.getSettings` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 20,
  "method": "sphereCollider.getSettings",
  "params": {
    "instanceId": 70003
  }
}
```

## `sphereCollider.setSettings` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 20,
  "method": "sphereCollider.setSettings",
  "params": {
    "instanceId": 70003,
    "radius": 1.5
  }
}
```

## `capsuleCollider.getSettings` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 20,
  "method": "capsuleCollider.getSettings",
  "params": {
    "instanceId": 70004
  }
}
```

## `capsuleCollider.setSettings` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 20,
  "method": "capsuleCollider.setSettings",
  "params": {
    "instanceId": 70004,
    "radius": 0.75,
    "height": 2.0,
    "direction": "Z"
  }
}
```

## `meshCollider.getSettings` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 20,
  "method": "meshCollider.getSettings",
  "params": {
    "instanceId": 70005
  }
}
```

## `meshCollider.setSettings` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 20,
  "method": "meshCollider.setSettings",
  "params": {
    "instanceId": 70005,
    "convex": true
  }
}
```

## `rigidbody2D.getSettings` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 20,
  "method": "rigidbody2D.getSettings",
  "params": {
    "instanceId": 71001
  }
}
```

## `rigidbody2D.setSettings` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 20,
  "method": "rigidbody2D.setSettings",
  "params": {
    "instanceId": 71001,
    "bodyType": "Dynamic",
    "gravityScale": 1.5,
    "collisionDetectionMode": "Continuous"
  }
}
```

## `collider2D.getSettings` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 20,
  "method": "collider2D.getSettings",
  "params": {
    "instanceId": 71002
  }
}
```

## `collider2D.setSettings` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 20,
  "method": "collider2D.setSettings",
  "params": {
    "instanceId": 71002,
    "isTrigger": true,
    "offset": [0.25, 0.0]
  }
}
```

## `boxCollider2D.setSettings` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 20,
  "method": "boxCollider2D.setSettings",
  "params": {
    "instanceId": 71003,
    "size": [2.0, 3.0],
    "edgeRadius": 0.1
  }
}
```

## `circleCollider2D.setSettings` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 20,
  "method": "circleCollider2D.setSettings",
  "params": {
    "instanceId": 71004,
    "radius": 0.75
  }
}
```

## `capsuleCollider2D.setSettings` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 20,
  "method": "capsuleCollider2D.setSettings",
  "params": {
    "instanceId": 71005,
    "size": [1.0, 2.5],
    "direction": "Vertical"
  }
}
```

## `polygonCollider2D.setSettings` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 20,
  "method": "polygonCollider2D.setSettings",
  "params": {
    "instanceId": 71006,
    "usedByEffector": false
  }
}
```

## `edgeCollider2D.setSettings` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 20,
  "method": "edgeCollider2D.setSettings",
  "params": {
    "instanceId": 71007,
    "edgeRadius": 0.05
  }
}
```

## `compositeCollider2D.setSettings` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 20,
  "method": "compositeCollider2D.setSettings",
  "params": {
    "instanceId": 71008,
    "geometryType": "Outlines",
    "generationType": "Manual"
  }
}
```

## `hingeJoint2D.setSettings` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 20,
  "method": "hingeJoint2D.setSettings",
  "params": {
    "instanceId": 72001,
    "useMotor": true,
    "motorSpeed": 120.0,
    "maxMotorTorque": 15.0,
    "useLimits": true,
    "lowerAngle": -30.0,
    "upperAngle": 45.0
  }
}
```

## `springJoint2D.setSettings` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 20,
  "method": "springJoint2D.setSettings",
  "params": {
    "instanceId": 72002,
    "distance": 2.0,
    "dampingRatio": 0.5,
    "frequency": 4.0
  }
}
```

## `distanceJoint2D.setSettings` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 20,
  "method": "distanceJoint2D.setSettings",
  "params": {
    "instanceId": 72003,
    "distance": 3.0,
    "maxDistanceOnly": true
  }
}
```

## `fixedJoint2D.setSettings` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 20,
  "method": "fixedJoint2D.setSettings",
  "params": {
    "instanceId": 72004,
    "dampingRatio": 0.25,
    "frequency": 5.0
  }
}
```

## `hingeJoint.setSettings` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 20,
  "method": "hingeJoint.setSettings",
  "params": {
    "instanceId": 73001,
    "useMotor": true,
    "motorTargetVelocity": 90.0,
    "motorForce": 20.0,
    "useLimits": true,
    "minLimit": -45.0,
    "maxLimit": 45.0
  }
}
```

## `springJoint.setSettings` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 20,
  "method": "springJoint.setSettings",
  "params": {
    "instanceId": 73002,
    "spring": 25.0,
    "damper": 3.0,
    "minDistance": 0.25,
    "maxDistance": 2.0
  }
}
```

## `fixedJoint.setSettings` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 20,
  "method": "fixedJoint.setSettings",
  "params": {
    "instanceId": 73003,
    "connectedBodyInstanceId": 73010,
    "breakForce": 100.0
  }
}
```

## `characterJoint.setSettings` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 21,
  "method": "characterJoint.setSettings",
  "params": {
    "instanceId": 73004,
    "connectedBodyInstanceId": 73010,
    "connectedAnchorMode": "matchAnchor",
    "enableProjection": true,
    "twistLimitSpring": {
      "spring": 12.0,
      "damper": 1.5
    },
    "lowTwistLimit": {
      "limit": -25.0,
      "contactDistance": 0.05
    },
    "swing1Limit": {
      "limit": 35.0
    }
  }
}
```

## `configurableJoint.setSettings` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 22,
  "method": "configurableJoint.setSettings",
  "params": {
    "instanceId": 73005,
    "configuredInWorldSpace": true,
    "xMotion": "Locked",
    "yMotion": "Limited",
    "zMotion": "Free",
    "angularXMotion": "Limited",
    "angularYMotion": "Locked",
    "angularZMotion": "Locked",
    "linearLimit": {
      "limit": 0.5,
      "contactDistance": 0.02
    },
    "targetPosition": [0.0, 0.5, 0.0],
    "rotationDriveMode": "Slerp",
    "slerpDrive": {
      "positionSpring": 50.0,
      "positionDamper": 4.0,
      "maximumForce": 200.0
    },
    "projectionMode": "PositionAndRotation",
    "projectionDistance": 0.1,
    "projectionAngle": 10.0
  }
}
```

## `scene.getComponents` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 21,
  "method": "scene.getComponents",
  "params": {
    "instanceId": 45444
  }
}
```

## `scene.destroyObject` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 22,
  "method": "scene.destroyObject",
  "params": {
    "instanceId": 45458
  }
}
```

## `scene.getComponentProperties` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 23,
  "method": "scene.getComponentProperties",
  "params": {
    "componentInstanceId": 45448
  }
}
```

## `scene.setComponentProperties` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 24,
  "method": "scene.setComponentProperties",
  "params": {
    "componentInstanceId": 45448,
    "properties": {
      "m_Enabled": true
    }
  }
}
```

## `scene.setTransform` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 25,
  "method": "scene.setTransform",
  "params": {
    "instanceId": 45444,
    "position": [7.24, 10.02, -7.99],
    "localScale": [1, 1, 1]
  }
}
```

## `scene.addComponent` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 26,
  "method": "scene.addComponent",
  "params": {
    "instanceId": 45458,
    "typeName": "BoxCollider"
  }
}
```

## `scene.setSelection` Example
Optional params:
- `ping` (`boolean`): highlights the active selected object in the Unity Editor.
- `focus` (`boolean`): best-effort frames the selection in the Scene view.

Request:
```json
{
  "jsonrpc": "2.0",
  "id": 14,
  "method": "scene.setSelection",
  "params": {
    "instanceIds": [45458, 45459],
    "ping": true,
    "focus": true
  }
}
```

## `scene.pingObject` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 15,
  "method": "scene.pingObject",
  "params": {
    "instanceId": 45458
  }
}
```

## `scene.frameSelection` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 16,
  "method": "scene.frameSelection"
}
```

`scene.frameSelection` success response (example):
```json
{
  "jsonrpc": "2.0",
  "id": 16,
  "result": {
    "framed": true,
    "selectionCount": 1,
    "hasSceneSelection": true,
    "sceneViewAvailable": true,
    "activeObject": {
      "instanceId": 45458,
      "name": "Cube",
      "unityType": "UnityEngine.GameObject"
    }
  }
}
```

## `scene.frameObject` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 17,
  "method": "scene.frameObject",
  "params": {
    "instanceId": 45458
  }
}
```

`scene.frameObject` success response (example):
```json
{
  "jsonrpc": "2.0",
  "id": 17,
  "result": {
    "framed": true,
    "selectionPreserved": true,
    "sceneViewAvailable": true,
    "hasSceneTarget": true,
    "instanceId": 45458,
    "target": {
      "instanceId": 45458,
      "name": "Cube",
      "unityType": "UnityEngine.GameObject"
    }
  }
}
```

`scene.selectObject` / `scene.selectByPath` / `scene.setSelection` success response (example):
```json
{
  "jsonrpc": "2.0",
  "id": 9,
  "result": {
    "count": 2,
    "activeObject": {
      "instanceId": 34567,
      "name": "Main Camera",
      "unityType": "UnityEngine.GameObject",
      "isPersistent": false,
      "assetPath": null,
      "sceneName": "TestScene",
      "scenePath": "Assets/Scenes/TestScene.unity",
      "hierarchyPath": "Cube/Main Camera",
      "activeSelf": true,
      "activeInHierarchy": true,
      "componentType": null
    },
    "activeGameObject": {
      "instanceId": 34567,
      "name": "Main Camera",
      "unityType": "UnityEngine.GameObject",
      "isPersistent": false,
      "assetPath": null,
      "sceneName": "TestScene",
      "scenePath": "Assets/Scenes/TestScene.unity",
      "hierarchyPath": "Cube/Main Camera",
      "activeSelf": true,
      "activeInHierarchy": true,
      "componentType": null
    },
    "items": [
      {
        "instanceId": 34567,
        "name": "Main Camera",
        "unityType": "UnityEngine.GameObject",
        "isPersistent": false,
        "assetPath": null,
        "sceneName": "TestScene",
        "scenePath": "Assets/Scenes/TestScene.unity",
        "hierarchyPath": "Cube/Main Camera",
        "activeSelf": true,
        "activeInHierarchy": true,
        "componentType": null
      },
      {
        "instanceId": 45678,
        "name": "PlayerController",
        "unityType": "UnityEngine.MonoScript",
        "isPersistent": true,
        "assetPath": "Assets/Scripts/PlayerController.cs",
        "sceneName": null,
        "scenePath": null,
        "hierarchyPath": null,
        "activeSelf": null,
        "activeInHierarchy": null,
        "componentType": null
      }
    ]
  }
}
```

## `assets.import` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 6,
  "method": "assets.import",
  "params": {
    "assetPath": "Assets/Textures/Test.png"
  }
}
```

## `assets.ping` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 19,
  "method": "assets.ping",
  "params": {
    "assetPath": "Assets/_Game/Scenes/TestScene.unity"
  }
}
```

## `assets.reveal` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 20,
  "method": "assets.reveal",
  "params": {
    "assetPath": "Assets/_Game/Scenes/TestScene.unity"
  }
}
```

## `assets.find` Example
Request:
```json
{
  "jsonrpc": "2.0",
  "id": 10,
  "method": "assets.find",
  "params": {
    "query": "Player",
    "maxResults": 25,
    "searchInFolders": ["Assets/Prefabs"],
    "types": ["Prefab"],
    "labels": ["Gameplay"]
  }
}
```

Success response (example):
```json
{
  "jsonrpc": "2.0",
  "id": 10,
  "result": {
    "query": "Player",
    "effectiveQuery": "Player t:Prefab l:Gameplay",
    "searchInFolders": ["Assets/Prefabs"],
    "types": ["Prefab"],
    "labels": ["Gameplay"],
    "totalMatched": 3,
    "returnedCount": 3,
    "maxResults": 25,
    "truncated": false,
    "items": [
      {
        "guid": "0123456789abcdef0123456789abcdef",
        "assetPath": "Assets/Prefabs/Player.prefab",
        "isFolder": false,
        "mainAssetType": "UnityEngine.GameObject",
        "mainAssetName": "Player",
        "assetImporterType": "UnityEditor.ModelImporter",
        "labels": ["Gameplay"],
        "fileExtension": ".prefab"
      }
    ]
  }
}
```

Success response (example):
```json
{
  "jsonrpc": "2.0",
  "id": 6,
  "result": {
    "assetPath": "Assets/Textures/Test.png",
    "guid": "0123456789abcdef0123456789abcdef",
    "isFolder": false,
    "exists": true,
    "mainAssetType": "UnityEngine.Texture2D",
    "mainAssetName": "Test",
    "imported": true
  }
}
```

## Error Response Example
```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "error": {
    "code": -32001,
    "message": "Unity is not connected."
  }
}
```

## Current Constraints
- Requests require a string or numeric `id` (notifications are not handled yet).
- Single Unity Editor connection is supported.
- Request forwarding is serialized and correlated by JSON-RPC `id`.
- No authentication yet (local-only MVP).
- `scene.findByTag` currently returns active objects only (`GameObject.FindGameObjectsWithTag`).
- Play mode control responses are immediate and do not wait for transition completion.
- `assets.import` currently supports only existing project-relative paths under `Assets/`.
- `scene.setParent` rejects cross-scene parenting, self-parenting, and cyclic parenting.
- `prefab.instantiate` currently targets prefab assets by `assetPath` and only supports parenting into the active loaded scene.
- `prefab.applyOverrides` / `prefab.revertOverrides` support only explicit `instanceRoot`, `object`, and `component` scopes; unsupported target/scope combinations fail instead of widening scope.
- Console log history is captured in-memory by the Unity package (bounded buffer) and resets on domain reload/editor restart.

## Resource URI Query Parameters (MVP)
- `resources/read` supports optional query params for console resources:
  - `unitymcp://editor/console-logs?maxResults=20&includeStackTrace=true`
  - `unitymcp://editor/console-tail/125?maxResults=10&includeStackTrace=false`
  - repeated `level` values are supported and normalized:
    - `unitymcp://editor/console-logs?level=warning&level=error`
    - `unitymcp://editor/console-tail/125?level=error`
  - `contains` is supported for case-insensitive message substring filtering:
    - `unitymcp://editor/console-logs?contains=MissingReference`
    - `unitymcp://editor/console-tail/125?contains=NullReference`
- `unitymcp://assets/find/{query}` resource template supports optional query params:
  - `maxResults` (single integer)
  - repeated `folder` (maps to `searchInFolders`)
  - repeated `type` (maps to `types`)
  - repeated `label` (maps to `labels`)
- Additional selection resources/templates:
  - `unitymcp://scene/selection/active`
  - `unitymcp://scene/selection/index/{index}` (`index` is 0-based)
- Invalid `resources/read` resource parameters now return JSON-RPC `InvalidParams` with `error.data` including:
  - `source = "resources/read"`
  - `resourceUri`
  - `parameter` (when applicable)
