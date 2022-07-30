// This class provides a handler for Ending a callout.
// * If the player has set LeaveCalloutsRunning to true, the callout will continue running after it completes until the player manually ends it.
// * If not, the callout waits two seconds and then ends automatically.
// * If the callout is forced end before it actually completes, CalloutForcedEnd will be set to true, indicating the possibility of a crash due to being forced end.

using Rage;

namespace YobbinCallouts
{
    class EndCalloutHandler
    {
        public static bool CalloutForcedEnd = false;
        public static void EndCallout()
        {
            if (Config.LeaveCalloutsRunning && !CalloutForcedEnd)
            {
                GameFiber.Wait(2000);
                Game.DisplayHelp("Press ~y~"+ Config.CalloutEndKey + " ~w~to ~b~Finish~w~ the Callout.");
                Game.LogTrivial("YOBBINCALLOUTS: ENDCALLOUTHANDLER - Player Will Manually End the Callout");
                while (!Game.IsKeyDown(Config.CalloutEndKey)) GameFiber.Wait(0);
                //End the Callout
            }
            else
            {
                if (!CalloutForcedEnd) GameFiber.Wait(2000);
                else Game.LogTrivial("YOBBINCALLOUTS: Callout Was Ended at Start, May Cause Issues!");
                Game.LogTrivial("YOBBINCALLOUTS: ENDCALLOUTHANDLER - Ending Callout Immediately");
                //End the Callout
            }
            CalloutForcedEnd = false;
        }
    }
}
