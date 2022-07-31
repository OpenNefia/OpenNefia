Elona.Read = {
    CannotSee = function(reader)
        return ("%s can see nothing."):format(_.name(reader))
    end,

    Activity = {
        Start = function(reader, book)
            return ("%s start%s to read %s."):format(_.name(reader), _.s(reader), _.name(book, nil, 1))
        end,
        Finish = function(reader, book)
            return ("%s %s finished reading %s."):format(_.name(reader), _.have(reader), _.name(book, nil, 1))
        end,
        FallsApart = function(book)
            return ("%s falls apart."):format(_.name(book, nil, 1))
        end,
    },

    AncientBook = {
        AlreadyDecoded = "You already have decoded the book.",
        FinishedDecoding = function(book)
            return ("You finished decoding %s!"):format(_.name(book, nil, 1))
        end,
    },

    PromptNotInterested = "You are not interested in this book. Do you want to read it anyway? ",

    BookOfRachel = "It's a lovely fairy tale written by Rachel.",
    VoidPermit = "According to the card, you are permitted to explore the void now.",
}
