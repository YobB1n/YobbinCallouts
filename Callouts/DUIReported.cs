//DUI Reported Callout - by YobB1n
//Started July 29, 2022

using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace YobbinCallouts.Callouts
{
    [CalloutInfo("[YC] DUI Reported", CalloutProbability.High)]
    class DUIReported : Callout
    {
        private Vector3 MainSpawnPoint;

        private Citizen Suspect;
        private Ped Suspect2;
        private Ped Witness;
        private Vehicle SuspectVehicle;
        private Blip SuspectBlip;
        private Blip WitnessBlip;
        private Ped player = Game.LocalPlayer.Character;
        private LHandle MainPursuit;
        private int SpeechCounter = 1;

        private int MainScenario;
        private bool CalloutRunning;

        //DIALOGUE VVvvVV
        private readonly List<string> WitnessOpening1 = new List<string>()
        {
            "~b~Caller:~w~ Officer, come quick! We got an emergency!",
            "~g~You:~w~ Are you the caller who reported a DUI?",
            "~b~Caller:~w~ Yes that's me! A friend of mine was in no condition to drive a car, but managed to get behind the wheel and drive off!",
            "~g~You:~w~ Do you know what the vehicle looks like?",
            "~b~Caller:~w~ I do, here's a vehicle description. Please find them before they hurt themselves or others!",
        };
        private readonly List<string> WitnessOpening2 = new List<string>()
        {
            "~b~Caller:~w~ Officer, over here!",
            "~g~You:~w~ Are you the person who called in a DUI?",
            "~b~Caller:~w~ Yes that's me! Someone at this party was in no condition to drive, but managed to get behind the wheel and drive off!",
            "~g~You:~w~ Do you have a vehicle description?",
            "~b~Caller:~w~ I do, here you go. Please find them before they hurt themselves or others!",
        };
        private readonly List<string> WitnessOpening3 = new List<string>()
        {
            "~b~Caller:~w~ Over here, Officer",
            "~g~You:~w~ Did you call in the DUI?",
            "~b~Caller:~w~ Yes that's me! My friend and I were drinking and they had a little to much.",
            "~b~Caller:~w~ When it was time for them to leave, I offered to call them a taxi, but they refused and managed to drive off despite my best efforts!",
            "~g~You:~w~ Do you know what their vehicle looks like?",
            "~b~Caller:~w~ I do, here's a description of their vehicle. Please find them before they hurt themselves or others!",
        };
        private readonly List<string> Argument1 = new List<string>()
        {
            "~r~Suspect:~w~ I-I told you, I'm TOTALLY fine to drive! Leave m-m-me alone!!",
            "~r~Suspect:~w~ L-Leave me alone! You can't tell me what to d-do!!",
            "~r~Suspect:~w~ You're not my mom!! I feel fine to d-drive, leave me alone!",
            "~r~Suspect:~w~ Shut up! I'm fine to drive, stop telling me what to do!",
        };
        private readonly List<string> Argument2 = new List<string>()
        {
            "~b~Caller:~w~ You've had way to many drinks! Come on, just hand me the keys!",
            "~b~Caller:~w~ I can't let you do that! Don't get behind that wheel!",
            "~b~Caller:~w~ No, you're way to drunk! I've seen how many you've had, you can't drive!",
            "~b~Caller:~w~ Trust me, you can't drive! I'll give you a lift home later, just pass me the keys!",
            "~b~Caller:~w~ Please give me the keys! You're not going anywhere, I'll call you a taxi!",
        };
        private readonly List<string> Argument3 = new List<string>()
        {
            "~r~Suspect:~w~ I've driven before when I was WAY more drunk than this, I can do it again!",
            "~r~Suspect:~w~ B-Believe me, this is nothing! I feel fine!",
            "~r~Suspect:~w~ Stop being such a baby about it, I've driven after having more, and I'm still here!!",
            "~r~Suspect:~w~ I ain't listening to you, stop whining like a little bitch!",
        };
        private readonly List<string> Argument4 = new List<string>()
        {
            "~b~Caller:~w~ I've already called the cops, they'll be here any moment! Do yourself a favor and lose the keys!",
            "~b~Caller:~w~ The cops are already on their way! Don't make this any worse for yourself!",
            "~b~Caller:~w~ The police are on the way now, they're gonna be here soon! Give me the keys now!",
            "~b~Caller:~w~ This won't end well for you unless you give me the keys right now!",
        };
        private readonly List<string> Reason1 = new List<string>()
        {
            "~g~You:~w~ Hey, stop right there for me!",
            "~r~Suspect:~w~ What do YOU want, Officer? I d-don't remember calling you!",
            "~g~You:~w~ Actually, someone called us saying that you were trying to drive after drinking too much.",
            "~r~Suspect:~w~ What?! You got the wrong person, I'm totally fine! Who snitched on me?!",
             "~g~You:~w~ Alright, well to me, you don't seem to be. Hang tight right here for me.",
        };
        private readonly List<string> Reason2 = new List<string>()
        {
            "~g~You:~w~ Hey, hold up right there for me!",
            "~r~Suspect:~w~ Who, me?! What do you want from me, Officer?! I'm just trying to get back home!!",
            "~g~You:~w~ Well, we got a call saying that you were trying to drive after drinking too much.",
            "~r~Suspect:~w~ H-huh?! ME? You got the wrong person!! Leave me alone and let me go!!",
             "~g~You:~w~ Alright, we'll see about that. Hang tight right here for me.",
        };
        private readonly List<string> Reason3 = new List<string>()
        {
            "~g~You:~w~ Hey, stop right there!",
            "~r~Suspect:~w~ What do YOU want, Officer? I never called YOU!",
            "~g~You:~w~ Actually, someone called us saying that you were going to drive off after drinking too much.",
            "~r~Suspect:~w~ That ain't me, Officer! I'm just trying to exercise my freedom of travel!",
             "~g~You:~w~ Alright, well to me, you don't seem fit to do that right now. Hang tight right here for me.",
        };

        public override bool OnBeforeCalloutDisplayed()
        {
            Game.LogTrivial("==========YOBBINCALLOUTS: DUI Reported Callout Start==========");
            int Scenario = CallHandler.RNG(0, 2); //scenario 0 - suspect gone. scenario 1 - suspect still there CHANGE LATER!!
            MainScenario = Scenario;
            Game.LogTrivial("YOBBINCALLOUTS: Scenario value is: " + MainScenario);

            CallHandler.locationChooser(CallHandler.HouseList);
            if (CallHandler.locationReturned) { MainSpawnPoint = CallHandler.SpawnPoint; }
            else //if no house local, move on to scenario 1
            {
                MainScenario = 1;
                MainSpawnPoint = World.GetNextPositionOnStreet(player.Position.Around(600)); //no house
            }

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
                        if (MainScenario == 0)
                        {
                            NativeFunction.Natives.GetClosestVehicleNodeWithHeading(World.GetNextPositionOnStreet(MainSpawnPoint.Around(300f)), out Vector3 nodePosition, out float heading, 1, 3.0f, 0);
                            bool success = NativeFunction.Natives.xA0F8A7517A273C05<bool>(World.GetNextPositionOnStreet(MainSpawnPoint.Around(300f)), heading, out Vector3 outPosition);
                            if (success)
                            {
                                SuspectVehicle = CallHandler.SpawnVehicle(outPosition, heading);
                            }
                            else
                            {
                                Game.LogTrivial("YOBBINCALLOUTS: Could Not Find Spawnpoint. Aborting Callout.");
                                return false;
                            }
                        }
                        else //house
                        {
                            NativeFunction.Natives.GetClosestVehicleNodeWithHeading(MainSpawnPoint, out Vector3 nodePosition, out float heading, 1, 3.0f, 0);
                            bool success = NativeFunction.Natives.xA0F8A7517A273C05<bool>(MainSpawnPoint, heading, out Vector3 outPosition);
                            if (success)
                            {
                                SuspectVehicle = CallHandler.SpawnVehicle(CallHandler.SpawnOnSreetSide(MainSpawnPoint), heading);
                            }
                            else
                            {
                                Game.LogTrivial("YOBBINCALLOUTS: Could Not Find Spawnpoint. Aborting Callout.");
                                return false;
                            }
                        }

                    }
                    catch (Exception)
                    {
                        Game.LogTrivial("YOBBINCALLOUTS: Could Not Find Spawnpoint. Aborting Callout.");
                        return false;
                    }
                    //Instantiate the driver (suspect)
                    Suspect = new Citizen(MainSpawnPoint);
                    Suspect.IsPersistent = true;
                    Suspect.BlockPermanentEvents = true;
                    //MAKE SUSPECT DRUNK
                    NativeFunction.Natives.SET_​PED_​IS_​DRUNK(Suspect, true);
                    if (Main.PR)
                    {
                        PolicingRedefinedFunctions.SetPedDrunk(Suspect, 0.15f);
                        Game.LogTrivial("YOBBINCALLOUTS: PR ped BAC is " + PolicingRedefinedFunctions.GetPedBAC(Suspect).ToString());
                        Game.LogTrivial("YOBBINCALLOUTS: Ped is drunk: " + PolicingRedefinedFunctions.IsPedDrunk(Suspect));
                    }
                    if (MainScenario == 0) Witness = new Ped(MainSpawnPoint); //suspect is gone, witness is only ped on scene
                    else Witness = new Ped(Suspect.Position.Around2D(0.4f)); //suspect AND witness on scene - Test this (don't want this ped to be stuck in a building)
                    Witness.SetPositionZ(Suspect.Position.Z); //test this
                    Witness.IsPersistent = true;
                    Witness.BlockPermanentEvents = true;
                    WitnessBlip = CallHandler.AssignBlip(Witness, Color.Blue, 0.69f, "Witness", true);
                }
                if (MainScenario == 0) //suspect gone
                {
                    Suspect.WarpIntoVehicle(SuspectVehicle, -1);
                    Witness.Tasks.PlayAnimation("random@domestic", "f_distressed_loop", -1, AnimationFlags.Loop);
                    //test: leave suspect doing nothing for now
                }
                else //suspect on scene
                {
                    SuspectBlip = CallHandler.AssignBlip(Suspect, Color.Red, 0.69f, "Suspect", true);
                    Witness.Heading = Suspect.Heading - 180;
                    Witness.Tasks.PlayAnimation("random@domestic", "balcony_fight_idle_male", -1, AnimationFlags.Loop);
                    //MAKE SUSPECT DRUNK WITH NATIVES, ARGUE WITH WITNESS TASK ANIMATION
                    //ARGUE WITH SUSPECT TASK
                }
            }
            catch (Exception e)
            {
                Game.LogTrivial("==========YOBBINCALLOUTS: ERROR CAUGHT ON CALLOUT INITIALIZATION==========");
                Game.LogTrivial("IN: " + this);
                string error = e.ToString();
                Game.LogTrivial("ERROR: " + error);
                Game.DisplayNotification("There was an ~r~Error~w~ Caught with ~b~YobbinCallouts. ~w~Please Check Your ~g~Log File.~w~ Sorry for the Inconvenience!");
                Game.DisplayNotification("Error: ~r~" + error);
                Game.LogTrivial("If You Believe this is a Bug, Please Report it on my Discord Server. Thanks!");
                Game.LogTrivial("==========YOBBINCALLOUTS: ERROR CAUGHT ON CALLOUT INITIALIZATION==========");
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


                        if (MainScenario == 0) //not on scene
                        {
                            while (player.DistanceTo(Witness) >= 5f && !Game.IsKeyDown(Config.CalloutEndKey)) GameFiber.Wait(0);
                            if (Game.IsKeyDown(Config.CalloutEndKey)) break;

                            int dialoguechosen = CallHandler.RNG(0, 3);
                            if (dialoguechosen == 0) CallHandler.Dialogue(WitnessOpening1, Witness);
                            else if (dialoguechosen == 1) CallHandler.Dialogue(WitnessOpening2, Witness);
                            else CallHandler.Dialogue(WitnessOpening3, Witness);
                            GameFiber.Wait(1500);
                            CallHandler.IdleAction(Witness, false);
                            Search();

                            //...
                        }
                        else //on scene
                        {
                            int TriggerDistance = CallHandler.RNG(25, 45);
                            Game.LogTrivial("YOBBINCALLOUTS: Callout will trigger when player is " + TriggerDistance + " Away.");
                            while (player.DistanceTo(Suspect) >= TriggerDistance && !Game.IsKeyDown(Config.CalloutEndKey)) GameFiber.Wait(0);
                            if (Game.IsKeyDown(Config.CalloutEndKey)) break;

                            if (Config.DisplayHelp) Game.DisplayHelp("~b~Investigate~w~ the Situation.");
                            Game.DisplaySubtitle("~b~Caller:~w~ You can't drive! I won't let you get in that car!", 2500);
                            GameFiber.Wait(1000);
                            int speechtowait = CallHandler.RNG(1, 5);

                            Game.LogTrivial("YOBBINCALLOUTS: Argument between suspect and caller begins, dialogue options: " + speechtowait);
                            //argument between the two peds starts now. After a certain point the suspect leaves to hop in the car.
                            int useless = 0;
                            switch (useless) //swtich statement doesn't do anything
                            {
                                case 0:
                                    if (speechtowait == 1 || Functions.IsPedStoppedByPlayer(Suspect)) break;
                                    DialogueAdvance(Argument1);
                                    GameFiber.Wait(2500);
                                    if (speechtowait == 2 || Functions.IsPedStoppedByPlayer(Suspect)) break;
                                    DialogueAdvance(Argument2);
                                    GameFiber.Wait(2500);
                                    if (speechtowait == 3 || Functions.IsPedStoppedByPlayer(Suspect)) break;
                                    DialogueAdvance(Argument3);
                                    GameFiber.Wait(2500);
                                    if (speechtowait == 4 || Functions.IsPedStoppedByPlayer(Suspect)) break;
                                    DialogueAdvance(Argument4);
                                    GameFiber.Wait(2500);
                                    break;
                            }

                            //Suspect.Tasks.EnterVehicle(SuspectVehicle, -1);
                            //while (!Functions.IsPedStoppedByPlayer(Suspect) && !Functions.IsPedArrested(Suspect) && !Suspect.IsInAnyVehicle(false)) GameFiber.Wait(0);
                            //Suspect.Tasks.CruiseWithVehicle(25f, VehicleDrivingFlags.AllowWrongWay|VehicleDrivingFlags.DriveAroundVehicles|VehicleDrivingFlags.Emergency);

                            //change logic to after
                            //=========THIS IS WHAT I NEED TO WORK ON BUT TOO LAZY RN===========
                            if (Functions.IsPedStoppedByPlayer(Suspect) || Functions.IsPedArrested(Suspect))  //test this
                            {
                                //dialogue
                                int dialoguechosen = CallHandler.RNG(0, 3);
                                if (dialoguechosen == 0) CallHandler.Dialogue(Reason1, Suspect);
                                else if (dialoguechosen == 1) CallHandler.Dialogue(Reason2, Suspect);
                                else CallHandler.Dialogue(Reason3, Suspect);

                                int action = CallHandler.RNG(0, 2);
                                if (action == 0) //discretion
                                {
                                    Game.LogTrivial("YOBBINCALLOUTS: player dealing with suspect at their descretion.");
                                    Game.DisplayHelp("Deal with the ~r~Suspect~w~ as you see fit. Press ~b~" + Config.CalloutEndKey + " ~w~when done.");
                                    while (!Game.IsKeyDown(Config.CalloutEndKey)) GameFiber.Wait(0);
                                    break;
                                }
                                else //late flee
                                {
                                    Game.LogTrivial("YOBBINCALLOUTS: Late flee scenario if suspect not arrested.");
                                    if (Suspect.Exists() && !Functions.IsPedArrested(Suspect))
                                    {
                                        Pursuit();
                                        CallHandler.SuspectWait(Suspect);
                                    }
                                    break;
                                }
                            }

                            //PROBLEM - if suspect is killed or arrested before task completes, callout does not continue
                            Suspect.Tasks.FollowNavigationMeshToPosition(SuspectVehicle.Position, SuspectVehicle.Heading - 90, 4f, 1f, -1).WaitForCompletion(5000); //test 5 sec wait   
                            while (true)
                            {
                                GameFiber.Yield();

                                if (!Suspect.Exists() || Suspect.IsDead || Functions.IsPedArrested(Suspect) || Game.IsKeyDown(Config.CalloutEndKey))
                                {
                                    Game.LogTrivial("YOBBINCALLOUTS: continuing, suspect killed/arrested/end key pressed...");
                                    break;
                                }

                                if (Suspect.Tasks.CurrentTaskStatus != TaskStatus.InProgress)
                                {
                                    Game.LogTrivial("YOBBINCALLOUTS: continuing, suspect task completed...");
                                    break;
                                }
                            }
                            Suspect.Tasks.EnterVehicle(SuspectVehicle, -1);
                            while (true)
                            {
                                GameFiber.Yield();

                                if (!Suspect.Exists() || Suspect.IsDead || Functions.IsPedArrested(Suspect) || Game.IsKeyDown(Config.CalloutEndKey))
                                {
                                    Game.LogTrivial("YOBBINCALLOUTS: continuing, suspect killed/arrested/end key pressed...");
                                    break;
                                }

                                if (Suspect.Tasks.CurrentTaskStatus != TaskStatus.InProgress)
                                {

                                    if (Suspect.IsInAnyVehicle(false))
                                    {
                                        Game.LogTrivial("YOBBINCALLOUTS: continuing, suspect task completed...");
                                        break;
                                    }
                                    else //this is if the suspect gets stuck and stops trying to enter the vehicle.
                                    {
                                        Suspect.Tasks.EnterVehicle(SuspectVehicle, -1);
                                    }
                                }
                            }
                            //END PROBLEM

                            if(Suspect.IsInAnyVehicle(false)) Suspect.Tasks.CruiseWithVehicle(SuspectVehicle, 20f, VehicleDrivingFlags.AllowWrongWay | VehicleDrivingFlags.DriveAroundVehicles);
                            
                            while (!Functions.IsPlayerPerformingPullover() && !Functions.IsPedArrested(Suspect) && Suspect.IsAlive) GameFiber.Wait(0);
                            if (Suspect.Exists() && !Functions.IsPedArrested(Suspect) && Suspect.IsAlive) Pursuit();
                            CallHandler.SuspectWait(Suspect);
                        }
                        break;
                    }
                    GameFiber.Wait(2000);
                    Game.LogTrivial("YOBBINCALLOUTS: Callout Finished, Ending...");
                    EndCalloutHandler.EndCallout();
                    End();
                }
                catch (System.Threading.ThreadAbortException)
                {
                    Game.LogTrivial("YOBBINCALLOUTS: THREADABORTEXCEPTION CAUGHT. Usually not a big deal, caused by another plugin/crash somewhere else.");
                }
                catch (Exception e)
                {
                    if (CalloutRunning)
                    {
                        Game.LogTrivial("==========YOBBINCALLOUTS: ERROR CAUGHT==========");
                        Game.LogTrivial("IN: " + this);
                        string error = e.ToString();
                        Game.LogTrivial("ERROR: " + error);
                        Game.DisplayNotification("There was an ~r~Error~w~ Caught with ~b~YobbinCallouts. ~w~Please Check Your ~g~Log File.~w~ Sorry for the Inconvenience!");
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
            if (WitnessBlip.Exists()) WitnessBlip.Delete();
            Game.LogTrivial("YOBBINCALLOUTS: DUI Reported Callout Finished Cleaning Up.");
        }
        public override void Process()
        {
            base.Process();
        }
        private String DialogueAdvance(List<string> dialogue)
        {
            Game.LogTrivial("YOBBINCALLOUTS: waiting for suspect to be stopped, dialogue advancing.");
            int dialoguechosen = CallHandler.RNG(0, dialogue.Count);
            SpeechCounter++;
            Game.DisplaySubtitle(dialogue[dialoguechosen]);
            return dialogue[dialoguechosen];
        }
        private void Pursuit()
        {
            Game.DisplayHelp("Stop the ~r~Suspect.");
            int waitforpursuit = CallHandler.RNG(2500, 6000); //add more scenarios later
            GameFiber.Wait(waitforpursuit);
            Game.LogTrivial("YOBBINCALLOUTS: Suspect Pursuit Started");
            Functions.ForceEndCurrentPullover();
            MainPursuit = LSPD_First_Response.Mod.API.Functions.CreatePursuit();
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("CRIME_SUSPECT_ON_THE_RUN_01");
            Game.DisplayNotification("Suspect is ~r~Evading!");
            LSPD_First_Response.Mod.API.Functions.RequestBackup(player.Position, LSPD_First_Response.EBackupResponseType.Code3, LSPD_First_Response.EBackupUnitType.LocalUnit);
            LSPD_First_Response.Mod.API.Functions.SetPursuitIsActiveForPlayer(MainPursuit, true);
            LSPD_First_Response.Mod.API.Functions.AddPedToPursuit(MainPursuit, Suspect);
            while (Functions.IsPursuitStillRunning(MainPursuit)) GameFiber.Wait(0);
        }
        private void Search()
        {
            CallHandler.VehicleInfo(SuspectVehicle, Suspect);
            GameFiber.Wait(1000);
            if (Config.DisplayHelp) Game.DisplayHelp("Search for the ~r~Suspect~w~.");

            //Suspect.Tasks.CruiseWithVehicle(20f, VehicleDrivingFlags.AllowWrongWay | VehicleDrivingFlags.DriveAroundVehicles);
            SuspectBlip = new Blip(Suspect.Position.Around(15), 175);
            SuspectBlip.IsRouteEnabled = true;
            SuspectBlip.Color = Color.Orange;
            SuspectBlip.Alpha = 0.69f;

            while (player.DistanceTo(Suspect) >= 85) GameFiber.Wait(0);
            Suspect.Tasks.CruiseWithVehicle(20f, VehicleDrivingFlags.AllowWrongWay | VehicleDrivingFlags.DriveAroundVehicles); //moving here to make it easier to find suspect
            Game.DisplayNotification("Callers have spotted the ~r~Suspect~w~ driving ~o~Recklessly~w~. Updating Map...");
            if (SuspectBlip.Exists()) SuspectBlip.Delete();
            SuspectBlip = CallHandler.AssignBlip(Suspect, Color.Red, 1, "Suspect");

            while (!Functions.IsPlayerPerformingPullover() && Suspect.Exists() && !Functions.IsPedArrested(Suspect)) GameFiber.Wait(0);
            Pursuit();
            CallHandler.SuspectWait(Suspect);
        }
    }
}
