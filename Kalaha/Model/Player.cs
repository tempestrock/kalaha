//
// Player
//
// Represents a player in the game.
// Exactly two instances of the class Player may exist.
//

using PST_Common;

namespace Kalaha.Model
{
    public class Player
    {
        // --- Enums ---

        /// <summary>The player's species can be either human or computer</summary>
        public enum Species { Human = 0, Computer = 1 }

        /// <summary>The computer's strength if the player is of species computer</summary>
        public enum ComputerStrength { Easy = 0, Medium = 1, Hard = 2 }

        /// <summary>The position the player is located on the board</summary>
        public enum Position { South, North }

        // --- Attributes of the class Player ---

        // A static counter to internally count the instances of this class. We may only have 2 instances:
        private static int _instanceCounter = 0;

        /// <summary>The ID of the player (may be 0 or 1)</summary>
        private int _id;

        /// <summary>The player's species (human or computer)</summary>
        private Species _species;

        /// <summary>The player's strength if the species is Computer</summary>
        private ComputerStrength _computerStrength;

        /// <summary>The player's position (south or north)</summary>
        private Position _position;

        /// <summary>The player's name</summary>
        private string _name;

        // --- Public methods of the class Player ---

        /// <summary>The default constructor creates a new human player and gives it a new id.</summary>
        public Player(Position position)
        {
            if (_instanceCounter >= 2)
            {
                // The number of players to created exceeds 2:
                throw new PSTException("Player: Trying to create more than two players");
            }
            _id = _instanceCounter;
            ++_instanceCounter;

            // By default the player is a human:
            _species = Species.Human;

            // Set some default for the computer strength:
            _computerStrength = ComputerStrength.Medium;

            _position = position;

            // By default the player's name is taken from the resources, i.e. something like "Player #1":
            _name = KalahaResources.I.GetRes("Player", (_id + 1));
        }

        /// <returns>The ID of the player</returns>
        public int GetId()
        {
            return _id;
        }

        /// <summary>Sets the species of the player.</summary>
        /// <param name="species">The species the player shall have</param>
        /// <param name="strength">The computer's strength (with default param mainly if the species is Human)</param>
        public void SetSpecies(Species species, ComputerStrength strength = ComputerStrength.Medium)
        {
            _species = species;
//DEBUG     Logging.Inst.LogMessage(this.ToString() + ": Species set to " + _species + ".\n");

            if (_species == Species.Computer)
            {
                _computerStrength = strength;
//DEBUG         Logging.Inst.LogMessage(this.ToString() + ": Strength set to " + _computerStrength + ".\n");
            }
        }

        /// <returns>The species of the player</returns>
        public Species GetSpecies()
        {
            return _species;
        }

        public ComputerStrength GetComputerStrength()
        {
            return _computerStrength;
        }

        public Position GetPosition()
        {
            return _position;
        }

        public void SetName(string name)
        {
            _name = name;
        }

        public string GetHumansName()
        {
            return _name;
        }

        public string GetName()
        {
            string returnStr = "";
            if (_species == Species.Human)
            {
                returnStr = _name;
            }
            else
            {
                switch (_computerStrength)
                {
                    case ComputerStrength.Easy:
                        returnStr = KalahaResources.I.GetLayoutRes("ComputerEasyName") + " (" + KalahaResources.I.GetRes("Label_ComputerEasy") + ")";
                        break;
                    case ComputerStrength.Medium:
                        returnStr = KalahaResources.I.GetLayoutRes("ComputerMediumName") + " (" + KalahaResources.I.GetRes("Label_ComputerMedium") + ")";
                        break;
                    case ComputerStrength.Hard:
                        returnStr = KalahaResources.I.GetLayoutRes("ComputerHardName") + " (" + KalahaResources.I.GetRes("Label_ComputerHard") + ")";
                        break;
                }
            }
            return returnStr;
        }

        /// <summary>Overrides the standard ToString() method by returning a nice text.</summary>
        /// <returns>The respective string of the currently set player name</returns>
        public override string ToString()
        {
            return GetName();
        }

        /// <summary>This static method is used when the game is restarted.</summary>
        public static void KillAllPlayers()
        {
            _instanceCounter = 0;
        }
    } // class Player

} // namespace Kalaha

