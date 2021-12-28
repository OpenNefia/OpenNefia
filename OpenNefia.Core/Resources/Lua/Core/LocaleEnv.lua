_FinalizedKeys = {}

local auto, assign

function auto(tab, key)
   return setmetatable({}, {
         __index = auto,
         __newindex = assign,
         parent = tab,
         key = key
   })
end

local meta = {__index = auto}

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
   for k, v in pairs(t) do
      if type(k) ~= "string" or k:sub(1, 1) ~= "_" then
         trail[#trail+1] = k
         if type(v) == "table" then
            finalize(v, trail)
         else
            local key = table.concat(trail, ".")
            _FinalizedKeys[key] = v
         end
         trail[#trail] = nil
      end
   end
end

_Root = {}

_Finalize = function() finalize(_Root, {}); end

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

setmetatable(_G, _G_mt)
