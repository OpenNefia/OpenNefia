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

defaultGrabberSize = 10

HScrollBar({
    grabber = styleBoxFlat({
        backgroundColor = "#80808080",
        contentMarginTopOverride = defaultGrabberSize,
    }),

    rule(":hover")({
        grabber = styleBoxFlat({
            backgroundColor = "#A0A0A080",
            contentMarginTopOverride = defaultGrabberSize,
        }),
    }),
    rule(":grabbed")({
        grabber = styleBoxFlat({
            backgroundColor = "#C0C0C080",
            contentMarginTopOverride = defaultGrabberSize,
        }),
    }),
})

VScrollBar({
    grabber = styleBoxFlat({
        backgroundColor = "#80808080",
        contentMarginLeftOverride = defaultGrabberSize,
        contentMarginTopOverride = defaultGrabberSize,
    }),

    rule(":hover")({
        grabber = styleBoxFlat({
            backgroundColor = "#A0A0A080",
            contentMarginLeftOverride = defaultGrabberSize,
            contentMarginTopOverride = defaultGrabberSize,
        }),
    }),
    rule(":grabbed")({
        grabber = styleBoxFlat({
            backgroundColor = "#C0C0C080",
            contentMarginLeftOverride = defaultGrabberSize,
            contentMarginTopOverride = defaultGrabberSize,
        }),
    }),
})

----------------------------------------
-- Custom
----------------------------------------

_({
    rule(".windowPanel")({
        panel = styleBoxFlat({
            backgroundColor = "#202040D0",
            borderColor = "#80808080",
            borderThickness = margin(1),
        }),
    }),
})

PanelContainer(".windowHeader")({
    panel = styleBoxFlat({
        backgroundColor = "#444488",
    }),
})

PanelContainer(".windowHeaderAlert")({
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

colorGold = "#F8ABAE"

Label(".windowTitle")({
    fontColor = "#FFFFFF",
    font = fontBold12,
})

Label(".windowTitleAlert")({
    fontColor = colorGold,
    font = fontBold12,
})

Button({
    styleBox = styleBoxFlat({
        borderColor = "#446666",
        backgroundColor = "#447777",
        borderThickness = margin(2),
    }),

    rule(":hover")({
        styleBox = styleBoxFlat({
            borderColor = "#446666",
            backgroundColor = "#44AAAA",
            borderThickness = margin(2),
        }),
    }),

    rule(":pressed")({
        styleBox = styleBoxFlat({
            borderColor = "#886666",
            backgroundColor = "#AAAA44",
            borderThickness = margin(2),
        }),
    }),

    rule(":disabled")({
        styleBox = styleBoxFlat({
            borderColor = "#444444",
            backgroundColor = "#666666",
            borderThickness = margin(2),
        }),
    }),
})

CheckBox({
    styleBox = styleBoxFlat({
        backgroundColor = "#00000000",
    }),
})

TextureButton(".windowCloseButton")({
    texture = asset("Elona.AutoTurnIcon"),
    modulateSelf = "#4B596A",
})

TextureRect(".checkBox")({
    texture = asset("Core.WispCheckboxUnchecked"),

    rule(".checkBoxChecked")({
        texture = asset("Core.WispCheckboxChecked"),
    }),
})

PanelContainer(".designerBackground")({
    styleBox = styleBoxFlat({
        backgroundColor = "#B0C4DE",
    }),
})

PanelContainer(".designerToolbar")({
    styleBox = styleBoxFlat({
        backgroundColor = "#223333",
    }),
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
