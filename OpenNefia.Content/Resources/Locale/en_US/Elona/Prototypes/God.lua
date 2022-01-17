OpenNefia.Prototypes.Elona.God.Elona = {
    Ehekatl = {
        Name = "Ehekatl of Luck",
        ShortName = "Lulwy",
        Desc = {
            Ability = "Ehekatl school of magic (Passive: Randomize casting mana<br>cost.)",
            Bonus = "CHR/LUCK/Evasion/Magic Capacity/Fishing/Lockpick",
            Offering = "Corpses/Fish",
            Text = "Ehekatl is a goddess of luck. Those faithful to Ehekatl are<br>really lucky.",
        },
        Servant = "Weapons and armor licked by this cat receive a blessing of Ehekatl which adds an extra enchantment.",
        Talk = {
            Believe = "Weee! You really have faith in me? Do you really?",
            Betray = "Mewmewmew! Really betray me? Really betray me?",
            FailToTakeOver = "Stupid stupid! Stupid!",
            Kill = { "More! More!", "It's dead! Dead!", "Meoow mew mew." },
            Night = "Are you going to sleep? Really sleepy? Good night!",
            Offer = "Mew mew mew meow!",
            Random = "Coconut crab!",
            ReadyToReceiveGift = "My heart aches! I think I like you...I guess!",
            ReadyToReceiveGift2 = "I love you! Love you! You will be with me forever...promise!",
            ReceiveGift = "Here's a gift! For you! Mewmew!",
            TakeOver = "Weeee! I'm so happy. I like you! I like you!",
            Welcome = "Weeee♪ You are back? You are back!",
            WishSummon = "\"Meeewmew!\"",
        },
    },

    Itzpalt = {
        Name = "Itzpalt of Element",
        ShortName = "Itzpalt",
        Desc = {
            Ability = "Absorb mana (Absorb mana from the air.)",
            Bonus = "MAG/Meditation/RES Fire/RES Cold/RES Lightning",
            Offering = "Corpses/Staves",
            Text = "Itzpalt is a god of elements. Those faithful to Itzpalt are<br>protected from elemental damages and learn to absorb mana from<br>their surroundings.",
        },
        Servant = "This exile can cast several spells in a row.",
        Talk = {
            Kill = {
                "Impressive.",
                "And so the soul returns to the element.",
                "Chant my name proudly. Flame and eternal rest for the dead.",
            },
            Believe = "Do not fail me, servant.",
            Betray = "Remember mortal, a betrayal is not something I can forgive.",
            FailToTakeOver = "You shall pay a painful price for your foolish try.",
            Night = "Relieve your fatigue. May the eternal flame protect you from filthy beings that crawl in the shroud of night.",
            Offer = "I appreciate it, mortal.",
            Random = {
                "Idleness is the devil's workshop.",
                "Idleness is the devil's workshop.",
                "You mortals will never understand the pain we hold.",
                "You mortals will never understand the pain we hold.",
                "My name is Itzpalt. I am the origin of elements, the king that bears the earliest flame and the master of all the Gods.",
                "My name is Itzpalt. I am the origin of elements, the king that bears the earliest flame and the master of all the Gods.",
                "The god's war never ends. In times to come, you shall be a warrior of my faction.",
                "The god's war never ends. In times to come, you shall be a warrior of my faction.",
                "The god's war never ends. In times to come, you shall be a warrior of my faction.",
            },
            ReadyToReceiveGift = "Long gone those worthy to bear my name are. But perhaps...",
            ReadyToReceiveGift2 = "Very impressive, mortal. You shall be the one worthy to carry my name.",
            ReceiveGift = "To answer your loyalty, you shall have this reward.",
            TakeOver = "Impressive. Your action shall be remembered.",
            Welcome = "And so the mortal returns. You shall serve me again.",
            WishSummon = nil, -- TODO No summon for now.
        },
    },

    Jure = {
        Name = "Jure of Healing",
        ShortName = "Jure",
        Desc = {
            Ability = "Prayer of Jure (Heal yourself.)",
            Bonus = "WIL/Healing/Meditation/Anatomy/Cooking/Magic Device/Magic Capacity",
            Offering = "Corpses/Ores",
            Text = "Jure is a god of healing. Those faithful to Jure can heal wounds.",
        },
        Servant = "This defender can use Lay on hand to heal a mortally wounded ally. The ability becomes re-useable after sleeping.",
        Talk = {
            Believe = "I-I don't expect you to be my servant...I-I r-really don't, you silly!",
            Betray = "I-I don't miss you. I-I really don't!",
            FailToTakeOver = "W-What have you done!",
            Kill = { "N-Not bad, for you", "S-Stop it. Disgusting...", "Don't look at me!" },
            Night = "I-I'll never give you a good night kiss...ever!",
            Offer = "I-I'm not that pleased. I-I'm not, you silly!",
            Random = { "W-What? Silly!", "Am I really suitable for this job?" },
            ReadyToReceiveGift = "N-No! Cut it! I-I don't love you. Stupid!",
            ReadyToReceiveGift2 = "I-I'm not falling l-love with you! D-don't you never ever leave me...okay? You stupid...!",
            ReceiveGift = "I-I'm not giving it to you as my gratitude! I-I r-really am not! Silly!",
            TakeOver = "W-What? I-I'm not gonna praise you. I'm not!",
            Welcome = "N-no...I-I wasn't looking for you! Silly!",
            WishSummon = nil, -- TODO No summon for now.
        },
    },

    Kumiromi = {
        Name = "Kumiromi of Harvest",
        ShortName = "Kumiromi",
        Desc = {
            Ability = "Kumiromi's recycle (Passive: Extract seeds from rotten <p>foods.)",
            Bonus = "PER/DEX/LER/Gardening/Alchemy/Tailoring/Literacy",
            Offering = "Corpses/Vegetables/Seeds",
            Text = "Kumiromi is a god of harvest. Those faithful to Kumiromi receive<br>the blessings of nature.",
        },
        Servant = "This fairy generates a seed after eating.",
        Talk = {
            Believe = "Welcome...I have...expectations of you...",
            Betray = "A traitor...I can't...tolerate....",
            FailToTakeOver = "I...will have no mercy..for enemies...absolutely.",
            Kill = { "You got dirty.", "Don't kill too much.", "...are you satisfied now?" },
            Night = "Good night...May a bright sprout bring forth tomorrow... ",
            Offer = "This is...very good....Thank you.",
            Random = {
                "Twitter of trees...song weaved by forests...if you strain your ears....",
                "This conflict between Gods...ugly.",
                "My Ehekatl...you are not what you used to be...",
                "I will give...more than you spoil...",
            },
            ReadyToReceiveGift = "You are...my precious valet...",
            ReadyToReceiveGift2 = {
                "Together forever...right? I will not let you go...until you die.",
            },
            ReceiveGift = "Here...there's something for...you...",
            TakeOver = "Good work...truly...",
            Welcome = "Welcome back...I was...waiting for you.",
            WishSummon = "Under construction.",
        },
    },

    Lulwy = {
        Name = "Lulwy of Wind",
        ShortName = "Lulwy",
        Desc = {
            Ability = "Lulwy's trick (Boost your speed for a short time.)",
            Bonus = "PER/SPD/Bow/Crossbow/Stealth/Magic Device",
            Offering = "Corpses/Bows",
            Text = "Lulwy is a goddess of wind. Those faithful to Lulwy receive<br>the blessing of wind and can move swiftly.",
        },
        Servant = "This black angel shows enormous strength when boosting.",
        Talk = {
            Believe = "You know, you've made a right choice. I will treat you with great love, little kitty.",
            Betray = "Huh, fool. A life without me is all but empty.",
            FailToTakeOver = "Bad bad puppy. I need to punish you.",
            Kill = { "How dirty. Wipe the blood off your hands.", "Aha! Mince! Mince!.", "Bad kitty." },
            Night = "Fine, I'll unshackle you for a little while. Enjoy your sleep. But remember kitty, if you cheat on me in your dream, you'll never see a daylight again.",
            Offer = "Oh, such a nice gift. Do you have a secret intention or something?",
            Random = {
                "Pathetic pigs.",
                "Mani? Say that name again and I'll mince you, kitty.",
                "I've torn the former servant limb from limb to feed the sylphs. I just didn't like his hair style. Ahaha!",
                "My children are the voices of the wind, never tied to anything.",
            },
            ReadyToReceiveGift = "Serve me to the end of this pathetic world, for you are my dearest slave.",
            ReadyToReceiveGift2 = "Obey me and surrender everything. I shall mince all the pigs that try to hurt your beautiful face, my kitty.",
            ReceiveGift = "My dear little kitty, you deserve a reward, don't you think?",
            TakeOver = "Oh my little puppet, you serve me well.",
            Welcome = "Where were you until now? You need more breaking, it seems.",
            WishSummon = "\"You dare to call my name?\"",
        },
    },

    Mani = {
        Name = "Mani of Machine",
        ShortName = "Mani",
        Desc = {
            Ability = "Mani's decomposition (Passive: Extract materials<br>from traps.)",
            Bonus = "DEX/PER/Firearm/Healing/Detection/Jeweler/Lockpick/Carpentry",
            Offering = "Corpses/Guns/Machinery",
            Text = "Mani is a clockwork god of machinery. Those faithful to Mani<br>receive immense knowledge of machines and learn a way to use them<br>effectively.",
        },
        Servant = "This android shows enormous strength when boosting.",
        Talk = {
            Believe = "Oh, someone comes to me finally. Devote yourself to my tasks, you'll be rewarded greatly, maybe.",
            Betray = "Great, a traitor.",
            FailToTakeOver = "Heh, Nice try.",
            Kill = {
                "Nice kill.",
                "Don't you want to decompose it?",
                "Ah, this soul could be a good component for a new machine.",
            },
            Night = "Flesh and blood, how pathetic to waste much of your limited life sleeping. But rest well for now, for you will need to serve me again.",
            Offer = "Cool. I like it.",
            Random = {
                "You should mechanize your body.",
                "Always behave true to my name.",
                "No need to hurry. The day machines dominate the world is much closer than you think.",
            },
            ReadyToReceiveGift = "Truly, you are an ideal believer. I'm impressed.",
            ReadyToReceiveGift2 = "Sacrifice your very soul to me and I shall protect you with my name.",
            ReceiveGift = "You've been a faithful servant of me. Here, use it wisely.",
            TakeOver = "Well done. After all, you might be useful than I thought.",
            Welcome = "Ah, you've returned. You're tougher than I thought.",
            WishSummon = "Under construction.",
        },
    },

    Opatos = {
        Name = "Opatos of Earth",
        ShortName = "Opatos",
        Desc = {
            Ability = "Opatos' shell (Passive: Reduce any physical damage you<br>receive.)",
            Bonus = "STR/CON/Shield/Weight Lifting/Mining/Magic Device",
            Offering = "Corpses/Ores",
            Text = "Opatos is a god of earth. Those faithful to Opatos have massive<br>strength and defense.",
        },
        Servant = "This knight can hold really heavy stuff for you.",
        Talk = {
            Believe = "Muwahahahahaha. You're mine!",
            Betray = "Muwahahahahahahaha!",
            FailToTakeOver = "Muwahahahaha! Weak! Weak!",
            Kill = { "Mwahaha!", "Die! Die! Mwahahaha!", "Muhan!" },
            Night = "Muwahaha! I'll be following you, to your dream.",
            Offer = "Muwahahahahaha!",
            Random = "Muwahaha.",
            ReadyToReceiveGift = "Mwahaha haha! Fun! Fun!",
            ReadyToReceiveGift2 = "Muwahahahaha! Mwaaaahahaha!",
            ReceiveGift = "Muwahahahaha, take it!",
            TakeOver = "Muwahaha muwaha. Good. Good!",
            Welcome = "Muwahahahahaha! Welcome back.",
            WishSummon = "Under construction.",
        },
    },
}