Elona.Mount = {
    Start = {
        Problems = {
            CanOnlyRideAlly = "You can only ride an ally.",
            CannotRideClient = "You can't ride a client.",
            RideSelf = function(rider)
                -- TODO better conjugation function
                return ("%s try%s to ride %s."):format(_.name(rider), _.s(rider), _.theTarget(rider, rider))
            end,
            IsStayer = "その仲間はこの場所に滞在中だ。",
            IsCurrentlyRiding = function(rider, mount)
                return ("%s %s currently riding %s."):format(_.name(rider), _.is(rider), _.name(mount))
            end,
        },

        YouRide = function(rider, mount, mountPrevSpeed, mountNewSpeed)
            return ("%s ride%s %s. (%s's speed: %s->%s)"):format(
                _.name(rider),
                _.s(rider),
                _.name(mount),
                _.name(mount),
                mountPrevSpeed,
                mountNewSpeed
            )
        end,

        Suitability = {
            Good = function(rider, mount)
                return ("%s feel%s comfortable."):format(_.name(rider), _.s(rider))
            end,
            Bad = function(rider, mount)
                return ("This creature is too weak to carry %s."):format(_.name(rider))
            end,
        },

        Dialog = {
            _.quote "Awww.",
            _.quote "You should go on a diet.",
            _.quote "Let's roll!",
            _.quote "Be gentle.",
        },
    },

    Stop = {
        NoPlaceToGetOff = "There's no place to get off.",
        YouDismount = function(rider, mount)
            return ("%s dismount%s from %s."):format(_.name(rider), _.s(rider), _.name(mount))
        end,
        DismountCorpse = function(rider, mount)
            return ("%s get%s off the corpse of %s."):format(_.name(rider), _.s(rider), _.name(mount, true))
        end,
    },

    Movement = {
        InterruptActivity = function(rider, mount)
            return ("%s stares in %s face."):format(_.name(mount), _.possessive(rider))
        end,
    },
}
