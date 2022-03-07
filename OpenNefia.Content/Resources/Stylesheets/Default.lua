local Maths = CLRPackage("OpenNefia.Core", "OpenNefia.Core.Maths")
local Drawing = CLRPackage("OpenNefia.Core", "OpenNefia.Core.UI.Wisp.Drawing")
local Styling = CLRPackage("OpenNefia.Core", "OpenNefia.Core.UI.Wisp.Styling")
local Color = Maths.Color
local StyleBoxFlat = Drawing.StyleBoxFlat
local Utilities = Styling.StylesheetUtilities

local function styleBoxFlat(color)
    return StyleBoxFlat(Color.FromXaml(color))
end

local function asset(id)
    return Utilities.GetAssetInstance(id)
end

_({
    rule(".windowPanel")({
        panel = styleBoxFlat("#888899"),
    }),

    rule(".windowTitleAlert")({
        panel = styleBoxFlat("#003332"),
    }),
})

PanelContainer(".windowHeader")({
    panel = styleBoxFlat("#884444"),
})

notoSansDisplayBold14 = font({
    name = "Noto Sans Display",
    size = 14,
    style = { "Bold" },
})

colorGold = "#A88B5E"

Label(".windowTitle")({
    fontColor = colorGold,
    font = notoSansDisplayBold14,
})

Label(".windowTitleAlert")({
    fontColor = "#FFFFFF",
    font = notoSansDisplayBold14,
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

local inspect = require("Lua.Core.Thirdparty.inspect")
print(inspect(_DeclaredRules))
