_FinalizedKeys = {}

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
    t = setmetatable(t, nil)
    for k, v in pairs(t) do
        if type(k) ~= "string" or k:sub(1, 1) ~= "_" then
            trail[#trail + 1] = k
            if type(v) == "table" then
                if type(v[1]) == "string" then
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

_Root = {}
_Collected = {}

_Finalize = function()
    finalize(_Collected, {})
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
