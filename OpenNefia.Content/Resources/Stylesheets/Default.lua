local Maths = CLRPackage("OpenNefia.Core", "OpenNefia.Core.Maths")
local Rendering = CLRPackage("OpenNefia.Core", "OpenNefia.Core.Rendering")
local Drawing = CLRPackage("OpenNefia.Core", "OpenNefia.Core.UI.Wisp.Drawing")
local Styling = CLRPackage("OpenNefia.Core", "OpenNefia.Core.UI.Wisp.Styling")
local Color = Maths.Color
local Thickness = Maths.Thickness
local FontSpec = Rendering.FontSpec
local StyleBoxFlat = Drawing.StyleBoxFlat
local Utilities = Styling.StylesheetUtilities

local function capitalize(str)
    return (str:gsub("^%l", string.upper))
end

local function setProps(result, t)
    if type(t) == "table" then
        for k, v in pairs(t) do
            k = capitalize(k)
            if type(v) == "string" then
                if v:match("^#") then
                    v = Color.FromXaml(v)
                end
            end

            result[k] = v
        end
    end
end

local function styleBoxFlat(t)
    local result = StyleBoxFlat()
    setProps(result, t)
    return result
end

local function asset(id)
    return Utilities.GetAssetInstance(id)
end

local function margin(x, y, z, w)
    if y == nil then
        return Thickness(x)
    elseif z == nil then
        return Thickness(x, y)
    else
        return Thickness(x, y, z, w)
    end
end

local function font(t)
    local result = FontSpec(t.size, t.smallSize or t.size)
    t.size = nil
    t.smallSize = nil
    setProps(result, t)
    return result
end

----------------------------------------
-- Fallback
----------------------------------------

defaultFont = font({
    size = 14,
    smallSize = 14,
})

defaultStyleBox = styleBoxFlat({ backgroundColor = "#202020" })

_({
    font = defaultFont,
    fontColor = "#FFFFFF",
    panel = defaultStyleBox,
    styleBox = defaultStyleBox,
    texture = asset("Elona.AutoTurnIcon"),
    modulateSelf = "#FFFFFF",
})

----------------------------------------
-- Custom
----------------------------------------

_({
    rule(".windowPanel")({
        panel = styleBoxFlat({ backgroundColor = "#888899" }),
    }),

    rule(".windowTitleAlert")({
        panel = styleBoxFlat({ backgroundColor = "#003332" }),
    }),
})

PanelContainer(".windowHeader")({
    panel = styleBoxFlat({ backgroundColor = "#884444" }),
})

font12 = font({
    size = 14,
    smallSize = 14,
})

fontBold12 = font({
    size = 12,
    smallSize = 12,
    -- style = { "Bold" }
})

colorGold = "#A88B5E"

Label(".windowTitle")({
    fontColor = colorGold,
    font = fontBold12,
})

Label(".windowTitleAlert")({
    fontColor = "#FFFFFF",
    font = fontBold12,
})

ContainerButton({
    styleBox = styleBoxFlat({
        borderColor = "#005555",
        backgroundColor = "#007777",
        borderThickness = margin(10),
    }),
})

TextureButton(".windowCloseButton")({
    texture = asset("Elona.AutoTurnIcon"),
    modulateSelf = "#4B596A",
})

--[[
ItemList {
    background = "#334455",
    itemBackground = "#AABB88",
    disabledItemBackground = "#888888",
    selectedItemBackground = "#88BBAA",

    rule ".transparentItemList" {
        background = "#FFFFFF00",
        itemBackground = "#FFFFFF00",
        disabledItemBackground = "#FFFFFF00",
        selectedItemBackground = "#FFFFFF00",
    },
}
--]]

-- local inspect = require "Lua.Core.Thirdparty.inspect"
-- print(inspect(_DeclaredRules))
