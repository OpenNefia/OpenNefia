--
-- This file is meant to be run in "REPL batch" mode with the old Lua prototype
-- of OpenNefia (https://github.com/Ruin0x11/OpenNefia). It is used for converting
-- the old Lua-based prototype format to the new YAML-based one.
--
-- It's run like this:
--
-- cd C:/users/yuno/build/elona-next && bash OpenNefia exec C:/users/yuno/build/OpenNefia.NET/Support/convert_lua_protos.lua -r
--

local automagic = require "thirdparty.automagic"
local fs = require "util.fs"
local lyaml = require "lyaml"
local Enum = require "api.Enum"
local Log = require "api.Log"
local IItemEquipment = require "mod.elona.api.aspect.IItemEquipment"
local IItemMeleeWeapon = require "mod.elona.api.aspect.IItemMeleeWeapon"
local IItemRangedWeapon = require "mod.elona.api.aspect.IItemRangedWeapon"
local IItemAmmo = require "mod.elona.api.aspect.IItemAmmo"
local IItemFood = require "mod.elona.api.aspect.IItemFood"
local IItemSpellbook = require "mod.elona.api.aspect.IItemSpellbook"
local IItemAncientBook = require "mod.elona.api.aspect.IItemAncientBook"
local IItemFromChara = require "mod.elona.api.aspect.IItemFromChara"

local rootDir = "C:/build/OpenNefia"

local tags = {}

local function capitalize(str)
    return str:gsub("^%l", string.upper)
end

local function camelize(str)
    return str:gsub("%W+(%w+)", capitalize)
end

local function classify(str)
    return str:gsub("%W*(%w+)", capitalize)
end

local replacements = {
    ["stat_life"] = "attr_life",
    ["stat_mana"] = "attr_mana",
    ["stat_strength"] = "attr_strength",
    ["stat_constitution"] = "attr_constitution",
    ["stat_dexterity"] = "attr_dexterity",
    ["stat_perception"] = "attr_perception",
    ["stat_learning"] = "attr_learning",
    ["stat_will"] = "attr_will",
    ["stat_magic"] = "attr_magic",
    ["stat_charisma"] = "attr_charisma",
    ["stat_speed"] = "attr_speed",
    ["stat_luck"] = "attr_luck",
}

local function dotted(str)
    local mod_id, data_id = str:match "([^.]+)%.([^.]+)"
    data_id = replacements[data_id] or data_id
    local result = ("%s.%s"):format(classify(mod_id), classify(data_id))
    result = result:gsub("^Elona.Stat([A-Z])", "Elona.Attr")
    return result
end

local function dottedEntity(str, ty)
    local _, ty = ty:match "([^.]+)%.([^.]+)"
    local mod_id, data_id = str:match "([^.]+)%.([^.]+)"
    return ("%s.%s%s"):format(classify(mod_id), classify(ty), classify(data_id))
end

local function itemCategory(str, ident)
    if str == "elona.no_generate" then
        return "Elona.NoGenerate"
    end
    local mod_id, data_id = str:match "([^.]+)%.([^.]+)"
    return ("%s.%s%s"):format(classify(mod_id), ident, classify(data_id))
end

local function modPart(str)
    local mod_id, data_id = str:match "([^.]+)%.([^.]+)"
    return classify(mod_id)
end

local function dataPart(str)
    local mod_id, data_id = str:match "([^.]+)%.([^.]+)"
    return classify(data_id)
end

local function dotted_keys(t)
    return fun.iter(t)
        :map(function(k, v)
            return dotted(k), v
        end)
        :to_map()
end

local function rgbToHex(rgb)
    local hexadecimal = "#"

    for key, value in pairs(rgb) do
        local hex = ""

        while value > 0 do
            local index = math.fmod(value, 16) + 1
            value = math.floor(value / 16)
            hex = string.sub("0123456789ABCDEF", index, index) .. hex
        end

        if string.len(hex) == 0 then
            hex = "00"
        elseif string.len(hex) == 1 then
            hex = "0" .. hex
        end

        hexadecimal = hexadecimal .. hex
    end

    return hexadecimal
end

local function enum(enumClass, x)
  if x ~= nil then
    return enumClass:to_string(x)
  end
  return function(i)
    return enumClass:to_string(i)
  end
end

local function timeSpan(hours, minutes, seconds)
    hours = hours or 0
    minutes = minutes or 0
    seconds = seconds or 0
    return ("%02d:%02d:%02d"):format(hours, minutes, seconds)
end

local function intRange(min, max)
    return ("%d~%d"):format(min, max)
end

local id = function(i)
    return i
end

local function field(from, to, field_name, transform, new_field_name)
    transform = transform or id

    if from[field_name] then
        local n = camelize(new_field_name or field_name)
        if n == "type" then
            error "field name must not be 'type'"
        end
        to[n] = transform(from[field_name])
    end
end

local function event(from, to, field_name, namespace, system, event_name)
    if field_name ~= nil and not from[field_name] then
        return
    end
    to.events = to.events or automagic()
    local e = automagic()
    e.type = ("%s.P_%s%sEvent"):format(namespace, dataPart(from._type), event_name)
    e.system = ("%s.%s"):format(namespace, system)
    e.method = ("%s_%s"):format(dataPart(from._id), event_name)
    table.insert(to.events, e)
end

local function ext(from, iface)
    return from._ext and from._ext[iface]
end

local ops = {
    all = {
        _id = dotted,
        _type = dotted,
    },
    ["base.class"] = {
        equipment_type = dotted,
        skills = dotted_keys,
    },
    ["base.chara"] = {
        color = rgbToHex,
    },
}

local function transform(i)
    local o = ops[i._type] or {}

    local result = {}

    for k, v in pairs(i) do
        local c
        if k:sub(1, 1) == "_" then
            c = k
        else
            c = camelize(k)
        end
        local f = o[k] or ops.all[k]
        if f then
            result[c] = f(v)
        else
            result[c] = v
        end
    end

    return result
end

local function tuple(t)
    return table.concat(t, ",")
end

local handlers = {}
local allTags = {}

local function comp(t, name)
    if t.components == nil then
        t.componentes = {}
    end

    for _, c in ipairs(t.components) do
        if c.type == name then
            return c
        end
    end

    local c = automagic()
    c.type = name
    t.components[#t.components + 1] = c
    return c
end

local function extData(t, name)
    if t.extendedData == nil then
        t.extendedData = {}
    end

    for _, c in ipairs(t.extendedData) do
        if c.type == name then
            return c
        end
    end

    local c = automagic()
    c.type = name
    t.extendedData[#t.extendedData + 1] = c
    return c
end

handlers["base.chara"] = function(from, to)
    local c = comp(to, "Chip")
    if from.image then
        c.id = dotted(from.image)
    end
    if from.color then
        c.color = rgbToHex(from.color)
    end

    c = comp(to, "Level")
    c.level = from.level or 1

    if from.quality then
        c = comp(to, "Quality")
        c.quality = Enum.Quality:to_string(from.quality)
    end

    if not table.set(from.categories or {})["elona.no_generate"] then
        c = comp(to, "RandomGen")
        c.tables.chara = {
            coefficient = from.coefficient or 400,
            rarity = from.rarity or 100000,
        }
        if (from.fltselect or 0) ~= 0 then
            c.tables.chara.fltselect = "Elona." .. Enum.FltSelect:to_string(from.fltselect)
        end
    end

    if (from.creaturepack or 0) ~= 0 then
        c = comp(to, "CreaturePack")
        c.category = "Elona." .. Enum.CharaCategory:to_string(from.creaturepack)
    end

    if #from.tags > 0 then
        c = comp(to, "Tag")
        c.tags = {}
        for _, tag in ipairs(from.tags or {}) do
            c.tags[#c.tags + 1] = "Elona.TagChara" .. classify(tag)
        end
    end

    c = comp(to, "Chara")
    c.race = dotted(from.race)
    c.class = dotted(from.class)
    if from.gender then
        c.gender = capitalize(from.gender)
    end

    if from.skills then
        local skills
        local spells
        for _, skill in ipairs(from.skills) do
            skill = skill:gsub("%.stat_", "%.attr_")
            local ty = data["base.skill"]:ensure(skill).type
            if ty == "spell" or ty == "action" then
                if spells == nil then
                    spells = comp(to, "Spells")
                    spells.spells = {}
                end
                spells.spells[dotted(skill)] = 1
            else
                if skills == nil then
                    skills = comp(to, "Skills")
                    skills.skills = {}
                end
                skills.skills[dotted(skill)] = 1
            end
        end
    end

    c = comp(to, "Faction")
    c.relation = Enum.Relation:to_string(from.relation)

    if from.can_talk then
        c = comp(to, "Dialog")
        c.canTalk = true
    end
    if from.dialog then
        c = comp(to, "Dialog")
        c.dialogID = dotted(from.dialog)
    end
    if from.portrait then
        c = comp(to, "Portrait")
        if from.portrait == "random" then
            c.hasRandomPortrait = true
        else
            c.portraitID = dotted(from.portrait)
        end
    end

    if from.tone then
        c = comp(to, "Tone")
        c.id = dotted(from.tone)
    end

    if from.ai_calm_action or from.ai_distance ~= 1 or from.ai_move_chance ~= 100 then
        c = comp(to, "VanillaAI")
        if from.ai_calm_action then
            c.calmAction = classify(from.ai_calm_action:gsub("elona.calm_(.*)", "%1"))
        end
        if from.ai_distance ~= 1 then
            c.targetDistance = from.ai_distance
        end
        if from.ai_move_chance ~= 100 then
            c.moveChance = from.ai_move_chance / 100.0
        end
    end

    if from.is_unique then
        c = comp(to, "UniqueCompanion")
    end

    if from.has_own_name then
        c = comp(to, "CharaNameGen")
        c.hasRandomName = true
    end

    if from.unarmed_element_id then
        c = comp(to, "UnarmedDamage")
        c.damageType = setmetatable({
            elementID = dotted(from.unarmed_element_id),
            power = from.unarmed_element_power,
        }, { tag = "type:ElementalDamageType", type = "mapping" })
    end

    if from.is_immune_to_elemental_damage then
        c = comp(to, "Resists")
        c.isImmuneToElementalDamage = true
    end
    if from.splits then
        c = comp(to, "Splittable")
        c.splitsOnHighDamage = true
    end
    if from.splits2 then
        c = comp(to, "Splittable")
        c.splitsRandomlyWhenAttacked = true
    end
    if from.is_quick_tempered or table.set(from.flags or {}).IsQuickTempered then
        c = comp(to, "QuickTempered")
    end

    if from._id == "elona.rich_person" then
        c = comp(to, "KarmaValue")
        c.karmaValue = 15
        c = comp(to, "RichLoot")
        c.richLootItemCount = 8
        c = comp(to, "Money")
        c.initialGold = intRange(5000, 16000)
    elseif from._id == "elona.noble_child" then
        c = comp(to, "KarmaValue")
        c.karmaValue = 10
        c = comp(to, "RichLoot")
        c.richLootItemCount = 4
        c = comp(to, "Money")
        c.initialGold = intRange(2000, 7000)
    elseif from._id == "elona.tourist" then
        c = comp(to, "KarmaValue")
        c.karmaValue = 5
        c = comp(to, "RichLoot")
        c.richLootItemCount = 2
        c = comp(to, "Money")
        c.initialGold = intRange(1000, 4000)
    end

    if from.always_drops_gold then
        c = comp(to, "Money")
        c.alwaysDropsGoldOnDeath = from.always_drops_gold
    end

    if from.equipment_type then
        c = comp(to, "EquipmentGen")
        c.equipmentType = dotted(from.equipmentType)
    end

    if from.loot_type then
        c = comp(to, "LootType")
        c.lootType = dotted(from.loot_type)
    end

    if from.initial_equipment then
      c = comp(to, "EquipmentGen")
      c.initialEquipment = c.initialEquipment or {}
      for k, v in pairs(from.initial_equipment) do
        if v.is_two_handed then
          k = "elona.two_handed_weapon"
        end
        local itemFilter = {}
        if v._id ~= nil then
          itemFilter.id = itemCategory(v._id, "Item")
        end
        if v.quality ~= nil then
          itemFilter.quality = enum(Enum.Quality, v.quality)
        end
        if v.category ~= nil then
          itemFilter.tags = { itemCategory(v.category, "ItemCat") }
        end

        c.initialEquipment[dotted(k)] = {
          itemFilter = itemFilter
        }
      end
    end

    if from._id == "elona.beggar" then
      c = comp(to, "EquipmentGen")
      c.initialEquipment = c.initialEquipment or {}
      c.initialEquipment[dotted("elona.amulet_1")] = {
        itemFilter = { id = itemCategory("elona.beggars_pendant", "Item")}, oneIn = 120
      }
    end

    if from._id == "elona.noble" then
      c = comp(to, "EquipmentGen")
      c.initialEquipment = c.initialEquipment or {}
      c.initialEquipment[dotted("elona.ranged_weapon")] = {
        itemFilter = { id = itemCategory("elona.shenas_panty", "Item")}, oneIn = 100
      }
    end

    if from._id == "elona.asura" or from._id == "elona.mitra" or from._id == "elona.varuna" then
      c = comp(to, "EquipmentGen")
      c.initialEquipment = c.initialEquipment or {}
      c.initialEquipment[dotted("elona.amulet_1")] = {
        itemFilter = { id = itemCategory("elona.twin_edge", "Item")}, oneIn = 600
      }
    end

    if from._id == "elona.asura" then
      c = comp(to, "EquipmentGen")
      c.initialEquipment = c.initialEquipment or {}
      c.initialEquipment[dotted("elona.amulet_1")] = {
        itemFilter = { id = itemCategory("elona.twin_edge", "Item")}, oneIn = 600
      }
    end

    if from._id == "elona.rogue_archer" then
      c = comp(to, "EquipmentGen")
      c.initialEquipment = c.initialEquipment or {}
      c.initialEquipment[dotted("elona.amulet_1")] = {
        itemFilter = { id = itemCategory("elona.arbalest", "Item")}, oneIn = 250
      }
    end

    if from._id == "elona.rock_thrower" then
      c = comp(to, "EquipmentGen")
      c.initialEquipment = c.initialEquipment or {}
      c.initialEquipment[dotted("elona.ranged_weapon")] = {
        itemFilter = { id = itemCategory("elona.vanilla_rock", "Item")}, oneIn = 200
      }
    end

    if from._id == "elona.blade" or from._id == "elona.blade_alpha" or from._id == "elona.blade_omega" then
      c = comp(to, "EquipmentGen")
      c.initialEquipment = c.initialEquipment or {}
      c.initialEquipment[dotted("elona.girdle")] = {
        itemFilter = { id = itemCategory("elona.crimson_plate", "Item")}, oneIn = 800
      }
    end

    if from._id == "elona.silver_eyed_witch" then
      c = comp(to, "EquipmentGen")
      c.initialEquipment = c.initialEquipment or {}
      c.initialEquipment[dotted("elona.two_handed_weapon")] = {
        itemFilter = { id = itemCategory("elona.claymore_unique", "Item")}, oneIn = 150
      }
    end

    if from._id == "elona.minotaur_king"
      or from._id == "elona.hound"
      or from._id == "elona.hand_of_the_murderer"
      or from._id == "elona.skeleton_berserker"
      or from._id == "elona.knight"
      or from._id == "elona.rogue_warrior"
      or from._id == "elona.conery"
      or from._id == "elona.silver_eyed_witch"
    then
      c = comp(to, "GenTwoHandedWeapon")
    end

    if from._id == "elona.lomias" then
      c = comp(to, "LootType")
      c.lootDrops = {
        { itemFilter = { id = itemCategory("elona.secret_experience_of_lomias", "Item")}, oneIn = 4 }
      }
    end

    if from._id == "elona.rabbit" then
      c = comp(to, "LootType")
      c.lootDrops = {
        { itemFilter = { id = itemCategory("elona.rabbits_tail", "Item")}, oneIn = 200 }
      }
    end

    if from._id == "elona.zombie" then
      c = comp(to, "LootType")
      c.lootDrops = {
        { itemFilter = { id = itemCategory("elona.potion_of_descent", "Item")}, oneIn = 100 }
      }
    end

    if from._id == "elona.executioner" or from._id == "elona.messenger_of_death" then
      c = comp(to, "LootType")
      c.lootDrops = {
        { itemFilter = { id = itemCategory("elona.scroll_of_contingency", "Item")}, oneIn = 150 }
      }
    end

    if from._id == "elona.imp" or from._id == "elona.nether_imp" or from._id == "elona.chaos_imp" then
      c = comp(to, "LootType")
      c.lootDrops = {
        { itemFilter = { id = itemCategory("elona.scroll_of_name", "Item")}, oneIn = 600 }
      }
    end

    if from._id == "elona.lesser_mummy" or from._id == "elona.mummy" or from._id == "elona.greater_mummy" then
      c = comp(to, "LootType")
      c.lootDrops = {
        { itemFilter = { id = itemCategory("elona.book_of_resurrection", "Item")}, oneIn = 300 }
      }
    end

    if from._id == "elona.cupid_of_love" then
      c = comp(to, "LootType")
      c.lootDrops = {
        { itemFilter = { id = itemCategory("elona.happy_bed", "Item")}, oneIn = 140 }
      }
    end

    if from._id == "elona.rogue_boss" or from._id == "elona.rogue_warrior" or from._id == "elona.rogue_archer" or from._id == "elona.rogue_wizard" then
      c = comp(to, "LootType")
      c.lootDrops = {
        { itemFilter = { id = itemCategory("elona.blue_capsule_drug", "Item")}, oneIn = 90 }
      }
    end

    if from._id == "elona.hermit_crab" then
      c = comp(to, "LootType")
      c.lootDrops = {
        { itemFilter = { id = itemCategory("elona.unknown_shell", "Item")}, oneIn = 100 }
      }
    end

    if from._id == "elona.mine_dog" then
      c = comp(to, "LootType")
      c.lootDrops = {
        { itemFilter = { id = itemCategory("elona.mine", "Item")}, oneIn = 3 }
      }
    end

    if from._id == "elona.deformed_eye" or from._id == "elona.impure_eye" then
      c = comp(to, "LootType")
      c.lootDrops = {
        { itemFilter = { id = itemCategory("elona.potion_of_evolution", "Item")}, oneIn = 150 }
      }
    end

    if from._id == "elona.silver_bell" then
      c = comp(to, "LootType")
      c.lootDrops = {
        { itemFilter = { id = itemCategory("elona.small_medal", "Item")}, oneIn = 2 }
      }
    end

    if from._id == "elona.silver_bell" then
      c = comp(to, "Money")
      c.initialPlatinum = intRange(1, 4)
      c = comp(to, "LootType")
      c.lootDrops = {
        { itemFilter = { id = itemCategory("elona.small_medal", "Item")}, oneIn = 2 }
      }
    end

    if from._id == "elona.gold_bell" then
      c = comp(to, "GoldLoot")
    end
end

handlers["base.item"] = function(from, to)
    local c = comp(to, "Chip")
    if from.image then
        c.id = dotted(from.image)
    end
    if from.color then
        c.color = rgbToHex(from.color)
    end

    c = comp(to, "Level")
    c.level = from.level or 1

    if not table.set(from.categories or {})["elona.no_generate"] then
        c = comp(to, "RandomGen")
        c.tables.item = {
            coefficient = from.coefficient or 100,
            rarity = from.rarity or 1000000,
        }
        if (from.fltselect or 0) ~= 0 then
            c.tables.item.fltselect = "Elona." .. Enum.FltSelect:to_string(from.fltselect)
        end
    end

    if from.quality then
        c = comp(to, "Quality")
        c.quality = Enum.Quality:to_string(from.quality)
    end

    c = comp(to, "Tag")
    c.tags = {}
    for _, cat in ipairs(from.categories or {}) do
        if not cat:match "elona.tag_" then
            local tag = itemCategory(cat, "ItemCat")
            c.tags[#c.tags + 1] = tag
            if not allTags[tag] then
                allTags[tag] = true
                allTags[#allTags + 1] = tag
            end
        end
    end
    for _, tag in ipairs(from.tags or {}) do
        c.tags[#c.tags + 1] = "Elona.TagItem" .. classify(tag)
    end
    if from.is_wishable == false then
        c.tags[#c.tags + 1] = "Elona.NoWish"
    end
    if from.is_precious then
        c.tags[#c.tags + 1] = "Elona.Precious"
    end

    c = comp(to, "Value")
    c.value = from.value

    if from.material then
        c = comp(to, "Material")
        c.materialID = dotted(from.material)
    end

    c = comp(to, "Item")
    -- if from.originalnameref2 then
    --     c.itemTypeName = from.originalnameref2
    -- end
    if from.is_precious then
        c.isPrecious = true
    end

    if from.knownnameref then
        if from.knownnameref == "staff" then
            from.knownnameref = "rod"
        end
        c = comp(to, "RandomItem")
        c.knownNameRef = dotted("elona." .. from.knownnameref)
    end

    if from.random_color then
        c = comp(to, "RandomColor")
        if from.random_color == "Random" then
            c.randomColor = "RandomItem"
        elseif from.random_color == "Furniture" then
            c.randomColor = "Furniture"
        end
    end

    if from.weight ~= 0 then
        c = comp(to, "Weight")
        c.weight = from.weight
    end

    if from.medal_value ~= 0 then
        c = comp(to, "MedalValue")
        c.medalValue = from.medal_value
    end

    local equipment = from._ext and from._ext[IItemEquipment]
    if equipment then
        c = comp(to, "Equipment")
        c.equipSlots = {}
        for _, slot in ipairs(equipment.equip_slots) do
            c.equipSlots[#c.equipSlots + 1] = dotted(slot)
        end

        c = comp(to, "EquipStats")
        if (equipment.dv or 0) ~= 0 then
            c.dv = equipment.dv
        end
        if (equipment.pv or 0) ~= 0 then
            c.pv = equipment.pv
        end
        if (equipment.hit_bonus or 0) ~= 0 then
            c.hitBonus = equipment.hit_bonus
        end
        if (equipment.damage_bonus or 0) ~= 0 then
            c.damageBonus = equipment.damage_bonus
        end
    end

    local weapon = from._ext and (from._ext[IItemMeleeWeapon] or from._ext[IItemRangedWeapon])
    if weapon then
        c = comp(to, "Weapon")
        if (weapon.dice_y or 0) ~= 0 then
            c.diceX = weapon.dice_x
        end
        if (weapon.dice_y or 0) ~= 0 then
            c.diceY = weapon.dice_y
        end
        if (weapon.pierce_rate or 0) ~= 0 then
            c.pierceRate = weapon.pierce_rate
        end
        if weapon.skill then
            c.weaponSkill = dotted(weapon.skill)
        end
    end

    local ranged_weapon = from._ext and from._ext[IItemRangedWeapon]
    if ranged_weapon then
        c = comp(to, "RangedWeapon")
        if ranged_weapon.effective_range then
            c.rangedAccuracy = setmetatable({
                table = fun.iter(ranged_weapon.effective_range)
                    :map(function(i)
                        return i / 100.0
                    end)
                    :to_list(),
            }, { tag = "type:RangedAccuracyTable", type = "mapping" })
        end
    end

    local ammo = from._ext and from._ext[IItemAmmo]
    if ammo then
        c = comp(to, "Ammo")
        field(ammo, c, "skill", dotted, "ammoSkill")
        field(ammo, c, "dice_x")
        field(ammo, c, "dice_y")
    end

    local food = from._ext and from._ext[IItemFood]
    if food then
        c = comp(to, "Food")
        field(food, c, "food_type", dotted)
        field(food, c, "food_quality")
        field(food, c, "nutrition")
        if food.exp_gains then
            c.experienceGains = {}
            for _, gain in ipairs(food.exp_gains) do
                table.insert(c.experienceGains, {
                    skillID = dotted(gain._id),
                    experience = gain.amount,
                })
            end
        end
        field(food, c, "spoilage_hours", timeSpan, "spoilageInterval")
    end

    if from.params and from.params.bed_quality then
        c = comp(to, "Bed")
        c.bedQuality = from.params.bed_quality / 100.0
    end

    if from._id == "elona.tree_of_fruits" then
        c = comp(to, "FruitTree")
    end

    if from.params.instrument_quality then
        c = comp(to, "Instrument")
        c.performanceQuality = from.params.instrument_quality
    end

    if from._id == "elona.treasure_map" then
        c = comp(to, "TreasureMap")
    end

    if from._id == "elona.small_gamble_chest" then
        local e = extData(to, "Shopkeeper.ExtShopAmountAdjustment")
        local t = { maxAmount = 8 }
        e.adjustment = setmetatable(t, { tag = "type:RandomShopAmountAdjustment", type = "mapping" })
    end

    if from._id == "elona.bottle_of_water" then
        local e = extData(to, "Shopkeeper.ExtShopExclusion")
        e.isExcludedFromShops = true
    end

    if (from.identify_difficulty or 0) ~= 0 then
        c = comp(to, "Identify")
        c.identifyDifficulty = from.identify_difficulty
    end

    if from._id == "elona.gold_piece" then
        c = comp(to, "GoldPiece")
    end

    if from._id == "elona.platinum_coin" then
        c = comp(to, "PlatinumCoin")
    end

    if
        from._id == "elona.gold_piece"
        or from._id == "elona.platinum_coin"
        or from._id == "elona.small_medal"
        or from._id == "elona.music_ticket"
        or from._id == "elona.token_of_friendship"
        or from._id == "elona.bill"
    then
        c = comp(to, "Identify")
        c.identifyState = "Full"
        c = comp(to, "CurseState")
        c.curseState = "Normal"
        c.noRandomizeCurseState = true
    end

    if from._id == "elona.book_of_rachel" then
        c = comp(to, "BookOfRachel")
    end

    if from._id == "elona.bill" then
        c = comp(to, "Bill")
    end

    if from._id == "elona.wallet" or from._id == "elona.suitcase" then
        c = comp(to, "LostProperty")
    end

    if from._id == "elona.shopkeepers_trunk" then
        c = comp(to, "Temporal")
    end

    if from._id == "elona.shelter" then
        c = comp(to, "Shelter")
    end

    if from._id == "elona.material_kit" then
        c = comp(to, "MaterialKit")
    end

    if from._id == "elona.book" then
        c = comp(to, "Book")
    end

    if from._id == "elona.textbook" then
        c = comp(to, "Textbook")
    end

    if from._id == "elona.disc" then
        c = comp(to, "MusicDisc")
    end

    if from._id == "elona.kitty_bank" then
        c = comp(to, "MoneyBox")
    end

    if from._id == "elona.monster_ball" then
        c = comp(to, "MonsterBall")
        c.splitAmount = 1
    end

    if from.cooldown_hours then
        c = comp(to, "UseInterval")
        c.useInterval = timeSpan(from.cooldown_hours)
    end

    if
        from._id == "elona.small_gamble_chest"
        or from._id == "elona.bejeweled_chest"
        or from._id == "elona.safe"
        or from._id == "elona.suitcase"
        or from._id == "elona.wallet"
        or from._id == "elona.treasure_ball"
        or from._id == "elona.rare_treasure_ball"
        or from._id == "elona.chest"
    then
        c = comp(to, "Chest")
    end

    if from._id == "elona.small_gamble_chest" then
        c = comp(to, "SmallGambleChest")
        c = comp(to, "Chest")
        c.smallMedalProb = 0.1
        c.itemCount = 1
    end

    if from._id == "elona.safe" then
        c = comp(to, "Safe")
    end

    if from._id == "elona.wallet" then
        c = comp(to, "Wallet")
    end

    if from._id == "elona.suitcase" then
        c = comp(to, "Suitcase")
    end

    if from._id == "elona.treasure_ball" then
        c = comp(to, "TreasureBall")
        c.itemQuality = "Good"
        c = comp(to, "Chest")
        c.itemCount = 1
    end

    if from._id == "elona.rare_treasure_ball" then
        c = comp(to, "TreasureBall")
        c.itemQuality = "Great"
        c = comp(to, "Chest")
        c.itemCount = 1
    end

    if from._id == "elona.bejeweled_chest" or from._id == "elona.chest" then
        c = comp(to, "Chest")
        c.smallMedalProb = 0.2
    end

    if from._id == "elona.bejeweled_chest" then
        c = comp(to, "BejeweledChest")
        c = comp(to, "Chest")
        c.smallMedalProb = 0.2
    end

    if from._id == "elona.lockpick" then
        c = comp(to, "Lockpick")
    end

    if from._id == "elona.skeleton_key" then
        c = comp(to, "SkeletonKey")
    end

    if from.always_drop then
        c = comp(to, "AlwaysDropOnDeath")
    end

    if from._id == "elona.corpse" then
        c = comp(to, "Corpse")
    end

    if from._id == "elona.cargo_travelers_food" then
        c = comp(to, "TravelersFood")
    end

    local spellbook = from._ext and from._ext[IItemSpellbook]
    if spellbook then
        c = comp(to, "Spellbook")
        field(spellbook, c, "skill_id", dotted, "spellID")

        c = comp(to, "Charged")
        field(spellbook, c, "charges")
        field(spellbook, c, "max_charges")
        field(spellbook, c, "can_be_recharged")
    end

    local ancientBook = from._ext and from._ext[IItemAncientBook]
    if ancientBook then
        c = comp(to, "AncientBook")

        c = comp(to, "Charged")
        field(ancientBook, c, "charges")
        field(ancientBook, c, "max_charges")
        c.displayChargeCount = false
    end

    local fromChara = from._ext and from._ext[IItemFromChara]
    if fromChara then
        c = comp(to, "EntityProtoSource")
    end
end

handlers["base.feat"] = function(from, to)
    local c = comp(to, "Chip")
    if from.image then
        c.id = dotted(from.image)
    end
    if from.color then
        c.color = rgbToHex(from.color)
    end
end

handlers["base.mef"] = function(from, to)
    local c = comp(to, "Chip")
    if from.image then
        c.id = dotted(from.image)
    end
    if from.color then
        c.color = rgbToHex(from.color)
    end
end

handlers["base.class"] = function(from, to)
    if from.is_extra then
        to.isExtra = true
    else
        to.isExtra = false
    end
    if from.properties.equipment_type then
        to.equipmentType = dotted(from.properties.equipment_type)
    end
    if from.on_init_player then
        local _, name = to.id:match "([^.]+)%.([^.]+)"
        to.onInitPlayer = setmetatable({}, { tag = ("type:Init%sEffect"):format(name), type = "mapping" })
    end
    to.baseSkills = {}
    for id, level in pairs(from.skills) do
        id = id:gsub("%.stat_", "%.attr_")
        to.baseSkills[dotted(id)] = level
    end
end

handlers["base.race"] = function(from, to)
    if from.is_extra then
        to.isExtra = true
    else
        to.isExtra = false
    end
    if from.properties.breed_power then
        to.breedPower = from.properties.breed_power
    end
    if from.properties.image then
        to.chipMale = dotted(from.properties.image)
        to.chipFemale = dotted(from.properties.image)
    end
    if from.properties.male_image then
        to.chipMale = dotted(from.properties.male_image)
    end
    if from.properties.female_image then
        to.chipFemale = dotted(from.properties.female_image)
    end
    if from.male_ratio then
        to.maleRatio = from.male_ratio / 100
    end
    if from.height then
        to.baseHeight = from.height
    end
    if from.age_min then
        to.minAge = from.age_min
        to.maxAge = assert(from.age_max)
    end
    to.baseSkills = {}
    for id, level in pairs(from.skills) do
        id = id:gsub("%.stat_", "%.attr_")
        to.baseSkills[dotted(id)] = level
    end
    to.initialEquipSlots = {}
    for _, part in ipairs(from.body_parts) do
        to.initialEquipSlots[#to.initialEquipSlots + 1] = dotted(part)
    end
    if type(from.traits) == "table" and table.count(from.traits) > 0 then
        to.initialFeats = {}
        for part, level in pairs(from.traits) do
            to.initialFeats[dotted(part)] = level
        end
    end
    if from.properties.breaks_into_debris then
        comp(to, "StoneBlood")
    end
    if from.properties.melee_style then
        local c = comp(to, "UnarmedDamageText")
        c.damageTextType = dotted(from.properties.melee_style)
    end
    -- if #from.traits > 0 then
    --    to.traits = {}
    --    for id, level in pairs(from.traits) do
    --       to.traits[dotted(id)] = level
    --    end
    -- end
    -- if #from.resistances > 0 then
    --    to.resistances = {}
    --    for id, level in pairs(from.resistances) do
    --       to.resistances[dotted(id)] = level
    --    end
    -- end
end

handlers["elona_sys.dialog"] = function(from, to) end

handlers["base.tone"] = function(from, to) end

handlers["base.portrait"] = function(from, to)
    to.image = {
        filepath = from.image.source:gsub("graphic", "/Graphic/Elona"),
        region = ("%s, %s, %s, %s"):format(from.image.x, from.image.y, from.image.width, from.image.height),
    }
end

handlers["base.map_tile"] = function(from, to)
    if from.wall_kind == 1 and to.id:match "Bottom$" then
        return false
    end

    if type(from.image) == "string" then
        to.image = {
            filepath = from.image:gsub("graphic", "/Graphic/Elona"),
        }
    else
        to.image = {
            filepath = from.image.source:gsub("graphic", "/Graphic/Elona"),
            region = ("%s, %s, %s, %s"):format(from.image.x, from.image.y, from.image.width, from.image.height),
        }
    end

    if from.wall then
        assert(to.id:match "Top$")
        to.id = to.id:gsub("Top$", "")

        assert(from.wall_kind == 2)
        local bottom = data["base.map_tile"]:ensure(from.wall)
        to.wallImage = {
            filepath = bottom.image.source:gsub("graphic", "/Graphic/Elona"),
            region = ("%s, %s, %s, %s"):format(bottom.image.x, bottom.image.y, bottom.image.width, bottom.image.height),
        }
    end

    to.isSolid = from.is_solid
    to.isOpaque = from.is_opaque

    if from.kind and from.kind ~= Enum.TileRole.None then
        to.kind = Enum.TileRole:to_string(from.kind)
    end
    if from.kind2 and from.kind2 ~= Enum.TileRole.None then
        to.kind2 = Enum.TileRole:to_string(from.kind2)
    end
end

handlers["base.chip"] = function(from, to)
    if from.group then
        to.group = capitalize(from.group)
    end
    if from.group ~= "item" or from.image.source == nil then
        return false
    end
    to.image = {
        filepath = from.image.source:gsub("graphic", "/Graphic/Elona"),
        region = ("\"%s, %s, %s, %s\""):format(from.image.x, from.image.y, from.image.width, from.image.height),
    }
    if (from.y_offset or 0) ~= 0 then
        to.offset = ("0,%d"):format(from.y_offset)
    end
    if (from.shadow or 0) ~= 0 then
        to.shadowRot = from.shadow
    end
    if (from.stack_height or 0) ~= 0 then
        to.stackYOffset = from.stack_height
    end
end

handlers["elona.field_type"] = function(from, to)
    to.defaultTile = dotted(from.default_tile)
    to.fogTile = dotted(from.fog)
    to.tiles = {}
    for _, tile in ipairs(from.tiles) do
        to.tiles[#to.tiles + 1] = { id = dotted(tile.id), density = tile.density }
    end
end

handlers["base.pcc_part"] = function(from, to)
    to.imagePath = from.image:gsub("graphic", "/Graphic/Elona")
    to.pccPartType = capitalize(from.kind)
end

handlers["base.trait"] = function(from, to)
    to.levelMin = from.level_min
    to.levelMax = from.level_max
    to.featType = camelize(from.type)
end

handlers["base.element"] = function(from, to)
    if from.color then
        to.color = rgbToHex(from.color)
    end
    if from.ui_color then
        to.uiColor = rgbToHex(from.ui_color)
    end
    if from.can_resist then
        to.canResist = from.can_resist or false
    end
    if from.preserves_sleep then
        to.preservesSleep = from.preserves_sleep
    end
    if from.sound then
        setmetatable({
            soundID = dotted(from.sound),
        }, { tag = "type:SoundPathSpecifier", type = "mapping" })
    end
    if from.death_anim then
        to.deathAnim = dotted(from.death_anim)
    end
    if from.death_anim_dy then
        to.deathAnimDY = from.death_anim_dy
    end
    if from.rarity then
        to.rarity = from.rarity
    end
end

handlers["elona_sys.map_tileset"] = function(from, to)
    if from.door then
        to.door = {}
        if from.door.open_tile then
            to.door.openChip = dotted(from.door.open_tile)
        end
        if from.door.closed_tile then
            to.door.closedChip = dotted(from.door.closed_tile)
        end
    end

    if from.tiles then
        to.tiles = {}
        for k, v in pairs(from.tiles) do
            local t = {}
            if type(v) == "function" then
                t.tiles = "TODO"
                to.tiles[dotted(k)] = setmetatable(t, { tag = "type:TileRandom", type = "mapping" })
            else
                t.tile = dotted(v)
                to.tiles[dotted(k)] = setmetatable(t, { tag = "type:TileSingle", type = "mapping" })
            end
        end
    end

    if from.fog then
        if type(from.fog) == "function" then
            to.fogTile = "TODO"
        else
            to.fogTile = dotted(from.fog)
        end
    end
end

handlers["elona.material_spot"] = function(from, to)
    to.materials = {}
    for i, m in ipairs(from.materials) do
        to.materials[i] = dotted(m)
    end
end

handlers["elona.material"] = function(from, to)
    to.level = from.level
    to.rarity = from.rarity
    to.chip = dotted(from.image)
end

handlers["elona.god"] = function(from, to)
    if from.is_primary_god then
        to.isPrimaryGod = true
    end
    to.servant = dotted(from.servant)
    to.items = {}
    for _, fromItem in ipairs(from.items) do
        local toItem = {
            itemId = dotted(fromItem.id),
            onlyOnce = fromItem.only_once,
            noStack = fromItem.no_stack,
        }
        if fromItem.properties then
            toItem.filter = {
                type = "TODO",
            }
        end
        to.items[#to.items + 1] = toItem
    end
    to.artifact = dotted(from.artifact)
    if from.summon then
        to.summon = dotted(from.summon)
    end
    if from.blessings then
        to.blessings = setmetatable(
            {},
            { tag = ("type:GodBlessing%sEffect"):format(dataPart(from._id)), type = "mapping" }
        )
    end
    to.offerings = {}
    for _, fromOffering in ipairs(from.offerings) do
        local toOffering = {}
        if fromOffering.type == "category" then
            toOffering.category = itemCategory(fromOffering.id, "Item")
        end
        if fromOffering.type == "item" then
            toOffering.itemId = dotted(fromOffering.id)
        end
        to.offerings[#to.offerings + 1] = toOffering
    end

    if from.on_join_faith or from.on_leave_faith then
        to.callbacks = setmetatable({}, { tag = ("type:God%sCallbacks"):format(dataPart(from._id)), type = "mapping" })
    end
end

handlers["elona_sys.magic"] = function(from, to) end

handlers["base.effect"] = function(from, to)
    if from.color then
        to.color = rgbToHex(from.color)
    end
    if from.stops_activity then
        to.stopsActivity = from.stops_activity
    end
    if from.auto_heal then
        to.autoHeal = from.auto_heal
    end
    if from.related_element then
        to.relatedElement = dotted(from.related_element)
    end
    if from.emotion_icon then
        to.emotionIconId = dotted(from.emotion_icon)
    end
    if from.on_sleep == "remove" then
        to.removeOnSleep = true
    end
    to.components = {
        {
            type = ("Status%s"):format(dataPart(from._id)),
        },
    }
end

handlers["elona.item_material"] = function(from, to)
    if from.weight then
        to.weight = from.weight
    end
    if from.value then
        to.value = from.value
    end
    if from.hit_bonus then
        to.hitBonus = from.hit_bonus
    end
    if from.damage_bonus then
        to.damageBonus = from.damage_bonus
    end
    if from.dv then
        to.dv = from.dv
    end
    if from.pv then
        to.pv = from.pv
    end
    if from.dice_y then
        to.diceY = from.dice_y
    end
    if from.color then
        to.color = rgbToHex(from.color)
    end
    if from.no_furniture then
        to.noFurniture = from.no_furniture
    end
end

handlers["elona.food_type"] = function(from, to)
    if from.uses_chara_name then
        to.usesCharaName = from.uses_chara_name
    end
    to.expGains = {}
    for _, gain in ipairs(from.exp_gains) do
        local t = {
            skillID = dotted(gain._id),
            amount = gain.amount,
        }
        table.insert(to.expGains, t)
    end
    to.baseNutrition = from.base_nutrition
    to.itemChips = {}
    for k, v in pairs(from.item_chips) do
        to.itemChips[k] = dotted(v)
    end
    if from.quest_reward_category then
        to.questRewardCategory = dotted(from.quest_reward_category)
    end
end

handlers["elona.rank"] = function(from, to)
    field(from, to, "decay_period_days")
    field(from, to, "provides_salary_items")
    event(from, to, "calc_income", "Ranks", "VanillaRanksSystem", "CalcIncome")
end

handlers["base.activity"] = function(from, to)
    if from.default_turns and type(from.default_turns) == "number" then
        field(from, to, "default_turns")
    end
    if from.animation_wait and type(from.animation_wait) == "number" then
        field(from, to, "animation_wait")
    end
    if from.auto_turn_anim and type(from.auto_turn_anim) == "string" then
        field(from, to, "auto_turn_anim", dotted)
    end
    field(from, to, "can_scroll")
    field(from, to, "on_interrupt", classify)
    field(from, to, "interrupt_on_displace")
    comp(to, ("Activity%s"):format(dataPart(from._id)))
end

local function shop_modifier(from)
    if from.choices then
        return nil
    else
        local t = {}
        if from.id == "Skip" then
            return nil
        else
            field(from, t, "categories", function(i)
                return { itemCategory(dotted(i), "ItemCat") }
            end, "tags")
            field(from, t, "id", function(i)
                return itemCategory(dotted(i), "Item")
            end)
            field(from, t, "quality", enum(Enum.Quality))
            return t
        end
    end
end

local function shop_action(from)
    if from.choices then
        local choices = fun.iter(from.choices):map(shop_modifier):to_list()
        return setmetatable({
            choices = choices,
        }, { tag = "type:ChoicesShopInventoryAction", type = "mapping" })
    else
        local t = {}
        if from.id == "Skip" then
            return setmetatable(t, { tag = "type:AbortShopInventoryAction", type = "mapping" })
        else
            return nil
        end
    end
end

handlers["elona.shop_inventory"] = function(from, to)
    to.rules = {}
    if type(from.rules) == "function" then
        to.rules[1] = "TODO"
    else
        for _, rule in ipairs(from.rules) do
            local r = {}
            field(rule, r, "one_in")
            field(rule, r, "all_but_one_in")
            if rule.index then
                r.itemIndex = rule.index
            end
            if rule.predicate then
                r.predicate = "TODO"
            end
            r.modifier = shop_modifier(rule)
            r.action = shop_action(rule)
            to.rules[#to.rules + 1] = r
        end
    end
    event(from, to, "item_number", "Shopkeeper", "VanillaShopInventoriesSystem", "CalcItemAmount")
    event(from, to, "item_base_value", "Shopkeeper", "VanillaShopInventoriesSystem", "CalcItemBaseValue")
    event(from, to, "on_generate_item", "Shopkeeper", "VanillaShopInventoriesSystem", "BeforeGenerateItem")
end

handlers["elona.random_event"] = function(from, to)
    field(from, to, "image", dotted)
    field(from, to, "luck_threshold")
    if from.choice_count then
        field(from, to, "choice_count")
        event(from, to, "choices", "RandomEvent", "VanillaRandomEventsSystem", "OnChoiceSelected")
    end
    event(from, to, "on_event_triggered", "RandomEvent", "VanillaRandomEventsSystem", "OnTriggered")
end

handlers["elona.ex_help"] = function(from, to) end

handlers["base.skill"] = function(from, to)
    local difficulty = from.difficulty
    local magic = data["base.magic"][from._id]
    if magic then
        difficulty = magic.difficulty or difficulty
    end

    if difficulty ~= 0 then
        local e = extData(to, "ExtSkillDifficulty")
        field(from, e, "difficulty")
    end
end

handlers["elona.guild"] = function(from, to) end

handlers["elona.fish"] = function(from, to)
    field(from, to, "level")
    field(from, to, "rarity")
    field(from, to, "power")
    field(from, to, "speed")
    field(from, to, "weight")
    field(from, to, "value")
    field(from, to, "item_id", function(id)
        return itemCategory(id, "Item")
    end, "itemID")
end

handlers["elona.bait"] = function(from, to)
    field(from, to, "image", dotted, "chipID")
    field(from, to, "rank")
    field(from, to, "value")
end

handlers["elona.home"] = function(from, to)
    to.mapBlueprint = "TODO"
    field(from, to, "image", nil, "areaEntranceChip")
    field(from, to, "value")
    field(from, to, "home_scale")
    event(from, to, "on_generate", "Home", "VanillaHomesSystem", "OnGenerated")
end

handlers["elona.book"] = function(from, to)
    field(from, to, "no_generate", function(i)
        return not i
    end, "generateRandomly")
end

handlers["base.equipment_type"] = function(from, to)
    event(from, to, "on_initialize_equipment", "Equipment", "VanillaEquipmentTypesSystem", "OnInitializeEquipment")
    event(from, to, "on_drop_loot", "Equipment", "VanillaEquipmentTypesSystem", "OnGenerateLoot")
end

handlers["base.loot_type"] = function(from, to)
    event(from, to, "on_drop_loot", "Loot", "VanillaLootTypesSystem", "OnGenerateLoot")
end

local function sort(a, b)
    return (a.elona_id or 0) < (b.elona_id or 0)
end

local sorts = {}

sorts["base.map_tile"] = function(a, b)
    if a.elona_atlas and b.elona_atlas and a.elona_atlas ~= b.elona_atlas then
        return (a.elona_atlas or 0) < (b.elona_atlas or 0)
    end
    return sort(a, b)
end

local hspParents = {
    ["base.chara"] = "BaseChara",
    ["base.item"] = "BaseItem",
    ["base.feat"] = "BaseFeat",
    ["base.mef"] = "BaseMef",
}

local hspTypes = {
    ["base.chip"] = "Chip",
    ["base.map_tile"] = "Tile",
    ["base.pcc_part"] = "Elona.PCCPart",
    ["elona.field_type"] = "Elona.FieldType",
    ["base.trait"] = "Elona.Feat",
    ["base.effect"] = "Elona.StatusEffect",
    ["elona.material"] = "Elona.BlendMaterial",
    ["elona.item_material"] = "Elona.Material",
}

local function transformMinimal(i)
    local t = automagic()
    if hspParents[i._type] then
        t.type = "Entity"
        t.id = dottedEntity(i._id, i._type)
        t.parent = hspParents[i._type]
    else
        t.type = hspTypes[i._type] or ("Elona." .. classify(i._type:gsub("(.*)%.(.*)", "%2")))
        t.id = dotted(i._id)
    end

    if i.elona_id then
        if i._type == "base.map_tile" then
            t.hspIds = {
                elona122 = ("%s,%s"):format(i.elona_atlas, i.elona_id),
            }
        else
            t.hspIds = {
                elona122 = i.elona_id,
            }
        end
    end

    if assert(handlers[i._type], i._type)(i, t) == false then
        return nil
    end

    return t
end

local concat = function(acc, s)
    return (acc and (acc .. "\n") or "") .. s
end
local concat2 = function(acc, s)
    return (acc and (acc .. "\n\n") or "") .. s
end

local function write_protos_file(ty, datas, protoClass)
    local tyName = dataPart(ty)
    local maxPad = fun.iter(datas):extract("id"):map(dataPart):map(fun.op.len):max()
    local ids = fun.iter(datas)
        :map(function(dat)
            local modName = modPart(dat.id)
            local idName = dataPart(dat.id)
            local padding = string.rep(" ", maxPad - #idName)
            return ("            public static readonly %sPrototypeId %s%s = new($\"%s.{nameof(%s)}\");"):format(
                tyName,
                idName,
                padding,
                modName,
                idName
            )
        end)
        :foldl(concat)
    local code = ([[
using %sPrototypeId = OpenNefia.Core.Prototypes.PrototypeId<%s>;

namespace OpenNefia.Content.Prototypes
{
    public static partial class Protos
    {
        public static class %s
        {
            #pragma warning disable format

%s

            #pragma warning restore format
        }
    }
}
]]):format(tyName, protoClass, tyName, ids)

    local file = io.open(("%s/OpenNefia.Content/Prototypes/Protos/Protos.%s.cs"):format(rootDir, tyName), "w")
    file:write(code)
    file:close()
end

local function write_entity_system(ty, eventful, system_type)
    -- assuming it will go in content
    local full_system_type = "OpenNefia.Content." .. system_type
    local namespace, short_system_type = full_system_type:match "^(.+)%.([^.]+)$"
    local prototype_type = ("%sPrototype"):format(dataPart(ty))

    local filename = ("%s/OpenNefia.Content/%s.cs"):format(rootDir, system_type:gsub("%.", "/"))
    if fs.exists(filename) then
        return
    end

    -- Namespace.P_SomeEvent -> P_.SomeEvent
    local function split_id(id)
        return id:gsub("^[^.]+%.(.+)$", "%1")
    end

    local function print_handler(event)
        return ([[
        public void %s(%s proto, ref %s ev)
        {
        }
]]):format(event.method, prototype_type, split_id(event.type))
    end

    local function print_event_handlers(dat)
        local events = dat.events
        local fns = fun.iter(dat.events):map(print_handler):foldl(concat)

        return ([[
        #region %s

%s
        #endregion]]):format(dat.id, fns)
    end

    local function print_event_class(event_name)
        return ([[
    [ByRefEvent]
    [PrototypeEvent(typeof(%s))]
    public sealed class %s : PrototypeEventArgs
    {
        public %s()
        {
        }
    }]]):format(prototype_type, event_name, event_name)
    end

    local event_handlers = fun.iter(eventful):map(print_event_handlers):foldl(concat2)

    local event_names = fun.iter(eventful)
        :extract("events")
        :flatmap(function(d)
            return fun.iter(d):extract("type"):map(split_id):to_list()
        end)
        :to_set()
    event_names = table.keys(event_names)
    local events = fun.iter(event_names):map(print_event_class):foldl(concat2)

    local code = ([[
using OpenNefia.Content.StatusEffects;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Game;
using OpenNefia.Content.Effects;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Logic;

namespace %s
{
    public sealed class %s : EntitySystem
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IEntityLookup _entityLookup = default!;
        [Dependency] private readonly IEntityGen _entityGen = default!;
        [Dependency] private readonly IRandomGenSystem _randomGen = default!;
        [Dependency] private readonly ICharaGen _charaGen = default!;
        [Dependency] private readonly IItemGen _itemGen = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IAudioManager _audio = default!;

%s
    }

%s
}]]):format(namespace, short_system_type, event_handlers, events)

    local file = io.open(filename, "w")
    file:write(code)
    file:close()
end

local function make_datas(ty)
    local sort_ = sorts[ty] or sort
    local datas = data[ty]
        :iter()
        :into_sorted(sort_)
        :map(transformMinimal)
        :filter(function(i)
            return i ~= nil
        end)
        :to_list()

    return datas
end

local function make_yaml(datas, tags)
    return lyaml.dump({ datas }, { tag_directives = tags })
end

local function write(ty, filename, namespace)
    tags = {}
    local datas = make_datas(ty)
    local yml = make_yaml(datas)

    if type(filename) == "function" then
        filename(datas, yml)
        return
    else
        local file = io.open(("%s/OpenNefia.Content/Resources/Prototypes/Elona/%s"):format(rootDir, filename), "w")
        file:write(yml)
        file:close()
    end

    if namespace then
        write_protos_file(ty, datas, namespace)
    end

    local eventful = fun.iter(datas)
        :filter(function(d)
            return rawget(d, "events")
        end)
        :to_list()
    local systems = fun.iter(eventful)
        :extract("events")
        :flatmap(function(evs)
            return fun.iter(evs):extract("system"):to_list()
        end)
        :to_set()
    systems = table.keys(systems)
    if #systems == 1 then
        write_entity_system(ty, eventful, systems[1])
    elseif #systems > 1 then
        error "Multiple entity systems specified in events."
    end
end

write("base.chara", "Entity/Chara.yml")
write("base.item", "Entity/Item.yml")
write("base.class", "Class.yml", "OpenNefia.Content.Prototypes.ClassPrototype")
write("base.race", "Race.yml", "OpenNefia.Content.Prototypes.RacePrototype")
-- write("elona_sys.dialog", "Dialog.yml")
-- write("base.tone", "Tone.yml")
-- write("base.portrait", "Portrait.yml")
-- write("base.map_tile", "Tile.yml")
-- write("base.chip", "Chip.yml")
-- write("elona.field_type", "FieldType.yml")
-- write("base.pcc_part", "PCCPart.yml")
-- write("base.trait", "Feat.yml")
-- write("base.element", "Element.yml")
-- write("elona_sys.map_tileset", "MapTileset.yml")
-- write("elona.material_spot", "MaterialSpot.yml")
-- write("elona.material", "BlendMaterial.yml")
-- write("elona.god", "God.yml")
-- write("elona_sys.magic", "Magic.yml")
-- write("base.effect", "StatusEffect.yml")
-- write("elona.item_material", "Material.yml")
write("elona.food_type", "FoodType.yml", "OpenNefia.Content.Food.FoodTypePrototype")
-- write("elona.rank", "Rank.yml")
-- write("base.activity", "Activity.yml", "OpenNefia.Content.Activity.ActivityPrototype")
-- write("elona.shop_inventory", "ShopInventory.yml", "OpenNefia.Content.Shopkeeper.ShopInventoryPrototype")
-- write("elona.ex_help", "ExHelp.yml", "OpenNefia.Content.ExHelp.ExHelpPrototype")
-- write("elona.random_event", "RandomEvent.yml", "OpenNefia.Content.RandomEvent.RandomEventPrototype")
-- write("elona.guild", "Guild.yml", "OpenNefia.Content.Guild.GuildPrototype")
-- write("elona.fish", "Fish.yml", "OpenNefia.Content.Fishing.FishPrototype")
write("elona.bait", "Bait.yml", "OpenNefia.Content.Fishing.BaitPrototype")
-- write("elona.home", "Home.yml", "OpenNefia.Content.Home.HomePrototype")
-- write("elona.book", "Book.yml", "OpenNefia.Content.Book.BookPrototype")
-- write("base.equipment_type", "EquipmentType.yml", "OpenNefia.Content.Equipment.EquipmentTypePrototype")
-- write("base.loot_type", "LootType.yml", "OpenNefia.Content.Loot.LootTypePrototype")

-- for _, tag in ipairs(allTags) do
--     print(tag)
-- end

-- print(inspect(data["base.item"]:iter():filter(function(a) return a.fltselect > 0 and a.rarity == 0 end):to_list()))

-- Local Variables:
-- open-nefia-always-send-to-repl: t
-- End:
