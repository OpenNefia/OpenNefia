Elona.Common =
{
   Quotes = function(str)
      return ("\"%s\""):format(str)
   end,
   ItIsImpossible = "It's impossible.",
   NothingHappens = "Nothing happens...",
   SomethingIsPut = "Something is put on the ground.",
   PutInBackpack = function(_1)
      return ("You put %s in your backpack.")
         :format(_1)
   end
}
