_FinalizedKeys = {}
_PendingRefs = {}

-- TODO move!
function math.clamp(i, min, max)
    return math.min(max, math.max(min, i))
end

local auto, assign

function auto(tab, key)
    return setmetatable({}, {
        __index = auto,
        __newindex = assign,
        parent = tab,
        key = key,
    })
end

local meta = {
    __index = auto,
}

-- The if statement below prevents the table from being
-- created if the value assigned is nil. This is, I think,
-- technically correct but it might be desirable to use
-- assignment to nil to force a table into existence.

function assign(tab, key, val)
    -- if val ~= nil then
    local oldmt = getmetatable(tab)
    oldmt.parent[oldmt.key] = tab
    setmetatable(tab, meta)
    rawset(tab, key, val)
    -- end
end

local finalize
function finalize(t, trail)
    local refs = {}
    t = setmetatable(t, nil)
    for k, v in pairs(t) do
        if type(k) ~= "string" or k:sub(1, 1) ~= "_" then
            trail[#trail + 1] = k
            if type(v) == "table" then
                if v.__type == "ref" then
                    local key = table.concat(trail, ".")
                    local ref = { sourceKey = v.key, targetKey = key, parent = t, parentKey = k }
                    _PendingRefs[#_PendingRefs + 1] = ref
                    log("debug", "GETREF " .. ref.sourceKey .. " -> " .. ref.targetKey)
                elseif type(v[1]) == "string" or type(v[1]) == "function" then
                    local key = table.concat(trail, ".")
                    _FinalizedKeys[key] = setmetatable(v, nil)
                else
                    finalize(v, trail)
                end
            else
                local key = table.concat(trail, ".")
                _FinalizedKeys[key] = v
            end
            trail[#trail] = nil
        end
    end
end

-- Duplicates an existing key.
local function ref(key)
    return {
        __type = "ref",
        key = key,
    }
end

-- Duplicates an existing key in the prototype namespace (OpenNefia.Prototypes).
local function refp(key)
    return ref("OpenNefia.Prototypes." .. key)
end

local function resolveRefs()
    local resolvedAny
    repeat
        resolvedAny = false
        for _, ref in ipairs(_PendingRefs) do
            if not ref.resolved then
                local source = _FinalizedKeys[ref.sourceKey]
                if source then
                    _FinalizedKeys[ref.targetKey] = source
                    ref.resolved = true
                    resolvedAny = true
                end
            end
        end
    until not resolvedAny

    for _, ref in ipairs(_PendingRefs) do
        if not ref.resolved then
            log("warning", "missing reference to locale key " .. ref.sourceKey .. " -> " .. ref.targetKey)
            _FinalizedKeys[ref.targetKey] = "<missing reference: " .. ref.sourceKey .. " -> " .. ref.targetKey .. ">"
        end
    end

    -- Place the resolved refs back into _Collected in case something like TryGetLocalizationData
    -- wants the actual Lua table.
    for _, ref in ipairs(_PendingRefs) do
        log("verbose", "SETREF " .. ref.parentKey .. " " .. _FinalizedKeys[ref.targetKey])
        ref.parent[ref.parentKey] = _FinalizedKeys[ref.targetKey]
    end

    _PendingRefs = {}
end

_Root = {}
_Collected = {}

_Finalize = function()
    log("debug", "Finalizing locale keys")
    finalize(_Collected, {})
    resolveRefs()
end

-- https://stackoverflow.com/a/1283608
function table.merge(t1, t2)
    for k, v in pairs(t2) do
        if type(v) == "table" then
            if type(t1[k] or false) == "table" then
                table.merge(t1[k] or {}, t2[k] or {})
            else
                t1[k] = v
            end
        else
            t1[k] = v
        end
    end
    return t1
end

function _BeforeLoad()
    _Root = {}
end

function _AfterLoad()
    _Collected = table.merge(_Collected, _Root)
end

_ = {}

_.ref = ref
_.refp = refp

local _G_mt = {}

function _G_mt:__index(key)
    if _Root[key] then
        return _Root[key]
    end
    local val = auto(_Root, key)
    _Root[key] = val
    return val
end

function _G_mt:__newindex(key, val)
    assert(type(val) == "table", "value was not table")
    local blank = auto(_Root, key)
    local mt = getmetatable(blank)
    _Root[key] = setmetatable(val, mt)
end

setmetatable(_G, _G_mt)
