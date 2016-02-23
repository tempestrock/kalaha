//
// Pit
//
// A Pit is part of the model and represents one of the pits on the gameboard. It is a very simple class, mainly storing
// the information about how many seeds are currently stored in the pit.
//


namespace Kalaha.Model
{
    public class Pit
    {
        // --- Attributes of the class Pit ---

        /// <summary>The number of seeds in the pit.</summary>
        private int _numSeeds;


        // --- Public methods of the class Pit ---
        /*
        /// <summary>The default constructor, setting the number of seeds to 0</summary>
        public Pit()
        {
            _numSeeds = 0;
        }
        */

        /// <summary>A constructor with the number of seeds, setting the initial number of seeds</summary>
        /// <param name="numSeeds">The initial number of seeds in the pit.</param>
        public Pit(int numSeeds)
        {
            _numSeeds = numSeeds;
        }

        /// <returns>The number of seeds currently stored in the pit</returns>
        public int GetNumberofSeeds()
        {
            return _numSeeds;
        }

        /// <summary>Adds a seed to the pit</summary>
        public void AddASeed()
        {
            ++_numSeeds;
        }

        /// <summary>Adds a number of seeds to the pit</summary>
        /// <param name="additionalNumSeeds">The number of seeds to be added to the pit.</param>
        public void AddSeeds(int additionalNumSeeds)
        {
            _numSeeds += additionalNumSeeds;
        }

        /// <summary>Removes a seed from the pit.</summary>
        /// <exception cref="PSTException">Trying to remove a seed from an empty pit</exception>
        public void RemoveASeed()
        {
            if (_numSeeds <= 0)
            {
                throw new PSTException("RemoveASeed: Trying to remove a seed from an empty pit");
            }

            --_numSeeds;
        }

        /// <summary>
        /// Removes a given number of seeds.
        /// </summary>
        /// <param name="numOfSeedsToRemove">The number of seeds to remove</param>
        public void RemoveSeeds(int numOfSeedsToRemove)
        {
            if (_numSeeds < numOfSeedsToRemove)
            {
                throw new PSTException("Pit.RemoveSeeds: Number of seeds (" + _numSeeds + ") is less than number of seeds to remove (" + numOfSeedsToRemove + ").");
            }

            _numSeeds -= numOfSeedsToRemove;
        }

        /// <summary>Removes all seeds from the pit.</summary>
        public void RemoveAllSeeds()
        {
            _numSeeds = 0;
        }

        /// <returns>True if there is no seed in the pit.</returns>
        public bool IsEmpty()
        {
            return (_numSeeds == 0);
        }

        /// <returns>True if there is at least one seed in the pit</returns>
        public bool ContainsASeed()
        {
            return (_numSeeds > 0);
        }

    } // class Pit

} // namespace Kalaha
