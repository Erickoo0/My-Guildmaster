## AI Context: My-Guildmaster (Unity)

### 1) Project Overview
- **Observed:** This is a Unity `2D` top-down RPG/life-sim style project built in `C#` (environment indicates Unity `6000.3.9f1`, C# `9.0`, target `net471`).
- **Observed:** The codebase contains implemented systems for player/entity control, combat skills/effects, inventory/items/crafting, dialogue, quests, world time/location, dungeon rounds/spawning, and save/load.
- **Observed:** Project metadata and code patterns indicate a systems-driven approach using modular gameplay subsystems under `Assets/_Project/System`.
- **Reasonable inference (from code + provided context):** Core loop likely alternates between city/life-sim interactions (NPC dialogue, quests, inventory/crafting/worker economy) and combat-focused dungeon progression (round-based enemy spawning and escalation).
- **Unclear from current code inspection:** Full gameplay progression/balance rules (e.g., long-term guild expansion logic, end-to-end roguelite reset/meta-progression) are not fully verifiable from sampled files alone.

### 2) Project Structure
- **`Assets/_Project`**: Primary game code/content root.
  - `System/`: Main gameplay and technical systems (most important folder for logic).
  - `UI/`, `Animations/`, `Art/`, `Data/`, `Scene/`, `Utility/`: Supporting content and shared utilities.
- **`Assets/_Project/System`** (major development focus):
  - `Combat/`, `Entity/`, `Item/`, `Dialogue/`, `Quest/`, `Dungeon/`, `Save/`, `World Time/`, `Environment/`, `Worker/`, `VFX/`.
- **`Assets/Plugins`**: Third-party/runtime tooling, including `AstarPathfindingProject`, `Demigiant`, and plugin resources.
- **`Assets/Editor`**: Custom editor scripts/tools (e.g., hierarchy/grid tooling).
- **`Docs/`**: Existing technical docs per subsystem (input, interaction, inventory, combat, state machine, save/load, etc.).
- **Not a focus for architecture orientation:** Unity-generated/engine folders like `Library`, `Logs`, `Temp`, `obj`, `UserSettings`.

### 3) Major Systems (Observed)
- **Entity + State Machine:**
  - Reusable `StateMachine` (`CurrentState`, `ChangeState`, `UpdateState`, `FixedUpdateState`) in `System/Entity/Script/StateMachine.cs`.
  - Player, mobs, and NPCs use controller/state classes (`ControllerPlayer`, `ControllerEntity`, `NPCController`, plus multiple concrete state files).
- **Player system:**
  - Input/lifecycle integration (`PlayerInputManager`, interaction detector, equipment, player stats persistence).
  - Interaction targeting via `IInteractable` proximity + priority logic.
- **Combat + Skills + Stats:**
  - Skill controllers (`SkillControllerPlayer`, `SkillControllerEntity`), hit/hurt boxes, projectile/effect pipeline.
  - Data and behavior assets under `Combat/ScriptableObject` (skills, modifiers, effects, skill tree).
  - Stats modules for health, mana, level, and providers implementing combat interfaces.
- **Item + Inventory + Crafting:**
  - `InventoryManager` with slot arrays, events, and save/load integration (`ISaveable`).
  - Item runtime/static split (`ItemInstance`, `ItemDataSo`, database, item properties, crafting/recipe managers).
- **Dialogue:**
  - `DialogueManager` orchestrates dialogue groups/nodes, line progression, option selection, and `DialogueAction` execution.
  - UI integration and menu focus management are event-driven.
- **Quest:**
  - `QuestManager` tracks active/completed quests, objective progress, and reacts to global events (entity death, flags/stats/time).
  - Includes save/load serialization via `ISaveable`.
- **World Time + Location:**
  - `WorldTime` advances simulated time and broadcasts updates.
  - Calendar/UI/light/location components are connected through global events.
- **Dungeon:**
  - `DungeonController` coordinates dungeon activation by location, round progression, enemy tracking, and spawner interaction.
- **Save/Load:**
  - `SaveManager` gathers all `ISaveable` MonoBehaviours, serializes to JSON (`Application.persistentDataPath/Save.json`), and restores on load.
- **Worker/Economy-adjacent:**
  - `WorkerManager` currently computes sustenance from inventory food properties and emits updates via event bus.

### 4) Architectural Overview
- **Observed patterns in active use:**
  - `ScriptableObject`-driven data for skills, effects, skill trees, quests/dialogue data, and item definitions/properties.
  - Generic/typed **state machine** pattern for entity behavior.
  - Central static **event bus** (`Utility/EventBus.cs`) for cross-system signaling (UI, quest, time, combat, crafting, worker stats).
  - **Controller + composition** style with MonoBehaviours coordinating focused components.
  - **Interface-based boundaries** in key places (`ISaveable`, `IInteractable`, `IDamagable`, storage/usable interfaces).
  - **Singleton usage** across several managers (`InventoryManager`, `SaveManager`, `QuestManager`, `DialogueManager`, `WorkerManager`, etc.).
- **Observed implementation style:** Hybrid architecture—modular subsystems with decoupling via events/interfaces, but also practical singleton access for global managers.

### 5) Important Technologies and Dependencies
- **Unity packages (manifest):**
  - `com.unity.inputsystem` (new Input System)
  - `com.unity.render-pipelines.universal` (URP)
  - `com.unity.cinemachine`
  - `com.unity.2d.*` stack (tilemap, animation, tooling, sprite pipelines)
  - `com.unity.test-framework`
  - `com.unity.ugui`, `com.unity.timeline`, `com.unity.visualscripting`
- **Third-party/plugin folders present and in use:**
  - `AstarPathfindingProject` (pathfinding plugin project files and plugin folder present)
  - `Demigiant` (plugin folder present; likely DOTween ecosystem, exact runtime usage not fully verified in sampled files)
  - `NavMeshPlus` project references exist in solution files.

### 6) Important Development Notes for Future AI Agents
- **Observed:** Core gameplay logic is concentrated in `Assets/_Project/System`; start investigations there before touching assets/scenes.
- **Observed:** Many systems communicate through `EventBus`; changes in one area can affect quests/UI/time/combat indirectly.
- **Observed:** Save/load depends on `ISaveable` discovery at runtime (`FindObjectsByType<MonoBehaviour>().OfType<ISaveable>()`), so scene object presence/order matters for persistence behavior.
- **Observed:** State-driven entity behavior is a foundational pattern across player, NPC, and mobs; new behavior often maps to new/extended states.
- **Observed:** Inventory acts as a dependency hub (items, crafting, worker sustenance, equipment usage, UI updates).
- **Unclear areas to verify when task-specific:**
  - Extent of implemented guild-building/room-expansion gameplay flow.
  - Full production use of all plugin folders (especially `Demigiant` and `NavMeshPlus`) beyond project presence.
  - Final authority boundaries between docs and runtime code when they differ (ode represents current implementation; documentation represents intended architecture and accumulated project knowledge. When they conflict, the discrepancy should be investigated rather than automatically assuming either is correct.).