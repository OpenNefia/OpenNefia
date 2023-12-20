OpenNefia.Prototypes.Elona.WishHandler.Elona = {
    Alias = {
        Keyword = { "aka", "title", "name", "alias" },

        Prompt = function(wisher)
            return ("What's %s new alias?"):format(_.possessive(wisher))
        end,
        Impossible = "*laugh*",
        NewAlias = function(wisher, newAlias)
            return ("%s will be known as <%s>."):format(_.name(wisher), newAlias)
        end,
        NoChange = "What a waste of a wish!",
    },
    Ally = {
        Keyword = { "friend", "company", "ally" },
    },
    Death = {
        Keyword = "death",
        Result = "If you wish so...",
    },
    Fame = {
        Keyword = "fame",
    },
    GodInside = {
        Keyword = "god inside",
        Result = "There's no God inside.",
    },
    Gold = {
        Keyword = { "money", "gold", "wealth", "fortune" },
        Result = "Lots of gold pieces appear.",
    },
    ManInside = {
        Keyword = "man inside",
        Result = "There's no man inside.",
    },
    Platinum = {
        Keyword = { "platina", "platinum" },
        Result = "Platinum pieces appear.",
    },
    Redemption = {
        Keyword = { "redemption", "atonement" },
        NotASinner = function(wisher)
            return ("%s %s a sinner."):format(_.name(wisher), _.is_not(wisher))
        end,
        Result = "What a convenient wish!",
    },
    Sex = {
        Keyword = { "sex", "gender" },
        Result = function(wisher, newGender)
            return ("%s become%s %s!"):format(_.name(wisher), _.s(wisher), newGender)
        end,
    },
    SmallMedal = {
        Keyword = { "coin", "medal", "small coin", "small medal" },
        Result = "A small coin appears.",
    },
    Youth = {
        Keyword = { "youth", "age", "beauty" },
        Result = "A typical wish.",
    },
}
