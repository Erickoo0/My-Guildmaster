# Architecture Analysis (Observed Implementation)

This document is an implementation-grounded architectural reference for the current Unity project. It is based on direct inspection of core runtime code under `Assets/_Project/System` plus contextual docs (`AGENTS.md`, `Docs/AI_CONTEXT.md`).

I explicitly mark statements as:
- **Observed:** directly supported by inspected code.
- **Inference:** reasonable interpretation from observed code paths, but not fully proven end-to-end in one file.
- **Ambiguous:** behavior cannot be fully confirmed from inspected files alone.

---

## 1) Overall Architecture

### Architectural style
- **Observed:** The project is a **hybrid modular gameplay architecture** built around:
  1. `MonoBehaviour` controllers/managers for runtime orchestration,
  2. `ScriptableObject` assets for static/content data,
  3. a static `EventBus` for cross-system signaling,
  4. selective singletons for globally accessed managers,
  5. interface boundaries (`ISaveable`, `IInteractable`, `IDamagable`, `ITargetable`) where needed.

### Major subsystems and responsibilities
- **Entity subsystem** (`System/Entity`): movement, animation, targeting, state-driven behavior for player/NPC/mobs.
- **Combat subsystem** (`System/Combat`): skills, skill trees, skill execution states, hit detection, effects, health/mana/level stats, hit feedback.
- **Item/Inventory subsystem** (`System/Item`): item definitions, runtime item instances, inventory storage, equipment usage, crafting hooks.
- **Dialogue subsystem** (`System/Dialogue`): dialogue flow control, line progression, option selection, dialogue-triggered actions.
- **Quest subsystem** (`System/Quest`): active/completed quest tracking, objective progress from world/combat/item/time signals.
- **World Time subsystem** (`System/World Time`): simulated time progression and periodic broadcast.
- **Save subsystem** (`System/Save`): scene-wide discovery of saveables, JSON serialization/deserialization.
- **Environment/VFX/Dungeon/Worker**: gameplay-specialized systems consuming events and global state.

### Communication model
- **Observed:** Two dominant channels coexist:
  1. **Direct references/singleton access** for high-frequency or authoritative operations (e.g., `InventoryManager.Instance`, `StateMachine` on controller, direct component references).
  2. **`EventBus` pub/sub** for cross-subsystem notifications (`OnWorldTimeChanged`, quest updates, menu open/close, entity death, crafting, floating text).

### High-level dependency overview

```text
Input System
  -> Player Controller / Dialogue Input / UI toggles

ControllerBase (Player/NPC/Mob)
  -> StateMachine -> State classes
  -> EntityMover / EntityAnimator / SkillController

SkillController + SkillState
  -> SkillData + SkillTree(+Ledger) -> SkillDataInstance
  -> HitBox / Effects -> IDamagable / Stats(Health)
  -> EventBus (hit impact, floating text, death)

InventoryManager (singleton)
  -> ItemDataSo + ItemInstance
  -> PlayerEquipment / UI / Quest progress (EventBus)
  -> SaveData via ISaveable

QuestManager (singleton)
  <- EventBus (item updates, death, time/stat/flag)
  -> EventBus (quest UI refresh)
  -> SaveData via ISaveable

WorldTime
  -> EventBus.OnWorldTimeChanged
  -> NPC schedule / quests / world visuals / time UI

SaveManager (singleton)
  -> Find all ISaveable at runtime
  -> SaveData JSON file
  -> Call Populate/Load on each saveable
```

### Responsibility overlap
- **Observed:** Some systems are cleanly bounded (e.g., `WorldTime` only advancing and publishing time), while others act as hubs:
  - `InventoryManager` owns storage and also triggers quest progression events.
  - `QuestManager` depends on many event categories (items/death/time/stats/flags), making it behaviorally central.
  - `PlayerEquipment` depends tightly on inventory slot events and also item-use execution.

---

## 2) Entity Architecture

### Base entity and controllers
- **Observed:** `ControllerBase` is the common runtime hub for controlled entities:
  - Requires `EntityMover` + `StateMachine`.
  - Caches `EntityAnimator`.
  - Bridges Unity `Update/FixedUpdate` to state machine methods.
  - Implements `ITargetable` with serialized `entityID`.

### State machine ownership and transitions
- **Observed:** `StateMachine` owns `CurrentState` and `PreviousState` tracking primitives, and performs `Enter/Exit/Update/PhysicsUpdate` dispatch.
- **Observed:** Controllers set up concrete state instances in `Awake`, then call `SetupState(startState)`.
- **Observed:** Transitions happen inside controllers/states by calling `StateMachine.ChangeState(...)`.

### Player path
- **Observed:** `ControllerPlayer`:
  - Routes input (`OnMove`, `OnDash`, `OnPoint`, skill triggers).
  - Maintains public state-readable data (`MovementInput`, `WorldMousePosition`).
  - Holds references to `SkillControllerPlayer`, stats provider, and impulse source.
  - Subscribes to `EventBus.OnPlayerMovementToggleRequested` (UI/menu systems can globally stop movement).
- **Observed:** Skill states are implemented as `PlayerSkillStateBase : State<ControllerPlayer>` and become real states inside the same player state machine.

### Mob/enemy path
- **Observed:** `ControllerEntity`:
  - Uses targeting radius + whitelist IDs (`TargetableList`) via `ITargetable`.
  - Caches `SkillControllerEntity`, `AILerp`, rigidbody, and state set (`Spawn`, `Idle`, `Wander`, `Chase`, `Kite`).
  - Scans for targets using `Physics2D.OverlapCircle` and ID matching.
  - Manages alert icon timer independently from state logic.
- **Inference:** Combat AI behavior is split between targeting in controller and tactical decisions in state classes.

### NPC path
- **Observed:** `NPCController` mirrors the same controller+state-machine pattern, but state set is schedule-oriented (`Home`, `Sleep`, `Hobby`, `Work`) plus override states.
- **Observed:** `NPCScheduleController` (from event references) consumes world-time events to drive scheduled states.
- **Observed:** Override states are periodically evaluated by priority and can preempt schedule states.

### Movement/combat/animation/AI interaction
- **Observed:** States are the integration point:
  - movement via `EntityMover`,
  - animation via `EntityAnimator`,
  - combat via skill state transitions and effect execution,
  - AI via state selection/evaluation.
- **Observed:** Player skill execution is state-based, so casting and movement lock are enforced through state transitions, not a separate combat mode manager.

---

## 3) Combat Architecture

### Static data vs runtime execution
- **Observed static data:** `SkillData` `ScriptableObject` stores skill metadata, base numbers, prefab, animation tag, effect list, and requirements.
- **Observed runtime compiled data:** `SkillDataInstance` is built from `SkillData` (and potentially modified by skill tree compilation) and used during execution.

### Skill trigger flow (player)

```text
Input action (M1/Q/E/R/F)
  -> ControllerPlayer.OnAttackX
  -> SkillControllerPlayer.TryTriggerSkill(slot, context)
       checks: placement mode, key state, current player state,
               cooldown, mana, slot validity
  -> StateMachine.ChangeState(PlayerSkillStateBase-derived state)
  -> Skill state Enter(): start animation + cast logic
  -> Animation event callback triggers skill-specific execution
  -> HitBox/effects pipeline applies impact/damage/status
```

- **Observed:** `PlayerSkillStateBase` subscribes to skill-tree ledger change events and recompiles `SkillDataInstance` when relevant.
- **Observed:** Cast can be canceled if key is no longer held during cast-bar phase.

### Hit/effect resolution
- **Observed:** `HitBox` is abstract and handles generic collision filtering and effect execution:
  - layer filtering (`VictimLayer`), self-hit prevention, wall handling,
  - `EffectPayload` creation with user/target/hit direction/skill runtime data,
  - iterate `EffectsList` and execute,
  - post-hit bookkeeping (hit-once list, max targets, destroy behavior),
  - event emission (`EventBus.RequestHitImpact`).

### Damage/stats/death
- **Observed:** `Health` stores current/max HP, emits HP change events and floating text request, and publishes `OnEntityDeathRequested` on death.
- **Inference:** Concrete damage effects likely mutate `Health.HpCurrent` through interfaces (e.g., `IDamagable`), with combat impact feedback driven by EventBus.

### Player vs entity combat differences
- **Observed:** Player skills are input-triggered and integrated as player states (`PlayerSkillStateBase`).
- **Observed:** Entity skills are represented by `SkillControllerEntity` and entity state classes (e.g., `EntitySkillStateBase` from event usage), likely AI-triggered.
- **Inference:** Shared lower layers (skill data/effects/hitbox) are reused, while trigger policies differ (input vs AI state logic).

### Skill tree modifiers
- **Observed:** Skill tree changes emit `OnSkillTreeLedgerChanged(skillDataID)`.
- **Observed:** Skill states (player and entity bases) listen and refresh their compiled runtime skill instance.
- **Observed:** `SkillTreeLedgerController` persists ledger allocation state via `ISaveable`.

---

## 4) Data Architecture

### Data categories
- **Observed static/config data:** `ScriptableObject` assets (`SkillData`, item data assets, quest database assets, dialogue data assets, etc.).
- **Observed runtime instance data:** `ItemInstance`, `SkillDataInstance`, active quest runtime objects (`QuestActive`), state machine current state.
- **Observed runtime mutable state:** inventory slot array, player HP/MP/level, active/completed quests, skill tree ledgers, current dialogue node/line, current world time.
- **Observed persisted state:** centralized `SaveData` JSON fields (player stats/position, inventory slots, skill tree ledgers, quest states, opened chests, location string, etc.).

### Configuration-to-runtime flow examples
- **Skills:** `SkillData` asset -> `CreateSkillDataInstance()` -> modified by skill tree compiler -> used by skill states/effects.
- **Items:** `ItemDataSo` asset -> `ItemInstance` in inventory slots -> active item object/effects usage -> serialized by item ID + stack.
- **Quests:** quest database lookup by quest ID -> `QuestActive` runtime object -> objective updates from events -> serialized as `SavedQuest`.
- **Entities:** serialized IDs and components on scene objects -> controller setup -> state machine runtime behavior.

### Ambiguities to keep in mind
- **Ambiguous:** exact persistent location-state handling (`_locationCurrent`) and full scene-transition contracts were not fully traced from all caller sites.

---

## 5) Event Architecture (`EventBus`)

### What `EventBus` actually owns
- **Observed:** `EventBus` is a static event aggregator exposing event fields and `Request...` methods for:
  - world time/day,
  - dialogue-triggered events,
  - game flags/stats,
  - menu open/close + movement toggle,
  - quest updates,
  - combat feedback/death,
  - skill-tree ledger updates,
  - worker sustenance,
  - crafting + recipe unlock.

### Publisher/subscriber relationships (selected concrete paths)
- **World time publishes** `OnWorldTimeChanged`; subscribers include NPC scheduling, quest checks, world light, calendar, time UI, light props.
- **Inventory publishes** quest objective updates when adding items; `QuestManager` subscribes.
- **Health publishes** entity death and floating text requests; subscribers include quest manager, dungeon enemy tracker, VFX manager, player stats manager, floating text manager.
- **UI systems publish** menu open/close requests; menu-aware systems (including dialogue forced close handling) subscribe.
- **Skill tree publishes** ledger-changed events; skill states recompute runtime skill instances.
- **Recipe manager publishes** unlock events; crafting UI consumes them.

### Dependency implications
- **Observed:** EventBus reduces direct references for notification-style cross-domain updates.
- **Observed:** It also creates **implicit coupling**: many systems depend on event names/payload shapes and subscription timing.
- **Inference:** Debugging event order/lifecycle issues can be non-trivial because dependencies are distributed and not obvious from constructors/fields.

### Where direct references are still used
- **Observed:** Core control paths remain direct: controller -> components, singletons (`InventoryManager.Instance`, `DialogueManager.Instance`, etc.), state machine references, direct database access.

---

## 6) Manager Architecture

### Pattern overview
- **Observed:** Many “manager” classes use singleton access with scene-local object lifetime checks in `Awake`.

### Key managers and scope
- `SaveManager` (singleton): global save/load + scene transition orchestration.
- `InventoryManager` (singleton): global inventory data structure + item operations + save integration + UI events.
- `QuestManager` (singleton): active/completed quest runtime authority + objective processing + save integration.
- `DialogueManager` (singleton): dialogue session state machine + dialogue UI orchestration.
- `SkillTreeLedgerController` (singleton): skill tree allocation persistence/runtime lookup.
- `PlayerEquipment` (singleton): active inventory-slot visual/use binding for player.
- **Inference:** Additional singleton-style managers likely exist (e.g., worker/gold/player stats), matching the same pattern.

### Architectural observations
- **Observed strength:** global access is practical for a game where many systems need cross-cutting services quickly.
- **Observed concern:** some managers are both **state authority + integration hub** (not inherently wrong, but complexity accumulates there), especially inventory and quest.
- **Observed non-issue:** singleton usage itself is consistent with project style and does not automatically imply failure.

---

## 7) Save/Load Architecture

### Core flow

```text
SaveManager.SaveGame()
  -> create SaveData container
  -> FindObjectsByType<MonoBehaviour>().OfType<ISaveable>()
  -> each saveable PopulateSaveData(saveData)
  -> JsonUtility.ToJson(saveData)
  -> write Save.json

SaveManager.LoadGame()
  -> read Save.json
  -> JsonUtility.FromJson<SaveData>()
  -> discover ISaveables in current scene
  -> each saveable LoadFromSaveData(loadedData)
```

### Discovery model
- **Observed:** Saveables are discovered dynamically each save/load call through scene object scanning, not via explicit registration.

### Serialization model
- **Observed:** One monolithic `SaveData` DTO holds multiple subsystem fields.
- **Observed:** saveables read/write only relevant subsets (e.g., `InventoryManager`, `QuestManager`, `PlayerStatsPersistance`, `SkillTreeLedgerController`, `ItemContainer`, etc.).

### Scene/runtime lifecycle dependency
- **Observed:** `TransitionScene()` loads scene asynchronously and calls either `NewGame()` or `LoadGame()` after load completion.
- **Inference:** Correct restore depends on required saveable scene objects being present and initialized by the time load iteration runs.

### Limitations/risks
- **Observed risk:** scan-based discovery means object presence and execution timing are critical.
- **Observed risk:** shared `SaveData` write surface can cause accidental field overwrite or ordering sensitivity if multiple saveables target same fields.
- **Ambiguous:** there is no inspected explicit versioning/migration for evolving save schema.

---

## 8) Inventory / Item Architecture

### Ownership model
- **Static item definition:** `ItemDataSo` asset (ID, properties, usability/effects, prefab references).
- **Runtime item instance:** `ItemInstance` class (`DataSo` + mutable `stackSize`).
- **Inventory state authority:** `InventoryManager.itemsList` array.

### Runtime flow

```text
Item pickup/reward/etc
  -> InventoryManager.AddItems(ItemInstance)
     -> stack merge or empty slot placement
     -> OnSlotUpdated / OnItemAddedToInventory events
     -> EventBus.RequestUpdateQuestObjective(itemID, amount)

Active slot change
  -> InventoryManager.OnActiveSlotIndexChanged
  -> PlayerEquipment.SetActiveSlotIndex
  -> instantiate active item object from ItemDataSo

Use item
  -> PlayerEquipment.TryUseActiveItem
  -> ItemDataSo.Use(itemInstance, player)
  -> on success remove stack from inventory
```

### Save/load integration
- **Observed:** inventory serializes to `List<SavedSlot>` storing slot index + item ID + stack size.
- **Observed:** load reconstructs `ItemInstance` using `ItemDatabase.GetItem(itemID)`.

### Crafting/equipment/usage coupling
- **Observed:** crafting uses EventBus request/response (`OnCraftItemRequested`) and inventory as sink/source.
- **Observed:** equipment is a consumer of inventory events and is tightly synchronized with active slot.

---

## 9) UI and Gameplay Communication

### Communication patterns
- **Observed:** UI interaction mostly uses event-driven menu requests and direct manager calls for data rendering.

### Concrete links
- **Dialogue UI:** `DialogueManager` opens/closes dialogue panel through EventBus and maintains option-selection focus with `EventSystem`.
- **Inventory/Quest/SkillTree/Pause/Stats menus:** request open/close through EventBus menu events.
- **Player movement gating:** player controller subscribes to movement-toggle event, allowing UI/menu systems to disable movement safely.
- **World time UI/calendar/light:** subscribe to `OnWorldTimeChanged`.
- **Combat feedback UI/VFX:** floating text and hit impact events feed dedicated display/effects systems.

### Direct-reference vs event split
- **Observed:** UI view updates often depend on direct subscriptions to manager events (`OnSlotUpdated`, etc.), while cross-domain control signals (menu, quest refresh) route through EventBus.

---

## 10) Architectural Strengths

1. **Reusable state-machine-driven entity behavior (Observed):**
   - Common controller/state scaffolding supports player, NPC, and mobs with specialized states.

2. **Strong data-driven skill/item foundation (Observed):**
   - `ScriptableObject` static data + runtime instance compilation enables skill/item customization without hardcoding all behavior in controllers.

3. **Practical event integration layer (Observed):**
   - EventBus provides clear publish points for cross-system updates (time, death, quests, UI, crafting, VFX).

4. **Simple persistence contract (Observed):**
   - `ISaveable` keeps per-system save responsibilities local while `SaveManager` handles orchestration.

5. **Development velocity alignment (Inference):**
   - Singletons plus event hooks likely support quick feature iteration, consistent with project stage.

---

## 11) Architectural Weaknesses (Meaningful Only)

### Critical
1. **Save/load ordering and presence sensitivity (Observed):**
   - Saveables are discovered dynamically at runtime; restoration correctness depends on scene object availability/timing.
   - Missing required object in scene can silently skip restore for that domain.

2. **Monolithic shared save container mutation (Observed):**
   - Multiple systems write into one `SaveData` object; ownership boundaries are informal.
   - This increases risk of accidental overwrite or future schema drift side effects.

### Significant
1. **EventBus implicit dependency web (Observed):**
   - Heavy cross-domain subscriptions (quest/time/combat/UI/worker/etc.) make behavior dependent on lifecycle correctness and event ordering.

2. **Hub managers mixing data authority and orchestration (Observed):**
   - `InventoryManager` and `QuestManager` are both data stores and broad integration hubs, increasing change surface and debugging complexity.

3. **State/event lifecycle coordination burden (Observed):**
   - Skill states subscribe to global events and must unsubscribe correctly (`OnDestroy` paths); subtle lifecycle mistakes would be hard to detect.

### Minor
1. **Some naming/placement inconsistency (Observed):**
   - e.g., `ScriptableObject` folders containing non-`ScriptableObject` runtime classes/interfaces.
   - This mainly affects discoverability, not runtime correctness.

2. **Scattered direct singleton usage (Observed):**
   - Acceptable for now, but grows cognitive load as system count rises.

---

## 12) Architectural Risk Areas for Future AI Agents

1. **Save/Load pipeline (`SaveManager`, `ISaveable`, `SaveData`)**
   - **Why risky:** scene/object lifecycle coupling + shared DTO mutation; easy to introduce partial restore bugs.

2. **EventBus contract surface**
   - **Why risky:** broad fan-out; changing event payloads/names/subscription timing can break distant systems (quests, UI, time, VFX, dungeon).

3. **Inventory as dependency hub**
   - **Why risky:** inventory impacts UI, equipment, quest progress, crafting, and save/load simultaneously.

4. **State-machine + skill-state lifecycle**
   - **Why risky:** transitions, animation event timing, cast cancellation, cooldown/mana checks, and event subscriptions all interact.

5. **ScriptableObject/runtime boundaries (skills/items/quests)**
   - **Why risky:** confusing static asset data with runtime mutable instances can cause shared-state bugs.

6. **QuestManager objective update logic**
   - **Why risky:** central collector of item/death/time/stat/flag signals; behavior bugs here affect progression globally.

7. **UI/gameplay movement-gating and menu closure flow**
   - **Why risky:** menu events and forced-close logic influence player control state and dialogue/session state.

---

## Final Practical Notes

- **Observed project direction:** architecture already has a meaningful modular core with pragmatic global access/event signaling.
- **Inference:** the most important future reliability gains are likely around persistence robustness and event-contract governance, rather than replacing established core patterns.
- **Ambiguous areas to verify before major changes:** full scene graph assumptions for saveables, exact item/skill effect implementations, and any non-inspected manager scripts that share SaveData/EventBus responsibilities.
