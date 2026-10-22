 

The Hong Kong Polytechnic University Department of Computing  

 

COMP4913 Capstone Project  

Proposal 

 

Freestyle Game Project 

A cooperative survival colony simulator with real time 3D navigation in a destructible cubic world 
 

Student Name: 			KWAN Kai Man 

Student ID No.:			23118556D  

Programme-Stream Code: 	61435-FCS 

Supervisor:				Dr Zackary Sin 

Submission Date: 			23 October 2026 


Table of Content

1 Background and Problem Statement

2 Objective and Outcome

3 Project Methodology

3.1 Literature Review

3.2 System Architecture / Proposed Approach

3.3 Implementation & Task Breakdown

4 Project Schedule

4.1 Project Milestones

4.2 Work Plan & Timeline

5 Resources Estimation

6 References


1 Background and Problem Statement 

Survival games and colony simulators draw on the same set of player motivations, which the game design literature describes in terms of rules, systems and meaningful choice [1], [2]. Don't Starve Together [19] builds tension from scarcity, permadeath and a day and night cycle. Minecraft [20] offers an unbounded world that the player can destroy block by block. Terraria [23] layers equipment progression onto exploration. RimWorld [21] removes direct control and lets colonists whose needs and moods drive the story. Palworld [22] makes recruitment and labour appealing. Each of these games is strong on one axis. None of them holds all three axes at once, and the reason is technical rather than artistic. A world that the player can reshape at block resolution, a population of many autonomous agents, and cooperative multiplayer all compete for the same budget of CPU time in every frame.

This project, a free style capstone game called Feeding the Maw, targets exactly that intersection. The player is a worker conscripted to the edge of a procedurally generated cubic world. The loop is to gather resources, build a base, and attract wanderers by providing shelter, which means a bed and a light source close together. Recruited wanderers can be drafted to follow the player on gathering trips, they fight beside the player, and they must be rescued when they are incapacitated. The base grows through crafting stations, walls, furniture and shortcuts such as lifts, rails and portals, and each improvement lets the next day's trip reach farther. Pressure arrives from scheduled raid nights and from a daily coin quota demanded by the Maw, a pit that consumes wealth. Meeting the quota raises maximum health and failing it lowers maximum health, so the player's own strength is rented from an economy that must be fed continuously.

A substantial prototype already exists, at roughly 28,400 lines of C# across 356 files in the game assembly. It contains a chunked cubic world, four procedural generation profiles, a custom navigation and behaviour system, server authoritative multiplayer, save and load, and more than 120 entity and structure types. This proposal states the problem the project addresses, the objectives for the remaining work, the method used to reach them, and the plan and resources required.

The design goal breaks down into five technical problems. These are what the project investigates, and the game is the vehicle that makes them measurable.

1. Navigation in a world that changes constantly. Conventional game AI assumes a navigation mesh built once from static geometry. When the player mines, builds and terraforms at block resolution, the walkable surface changes all the time and rebuilding a 3D navmesh after each edit is not practical. Agents must also move vertically, jumping gaps, climbing ledges and dropping down, and classic grid pathfinding treats those moves as impossible. Agents outside the player's view still have to be simulated, so collision cannot depend on colliders being loaded.

2. Simulating more than a hundred agents under one authority. Colony behaviour only looks alive when many agents act at once, with migrants arriving, workers hauling, raiders approaching and animals wandering. Pathfinding for a hundred agents cannot stall the frame, so the search has to run off the main thread, yet Unity objects are not safe to touch from other threads. That forces a separation between the data the search uses and the engine objects it must not touch.

3. Persisting and streaming a large generated world. A world built from a seed must come out identical on every client, otherwise clients disagree about geometry. Player edits have to survive a save and load cycle, and a player joining an existing session must receive the current world state without freezing the host.

4. Behaviour that looks intelligent without being expensive. Recruitable companions have to decide when to follow, fight, flee, rest or be rescued, and enemies have to pursue, ambush and retreat. Running a full behaviour tree for every agent every frame is affordable only if behaviour is assembled from cheap reusable pieces rather than written once per creature.

5. Authoring throughput for a single developer. With more than 120 entity and structure types defined, writing each one by hand is the largest cost in the project. Content therefore has to be driven by data, with tooling that keeps iteration fast.

The research interest sits in the first and fourth problems, because those are where the project departs from common practice and where the departure can be measured. I use a 3D A* variant whose heuristic accepts airborne cells instead of rejecting them, and I evaluate it by comparison against a plain grid A* of the kind used in voxel games [20]. I also argue for a combination of explicit state machines and shared behaviour modules, which should be cheaper than per agent behaviour trees [11], [12] at this agent count. Both claims can be tested with the experiments described later in this proposal. The practical interest is the artefact itself. A playable cooperative game whose world data, navigation data, simulation and replication are cleanly separated makes the AI cheap and testable, and that separation is the part most likely to be useful to other people building similar games.

2 Objective and Outcome 

The aim is to design, implement and evaluate a cooperative survival colony simulator that supports a fully destructible cubic world, more than one hundred concurrently simulated agents, and server authoritative multiplayer at the same time, within a real time frame budget on consumer hardware.

The objectives below are specific and each one carries the measure by which it will be judged. The numeric targets are provisional and will be revised against the baseline recorded in the first milestone, because a target chosen before any measurement is only a guess.

1. Design a 3D grid pathfinding method for destructible terrain, including a heuristic that treats airborne nodes as passable but penalised so that agents can jump, climb and drop. Success means at least 95 percent path success on a held out set of parkour and cave maps, with a median solve time of 8 milliseconds or less over a 48 by 48 by 48 block search volume.

2. Make navigation scale to many agents by running searches on worker threads over an engine independent navigation bitmap, with a fixed ceiling on concurrency. Success means at least 100 agents pathing at once while the host holds 30 frames per second or better, with no main thread stall above 2 milliseconds caused by pathfinding.

3. Build a behaviour architecture assembled from reusable states and information modules and selected per archetype, instead of code written per creature. Success means all 120 or more archetypes are expressed as combinations from one shared library, and a new archetype can be added without writing a new state class.

4. Build a server authoritative multiplayer layer in which each simulated subsystem is replicated on its own, with players able to join in the middle of a session through chunked world snapshots. Success means a four player session runs for 45 minutes with no unhandled desync, joining mid session does not stall the host, and per client bandwidth is measured and reported.

5. Provide deterministic world generation and complete persistence, covering world edits, entities, inventories and quests. Success means the same seed produces an identical world hash on host and client, and a save and load cycle preserves all state with no regression in the automated test suite.

6. Complete the gameplay arc so that the systems can be judged as a game rather than as a technology demonstration. Success means a first time player can finish the tutorial, recruit a companion, survive a raid and meet the Maw quota without help inside a 40 minute session.

7. Evaluate the result and report what is found. Success means an ablation of the navigation heuristic, an ablation of the behaviour architecture, and a playtest with at least twelve participants, reported with both numbers and player comments.

The expected deliverables are a playable Windows build that supports single player and four player cooperative sessions, the full Unity and C# source in version control with the navigation, behaviour and replication systems documented, a benchmark harness with recorded results for pathfinding latency and success rate, agent throughput, memory per loaded region, save and load time and per client bandwidth, the two ablation studies, and a dissertation with a poster and demonstration.

The scope covers one world per session, up to four players, and Windows desktop only. It excludes matchmaking services, anti cheat, player versus player combat, mobile and console builds, and voice chat. It also excludes handmade art, since asset packs and generated textures are used and the contribution of the project is engineering rather than art direction.

3 Project Methodology 

3.1 Literature Review 

Procedural content generation divides into generators that emit content directly from rules and generators that search for content against a fitness function [3], [4]. Both are usually judged on aesthetic or playability measures computed offline, which is not the constraint here. My requirement is narrower and stricter. Generation has to be deterministic, independent of evaluation order and incremental, because the world is streamed in and out and because host and clients must derive identical geometry from one seed. The approach I take, following the voxel tradition [20], is to hash the world seed with a task salt and the chunk coordinate so that no chunk depends on when its neighbours are generated. Determinism is treated as a correctness property and is tested by comparing world hashes between host and client rather than by inspection.

For pathfinding, A* [5] remains the reference algorithm. It is complete, it is optimal under an admissible heuristic, and its cost is easy to bound. Later work attacks that cost. HPA* [6] clusters the grid and searches an abstract graph, trading optimality for speed on large maps. Jump Point Search [7] prunes symmetric successors and is very effective on uniform cost 2D grids. D* Lite [8] repairs a previous solution incrementally when costs change, which suits a single agent in a slowly changing map. Flow fields [9] amortise one search across a whole crowd, and continuum crowds [10] produce smooth steering for dense groups. None of these fits this project. HPA*, Jump Point Search and flow fields assume a mostly static cost field, and the flow field approach also assumes a shared destination. D* Lite keeps bookkeeping per agent, so it becomes the bottleneck exactly when agents are numerous. Continuum crowds solve steering rather than 3D traversal. Unity's own NavMesh [17] is excluded for a more basic reason, since it is baked from scene geometry and ties navigation to collider physics, which rules out both per block edits and simulation outside the visible range.

For AI architecture, finite state machines are easy to read but their transition count grows badly. Behaviour trees [11], [12] compose and reuse subtrees and are the usual choice for complicated agents, at the cost of evaluating a tree for every agent every tick. Colony simulators such as RimWorld [21] instead rely on short and frequently re evaluated jobs, with the story emerging from the interaction of those jobs [15]. This project takes a middle position. Each agent runs an explicit state machine, but the states are shared and parameterised classes rather than per creature code, so authoring cost is paid once per state and evaluation cost stays proportional to the current state alone.

For networking, server authority with prediction and reconciliation [13] is the standard answer to latency, and the tolerable delay depends on the action being performed [14]. Because all AI here runs on the server and clients only send intents, I use full server authority with snapshot replication for each subsystem and no rollback. That gives slightly less responsive combat than a rollback design and much simpler determinism, which is the right trade for a cooperative game. For evaluation, GameFlow [16] provides a validated instrument for enjoyment, immersion and control, and I use it alongside telemetry.

3.2 System Architecture / Proposed Approach 

The prototype is organised in four layers that depend in one direction only, so the simulation can run headless on a server with no presentation layer present.

Presentation. Viewport, chunk meshing into native vertex buffers, chunk culling, lighting and particles, user interface, audio and screen effects.

Simulation. The world and its chunks, the generation profiles and their task pipeline, the behaviour system with its archetypes, states and information modules, the navigation system with its bitmap, path search and wander, and the game systems for crafting, items, quests, trade, the Maw quota and raid scheduling.

Persistence. Save and load of the world, the placed machines and the quest state, with its own replication path.

Network. Mirror as the transport and serialisation layer [18], a server that manages joining and leaving and streams world snapshots, and one replication component for each subsystem covering entities, items, projectiles, drops, storage, environment, lighting, effects, chat and navigation. Mirror handles serialisation and transport, so my code is concerned with what to replicate and when rather than with sockets.

Seven decisions shape the architecture, and each one answers one of the problems stated above.

Chunks as flat arrays with sentinel reads. A chunk holds a flat integer array of block identifiers, indexed as x plus size times the quantity y plus size times z, together with lists of placed entities and ground items. The world holds one flat array of chunks addressed by chunk coordinate and sized from the generation profile. Reads outside the world return a shared sentinel chunk while writes throw. Neighbour sampling during generation and meshing is then branch free at the boundary, and a common class of off by one error turns into a visible exception instead of silent corruption.

Salted determinism per chunk. Every generation task hashes the world seed with its own salt and the chunk coordinate to obtain its random stream. Chunk content is therefore independent of evaluation order and of how much of the world is loaded, which is what allows a client to regenerate unloaded regions and rejoin a session with matching geometry.

A navigation bitmap separate from the world. The nested world and chunk object graph is convenient for gameplay but hostile to a worker thread. A separate navigation map mirrors only the question the search asks, namely whether a cell is solid, into flat storage that is safe to share. It also has its own delta replication, so a late joiner gains walkability without receiving every block. Because collision is resolved against the bitmap instead of against colliders, agents beyond the visible range simulate with no physics objects at all, which keeps render distance and simulation correctness independent of each other.

Bounded parallelism. Searches are submitted as tasks gated by a counting semaphore, currently capped at ten at a time, so a burst of requests queues instead of starving the frame. Worker threads touch only the bitmap and their own nodes. The price is latency under saturation, which is why latency percentiles are part of the benchmark.

Airtime aware A* with traversal costs. Airborne cells are not treated as unreachable, because that assumption is the reason plain grid search will not jump. They are admissible at a cost that falls with distance from the nearest supported cell, so a short hop is cheap and a drop into open air is not. Doors and semi solid cells carry their own penalty, which biases routes around them when a route of equal length exists. This heuristic is the subject of the first ablation.

Search separated from the follow policy. The search returns nodes and a separate follow policy decides how closely to track them. It jumps at the edge of a node when the next node is airborne, loosens the corridor tolerance and skips nodes to keep momentum, and steers in the air through corners. If an agent drifts too far from its path, or the target turns out to be unreachable, the agent drops the path, wanders for a cooldown and then retries. A few stuck agents therefore cannot flood the search queue.

Composition over inheritance. The archetypes are thin subclasses that declare which states and information modules they use, and the state classes hold the behaviour. Adding a creature means declaring data rather than writing a brain. The same principle extends to content, where editor tools for placing structures and editing the world keep iteration fast.

3.3 Implementation & Task Breakdown 

Around two thirds of the engineering is finished. The rest of this section records what exists and what remains, and the schedule in the next section turns the remainder into dated work.

World and persistence. Chunk arrays, world bounds, coordinate mapping, and save and load of the world, machines and quests are complete.

Procedural generation. Four generation profiles (Abyss, SkyBlock, SuperFlat and Backrooms), more than twenty generation tasks, dungeons and set pieces, and deterministic salting are complete, with content breadth still growing.

Navigation. The navigation bitmap with delta sync, parallel A*, the wander behaviour, the follow policy and stuck recovery are complete, with tuning and benchmarking still in progress.

Agent behaviour. Over 120 archetypes, 29 states and 29 information modules cover combat, fleeing, rescue, hauling, trading and quest giving. This is in progress, because logic that was written per archetype is still being moved into shared modules.

Multiplayer. Server join and leave, chunked world snapshots, and replication for entities, items, projectiles, drops, storage, environment, lighting, effects and navigation are complete. Soak testing and desync hardening remain.

Game systems. Crafting, inventory, trade with currency, quests, the Maw quota, raid scheduling, the tutorial and the interface are in progress, with the progression curve and balance unfinished.

Client presentation. Chunk meshing into native vertex buffers, culling, lighting, particles, audio and screen effects are complete. Developer tooling, including the in editor world and structure editors, the debug console and the performance overlay, is also complete.

Two practices apply to the remaining work. Because one person cannot regression test 120 archetypes by hand, I add a small automated suite covering world determinism, a save and load round trip, and a headless server that simulates a fixed number of ticks. It is cheap to run and it catches the most expensive class of fault. Content is also frozen at the second milestone, so the evaluation measures a stable system and later ideas are logged as stretch goals instead of being implemented.

Evaluation has four parts. The first is a benchmark harness that records, for each build on a fixed reference machine, median and 95th percentile pathfinding latency at increasing agent counts, path success rate on held out vertical, cave and flooded maps, host frame time at 25, 50, 100 and 150 agents, memory per loaded region, save and load time, and per client bandwidth over a scripted 45 minute session. The second is the navigation ablation, which compares three variants on identical maps. Variant A is plain 3D grid A* with airborne cells impassable, matching the voxel game baseline [20]. Variant B makes airborne cells admissible at a flat cost. Variant C is the airtime aware heuristic. The measures are path success rate, time to reach the target, node expansions and stuck events per minute, and the expectation is that variant C raises success through parkour terrain at a modest cost over B and a large gain over A. The third is the architecture ablation, which runs the same archetypes through the shared state design and through a naive per archetype update routine, comparing CPU time per agent tick and the lines of code needed to add a new archetype. The fourth is a playtest with twelve to sixteen participants, drawn from friends who play games regularly, with half of them playing as a pair. Data comes from telemetry on session length, deaths, quota success and recruitment, from the GameFlow questionnaire [16], and from a short semi structured interview analysed by thematic coding. The threats to validity are a small sample, the developer's own judgement as designer, familiarity with friends as testers, and novelty, and these are reported rather than hidden.

4 Project Schedule 

Work from September to October 2026 produced the world, generation, navigation, multiplayer, presentation and tooling layers, and is reported in the interim report. The milestones below cover the remaining work, counted from the first week of November 2026.

4.1 Project Milestones 

The first milestone is navigation benchmarked, due in week 4. The harness runs, baseline numbers for latency, success rate and agent throughput are recorded, and the heuristic is tuned to meet the first two objectives on the reference machine.

The second is feature freeze and persistence, due in week 8. A save and load round trip is verified for world, entities, inventories and quests, the world hash comparison between host and client passes, and the feature list stops growing.

The third is cooperative hardening, due in week 12. A four player session runs for 45 minutes with no unhandled desync, joining mid session is verified, and bandwidth is measured.

The fourth is content and balance complete, due in week 16. The full arc is playable from start to finish, the progression and difficulty curves are tuned, and the tutorial is finished.

The fifth is evaluation complete, due in week 20. Both ablations have been run and analysed and the playtest has been conducted, with results tabulated.

The sixth is final submission, due in week 24. The release build, dissertation, poster and demonstration are submitted and the repository is documented.

4.2 Work Plan & Timeline 

| Weeks | Period | Work | Output |
|---|---|---|---|
| 1 to 2 | November 2026 | Build the benchmark harness, instrument pathfinding and simulation, capture the baseline | Baseline report |
| 3 to 4 | November 2026 | Tune the airtime heuristic and the follow policy, run agent count scaling tests, close the remaining stuck agent cases | Milestone 1 |
| 5 to 6 | November 2026 | Move the remaining archetype specific logic into shared modules | Less duplication |
| 7 to 8 | December 2026 | Complete persistence, add determinism tests, freeze features | Milestone 2 |
| 9 to 10 | December 2026 | Harden replication and finish snapshot streaming for players who join late | Stable four player session |
| 11 to 12 | January 2027 | Soak testing, desync triage, bandwidth profiling | Milestone 3 |
| 13 to 16 | January and February 2027 | Progression and balance pass, raid escalation, dialogue and quest content, tutorial polish | Milestone 4 |
| 17 to 18 | February 2027 | Build the ablation variants and run both experiments | Experiment data |
| 19 to 20 | March 2027 | Run playtest sessions, transcribe and code the notes, analyse | Milestone 5 |
| 21 to 22 | March 2027 | Act on the findings, fix the highest impact issues, final bug pass | Release candidate |
| 23 to 24 | April 2027 | Write the dissertation, prepare the poster and demonstration, submit | Milestone 6 |

Two weeks of contingency sit inside weeks 21 and 22 and are released only if the earlier milestones finish on time.

The main risks are as follows.

* Scope creep from new content ideas, which is likely to have a large effect and is controlled by the feature freeze at the second milestone.
* Multiplayer desyncs that resist reproduction, which would also have a large effect. The mitigation is replication logging with deterministic replays, falling back to a verified two player configuration if a fault cannot be isolated in time.
* Benchmark results from a single machine that do not generalise, so hardware is reported explicitly and comparisons between variants are presented rather than absolute claims.
* Testers who are friends and therefore less critical than a stranger, so sessions run without the developer present where possible and the questionnaire is completed before any discussion.
* Performance regression on weaker hardware, handled with quality presets and a limit on chunk loading per frame.

5 Resources Estimation 

The project uses only hardware, software and services that are already available at no cost, so the estimated cash requirement is HKD 0.

The development machine is my personal computer, which runs Windows 11 with an eight core processor, 32 GB of memory, a RTX 5060 graphics card and a 1 TB solid state drive, and it doubles as the benchmark reference machine. A mid range laptop serves as the second client for cooperative testing, and both are already owned. The engine is Unity 2022.3.62f1 LTS under a free personal licence, and code edited in Visual Studio. Mirror, Newtonsoft JSON and the Universal Render Pipeline are open source or bundled with the engine.

All art is produced in house rather than purchased. Sprites, tiles, item icons and interface art are drawn in Clip Studio Paint, which covers nearly all of the visual content the game needs. Any 3D conversion or export is done with Blender, which is free. Sound is taken from free libraries or generated, so there is no asset pack budget. Playtesting is arranged with friends who play games regularly.

Human effort is estimated at 12 to 15 hours per week across 24 weeks, roughly 320 to 360 hours, plus fortnightly supervision meetings. Department laboratory machines are available as an alternative reference platform if the development machine turns out to be unrepresentative.

6 References 
 
1. Adams, E. (2014). Fundamentals of Game Design (3rd ed.). New Riders. 
2. Salen, K., & Zimmerman, E. (2004). Rules of Play: Game Design Fundamentals. MIT Press. 
3. Shaker, N., Togelius, J., & Nelson, M. J. (2016). Procedural Content Generation in Games. Springer. 
4. Togelius, J., Yannakakis, G. N., Stanley, K. O., & Browne, C. (2011). Search-Based Procedural Content Generation: A Taxonomy and Survey. IEEE Transactions on Computational Intelligence and AI in Games, 3(3), 172-186. 
5. Hart, P. E., Nilsson, N. J., & Raphael, B. (1968). A Formal Basis for the Heuristic Determination of Minimum Cost Paths. IEEE Transactions on Systems Science and Cybernetics, 4(2), 100-107. 
6. Botea, A., Müller, M., & Schaeffer, J. (2004). Near Optimal Hierarchical Path-Finding. Journal of Game Development, 1(1), 7-28. 
7. Harabor, D., & Grastien, A. (2011). Online Graph Pruning for Pathfinding on Grid Maps. Proceedings of the Twenty-Fifth AAAI Conference on Artificial Intelligence. 
8. Koenig, S., & Likhachev, M. (2002). D* Lite. Proceedings of the Eighteenth National Conference on Artificial Intelligence. 
9. Emerson, E. (2013). Crowd Pathfinding and Steering Using Flow Field Tiles. In S. Rabin (Ed.), Game AI Pro (Chapter 23). CRC Press. 
10. Treuille, A., Cooper, S., & Popović, Z. (2006). Continuum Crowds. ACM Transactions on Graphics, 25(3), 1160-1168. 
11. Isla, D. (2005). Handling Complexity in the Halo 2 AI. Game Developers Conference 2005. 
12. Colledanchise, M., & Ögren, P. (2018). Behavior Trees in Robotics and AI: An Introduction. CRC Press. 
13. Bernier, Y. W. (2001). Latency Compensating Methods in Client/Server In-Game Protocol Design and Optimization. Game Developers Conference 2001. 
14. Claypool, M., & Claypool, K. (2006). Latency and Player Actions in Online Games. Communications of the ACM, 49(11), 40-45. 
15. Riedl, M. O., & Bulitko, V. (2013). Interactive Narrative: An Intelligent Systems Approach. AI Magazine, 34(1), 67-77. 
16. Sweetser, P., & Wyeth, P. (2005). GameFlow: A Model for Evaluating Player Enjoyment in Games. ACM Computers in Entertainment, 3(3), Article 3A. 
17. Unity Technologies. (2024). Unity 2022.3 LTS Manual: Navigation and AI. https://docs.unity3d.com/2022.3/Documentation/Manual/ 
18. Mirror Networking. (2024). Mirror Documentation. https://mirror-networking.gitbook.io/docs 
19. Klei Entertainment. (2016). Don't Starve Together [Video game]. 
20. Mojang Studios. (2011). Minecraft [Video game]. 
21. Ludeon Studios. (2018). RimWorld [Video game]. 
22. Pocketpair. (2024). Palworld [Video game]. 
23. Re-Logic. (2011). Terraria [Video game]. 
