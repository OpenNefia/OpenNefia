Elona.Food.Effect = {
    Poisoned = {
        Dialog = { "Gyaaaaa...!", "Ugh!" },
        Text = function(_1)
            return ("It's poisoned! %s writhe%s in agony!"):format(_.name(_1), _.s(_1))
        end,
    },
    Spiked = {
        Other = {
            function(_1)
                return ("%s gasps, \"I f-feel...strange...\""):format(_.name(_1))
            end,
            function(_1)
                return ("%s gasps \"Uh..uh..What is this feeling...\""):format(_.name(_1))
            end,
        },
        Self = "You are excited!",
    },

    Corpse = {
        Alien = function(_1)
            return ("Something gets into %s%s body."):format(_.name(_1), _.possessive(_1))
        end,
        At = "You dare to eat @...",
        Beetle = "Mighty taste!",
        Calm = function(_1)
            return ("Eating this brings %s inner peace."):format(_.name(_1))
        end,
        Cat = "How can you eat a cat!!",
        ChaosCloud = function(_1)
            return ("%s %s shaken by a chaotic power."):format(_.name(_1), _.is(_1))
        end,
        CupidOfLove = function(_1)
            return ("%s feel%s love!"):format(_.name(_1), _.s(_1))
        end,
        DeformedEye = "It tastes really, really strange.",
        Ether = function(_1)
            return ("Ether corrupts %s%s body."):format(_.name(_1), _.possessive(_1))
        end,
        Ghost = "This food is good for your will power.",
        Giant = "This food is good for your endurance.",
        Grudge = function(_1)
            return ("Something is wrong with %s%s stomach..."):format(_.name(_1), _.possessive(_1))
        end,
        Guard = "Guards hate you.",
        HolyOne = function(_1)
            return ("%s feel%s as %s %s been corrupted."):format(_.name(_1), _.s(_1), _.he(_1), _.has(_1))
        end,
        Horse = "A horsemeat! It's nourishing",
        Imp = "This food is good for your magic.",
        Insanity = "Sheer madness!",
        Iron = function(_1)
            return ("It's too hard! %s%s stomach screams."):format(_.name(_1), _.possessive(_1))
        end,
        Lightning = function(_1)
            return ("%s%s nerve is damaged."):format(_.name(_1), _.possessive(_1))
        end,
        Mandrake = function(_1)
            return ("%s %s magically stimulated."):format(_.name(_1), _.is(_1))
        end,
        Poisonous = "Argh! It's poisonous!",
        Putit = function(_1)
            return ("%s%s skin becomes smooth."):format(_.name(_1), _.possessive(_1))
        end,
        Quickling = function(_1)
            return ("Wow, %s speed%s up!"):format(_.name(_1), _.s(_1))
        end,
        RottenOne = "Of course, it's rotten! Urgh...",
        Strength = "This food is good for your strength.",
        Troll = "A troll meat. This must be good for your body.",
        Vesda = function(_1)
            return ("%s%s body burns up for a second."):format(_.name(_1), _.possessive(_1))
        end,
        LittleSister = function(_1)
            return ("%s evolve%s."):format(_.name(_1), _.s(_1))
        end,
    },
    FortuneCookie = function(_1)
        return ("%s read%s the paper fortune."):format(_.name(_1), _.s(_1))
    end,
    Mochi = {
        Chokes = function(_1)
            return ("%s choke%s on mochi!"):format(_.name(_1), _.s(_1))
        end,
        Dialog = _.quote "Mm-ghmm",
    },
    Herb = {
        Alraunia = "Your hormones are activated.",
        Curaria = "This herb invigorates you.",
        Mareilon = "You feel magical power springs up inside you.",
        Morgia = "You feel might coming through your body.",
        Spenseweed = "You feel as your sense is sharpened.",
    },
    KagamiMochi = "The food is a charm!",
    FairySeed = function(_1, _2)
        return ("「Ugh-Ughu」 %s spew%s up %s."):format(_.name(_1), _.s(_1), _2)
    end,
    SistersLoveFueledLunch = function(_1)
        return ("%s%s heart is warmed."):format(_.name(_1), _.possessive(_1))
    end,
}
