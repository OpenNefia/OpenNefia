OpenNefia.Prototypes.Elona.Element.Elona = {
    Chaos = {
        Name = "混沌",
        ShortName = "沌",
        Description = "混沌の効果への耐性",
        Ego = "混沌の",
        Resist = {
            Gain = function(_1)
                return ("%sは騒音を気にしなくなった。"):format(name(_1))
            end,
            Lose = function(_1)
                return ("%sはカオスへの理解を失った。"):format(name(_1))
            end,
        },
        Damage = function(_1)
            return ("%sは混沌の渦で傷ついた。"):format(name(_1))
        end,
        Death = {
            Active = "混沌の渦に吸い込んだ。",
            Passive = function(_1)
                return ("%sは混沌の渦に吸収された。"):format(name(_1))
            end,
        },
    },
    Cold = {
        Name = "冷気",
        ShortName = "冷",
        Description = "冷気や水への耐性",
        Ego = "冷たい",
        Resist = {
            Gain = function(_1)
                return ("%sの身体は急に冷たくなった。"):format(name(_1))
            end,
            Lose = function(_1)
                return ("%sは急に寒気を感じた。"):format(name(_1))
            end,
        },
        Damage = function(_1)
            return ("%sは凍えた。"):format(name(_1))
        end,
        Death = {
            Active = "氷の塊に変えた。",
            Passive = function(_1)
                return ("%sは氷の彫像になった。"):format(name(_1))
            end,
        },
    },
    Cut = {
        Name = "出血",
        Description = "切り傷への耐性",
        Ego = "出血の",
        Damage = function(_1)
            return ("%sは切り傷を負った。"):format(name(_1))
        end,
        Death = {
            Active = "千切りにした。",
            Passive = function(_1)
                return ("%sは千切りになった。"):format(name(_1))
            end,
        },
    },
    Darkness = {
        Name = "暗黒",
        ShortName = "闇",
        Description = "暗黒や盲目への耐性",
        Ego = "暗黒の",
        Resist = {
            Gain = function(_1)
                return ("%sは急に暗闇が怖くなくなった。"):format(name(_1))
            end,
            Lose = function(_1)
                return ("%sは急に暗闇が怖くなった。"):format(name(_1))
            end,
        },
        Damage = function(_1)
            return ("%sは闇の力で傷ついた。"):format(name(_1))
        end,
        Death = {
            Active = "闇に飲み込んだ。",
            Passive = function(_1)
                return ("%sは闇に蝕まれて死んだ。"):format(name(_1))
            end,
        },
    },
    Fire = {
        Name = "火炎",
        ShortName = "火",
        Description = "熱や炎への耐性",
        Ego = "燃える",
        Resist = {
            Gain = function(_1)
                return ("%sの身体は急に火照りだした。"):format(name(_1))
            end,
            Lose = function(_1)
                return ("%sは急に汗をかきだした。"):format(name(_1))
            end,
        },
        Damage = function(_1)
            return ("%sは燃え上がった。"):format(name(_1))
        end,
        Death = {
            Active = "燃やし尽くした。",
            Passive = function(_1)
                return ("%sは燃え尽きて灰になった。"):format(name(_1))
            end,
        },
    },
    Lightning = {
        Name = "電撃",
        ShortName = "雷",
        Description = "雷や電気への耐性",
        Ego = "放電する",
        Resist = {
            Gain = function(_1)
                return ("%sの身体に電気が走った。"):format(name(_1))
            end,
            Lose = function(_1)
                return ("%sは急に電気に敏感になった。"):format(name(_1))
            end,
        },
        Damage = function(_1)
            return ("%sに電流が走った。"):format(name(_1))
        end,
        Death = {
            Active = "焦げカスにした。",
            Passive = function(_1)
                return ("%sは雷に打たれ死んだ。"):format(name(_1))
            end,
        },
    },
    Magic = {
        Name = "魔法",
        ShortName = "魔",
        Description = "魔法攻撃への耐性",
        Resist = {
            Gain = function(_1)
                return ("%sの皮膚は魔力のオーラに包まれた。"):format(name(_1))
            end,
            Lose = function(_1)
                return ("%sの皮膚から魔力のオーラが消えた。"):format(name(_1))
            end,
        },
    },
    Mind = {
        Name = "幻惑",
        ShortName = "幻",
        Description = "精神攻撃への抵抗力",
        Ego = "霊的な",
        Resist = {
            Gain = function(_1)
                return ("%sは急に明晰になった。"):format(name(_1))
            end,
            Lose = function(_1)
                return ("%sは以前ほど明晰ではなくなった。"):format(name(_1))
            end,
        },
        Damage = function(_1)
            return ("%sは狂気に襲われた。"):format(name(_1))
        end,
        Death = {
            Active = "再起不能にした。",
            Passive = function(_1)
                return ("%sは発狂して死んだ。"):format(name(_1))
            end,
        },
    },
    Nerve = {
        Name = "神経",
        ShortName = "神",
        Description = "睡眠や麻痺への耐性",
        Ego = "痺れる",
        Resist = {
            Gain = function(_1)
                return ("%sは急に神経が図太くなった。"):format(name(_1))
            end,
            Lose = function(_1)
                return ("%sの神経は急に萎縮した。"):format(name(_1))
            end,
        },
        Damage = function(_1)
            return ("%sの神経は傷ついた。"):format(name(_1))
        end,
        Death = {
            Active = "神経を破壊した。",
            Passive = function(_1)
                return ("%sは神経を蝕まれて死んだ。"):format(name(_1))
            end,
        },
    },
    Nether = {
        Name = "地獄",
        ShortName = "獄",
        Description = "生命吸収への抵抗力",
        Ego = "地獄の",
        Resist = {
            Gain = function(_1)
                return ("%sの魂は地獄に近づいた。"):format(name(_1))
            end,
            Lose = function(_1)
                return ("%sの魂は地獄から遠ざかった。"):format(name(_1))
            end,
        },
        Damage = function(_1)
            return ("%sは冥界の冷気で傷ついた。"):format(name(_1))
        end,
        Death = {
            Active = "冥界に墜とした。",
            Passive = function(_1)
                return ("%sは冥界に墜ちた。"):format(name(_1))
            end,
        },
    },
    Poison = {
        Name = "毒",
        ShortName = "毒",
        Description = "毒への耐性",
        Ego = "毒の",
        Resist = {
            Gain = function(_1)
                return ("%sの毒への耐性は強くなった。"):format(name(_1))
            end,
            Lose = function(_1)
                return ("%sの毒への耐性は薄れた。"):format(name(_1))
            end,
        },
        Damage = function(_1)
            return ("%sは吐き気を催した。"):format(name(_1))
        end,
        Death = {
            Active = "毒殺した。",
            Passive = function(_1)
                return ("%sは毒に蝕まれて死んだ。"):format(name(_1))
            end,
        },
    },
    Sound = {
        Name = "音",
        ShortName = "音",
        Description = "音波や轟音への耐性",
        Ego = "震える",
        Resist = {
            Gain = function(_1)
                return ("%sは騒音を気にしなくなった。"):format(name(_1))
            end,
            Lose = function(_1)
                return ("%sは急に辺りをうるさく感じた。"):format(name(_1))
            end,
        },
        Damage = function(_1)
            return ("%sは轟音の衝撃を受けた。"):format(name(_1))
        end,
        Death = {
            Active = "聴覚を破壊し殺した。",
            Passive = function(_1)
                return ("%sは朦朧となって死んだ。"):format(name(_1))
            end,
        },
    },
    Acid = {
        Damage = function(_1)
            return ("%sは酸に焼かれた。"):format(name(_1))
        end,
        Death = {
            Active = "ドロドロに溶かした。",
            Passive = function(_1)
                return ("%sは酸に焼かれ溶けた。"):format(name(_1))
            end,
        },
    },
    Ether = {
        Ego = "エーテルの",
    },
    Fear = {
        Ego = "恐ろしい",
    },
    Rotten = {
        Ego = "腐った",
    },
    Silky = {
        Ego = "柔らかい",
    },
    Starving = {
        Ego = "飢えた",
    },
}
