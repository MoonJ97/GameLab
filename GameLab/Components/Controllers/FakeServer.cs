using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using GameLab.Models;

namespace GameLab
{
    /// <summary>
    /// Simulates a game server by updating objects in the world 
    /// once per frame, with a certain frame rate
    /// </summary>
    internal class FakeServer
    {
        /// <summary>
        /// The Model
        /// </summary>
        private World theWorld;

        /// <summary>
        /// RNG used to place new objects in random locations
        /// </summary>
        private Random rand = new();

        /// <summary>
        /// Timer for simulating the frame rate of a game
        /// </summary>
        System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();

        /// <summary>
        /// Delay between each frame (determines frame rate)
        /// </summary>
        private long msPerFrame = 20;

        /// <summary>
        /// Maximum number of allowed players
        /// </summary>
        private int maxPlayers = 50;

        /// <summary>
        /// Maximum number of allowed powerups
        /// </summary>
        private int maxPowerups = 50;

        /// <summary>
        /// Size of the world on one side
        /// </summary>
        private int size;

        /// <summary>
        /// Used to assign unique IDs to players
        /// </summary>
        private int nextPlayerID = 0;

        /// <summary>
        /// Used to assign unique IDs to powerups
        /// </summary>
        private int nextPowID = 0;

        /// <summary>
        /// Create a new server with the given size
        /// </summary>
        /// <param name="s"></param>
        public FakeServer( int s )
        {
            size = s;
            theWorld = new( size );
        }

        /// <summary>
        /// Simulates a client requesting data over the network,
        /// delays by the frame time before responding with new World data
        /// </summary>
        /// <param name="ships"></param>
        /// <param name="pows"></param>
        public void GetData(out List<Player> ships, out List<Powerup> pows)
        {
            if(!watch.IsRunning)
                watch.Start();

            while ( watch.ElapsedMilliseconds < msPerFrame )
            { /* empty loop body */ }

            watch.Restart();

            Update();
            ships = new(theWorld.Players.Values);
            pows = new(theWorld.Powerups.Values);
        }

        /// <summary>
        /// Updates every object in the world for one frame
        /// </summary>
        private void Update()
        {
            // cleanup the deactivated objects
            IEnumerable<int> playersToRemove = theWorld.Players.Values.Where(x => !x.Active).Select(x => x.ID);
            IEnumerable<int> powsToRemove = theWorld.Powerups.Values.Where(x => !x.Active).Select(x => x.ID);
            foreach ( int i in playersToRemove )
                theWorld.Players.Remove( i );
            foreach ( int i in powsToRemove )
                theWorld.Powerups.Remove( i );

            // add new objects back in
            int halfSize = size / 2;
            while ( theWorld.Players.Count < maxPlayers )
            {
                Player p = new Player( nextPlayerID++ , rand.Next( size ), rand.Next( size ), rand.NextDouble() * 360 );
                theWorld.Players.Add( p.ID, p );
            }

            while ( theWorld.Powerups.Count < maxPowerups )
            {
                Powerup p = new Powerup( nextPowID++ , rand.Next( size ), rand.Next( size ) );
                theWorld.Powerups.Add( p.ID, p );
            }

            // move/update the existing objects in the world
            foreach ( Player p in theWorld.Players.Values )
                p.Step( theWorld.Size );

            foreach ( Powerup p in theWorld.Powerups.Values )
                p.Step();

        }
    }
}
