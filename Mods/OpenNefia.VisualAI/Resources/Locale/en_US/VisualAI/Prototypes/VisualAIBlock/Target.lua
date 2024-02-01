OpenNefia.Prototypes.VisualAI.Block.VisualAI = {
    TargetPlayer = {
        Name = "Player",
    },
    TargetSelf = {
        Name = "Self",
    },
    TargetAllies = {
        Name = "Allies",
    },
    TargetEnemies = {
        Name = "Enemies",
    },
    TargetCharacters = {
        Name = "Characters",
    },
    TargetGroundItems = {
        Name = "Items on ground",
    },
    TargetInventory = {
        Name = "Inventory",
    },
    TargetStored = {
        Name = "Previously preserved target",
    },
    TargetPlayerTargetingCharacter = {
        Name = "Target of player",
    },
    TargetSpecificLocation = {
        Name = function(x, y)
            return ("Position at (%d, %d)"):format(x, y)
        end,
    },
    TargetPlayerTargetingPosition = {
        Name = "Target position of player",
    },
    TargetHpMpSpThreshold = {
        Name = function(kind, comparator, threshold)
            return ("Target with %s %s %s%%"):format(kind, comparator, threshold)
        end,
    },

    --
    -- Target Order
    --

    TargetOrderNearest = {
        Name = "Thing nearest to me",
    },
    TargetOrderFurthest = {
        Name = "Thing furthest from me",
    },
    TargetOrderHpMpSp = {
        Name = function(comparator, kind)
            return ("Target with the %s %s remaining"):format(comparator, kind)
        end,
        comparator = {
            [">"] = "most",
            ["<"] = "least",
        },
    },
}
