//BROKEN DOWN VEHICLE Callout by YobB1n, YobbinCallouts
//*ORIGINALLY UPDATED: July 22, 2020
//*REFACTORED: April 24, 2022
//*OBJECTIVE: Broken Down Vehicle blocking road, player must talk to the driver, then call a tow truck and clear the road.
//*REMARKS: This code is far from perfect, no bugs/crashes in my testing but it is certainally inefficient and there probably are unused variables and things that could be better. I'm new to this, I apologize.
//*Some Commented out code is for a future implementation of this code where I add an event where the player has the option to drive the Driver back to their house.
//*Comments, Remarks, or Questions, as well as Patches please use the LSPDFR Plugin Page for this Callout.
//*Thank You!
//*This was my first callout ever :)

//To-do: Detect STP, Use STP when 'Y' Pressed, get rid of old variables, finish vehicle old and vehicle exploded scenarios

using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.Drawing;
//using StopThePed.API;

namespace YobbinCallouts.Callouts
{
    [CalloutInfo("Broken Down Vehicle", CalloutProbability.High)]

    public class BrokenDownVehicle : Callout
    {
        private Ped Driver;
        private Ped TowTruckDriver;
        private Ped player = Game.LocalPlayer.Character;

        private Vector3 SpawnPoint;
        private Vector3 SpawnPointTruck;
        private Vector3 DriverDestination;
        private Vector3 DriverHood;

        private Blip DriverBlip;
        private Blip DriverVehicleBlip;
        private Blip TowTruckBlip;

        private Vehicle DriverVehicle;
        private Vehicle TowTruck;

        private bool TowTruckWarped = false;
        private string Zone;
        private int SeatIndex;
        private float VehicleHeading;
        private int Scenario;

        private bool CalloutRunning = false;

        private readonly List<string> OpeningDialogue1 = new List<string>()
        {
         "~b~Driver:~w~ Thanks for getting here so quickly, officer!",
         "~g~You:~w~ No problem, what seems to be the issue?",
         "~b~Driver:~w~ My car just stopped in the middle of the road! No warning, it just died!",
         "~g~You:~w~ Wow, that's quite bad luck! you're in a very dangerous spot, we have to move you're vehicle as soon as possible.",
         "~b~Driver:~w~ I know! I have no idea what's wrong with it, i've tried restarting the engine, but it never works!",
         "~g~You:~w~ Okay, let me sort this out. how about you stand by my car for the time being?",
         "~b~Driver:~w~ Sounds good, officer.",
        };
        private readonly List<string> OpeningDialogue2 = new List<string>()
        {
            "~b~Driver:~w~ Thank god you're here officer! My car died on me!",
            "~g~You:~w~ Were there any signs of damage before it stopped working?",
            "~b~Driver:~w~ No officer, one minute it was working fine, the next it stopped! I didn't have time to react!",
            "~g~You:~w~ Okay, calm down. we have to move your car as soon as possible.",
            "~g~You:~w~ I'll see what I can do. can you stand over there by my car, to stay out of traffic?",
            "~b~Driver:~w~ Of course officer. Thanks so much!",
        };
        private readonly List<string> OpeningDialogue3 = new List<string>()
        {
            "~b~Driver:~w~ Hey Officer, sorry about the inconvience my vehicle has caused here.",
            "~g~You:~w~ What happened to it?",
            "~b~Driver:~w~ It just died on me! It's been giving me problems for a while and today it boiled over.",
            "~g~You:~w~ Well, you're vehicle is quite old. you're probably going to have to get a new one after this!",
            "~b~Driver:~w~ , I know! anyways, my phone died, so I didn't call anyone to fix it yet. could you sort that out for me?",
            "~g~You:~w~ Of course. could you do me a favour and go over to my car, so you can get off the road?",
            "~b~Driver:~w~ Yeah sure. thanks!",
        };
        private readonly List<string> OpeningDialogue4 = new List<string>()
        {
            "~b~Driver:~w~ Thank God You're Here Officer! My Car Overheated!",
            "~g~You:~w~ Is it Possible to Move it Out of the Way, or is it Dead?",
            "~b~Driver:~w~ It Doesn't Move at All. I Was Really Scared to Leave My Vehicle in the Middle of Traffic Like This!",
        };
        private int OpeningDialogueCount;
        private readonly List<string> TowTruckDialogue1 = new List<string>()
        {
            "~g~You:~w~ Thanks for getting here so quickly!",
            "~b~Tow Truck Driver:~w~ Not a problem. Quite a bad place to break down, Eh?",
            "~g~You:~w~ Yes, the Driver said the vehicle just died for no apparent reason.",
            "~b~Tow Truck Driver:~w~ Well, I'll get this out of the road now, Officer!",
        };
        private readonly List<string> DriverEndingDialogue1 = new List<string>()
        {
            "~b~Driver:~w~ ~Wow, that was really stressful! Thanks again for your help, Officer!",
            "~g~You:~w~ No worries! You'll get a phone call later today with all the information you need regarding your vehicle.",
            "~b~Driver:~w~ Alright, sounds good. Take care!",
        };
        private readonly List<string> DriverEndingDialogue2 = new List<string>()
        {
            "~b~Driver:~w~ Appreciate the help Officer!",
            "~g~You:~w~ Of course! You should get a phone call soon with all the details regarding your car.",
            "~b~Driver:~w~ Sounds good officer. have a great day!",
        };
        private readonly List<string> DriverEndingDialogue3 = new List<string>()
        {
            "~b~Driver:~w~ Wow, that was really stressful! Thanks again for your help, officer!",
            "~g~You:~w~ No worries! You'll get a phone call later today with all the information you need regarding your vehicle.",
            "~b~Driver:~w~ Alright, sounds good. Mind if I ask you for another favor, officer?",
            "~g~You:~w~ Go ahead.",
            "~b~Driver:~w~ Could I get a lift to my place? It isn't very far away. I would really appreciate it!",
            "~b~Driver:~w~ Don't worry if you can't, I know you guys are pretty busy these days.",
        };
        private readonly List<string> DriverEndingDialogue4 = new List<string>()
        {
            "~b~Driver:~w~ Appreciate the help officer!",
            "~g~You:~w~ Of course! You should get a phone call soon with all the details regarding your car.",
            "~b~Driver:~w~ Sounds good officer. Could I ask one more thing of you, officer?",
            "~g~You:~w~ Go for it.",
            "~b~Driver:~w~ I'm running late to get back home, do you think you could give me a ride there?",
            "~b~Driver:~w~ It's not too far away. No worries if you can't, I know you're busy and all.",
        };
        private readonly List<string> FixCarDialogue = new List<string>()
        {
            "~g~You:~w~ I managed to get your car working again. Turned out your battery died.",
            "~b~Driver:~w~ Oh no! Do I have to get a new one?",
            "~g~You:~w~ I jumped your car, so you should be good to go.",
            "~b~Driver:~w~ Oh thank god, you're a lifesaver. Thanks so much officer.",
            "~g~You:~w~ No worries, drive safe!",
        };

        public override bool OnBeforeCalloutDisplayed()
        {
            Game.LogTrivial("==========YOBBINCALLOUTS: Broken Down Vehicle Callout Start==========");
            Vector3 Spawn = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(650f));
            try
            {
                NativeFunction.Natives.GetClosestVehicleNodeWithHeading(Spawn, out Vector3 nodePosition, out float heading, 1, 3.0f, 0);
                bool success = NativeFunction.Natives.xA0F8A7517A273C05<bool>(Spawn, heading, out Vector3 outPosition);
                if (success)
                {
                    SpawnPoint = outPosition;
                    VehicleHeading = heading;
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
            Game.LogTrivial("YOBBINCALLOUTS: Sucessfuly Found Spawn Point");
            ShowCalloutAreaBlipBeforeAccepting(SpawnPoint, 75f);    //Callout Blip Circle with radius 70m
            AddMinimumDistanceCheck(50f, SpawnPoint);   //Player must be 50m or further away

            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("CITIZENS_REPORT CRIME_MOTOR_VEHICLE_ACCIDENT_01"); //Default
            CalloutMessage = "Broken Down Vehicle";
            CalloutPosition = SpawnPoint;
            CalloutAdvisory = "A Car Has Reportedly ~r~Stalled~w~ in the Middle of Traffic.";
            Zone = Functions.GetZoneAtPosition(Game.LocalPlayer.Character.Position).GameName;
            Game.LogTrivial("YOBBINCALLOUTS: Zone is " + Zone);

            return base.OnBeforeCalloutDisplayed();
        }
        public override bool OnCalloutAccepted()
        {
            if (Main.CalloutInterface)
            {
                CalloutInterfaceHandler.SendCalloutDetails(this, "CODE 2", "");
            }
            else
            {
                Game.DisplayNotification("Respond ~b~Code 2~w~");
            }
            DriverVehicle = CallHandler.SpawnVehicle(SpawnPoint, VehicleHeading);
            //DriverVehicle.Position = DriverVehicle.GetOffsetPositionRight(3.5f);
            DriverVehicle.IsPersistent = true;
            DriverVehicle.IsEngineOn = false;
            DriverVehicle.EngineHealth = 0;
            DriverVehicle.IsDriveable = false;
            DriverVehicle.IsInvincible = true;
            DriverVehicle.IndicatorLightsStatus = VehicleIndicatorLightsStatus.Both;
            Game.LogTrivial("YOBBINCALLOUTS: Driver Vehicle Spawned");

            Driver = DriverVehicle.CreateRandomDriver();
            RandomCharacter.RandomizeCharacter(Driver); //test
            Driver.IsPersistent = true;
            Driver.BlockPermanentEvents = true;
            Driver.Tasks.CruiseWithVehicle(0f);
            Game.LogTrivial("YOBBINCALLOUTS: Driver Spawned");

            DriverVehicleBlip = DriverVehicle.AttachBlip();
            DriverVehicleBlip.Color = Color.Yellow;
            DriverVehicleBlip.IsRouteEnabled = true;
            DriverVehicleBlip.Name = "Broken Down Vehicle";

            if (CalloutRunning == false) { Callout(); }
            return base.OnCalloutAccepted();
        }
        public override void OnCalloutNotAccepted()
        {
            Game.LogTrivial("YOBBINCALLOUTS: Broken Down Vehicle Callout Not Accepted by User");
            Functions.PlayScannerAudio("OTHER_UNIT_TAKING_CALL_01");    //Add more later
            base.OnCalloutNotAccepted();
        }
        private void Callout()
        {
            CalloutRunning = true;
            GameFiber.StartNew(delegate
            {
                try
                {
                    // on scene
                    while (CalloutRunning)
                    {
                        GameFiber.Yield();
                        while (player.DistanceTo(Driver) >= 30f) GameFiber.Wait(0);

                        if (Config.DisplayHelp) Game.DisplayHelp("Park Up Beside the ~y~Vehicle~w~, Then Approach When Ready.");
                        if (Main.CalloutInterface) CalloutInterfaceHandler.SendMessage(this, "UNIT ON SCENE. Reporting one stalled vehicle, model " + DriverVehicle.Model.Name);

                        DriverBlip = new Blip(Driver);
                        DriverBlip.Scale = 0.8f;
                        DriverBlip.Color = Color.Blue;
                        DriverBlip.Name = "Driver";
                        Random r2 = new Random();  //Instantiate Random scenario generator
                        Scenario = r2.Next(0, 2);    //Use Random scenario generator
                        while (player.DistanceTo(Driver) >= 6f) GameFiber.Wait(0);

                        // talking to driver
                        Driver.Tasks.LeaveVehicle(DriverVehicle, LeaveVehicleFlags.None).WaitForCompletion();
                        NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(Driver, player, -1);
                        if (Config.DisplayHelp == true)
                        {
                            Game.DisplayHelp("Press ~y~" + Config.MainInteractionKey + " ~w~to Talk to the ~b~Driver.");
                        }

                        Random ODC = new Random();
                        int OpeningDialogue = ODC.Next(0, 3);
                        if (OpeningDialogue == 0) CallHandler.Dialogue(OpeningDialogue1, Driver);
                        else if (OpeningDialogue == 1) CallHandler.Dialogue(OpeningDialogue2, Driver);
                        else CallHandler.Dialogue(OpeningDialogue3, Driver);

                        GameFiber.Wait(1500);

                        if (player.LastVehicle.Exists())
                        {
                            Driver.Tasks.FollowNavigationMeshToPosition(player.LastVehicle.GetOffsetPositionRight(1f), (Game.LocalPlayer.Character.Heading) - 180, 1.25f).WaitForCompletion();
                        }
                        CallHandler.IdleAction(Driver, false);

                        Game.DisplayHelp("~y~" + Config.Key1 + ":~b~ Call a Tow Truck.~y~ " + Config.Key2 + ":~b~ Try to Fix the Car Yourself.");
                        while (!Game.IsKeyDown(Config.Key1) && !Game.IsKeyDown(Config.Key2)) GameFiber.Wait(0);
                        if (Game.IsKeyDown(Config.Key1))
                        {
                            TowTruckLogic();
                        }
                        else
                        {
                            FixCar();
                        }
                        break;
                    }
                    Game.LogTrivial("YOBBINCALLOUTS: Callout Finished, Ending...");
                    if (CalloutRunning) EndCalloutHandler.EndCallout();
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
            }
            );
        }
        public override void End()
        {
            base.End();

            if (CalloutRunning)
            {
                Game.DisplayNotification("~g~Code 4~w~, return to patrol.");
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("ATTENTION_ALL_UNITS WE_ARE_CODE_4");
            }

            CalloutRunning = false;
            if (Driver.Exists())
            {
                if (Driver.IsInAnyVehicle(false)) Driver.Tasks.LeaveVehicle(Driver.CurrentVehicle, LeaveVehicleFlags.None).WaitForCompletion(4000);
                Driver.Dismiss();
            }
            if (DriverVehicle.Exists()) { DriverVehicle.Dismiss(); }
            if (DriverBlip.Exists()) { DriverBlip.Delete(); }
            if (DriverVehicleBlip.Exists()) { DriverVehicleBlip.Delete(); }
            if (TowTruckDriver.Exists()) { TowTruckDriver.Dismiss(); }
            if (TowTruckBlip.Exists()) { TowTruckBlip.Delete(); }

            Game.LogTrivial("YOBBINCALLOUTS: Broken Down Vehicle Callout Cleaned Up");
        }
        public override void Process()
        {
            base.Process();
        }
        private void TowTruckLogic()
        {
            if (Main.STP == false)
            {
                Game.DisplayHelp("Press " + Config.MainInteractionKey + " to Call a ~b~Tow Truck.");
                while (!Game.IsKeyDown(Config.MainInteractionKey) && DriverVehicle.Speed < 1f) { GameFiber.Wait(0); }
                if (Main.CalloutInterface) CalloutInterfaceHandler.SendMessage(this, "Requesting Tow Truck to Clear Disabled Veicle.");
                Game.DisplaySubtitle("Hey Dispatch, We Need a Tow Truck ASAP, Vehicle Blocking the Road.", 3500);
                Game.LocalPlayer.Character.Tasks.PlayAnimation("random@arrests", "generic_radio_chatter", -1, AnimationFlags.SecondaryTask | AnimationFlags.UpperBodyOnly).WaitForCompletion(4000);
                GameFiber.Wait(3000);
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("REPORT_RESPONSE_COPY_02");  //REPORT RESPONSE
                GameFiber.Wait(1000);
                if (Main.CalloutInterface) CalloutInterfaceHandler.SendMessage(this, "Tow Truck is en Route.");
                else Game.DisplayHelp("Tow Truck is ~g~En Route!", 3500);
                SpawnPointTruck = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(200f));   //Tow Truck spawned around 250 m from player
                TowTruck = new Vehicle("TOWTRUCK", SpawnPointTruck, 180f);
                TowTruck.IsPersistent = true;
                Game.LogTrivial("YOBBINCALLOUTS: Tow Truck Has Been Spawned");
                TowTruckDriver = TowTruck.CreateRandomDriver();
                TowTruckDriver.IsPersistent = true;
                TowTruckDriver.BlockPermanentEvents = true;
                TowTruckBlip = TowTruck.AttachBlip();
                TowTruckBlip.IsFriendly = true;
                TowTruckBlip.Name = "Tow Truck";
                TowTruckDriver.Tasks.DriveToPosition(DriverVehicle.GetOffsetPositionFront(7), 25, VehicleDrivingFlags.Emergency, 15);
                if (TowTruck.Exists())
                {
                    GameFiber.Wait(6000);
                    Game.DisplayHelp("~b~Tow Truck~w~ Taking Too Long? Press " + Config.MainInteractionKey + " To ~g~Warp~w~ it Closer.");
                    while (TowTruck.DistanceTo(DriverVehicle) >= 30f && !Game.IsKeyDown(Config.MainInteractionKey)) { GameFiber.Wait(0); }
                    if (TowTruck.DistanceTo(DriverVehicle) >= 30f && Game.IsKeyDown(Config.MainInteractionKey))
                    {
                        Game.DisplayNotification("~y~Warping~w~ Tow Truck Now, Please Wait.");
                        GameFiber.Wait(2000);
                        TowTruck.Position = DriverVehicle.GetOffsetPositionFront(7f);
                        TowTruck.Heading = DriverVehicle.Heading;
                        TowTruckWarped = true;
                    }
                    while (TowTruckWarped == false && TowTruck.DistanceTo(DriverVehicle) >= 10f && TowTruck.DistanceTo(DriverVehicle) <= 30f)   //if tow truck isnt warped, botches
                    {
                        GameFiber.Wait(0);
                    }
                    if (TowTruckWarped == false && TowTruck.DistanceTo(DriverVehicle) <= 10f)
                    {
                        TowTruck.Position = DriverVehicle.GetOffsetPositionFront(10f);
                        TowTruck.Heading = DriverVehicle.Heading;
                        TowTruckWarped = true;
                    }
                    GameFiber.Wait(2000);
                    TowTruckDriver.Tasks.LeaveVehicle(TowTruck, LeaveVehicleFlags.LeaveDoorOpen).WaitForCompletion();
                    TowTruckDriver.Tasks.ClearImmediately();
                    GameFiber.Wait(2000);
                    Game.DisplayHelp("Talk to the ~b~Tow Truck Driver.");
                    while (Game.LocalPlayer.Character.DistanceTo(TowTruckDriver) >= 2.5f) { GameFiber.Wait(0); }
                    TowTruckDriver.Tasks.AchieveHeading(Game.LocalPlayer.Character.Heading - 180).WaitForCompletion(750); ;
                    if (Config.DisplayHelp == true)
                    {
                        Game.DisplayHelp("Press ~y~" + Config.MainInteractionKey + " ~w~to Advance the ~b~Tow Truck Driver.");
                    }

                    CallHandler.Dialogue(TowTruckDialogue1, TowTruckDriver);

                    GameFiber.Wait(2500);
                    Game.DisplaySubtitle("~b~I'm Attaching the Vehicle in Three Seconds, Please Stand Clear!");
                    GameFiber.Wait(3000);
                    TowTruck.TowVehicle(DriverVehicle, true);
                    GameFiber.Wait(1000);
                    TowTruckDriver.Dismiss();
                    if (TowTruckBlip.Exists()) { TowTruckBlip.Delete(); }
                    if (DriverVehicleBlip.Exists()) { DriverVehicleBlip.Delete(); }
                }
            }
            else  //CHANGE
            {
                //Game.DisplayHelp("Press " + Config.MainInteractionKey + " to Use ~y~StopThePed~w~ to Call a ~b~Tow Truck.");
                //while (!Game.IsKeyDown(Config.MainInteractionKey)) GameFiber.Wait(0);
                try
                {
                    StopThePed.API.Functions.callTowService();  //breaks for some reason
                }
                catch (Exception e)
                {
                    Game.LogTrivial("==========YOBBINCALLOUTS: ERROR CAUGHT IN CALLING STOPTHEPED TOW SERVICE==========");
                    Game.LogTrivial("IN: " + this);
                    Game.LogTrivial("EXCEPTION: " + e);
                    Game.DisplayHelp("Use ~y~StopThePed~w~ to Call a ~b~Tow Truck.");
                }

                //Game.LogTrivial("YOBBINCALLOUTS: STP Tow Vehicle Prompt Shown");
                //GameFiber.Wait(1000);
                while (DriverVehicle.DistanceTo(SpawnPoint) <= 5) { GameFiber.Wait(0); }
                if (DriverVehicleBlip.Exists()) DriverVehicleBlip.Delete(); // could break?
            }
            GameFiber.Wait(1500);
            if (Main.CalloutInterface) CalloutInterfaceHandler.SendMessage(this, "Tow Truck has Cleared the Disabled Vehicle.");
            else Game.DisplayNotification("Tow Truck has Collected ~g~Vehicle,~w~ Towing to Repair Lot.");
            Ending();
        }
        private void FixCar()
        {
            if (CalloutRunning)
            {
                Game.LogTrivial("YOBBINCALLOUTS: Player Has Started Attempting to Repair Vehicle.");
                DriverVehicleBlip.Delete();
                Game.DisplayHelp("Go to the ~b~Hood~w~ of the Vehicle to Try and ~y~Repair~w~ it.");
                if (DriverVehicle.HasBone("bonnet"))
                {
                    DriverHood = DriverVehicle.GetBonePosition("bonnet");
                    Game.LogTrivial("YOBBINCALLOUTS: Driver Vehicle Hood Position at " + DriverHood + ".");
                }
                else
                {
                    Game.DisplayNotification("Sorry, there was an ~r~Error~w~. Please Call a ~b~Tow Truck~w~ instead.");
                    Game.LogTrivial("YOBBINCALLOUTS: Could not find bone for driver's vehicle. Player Must Call Tow Truck.");
                    TowTruckLogic();
                }
                DriverVehicleBlip = new Blip(DriverHood);
                DriverVehicleBlip.Scale = 0.5f;
                DriverVehicleBlip.IsFriendly = true;
                while (Game.LocalPlayer.Character.DistanceTo2D(DriverVehicle.FrontPosition) >= 1.5f) GameFiber.Wait(0);
                Game.LocalPlayer.Character.Tasks.FollowNavigationMeshToPosition(DriverVehicle.FrontPosition, 2, DriverVehicle.Heading - 180, 0.05f, -1).WaitForCompletion(1500);
                Game.LocalPlayer.Character.Heading = DriverVehicle.Heading - 180;
                Game.LogTrivial("YOBBINCALLOUTS: Player Has Started Looking at Vehicle's Hood.");
                //some task to open the hood
                if (DriverVehicle.Doors[4].IsValid())
                {
                    DriverVehicle.Doors[4].Open(false);
                    Game.LogTrivial("YOBBINCALLOUTS: Opened the Hood");
                }
                GameFiber.Wait(1000);
                Game.DisplayHelp("Press ~y~" + Config.MainInteractionKey + "~w~ to Look at the ~b~Engine.");
                while (!Game.IsKeyDown(Config.MainInteractionKey)) GameFiber.Wait(0);
                Game.LocalPlayer.Character.Tasks.PlayAnimation("mini@repair", "fixing_a_ped", -1, AnimationFlags.Loop).WaitForCompletion(5000);
                GameFiber.Wait(2000);
                Game.LocalPlayer.Character.Tasks.ClearImmediately();
                GameFiber.Wait(1500);
                if (Scenario == 0)
                {
                    DriverVehicle.Repair();
                    GameFiber.Wait(1000);
                    Game.DisplaySubtitle("~g~You:~w~ Looks Like That Worked!", 3500);
                    Game.LogTrivial("YOBBINCALLOUTS: Player fixed le car");
                    if (DriverVehicle.Doors[4].IsValid())
                    {
                        DriverVehicle.Doors[4].Close(false);
                        Game.LogTrivial("YOBBINCALLOUTS: Closed the Hood");
                    }
                    Game.DisplayHelp("Talk to the ~b~Driver.");
                    GameFiber.Wait(4500);

                    NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(Driver, Game.LocalPlayer.Character, -1);

                    while (Game.LocalPlayer.Character.DistanceTo(Driver) >= 5f) GameFiber.Wait(0);
                    Game.LogTrivial("YOBBINCALLOUTS: Player Started Talking with Driver After Fixing the Car.");
                    if (Config.DisplayHelp == true)
                    {
                        Game.DisplayHelp("Press ~y~" + Config.MainInteractionKey + " ~w~to Inform the ~b~Driver.");
                    }

                    CallHandler.Dialogue(FixCarDialogue, Driver);

                    GameFiber.Wait(500);
                    if (DriverBlip.Exists()) DriverBlip.Delete();
                    if (Driver.Exists()) Driver.Dismiss();
                    //End();
                }
                else
                {
                    GameFiber.Wait(1000);
                    Game.DisplaySubtitle("~g~You:~w~ Damn, Looks Like I Couldn't Get it Working.", 3500);
                    Game.LogTrivial("YOBBINCALLOUTS: Player did not fix le car");
                    if (DriverVehicle.Doors[4].IsValid())
                    {
                        DriverVehicle.Doors[4].Close(false);
                        Game.LogTrivial("YOBBINCALLOUTS: Closed the Hood");
                    }
                    if (DriverVehicleBlip.Exists()) DriverVehicleBlip.Scale = 0.8f;
                    TowTruckLogic();
                }
            }
        }
        private void Ending()
        {
            if (CalloutRunning)
            {
                GameFiber.Wait(1500);
                if (DriverVehicleBlip.Exists()) DriverVehicleBlip.Delete();

                while (Game.LocalPlayer.Character.DistanceTo(Driver) >= 2f) { Game.DisplayHelp("Talk to the ~b~Driver~w~ to Finish the Callout."); GameFiber.Wait(0); }
                Driver.Tasks.AchieveHeading(Game.LocalPlayer.Character.Heading - 180).WaitForCompletion(1250);  //different waitforcompletion value, 1250 instead of 750. Wanted to reduce the sense of urgency once the car is towed
                if (Config.DisplayHelp == true)
                {
                    Game.DisplayHelp("Press~y~ " + Config.MainInteractionKey + " ~w~to Talk to the ~b~Driver.");
                }
                System.Random r = new System.Random();
                int DriverEndingDialogue = r.Next(0, 4);    //change to (0, 4) later

                if (DriverEndingDialogue == 0) CallHandler.Dialogue(DriverEndingDialogue1, Driver);
                else if (DriverEndingDialogue == 1) CallHandler.Dialogue(DriverEndingDialogue2, Driver);
                else if (DriverEndingDialogue == 2) CallHandler.Dialogue(DriverEndingDialogue3, Driver);
                else CallHandler.Dialogue(DriverEndingDialogue4, Driver);

                GameFiber.Wait(2000);
                Driver.Tasks.ClearImmediately();
                if (DriverEndingDialogue >= 2)
                {
                    DrivePeep();
                }
            }
        }
        private void DrivePeep()
        {
            if (CalloutRunning)
            {
                Driver.Tasks.AchieveHeading(Game.LocalPlayer.Character.Heading - 180).WaitForCompletion(750);
                Game.DisplayHelp("Enter Your Vehicle Give the Driver a ~g~Ride~w~. To ~r~Decline~w~ the Ride, Press ~y~" + Config.CalloutEndKey);
                CallHandler.IdleAction(Driver, false);
                while (!Game.LocalPlayer.Character.IsInAnyPoliceVehicle && !Game.IsKeyDown(Config.CalloutEndKey)) { GameFiber.Wait(0); }
                if (Game.IsKeyDown(Config.CalloutEndKey))
                {
                    if (Driver.Exists())
                    {
                        Driver.Tasks.ClearImmediately();
                        Driver.ClearLastVehicle();
                        Driver.Dismiss();
                    }
                    else Game.DisplayNotification("Dispatch, ~b~Vehicle is Cleared.");
                    GameFiber.Wait(2000);
                    if (Main.CalloutInterface) CalloutInterfaceHandler.SendMessage(this, "Vehicle Has been Cleared, Code 4.");
                }
                else
                {
                    GameFiber.Wait(1000);
                    Game.LogTrivial("YOBBINCALLOUTS: Player Has Accepted the Ride.");
                    Game.DisplaySubtitle("~g~You:~w~ I Can Give You a Ride, Hop In!", 3000);
                    if (Main.CalloutInterface) CalloutInterfaceHandler.SendMessage(this, "Unit is Giving the Citizen a Ride Home.");
                    GameFiber.Wait(2000);
                    Game.DisplayHelp("~y~" + Config.Key1 + ": ~b~Tell the Driver to Enter the Passenger Seat. ~y~" + Config.Key2 + ":~b~ Tell the Driver to Enter the Rear Seat.");
                    while (!Game.IsKeyDown(Config.Key1) && !Game.IsKeyDown(Config.Key2)) GameFiber.Wait(0); //might do an animation?
                    if (Game.IsKeyDown(Config.Key1))
                    {
                        SeatIndex = (int)Game.LocalPlayer.Character.CurrentVehicle.GetFreePassengerSeatIndex();
                        Driver.Tasks.EnterVehicle(Game.LocalPlayer.Character.CurrentVehicle, SeatIndex, EnterVehicleFlags.None).WaitForCompletion();
                    }
                    else
                    {
                        SeatIndex = (int)Game.LocalPlayer.Character.CurrentVehicle.GetFreeSeatIndex(1, 2);
                        Driver.Tasks.EnterVehicle(Game.LocalPlayer.Character.CurrentVehicle, SeatIndex, EnterVehicleFlags.None).WaitForCompletion();
                    }
                    if (DriverBlip.Exists()) { DriverBlip.Delete(); }
                    CallHandler.nearestLocationChooser(CallHandler.getHouseList);
                    if (CallHandler.locationReturned) { DriverDestination = CallHandler.SpawnPoint; }  //boom
                    else
                    {
                        DriverDestination = World.GetNextPositionOnStreet(player.Position.Around(420f));
                    }
                    DriverBlip = new Blip(DriverDestination);
                    DriverBlip.Color = System.Drawing.Color.Green;
                    DriverBlip.IsRouteEnabled = true;
                    DriverBlip.Name = "Destination";
                    GameFiber.Wait(1000);
                    Game.DisplayHelp("Drive to the ~g~Location~w~ Marked on the Map.");
                    while (Driver.DistanceTo(DriverDestination) >= 35f && !Game.IsKeyDown(Config.CalloutEndKey)) { GameFiber.Wait(0); }
                    if (Game.IsKeyDown(Config.CalloutEndKey)) { End(); }
                    Game.DisplayHelp("Stop Your Vehicle to Let the ~b~Driver ~w~Out.");
                    while (Game.LocalPlayer.Character.CurrentVehicle.Speed > 0)
                    {
                        GameFiber.Wait(0);
                    }
                    Driver.PlayAmbientSpeech("generic_thanks");    //fix this later
                    Driver.Tasks.LeaveVehicle(Game.LocalPlayer.Character.CurrentVehicle, LeaveVehicleFlags.None).WaitForCompletion();
                    if (DriverBlip.Exists()) { DriverBlip.Delete(); }
                    GameFiber.StartNew(delegate
                    {
                        Driver.Tasks.FollowNavigationMeshToPosition(DriverDestination, 69, 1.25f, -1).WaitForCompletion();
                        //if (Driver.Exists()) Driver.Delete();
                    });
                    GameFiber.Wait(1000);
                    if (Main.CalloutInterface) CalloutInterfaceHandler.SendMessage(this, "Vehicle Has been Cleared, Code 4.");
                    else Game.DisplayNotification("Dispatch, ~b~Vehicle is Cleared.~w~ We Have ~b~Given the Driver~w~ a Ride From the Scene.");
                }
            }
        }
        private void CarFire()  //unused atm
        {
            Driver.Tasks.LeaveVehicle(DriverVehicle, LeaveVehicleFlags.None).WaitForCompletion();
            Driver.Tasks.AchieveHeading(Game.LocalPlayer.Character.Heading - 180).WaitForCompletion(750);
            if (Config.DisplayHelp == true)
            {
                Game.DisplayHelp("Press " + Config.MainInteractionKey + " to Advance the Conversation.");
            }
            System.Random FODC = new System.Random();
            int OpeningDialogue = FODC.Next(0, 0);
            switch (OpeningDialogue)
            {
                case 0:
                    while (OpeningDialogueCount < OpeningDialogue4.Count)
                    {
                        GameFiber.Yield();
                        if (Game.IsKeyDown(Config.MainInteractionKey))
                        {
                            Driver.Tasks.PlayAnimation("missfbi3_party_d", "stand_talk_loop_a_male2", -1, AnimationFlags.Loop);
                            Game.DisplaySubtitle(OpeningDialogue4[OpeningDialogueCount]);
                            OpeningDialogueCount++;
                        }
                    }
                    break;
            }
            DriverVehicle.EngineHealth = -1;
            DriverVehicle.IsOnFire = true;
            Game.DisplaySubtitle("Shit, Your Car's on Fire! Take Cover, Now!", 2500);
            GameFiber.Wait(2000);
            Driver.Tasks.FollowNavigationMeshToPosition(Game.LocalPlayer.Character.LastVehicle.GetOffsetPositionFront(1f), (Game.LocalPlayer.Character.Heading) - 180, 2.25f).WaitForCompletion(6000);
            GameFiber.Wait(500);
            DriverVehicle.Explode();
            Game.LogTrivial("YOBBINCALLOUTS: Vehicle Has Exploded");
            GameFiber.Wait(750);
            Game.DisplaySubtitle("Holy Crap!", 1500);
            DriverVehicleBlip.Delete();
            DriverBlip = new Blip(Driver);
            DriverBlip.Scale = 0.8f;
            DriverBlip.Color = Color.Blue;
            //
            GameFiber.Wait(1000);
            Game.DisplayNotification("~r~Fire Department~w~ is En Route!");
            LSPD_First_Response.Mod.API.Functions.RequestBackup(DriverVehicle.Position, LSPD_First_Response.EBackupResponseType.Code3, LSPD_First_Response.EBackupUnitType.Firetruck);
            //Might add a way to fight the fire with a fire extinguisher, I have to see if STP API will let me disable the realistic weapon system tho
            Game.LogTrivial("YOBBINCALLOUTS: Fire Department Has Been Called");
            //Crushes for some reason around here. No idea
            GameFiber.Wait(4500);
            Game.DisplayHelp("Once You are Done, Go to the ~b~Driver ~w~to Speak With Them.");
            while (Game.LocalPlayer.Character.DistanceTo(Driver) >= 2.5f) { GameFiber.Wait(0); }
        }
    }
}


