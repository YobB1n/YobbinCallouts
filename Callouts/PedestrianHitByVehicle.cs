using System.Drawing;
using Rage;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage.Native;
using System.Collections.Generic;
using System;

namespace YobbinCallouts.Callouts
{
    [CalloutInfo("[YC] Pedestrian Hit By Vehicle", CalloutProbability.High)]

    class PedestrianHitByVehicle : Callout
    {
        private int MainScenario; //If there are multiple callout scenarios, which one, starting from zero.
        private bool CalloutRunning; //If the callout is currently running or not.

        private Vector3 MainSpawnPoint;
        private Ped player = Game.LocalPlayer.Character;
        private Ped Suspect;
        private Citizen Victim;
        private Citizen Witness;
        private Vehicle SuspectVehicle;

        private Blip SuspectBlip;
        private Blip VictimBlip;
        private Blip AreaBlip;
        private Blip WitnessBlip;
        private LHandle MainPursuit;

        public static string SuspectVehicleDescription;

        //DIALOGUE VvvV
        private readonly List<string> WitnessDriverAtFault1 = new List<string>()
        {
         "~g~You:~w~ Hello, are you the caller?",
         "~b~Witness:~w~ Yes I am, Officer. I was just walking down the street when this car hit that Pedestrian over there!",
         "~g~You:~w~ Did you see what happened?",
         "~b~Witness:~w~ Yes, it was 100% the Driver's fault. They were just crossing the street!",
         "~g~You:~w~ Do you have a description of their vehicle?",
        };
        private readonly List<string> WitnessDriverAtFault2 = new List<string>()
        {
         "~g~You:~w~ Hello, did you call us?",
         "~b~Witness:~w~ Yes I did, Officer. I was walking down the street when a car struck that Pedestrian!",
         "~g~You:~w~ Did you get what happened?",
         "~b~Witness:~w~ Yes, it was the Driver's fault for sure. They were just crossing the street!",
         "~g~You:~w~ Do you have a description of the vehicle?",
        };
        private readonly List<string> WitnessDriverAtFault3 = new List<string>()
        {
         "~g~You:~w~ Hi, are you the caller?",
         "~b~Witness:~w~ Yes I am, Officer. This car struck that pedestrian and just drove off like nothing happened!",
         "~g~You:~w~ Did you see the crash?",
         "~b~Witness:~w~ Yes, it was definitely the driver's fault. They didn't slow down at all!",
         "~g~You:~w~ Do you have a description of their vehicle?",
        };
        private readonly List<string> DriverDriverNotAtFault1 = new List<string>()
        {
         "~r~Driver:~w~ Officer I really hope that person will be alright! I swear I couldn't have avoided them!",
         "~g~You:~w~ What happened?",
         "~r~Driver:~w~ I was just going to get some groceries and they crossed into the street without looking!",
         "~r~Driver:~w~ I tried to slam on my brakes but they locked up and I couldn't stop in time.",
         "~g~You:~w~ Okay, hang tight here for me, I'll figure this out.",
        };
        private readonly List<string> DriverDriverNotAtFault2 = new List<string>()
        {
         "~g~You:~w~ Hello, can you tell me what happened?",
         "~r~Driver:~w~ It all happened so fast! I think they stepped out into the street without looking and I couldn't stop in time.",
         "~g~You:~w~ Did you try to help them?",
         "~r~Driver:~w~ I was going to, I was just really shocked at what happened. I called 9-1-1 first thing.",
         "~g~You:~w~ Okay, hang tight here for me, I'll sort this out.",
        };
        private readonly List<string> DriverDriverNotAtFault3 = new List<string>()
        {
         "~g~You:~w~ Hello, Are you all right?",
         "~r~Driver:~w~ Yeah I'm fine. Hopefully they will be too! It's not my fault Officer, trust me!",
         "~g~You:~w~ Tell me what happened.",
         "~r~Driver:~w~ I was just driving along and I guess they started jaywalking down the street. I didn't see them until it was too late!",
         "~g~You:~w~ Okay, hang tight here for me, I'll sort this out.",
        };

        //pre-callout logic
        public override bool OnBeforeCalloutDisplayed()
        {
            Game.LogTrivial("==========YOBBINCALLOUTS: Pedestrian Hit By Vehicle Callout Start=========="); //All callout starts are logged like this
            int Scenario = CallHandler.RNG(0, 2); //0 - hit and run, 1 - still on scene.
            MainScenario = Scenario;
            Game.LogTrivial("YOBBINCALLOUTS: Scenario value is: " + MainScenario);

            MainSpawnPoint = World.GetNextPositionOnStreet(player.Position.Around(600)); //get the Main Spawn point.
            ShowCalloutAreaBlipBeforeAccepting(MainSpawnPoint, 25f);
            AddMinimumDistanceCheck(60f, MainSpawnPoint);
            if (Scenario == 0) Functions.PlayScannerAudio("ATTENTION_ALL_UNITS_01 WE_HAVE_01 YC_HITANDRUN CITIZENS_REPORT_01 YC_PEDESTRIANSTRUCKVEHICLE");
            else Functions.PlayScannerAudio("ATTENTION_ALL_UNITS_01 WE_HAVE_01 CITIZENS_REPORT_01 YC_PEDESTRIANSTRUCKVEHICLE");

            CalloutMessage = "Pedestrian Hit by Vehicle";
            CalloutPosition = MainSpawnPoint;
            if (MainScenario == 0) CalloutAdvisory = "Witness reports the driver fled the scene.";
            else CalloutAdvisory = "Witness reports the driver is still at the scene.";
            return base.OnBeforeCalloutDisplayed();
        }
        public override bool OnCalloutAccepted()
        {
            try
            {
                Game.LogTrivial("YOBBINCALLOUTS: Pedestrian Hit by Vehicle Callout Accepted by User");
                if (Main.CalloutInterface)
                {
                    CalloutInterfaceHandler.SendCalloutDetails(this, "CODE 03", ""); //For Opus' Callout Interface Plugin
                }
                else
                {
                    Game.DisplayNotification("Respond ~r~Code 03"); //If they don't have the plugin installed
                }

                Victim = new Citizen(MainSpawnPoint);
                Victim.IsPersistent = true;
                Victim.BlockPermanentEvents = true;

                SuspectVehicle = CallHandler.SpawnVehicle(Victim.AbovePosition, Victim.Heading - 180);
                SuspectVehicle.IsPersistent = true;
                SuspectVehicle.IsDeformationEnabled = true;
                SuspectVehicle.Deform(SuspectVehicle.GetPositionOffset(SuspectVehicle.GetBonePosition("door_dside_f")), 100f, 600f);

                Suspect = SuspectVehicle.CreateRandomDriver();
                Suspect.IsPersistent = true;
                Suspect.BlockPermanentEvents = true;

                AreaBlip = new Blip(MainSpawnPoint, 15);
                AreaBlip.IsRouteEnabled = true;
                AreaBlip.Color = Color.Orange;
                AreaBlip.Alpha = 0.69f;

                if (MainScenario == 0) //not on scene
                {
                    if (Suspect.DistanceTo(player) < 200f)
                    {
                        SuspectVehicle.Position = World.GetNextPositionOnStreet(player.Position.Around(400));
                        Game.LogTrivial("YOBBINCALLOUTS: SUSPECT TOO CLOSE, WARPING");
                    }
                    Suspect.Tasks.CruiseWithVehicle(15f, VehicleDrivingFlags.FollowTraffic);

                    //Spawn Witness
                    NativeFunction.Natives.xA0F8A7517A273C05<bool>(World.GetNextPositionOnStreet(MainSpawnPoint), 0, out Vector3 outPosition);
                    Witness = new Citizen(CallHandler.SpawnOnSreetSide(outPosition), 69);  //might have to specify witness model
                    Witness.IsPersistent = true;
                    Witness.BlockPermanentEvents = true;

                    Vector3 dir = (player.Position - Witness.Position); //thanks Albo!
                    Witness.Tasks.AchieveHeading(MathHelper.ConvertDirectionToHeading(dir)).WaitForCompletion(1100);
                    Witness.Tasks.PlayAnimation("friends@frj@ig_1", "wave_a", 1.1f, AnimationFlags.Loop);
                }
                else //on scene
                {
                    SuspectVehicle.Position = Victim.GetOffsetPositionFront(5); //test
                    SuspectVehicle.IndicatorLightsStatus = VehicleIndicatorLightsStatus.Both;
                }
            }
            catch (Exception e) //standard error message for callout initializing
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
            if (!CalloutRunning) { Callout(); } //Call the Callout method itself
            return base.OnCalloutAccepted();
        }
        public override void OnCalloutNotAccepted()
        {
            Game.LogTrivial("YOBBINCALLOUTS: Pedestrian Hit By Vehicle Callout Not Accepted by User.");
            //Functions.PlayScannerAudio("OTHER_UNIT_TAKING_CALL_02");
            base.OnCalloutNotAccepted();
        }
        //For Callout logic
        private void Callout()
        {
            CalloutRunning = true; //set the callout to be running
            GameFiber.StartNew(delegate //start a new GameFiber
            {
                try
                {
                    while (CalloutRunning) //similar to lspfr's Process() method
                    {
                        //Victim.Health = ((int)((int)Victim.FatalInjuryHealthThreshold - 1f));
                        if (Victim.Exists()) Victim.Kill();
                        Game.LogTrivial("YOBBINCALLOUTS: Pedestrian Killed");

                        while (player.DistanceTo(MainSpawnPoint) >= 20f && !Game.IsKeyDown(Config.CalloutEndKey)) { GameFiber.Wait(0); }
                        if (Game.IsKeyDown(Config.CalloutEndKey)) { EndCalloutHandler.CalloutForcedEnd = true; break; }
                        if (Main.CalloutInterface) CalloutInterfaceHandler.SendMessage(this, "Unit on Scene.");

                        //These seem to get removed when the Ped is treated by STP first aid/EMS
                        NativeFunction.Natives.APPLY_PED_DAMAGE_PACK(Victim, "SCR_TracySplash", 1.0f, 1.0f);
                        NativeFunction.Natives.APPLY_PED_DAMAGE_PACK(Victim, "Car_Crash_Light", 1.0f, 1.0f);
                        NativeFunction.Natives.APPLY_PED_DAMAGE_PACK(Victim, "HOSPITAL_9", 1.0f, 1.0f);
                        NativeFunction.Natives.APPLY_PED_DAMAGE_PACK(Victim, "Dirt_Dry", 1.0f, 1.0f);

                        if (MainScenario == 0) SuspectNotOnScene();
                        else SuspectOnScene();

                        break; //break out of the callout loop when done
                    }
                    GameFiber.Wait(2000);
                    Game.LogTrivial("YOBBINCALLOUTS: Callout Finished, Ending...");
                    EndCalloutHandler.EndCallout(); //call the End Callout Handler (you don't need to worry about this)
                    End();
                }
                catch (System.Threading.ThreadAbortException)
                {
                    Game.LogTrivial("YOBBINCALLOUTS: THREADABORTEXCEPTION CAUGHT. Usually not a big deal, caused by another plugin/crash somewhere else.");
                }
                catch (Exception e) //similar error catching to before
                {
                    if (CalloutRunning) //if the callout is currently running
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
                    else //sometimes, an error will be thrown if the callout is no longer running, especially if it was forced finished early
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
        public override void End() //cleanup
        {
            base.End();
            if (CalloutRunning) //play and display a Code 4 message iff the callout is still running
            {
                Game.DisplayNotification("~g~Code 4~w~, return to patrol.");
                Functions.PlayScannerAudio("ATTENTION_ALL_UNITS WE_ARE_CODE_4");
            }
            CalloutRunning = false; //once this is done, set the callout to no longer running
            if (SuspectBlip.Exists()) { SuspectBlip.Delete(); } //delete anything else
            if (VictimBlip.Exists()) VictimBlip.Delete();
            if (AreaBlip.Exists()) AreaBlip.Delete();
            if (WitnessBlip.Exists()) WitnessBlip.Delete();
            Game.LogTrivial("YOBBINCALLOUTS: Pedestrian Hit by Vehicle Callout Finished Cleaning Up."); //log it
        }
        public override void Process() //you don't need this but if you want to use it instead of my method, go ahead
        {
            base.Process();
        }
        private void SuspectNotOnScene()
        {
            if (CalloutRunning)
            {
                if (AreaBlip.Exists()) AreaBlip.Delete();
                VictimBlip = CallHandler.AssignBlip(Victim, Color.Orange, 1f, "Victim");
                WitnessBlip = CallHandler.AssignBlip(Witness, Color.Blue, 1f, "Witness");
                Game.DisplayHelp("Tend to the ~o~Pedestrian~w~. Speak with the ~b~Witness~w~ when Ready.");
                while (player.DistanceTo(Witness) >= 2f) GameFiber.Wait(0);

                if (Config.DisplayHelp) Game.DisplayHelp("Press ~y~" + Config.MainInteractionKey + " ~w~to Speak with the ~b~Witness.");
                Witness.Tasks.AchieveHeading(player.Heading - 180f).WaitForCompletion(500);
                if (VictimBlip.Exists()) VictimBlip.Delete();
                int OpeningDialogue = CallHandler.RNG(0, 3); //change later
                if (OpeningDialogue == 0)  CallHandler.Dialogue(WitnessDriverAtFault1, Witness);
                else if (OpeningDialogue == 1) CallHandler.Dialogue(WitnessDriverAtFault2, Witness);
                else CallHandler.Dialogue(WitnessDriverAtFault3, Witness);
                if (SuspectVehicle.Exists())
                {
                    Witness.Tasks.PlayAnimation("missfbi3_party_d", "stand_talk_loop_a_male1", -1, AnimationFlags.Loop);
                    Game.DisplaySubtitle("~b~Caller:~w~ The Vehicle was a ~b~" + CallHandler.GetSetVehicleColor(SuspectVehicle) + "-Colored ~r~" + SuspectVehicle.Model.Name + ".", 4000); //fix color
                    if (Main.CalloutInterface)
                    {
                        CalloutInterfaceHandler.SendMessage(this, "Suspect Vehicle reported as a " + CallHandler.GetSetVehicleColor(SuspectVehicle) + "-colored " + SuspectVehicle.Model.Name + ".");
                    }
                }
                else
                {
                    SuspectVehicleDescription = "~b~Witness:~w~ No, sorry. It all happened so fast and I can't remember!";
                    Game.LogTrivial("YOBBINCALLOUTS: SUSPECTVEHICLE INVALID.");
                }
                GameFiber.Wait(1500);
                if (Witness.Exists())
                {
                    Witness.Dismiss();
                    if (WitnessBlip.Exists()) WitnessBlip.Delete();
                }
                Game.DisplayHelp("Start Searching for the ~r~Suspect.");
                Search();
            }
        }
        private void SuspectOnScene()
        {
            if (CalloutRunning)
            {
                if (AreaBlip.Exists()) AreaBlip.Delete();
                VictimBlip = CallHandler.AssignBlip(Victim, Color.Orange, 1f, "Victim");
                SuspectBlip = CallHandler.AssignBlip(Suspect, Color.Red, 1f, "Driver");
                Game.DisplayHelp("Tend to the ~o~Pedestrian~w~. Speak with the ~r~Driver~w~ when Ready.");

                if (CallHandler.FiftyFifty()) //cooperates
                {
                    Game.LogTrivial("YOBBINCALLOUTS: Suspect Cooperates.");
                    while (player.DistanceTo(Suspect) >= 2f) GameFiber.Wait(0);

                    if (Config.DisplayHelp) Game.DisplayHelp("Press ~y~" + Config.MainInteractionKey + " ~w~to Speak with the ~r~Driver.");
                    if (VictimBlip.Exists()) VictimBlip.Delete();
                    try { NativeFunction.Natives.ROLL_DOWN_WINDOW(SuspectVehicle, 0); }
                    catch { Game.LogTrivial("YOBBINCALLOUTS: Error Rolling Down Driver Window."); }
                    int OpeningDialogue = CallHandler.RNG(0, 3); //change later
                    if (OpeningDialogue == 0) CallHandler.Dialogue(DriverDriverNotAtFault1);
                    else if (OpeningDialogue == 1) CallHandler.Dialogue(DriverDriverNotAtFault2);
                    else CallHandler.Dialogue(DriverDriverNotAtFault3);

                    Game.DisplayHelp("Deal With the ~r~Driver. ~w~Press ~y~" + Config.CalloutEndKey + " ~w~When Finished.");
                    while (!Game.IsKeyDown(Config.CalloutEndKey)) GameFiber.Wait(0);
                    //if (Suspect.Exists()) if (Suspect.IsAlive) Game.DisplayNotification("Dispatch, We Have ~b~Arrested~w~ the Suspect.");
                }
                else //flees
                {
                    Game.LogTrivial("YOBBINCALLOUTS: Suspect Flees.");

                    int WaitTime = CallHandler.RNG(2500, 8000);
                    GameFiber.Wait(WaitTime);

                    if (Suspect.Exists() && Suspect.IsAlive)
                    {
                        CallHandler.CreatePursuit(MainPursuit, true, true, false, Suspect);
                        CallHandler.SuspectWait(Suspect);
                    }
                }
            }
        }
        private void Search()
        {
            if (CalloutRunning)
            {
                if (!SuspectVehicle.Exists())
                {
                    Game.LogTrivial("YOBBINCALLOUTS: Suspectvehicle does not exist. creating new suspect and vehicle.");
                    SuspectVehicle = CallHandler.SpawnVehicle(World.GetNextPositionOnStreet(player.Position.Around(300)), 69);
                    if (Suspect.Exists())
                    {
                        Suspect.Delete();
                        Suspect = SuspectVehicle.CreateRandomDriver();
                        Suspect.IsPersistent = true;
                        Suspect.BlockPermanentEvents = true;
                    }
                }
                if (SuspectVehicle.DistanceTo(player) > 500) SuspectVehicle.Position = World.GetNextPositionOnStreet(player.Position.Around(300));

                SuspectBlip = new Blip(Suspect.Position.Around(15), 150f);
                SuspectBlip.Color = Color.Orange;
                SuspectBlip.Alpha = 0.5f;
                SuspectBlip.IsRouteEnabled = true;
                GameFiber.Wait(1500);

                while (player.DistanceTo(Suspect) >= 100) GameFiber.Wait(0);
                SuspectVehicle.IsDriveable = true;
                SuspectVehicle.IsVisible = true;
                Suspect.Tasks.CruiseWithVehicle(SuspectVehicle, 17, VehicleDrivingFlags.DriveAroundVehicles);
                Functions.PlayScannerAudio("ATTENTION_ALL_UNITS_01");   //change
                GameFiber.Wait(1000);
                if (Main.CalloutInterface)
                {
                    CalloutInterfaceHandler.SendMessage(this, "Caller spotted the suspect driving recklessly, updating map.");
                }
                Game.DisplayNotification("~b~Update:~w~ A Caller Has ~y~Spotted~w~ the ~r~Suspect~w~ Driving Recklessly. ~g~Updating Map.");    //fix this, too hard to see suspect. maybe remind them what the car looks like.
                GameFiber.Wait(1000);

                while (player.DistanceTo(Suspect) >= 25) GameFiber.Wait(0);
                if (SuspectBlip.Exists()) SuspectBlip.Delete();
                SuspectBlip = Suspect.AttachBlip();
                SuspectBlip.IsFriendly = false;
                SuspectBlip.Scale = 0.75f;
                GameFiber.Wait(1500);
                Game.DisplaySubtitle("Dispatch, We Have Located the ~r~Suspect!");
                GameFiber.Wait(1500);
                Game.DisplayHelp("Arrest the ~r~Suspect.");
                GameFiber.Wait(2000);

                if (CallHandler.FiftyFifty())
                {
                    if (CallHandler.FiftyFifty()) //pursuit
                    {
                        Game.LogTrivial("YOBBINCALLOUTS: Pursuit.");
                        CallHandler.CreatePursuit(MainPursuit, true, true, true, Suspect);
                    }
                    else //shoots
                    {
                        Game.LogTrivial("YOBBINCALLOUTS: Suspect attacks.");
                        int wait = CallHandler.RNG(2000, 6900);
                        GameFiber.Wait(wait);
                        Suspect.Inventory.GiveNewWeapon("WEAPON_ASSAULTRIFLE", -1, true);
                        Suspect.Tasks.ParkVehicle(SuspectVehicle, SuspectVehicle.Position, SuspectVehicle.Heading).WaitForCompletion(5000);
                        Suspect.Tasks.LeaveVehicle(SuspectVehicle, LeaveVehicleFlags.LeaveDoorOpen).WaitForCompletion();
                        Suspect.Tasks.AchieveHeading(Game.LocalPlayer.Character.LastVehicle.Heading - 180).WaitForCompletion(1500);
                        Suspect.Tasks.AimWeaponAt(Game.LocalPlayer.Character.Position, 1500).WaitForCompletion();   //test this
                        Suspect.Tasks.FightAgainst(Game.LocalPlayer.Character, -1);
                        if (LSPD_First_Response.Mod.API.Functions.IsPlayerPerformingPullover()) { LSPD_First_Response.Mod.API.Functions.ForceEndCurrentPullover(); }
                        GameFiber.Wait(2000);
                        CallHandler.SuspectWait(Suspect);
                    }
                }
                else //cooperates
                {
                    Game.LogTrivial("YOBBINCALLOUTS: Cooperates.");
                    CallHandler.SuspectWait(Suspect);
                }
            }
        }
    }
}
