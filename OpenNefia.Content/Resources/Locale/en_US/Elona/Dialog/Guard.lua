Elona.Dialog.Guard = {
    Choices = {
        WhereIs = function(entity)
            return ("Where is %s?"):format(_.name(entity, true))
        end,
        LostProperty = function(item)
            return ("Here is a lost %s I found."):format(_.basename(item))
        end,
    },

    LostProperty = {
        TurnIn = {
            Dialog = "How nice of you to take the trouble to bring it. You're a model citizen indeed!",
            Choice = "It's nothing.",
        },
        Empty = {
            Dialog = "Hmm! It's empty!",
            Choice = "Oops...!",
        },
        FoundOften = {
            Dialog = {
                "Oh, it's you again? How come you find the wallets so often?",
                "(...suspicious)",
            },
            Choice = "I really found it on the street!",
        },
    },

    WhereIs = {
        Dead = "Oh forget it, dead for now.",
        VeryClose = function(speaker, target, direction)
            return ("Oh look carefully before asking, just turn %s."):format(direction)
        end,
        Close = function(speaker, target, direction)
            return ("I saw %s just a minute ago. Try %s."):format(_.name(target, true), direction)
        end,
        Moderate = function(speaker, target, direction)
            return ("Walk to %s for a while, you'll find %s."):format(direction, _.name(target, true))
        end,
        Far = function(speaker, target, direction)
            return ("If you want to meet %s, you have to considerably walk to %s."):format(
                _.name(target, true),
                direction
            )
        end,
        VeryFar = function(speaker, target, direction)
            return ("You need to walk long way to %s to meet %s."):format(direction, _.name(target, true))
        end,
    },
}
