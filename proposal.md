

    The Hong Kong Polytechnic University Department of Computing



    COMP4913 Capstone Project

    Proposal



    Free-style Game Project: Morimori 

    A cooperative survival colony simulator with real time 3D navigation in a destructible cubic world


    Student Name: 			KWAN Kai Man

    Student ID No.:			23118556D

    Programme-Stream Code: 	61435-FCS

    Supervisor:				Dr Zackary Sin

    Submission Date: 			23 October 2026

    Table of Content


    1 Background and Problem Statement

    Survival games and colony simulators share their motivations, which come down to rules, systems and meaningful choice. Don't Starve Together [1] focuses on scarce resources and a death that matters. Minecraft [2] focuses on a world that can be dug out and rebuilt block by block. Terraria [3] focuses on equipment progression that gives each outing a goal beyond survival. RimWorld [4] focuses on colonists with needs and moods of their own rather than puppets. Palworld [5] focuses on recruits who work for the player. 99 Nights in the Forest [6] focuses on nightly raids on a camp that has to be held.

    Taking one of these is affordable. Taking all six asks for a world that changes, a crowd that thinks for itself, a settlement worth defending and a second player in the same build, and most studios meet that cost by dropping one of them. Whether those strengths can sit in one game, to create a new survival game experience, is the problem this project wants to tackle.

    This project is a free style capstone game with the working codename Morimori. "Mori" means "to die" in Latin, and "forest" in Japanese.

    In Morimori, the player starts with nothing in a forest, gathers material and builds a base, which attracts wanderers who can be drafted to gather and fight. Shortcuts such as lifts, rails and portals let each day reach further than the last, while raids arrive at night and grow, sent by different hostile groups such as cultists, forest creatures and stranger things from further out, so the settlement has to keep pace. These parts depend on each other, since shelter earns recruits, recruits make a party, and a party survives the raid that makes expanding worth it, while a second player lets one person hold the base while the other gathers.

    2 Objectives and Outcome

    The aim is to design, implement and evaluate a cooperative survival colony simulator built on a destructible cubic world, with many concurrent entities and server authoritative multiplayer.

    2.1 Gameplay

    The game asks the player to keep a settlement alive while reaching further and further from it. It does this with four loops of different lengths, from a single trip to a whole run, which is a common way to hold a player's attention over a long session [7].

    Real time gameplay covers what happens on a single trip. The player fights, carries loot, chooses whether to push deeper or turn back, and decides what to do about a companion who goes down. A downed companion is not dead, so the choice is to carry them home and lose time, or leave them and keep what has been gathered.

    The short term goal is to survive the day. The player leaves in the morning, gathers, builds what can be built, and has to be home before nightfall, because that is when raids arrive. Better material lies further out, so every trip is a bet on getting back in time.

    The medium term goal is the base, which grows over several days. Material becomes stations, walls, furniture and shortcuts. Each upgrade widens the area one day can cover, and shortcuts widen it most, since they turn a long walk into a short one. Building is therefore how the player extends their reach, not decoration, and shelter also attracts a wanderer who can gather, fight or stay behind to work, so the base turns into people as well as equipment.

    The overarching goal is to keep the run alive while exploring further out, into areas the settlement could not reach before. The run lasts only while the base and the party grow faster than the raids, and each new area brings better material with worse danger. A base that stops expanding eventually falls, and the next run starts from nothing.

    The loops feed each other, since a good trip supplies the base and a base that survives the night keeps the run going, while losing the run sends the player back to the first day. 

    2.2 Development

    The loops above need the following systems.

    * The world is a destructible grid of chunks that saves and loads, so the same seed has to produce the same world on host and client, and a save cycle has to keep edits, entities, inventories and quests.
    * Terrain, caves, dungeons and set pieces come from four generation profiles and over twenty tasks, and a fixed seed has to produce variety without any manual placement.
    * Navigation is 3D pathfinding over a shared bitmap, with a jump and climb heuristic and a separate follow policy. The target is paths that succeed through parkour and cave terrain, and many entities pathing without the frame rate dropping.
    * Recruitable and hostile entities are built from shared states and information modules, so every archetype has to come from the shared library and a new archetype has to need no new state class.
    * Multiplayer is server authoritative, with each subsystem replicated separately and joining mid session supported. Four players have to hold one session without unhandled desync and without the host stalling on join.
    * Crafting, inventory, merchant trade, quests and raid scheduling give these loops something to spend and something to gain, so a day of gathering turns into a better base and a raid that can be survived.
    * Menus, HUD, tutorial and dialogue have to teach the game without outside help, and the editor tools have to make new content quick to author.

    Deliverables are a Windows build for single player and four player sessions, the Unity and C# source, the recorded results, and a dissertation with a poster and demonstration. Scope is one world per session, and it excludes matchmaking, anti cheat, player versus player combat, mobile and console builds, voice chat and handmade art, since the contribution is engineering.

    3 Project Methodology

    The design question breaks into five problems, and each needs its own research, solution and implementation. They are taken one at a time below.

    3.1 Navigation in a Destructible World

    3.1.1 Problem

    The walkable surface changes whenever a block is mined or placed, so navigation cannot be baked once. Entities must also jump, climb and drop, which a flat grid search refuses, and entities outside view must move without colliders.

    3.1.2 Literature Review

    A* [8] is the reference, while Jump Point Search [9] and flow fields [10] assume a mostly static map and a shared destination. Lifelong planning A* [11] repairs an earlier search when blocks change, but it keeps bookkeeping for every search it runs, so it suits few entities rather than many. Unity's NavMesh [12] is baked from scene geometry, so it cannot follow per block edits or run outside view.

    3.1.3 Solution

    Navigation keeps its own map of which cells are solid, separate from the blocks themselves, so it can be read from a background worker and sent to a client without the block data. Pathfinding is a three dimensional version of A* in which cells in mid air are allowed but cost more the further they sit from solid ground, so a short hop is cheap and a long drop is not. The search produces a list of points and a separate routine decides how to follow them, and collision is checked against the same map, so a creature far from the camera behaves exactly as one on screen. A creature that drifts off its path wanders for a moment, then tries again.

    3.2 Simulating Many Entities

    3.2.1 Problem

    Pathfinding for many entities cannot stall the frame, so it has to run off the main thread, while Unity objects are unsafe to touch from one.

    3.2.2 Solution

    Path requests are handed to background workers, with at most ten searched at once, so a sudden burst waits in a queue instead of freezing the frame. Those workers read only the walkable map and their own data, never Unity objects, which is what makes it safe to run them off the main thread. The trade off is that a request can wait when the queue is busy, so response time and entity counts are measured rather than assumed.

    3.3 World Storage, Determinism and Persistence

    3.3.1 Problem

    A seeded world must come out identical on every client, edits must survive save and load, a late joiner must receive the state without freezing the host, and the world must stay cheap to read from a worker thread.

    3.3.2 Solution

    Each chunk is generated from the world seed combined with that chunk's own coordinates, so the same chunk always comes out the same, no matter what else is loaded or what order things are generated in. Voxel games have done it this way for years [2]. Chunks sit in one flat array indexed by coordinate, and a read outside the world returns an empty chunk instead of failing, which keeps lookups from a worker thread safe. Player edits are stored apart from generated content, so a save records only what changed. When a block changes, that one event updates the walkable bitmap, queues a mesh rebuild and tells clients the walkable surface moved.

    3.4 Behaviour Architecture and Content Authoring

    3.4.1 Problem

    Companions must follow, fight, flee, rest and be rescued, and enemies must pursue, ambush and retreat. A tree walked per entity per frame is expensive when many entities act at once, and the prototype already defines over 120 types, so per creature code is the largest cost in the project.

    3.4.2 Literature Review

    State machines are easy to read, but every new ability adds transitions to them. Behaviour trees [13] reuse subtrees at the cost of a walk per entity per tick, while colony simulators such as RimWorld [4] rely on short, frequently re evaluated jobs.

    3.4.3 Solution

    Each behaviour is written once as a shared state and reused by any creature that needs it. A creature is then described by a short list saying which states and which information modules it uses, so adding a new type means filling in a data entry rather than writing new behaviour code. Only the state a creature is currently in runs each tick, so cost per creature does not grow as more states are added. Editor tools, a debug console and a performance overlay make it quick to try an idea and see it working.

    3.5 Server Authoritative Multiplayer

    3.5.1 Problem

    Every player must see the same game, a player must be able to join a session already in progress, and neither may stall the host.

    3.5.2 Literature Review

    Latency is unavoidable once the server owns the simulation, and how much a delay matters depends on the action being performed [14]. Client side prediction and reconciliation hide it, at the cost of a simulation that has to be reconstructible.

    3.5.3 Solution

    The server runs the game and clients only send what the player is trying to do, so no client can change the world on its own. Nothing is predicted ahead on the client and nothing is rolled back, which makes combat slightly less responsive and keeps every player in step, a fair trade for a cooperative game. Sending is split by kind of thing, with one channel each for entities, items, projectiles, drops, storage, environment, lighting, effects, chat and navigation, so a busy channel can be slowed without disturbing the others. A player who joins late receives the world in pieces, then the walkable map, then regular updates, so the host never has to send one huge message. Mirror handles the transport [15].

    4 Project Schedule

    A prototype already exists at roughly 28,400 lines of C#, built from September to October 2026. The table covers what remains, counted from the first week of November 2026. 

    | Weeks | Period | Milestone | Explaination |
    |---|---|---|---|
    | 1 to 4 | November 2026 | Base MVP gameplay in singleplayer | One person can start a world, gather, build, survive a night and face a raid, while many companions move at once without the game slowing. Networking stays off so gameplay can be assessed on its own. |
    | 5 to 6 | November and December 2026 | First playtest | Up to three friends play the singleplayer build without me present. The worst problems are ranked before more systems are touched. |
    | 7 to 12 | December 2026 and January 2027 | Save, load and multiplayer work consistently | A session saves and reloads exactly as it was, and three friends and I hold one world without the clients disagreeing about it. Content is frozen after this point. |
    | 13 to 14 | January and February 2027 | Second playtest | The same friends play the networked build, so the two playtests show what the multiplayer work changed. |
    | 15 to 20 | February and March 2027 | Complete loop and content | A new player can follow the tutorial, gather and build, rescue and recruit wanderers, and face raids that get harder each night. Balance comes from playtest data. |
    | 21 to 22 | March 2027 | Evaluation | Up to three friends take part in supervised sessions, playing alone or as a pair, and their feedback is written up. |
    | 23 to 24 | March and April 2027 | Submission | Findings are fixed, a bug pass is done, and the build, dissertation, poster and demonstration are submitted. |

    5 Resources Estimation

    Development Hardware: my personal device, a Windows 11 machine with an eight core processor, 32 GB of memory, an RTX 5060 and a 1 TB solid state drive. A laptop already owned serves as the second client for cooperative testing.

    Development Software: Unity 2022.3.62f1 LTS under a free personal licence, edited in Visual Studio, with Mirror, Newtonsoft JSON and the Universal Render Pipeline open source or bundled.

    Game Texture: self made, drawn in Clip Studio Paint, in a 2.5D pixel art style. High quality visuals would need more art time than one person has, so pixel art keeps the cost sustainable. It suits a cubic world, stays cheap to extend with new blocks and creatures, and lets one developer keep everything consistent.

    Game Audio: free assets online, but soon may employ help from friends.

    Game Code: hand coded with assistance from coding agents.

    6 References/Bibliography

1. Klei Entertainment. (2016). *Don't Starve Together* [Video game]. Windows. https://www.klei.com/games/dont-starve-together
2. Mojang Studios. (2011). *Minecraft* [Video game]. Windows. https://www.minecraft.net
3. Re-Logic. (2011). *Terraria* [Video game]. Windows. https://terraria.org
4. Ludeon Studios. (2018). *RimWorld* [Video game]. Windows. https://rimworldgame.com
5. Pocketpair. (2024). *Palworld* [Video game]. Windows. https://www.pocketpair.jp/palworld
6. Grandma's Favourite Games. (2025). *99 Nights in the Forest* [Video game]. Roblox. https://www.roblox.com/games/15821191569/99-Nights-in-the-Forest
7. Schell, J. (2019). *The Art of Game Design: A Book of Lenses* (3rd ed.). A K Peters/CRC Press. https://doi.org/10.1201/b22101
8. Hart, P. E., Nilsson, N. J., & Raphael, B. (1968). A formal basis for the heuristic determination of minimum cost paths. *IEEE Transactions on Systems Science and Cybernetics*, 4(2), 100-107. https://doi.org/10.1109/TSSC.1968.300136
9. Harabor, D., & Grastien, A. (2011). Online graph pruning for pathfinding on grid maps. *Proceedings of the Twenty-Fifth AAAI Conference on Artificial Intelligence*, 1114-1119. https://doi.org/10.1609/aaai.v25i1.7994
10. Emerson, E. (2013). Crowd pathfinding and steering using flow field tiles. In S. Rabin (Ed.), *Game AI Pro* (pp. 336-345). CRC Press. https://doi.org/10.1201/b16725-29
11. Koenig, S., Likhachev, M., & Liu, Y. (2004). Lifelong planning A*. *Artificial Intelligence*, 155(1-2), 93-146. https://doi.org/10.1016/j.artint.2003.12.001
12. Unity Technologies. (2024). *AI Navigation package manual, version 1.1, for Unity 2022.3 LTS*. https://docs.unity3d.com/Packages/com.unity.ai.navigation@1.1/manual/index.html
13. Colledanchise, M., & Ögren, P. (2018). *Behavior Trees in Robotics and AI: An Introduction*. CRC Press. https://doi.org/10.1201/9780429489105
14. Claypool, M., & Claypool, K. (2006). Latency and player actions in online games. *Communications of the ACM*, 49(11), 40-45. https://doi.org/10.1145/1167838.1167860
15. Mirror Networking. (2024). *Mirror documentation*. https://mirror-networking.gitbook.io/docs