Elona.CharSheet = {
   Potential = {
      Superb = "Superb",
      Great = "Great",
      Good = "Good",
      Bad = "Bad",
      Hopeless = "Hopeless"
   },
   Name = "Name",
   Aka = "Aka",
   Race = "Race",
   Sex = "Sex",
   Class = "Class",
   Age = "Age",
   Height = "Height",
   Cm = "cm",
   Weight = "Weight",
   Kg = "kg",
   Level = "Level",
   Exp = "EXP",
   NextLv = "Next Lv",
   God = "God",
   Guild = "Guild",
   Sanity = "Sanity",
   Fame = "Fame",
   Karma = "Karma",
   Turns = "Turns",
   TurnsPassed = "Turns",
   Days = "Days",
   DaysPassed = "Days",
   Kills = "Kills",
   Time = "Time",
   CargoWt = "Cargo Wt",
   CargoLmt = "Cargo Lmt",
   EquipWt = "Equip Wt",
   DeepestLv = "Deepest Lv",
   DeepestLvDesc = function(level)
      local last = math.floor(level % 10)
      local lvlDesc = " Level"
      if last == 1 then
         return "st"..lvlDesc
      elseif last == 2 then
         return "nd"..lvlDesc
      elseif last == 3 then
         return "rd"..lvlDesc
      else
         return "th"..lvlDesc
      end
   end,
   Topic = {
      Attribute = "Attributes(Org) - Potential",
      Blessing = "Blessing and Hex",
      Trace = "Trace",
      Extra = "Extra Info",
      Rolls = "Combat Rolls"
   }
}