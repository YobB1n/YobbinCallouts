using System.Drawing;
using Rage;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage.Native;

namespace YobbinCallouts
{
    class InvestigationHandler
    {
        public static bool StartInvestigation = false;
        public static bool InvestigationRunning = false;

        public static Vector3 InvestigationLocation;

        public static Blip InvestigationBlip;

        public static string InvestigationName;

        public static Ped player = Game.LocalPlayer.Character;

        public static void OnBeforeInvestigationStarted()
        {
            //InvestigationLoaded = true;
            GameFiber.StartNew(delegate
            {
                while (true)
                {
                    while (Functions.IsCalloutRunning()) GameFiber.Yield();

                    if (StartInvestigation)
                    {
                        Game.LogTrivial("==========YOBBINCALLOUTS: " + InvestigationName + " Investigation Started==========");
                        Game.LogTrivial("Player Should Go to Investigation Start Point While not on a Call to Start the Investigation.");
                        Game.DisplayNotification("A New ~y~Investigation~w~ Has Been ~g~Started!~w~ Go to the ~b~Blip~w~ on the Map!");
                        InvestigationBlip = new Blip(InvestigationLocation, 15);
                        InvestigationBlip.Color = Color.LightBlue;  //add blipsrpite after
                        InvestigationBlip.Name = InvestigationName;
                        while (!Game.IsKeyDown(Config.InvestigationEndKey))
                        {
                            if (player.DistanceTo(InvestigationLocation) <= 15f)
                            {
                                Game.DisplayHelp("Press ~y~ "+Config.MainInteractionKey+" ~w~to Start this ~b~Investigation.");    //might repeat
                                if (Game.IsKeyDown(Config.MainInteractionKey))    //change these
                                {
                                    Game.LogTrivial("YOBBINCALLOUTS: Player Attempted to Start Investigation Within Radius.");
                                    if (!Functions.IsCalloutRunning())
                                    {
                                        Game.LogTrivial("YOBBINCALLOUTS: Player Not on Call. Starting Investigation.");
                                        {
                                            //Functions.RegisterCallout(typeof(Callouts.StolenPoliceHardwareInvestigation));
                                            try { Functions.StartCallout(InvestigationName); }
                                            catch
                                            {

                                            }
                                            Game.LogTrivial("YOBBINCALLOUTS: Player Started Investigation.");
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        Game.LogTrivial("YOBBINCALLOUTS: Player On Call, Should Come Back Once it is Finished..");
                                        Game.LogTrivial("~g~Finish~w~ your Current ~b~Callout~w~ to Start the ~y~Investigation.");
                                    }
                                }
                            }
                            GameFiber.Yield();
                        }
                        if (Game.IsKeyDown(Config.InvestigationEndKey))
                        {
                            Game.LogTrivial("YOBBINCALLOUTS: Player Did not Start the Investigation.");
                            Game.DisplayHelp("Investigation ~r~Ended.");
                            InvestigationRunning = false;
                            if (InvestigationBlip.Exists()) InvestigationBlip.Delete();
                            break;
                        }
                        else
                        {
                            if (InvestigationBlip.Exists()) InvestigationBlip.Delete();
                            break;
                        }
                    }
                    while (!Functions.IsCalloutRunning()) GameFiber.Yield();
                    //GameFiber.Yield();
                }
            }
            );
        }
        //
    }
}
