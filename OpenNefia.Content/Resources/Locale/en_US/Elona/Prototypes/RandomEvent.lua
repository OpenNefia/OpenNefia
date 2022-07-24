OpenNefia.Prototypes.Elona.RandomEvent.Elona = {
    AvoidingMisfortune = {
        Title = "Avoiding Misfortune",
        Body = "You sense a bad feeling for a moment but it fades away quickly.",
        Choices = {
            ["0"] = "Good.",
        },
    },
    CampingSite = {
        Title = "Camping Site",
        Body = "You discover a camping site someone left behind. Chunks of leftovers and junks remain here. You may possibly find some useful items.",
        Choices = {
            ["0"] = "(Search)",
            ["1"] = "(Leave)",
        },
    },
    Corpse = {
        Title = "Corpse",
        Body = "You find a corpse of an adventurer. There're bones and equipment scatters on the ground waiting to decay.",
        Choices = {
            ["0"] = "(Loot)",
            ["1"] = "(Bury)",
        },

        Loot = "You loot the remains.",
        Bury = "You bury the corpse with respect.",
    },
    SmallLuck = {
        Title = "Small Luck",
        Body = "You stumble over a stone and find some materials on the ground. ",
        Choices = {
            ["0"] = "Nice.",
        },
    },
    SmellOfFood = {
        Title = "Smell of Food",
        Body = "A sweet smell of food floats from nowhere. Your stomach growls but you can't find out where it comes from.",
        Choices = {
            ["0"] = "I'm hungry now!",
        },
    },
    StrangeFeast = {
        Title = "Strange Feast",
        Body = "You come across a strange feast.",
        Choices = {
            ["0"] = "(Eat)",
            ["1"] = "(Leave)",
        },
    },
    Murderer = {
        Title = "Murderer",
        Body = "Suddenly, a painful shriek rises from somewhere in the town. You see several guards hastily run by.",
        Choices = {
            ["0"] = "Sorry for you.",
        },

        Scream = function(victim)
            return ("%s screams, \"Ahhhhhhh!\""):format(_.name(victim))
        end,
    },
    MadMillionaire = {
        Title = "Mad Millionaire",
        Body = "A rich mad man is scattering his money all over the ground.",
        Choices = {
            ["0"] = "What a luck!",
        },

        YouPickUp = function(amount)
            return ("You pick up %s gold pieces."):format(amount)
        end,
    },
    WanderingPriest = {
        Title = "Wandering Priest",
        Body = "A priest comes up to you and casts a spell on you. No problem.",
        Choices = {
            ["0"] = "Thanks.",
        },
    },
    GainingFaith = {
        Title = "Gaining Faith",
        Body = "In your dream, a saint comes out and blesses you.",
        Choices = {
            ["0"] = "Great.",
        },
    },
    TreasureOfDream = {
        Title = "Treasure of Dream",
        Body = "You buried treasure in your dream. You quickly get up and write down the location.",
        Choices = {
            ["0"] = "Woohoo!",
        },
    },
    WizardsDream = {
        Title = "Wizard's Dream",
        Body = "In your dream, you meet a wizard with a red mustache. Who are you? Hmm, I guess I picked up the wrong man's dream. My apology for disturbing your sleep. To make up for this... The wizard draws a circle in the air and vanishes. You feel the effects of a faint headache.",
        Choices = {
            ["0"] = "A weird dream.",
        },
    },
    LuckyDay = {
        Title = "Lucky Day",
        Body = "Mewmewmew!",
        Choices = {
            ["0"] = "Woohoo!",
        },
    },
    QuirkOfFate = {
        Title = "Quirk of Fate",
        Body = "Mewmew? You've found me!",
        Choices = {
            ["0"] = "Woohoo!",
        },
    },
    MonsterDream = {
        Title = "Monster Dream",
        Body = "You are fighting an ugly monster. You are about to thrust a dagger into the neck of the monster. And the monster screams. You are me! I am you! You are awakened by your own low moan.",
        Choices = {
            ["0"] = "Urrgh..hh..",
        },
    },
    DreamHarvest = {
        Title = "Dream Harvest",
        Body = "In your dream, you harvest materials peacefully.",
        Choices = {
            ["0"] = "Sweet.",
        },
    },
    YourPotential = {
        Title = "Your Potential",
        Body = "Suddenly you develop your gift!",
        Choices = {
            ["0"] = "Woohoo!",
        },
    },
    Development = {
        Title = "Development",
        Body = "You lie awake, sunk deep into thought. As memories of your journey flow from one into another, you chance upon a new theory to improve one of your skills.",
        Choices = {
            ["0"] = "Good!",
        },
    },
    CreepyDream = {
        Title = "Creepy Dream",
        Body = "In your dreams, several pairs of gloomy eyes stare at you and laughter seemingly from nowhere echoes around you.  Keh-la keh-la keh-la I found you...I found you.. keh-la keh-la keh-la After tossing around a couple times, the dream is gone.",
        Choices = {
            ["0"] = "Strange...",
        },
    },
    CursedWhispering = {
        Title = "Cursed Whispering",
        Body = "Your sleep is disturbed by a harshly whispering that comes from nowhere.",
        Choices = {
            ["0"] = "Can't...sleep...",
        },

        NoEffect = "Your prayer nullifies the curse.",
    },
    Regeneration = {
        Title = "Regeneration",
        Body = "Your entire body flushes. When you wake up, a scar in your arm is gone.",
        Choices = {
            ["0"] = "Good.",
        },
    },
    Meditation = {
        Title = "Meditation",
        Body = "In your dream, you meditate and feel inner peace.",
        Choices = {
            ["0"] = "Good.",
        },
    },
    MaliciousHand = {
        Title = "Malicious Hand",
        Body = "A malicious hand slips and steals your money.",
        Choices = {
            ["0"] = "Bloody thieves...",
        },

        YouLose = function(amount)
            return ("You lose %s gold pieces."):format(amount)
        end,
        NoEffect = "The thief fails to steal money from you.",
    },
    GreatLuck = {
        Title = "Great Luck",
        Body = "You stumble over a stone and find a platinum coin.",
        Choices = {
            ["0"] = "What a luck!",
        },
    },
    Marriage = {
        Title = "Marriage",
        Body = function(lover)
            return ("At last, you and %s are united in marriage! After the wedding ceremony, you receive some gifts."):format(
                _.name(lover)
            )
        end,
        Choices = {
            ["0"] = "Without you, life has no meaning.",
        },
    },
    ReunionWithPet = {
        Title = "Reunion with your pet",
        Body = "As you approach the mining town, you notice a familiar call and stop walking. Your old pet who got separated from you during the shipwreck is now running towards you joyfully! Your pet is...",
        Choices = {
            ["0"] = "a dog!",
            ["1"] = "a cat!",
            ["2"] = "a bear!",
            ["3"] = "a little girl!",
        },
    },
}
