local Rule = {}

function rule(...)
    local classes = {}
    local elementId = nil
    local pseudos = {}

    local count = select("#", ...)
    if count == 0 then
        error "No rules provided."
    end

    local body = nil

    for i = 1, count do
        local rule = select(i, ...)

        if type(rule) ~= "string" then
            error("Invalid rule " .. tostring(rule))
        end

        if rule:match "^%." then
            classes[#classes + 1] = rule:sub(2)
        elseif rule:match "^#" then
            if elementId then
                error("elementId declared twice in rules list (" .. elementId .. ", " .. rule:sub(2) .. ")")
            end
            elementId = rule:sub(2)
        elseif rule:match "^:" then
            pseudos[#pseudos + 1] = rule:sub(2)
        else
            error("Invalid rule " .. rule)
        end

        if body then
            body = body .. " " .. rule
        else
            body = rule
        end
    end

    local t = {
        type = "Rule",
        body = body,
        classes = classes,
        elementId = elementId,
        pseudos = pseudos,
    }
    return setmetatable(t, Rule)
end

function Rule:__call(arg)
    assert(self.properties == nil, "Properties already declared for rule: " .. self.body)
    self.properties = arg
    return self
end

function Rule:__tostring()
    return self.body
end

function literal(value)
    return {
        type = "literal",
        value = value,
    }
end

function font(t)
    if type(t) ~= "table" then
        error("Invalid font table " .. tostring(t))
    end

    return {
        type = "font",
        name = t.name,
        size = t.size,
        style = t.style or {},
    }
end

local Element = {}

function Element.new(name)
    local t = {
        type = "Element",
        name = name,
        selector = false,
        properties = false,
    }
    return setmetatable(t, Element)
end

function Element:__index(k)
    return Element.new(self.name .. "." .. k)
end

function Element:__tostring()
    local s = self.name
    if self.selector then
        s = s .. "(" .. tostring(self.selector) .. ")"
    end
    return s
end

_DeclaredProperties = {}
_DeclaredRules = {}

function Element:__call(arg, ...)
    if type(arg) == "string" then
        if self.selector then
            error("Selector already declared for element: " .. self.name)
        end
        self.selector = rule(arg, ...)
        return self
    elseif type(arg) == "table" then
        if self.properties then
            error("Properties already declared for element: " .. self.name .. "\n" .. tostring(self.properties))
        end
        self.properties = arg
        _DeclaredRules[#_DeclaredRules + 1] = self
    else
        error("Invalid element argument " .. tostring(arg))
    end
end

local _G_mt = {}

function _G_mt:__index(k)
    if _DeclaredProperties[k] then
        return _DeclaredProperties[k]
    end
    return Element.new(k)
end

function _G_mt:__newindex(k, v)
    _DeclaredProperties[k] = v
end

setmetatable(_G, _G_mt)
