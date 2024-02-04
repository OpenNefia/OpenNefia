OpenNefia.Prototypes.Entity.Elona = {
AreaVernis = {
    MetaData = {
        Name = "维尔尼斯",
    },
    AreaEntrance = {
        EntranceMessage = "可以看到维尔尼斯城。周围充满了活力。",
    },

    VillagerTalk = {
        "欢迎来到维尔尼斯。",
        function(npc)
            return ("多亏了矿山，这个城市才如此兴旺%s。"):format(_.noda(npc))
        end,
        function(npc)
            return ("维尔尼斯是一个历史悠久的煤矿城%s。"):format(_.da(npc))
        end,
        function(npc)
            return ("最好别在那个酒吧弹钢琴%s。"):format(_.yo(npc))
        end,
        function(npc)
            return ("卷入叶尔斯和欧达那之间的战争是很遗憾的%s。"):format(_.da(npc))
        end,
        function(npc)
            return ("听说在莱施马斯发现了什么东西%s。"):format(_.yo(npc))
        end,
        function(npc)
            return ("好像有办法学到新的技能%s。"):format(_.yo(npc))
        end,
        function(npc)
            return ("注意不要盲目携带太多行李%s。"):format(_.na(npc))
        end,
        function(npc)
            return ("那个墓地附近的人？啊，那与毒品无关%s。"):format(
                _.yo(npc)
            )
        end,
        function(npc)
            return ("米谢斯是个玩偶迷%s。"):format(_.da(npc))
        end,
        function(npc)
            return ("去帕尔米亚王都的话，只需要一直往东走%s。"):format(
                _.yo(npc)
            )
        end,
        function(npc)
            return ("西娜的臀部是最好的%s。"):format(_.da(npc))
        end,
        function(npc)
            return ("最近，出现了一帮叫做“盗贼团”的人，很令人困扰%s。"):format(_.yo(npc))
        end,
    },
},

AreaYowyn = {
    MetaData = {
        Name = "约温",
    },
    AreaEntrance = {
        EntranceMessage = "可以看到约温村。有一股怀旧的土壤气息。",
    },

    VillagerTalk = {
        function(npc)
            return ("就是这样一个乡村镇也有称作约温的名字%s。"):format(
                _.da(npc)
            )
        end,
        function(npc)
            return ("如果要买马，最好在这里买%s。"):format(_.yo(npc))
        end,
        function(npc)
            return ("收成季节总是人手不够%s。"):format(_.na(npc))
        end,
        function(npc)
            return ("虽然是个没什么好东西的地方，但是慢慢欣赏还是不错的%s。"):format(_.yo(npc))
        end,
        function(npc)
            return ("听说西边有个无法之城%s。"):format(_.da(npc))
        end,
        function(npc)
            return ("离开城市后，沿着东边的道路走就能到达王都%s。"):format(_.da(npc))
        end,
        function(npc)
            return ("西南方看到过一座古城%s。"):format(_.noda(npc))
        end,
        function(npc)
            return ("这个城市的丧葬用品是无与伦比的%s。"):format(_.da(npc))
        end,
    },
},

AreaPalmia = {
    MetaData = {
        Name = "帕尔米亚",
    },
    AreaEntrance = {
        EntranceMessage = "可以看到帕尔米亚都。都市被高墙围住。",
    },

    VillagerTalk = {
        "欢迎来到帕尔米亚国的王都。",
        function(npc)
            return ("怎么说都无法改变米拉的说话方式%s。"):format(_.na(npc))
        end,
        function(npc)
            return (
                "帕尔米亚什么都有，很方便%s，但因为太大了找人很麻烦%s。"
            ):format(_.ga(npc), _.da(npc))
        end,
        function(npc)
            return ("贾比和斯塔夏非常要好%s。"):format(_.noda(npc))
        end,
        function(npc)
            return ("帕尔米亚的特产就是贵族的玩物%s。"):format(_.da(npc))
        end,
    },
},

AreaDerphy = {
    MetaData = {
        Name = "达尔菲",
    },
    AreaEntrance = {
        EntranceMessage = "可以看到达尔菲城。嗅到一股危险的气息。",
    },

    VillagerTalk = {
        "欢迎来到无法之城达尔菲。",
        function(npc)
            return ("无法变成像诺埃尔那样的人%s。"):format(_.na(npc))
        end,
        function(npc)
            return ("如果对那方面有兴趣，去盗贼公会会比较好%s。"):format(_.yo(npc))
        end,
        function(npc)
            return ("奴隶是社会必需品%s。"):format(_.da(npc))
        end,
        function(npc)
            return ("喜欢在竞技场看到鲜血%s。"):format(_.da(npc))
        end,
        function(npc)
            return ("这个城市没有卫兵%s。"):format(_.yo(npc))
        end,
    },
},

AreaPortKapul = {
    MetaData = {
        Name = "港口卡普尔",
    },
    AreaEntrance = {
        EntranceMessage = "可以看到港口卡普尔。港口繁忙的船只。",
    },

    VillagerTalk = {
        function(npc)
            return ("闻起来有海风的味道%s。"):format(_.na(npc))
        end,
        function(npc)
            return ("在宠物竞技场观战是我的爱好%s。"):format(_.da(npc))
        end,
        function(npc)
            return ("拉斐尔是女性的天敌%s。"):format(_.dana(npc))
        end,
        function(npc)
            return ("如果想变得更强，可以去战士公会%s。"):format(_.yo(npc))
        end,
        function(npc)
            return ("这里的海产品在内陆地区很受欢迎%s。"):format(_.noda(npc))
        end,
        function(npc)
            return ("要怎样才能进入金字塔呢%s？"):format(_.noda(npc))
        end,
    },
},AreaNoyel = {
        MetaData = {
            Name = "诺埃尔",
        },
        AreaEntrance = {
            EntranceMessage = "这里是诺埃尔村庄。可以听到孩子们的笑声。",
        },

        VillagerTalk = {
            function(npc)
                return ("哦呀！呼，今天也很冷%s。"):format(_.na(npc))
            end,
            function(npc)
                return ("每天都要扫雪真辛苦%s。"):format(_.da(npc))
            end,
            function(npc)
                return ("那个巨人的名字叫埃文%s。"):format(_.da(npc))
            end,
            function(npc)
                return ("如果不能忍受罪恶感的话，最好去教堂%s。"):format(_.yo(npc))
            end,
            "好冷啊！",
            function(npc)
                return ("稍微往南走有一座小工房%s。"):format(
                    _.noda(npc)
                )
            end,
        },
    },

    AreaLumiest = {
        MetaData = {
            Name = "卢米埃斯特",
        },
        AreaEntrance = {
            EntranceMessage = "可以看到卢米埃斯特城市。可以听到水流声。",
        },

        VillagerTalk = {
            "欢迎来到水与艺术之城。",
            function(npc)
                return ("听说这附近有一座有名的温泉城市%s。"):format(_.da(npc))
            end,
            function(npc)
                return ("在这个城市，无论哪里都可以钓鱼%s。"):format(_.noda(npc))
            end,
            function(npc)
                return ("对于绘画很挑剔%s。"):format(_.yo(npc))
            end,
            function(npc)
                return ("帕尔米亚城没有魔法师公会，只有这个城市有%s。"):format(_.noda(npc))
            end,
        },
    },

    AreaShelter = {
        VillagerTalk = {
            function(npc)
                return ("希望天气能早点好起来%s。"):format(_.na(npc))
            end,
            function(npc)
                return ("这个避难所是大家的捐款建造的%s。"):format(_.noda(npc))
            end,
            function(npc)
                return ("有了这个避难所真是太好了%s。"):format(_.yo(npc))
            end,
            function(npc)
                return ("好无聊啊%s。"):format(_.da(npc))
            end,
            function(npc)
                return ("感觉有点兴奋%s。"):format(_.yo(npc))
            end,
            function(npc)
                return ("看起来食物是充足的%s。"):format(_.da(npc))
            end,
        },
    },
}