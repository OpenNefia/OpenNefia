Elona.Read = {
    CannotSee = function(reader)
        return ("无法看见%s。"):format(_.name(reader))
    end,

    Activity = {
        Start = function(reader, book)
            return ("%s开始阅读%s。"):format(_.sore_wa(reader), _.name(book, nil, 1))
        end,
        Finish = function(reader, book)
            return ("%s阅读完%s。"):format(_.sore_wa(reader), _.name(book, nil, 1))
        end,
        FallsApart = function(book)
            return ("%s变成了尘土崩塌。"):format(_.name(book, nil, 1))
        end,
    },

    Book = {
        ItemName = {
            Title = function(name, title)
                return ("名为《%s》的%s"):format(title, name)
            end,
        },
    },

    AncientBook = {
        ItemName = {
            Decoded = function(name)
                return ("已解读的%s"):format(name)
            end,
            Undecoded = function(name)
                return name
            end,
            Title = function(name, title)
                return ("名为《%s》的%s"):format(title, name)
            end,
            Titles = {
                ["0"] = "福音书",
                ["1"] = "人偶颂",
                ["2"] = "波纳佩教典",
                ["3"] = "格拉西启示录",
                ["4"] = "古韩片段",
                ["5"] = "断罪之书",
                ["6"] = "陀舍雅经",
                ["7"] = "埃文之书",
                ["8"] = "伟大教书",
                ["9"] = "塞拉伊诺片段",
                ["10"] = "死灵书",
                ["11"] = "鲁鲁伊耶异本",
                ["12"] = "艾尔特敦·夏尔兹",
                ["13"] = "金枝篇",
                ["14"] = "终焉之书",
            },
        },
        AlreadyDecoded = "已经解读过了。",
        FinishedDecoding = function(book)
            return ("已解读%s！"):format(_.name(book, nil, 1))
        end,
    },

    Textbook = {
        ItemName = {
            Title = function(name, skillName)
                return ("名为《%s》的%s"):format(skillName, name)
            end,
        },
        PromptNotInterested = "对这本书的内容没有兴趣。还要继续阅读吗？ ",
    },

    BookOfRachel = {
        ItemName = {
            Title = function(name, no)
                return ("第%s卷的%s"):format(no, name)
            end,
        },
        Text = "这是来自作家Rachel的温馨童话集。",
    },
    VoidPermit = "上面写着允许进入虚空的探索内容。",
}