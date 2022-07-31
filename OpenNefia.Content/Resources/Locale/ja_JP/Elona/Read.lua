Elona.Read = {
    CannotSee = function(reader)
        return ("%sは何も見えない。"):format(_.name(reader))
    end,

    Activity = {
        Start = function(reader, book)
            return ("%s%sを読み始めた。"):format(_.sore_wa(reader), _.name(book, nil, 1))
        end,
        Finish = function(reader, book)
            return ("%s%sを読み終えた。"):format(_.sore_wa(reader), _.name(book, nil, 1))
        end,
        FallsApart = function(book)
            return ("%sは塵となって崩れ落ちた。"):format(_.name(book, nil, 1))
        end,
    },

    AncientBook = {
        AlreadyDecoded = "それは既に解読済みだ。",
        FinishedDecoding = function(book)
            return ("%sを解読した！"):format(_.name(book, nil, 1))
        end,
    },

    PromptNotInterested = "この本の内容には興味がない。それでも読む？ ",

    BookOfRachel = "レイチェルという作家による、心あたたまる童話集だ。",
    VoidPermit = "すくつの探索を許可する、という内容の文面が形式的に書いてある。",
}
