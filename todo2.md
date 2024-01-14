## main
- [x] item material
- [x] item enchantments
- [x] house board
- [x] respawning
- [x] random encounters
- [x] weather
- [x] town quests
- [x] PCCs & ring light
- [x] journal
- [x] scene playback
- [x] intro/first pet event
- [x] riding
- [ ] magic/effects
  + [ ] buffs
  + [ ] timestop
  + [ ] spell stock check
  + [ ] rods
  + [ ] add all effect alignments
  + [ ] add all effect max ranges
  + [ ] remove/refactor old effect system
  + [ ] move spell/action skills into effect prototype files
  + [ ] bomb ammo enchantment
  + [ ] curse state teleportation/effects
  + [ ] effect damage that chains multiple effects
  + [ ] magic items
- [ ] inventory/spell/skill shortcuts
- [x] wishing
- [x] mefs
  + [x] potion puddle
  + [x] retain original mef creator for magic
- [x] living weapons
- [ ] cargo
- [ ] all item events/properties
  + [ ] corpses
  + [ ] card/figure/sandbag drawables
- [ ] AI/calm actions
- [ ] all commands
- [ ] buildings/zones
- [ ] all unique areas
  + [ ] jail/cursed return
  + [ ] shelter
- [ ] house guests
- [ ] house upgrading
- [ ] all chara events/properties
- [ ] religion
- [ ] water tiles
- [ ] animated tiles
- [ ] enemies follow player up/down stairs
- [ ] main quest/sidequests/dialogs
  + [ ] "main quest" separate from "scenario"
    * what happens is you may want to create a scenario that starts the player at mid-game in the middle of another main quest
    * however OpenNefia/LÖVE always tracked which "main quest" was active by scenario ID
    * so this time two scenarios should be able to track the same "main quest"
  + [ ] guilds
- [ ] all villager roles/dialog options
- [ ] all interact options/inventory contexts
- [ ] status effect protoevents -> ECS
  + [ ] gravity/floating buff
  + [ ] fear prevents melee attacking
  + [ ] all other status effect checks in HSP code
- [x] staying charas
- [ ] nefia generation refactor
- [ ] elonaextender features
  + [ ] save backup
- [ ] arena/pet arena/show house
- [ ] spell tracker
- [ ] the void
- [ ] blackjack
- [ ] deck/tcg
- [ ] noyel festival
- [ ] correct prototype ordering (spells, etc.)
- [ ] custom talk
- [ ] theming
- [ ] port all OpenNefia/LÖVE unit tests
- [ ] check rest of OpenNefia/LÖVE event handlers
- [ ] loc manager doesn't re-watch if language is switched
- [ ] non-compiled mods
- [ ] Qy@

## enhancements
- [ ] mod API examples
- [ ] update NuGet packages
- [ ] robust upstream commits
- [ ] serializer strict mode
  + [ ] throw if unknown field is found in mapping
  + [ ] throw if duplicate prototype IDs are detected
- [] add new effect event type for checking/modifying area positions (`ApplyEffectPositionsEvent`)
  + pass the positions list to the new event
  + this should allow specifying which effect animation to show from YAML
  + this way you can show the ball animation for the web area or the bolt area
  + effect area components run their own logic inside the positions event (FOV checking for each tile, etc.) 
- [ ] how to combine more than one effect damage
  + each individual effect handles afterward with the turn result
  + events could also handle with failure to block running damage
  + in the future handled events will be exited out of early so this needs to be changed
- [ ] "tile data" API (register a 2D array of data that is automatically instantiated/resized along with map)
- [ ] entity gen interfaces `SpawnEntity` -> `TrySpawnEntity` everywhere
- [ ] make event subscriptions declarative (no more `IEntitySystem.Initialize()`) for better hot reloading
- [ ] new effect event for modifying returned area positions and showing a message
- [ ] data definition for "randomly pickable list" (element associated with weight)
- [ ] "start engine with this mod" compile targets
- [ ] autodetect dependent mods during debugging and mount resource folders for hotloading
- [ ] handled event changes
  + all `EntityEventArgs` should come with a `Handled` property
  + if `Handled` is set to `true`, the rest of the event handlers are skipped unless the `alwaysRun` flag is set on them
  + what this allows: events that modify the handled results later
    - example: an `EffectSummonEvent` returns a summoned character and handles the event.
    - an `alwaysRun` event can modify the level of the character afterwards.
      + in Elona+ CGX the summoned characters are set to have a friendly relation to the player.
    - other events will not run (summon other characters).
  + can no longer raise arbitrary class instances as events (but can do so for structs still)
    - all events must derive from `EntityEventArgs` so `Handled` is always available
- [ ] mod "feature flags"
  + mods can register a flag name for a specific behavior of a method overridden with Harmony or similar
  + example: you can pick "Elona.122", "Elona.Plus" or "Elona.Omake" for a "Elona.CalcQuestDifficulty" flag
  + this is so you don't have to inherit all of a variant's behavior by installing the mod and only keep the behaviors you want. 
  + it allows for picking and choosing how you want the game to act, hence "building your own Elona".
  + sets of feature flags compatible with a set of mods can be saved into one file and loaded in a profile
- [ ] "garbage collectable" components (all flags inside the component are false, etc.)
- [ ] remaining TODOs

## debug
- [ ] edit game save data in one debug view

## analyzers
- [ ] data definitions must have a zero-arg constructor
- [ ] any components with `Stat<T>` must implement `IComponentRefreshable`
- [ ] `[DataField]` types must be serializable
- [ ] `HandleableEntityEventArgs.Handled` must be checked in all event handlers
- [ ] use primitive type functions (`int.Clamp`) instead of `Math.Clamp` and similar
- [ ] prevent reserved `[DataField]` names (`id`, `type`, `events`, ...)
- [ ] component event registrations where `EventUsage` and `ComponentUsage` do not match

## mods
- [ ] visual ai
- [ ] FFHP items
