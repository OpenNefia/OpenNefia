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

    Book = {
        ItemName = {
            Title = function(name, title)
                return ("《%s》という題名の%s"):format(title, name)
            end,
        },
    },

    AncientBook = {
        ItemName = {
            Decoded = function(name)
                return ("解読済みの%s"):format(name)
            end,
            Undecoded = function(name)
                return name
            end,
            Title = function(name, title)
                return ("《%s》という題名の%s"):format(title, name)
            end,
            Titles = {
                ["0"] = "ヴォイニッチ写本",
                ["1"] = "ドール賛歌",
                ["2"] = "ポナペ教教典",
                ["3"] = "グラーキ黙示録",
                ["4"] = "グ＝ハーン断章",
                ["5"] = "断罪の書",
                ["6"] = "ドジアンの書",
                ["7"] = "エイボンの書",
                ["8"] = "大いなる教書",
                ["9"] = "セラエノ断章",
                ["10"] = "ネクロノミコン",
                ["11"] = "ルルイエ異本",
                ["12"] = "エルトダウン・シャールズ",
                ["13"] = "金枝篇",
                ["14"] = "終焉の書",
            },
        },
        AlreadyDecoded = "それは既に解読済みだ。",
        FinishedDecoding = function(book)
            return ("%sを解読した！"):format(_.name(book, nil, 1))
        end,
    },

    Textbook = {
        ItemName = {
            Title = function(name, skillName)
                return ("《%s》という題名の%s"):format(skillName, name)
            end,
        },
        PromptNotInterested = "この本の内容には興味がない。それでも読む？ ",
    },

    BookOfRachel = {
        ItemName = {
            Title = function(name, no)
                return ("第%s巻目の%s"):format(no, name)
            end,
        },
        Text = "レイチェルという作家による、心あたたまる童話集だ。",
    },
    VoidPermit = "すくつの探索を許可する、という内容の文面が形式的に書いてある。",
}
