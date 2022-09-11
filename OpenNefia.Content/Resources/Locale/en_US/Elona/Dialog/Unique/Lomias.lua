Elona.Dialog.Unique.LomiasNewGame = {
    GameBegin = {
        Text = {
            T1_Lomias = {
                "...you...you're awake already? Remarkable. I was beginning to worry that nursing a lowly adventurer would bring our urgent travel to a halt.",
                "You were badly wounded, passing out on the bank of a river. It was fortunate that we found you before the dark mantle of night enveloped this whole valley, almost as if Ehekatl, the goddess of luck herself had her eyes upon you.",
                "...stop your curious eyes. Yes, we are sons of Vindale whom they call the irreverent forest. Though we Eleas, noble but blameless heretics, aren't keen to spend idle time responding to every senseless question about our race, you should be more thankful for your fate. If it weren't the lady Larnneire who cured your mortal wound, you wouldn't be hearing my tirade. For the lady is no ordinary Elea and only she can...",
            },
            T2_Larnneire = {
                "You talk too much Lomias, even though the one injured before you is still dazed.",
            },
            T3_Lomias = {
                function(speaker, player)
                    return ("...yes, it's a bad habit of mine. Well, %s..."):format(_.basename(player))
                end,
            },
        },
        RegainConsciousness = function(speaker, player)
            return ("%s regain%s consciousness."):format(_.name(player), _.s(player))
        end,
    },
}

Elona.Dialog.Unique.Lomias = {
    Tutorial0 = {
        Text = "It looks like you aren't familiar with this land. Before we leave, I can spare a moment to teach you a few lessons.",
        Choices = {
            Yes = "I would like to play the tutorial.",
            No = "I can take care of myself.",
        },
    },

    Tutorial0_Start = {
        Text = "A wise choice. I will start from the beginning.",
    },

    Tutorial1 = {
        Text = {
            "Pray tell me at least you know how to move. Sure, pressing cursor keys will do. But it's better to use a key pad if you have one. By using a key pad, you can easily access keys which are used frequently. Like [0] for picking up stuff, [5] for passing a turn and [*] for targeting.",
            "Although there're many actions you can perform, basically you can access them by using only 3 keys, [z],[x] and [c].",
            "Let's try them now. First, you need food to live. If your stomach is empty, you will lose Hp and your action speed slows down. Make sure you always have enough food in your inventory.",
            "Here, you can have this meat. Press [x], then press [9] a few times to select the <Eat> menu. Once there, select the meat on the ground.",
        },
        Choices = {
            Alright = "Alright.",
            Ate = "I ate it.",
        },
    },

    Tutorial1_Ate1 = {
        Text = "...err..you really ate that thing? Oh well..",
        Choices = {
            What = "What!",
        },
    },

    Tutorial1_Ate2 = {
        Text = {
            "You can also use other items by pressing [x]. For example, if you want to read a book, press [x], hit [9] a few times to select the <Read> menu, then choose a book you want to read.",
            "Now, allow me to explain how to act by using the [z] key.",
        },
    },

    Tutorial2 = {
        Text = {
            "You can perform skills or other actions including bashing and digging by pressing [z]. Here's a tip. You can bash doors to break locks and trees to get some fruits. It can be used to wake someone, but surely they won't be happy.",
            "Also, remember that [space] key is a very useful key. When there's an object under your foot, it automatically chooses a suitable action for you.",
            "Let's try it now. Try digging some walls by pressing [z] and choosing <dig>.",
        },
        Choices = {
            Okay = "Okay.",
        },
    },

    Tutorial3 = {
        Text = "Looks like you found something.",
    },

    Tutorial4 = {
        Text = {
            "Many items need to be identified before you can know what exactly they are. You can identify items by reading some scrolls or asking a wizard in a town. Remember that using unidentified potions or scrolls is very dangerous. ",
            "Weapons and armor also need to be identified. If you carry them long enough, you will get a hunch as to how good they are. But to gain full knowledge of the items, you need to identify them.",
            "Now I'll give you a scroll of identify. Read it by using [x] and identify the gold bar you just found.",
        },
        Choices = {
            Alright = "Alright, I will try.",
            Done = "Done...",
        },
    },

    Tutorial4_IdentifyDone = {
        Text = {
            "(Lomias grins. Looks like he buried it unnoticed.)",
            "Okay, I will now tell you how to fight. Before the combat starts, you need to equip weapons. Take my old bow and arrows and equip them.",
        },
    },

    Tutorial5 = {
        Text = "To equip weapons or armor, press [c] and press [9] to select [Wear]. Note that if you wear cursed equipment, they can't be removed normally and cause some unwelcome effects. That bow is cursed. Use this scroll of uncurse to uncurse it.",
        Choices = {
            Alright = "Will do.",
            Done = "All done.",
        },
    },

    Tutorial5_EquipDone = {
        Text = {
            "Good. Now listen carefully.",
            "By moving towards a target, you automatically attack it with your close range weapon. To use your long range weapon, you can either press [z] and choose [Shoot] or simply press [f] (Fire). You will shoot a nearby enemy. If you want to change your target, press [*].",
            "Get ready. I will summon some weak monsters. Kill them with your bow if possible. Try to stay away from the enemies as bows aren't effective in close range. I've dropped a few potions in case you get hurt. You know how to use them right? Yes, use [x] key.",
        },
        LomiasReleasesPutits = "Lomias releases tiny cute creatures.",
    },

    Tutorial6_PutitsRemaining = {
        Text = "Kill them all.",
    },

    Tutorial6_Finished = {
        Text = "Well done.",
    },

    Tutorial7 = {
        Text = {
            "Let's learn a little history of North Tyris. This sacred land governed by Palmia is known for ancient ruins <<Nefia>>. Occasionally new ruins are found and lost by erratic movements of the earths crust.",
            "A lord lives at the lowest layer of these ruins, protecting great treasures and therefore attracts numerous adventurers. However, avoid those ruins which exceed your current level. You may gain a lot, but you may lose your life.",
        },
    },

    Tutorial7_Chest = {
        Text = "You might find chests containing loot in ruins. There's one nearby, open it.",
    },

    Tutorial8 = {
        Text = {
            "Notice the chest has a lock? Locked chests require sufficient lockpick skill and lockpicks to open. You need to practice to open that chest. Be aware, those chests are heavy and trust me, give up if you can't open them when you're in dungeons. I saw a fool running around with a chest on his back and he got killed.",
            "As you explore dungeons, your backpack may get heavier. Remember to leave stuff you don't need in your house. Overweight will slow your movement.",
            "Finally, I'm going to explain a bit about your house. As you already know, you can safely store items in your house. And the salary chest periodically gets filled with some gold and items. Eventually you might be able to buy a new house.",
            "You can do several things by using a house board in your house. Try it later.",
        },
    },

    Tutorial99 = {
        Text = {
            "Alright, we're finished. You should already know how to survive in North Tyris by now.",
            "(You've finished the tutorial!)",
        },
    },

    TutorialAfter = {
        Text = "What?",
        Choices = {
            GetOut = "Get out of my house!",
            Nothing = "Nothing.",
        },
    },

    GetOut_LarnneireDied = {
        Text = "You...you scum!",
    },
    GetOut_Execute = {
        Text = {
            T1_Larnneire = {
                function(speaker, player)
                    return ("%s is right. The time left for us is scarce. We need to depart, Lomias."):format(
                        _.basename(player)
                    )
                end,
            },
            T2_Lomias = {
                "Yes. Palmia is still far away. Perhaps, It was fortunate that we could have a little rest before everything begins.",
                "Farewell..until we meet again. May the blessing of Lulwy be with you.",
            },
        },
    },
}
