//THIS CALLOUT IS NOT FINISHED (or even started lol)
using System.Drawing;
using Rage;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage.Native;
using System;
using System.Collections.Generic;

namespace YobbinCallouts.Callouts
{
    public class StolenMail : Callout
    {
        private Vector3 MainSpawnPoint;

        private Ped Suspect;
        private Ped HouseOwner;
        Ped player = Game.LocalPlayer.Character;
        private Blip HouseOwnerBlip;
        private Blip SuspectBlip;
        private Blip SearchArea;
        private int MainScenario;
        private bool CalloutRunning;
        List<string> HouseOwnerDialogue = new List<string>()
        {

        };
        List<string> SuspectDialogue = new List<string>()
        {

        };
        System.Windows.Forms.Keys EndKey = Config.CalloutEndKey;
        System.Windows.Forms.Keys InteractionKey = Config.MainInteractionKey;
        Random monke = new Random();

        public override bool OnBeforeCalloutDisplayed()
        {
            Game.LogTrivial("==========YOBBINCALLOUTS: Explosion Callout Start==========");
            MainScenario = monke.Next(0, 0);
            Game.LogTrivial("YOBBINCALLOUTS: Scenario Number is " + MainScenario + "");

            Game.LogTrivial("Getting Location");
            CallHandler.locationChooser(CallHandler.HouseList);
            if (CallHandler.locationReturned) { MainSpawnPoint = CallHandler.SpawnPoint; Game.LogTrivial("Spawnpoint vector is " + MainSpawnPoint); } else { Game.LogTrivial("No Location found. Ending Callout");  return false; }
            ShowCalloutAreaBlipBeforeAccepting(MainSpawnPoint, 25f);

            // Add Scanner Audio

            CalloutMessage = "Stolen Mail";
            CalloutPosition = MainSpawnPoint;
            CalloutAdvisory = "RP states that he has not gotten mail in several days. He thinks that his mail was stolen.";

            return base.OnBeforeCalloutDisplayed();
        }

        public override void OnCalloutNotAccepted()
        {
            Game.LogTrivial("Callout Not Accepted by User.");
            base.OnCalloutNotAccepted();
        }

        public override bool OnCalloutAccepted()
        {
            try
            {
                Game.LogTrivial("Callout accepted by user");

                if (Main.CalloutInterface)
                {
                    CalloutInterfaceHandler.SendCalloutDetails(this, "Code 2", "");
                }
                else
                {
                    Game.DisplayNotification("Respond ~b Code 2.");

                }
                HouseOwner = new Ped(MainSpawnPoint.Around(2));
                HouseOwner.IsPersistent = true;
                HouseOwner.BlockPermanentEvents = true;
                HouseOwnerBlip = CallHandler.AssignBlip(HouseOwner, Color.Blue, .69f, "Caller", true);
                NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(HouseOwner, player, -1);

                Game.DisplayHelp("Speak with RP.");
            }
            catch (Exception e)
            {
                Game.LogTrivial("==========YOBBINCALLOUTS: ERROR CAUGHT ON CALLOUT INTIALIZATION==========");
                Game.LogTrivial("IN: " + this);
                string error = e.ToString();
                Game.LogTrivial("ERROR: " + error);
                Game.DisplayNotification("There was an ~r~Error~w~ Caught with ~b~YobbinCallouts. ~w~Please Chck Your ~g~Log File.~w~ Sorry for the Inconvenience!");
                Game.DisplayNotification("Error: ~r~" + error);
                Game.LogTrivial("If You Believe this is a Bug, Please Report it on my Discord Server. Thanks!");
                Game.LogTrivial("==========YOBBINCALLOUTS: ERROR CAUGHT ON CALLOUT INTIALIZATION==========");
            }
            Callout();


            return base.OnCalloutAccepted();
        }

        private void Callout()
        {
            CalloutRunning = true;
            GameFiber.StartNew(delegate
            {
                try
                {
                    while (CalloutRunning)
                    {
                        while(Vector3.Distance(player.Position, HouseOwner.Position) >= 25f && !Game.IsKeyDown(EndKey)) { GameFiber.Wait(0); }
                        if (Game.IsKeyDown(EndKey)) { break; }
                        CallHandler.IdleAction(HouseOwner, false);
                        while (Vector3.Distance(player.Position, HouseOwner.Position) >= 7.5f) { GameFiber.Wait(0); }
                        Game.DisplaySubtitle("Hello Sir. Did you call about your mail being stolen.");
                        HouseOwner.Tasks.AchieveHeading(player.Heading - 180f).WaitForCompletion(500);
                        if (Config.DisplayHelp) Game.DisplayHelp("Press ~y~" + Config.MainInteractionKey + "~w~ to speak with the ~b~Landlord.");

                        CallHandler.Dialogue(HouseOwnerDialogue, HouseOwner);



                    }
                }
                catch (Exception e)
                {
                    if (CalloutRunning)
                    {
                        Game.LogTrivial("==========YOBBINCALLOUTS: ERROR CAUGHT==========");
                        Game.LogTrivial("IN: " + this);
                        string error = e.ToString();
                        Game.LogTrivial("ERROR: " + error);
                        Game.DisplayNotification("There was an ~r~Error~w~ Caught with ~b~YobbinCallouts. ~w~Please Chck Your ~g~Log File.~w~ Sorry for the Inconvenience!");
                        Game.DisplayNotification("Error: ~r~" + error);
                        Game.LogTrivial("If You Believe this is a Bug, Please Report it on my Discord Server. Thanks!");
                        Game.LogTrivial("==========YOBBINCALLOUTS: ERROR CAUGHT==========");
                    }
                    else
                    {
                        Game.LogTrivial("==========YOBBINCALLOUTS: ERROR CAUGHT - CALLOUT NO LONGER RUNNING==========");
                        string error = e.ToString();
                        Game.LogTrivial("ERROR: " + error);
                        Game.LogTrivial("No Need to Report This Error if it Did not Result in an LSPDFR Crash.");
                        Game.LogTrivial("==========YOBBINCALLOUTS: ERROR CAUGHT - CALLOUT NO LONGER RUNNING==========");
                    }
                    End();
                }
            });
        }

        private void FindSuspect()
        {
            if (CalloutRunning)
            {
                while (!player.IsInAnyVehicle(false)) GameFiber.Wait(0);
                Vector3 SuspectSpawn = World.GetNextPositionOnStreet(player.Position.Around(550));
                Suspect = new Ped(SuspectSpawn, 69);
                SuspectBlip = CallHandler.AssignBlip(Suspect, Color.Red, .69f, "Tenant");
                Suspect.IsPersistent = true;
                Suspect.BlockPermanentEvents = true;
                Suspect.Tasks.Wander();
                while (player.DistanceTo(Suspect) >= 5) GameFiber.Wait(0);
                Game.DisplaySubtitle("~g~You:~w~ Hey, Could I Speak With You for a Sec?", 3000);
                if()
                GameFiber.Wait(3000);
                Suspect.Tasks.AchieveHeading(player.Heading - 180).WaitForCompletion(500);
                if (Config.DisplayHelp) Game.DisplayHelp("Press ~y~" + Config.MainInteractionKey + " ~w~to talk to the ~r~Suspect.");
                if (CallHandler.FiftyFifty()) { CallHandler.Dialogue(SuspectDialogue, Suspect); }
                else
                {
                    if (CallHandler.FiftyFifty()) { Runs();} else { Shoots(); }
                }
            }
        }

        private void Runs()
        {
            if (CalloutRunning)
            {
                Functions.PlayScannerAudio("CRIME_SUSPECT_ON_THE_RUN_01");
                LHandle SuspectPursuit = Functions.CreatePursuit();
                Suspect.Inventory.Weapons.Clear();  
                Functions.SetPursuitIsActiveForPlayer(SuspectPursuit, true);
                Functions.AddPedToPursuit(SuspectPursuit, Suspect);
                while (Functions.IsPursuitStillRunning(SuspectPursuit)) GameFiber.Wait(0);
                while (Suspect.Exists())
                {
                    GameFiber.Yield();
                    if (!Suspect.Exists() || Suspect.IsDead || Functions.IsPedArrested(Suspect))
                    {
                        break;
                    }
                }
                if (Suspect.Exists())
                {
                    if (Functions.IsPedArrested(Suspect)) { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect is Under ~g~Arrest~w~ Following the Pursuit."); }
                }
                if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }
                GameFiber.Wait(2000);
                Functions.PlayScannerAudio("REPORT_RESPONSE_COPY_02");
                GameFiber.Wait(2000);
                if (SuspectBlip.Exists()) SuspectBlip.Delete();
                WrapUp();
            }
        }
        private void Shoots()
        {
            Suspect.Inventory.

        }
        private void WrapUp()
        {

        }




    }
}
