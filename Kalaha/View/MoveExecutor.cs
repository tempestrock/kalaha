//
// View_MoveExecutor
//
// An object of this class actually executes the move of a seed from one pit to another.
//

using PST_Common;
using Kalaha.Model;
using Kalaha.View.Model;
using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;


namespace Kalaha.View
{

    public static class MoveExecutor
    {
        // --- Enums ---

        // --- Attributes of the class ---

        /// <summary>Asynchronous lock to check whether a storyboard has ended already</summary>
        private static readonly AsyncLock _asyncLock = new AsyncLock();

        /// <summary>
        /// An important means for the handling of asynchronous storyboards: We count the number of storyboards that are completed.
        /// Only if all storyboards of a move are completed (i.e., the counter for this move is equal to 0), the next steps are to be performed.
        /// </summary>
        private static int _storyCompleteCounter = 0;

        /// <summary>
        /// This counter is increased for each seed we move. It is used to bring the seed images on top when moving.
        /// </summary>
        private static int _seedMoveCounter = 10;


        // --- Methods of the class ---

        /// <summary>Checks whether the current move is completely visualized, i.e. all seeds have been shown.</summary>
        /// <returns>True if the current move is completely visualized</returns>
        public static bool MoveIsCompletelyVisualized()
        {
            return (_storyCompleteCounter == 0);
        }

        /// <summary>
        /// Prepares the actual move by setting the expected number of seeds to be moved.
        /// </summary>
        /// <param name="numSeeds">The number of seeds to be moved for the next move</param>
        public static void PrepareMove(int numSeeds)
        {
//DEBUG     Logging.I.LogMessage("Calling PrepareMove (" + numSeeds + ").\n");
            _storyCompleteCounter = numSeeds;
        }

        /// <summary>Visually executes the move.</summary>
        /// <param name="move">The move to perform</param>
        /// <param name="pit">The array of visual pits</param>
        /// <param name="nextMethodToCall">The method to be called once all moves have been performed</param>
        /// <param name="moveFast">Flag whether or not to move the seeds very fast</param>
        public async static void ExecuteMove(Move move, Kalaha.View.Model.Pit[] pit, NextMethodToCall nextMethodToCall, bool moveFast)
        {
            // Take the seed movement that comes next in the move:
            SeedMovement seedMovement = move.GetNextSeedMovement();

            // Remove this seed movement from the move:
            move.RemoveSeedmovement(seedMovement);

            // Get data out of the seed movement:
            Kalaha.View.Model.Pit fromPit = pit[seedMovement.FromPit];
            Kalaha.View.Model.Pit toPit = pit[seedMovement.ToPit];
            int numSeeds = seedMovement.NumberOfSeeds;

            // Prepare the move executor by telling it the total number of seeds:
            MoveExecutor.PrepareMove(numSeeds);
            
//DEBUG     Logging.I.LogMessage("Performing move with " + fromPit + ", " + toPit + ", and " + numSeeds + ".\n");

            for (int seedIndex = 0; seedIndex < numSeeds; ++seedIndex)
            {
                Seed seedToMove = fromPit.GetSomeSeedAndRemoveIt();
                Point newPlace = toPit.FindPlaceForNewSeed(seedToMove.GetWidth(), seedToMove.GetHeight());

                Storyboard story = CreateLinearMoveStory(seedToMove.GetImage(), seedToMove.GetTopLeftCorner(), newPlace, moveFast);
                
                // Set the zIndex of the seed to a high number it order to put it visually "on top" of the stack:
                seedToMove.SetZIndex(_seedMoveCounter);
                _seedMoveCounter++;

//DEBUG         Logging.I.LogMessage("ExcuteMove (fromPit = " + fromPit + ", toPit = " + toPit + ", numSeeds = " + numSeeds +
//DEBUG                              "), seedIndex = " + seedIndex + " -> Before asyncLock.\n");

                // The following line is pretty tricky: We jump out of this method if some other seed is still being visualized.
                // This causes the complete call stack to return to the original call of the GameBoardPage.ButtonClickedOnPit() method.
                // Only if one seed is fully animated, the lock is released and the next seed will be animated:
          //      using (var releaser = await _asyncLock.LockAsync())
                {
//DEBUG             Logging.I.LogMessage("ExcuteMove (fromPit = " + fromPit + ", toPit = " + toPit + ", numSeeds = " + numSeeds +
//DEBUG                                  "), seedIndex = " + seedIndex + " -> Inside asyncLock.\n");
                    await story.BeginAsync();
                    story.Stop();

                }

                // Play the sound when moving the seed:
                seedToMove.PlayMovingSound();

                // Rotate the image to the angle that it had at the time of creation of the seed:
                seedToMove.RotateImage();


//DEBUG         Logging.I.LogMessage("ExcuteMove (fromPit = " + fromPit + ", toPit = " + toPit + ", numSeeds = " + numSeeds +
//DEBUG                              "), seedIndex = " + seedIndex + " -> After asyncLock.\n");

                toPit.MoveSeedHere(seedToMove, newPlace);

                // Check if in all the asynchronous storyboards we are now done with the last storyboard for this move:
                if (MoveIsCompletelyVisualized())
                {
//DEBUG             Logging.I.LogMessage("This was the last seed to be completed.\n");

                    // This is the last seed that has been visualized. All aynchronous calls have been executed.
                    // If there is some seed movement left in the original move, we perform this now by calling the visualization method
                    // recursively. If there is nothing left, and some original caller of this method told us what to do once we are done, we do it now:
                    if (move.GetNumberOfSeedMovements() > 0)
                    {
//DEBUG                 Logging.I.LogMessage("Recursively calling the visualizer.\n");
                        ExecuteMove(move, pit, nextMethodToCall, moveFast);
                    }
                    else
                    {
                        // We are completely done with the move.
                        if (nextMethodToCall != null)
                        {
//DEBUG                     Logging.I.LogMessage("Calling the next method to call.\n");
                            nextMethodToCall();
                        }
                    }
                }
            }
        }

        /// <summary>Just a test to blow up the given element.</summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static Storyboard CreateLinearMoveStory(UIElement obj, Point from, Point to, bool moveFast)
        {
            obj.RenderTransform = new CompositeTransform();

            var xAnim = new DoubleAnimation();
            var yAnim = new DoubleAnimation();

            // In case of a "fast move", we simply take 100 milliseconds. Otherwise the time is taken from the resources:
            int millisecsForAnimation = (moveFast ? 100 : KalahaResources.I.GetLayoutValue("TimeForSeedMove"));
            xAnim.Duration = TimeSpan.FromMilliseconds(millisecsForAnimation);
            yAnim.Duration = TimeSpan.FromMilliseconds(millisecsForAnimation);

            xAnim.From = 0;
            xAnim.By = PSTScreen.I.ToScreenX(to.X - from.X);

            yAnim.From = 0;
            yAnim.By = PSTScreen.I.ToScreenY(to.Y - from.Y);

            Storyboard.SetTarget(xAnim, obj);
            Storyboard.SetTarget(yAnim, obj);
            Storyboard.SetTargetProperty(xAnim, "(UIElement.RenderTransform).(CompositeTransform.TranslateX)");
            Storyboard.SetTargetProperty(yAnim, "(UIElement.RenderTransform).(CompositeTransform.TranslateY)");

            // Create the actual storyboard:
            var story = new Storyboard();

            // Add the animation to the storyboard:
            story.Children.Add(xAnim);
            story.Children.Add(yAnim);

            // Add flapping to the animation if requested:
            bool addFlappingToAnimation = ((!moveFast) && (KalahaResources.I.GetLayoutRes("UseFlappingInSeedAnimation") == "true"));
            if (addFlappingToAnimation)
            {
                var xAnimFlapping = new DoubleAnimation();
                xAnimFlapping.Duration = TimeSpan.FromMilliseconds(millisecsForAnimation / 20);
                xAnimFlapping.AutoReverse = true;
                xAnimFlapping.RepeatBehavior = new RepeatBehavior(10);  // This is the actual flapping
                xAnimFlapping.To = 0.1;
                Storyboard.SetTarget(xAnimFlapping, obj);
                Storyboard.SetTargetProperty(xAnimFlapping, "(UIElement.RenderTransform).(CompositeTransform.ScaleX)");

                story.Children.Add(xAnimFlapping);
            }

            return story;
        }

        private static Task BeginAsync(this Storyboard story)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            if (story == null)
            {
                tcs.SetException(new ArgumentNullException());
            }
            else
            {
                EventHandler<object> onComplete = null;
                onComplete = (s, e) =>
                {
                    story.Completed -= onComplete;

                    // Subtract the counter of completed Storyboards:
                    _storyCompleteCounter--;
//DEBUG             Logging.Inst.LogMessage("Storyboard completed. Number of open Storyboards left: " + _storyCompleteCounter + ".\n");

                    tcs.SetResult(true);
                };
                story.Completed += onComplete;
                story.Begin();
            }
            return tcs.Task;
        }
    }
}
