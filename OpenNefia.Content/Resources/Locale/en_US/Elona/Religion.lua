Elona.Religion = {
    Menu = {
        Ability = function(_1)
            return (" Ability: %s"):format(_1)
        end,
        Bonus = function(_1)
            return ("   Bonus: %s"):format(_1)
        end,
        Offering = function(_1)
            return ("Offering: %s"):format(_1)
        end,
        Window = {
            Abandon = "Abandon God",
            Believe = function(godName)
                return ("Believe in %s"):format(godName)
            end,
            cancel = "Cancel",
            Convert = function(godName)
                return ("Convert to %s"):format(godName)
            end,
            Title = function(godName)
                return ("< %s >"):format(godName)
            end,
        },
    },
    Enraged = function(godName)
        return ("%s is enraged."):format(godName)
    end,
    Indifferent = " Your God becomes indifferent to your gift.",
    Pray = {
        DoNotBelieve = "You don't believe in God.",
        Indifferent = function(godName)
            return ("%s is indifferent to you."):format(godName)
        end,
        Prompt = "Really pray to your God?",
        Servant = {
            NoMore = "No more than 2 God's servants are allowed in your party.",
            PartyIsFull = "Your party is full. The gift is reserved.",
            PromptDecline = "Do you want to decline this gift?",
        },
        YouPrayTo = function(godName)
            return ("You pray to %s."):format(godName)
        end,
    },
    Switch = {
        Follower = function(godName)
            return ("You become a follower of %s!"):format(godName)
        end,
        Unbeliever = "You are an unbeliever now.",
    },
    Offer = {
        Claim = function(godName)
            return ("%s claims the empty altar."):format(godName)
        end,
        DoNotBelieve = "You don't believe in God.",
        Execute = function(item, godName)
            return ("You put %s on the altar and mutter the name of %s."):format(item, godName)
        end,
        Result = {
            Best = function(item)
                return ("%s shine%s all around and dissappear%s."):format(item, _.s(item), _.s(item))
            end,
            Good = function(item)
                return ("%s for a moment and disappear%s. A three-leaved falls from the altar."):format(item, _.s(item))
            end,
            Okay = function(item)
                return ("%s shine%s for a moment and disappear%s."):format(item, _.s(item), _.s(item))
            end,
            Poor = function(item)
                return ("%s disappear%s."):format(item, _.s(item))
            end,
        },
        TakeOver = {
            Attempt = function(godName, otherGodName)
                return ("Strange fogs surround all over the place. You see shadows of %s and %s make a fierce dance."):format(
                    godName,
                    otherGodName
                )
            end,
            Fail = function(godName)
                return ("%s keeps the altar."):format(godName)
            end,
            Shadow = "The shadow of your god slowly gets bolder.",
            Succeed = function(godName)
                return ("%s takes over the altar."):format(godName)
            end,
        },
    },
}
