//
// Move
//
// A move stores how the seeds of one user's move are distributed among the pits.
//


using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace Kalaha.Model
{

    /// <summary>
    /// A seed movement is a very simple object, consisting of the pit the seeds come from, the pit the seeds go to,
    /// and the number of seeds.
    /// </summary>
    public class SeedMovement
    {
        public int FromPit { get; set; }
        public int ToPit { get; set; }
        public int NumberOfSeeds { get; set; }

        public SeedMovement(int from, int to, int numOfSeeds)
        {
            FromPit = from;
            ToPit = to;
            NumberOfSeeds = numOfSeeds;
        }

        public override string ToString()
        {
            return (FromPit + " --> " + ToPit + ": " + NumberOfSeeds + " seed" + ((NumberOfSeeds == 1) ? "" : "s"));
        }
    }


    public class Move
    {
        // --- Attributes of the class ---
        private List<SeedMovement> _seedMovementList;


        // --- Methods of the class ---

        /// <summary>Default constructor</summary>
        public Move()
        {
            _seedMovementList = new List<SeedMovement>();
        }

        /// <summary>Adds a seed movement at the end of the list of movements.</summary>
        /// <param name="seedMovement">The seed movement to add</param>
        public void AddSeedMovement(SeedMovement seedMovement)
        {
            _seedMovementList.Add(seedMovement);
        }

        /// <summary>Adds a seed movement at the beginning of the list of movements.</summary>
        /// <param name="seedMovement">The seed movement to add</param>
        public void AddSeedMovementToTheFront(SeedMovement seedMovement)
        {
            _seedMovementList.Insert(0, seedMovement);
        }


        /// <returns>The number of currently stored seed movements</returns>
        public int GetNumberOfSeedMovements()
        {
            return _seedMovementList.Count;
        }

        /// <summary>Returns the seed movement that comes next in the move.</summary>
        public SeedMovement GetNextSeedMovement()
        {
            return GetSeedMovement(0);
        }

        /// <summary>Removes the given seed movement from the move.</summary>
        /// <param name="movementToRemove">The seed movement to remove from the move (yeah! :-).</param>
        public void RemoveSeedmovement(SeedMovement movementToRemove)
        {
            _seedMovementList.Remove(movementToRemove);
        }

        /// <returns>The number of currently stored seed movements</returns>
        public SeedMovement GetSeedMovement(int position)
        {
            if (position >= _seedMovementList.Count)
            {
                // The given position does not exist.
                throw new PSTException("Move.GetSeedMovement: Position too high: " + position + " (max = " +
                                            (_seedMovementList.Count - 1) + ").");
            }

            return _seedMovementList[position];
        }

        /// <summary>
        /// Returns the backward move of the given move.
        /// A "backward move" of a move has all the seed movements in the converse order, and each seed movement has swapped "FromPit" and "ToPit".
        /// </summary>
        public Move GetBackwardMove()
        {
            Move backMove = new Move();

            // Walk backwards through the list of seed movements:
            for (int index = _seedMovementList.Count - 1; index >= 0;  --index)
            {
                // Take the seed movement at position "index":
                SeedMovement origSeedMovement = _seedMovementList[index];

                // Create a new seed movement that has swapped FromPit and ToPit:
                SeedMovement backSeedMovement = new SeedMovement(origSeedMovement.ToPit, origSeedMovement.FromPit, origSeedMovement.NumberOfSeeds);

                // Add the new seed movement to the backward move:
                backMove.AddSeedMovement(backSeedMovement);
            }

            return backMove;
        }

        /// <summary>
        /// Adds the SeedMovements of "moveToIntegrate" in the parameter
        /// </summary>
        /// <param name="move"></param>
        /// <returns></returns>
        public void IntegrateMove(Move moveToIntegrate)
        {
            for (int index = 0; index < moveToIntegrate.GetNumberOfSeedMovements(); ++index)
            {
                AddSeedMovementToTheFront(moveToIntegrate.GetSeedMovement(index));
            }
        }

        /// <summary>
        /// Creates a string of the currently valid seed movement structure and returns this string.
        /// </summary>
        public override string ToString()
        {
            string retStr = "Move:\n";
            for (int index = 0; index < _seedMovementList.Count; ++index)
            {
                retStr += "               " + _seedMovementList[index] + "\n";
            }

            return retStr;
        }
    }
}
