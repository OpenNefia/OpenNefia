Elona.Read = {
    CannotSee = function(reader)
        return ("%s can see nothing."):format(_.name(reader))
    end,

    Activity = {
        Start = function(reader, book)
            return ("%s start%s to read %s."):format(_.name(reader), _.s(reader), _.name(book, nil, 1))
        end,
        Finish = function(reader, book)
            return ("%s %s finished reading %s."):format(_.name(reader), _.has(reader), _.name(book, nil, 1))
        end,
        FallsApart = function(book)
            return ("%s falls apart."):format(_.name(book, nil, 1))
        end,
    },

    Book = {
        ItemName = {
            Title = function(name, title)
                return ("%s titled <%s>"):format(name, title)
            end,
        },
    },

    AncientBook = {
        ItemName = {
            Decoded = "",
            Undecoded = function(name)
                return ("undecoded %s"):format(name)
            end,
            Title = function(name, title)
                return ("%s titled <%s>"):format(name, title)
            end,
            Titles = {
                ["0"] = "Voynich Manuscript",
                ["1"] = "Dhol Chants",
                ["2"] = "Ponape Scripture",
                ["3"] = "Revelations of Glaaki",
                ["4"] = "G'harne Fragments",
                ["5"] = "Liber Damnatus",
                ["6"] = "Book of Dzyan",
                ["7"] = "Book of Eibon",
                ["8"] = "Grand Grimoire",
                ["9"] = "Celaeno Fragments",
                ["10"] = "Necronomicon",
                ["11"] = "The R'lyeh Text",
                ["12"] = "Eltdown Shards",
                ["13"] = "The Golden Bough",
                ["14"] = "Apocalypse",
            },
        },
        AlreadyDecoded = "You already have decoded the book.",
        FinishedDecoding = function(book)
            return ("You finished decoding %s!"):format(_.name(book, nil, 1))
        end,
    },

    Textbook = {
        ItemName = {
            Title = function(name, skillName)
                return ("%s titled <Art of %s>"):format(name, skillName)
            end,
        },
        PromptNotInterested = "You are not interested in this book. Do you want to read it anyway? ",
    },

    BookOfRachel = {
        ItemName = {
            Title = function(name, no)
                return ("%s of Rachel No.%s"):format(name, no)
            end,
        },
        Text = "It's a lovely fairy tale written by Rachel.",
    },
    VoidPermit = "According to the card, you are permitted to explore the void now.",
}
