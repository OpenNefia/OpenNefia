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
- [ ] magic/buffs/effects
  + [ ] timestop
- [ ] AI/calm actions
- [x] living weapons
- [ ] cargo
- [ ] magic items
- [ ] all item events/properties
  + [ ] corpses
  + [ ] card/figure/sandbag drawables
- [ ] buildings/zones
- [ ] all unique areas
- [ ] house guests
- [ ] house upgrading
- [ ] all chara events/properties
  + [ ] custom talk
- [ ] religion
- [ ] effect protoevents -> ECS
- [ ] main quest/sidequests/dialogs
- [ ] all villager roles/dialog options
- [ ] all interact options/inventory contexts
- [x] staying charas
- [ ] arena/pet arena
- [ ] nefia generation refactor
- [ ] the void
- [ ] noyel festival
- [ ] blackjack
- [ ] deck/tcg
- [ ] theming
- [ ] port all OpenNefia/LÖVE unit tests
- [ ] non-compiled mods
- [ ] Qy@
- [ ] mod API examples
- [ ] robust upstream commits
- [ ] "start engine with this mod" compile targets
- [ ] autodetect dependent mods during debugging and mount resource folders for hotloading
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
