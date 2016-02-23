//
// UndoManager
//
// The UndoManager stores the path of moves of the players in order to allow a complete "undo path" back to the beginning of the game.
// We directly store "backward moves" of the original moves in order to make it simple to perform an undo. 
//

using PST_Common;
using System.Collections.Generic;


namespace Kalaha.Model
{
    public class UndoManager
    {
        // --- Attributes of the class ---

        /// <summary>
        /// The list of moves. These are actually "backward moves" to the original moves.
        /// A "backward move" of a move has all the seed movements in the converse order, and each seed movement has swapped "FromPit" and "ToPit".
        /// </summary>
        private List<Move> _moveList;

        /// <summary>
        /// For each move in the _moveList, the player whose turn it is after the respective move has been made. These are actually references to the
        /// two player instances in the game.
        /// </summary>
        private List<Player> _playerList;


        // --- Methods of the class ---

        /// <summary>Default constructor</summary>
        public UndoManager()
        {
            Inititalize();
        }

        /// <summary>
        /// Initializes the UndoManager
        /// </summary>
        public void Inititalize()
        {
            _moveList = new List<Move>();
            _playerList = new List<Player>();
        }

        /// <summary>
        /// Remembers the given move as a "normal" by the given player.
        /// </summary>
        /// <param name="move">The move to remember</param>
        /// <param name="currentPlayer">The player who made the move</param>
        public void RememberNormalNove(Move move, Player currentPlayer)
        {
            // A "normal" move is quite easy to remember: We store a new list entry with the move's "backward move".

            // Get the move's backward move:
            Move backMove = move.GetBackwardMove();

            // Add the backward move and the current player at the beginnings of the respecive lists:
            _moveList.Insert(0, backMove);
            _playerList.Insert(0, currentPlayer);
        }

        /// <summary>
        /// Remembers the given move as a move that has to be added to the last "normal" move of the same player.
        /// This is supposed to be used for capture moves or "final" moves.
        /// </summary>
        /// <param name="move">The move to remember</param>
        /// <param name="currentPlayer">The player who made the move</param>
        public void RememberFollowUpMove(Move move, Player currentPlayer)
        {
            // We need an existing player list:
            if (_playerList.Count == 0)
            {
                throw new PSTException("UndoManager.RememberFollowUpMove: player list is empty.");
            }

            // We need an existing move list:
            if (_moveList.Count == 0)
            {
                throw new PSTException("UndoManager.RememberFollowUpMove: move list is empty.");
            }

            // We can only add a follow-up move for the same player as exists as the first entry in the list of players:
            if (_playerList[0] != currentPlayer)
            {
                throw new PSTException("UndoManager.RememberFollowUpMove: Unexpected player: " + currentPlayer + ". Expected " + _playerList[0] + ".");
            }
            
            // Get the move's backward move:
            Move backMove = move.GetBackwardMove();

            // Integrate the backward move into the move that is the first in the already existing list of "undo moves":
            _moveList[0].IntegrateMove(backMove);
        }

        /// <summary>
        /// Returns true if there is an undo move left.
        /// </summary>
        public bool HasAnUndoMoveLeft()
        {
//DEBUG     Logging.I.LogMessage("Calling UndoManager.HasAnUndoMoveLeft().\n");
//DEBUG     Logging.I.LogMessage(ToString());

            return (_moveList.Count > 0);
        }

        /// <summary>
        /// Returns the next undo move to be taken together with the player whose turn it is after that move.
        /// The undo move is removed from the internal list of the UndoManager.
        /// </summary>
        /// <param name="undoMove">The move to undo the last move that one of the two players made.</param>
        /// <param name="nextPlayer">The player whose turn it is after the undoMove</param>
        public void GetNextUndoMove(ref Move undoMove, ref Player nextPlayer)
        {
            if (_moveList.Count == 0)
            {
                throw new PSTException("UndoManager.GetNextUndoMove: No move left.");
            }

            // Store the respective first elements of the internal lists in the reference parameters:
            undoMove = _moveList[0];
            nextPlayer = _playerList[0];

            // Remove both entries from the lists:
            _moveList.RemoveAt(0);
            _playerList.RemoveAt(0);
        }

        /// <summary>
        /// Creates a string of the currently valid undo move structure and returns this string.
        /// </summary>
        public override string ToString()
        {
            string retStr = "Undo moves:\n";

            for (int index = 0; index < _moveList.Count; ++index)
            {
                retStr += ((index+1) + ". " + _moveList[index].ToString());
                retStr += "               Player " + _playerList[index].ToString() + " after this move.\n";
            }

            return retStr;
        }
    }
}
