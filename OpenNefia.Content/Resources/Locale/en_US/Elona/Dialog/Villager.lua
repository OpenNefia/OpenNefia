Elona.Dialog.Villager = {
    Choices = {
        Talk = "Let's Talk.",
        Trade = "Are you interested in trade?",
        Sex = "Interested in a little tail t'night?",
    },

    Talk = {
        Default = {
            "Scut stands for it's cute.",
            "I've got nothing to do.",
            function(speaker)
                return ("%s? I've never heard of you."):format(_.alias(_.player()))
            end,
            function(speaker)
                return ("%s! You say you ARE %s?! ...nope, I've never heard of that. What is it?"):format(
                    _.alias(_.player()),
                    _.alias(_.player())
                )
            end,
            function(speaker)
                return (
                    "Hey aren't you the sparkling queer?  No?  The vigilant rainbow?  No?  Then what's your callsign?  %s?  That's a stupid name."
                ):format(_.alias(_.player()))
            end,
            "Some say the Vindale forest is the source of the etherwind.  I guess it's not impossible but it seems a bit silly.",
            "Our lives would be meaningless without the gods.",
            "The land of North Tyris has countless ruins and dungeons on it.  They are part of a long lost civilization we call Nefia.",
            "I'm so bored!",
            "You're an adventurer right? Got any news from afar?",
            "Nothing beats a good old crim ale after work.",
            "Cats... why are they so cute?",
            "What are you looking at?",
            "It is rumored that the prince of Zanan is planning a massive attack on the Vindale forests.",
            "The ether plague spreads from land to land. It started in the Vindale forest and brought ruin upon the eastern country of Karune. Fortunately, the great central ocean has, to some extent, been protecting us from the winds. I can't imagine any of the  countries in North Tyris would have survived that disaster.",
            "Ah, another adventurer looking for a fortune.",
            "Eh? What do you want?",
            "I hear that South Tyris is a violent and uninhabitable land.  Who knows what sort of horrors lurk there.",
        },

        Ally = {
            function(npc)
                return ("(%s fixedly looks at you.)"):format(_.name(npc))
            end,
            "...?",
            "Did you need something?",
            "What do you want?",
            "What's up?",
            "Is something wrong?",
            "What is it?",
        },

        Prostitute = {
            "Hey sexy, wanna relax for a while?",
        },

        Bored = {
            function(npc)
                return ("(%s is bored. )"):format(_.name(npc))
            end,
            function(npc)
                return ("(%s glances at you and steps back.)"):format(_.name(npc))
            end,
        },

        ChristmasFestival = {
            "Welcome to the Holy Night Festival! Enjoy yourself!",
            "The Holy Night Festival is also known as the Festival of St.Jure. This is a feast to honor the Goddess of Healing and to celebrate the end of the year.",
            "I heard Moyer is preparing some showpiece for the festival.",
            "You, have you seen the great celestial statue of Jure yet? Oh, what a beautiful woman she is!",
            "I will meet my lover tonight!",
            "Many tourists visit this town just to see the festival at this time of year.",
            "The Holy Night Festival is an old and historical event. It is said that even the royal family of Palmia used to visit this little town to offer their prayers to St.Jure.",
            "A legend says Jure's tear can cure any diseases. In fact, there have been many witnesses of her miracles in the past events. Such as an old blind woman recovering sight, can you believe it?",
        },

        Moyer = {
            "Hear hear! This monster before you is the notorious fire giant who ruled Verron of Nefia for a century. You're lucky, quite lucky to have him chained before you today! Indeed, this monster could burn the whole village if it weren't for the magical chains that bind him. Buy some goods at my shop, and I will even let you touch this fearsome giant!",
            "Don't worry. He can't move a muscle while he is shackled.",
        },

        Personality = {
            ["0"] = {
                "King Xabi is a man of his word.  You can trust him with anything.",
                "I want to drink crim ale to death!",
                "What's an another name for a hare's tail?",
                "King Xabi is a wise man.  At least, that's what I've heard.",
                "Sometimes I drink crim ale until I pass out.",
                "Cats... why are they so cute?",
                "Yerles is a new kingdom that has recently risen to prominence.",
                "Eulderna was the first country to explore and research the ruins of Nefia.",
                "I wonder if the shopkeepers really are invincible...",
                "Apparently, there is a way to learn the location of artifacts after all.",
                "Hungry sea lions love to eat fish.",
                "Sleep and rest well when you are sick! My grandma told me the blessed healing potion also works.",
            },
            ["1"] = {
                "Money makes the world go round.  I wonder where I can get some more money.",
                "Money and currency are critical to our society. ",
                "Do you have any good ideas for investments?",
                "I'm very much into economics. ",
                "Platinum coins are a lot rarer than gold.  Spend them wisely. ",
                "Eulderna always suffers from huge deficits, but it never really seems to slow them down.  They can't just invent money like that forever.",
                "Zanan's postwar system is a monarchy, but internally the country still operates on the outdated systems used during Eyth Terre.",
            },
            ["2"] = {
                "Sierre Terre is the 11th era on Irva.",
                "Are you on drugs? Irva is our world, of course.",
                "The land of North Tyris has countless ruins and dungeons on it.  They are part of a long lost civilization called Nefia.",
                "Science in the eighth era of Eyth Terre was far more advanced than ours.",
                "In Eyth Terre, magic and science were thought to be opposites.",
                "I like talking about science. ",
                "Vernis is the biggest coal-mining town in North Tyris.",
                "Lumiest is the famous city of art. ",
            },
            ["3"] = {
                "You should never leave home without an ample supply of food.  Starving to death would be a pretty abysmal fate.",
                "Eternal League...? I've never heard of that. ",
                "Sierre Terre is the 11th era.",
                "I like travelling.  I've been to so many places.",
                "If you find adventuring too difficult by yourself, go to Derphy and buy a few slaves to watch your back.",
                "Don't drop items in your shop that you want to keep. They will probably be sold!",
                "You can safely store items in buildings that you own.",
                "If you drop items in a town, the janitors will probably dump them.",
                "Guards will attack you if your karma drops too low.  At least *try* to keep a low profile.",
                "If your pets and allies fall in combat, bartenders in town can bring them back.",
                "Be sure to give good equipment to your allies and pets, since it will make them more effective in battle.",
                "Potions that reverse the effects of the etherwind exist, but they are rare and quite valuable.",
                "If you're planning to buy a building, start with a museum. The maintenance cost is cheap and they start making a profit quickly.",
                "One of your best sources of income and fame is the arena.",
                "If you are having hard time traveling, maybe you should purposely lower your fame.",
                "Gamble chests are good for practicing lock picking.",
            },
        },

        Rumor = {
            "Rabbit's tails are said to bring good luck to those who eat them.",
            "It's rumored that some beggars carry a magical pendant which purifies their stomach.",
            "Zombies sometimes drop a strange potion.",
            "Once I met this extraordinary bard who played a truly exquisite stringed instrument. He was so good that I even threw my expensive shoes at him!",
            "I've heard mummies carry a book that has power to resurrect the dead.",
            "I swear I saw it. The executioner came back to life after he got his head lopped off!",
            "Those who gaze into deformed eyes mutate. But they say that sometimes the eyes carry a potion that makes creatures evolve.",
            "Fairies hide a secret experience!",
            "Don't underestimate the rock thrower.  His rock can be very deadly.",
            "Those silver-eyed witches are deadly. You won't stand a chance against their huge swords.  I've heard they're not completely human and supposedly one of them carries even deadlier weapon.",
            "I saw a cupid carrying very heavy... thing.",
            "I met my god in a dream!",
            "If you ever encounter a four-armed monster wearing a yellow necklace, you'd better run like hell.",
            "Rogue assassins sometimes carry a magical necklace that gives you an extra attack.",
            "Some nobles collect strange things.",
            "Watch out for drunkards at parties! They sometimes throw crazy things at you.",
            "I saw a robot wearing a red plate girdle.",
            "A magical scroll imps carry can change the name of an artifact.",
            "The innocent girl in Yowyn has a secret treasure.",
            "Some time ago, I saw a hermit crab carrying a beautiful shell I've never seen before.",
            "Those robbing bastards, I hear they are addicted to some drugs.",
        },

        Shopkeeper = {
            "Welcome to my shop!",
            "We have the best selection of goods!",
            "I hate thieves.  No, not you, right?",
            "It's difficult to maintain a shop.",
            "I can handle bandits myself. We have to be tough.",
            "Running a store is hard work! ",
            "Come in.  Take a closer look at my wares. ",
            "I have confidence in my assortment of goods. ",
            "Look at our fine selection.",
            "I feel the world is growing more and more dangerous.",
            "I can't give you a fair price if your weapons and armors aren't fully identified.",
        },

        Slavekeeper = {
            "He he he.  I think I have just what you need.",
            "Don't look at me like that.",
            "What's your problem?",
        },
    },
}
