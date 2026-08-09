# Map Display System

A per-world 2D map showing an aerial view of the terrain plus structure markers,
revealed gradually as the player explores (fog of war).

---

## 1. Overview

Each world gets a persistent 2D map. The map is a top-down (XZ) view where each
pixel represents one world column, colored by the surface block. Structures
(trees, chests, workbenches, etc.) are drawn as small icons on top. Areas the
player hasn't visited are hidden (dark/fogged) and are revealed in a radius
around the player as they move.

```
┌─────────────────────────────────────────────┐
│  Map (M)                                    │
│  ┌───────────────────────────────────────┐  │
│  │  ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░  │  │  ░ = unexplored (fog)
│  │  ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░  │  │  █ = terrain (surface color)
│  │  ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░  │  │  ▲ = structure marker
│  │  ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░  │  │  ● = player
│  │  ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░  │  │
│  │  ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░  │  │
│  │  ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░  │  │
│  └───────────────────────────────────────┘  │
└─────────────────────────────────────────────┘
```

---

## 2. New Files

### 2.1 `Assets/Script/Game/Map/WorldMap.cs` — core data & logic

Holds the per-world map state and does the heavy lifting.

```csharp
[Serializable]
public class MapMarker
{
    public ID id;          // structure type (tree, chest, ...)
    public int x, z;       // world column
}

[Serializable]
public class WorldMap
{
    public byte[] Explored;          // per-column explored flag (x*z), 0/1
    public List<MapMarker> Markers;  // cached structure markers
    [NonSerialized] public Texture2D Texture; // generated map texture (cached)
    [NonSerialized] public bool Dirty;        // texture needs regeneration
}
```

Key responsibilities:
- **`Reveal(int x, int z, int radius)`** — marks columns within `radius` of a
  world position as explored, sets `Dirty = true`.
- **`GetSurfaceBlock(World world, int x, int z)`** — returns the topmost
  non-air block ID for a column (walks down from the top of the world).
- **`BuildMarkers(World world)`** — scans every chunk's `StaticEntity` list and
  records structure positions as `MapMarker`s. Called once per world (lazily on
  first map open) and cached.
- **`RegenerateTexture(World world)`** — builds a `Texture2D` of size
  `(world.Size.x*ChunkSize, world.Size.z*ChunkSize)`. For each column:
  - If unexplored → fog color (dark).
  - Else → color from `GetSurfaceBlock` via a block→color table.
  - Then blits structure markers and the player marker as small colored pixels.
- **`GetColor(ID block)`** — static block→color mapping (dirt=tan, forest=green,
  sand=yellow, stone=gray, water=blue, etc.).

### 2.2 `Assets/Script/Game/GUI/GUIMap.cs` — the UI panel

A `GUI` subclass that displays the map texture and handles input.

- Builds a panel (parented under `Main.GUIInv` like other GUIs) containing a
  `RawImage`/`Image` showing `WorldMap.Texture`.
- **`Update()`** — refreshes the texture when `Dirty`, and re-centers the player
  marker each frame.
- **Toggle** — opens/closes with a key (see §4.3).
- While open, hides the normal inventory GUI (or overlays it) and pauses
  gameplay input so the player can pan/zoom the map.

---

## 3. Modified Files

### 3.1 `Assets/Script/Game/World/World.cs`
- Add a serialized field: `public WorldMap Map;`
- Initialize it in the constructor: `Map = new WorldMap { Explored = new byte[Size.x*ChunkSize * Size.z*ChunkSize] };`
- (BinaryFormatter serializes `World`, so `WorldMap` must be `[Serializable]`
  and its `Texture2D`/`Dirty` fields marked `[NonSerialized]`.)

### 3.2 `Assets/Script/Game/Scene.cs` (or a new `MapTracker`)
- Each frame (in `Scene.Update()`), reveal the map around the local player:
  ```csharp
  Vector3 p = Main.Player.transform.position;
  World.Inst.Map.Reveal((int)p.x, (int)p.z, RevealRadius);
  ```
- Only reveal on the host (authoritative) and only when the player exists.

### 3.3 `Assets/Script/Game/Control.cs`
- Add a key binding: `public readonly ControlKey Map = new (KeyCode.M);`

### 3.4 `Assets/Script/Game/GUI/GUIMain.cs`
- Add `public static GUIMap Map;`
- Initialize it in `Initialize()`.
- Call `Map.Update()` in `Update()`.
- Handle the `Map` key toggle.

### 3.5 `Assets/Script/Game/Main.cs`
- Add a reference to the map panel GameObject (e.g. `GUIMapPanel`) found under
  the GUI hierarchy, mirroring how `GUIInv`/`GUIInfoPanel` are wired up.

---

## 4. Design Details

### 4.1 Aerial view (terrain color)
The world is 3D; the map is 2D. For each `(x, z)` column we find the **surface**
block — the highest non-air block — and color the pixel by its block type. This
gives a clean top-down terrain view. Because world gen is deterministic and
chunks are stored, this is computed once and cached in `WorldMap.Texture`.

### 4.2 Gradual exploration (fog of war)
- `WorldMap.Explored` is a per-column byte mask persisted with the world.
- `Reveal()` marks a circular radius around the player as explored.
- The texture is regenerated (or partially updated) only when `Dirty` is set,
  so we don't rebuild it every frame.
- Unexplored columns render as dark fog; explored columns show terrain.

### 4.3 Structure markers
- Structures live in `chunk.StaticEntity` (each `Info` has an `id`).
- `BuildMarkers()` scans these once per world and caches `MapMarker`s.
- Markers are drawn as small colored pixels/icons on the texture.
- Only markers in explored columns are shown (so you can't see structures you
  haven't discovered).

### 4.4 Player marker
- Drawn as a distinct color (e.g. white) at the player's current column.
- Updated every frame while the map is open.

### 4.5 Performance
- Terrain texture is generated once per world and cached.
- Exploration only marks `Dirty` when new columns are revealed (not every frame).
- Structure scanning is done once per world, not per frame.
- Map texture size is bounded by world size (e.g. 25×15×25 chunks → 375×375 px).

---

## 5. Implementation Order

1. **`WorldMap.cs`** — data structures, `Reveal`, `GetSurfaceBlock`, block→color
   table, `BuildMarkers`, `RegenerateTexture`.
2. **`World.cs`** — add `Map` field + init.
3. **`Scene.cs`** — reveal around player each frame.
4. **`Control.cs`** — add `Map` key.
5. **`GUIMap.cs`** — UI panel + texture display + player marker.
6. **`GUIMain.cs` / `Main.cs`** — wire up init, update, and toggle.
7. **Test** — generate a new world, walk around, open map (M), confirm terrain,
   structures, and fog-of-war reveal.

---

## 6. Open Questions / Notes
- **Map resolution**: per-block (fine, ~375×375) vs per-chunk (coarse, ~25×25).
  Plan assumes per-block for a Don't Starve feel; can fall back to per-chunk if
  texture memory is a concern.
- **Multiplayer**: exploration is revealed by the host; clients receive the map
  texture/state via the existing save/network sync. Initial version can be
  host-authoritative.
- **Pan/zoom**: optional polish — can add drag-to-pan and scroll-to-zoom on the
  map panel later.
