LightenedTravels.FueledLight = {
    ItemName = {
        FuelLeft = function(item, fuel, maxFuel)
            return ("(Fuel: %s/%s)"):format(fuel, maxFuel)
        end,
    },
    HasNoFuel = function(wielder, item)
        return ("%s %s no fuel."):format(_.name(item), _.has(item))
    end,
    GettingDim = function(wielder, item)
        return ("%s is getting dim."):format(_.name(item))
    end,
    AboutToRunDry = function(wielder, item)
        return ("%s is about to run dry."):format(_.name(item))
    end,
    RunsOutOfFuel = function(wielder, item)
        return ("%s run%s out of fuel."):format(_.name(item), _.s(item))
    end,
}
