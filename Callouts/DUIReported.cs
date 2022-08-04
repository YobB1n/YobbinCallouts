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

        public override bool OnBeforeCalloutDisplayed()
        {
            Game.LogTrivial("==========YOBBINCALLOUTS: DUI Reported Callout Start==========");
            System.Random r = new System.Random();
            int Scenario = r.Next(1, 2); //scenario 0 - suspect gone. scenario 1 - suspect still there CHANGE LATER!!
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
                        NativeFunction.Natives.GetClosestVehicleNodeWithHeading(MainSpawnPoint, out Vector3 nodePosition, out float heading, 1, 3.0f, 0);
                        bool success = NativeFunction.Natives.xA0F8A7517A273C05<bool>(MainSpawnPoint, heading, out Vector3 outPosition);
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
                    NativeFunction.Natives.SET_​PED_​IS_​DRUNK(Suspect, true);

                    if (MainScenario == 0) Witness = new Ped(MainSpawnPoint);
                    else Witness = new Ped(Suspect.GetOffsetPosition(Suspect.Position.Around(3f)));
                    Witness.IsPersistent = true;
                    Witness.BlockPermanentEvents = true;
                    WitnessBlip = CallHandler.AssignBlip(Witness, Color.Blue, 0.69f, "Witness", true);
                }
                if (MainScenario == 0) //suspect gone
                {
                    Suspect.WarpIntoVehicle(SuspectVehicle, -1);
                    //test: leave suspect doing nothing for now
                }
                else //suspect on scene
                {
                    SuspectBlip = CallHandler.AssignBlip(Suspect, Color.Red, 0.69f, "Suspect", true);
                    Witness.Heading = Suspect.Heading - 180;
                    //MAKE SUSPECT DRUNK WITH NATIVES, ARGUE WITH WITNESS TASK ANIMATION
                    //ARGUE WITH SUSPECT TASK
                }
            }
            catch (Exception e)
            {
                Game.LogTrivial("==========YOBBINCALLOUTS: ERROR CAUGHT ON CALLOUT INTIALIZATION==========");
                Game.LogTrivial("IN: " + this);
                string error = e.ToString();
                Game.LogTrivial("ERROR: " + error);
                Game.DisplayNotification("There was an ~r~Error~w~ Caught with ~b~YobbinCallouts. ~w~Please Check Your ~g~Log File.~w~ Sorry for the Inconvenience!");
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
                        System.Random r = new System.Random();
                        int TriggerDistance = r.Next(25, 45);
                        Game.LogTrivial("YOBBINCALLOUTS: Callout will trigger when player is " + TriggerDistance + " Away.");
                        while (player.DistanceTo(Suspect) >= TriggerDistance && !Game.IsKeyDown(Config.CalloutEndKey)) GameFiber.Wait(0);
                        if (Game.IsKeyDown(Config.CalloutEndKey)) break;

                        if (MainScenario == 0) //not on scene
                        {
                            System.Random twboop = new System.Random();
                            int dialoguechosen = twboop.Next(0, 0);
                            if (dialoguechosen == 0) CallHandler.Dialogue(WitnessOpening1, Witness);
                        }
                        else //on scene
                        {
                            if (Config.DisplayHelp) Game.DisplayHelp("~b~Investigate~w~ the Situation.");
                            if (Suspect.IsMale) Game.DisplaySubtitle("~b~Caller:~w~ You can't drive bro! I won't let you get in that car!", 2500);
                            else Game.DisplaySubtitle("~b~Caller:~w~ You can't drive miss! I won't let you get in that car!", 2500);
                            GameFiber.Wait(2500);
                            System.Random monke = new System.Random();
                            int speechtowait = monke.Next(1, 6);

                            //argument between the two peds starts now. After a certain point the suspect leaves to hop in the car.
                            int useless = 0;
                            switch (useless)
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

                            if (Functions.IsPedStoppedByPlayer(Suspect))  //test this
                            {
                                //dialogue

                                System.Random r2 = new System.Random();
                                int action = r2.Next(0, 2);
                                if(action == 0) //discretion
                                {
                                    Game.DisplayHelp("Deal with the ~r~Suspect~w~ as you see fit. Press ~b~" + Config.CalloutEndKey + " ~w~when done.");
                                    while (!Game.IsKeyDown(Config.CalloutEndKey)) GameFiber.Wait(0);
                                    break;
                                }
                                else //late flee
                                {
                                    Pursuit();
                                    CallHandler.SuspectWait(Suspect);
                                }
                            }

                            Suspect.Tasks.FollowNavigationMeshToPosition(SuspectVehicle.Position, SuspectVehicle.Heading - 90, 5f, 1f, -1).WaitForCompletion();
                            Suspect.Tasks.EnterVehicle(SuspectVehicle, -1).WaitForCompletion();
                            Suspect.Tasks.CruiseWithVehicle(20f, VehicleDrivingFlags.AllowWrongWay | VehicleDrivingFlags.DriveAroundVehicles);
                            Game.DisplayHelp("Stop the ~r~Suspect.");
                            while (!Functions.IsPlayerPerformingPullover()) GameFiber.Wait(0);                           
                            Pursuit();
                            CallHandler.SuspectWait(Suspect);
                        }
                        break;
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
            System.Random twboop = new System.Random();
            int dialoguechosen = twboop.Next(0, dialogue.Count);
            SpeechCounter++;
            return dialogue[dialoguechosen];
        }
        private void Pursuit()
        {
            System.Random monke2 = new System.Random();
            int waitforpursuit = monke2.Next(2500, 6000); //add more scenarios later
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
        private void VehicleDescription()
        {
            Functions.SetVehicleOwnerName(SuspectVehicle, Functions.GetPersonaForPed(Suspect).FullName);
            var personaarray = new string[3];
            personaarray[0] = "~n~~w~Registered to: ~y~" + Functions.GetPersonaForPed(Suspect).Forename;
            personaarray[1] = "~n~~w~Plate: ~y~" + SuspectVehicle.LicensePlate;
            personaarray[2] = "~n~~w~Color: ~y~" + SuspectVehicle.PrimaryColor.Name;
            var persona = string.Concat(personaarray);
            //change icon later
            Game.DisplayNotification("commonmenu", "shop_health_icon_b", "~g~Vehicle Description", "~b~" + SuspectVehicle.Model.Name, persona);
        }
    }
}
