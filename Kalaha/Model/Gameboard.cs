//
// Gameboard
//
// The Gameboard is part of the model and stores the information about the number of Pit
// and the pits themselves.
// The model does not know anything about the view.
//

using PST_Common;

namespace Kalaha.Model
{

    public class GameBoard
    {

        // --- Attributes of the class GameBoard ---

        /// <summary>The number of houses per player</summary>
        private int _numHousesPerPlayer;

        /// <summary>The number of pits in total, including the two Kalahs</summary>
        private int _numPitsInTotal;

        /// <summary>
        /// The array of all pits in the game. In order to iterate easily over all pits, this is one array for the
        /// whole gameboard. The size of the array is (_numPits+1)*2 because each player has their pits plus one Kalah
        /// and there are two players.
        /// The meaning behind the pits is the following:
        ///    indexes 0 through _numPits-1 represent the pits of the first player
        ///    index _numPits represents the Kalah of the first player
        ///    indexes _numPits+1 through (_numPits*2) represent the pits of the second player
        ///    index (_numPits*2)+1 represents the Kalah of the second player
        /// </summary>
        private Pit[] _pit;

        /// <summary>The initial number of seeds per pit (currently always set to the number of pits).</summary>
        private int _numInitialSeeds;


        // --- Public methods of the class Pit ---

        /// <summary>The default constructor sets everything to 0.</summary>
        public GameBoard()
        {
            _numHousesPerPlayer = 0;
            _numPitsInTotal = 0;
            _pit = null;
            _numInitialSeeds = 0;
        }

        /// <summary>
        /// Constructor with a direct assignment of sizes. The internal arrays are not allocated by this constructor.
        /// </summary>
        /// <param name="numHousesPerPlayer">The number of houses per player</param>
        /// <param name="numInitialSeeds">The number of seeds initially stored in each pit</param>
        private GameBoard(int numHousesPerPlayer, int numInitialSeeds)
        {
            // Initialize the private member that stores the number of pits:
            _numHousesPerPlayer = numHousesPerPlayer;

            // For each player the given number of pits is reserved plus the two Kalahs:
            _numPitsInTotal = (_numHousesPerPlayer + 1) * 2;

            // Set the initial number of seeds:
            _numInitialSeeds = numInitialSeeds;

            // Create the array of pits which quite frankly defines the gameboard.
            _pit = new Pit[_numPitsInTotal];
        }

        /// <summary>Sets the size of the game board in terms of pits per player and initial number of seeds.
        /// Allocates the internal structures accordingly.</summary>
        /// <param name="numHousesPerPlayer">The number of houses per player</param>
        /// <param name="numInitialSeeds">The number of seeds initially stored in each pit</param>
        public void SetSize(int numHousesPerPlayer, int numInitialSeeds)
        {
            // Initialize the private member that stores the number of pits:
            _numHousesPerPlayer = numHousesPerPlayer;

            // For each player the given number of pits is reserved plus the two Kalahs:
            _numPitsInTotal = (_numHousesPerPlayer + 1) * 2;

            // Set the initial number of seeds in a house:
            _numInitialSeeds = numInitialSeeds;

            // Create the array of pits which quite frankly defines the gameboard.
            _pit = new Pit[_numPitsInTotal];
            for (int index = 0; index < _numPitsInTotal; ++index)
            {
                if ((index == _numHousesPerPlayer) || (index == (_numPitsInTotal - 1)))
                {
                    // This is the index of a Kalah --> initially 0 seeds:
                    _pit[index] = new Pit(0);
                }
                else
                {
                    // This is the index of a pit --> initially n seeds:
                    _pit[index] = new Pit(_numInitialSeeds);
                }
            }
        }


        /// <returns>A complete copy of the Gameboard</returns>
        public GameBoard GetACopy()
        {
            GameBoard theCopy = new GameBoard(_numHousesPerPlayer, _numInitialSeeds);

            for (int index = 0; index < _numPitsInTotal; ++index)
            {
                theCopy._pit[index] = new Pit(0);
                theCopy._pit[index].AddSeeds(_pit[index].GetNumberofSeeds());
            }

            return theCopy;
        }


        /// <returns>The pit with the given number of the given player</returns>
        /// <param name="player">The player whose pit is asked for</param>
        /// <param name="pitIndex">The index of the pit whose internal index is to be returned</param>
        private int GetInternalPitIndex(Player player, int pitIndex)
        {
            if ((pitIndex < 0) || (pitIndex >= _numHousesPerPlayer))
            {
                throw new PSTException("GameBoard.GetInternalPitIndex: pitIndex out of range: " + pitIndex);
            }
            return (pitIndex + (player.GetId() * (_numHousesPerPlayer + 1)));
        }

        /// <returns>The pit that lies on the opposite side of the given pit number and the given player</returns>
        /// <param name="player">The player whose pit is asked for</param>
        /// <param name="houseIndex">The index of the house whose opposite house index is to be returned</param>
        private int GetInternalIndexOfOppositeHouse(Player player, int houseIndex)
        {
            if ((houseIndex < 0) || (houseIndex >= _numHousesPerPlayer))
            {
                throw new PSTException("GameBoard.GetInternalIndexOfOppositePit: houseIndex out of range: " + houseIndex);
            }
            return (2 * _numHousesPerPlayer - (houseIndex + (player.GetId() * (_numHousesPerPlayer + 1))));
        }

        /// <returns>The internal index of the Kalah for the given player</returns>
        /// <param name="player">The player whose Kalah's internal index is asked for</param>
        private int GetInternalIndexOfKalah(Player player)
        {
            return (_numHousesPerPlayer + (player.GetId() * (_numHousesPerPlayer + 1)));
        }

        /// <returns>The pit with the given number of the given player</returns>
        /// <param name="player">The player whose pit is asked for</param>
        /// <param name="pitIndex">The index of the pit to be returned</param>
        /// <exception cref="PSTException">Index out of range</exception>
        public Pit GetPitOfPlayer(Player player, int pitIndex)
        {
            return _pit[GetInternalPitIndex(player, pitIndex)];
        }

        /// <summary>
        /// Returns the pit with the given internal number.
        /// </summary>
        /// <param name="pitIndex">A number between 0 and (_numHousesPerPlayer*2)+1</param>
        public Pit GetPit(int pitIndex)
        {
            return _pit[pitIndex];
        }

        /// <returns>The house which is located to the opposite of the given pit</returns>
        /// <param name="player">The player whose house is given</param>
        /// <param name="houseIndex">The index of the house whose opposite house is to be returned</param>
        /// <exception cref="PSTException">Index out of range</exception>
        public Pit GetOppositeHouse(Player player, int houseIndex)
        {
            return _pit[GetInternalIndexOfOppositeHouse(player, houseIndex)];
        }

        /// <returns>The Kalah of the given player</returns>
        /// <param name="player">The player whose Kalah is asked for.</param>
        public Pit GetKalahOfPlayer(Player player)
        {
            return _pit[GetInternalIndexOfKalah(player)];
        }

        /// <returns>The Kalah of the given player's opponent</returns>
        /// <param name="player">The player whose opponent's Kalah is asked for.</param>
        public Pit GetKalahOfOpponent(Player player)
        {
            return _pit[_numHousesPerPlayer + (((player.GetId()+1)%2) * (_numHousesPerPlayer + 1))];
        }

        /// <returns>The number of pits per player</returns>
        public int GetNumOfHousesPerPlayer()
        {
            return _numHousesPerPlayer;
        }

        /// <returns>The intial number of seeds</returns>
        public int GetInitialNumOfSeeds()
        {
            return _numInitialSeeds;
        }

        /// <returns>True if the given pit is one of the player's pits</returns>
        /// <param name="player">The player whose pit is to be checked</param>
        /// <param name="pitIndex">The pit's index which can have a range from 0 to _numPitsInTotal-1, i.e. the complete array</param>
        public bool IsPlayersOwnPit(Player player, int pitIndex)
        {
            if ((pitIndex < 0) || (pitIndex >= _numPitsInTotal))
            {
                throw new PSTException("PlayerOwnsPit: pitIndex out of range: " + pitIndex);
            }

            return ((pitIndex >= (player.GetId() * (_numHousesPerPlayer + 1))) &&
                    (pitIndex < ((player.GetId() * (_numHousesPerPlayer + 1)) + _numHousesPerPlayer)));
        }

        /// <returns>True if the given player has at least one house that still contains seeds</returns>
        /// <param name="player">The player whose houses are to be checked</param>
        public bool PlayerStillHasFullHouses(Player player)
        {
            int playersId = player.GetId();
            for (int pitIndex = 0; pitIndex < _numHousesPerPlayer; ++pitIndex)
            {
                if (_pit[pitIndex + (playersId * (_numHousesPerPlayer + 1))].GetNumberofSeeds() > 0)
                {
                    return true;
                }
            }

            // We walked through all pits and found no filled one:
            return false;
        }

        /// <returns>True if the given player has no only empty houses left</returns>
        /// <param name="player">The player whose houses are to be checked</param>
        public bool PlayerOnlyHasEmptyHouses(Player player)
        {
            return (!PlayerStillHasFullHouses(player));
        }

        /// <summary>Selects the given house and distributes the seeds of that house among
        /// the following pits, one seed per pit. Takes into account the direction of sowing, depending on the rules.</summary>
        /// <param name="player">The player whose pit is to be moved</param>
        /// <param name="houseIndexToBeMoved">The index of the house to be moved</param>
        /// <param name="lastSeedFellInOwnKalah">A reference parameter which gets the information whether the
        /// <param name="lastSeedFellIntoEmptyOwnHouse">A reference parameter which gets the information whether the
        /// last seed fell into empty pit that is owned by the player.</param>
        /// <param name="lastSeedsHouse">If "lastSeedFellIntoEmptyOwnHouse" is true, then this reference parameter
        /// gets the index of the house that the last seed fell into. Otherwise "lastSeedsHouse" gets the value -1 (for "undefined").
        /// <exception cref="KalahaException">Index out of range</exception>
        public Move MoveHouse(Player player, int houseIndexToBeMoved,
                                ref bool lastSeedFellIntoOwnKalah,
                                ref bool lastSeedFellIntoEmptyOwnHouse,
                                ref int lastSeedsHouse,
                                bool returnMove)
        {
            if ((houseIndexToBeMoved < 0) || (houseIndexToBeMoved >= _numHousesPerPlayer))
            {
                throw new PSTException("MovePit: houseIndex out of range: " + houseIndexToBeMoved);
            }
            
            lastSeedFellIntoOwnKalah = false;
            lastSeedFellIntoEmptyOwnHouse = false;
            lastSeedsHouse = -1;

            // Create a move object if this is requested by the caller of the method:
            Move move = null;
            if (returnMove)
            {
                move = new Move();
            }

            // Calculate the array index of the house from which we take the seeds out:
            int internalHouseIndex = houseIndexToBeMoved + (player.GetId() * (_numHousesPerPlayer + 1));

            // Keep the number of seeds to be distributed in mind:
            int numOfSeeds = _pit[internalHouseIndex].GetNumberofSeeds();

            // The distribution of seeds is done in a fast way, using the internal array structure.
            // Find out about the sowing direction: It is clockwise if the rule is simply defined that way or if the rule
            // is set to "Cross-Kalah" and the number of seeds is odd:
            bool sowingDirectionIsClockwise = ((Rules.I.GetDirectionOfSowing() == Rules.DirectionOfSowing.Clockwise) ||
                                               ((Rules.I.GetDirectionOfSowing() == Rules.DirectionOfSowing.CrossKalah) && ((numOfSeeds % 2) == 1)));

            // Empty the pit we start from:
            _pit[internalHouseIndex].RemoveAllSeeds();

            // Distribute the seeds:
            int maybeOneMore = 0;
            Pit kalahOfOpponent = GetKalahOfOpponent(player);
            int loopIndex = 0;
            for (; loopIndex < numOfSeeds+maybeOneMore; ++loopIndex)
            {
                // Depending on the sowing direction, we set the index of the pit that gets a seed accordingly:
                int pitIndex = -1;
                if (sowingDirectionIsClockwise)
                {
                    pitIndex = (internalHouseIndex + 10*_numPitsInTotal - loopIndex - 1) % _numPitsInTotal;
                }
                else
                {
                    pitIndex = (internalHouseIndex + 1 + loopIndex) % _numPitsInTotal;
                }

                if (_pit[pitIndex] != kalahOfOpponent)
                {
                    // This is not the Kalah of the opponent --> Add a seed:
                    _pit[pitIndex].AddASeed();
                    if (returnMove)
                    {
                        // Store this step in the move:
                        move.AddSeedMovement(new SeedMovement(internalHouseIndex, pitIndex, 1));
                    }
                }
                else
                {
                    // This is the opponent's Kalah --> Do not add a seed but instead increase the loop by 1:
                    ++maybeOneMore;
                }
            }

            // Calculate which pit the last seed fell into, depending on the sowing direction:
            int pitOfLastSeed = -1;
            if (sowingDirectionIsClockwise)
            {
                pitOfLastSeed = (internalHouseIndex + 10*_numPitsInTotal - loopIndex) % _numPitsInTotal;
            }
            else
            {
                pitOfLastSeed = (internalHouseIndex + loopIndex) % _numPitsInTotal;
            }

            lastSeedFellIntoOwnKalah = (_pit[pitOfLastSeed] == GetKalahOfPlayer(player));

            if (!lastSeedFellIntoOwnKalah)
            {
                // Check whether the last seed fell into an empty own pit:
                if ((_pit[pitOfLastSeed].GetNumberofSeeds() == 1) && IsPlayersOwnPit(player, pitOfLastSeed))
                {
                    // Yes, it did.
                    lastSeedFellIntoEmptyOwnHouse = true;

                    // We have to calculate the player's pit from the internal pit:
                    lastSeedsHouse = pitOfLastSeed % (_numHousesPerPlayer + 1);
                }
            }

            return move;
        }

        /// <summary>
        /// The given player wins his own pit (given by "ownPitIndex") and the opposite pit. Both pit's contents move into
        /// the player's Kalah.</summary>
        /// <param name="player">The player who wins the pit's contents</param>
        /// <param name="ownPitIndex">The index of the player's pit</param>
        /// <param name="captureOpponentsHouse">A flag whether the seeds in the opponent's house may be captured, too</param>
        /// <param name="returnMove">A flag whether or not to return the move in detail</param>
        /// <returns>The details of this move</returns>
        public Move PlayerWinsHouses(Player player, int ownPitIndex, bool captureOpponentsHouse, bool returnMove)
        {
            Pit ownPit = GetPitOfPlayer(player, ownPitIndex);
            Pit kalahToBeFilled = GetKalahOfPlayer(player);
            int internalKalahIndex = -1;  // Only used when generating a Move
            Move move = null;

            if (returnMove)
            {
                move = new Move();
                internalKalahIndex = GetInternalIndexOfKalah(player);
            }

            if (captureOpponentsHouse)
            {
                // Also the opponent's house shall be captured.
                Pit opponentsPit = GetOppositeHouse(player, ownPitIndex);
                if (returnMove)
                {
                    // Move the opponent's seeds to the player's Kalah:
                    move.AddSeedMovement(new SeedMovement(GetInternalIndexOfOppositeHouse(player, ownPitIndex), internalKalahIndex,
                                         opponentsPit.GetNumberofSeeds()));
                }

                kalahToBeFilled.AddSeeds(opponentsPit.GetNumberofSeeds());
                opponentsPit.RemoveAllSeeds();
            }

            if (returnMove)
            {
                // Move the one seed from the own house into the player's Kalah:
                move.AddSeedMovement(new SeedMovement(GetInternalPitIndex(player, ownPitIndex), internalKalahIndex, 1));
            }

            // Fill the own Kalah with the own seed and remove the own seed from the house:
            kalahToBeFilled.AddSeeds(1);
            ownPit.RemoveAllSeeds();

            return move;
        }

        /// <summary>Moves all seeds into the respective Kalahs.</summary>
        public Move CollectFinalSeeds(Player firstPlayer, Player secondPlayer)
        {
            Move move = new Move();
            Player currentPlayer = firstPlayer;
            for (int playerIndex = 0; playerIndex < 2; ++playerIndex)
            {
                Pit currentPlayersKalah = GetKalahOfPlayer(currentPlayer);
                int internalIndexOfKalah = GetInternalIndexOfKalah(currentPlayer);

                for (int pitIndex = 0; pitIndex < _numHousesPerPlayer; ++pitIndex)
                {
                    Pit currentPit = GetPitOfPlayer(currentPlayer, pitIndex);

                    if (currentPit.GetNumberofSeeds() > 0)
                    {
                        int currentPitsInternalIndex = GetInternalPitIndex(currentPlayer, pitIndex);
                        move.AddSeedMovement(new SeedMovement(currentPitsInternalIndex, internalIndexOfKalah, currentPit.GetNumberofSeeds()));

                        currentPlayersKalah.AddSeeds(currentPit.GetNumberofSeeds());
                        currentPit.RemoveAllSeeds();
                    }
                }
                currentPlayer = secondPlayer;
            }

            return move;
        }

        /// <summary>
        /// Performs the move given in the parameter.
        /// </summary>
        /// <param name="move">The move to perform</param>
        public void PerformMove(Move move)
        {
//DEBUG     Logging.I.LogMessage("Entering GameBoard.PerformMove().\n");
            // Go through all seed movements of the move and add and remove seeds of the respective pits:
            for (int index = 0; index < move.GetNumberOfSeedMovements(); ++index)
            {
                SeedMovement seedMovement = move.GetSeedMovement(index);
//DEBUG         Logging.I.LogMessage("             Removing " + seedMovement.NumberOfSeeds + " seeds from pit " + seedMovement.FromPit +
//DEBUG                              " and adding them to pit " + seedMovement.ToPit + ".\n");

                _pit[seedMovement.FromPit].RemoveSeeds(seedMovement.NumberOfSeeds);
                _pit[seedMovement.ToPit].AddSeeds(seedMovement.NumberOfSeeds);
            }
        }

        /// <param name="player"></param>
        /// <returns>The sum of all seeds that the given player currently has in their pits</returns>
        public int SumOfAllSeedsOfPlayer(Player player)
        {
            int sum = 0;

            for (int pitIndex = 0; pitIndex < _numHousesPerPlayer; ++pitIndex)
            {
                Pit currentPit = GetPitOfPlayer(player, pitIndex);
                sum += currentPit.GetNumberofSeeds();
            }

            return sum;
        }

        /// <summary>Logs the board contents for debug purposes.</summary>
        public void LogBoard(Player firstPlayer, Player secondPlayer, Logging.LogLevel logLevel)
        {
            if ((Logging.I.GetLogLevel() & logLevel) == Logging.LogLevel.Off)
            {
                // Small optimization to leave as soon as possible if logging is not active anyway.
                return;
            }

            if (firstPlayer == secondPlayer)
            {
                Logging.I.LogMessage("LogBoard: first and second player are the same: " + firstPlayer + ".\n");
                return;
            }

            // We find out who of the players is south and who is north;
            Player south = null;
            Player north = null;
            if (firstPlayer.GetPosition() == Player.Position.South)
            {
                south = firstPlayer;
                north = secondPlayer;
            }
            else
            {
                south = secondPlayer;
                north = firstPlayer;
            }

            // Now we start building up the log string:
            string logString = ("Game board:\n                ");

            for (int pitIndex = _numHousesPerPlayer - 1; pitIndex >= 0; --pitIndex)
            {
                int numberOfSeeds = GetPitOfPlayer(north, pitIndex).GetNumberofSeeds();
                if (numberOfSeeds < 10) {
                    logString += " ";
                }
                logString += numberOfSeeds + " ";
            }

            logString += "\n              " + GetKalahOfPlayer(north).GetNumberofSeeds();
            for (int index = 0; index < ((3*_numHousesPerPlayer)+3); ++index)
            {
                logString += " ";
            }
            logString += (GetKalahOfPlayer(south).GetNumberofSeeds() + "\n                ");

            for (int pitIndex = 0; pitIndex < _numHousesPerPlayer; ++pitIndex)
            {
                int numberOfSeeds = GetPitOfPlayer(south, pitIndex).GetNumberofSeeds();
                if (numberOfSeeds < 10) {
                    logString += " ";
                }
                logString += numberOfSeeds + " ";
            }

            Logging.I.LogMessage(logString + "\n", logLevel);
        }
    } // class GameBoard

} // namespace Kalaha
