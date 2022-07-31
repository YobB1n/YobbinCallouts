//DUI Reported Callout - by YobB1n
//Started July 29, 2022

using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using System;
using Rage.Native;
using System.Drawing;
using System.Collections.Generic;

namespace YobbinCallouts.Callouts
{
    [CalloutInfo("DUI Reported", CalloutProbability.High)]
    class DUIReported : Callout
    {
        private Vector3 MainSpawnPoint;

        private Ped Suspect;
        private Ped Suspect2;
        private Ped Witness;
        private Vehicle SuspectVehicle;
        private Blip SuspectBlip;
        private Blip WitnessBlip;
        private Ped player = Game.LocalPlayer.Character;
        private LHandle MainPursuit;

        private int MainScenario;
        private bool CalloutRunning;

        //DIALOGUE VVvvVV
        private readonly List<string> WitnessOpening1 = new List<string>()
        {
            "~b~Caller:~w~ Officer, ",
        };

        public override bool OnBeforeCalloutDisplayed()
        {
            Game.LogTrivial("==========YOBBINCALLOUTS: DUI Reported Callout Start==========");
            System.Random r = new System.Random();
            int Scenario = r.Next(0, 2); //scenario 0 - suspect gone. scenario 1 - suspect still there
            MainScenario = Scenario;
            Game.LogTrivial("YOBBINCALLOUTS: Scenario value is: " + MainScenario);

            if(MainScenario == 0) //suspect gone - house
            {
                CallHandler.locationChooser(CallHandler.
                    );
                if (CallHandler.locationReturned) { MainSpawnPoint = CallHandler.SpawnPoint; }
                else { MainScenario = 1; } //if no house local, move on to scenario 1
            }
            else MainSpawnPoint = World.GetNextPositionOnStreet(player.Position.Around(600)); //no house

            ShowCalloutAreaBlipBeforeAccepting(MainSpawnPoint, 25f);
            AddMinimumDistanceCheck(60f, MainSpawnPoint);
            Functions.PlayScannerAudio("ATTENTION_ALL_SWAT_UNITS_01 WE_HAVE_01 UNITS_RESPOND_CODE_03_01"); //ADD SCANNER AUDIO
            CalloutMessage = "DUI Reported";
            CalloutPosition = MainSpawnPoint;
            if (MainScenario == 0) CalloutAdvisory = "Caller has reported an intoxicated individual who has just left their residence.";
            else CalloutAdvisory = "Caller has reported an intoxicated individual attempting to enter a vehicle.";
            return base.OnBeforeCalloutDisplayed();
        }
        public override bool OnCalloutAccepted()
        {
            try
            {
                Game.LogTrivial("YOBBINCALLOUTS: DUI Reported Callout Accepted by User");
                if (Main.CalloutInterface)
                {
                    CalloutInterfaceHandler.SendCalloutDetails(this, "CODE 03", "");
                }
                else
                {
                    Game.DisplayNotification("Respond ~r~Code 3.");
                }

                if (MainScenario >= 0) //vehicle
                {
                    try //vehicle spawning on the side of the road beside MainSpawnPoint
                    {
                        NativeFunction.Natives.GetClosestVehicleNodeWithHeading(MainSpawnPoint, out Vector3 nodePosition, out float heading, 1, 3.0f, 0);
                        bool success = NativeFunction.Natives.xA0F8A7517A273C05<bool>(MainSpawnPoint, heading, out Vector3 outPosition);
                        if (success)
                        {
                            SuspectVehicle = CallHandler.SpawnVehicle(outPosition, heading);
                            SuspectVehicle.IsPersistent = true;
                        }
                        else
                        {
                            Game.LogTrivial("YOBBINCALLOUTS: Could Not Find Spawnpoint. Aborting Callout.");
                            return false;
                        }
                    }
                    catch (Exception)
                    {
                        Game.LogTrivial("YOBBINCALLOUTS: Could Not Find Spawnpoint. Aborting Callout.");
                        return false;
                    }
                    //Instantiate the driver (suspect)
                    Suspect = new Ped(MainSpawnPoint);
                    Suspect.IsPersistent = true;
                    Suspect.BlockPermanentEvents = true;
                    //MAKE SUSPECT DRUNK WITH NATIVES

                    if (MainScenario == 0) Witness = new Ped(MainSpawnPoint);
                    else Witness = new Ped(Suspect.GetOffsetPosition(Suspect.Position.Around(3f)));
                    Witness.IsPersistent = true;
                    Witness.BlockPermanentEvents = true;
                    WitnessBlip = CallHandler.AssignBlip(Witness, Color.Blue, 0.69f, "Witness");
                }
                if(MainScenario == 0) //suspect gone
                {
                    Suspect.WarpIntoVehicle(SuspectVehicle, -1);
                    //test: leave suspect doing nothing for now
                }
                else //suspect on scene
                {
                    SuspectBlip = CallHandler.AssignBlip(Suspect, Color.Red, 0.69f, "Suspect", true);
                    //MAKE SUSPECT DRUNK WITH NATIVES, ARGUE WITH WITNESS TASK
                    //ARGUE WITH SUSPECT TASK
                }
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
            if (!CalloutRunning) { Callout(); }
            return base.OnCalloutAccepted();
        }
        public override void OnCalloutNotAccepted()
        {
            Game.LogTrivial("YOBBINCALLOUTS: DUI Reported Callout Not Accepted by User.");
            //Functions.PlayScannerAudio("OTHER_UNIT_TAKING_CALL_02");
            base.OnCalloutNotAccepted();
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
                        while (player.DistanceTo(Suspect) >= 25 && !Game.IsKeyDown(Config.CalloutEndKey)) GameFiber.Wait(0);
                        if (Game.IsKeyDown(Config.CalloutEndKey)) break;

                        if (MainScenario == 0) //not on scene
                        { 
                            
                        }
                        else //on scene
                        {
                            if (Suspect.IsMale) Game.DisplaySubtitle("~b~Caller:~w~ You can't drive bro! I won't let you get in that car!");
                            else Game.DisplaySubtitle("~b~Caller:~w~ You can't drive miss! I won't let you get in that car!");
                        }
                    }
                    GameFiber.Wait(2000);
                    Game.LogTrivial("YOBBINCALLOUTS: Callout Finished, Ending...");
                    EndCalloutHandler.EndCallout();
                    End();
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
        public override void End()
        {
            base.End();
            if (CalloutRunning)
            {
                Game.DisplayNotification("~g~Code 4~w~, return to patrol.");
                Functions.PlayScannerAudio("ATTENTION_ALL_UNITS WE_ARE_CODE_4");
            }
            CalloutRunning = false;
            if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }
            Game.LogTrivial("YOBBINCALLOUTS: DUI Reported Callout Finished Cleaning Up.");
        }
        public override void Process()
        {
            base.Process();
        }
    }
}
