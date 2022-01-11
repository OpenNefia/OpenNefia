require("LocaleEnv")

OpenNefia.Core.UI.Layer.TitleScreenLayer = {
    Window = {
        Title = "Starting Menu",
    },

    List = {
        Restore = {
            Text = "Restore an Adventurer",
        },
        Generate = {
            Text = "Generate an Adventurer",
        },
        Incarnate = {
            Text = "Incarnate an Adventurer",
        },
        About = {
            Text = "About",
        },
        Options = {
            Text = "Options",
        },
        Mods = {
            Text = "Mods",
        },
        Exit = {
            Text = "Exit",
        },
    },
}

OpenNefia.Core.Data.Types.CharaDef = {
    Core = {
        Putit = {
            Name = "putit",
        },
        Chicken = {
            Name = "chicken",
        },
    },
}

local inspect = require("Thirdparty.inspect")
print(inspect(_Root))
