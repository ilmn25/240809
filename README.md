# 240809

A 2.5D survival game built in Unity featuring optimized chunk-based world management, interconnected pathfinding and behavioral algorithms, and more. (visit repository website)

<img width="600" height="1079" alt="image" src="https://github.com/user-attachments/assets/15db5335-5faf-412a-9984-43eb1627d2bb" />  
<img width="600" height="1080" alt="image" src="https://github.com/user-attachments/assets/722913ad-4505-4b15-929b-39496a0cccda" />  
<img width="600" height="1080" alt="image" src="https://github.com/user-attachments/assets/9d821a4f-6219-4c95-b74c-76f38e38de94" />

## Core Technical Systems

**Optimized Map Partitioning**
- 3D chunk-based world management enabling near-infinite procedural generation
- Dynamic chunk loading/unloading based on player proximity
- Efficient spatial data structures for world state management
- Multi-threaded world generation with configurable seed system
- Procedural terrain generation with biome blending (caves, craters, granite, marble, sand)

**Custom Pathfinding & AI**
- Custom pathfinding algorithms designed to address Unity NavMesh limitations
- Behavioral AI system with interconnected decision-making
- Parkour heuristics for vertical traversal and complex navigation
- Dynamic entity loading with object pooling for performance
- Navigation system supporting non-planar surfaces and multi-level environments

**Performance Optimization**
- Unity Job System integration for parallelized computation
- Burst Compiler implementation for heavy mathematical operations
- Offloaded chunk generation, entity updates, and pathfinding to worker threads
- Optimized rendering pipeline with viewport culling
- Frame time capping system with delta time smoothing (max 30ms)

**DevTools Integration**
- Custom editor tooling for rapid asset creation and iteration
- Debug console system with runtime command execution
- FPS monitoring and performance profiling tools
- Streamlined workflow reducing content creation time by approximately 80%
- Real-time world editor with block preview and structure placement

**Data Management**
- Serialization system for world persistence and save/load functionality
- Chunk-based data storage with compressed block data
- Entity state management with dynamic serialization
- Item and inventory data structures with efficient lookup tables

**Game Systems**
- Block-based world interaction with multiple collision layer types
- Player inventory and item management with slot-based system
- Crafting system with recipe validation and structure previews
- Audio engine with positional sound effects and environmental audio
- Particle system for environmental effects and feedback
- Lighting system with indoor/outdoor transitions and dynamic shadows

## Architecture

**Code Organization**
```
Assets/Script/
├── Game/
│   ├── World/           # Chunk management, procedural generation, save/load
│   ├── Entity/          # Dynamic loading, object pooling, AI navigation
│   ├── Brain/           # Behavioral algorithms and decision trees
│   ├── Block/           # Block data structures and placement logic
│   ├── Map/             # Mesh generation and rendering optimization
│   ├── Player/          # State management and task queue system
│   ├── Item/            # Item definitions, loot tables, status effects
│   └── GUI/             # UI components and HUD rendering
└── Utility/
    ├── Helper.cs        # Math and utility functions
    ├── Console.cs       # Debug console implementation
    ├── FPSLogger.cs     # Performance profiling
    └── Wrapper/         # Unity API abstractions
```

**Key Classes**
- `World`: Manages chunk array, world bounds, and spatial queries
- `Chunk`: Handles block storage, mesh generation, and serialization
- `Gen`: Procedural generation pipeline with pluggable generation tasks
- `EntityDynamicLoad`: Proximity-based entity loading system
- `Navigation`: Custom pathfinding with A* optimization and heuristics

## Performance Characteristics

- Chunk generation offloaded to background threads
- Burst-compiled mathematical operations for terrain generation
- Job System parallelization for entity updates and pathfinding
- Viewport-based culling reducing unnecessary rendering
- Object pooling eliminating garbage collection spikes
- Delta time clamping preventing physics explosions under load

## Build Requirements

- Unity 2022.3.62f1 or compatible version
- Burst Compiler package
- TextMeshPro package
- Collections package for native containers

## Technical Highlights

- Near-infinite world generation through efficient chunk streaming
- Custom pathfinding outperforms Unity NavMesh for complex 3D navigation
- DevTools pipeline enables rapid iteration and content authoring
- Consistent high framerates through parallelized computation and optimized rendering

<sub>© 2025 illu. All rights reserved. This code is for reference only and may not be used, copied, or distributed without permission.</sub>