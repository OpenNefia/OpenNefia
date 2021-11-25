using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Why.Core.IoC;
using Why.Core.Maps;

namespace Why
{
    /// <summary>
    /// I have no idea what I'm doing anymore.
    /// 
    /// I figure that if I'm dumb enough to start over, and clueless enough to have no sense of
    /// what's decent, I might as well throw things around and see what happens. This time I'm prototyping an ECS-based
    /// approach like Caves of Qud, by blatantly repurposing some architecture from RobustToolbox.
    /// (I don't claim to understand half of their design decisions, but I don't want to spend hundreds
    /// of hours building the parts I know I need if many other people have already done a lot of the work.)
    /// 
    /// My reasoning for using an ECS is that ON/LOVE's events lacked organization, and while I 
    /// lamented this, I didn't really have any solutions. Components and Systems
    /// are one solution to the organizational problem - new data goes into Components, new logic
    /// goes into Systems. Previously, global and map object-local events were registered as
    /// independent units, with no grouping - Systems act as that grouping. Events were going to need
    /// to be put somewhere eventually. Aspects were a slight improvement but held too much logic
    /// and tried to do too many things. Trying to go off and reinvent everything by myself doesn't
    /// tend to work well, but even so, ON/LOVE wasn't exactly refactoring-friendly to correct those
    /// mistakes later.
    /// 
    /// What about the hybrid approach that does use components but still has a class hierarchy like
    /// RimWorld? My opinion is that it's trying to be an ECS but not really, so not going in all the
    /// way has its drawbacks. With an entire component system at the ready, why distinguish characters 
    /// and items at a type level anymore? It's a different paradigm that needs to be played to its strengths.
    /// This is nothing more than a blind, foolish experiment to see what happens if I do go all in on an ECS.
    /// 
    /// Still, I don't know if this will work. I might have to go for something stripped-down
    /// and less complex than RobustToolbox. This will probably not be what the final API looks like.
    /// 
    /// I like writing massive paragraphs of text to make it sound like I know what I'm talking about.
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
            IoCSetup.Run();
            IoCManager.BuildGraph();

            StartGame();
        }

        private static void StartGame()
        {
            var mapManager = IoCManager.Resolve<IMapManager>();

            var map = new Map(40, 10);

            mapManager.RegisterMap(map);

            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    Console.Write($"{(char)(map.Tiles[x, y].Type + 'A')}");
                }
                Console.Write("\n");
            }
        }
    }
}
