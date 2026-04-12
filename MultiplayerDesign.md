# Coop Multiplayer System Change List

## Goal
Implement a low-security, Mirror-based coop multiplayer system for a Terraria/Rimworld/Minecraft-style survival game.
- One player acts as the Mirror host and owns the world state.
- The host saves all game data.
- Clients connect to the host via Mirror's transport and can join with a simple code or address.
- Clients only send local input and receive authoritative world updates.
- Low latency is prioritized over cheat resistance.

## Recommended networking approach
- Use Mirror for host/client networking and avoid custom low-level network plumbing unless necessary.
- Mirror provides `NetworkManager`, `NetworkIdentity`, `NetworkBehaviour`, transport, and connection lifecycle management.
- Use Mirror's default UDP-based transport or a compatible transport layer for low-latency direct connect.
- Implement a simple join-code UI that maps to a host address/port or a lightweight matchmaking relay if needed.
- Because anti-cheat is not needed, the host peer can remain authoritative and clients can be lightweight input/render peers.

## Core systems that must be modified

### 1. Network/session foundation
Files: new system, integrate into `Assets/Script/Game/Main.cs` / startup

- Use Mirror's `NetworkManager` and `NetworkDiscovery`/`NetworkAuthenticator` if you want session discovery or join codes.
- Host responsibilities:
  - start Mirror host and optionally advertise a session code
  - accept client connections through Mirror
  - assign client IDs and player ownership via Mirror connection IDs
  - broadcast authoritative updates using Mirror's `ClientRpc`/`SyncVar`/`NetworkMessage` APIs
- Client responsibilities:
  - connect to host using address/port or a join code resolved to that host
  - send local input/command messages to host via Mirror `Command`s or custom network messages
  - receive replicated world state and instantiate local entities using Mirror networked prefabs
- Add connection state and peer list.
- Add packet/message types or Mirror messages: `InputCommand`, `PlayerSpawn`, `WorldState`, `EntityUpdate`, `ChunkDiff`, `ItemSpawn`, `PlayerState`, `EnvironmentUpdate`, `SaveRequest`, `Disconnect`.

### 2. World loading & session state
Files: `Assets/Script/Game/Scene.cs`, `Assets/Script/Game/World/Saves.cs`, `Assets/Script/Game/World/World.cs`, `Assets/Script/Game/World/Gen.cs`

- Make world startup host-driven.
- Host loads/creates `Save.Inst`, generates `seed`, and builds deterministic chunk/world state.
- Clients receive session metadata: current world type, seed, time, day, player list, and chunk sync policy.
- Modify `Scene.LoadWorld()` / `Scene.SwitchWorld()` so clients do not independently instantiate a new world state.
- Add a multiplayer-specific branch in `Scene.Start()` to skip `Control.SetPlayer(0)` and instead let the local client pick their player.
- Ensure `World.Inst` references the host world state or replicated chunk state.
- Add `NetworkWorldState` or `WorldSnapshot` messages for chunk / terrain consistency.

### 3. Host-only save and persistence
Files: `Assets/Script/Game/World/Saves.cs`, `Assets/Script/Game/Main.cs`, `Assets/Script/Utility/Helper.cs`

- Keep saving and load logic on the host only.
- Clients should not call `Saves.SaveGame()` or `Saves.Quit()` except for local UI actions forwarded to host.
- Add a host-only autosave/tick save mechanism for multiplayer.
- Make `Save.Inst.players` the authoritative player list.
- On client connect, receive the host player list and map it to local `PlayerInfo`/`Machine` objects.
- `Helper.FileSave` and `FileLoad` remain for disk persistence on host only. Consider separate network serialization for in-flight data.

### 4. Player ownership, selection, and local input
Files: `Assets/Script/Game/Control.cs`, `Assets/Script/Game/Brain/Module/Info/PlayerInfo.cs`, `Assets/Script/Game/Brain/Machine/PlayerMachine.cs`, `Assets/Script/Game/Main.cs`

- Introduce a local-player/remote-player ownership model.
- `Control.SetPlayer(int)` becomes local-selection, not global host selection.
- Add a concept like `PlayerInfo.OwnerClientId` or `PlayerInfo.IsLocalPlayer`.
- `PlayerInfo.OnUpdate()` should only run local movement/input code when the player is owned locally.
- Remote players should be updated from host snapshots, not from local input.
- Clients should send `Move`, `Jump`, `Action`, `SwapCharacter`, `InventoryChange` commands to host, not modify `Main.PlayerInfo` directly.
- Host should validate client commands and apply them to the correct `PlayerInfo`.
- `Control.Update()` should only process input for the local player and then send network commands.
- Keep `SwapChar` logic but make it local-client-only with server-side permission if players may control multiple local avatars.

### 5. Entity spawning and replication
Files: `Assets/Script/Game/Entity/Entity.cs`, `Assets/Script/Game/Entity/EntityDynamicLoad.cs`, `Assets/Script/Game/Entity/EntityStaticLoad.cs`, `Assets/Script/Game/Brain/Module/Info/Info.cs`

- Add stable network IDs for every spawned entity.
- `Entity.Spawn(...)`, `Entity.SpawnItem(...)`, and `Entity.SpawnFromInfo(...)` must support network-aware spawn flows.
- Host should broadcast entity spawns and clients should instantiate matching networked entities.
- Clients should not independently spawn entities except for purely visual effects.
- Add a lightweight `NetworkIdentity` / `NetworkEntity` wrapper if using a custom system.
- Sync entity destroy/despawn events from host to clients.

### 6. Item drops, inventory, and storage sync
Files: `Assets/Script/Game/Player/Inventory.cs`, `Assets/Script/Game/Craft/ItemRecipe.cs`, `Assets/Script/Game/GUI/GUIChest.cs`, `Assets/Script/Game/Brain/Module/Info/Storage.cs`

- Host must own inventory modifications.
- Convert local item-drop actions into host commands (e.g. drop item, pick up item).
- Sync inventory slot changes and equipment changes from host to clients.
- Ensure `ItemRecipe` crafting and `Storage.AddItem` / `RemoveItem` calls run on host.
- Broadcast inventory updates or full inventory snapshots when necessary.
- Add remote inventory state refresh triggers on the client.

### 7. Block/terrain manipulation and map updates
Files: `Assets/Script/Game/Map/Terraform.cs`, `Assets/Script/Game/Brain/Module/Info/ConstructionInfo.cs`, `Assets/Script/Game/World/GenTaskEntity.cs`, `Assets/Script/Game/World/Gen.cs`

- All block placement/destruction commands must be sent to the host.
- Host applies changes to world data and broadcasts block updates/deltas.
- `Terraform` logic should not mutate local world state on clients first.
- Clients should apply received map updates and preserve deterministic generation if using on-demand chunk generation.
- If terrain is large, use chunk-based diff sync rather than replicating every block individually.

### 8. Environment, day/night, weather, and events
Files: `Assets/Script/Game/Environment.cs`, `Assets/Script/Game/RaidEvent.cs`, `Assets/Script/Game/Brain/Machine/PlanterMachine.cs`, `Assets/Script/Game/Collapse.cs`

- Host authoritatively simulates time, weather, raids, growth, collapse, and other global events.
- Clients receive the current `Save.Inst.time`, `Save.Inst.day`, and `Save.Inst.weather` values.
- Host broadcasts event spawns such as raid enemies and growth changes.
- Clients should not independently advance environment state.
- Ensure event systems query host player list / world state correctly rather than assuming `Main.PlayerInfo` is local-only.

### 9. AI and mob simulation
Files: `Assets/Script/Game/Brain/Module/PathingModule.cs`, `Assets/Script/Game/Entity/MobSpawner.cs`, `Assets/Script/Game/Brain/Module/Info/MobInfo.cs`, `Assets/Script/Game/Brain/Module/Info/DynamicInfo.cs`

- AI pathfinding and mob behavior must be host-side.
- `MobSpawner.Update()` should only run on host.
- Mobs should target host player state or `Save.Inst.players` and then broadcast their position/animation state to clients.
- Remove any `Main.PlayerInfo` assumption from mob targeting unless the target is a real player object in `Save.Inst.players`.
- Clients portray mob movement, hit reactions, and death from replicated updates.

### 10. Local UI and HUD
Files: `Assets/Script/Game/GUI/GUIMain.cs`, `Assets/Script/Game/GUI/GUIBar.cs`, `Assets/Script/Game/GUI/GUISave.cs`, `Assets/Script/Game/GUI/GUILoad.cs`

- Add multiplayer UI for join code, connection status, host/client mode, and player selection.
- Make HUD display the local owned player's state, not a globally selected player.
- When a client joins, allow them to choose a player from the host list.
- Sync inventory UI, health/hunger bars, and other local indicators from host state.
- `GUISave`/`GUILoad` should be host-only save/load screens or client reporting only.
- Add a simple session browser or join code entry panel.

### 11. Player switching and camera control
Files: `Assets/Script/Game/Control.cs`, `Assets/Script/Game/ViewPort.cs`, `Assets/Script/Game/GUI/GUIMain.cs`

- The local client should be able to pick any existing player on the host.
- `Control.SwapChar` should only change the local camera target, not the authoritative player if the client does not own that character.
- Hosts may allow local player switching if controlling multiple players; clients should request switch permission from host.
- `ViewPort` camera follow should track local owned player only.

### 12. Serialization and network data formats
Files: `Assets/Script/Utility/Helper.cs`, plus new network serializer helpers

- Do not use `BinaryFormatter` for network traffic.
- Add a custom serializer or use `Unity.Netcode` serialization utilities.
- Keep save serialization separate from network packet serialization.
- Add efficient typed packets for position, rotation, action commands, and entity updates.

### 13. Main update loop integration
Files: `Assets/Script/Game/Main.cs`

- Integrate the network update tick into `Main.Update()` or `Main.FixedUpdate()`.
- Ensure server/client logic branches are clear:
  - host: run full simulation, handle local input + remote commands, broadcast snapshots
  - client: run only local UI/render, process received updates, send input
- Do not let clients run host-only game systems like `MobSpawner.Update()`, `Terraform.Update()`, or `Environment.Update()`.

### 14. Debugging and local testing
Files: new debug systems, `Main.cs`, `NetworkManager`

- Add a local host/client mode for same-machine testing.
- Add verbose debug logging for join code, connection state, snapshot latency, and packet loss.
- Provide a fallback `offline` mode that uses current single-player flow.

## Suggested file-level modification map

- `Assets/Script/Game/Main.cs`
  - initialize Mirror `NetworkManager` and host/client mode
  - branch update loop for Mirror host/client (`NetworkServer.active` / `NetworkClient.isConnected`)
  - prevent host-only save calls from clients

- `Assets/Script/Game/Scene.cs`
  - make world load deterministic and host-driven
  - support client join after world initialized
  - assign local player after client joins

- `Assets/Script/Game/World/Saves.cs`
  - host-only save/load semantics
  - expose player list and world metadata for network sync

- `Assets/Script/Game/Control.cs`
  - local input -> network command conversion
  - remove direct action execution for remote players
  - add local ownership checks

- `Assets/Script/Game/Brain/Module/Info/PlayerInfo.cs`
  - separate local-control update from replicated state update
  - add ownership flags and network state fields

- `Assets/Script/Game/Entity/Entity.cs`, `EntityDynamicLoad.cs`, `EntityStaticLoad.cs`
  - add network identity and spawn replication
  - sync entity lifecycle across peers

- `Assets/Script/Game/World/World.cs`, `Gen.cs`, `GenTaskEntity.cs`
  - host authoritative world generation and chunk sync
  - avoid client-side independent generation except deterministic seeded setup

- `Assets/Script/Game/Map/Terraform.cs`
  - make terraform actions network-validated and broadcast

- `Assets/Script/Game/Player/Inventory.cs`, `Assets/Script/Game/Craft/ItemRecipe.cs`, `Assets/Script/Game/GUI/GUIChest.cs`
  - host-owned item/inventory operations with client sync

- `Assets/Script/Game/Environment.cs`, `RaidEvent.cs`, `Collapse.cs`, `PlanterMachine.cs`
  - host-side simulation only
  - broadcast environment values and event spawns

- `Assets/Script/Game/Entity/MobSpawner.cs`, `PathingModule.cs`
  - host-only AI processing
  - client-only display of replicated mob positions/animations

- `Assets/Script/Utility/Helper.cs`
  - add safe network serializers for packet formats

## Important behavior changes

- The host must remain authoritative for:
  - world terrain and chunk state
  - entity spawn/despawn
  - inventory and item drops
  - player state, health, equipment, and stats
  - global environment and events
- Clients should only own:
  - local input commands
  - camera/render state
  - local prediction of movement if desired for latency reduction
- The host saving every change means clients never write session data to disk.

## Practical multiplayer path

1. Build network manager and session join code.
2. Make host authoritative for scene/world startup.
3. Add local ownership in `PlayerInfo` and `Control`.
4. Replicate entity spawn/despawn and world changes.
5. Migrate inventory/crafting/item drops to host.
6. Migrate global environment and AI logic to host.
7. Add UI for join code and connection status.

## Summary
This game already has a clear single-player structure. The biggest changes are:
- add a host/client network layer,
- convert player input into host commands,
- make world/terrain/entity state authoritative on host,
- keep client code as render/input-only,
- save only on the host.

With these changes, the game can support direct-connect coop with low latency, a host-authoritative session, and a client experience where players can join and pick any character.
