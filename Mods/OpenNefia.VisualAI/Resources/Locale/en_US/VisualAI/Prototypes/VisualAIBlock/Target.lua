local VisualAI = _.VisualAI

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
        Name = function(vars)
            local x = vars.targetFilter.position.X
            local y = vars.targetFilter.position.Y
            return ("Position at (%d, %d)"):format(x, y)
        end,
    },
    TargetPlayerTargetingPosition = {
        Name = "Target position of player",
    },
    TargetHpMpSpThreshold = {
        Name = function(vars)
            local kind = VisualAI.formatEnum(vars.targetFilter.type) -- HP
            local comparison = VisualAI.formatEnum(vars.targetFilter.comparison) -- ==
            local threshold = math.floor(vars.targetFilter.threshold * 100) -- 50
            return ("Target with %s %s %s%%"):format(kind, comparison, threshold)
        end,
    },

    --
    -- Target Order
    --

    TargetOrderingNearest = {
        Name = "Thing nearest to me",
    },
    TargetOrderingFurthest = {
        Name = "Thing furthest from me",
    },
    TargetOrderingHpMpSp = {
        Name = function(vars)
            local comparator = VisualAI.formatEnum(vars.targetOrdering.comparator) -- least
            local kind = VisualAI.formatEnum(vars.targetOrdering.type) -- HP
            return ("Target with the %s %s remaining"):format(comparator, kind)
        end,
    },
}
