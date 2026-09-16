# Architecture

## 1. Overview

The project uses a modular, systems-driven Unity architecture.

Major patterns include:

- MonoBehaviour controllers and composition for runtime behavior
- State machines for player, NPC, and enemy behavior
- ScriptableObjects for static/configuration data
- Runtime classes for mutable gameplay state
- Interfaces for important system boundaries
- A central EventBus for cross-system communication
- Singleton managers for genuinely global systems

The architecture is pragmatic rather than strictly following a single architectural pattern. Do not refactor established patterns without a concrete benefit.

---

## 2. Core Architecture

### Entities

Player, NPCs, and enemies are built around controller classes and reusable components.

Controllers coordinate:

- State machines
- Movement
- Combat
- Interaction
- Stats
- Equipment
- Other entity-specific systems

Behavior is generally implemented through states rather than large controller classes.

### State Machines

Entities use a generic state machine architecture.

Typical flow:

Controller → StateMachine → Current State → State behavior

States can transition through the state machine and interact with their owning controller.

When adding new entity behavior, prefer extending the existing state architecture rather than creating a parallel behavior system.

---

## 3. Data Architecture

The project separates static data from runtime state.

ScriptableObjects are primarily used for static/configuration data such as:

- Skills
- Skill modifiers
- Skill trees
- Effects
- Items
- Quests
- Dialogue data

Runtime classes represent mutable gameplay state such as:

- Item instances
- Inventory state
- Entity state
- Combat state

Be careful not to place mutable runtime state directly into shared ScriptableObject assets.

---

## 4. System Communication

The project uses three primary forms of communication:

### Direct References

Used when one object has a clear dependency on another.

### Interfaces

Used to define reusable boundaries such as:

- `ISaveable`
- `IInteractable`
- `IDamagable`

### EventBus

`EventBus` provides global event-based communication between systems.

It is used by systems including:

- Combat
- Quests
- UI
- World Time
- Crafting
- Workers

When modifying an EventBus event, inspect both publishers and subscribers because changes can affect systems indirectly.

Do not replace EventBus usage simply because direct references or another architecture could theoretically be cleaner.

---

## 5. Major System Relationships

### Combat

Static combat data is stored in ScriptableObjects.

General architecture:

Skill Data → Skill Controller → Skill Execution → Hitbox/Projectile/Effect → Damage/Stats

Player and entity combat use separate skill controllers while sharing underlying combat concepts.

### Inventory / Items

The item system separates item definitions from runtime item instances.

General relationship:

ItemData → ItemInstance → Inventory → Equipment/Crafting/Usage

Inventory interacts with multiple systems including:

- Crafting
- Equipment
- Workers
- UI
- Save/Load

`InventoryManager` is therefore an important dependency hub.

### Quest / Dialogue

Quest and dialogue systems use data-driven definitions and interact with global events.

Quest progression can respond to events such as:

- Entity deaths
- Flags
- Stats
- World time

### World Time

`WorldTime` controls simulated time and broadcasts updates used by other systems such as:

- Calendar
- UI
- Lighting
- Locations
- Quests

### Dungeon

`DungeonController` manages dungeon activation, rounds, enemy tracking, and spawning.

---

## 6. Save / Load

Persistence is based on the `ISaveable` interface.

`SaveManager` discovers saveable MonoBehaviours at runtime, serializes their state to JSON, and restores it during loading.

Current implementation depends on runtime object discovery and scene object lifecycle.

Therefore, changes involving:

- Scene objects
- Runtime instantiation
- Object destruction
- Saveable components
- Initialization order

should be reviewed carefully for save/load implications.

---

## 7. Managers and Singletons

Several major systems use singleton managers, including:

- `InventoryManager`
- `SaveManager`
- `QuestManager`
- `DialogueManager`
- `WorkerManager`

These provide convenient access to genuinely global systems but can create hidden dependencies.

Do not remove or replace singleton managers automatically.

Only consider changing them when they create a concrete problem such as:

- Difficult testing
- Excessive coupling
- Conflicting responsibilities
- Initialization problems
- Significant difficulty extending the system

---

## 8. Architectural Risk Areas

### High Priority

**Save/Load**
- Runtime discovery and object lifecycle can affect persistence.
- Changes to saveable objects should be tested carefully.

**EventBus**
- Creates indirect dependencies between systems.
- Changes to events may have effects far from the modified code.

**ScriptableObject / Runtime State Boundary**
- Shared ScriptableObject assets must not accidentally become containers for per-instance mutable state.

### Medium Priority

**InventoryManager**
- Integrates with many systems and should be changed carefully.

**State Machine Lifecycle**
- State transitions and Unity lifecycle events can interact in non-obvious ways.

---

## 9. Architectural Principles

When modifying the project:

1. Understand the existing system before changing it.
2. Prefer existing architecture when it is appropriate.
3. Improve architecture when there is a meaningful benefit.
4. Avoid refactoring merely for theoretical cleanliness.
5. Avoid introducing unnecessary abstractions.
6. Keep system responsibilities clear.
7. Treat source code as the authority for current behavior.
8. Treat documentation as architectural guidance and accumulated project knowledge.
9. When code and documentation disagree, investigate the discrepancy rather than blindly trusting either one.