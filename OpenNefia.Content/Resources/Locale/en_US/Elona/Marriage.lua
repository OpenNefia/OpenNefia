Elona.Marriage = {
    Event = {
        Title = "Marriage",
        Text = function(source, target)
            return ("At last, %s and %s are united in marriage! After the wedding ceremony, %s receive%s some gifts."):format(
                _.name(source, true),
                _.name(target, true),
                _.name(source, true),
                _.s(source)
            )
        end,
        Choices = {
            ["0"] = "Without you, life has no meaning.",
        },
    },
}
