//TO-DO: Refactor this callout with more RNG and methods, and add more ending scenarios.

using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Rage;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage.Native;
using System;

namespace YobbinCallouts.Callouts
{
    [CalloutInfo("Bait Car", CalloutProbability.Medium)]

    public class BaitCar : Callout
    {
        private Vector3 MainSpawnPoint;

        private Blip AreaBlip;
        private Blip SuspectBlip;
        private Blip OfficerBlip;
        private Blip BaitVehicleBlip;

        private Ped Suspect;
        private Ped Rando1;
        private Ped Rando2;
        private Ped Rando3;
        private Ped Officer;
        private Ped MainSuspect;
        Ped player = Game.LocalPlayer.Character;

        private int MainScenario;
        private int WaitTime;
        private int Peeps;
        private int PeepsSpawned = 1;
        private bool CalloutRunning = false;
        private bool HighCrime = false;

        private string Zone;

        private LHandle MainPursuit;

        private Vehicle BaitVehicle;
        private Vehicle OfficerVehicle;

        private Vehicle PlayerVehicle;
        private uint VehicleHash;
        private float VehicleHeading;
        private float BaitCarHeading;
        private int SuspectModel;

        //DIALOGUE
        private readonly List<string> OpeningDialogue1 = new List<string>()
        {
            "~b~Officer: ~w~Hey, hope you're doing well Officer.",
            "~b~Officer: ~w~As part of a new effort to reduce vehicle theft, we've decided to run a bait car setup in a high crime area.",
            "~b~Officer: ~w~The ~g~bait car~w~ is parked down the street.",
            "~b~Officer: ~w~Park your cruiser in a nondescript location in view of the car.",
            "~g~You:~w~ Sounds good, hopefully we catch some people!",
            "~b~Officer:~w~ One more thing, if someone drives off, you can remotely switch the car off using ~y~"+Config.MainInteractionKey+"~w~.",
            "~b~Officer:~w~ The car will have to move around 100 metres from the start point before you can shut the engine off.",
        };

        public override bool OnBeforeCalloutDisplayed()
        {
            Game.LogTrivial("==========YOBBINCALLOUTS: Bait Car Callout Start==========");
            System.Random r = new System.Random();
            int Scenario = r.Next(0, 0);    //change this
            MainScenario = Scenario;
            Game.LogTrivial("YOBBINCALLOUTS: Scenario is Value is " + MainScenario);

            Zone = LSPD_First_Response.Mod.API.Functions.GetZoneAtPosition(Game.LocalPlayer.Character.Position).RealAreaName;
            Game.LogTrivial("YOBBINCALLOUTS: Zone is " + Zone);
            if (MainScenario >= 0)
            {
                Vector3 Spawn = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(550f));
                try
                {
                    NativeFunction.Natives.GetClosestVehicleNodeWithHeading(Spawn, out Vector3 nodePosition, out float heading, 1, 3.0f, 0);
                    bool success = NativeFunction.Natives.xA0F8A7517A273C05<bool>(Spawn, heading, out Vector3 outPosition);
                    if (success)
                    {
                        MainSpawnPoint = outPosition;
                        VehicleHeading = heading;
                    }
                    else
                    {
                        Game.LogTrivial("YOBBINCALLOUTS: Could Not Find Spawnpoint. Aborting Callout.");
                        return false;
                    }
                }
                catch (System.Reflection.TargetInvocationException)
                {
                    Game.LogTrivial("YOBBINCALLOUTS: Could Not Find Spawnpoint. Aborting Callout.");
                    return false;
                }
            }

            ShowCalloutAreaBlipBeforeAccepting(MainSpawnPoint, 50f);    //Callout Blip Circle with radius 50m
            AddMinimumDistanceCheck(50f, MainSpawnPoint);   //Player must be 50m or further away
            Functions.PlayScannerAudio("WE_HAVE_01 CRIME_OFFICER_IN_NEED_OF_ASSISTANCE_02");  //change this
            CalloutMessage = "Bait Car";
            CalloutPosition = MainSpawnPoint;
            CalloutAdvisory = "Officer Requests Assistance Monitoring a Bait Car Operation.";
            return base.OnBeforeCalloutDisplayed();
        }
        public override bool OnCalloutAccepted()
        {
            Game.LogTrivial("YOBBINCALLOUTS: Bait Car Callout Accepted by User.");
            if (Main.CalloutInterface)
            {
                CalloutInterfaceHandler.SendCalloutDetails(this, "CODE 2", "");
            }
            else
            {
                Game.DisplayNotification("Respond ~b~Code 2.~w~");
            }


            if (MainScenario >= 0)
            {
                OfficerVehicle = new Vehicle(Config.PoliceVehicle, MainSpawnPoint, VehicleHeading);
                OfficerVehicle.IsPersistent = true;
                OfficerVehicle.LicensePlate = "COCONUT";    //wow am funny
                Officer = OfficerVehicle.CreateRandomDriver();
                Officer.IsPersistent = true;
                Officer.BlockPermanentEvents = true;
            }
            AreaBlip = new Blip(MainSpawnPoint, 25);
            AreaBlip.Color = Color.Yellow;
            AreaBlip.Alpha = 0.67f;
            AreaBlip.IsRouteEnabled = true;
            AreaBlip.Name = "Callout Location";

            if (Config.BaitVehicle == "None" || Config.BaitVehicle == "none")
            {
                BaitVehicle = CallHandler.SpawnVehicle(OfficerVehicle.GetOffsetPositionFront(-7), OfficerVehicle.Heading);
            }
            else
            {
                Game.LogTrivial("YOBBINCALLOUTS: Player Has Specified a Custom Bait Car. Attempting Spawn.");
                try
                {
                    BaitVehicle = new Vehicle(Config.BaitVehicle, OfficerVehicle.GetOffsetPositionFront(-7), OfficerVehicle.Heading);
                }
                catch (Exception)
                {
                    Game.DisplayNotification("~r~Error~w~ Spawning ~b~Custom Bait Car.~w~ Please Ensure the ~y~Vehicle name~w~ in ~y~Yobbincallouts.ini~w~ is ~g~Valid.");
                    Game.LogTrivial("YOBBINCALLOUTS: Error Spawning Custom Bait Car Model. Most Likely an Invalid Vehicle Model/Name Changed by the User in YobbinCallouts.ini.");
                    BaitVehicle = CallHandler.SpawnVehicle(OfficerVehicle.GetOffsetPositionFront(-7), OfficerVehicle.Heading);
                }
            }
            Game.LogTrivial("YOBBINCALLOUTS: Bait Car Successfully Spawned.");
            BaitVehicle.IsPersistent = true;

            if (!CalloutRunning) CalloutRunning = true; Callout();
            return base.OnCalloutAccepted();
        }
        public override void OnCalloutNotAccepted()
        {
            Game.LogTrivial("YOBBINCALLOUTS: Bait Car Callout Not Accepted by User.");
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
                        Setup();
                        Observe();
                        Stolen();
                        break;
                    }
                    Game.LogTrivial("YOBBINCALLOUTS: Callout Finished, Ending...");
                    EndCalloutHandler.EndCallout();
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
            }
            );
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

            if (Suspect.Exists() && Suspect != MainSuspect) Suspect.Delete();
            if (Rando1.Exists() && Rando1 != MainSuspect) Rando1.Delete();
            if (Rando2.Exists() && Rando2 != MainSuspect) Rando2.Delete();
            if (Officer.Exists()) Officer.Dismiss();
            if (AreaBlip.Exists()) AreaBlip.Delete();
            if (SuspectBlip.Exists()) SuspectBlip.Delete();
            if (OfficerBlip.Exists()) OfficerBlip.Delete();
            if (BaitVehicleBlip.Exists()) BaitVehicleBlip.Delete();
            if (Rando1.Exists()) Rando1.Dismiss();
            Game.LogTrivial("YOBBINCALLOUTS: Bait Car Callout Finished Cleaning Up.");
        }
        public override void Process()
        {
            base.Process();
        }
        private void Setup()
        {
            if (CalloutRunning)
            {
                //while (player.IsInAnyVehicle(false) && !Game.IsKeyDown(Keys.End)) GameFiber.Wait(0);
                while (player.DistanceTo(Officer) >= 25f && !Game.IsKeyDown(Config.CalloutEndKey)) GameFiber.Wait(0);
                if (Game.IsKeyDown(Config.CalloutEndKey)) End();
                Game.DisplayHelp("Talk to the ~b~Officer.");
                AreaBlip.Delete();
                OfficerBlip = Officer.AttachBlip();
                OfficerBlip.IsFriendly = true;
                OfficerBlip.Scale = 0.75f;
                OfficerBlip.Name = "Officer";
                while (player.IsInAnyVehicle(false)) GameFiber.Wait(0);
                Officer.Tasks.LeaveVehicle(LeaveVehicleFlags.None).WaitForCompletion();
                Officer.Tasks.AchieveHeading(player.Heading - 180).WaitForCompletion(500);
                CallHandler.IdleAction(Officer, true);
                if (Config.DisplayHelp) Game.DisplayHelp("Press ~y~" + Config.MainInteractionKey + " ~w~to Speak With the ~b~Officer.");
                NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(Officer, player, -1);
                while (player.DistanceTo(Officer) >= 5f) GameFiber.Wait(0);
                Officer.Tasks.ClearImmediately();
                Random r = new Random();
                int Dialogue = r.Next(0, 1); //dialogue switcher I haven't got around to using yet
                if (Dialogue == 0) CallHandler.Dialogue(OpeningDialogue1, Officer);
                BaitVehicleBlip = CallHandler.AssignBlip(BaitVehicle, Color.Green, 1, "Bait Car");
                GameFiber.Wait(1000);
                Officer.PlayAmbientSpeech("generic_thanks");
                OfficerBlip.Delete();
                Officer.Dismiss();
                GameFiber.Wait(2500);
                Game.DisplayHelp("Find a ~y~suitable location~w~ to monitor the ~b~bait car.~w~ when you're ~g~ready, ~w~press ~y~" + Config.MainInteractionKey + " ~w~to ~g~Start Observing.");
                GameFiber.Wait(4000);
                Game.DisplayHelp("You can also move the ~g~Bait Car~w~ to a better position before Observing.");
            }
        }
        private void Observe()
        {
            if (CalloutRunning)
            {
                while (!Game.IsKeyDown(Config.MainInteractionKey)) GameFiber.Wait(0);
                Random r = new Random();
                Peeps = r.Next(0, 3);
                if (Peeps == 0) SpawnSuspects();
                if (Peeps == 1) SpawnSuspects();
                if (Peeps == 2) SpawnSuspects();
                Game.LogTrivial("YOBBINCALLOUTS: Finished Spawning Suspects.");
                Game.LogTrivial("YOBBINCALLOUTS: There will be "+Peeps+" Suspects Before one Enters the Bait Car.");
                Game.LogTrivial("YOBBINCALLOUTS: Starting Bait Car Event.");
                Game.DisplayNotification("Dispatch, We are Starting the ~b~Bait Car.");
                if (Main.CalloutInterface) CalloutInterfaceHandler.SendMessage(this, "Bait Car Operation Started");

                BaitVehicleBlip.Delete();
                BaitVehicleBlip = CallHandler.AssignBlip(BaitVehicle, Color.Green, 1, "Bait Car");
                OfficerBlip = new Blip(BaitVehicle.Position, 50);
                OfficerBlip.Alpha = 0.5f;
                OfficerBlip.Color = Color.Green;
                OfficerBlip.Name = "Detection Radius";
                GameFiber.Wait(4000);                
                if (Config.DisplayHelp) Game.DisplayHelp("Stay Outside the ~g~Green Circle~w~ to Avoid ~y~Detection.");
                if (Rando1.Exists()) Rando1 = Suspect;
                //NULLREFERENCE HERE VVvvVV (tryna fix with if/else)
                if (Suspect.Exists()) Suspect.Tasks.FollowNavigationMeshToPosition(BaitVehicle.GetOffsetPositionRight(2), BaitVehicle.Heading, 1.25f, 2, -1).WaitForCompletion(); //apparently this crashed once but can't figure out why.
                else
                {
                    SuspectRespawn();
                }
                while (!BaitVehicle.HasDriver)
                {
                    while (Peeps >= 0)
                    {
                        Game.LogTrivial("YOBBINCALLOUTS: First Suspect Event Started");
                        Suspect.Tasks.ClearImmediately();
                        Suspect.Tasks.AchieveHeading(BaitVehicle.Heading + 90).WaitForCompletion(500);
                        Suspect.Tasks.PlayAnimation("missarmenian2lamar_idles", "idle_look_behind_left", 1, AnimationFlags.None).WaitForCompletion(1500);
                        Suspect.Tasks.PlayAnimation("missarmenian2lamar_idles", "idle_look_behind_right", 1, AnimationFlags.None).WaitForCompletion(1500);
                        if (player.DistanceTo(BaitVehicle) >= 50)
                        {
                            if (Peeps == 0)
                            {
                                Suspect.Tasks.EnterVehicle(BaitVehicle, -1).WaitForCompletion();
                                Game.LogTrivial("YOBBINCALLOUTS: First Suspect Entered Vehicle");
                                break;
                                //peeps = 0;
                            }
                            else { Game.LogTrivial("YOBBINCALLOUTS: First Suspect Did Not Enter Vehicle"); Suspect.Dismiss(); break; }
                        }
                        else
                        {
                            if (Suspect.Exists()) Suspect.Dismiss();
                            GameFiber.Wait(2000);
                            Game.DisplayHelp("You're ~r~Too Close~w~ to the ~g~Bait Car.~w~ Find a ~b~Discrete Location~w~ Further Away.");
                            Game.LogTrivial("YOBBINCALLOUTS: Player too Close to First Suspect.");
                            GameFiber.Wait(2000);
                            break;
                        }
                    }
                    if (BaitVehicle.HasDriver) break;
                    while (Peeps >= 1)
                    {
                        Game.LogTrivial("YOBBINCALLOUTS: Second Suspect Event Started");
                        Rando2.IsVisible = true;
                        Rando2.Tasks.FollowNavigationMeshToPosition(BaitVehicle.GetOffsetPositionRight(2), BaitVehicle.Heading, 1.25f, 2, -1).WaitForCompletion();
                        Rando2.Tasks.ClearImmediately();
                        Rando2.Tasks.AchieveHeading(BaitVehicle.Heading + 90).WaitForCompletion(500);
                        Rando2.Tasks.PlayAnimation("missarmenian2lamar_idles", "idle_look_behind_left", 1, AnimationFlags.None).WaitForCompletion(1500);
                        Rando2.Tasks.PlayAnimation("missarmenian2lamar_idles", "idle_look_behind_right", 1, AnimationFlags.None).WaitForCompletion(1500);
                        if (player.DistanceTo(BaitVehicle) >= 50)
                        {
                            if (Peeps == 1 || Peeps == 2)
                            {
                                Rando2.Tasks.EnterVehicle(BaitVehicle, -1).WaitForCompletion();
                                Game.LogTrivial("YOBBINCALLOUTS: Second Suspect Entered Vehicle");
                                break;
                                //peeps = 0;
                            }
                            else { Game.LogTrivial("YOBBINCALLOUTS: Second Suspect Did not Enter Vehicle"); Rando1.Dismiss(); break; }
                        }
                        else
                        {
                            if (Rando2.Exists()) Rando2.Dismiss();
                            GameFiber.Wait(2000);
                            Game.DisplayHelp("You're ~r~Too Close~w~ to the ~g~Bait Car.~w~ Find a ~b~Discrete Location~w~ Further Away.");
                            Game.LogTrivial("YOBBINCALLOUTS: Player too Close to Second Suspect");
                            GameFiber.Wait(2000);
                            break;
                        }
                    }
                    if (BaitVehicle.HasDriver) break;
                    if (Peeps >= 2)
                    {
                        Game.LogTrivial("YOBBINCALLOUTS: Third and Final Suspect Event Started");
                        Rando2.Tasks.ClearImmediately();
                        Rando2.Tasks.AchieveHeading(BaitVehicle.Heading + 90).WaitForCompletion(500);
                        Rando2.Tasks.PlayAnimation("missarmenian2lamar_idles", "idle_look_behind_left", 1, AnimationFlags.None).WaitForCompletion(1500);
                        Rando2.Tasks.PlayAnimation("missarmenian2lamar_idles", "idle_look_behind_right", 1, AnimationFlags.None).WaitForCompletion(1500);
                        Rando2.Dismiss();
                        GameFiber.Wait(1500);
                        Game.DisplayHelp("Keep Monitoring the ~g~Bait Car.~w~ Parking the Car in ~o~High Crime~w~ Areas Will ~b~Increase~w~ the Chance of a ~o~Hit.");
                        GameFiber.Wait(1500);
                        Rando3.IsVisible = true;
                        Rando3.Tasks.FollowNavigationMeshToPosition(BaitVehicle.GetOffsetPositionRight(2), BaitVehicle.Heading, 1.25f, 2, -1).WaitForCompletion();
                        SuspectBlip = Rando3.AttachBlip();
                        SuspectBlip.IsFriendly = false;
                        SuspectBlip.Scale = 0.75f;
                        Rando3.Tasks.ClearImmediately();
                        Rando3.Tasks.AchieveHeading(BaitVehicle.Heading + 90).WaitForCompletion(500);
                        Rando3.Tasks.PlayAnimation("missarmenian2lamar_idles", "idle_look_behind_left", 1, AnimationFlags.None).WaitForCompletion(1500);
                        Rando3.Tasks.PlayAnimation("missarmenian2lamar_idles", "idle_look_behind_right", 1, AnimationFlags.None).WaitForCompletion(1500);
                        if (player.DistanceTo(BaitVehicle) >= 50)
                        {
                            Rando3.Tasks.EnterVehicle(BaitVehicle, -1).WaitForCompletion();
                            Game.LogTrivial("YOBBINCALLOUTS: Third Suspect Entered Vehicle");
                            break;
                            //peeps = 0;
                        }
                        else
                        {
                            if (Rando3.Exists()) Rando3.Dismiss();
                            GameFiber.Wait(2000);
                            Game.DisplayHelp("You're Still ~r~Too Close~w~ to the ~g~Bait Car.~y~ Try Again~w~ Some Other Time!");
                            Game.LogTrivial("YOBBINCALLOUTS: Third Suspect Did not Enter Vehicle, Player Still too close (noob lol)");
                            if (SuspectBlip.Exists()) SuspectBlip.Delete();
                            GameFiber.Wait(2000);
                            break;
                        }
                    }
                }
            }
        }
        private void Stolen()
        {
            if (CalloutRunning)
            {
                Game.LogTrivial("YOBBINCALLOUTS: A Suspect Has Entered the Bait Car");
                GameFiber.Wait(1000);
                Game.DisplaySubtitle("Dispatch, Someone Just ~r~Entered~w~ the ~g~Bait Car!", 3000);
                player.Tasks.PlayAnimation("random@arrests", "generic_radio_chatter", -1, AnimationFlags.SecondaryTask | AnimationFlags.UpperBodyOnly).WaitForCompletion(3000);
                GameFiber.Wait(3000);
                if (Main.CalloutInterface)
                {
                    CalloutInterfaceHandler.SendMessage(this, "Suspect has Entered Bait Car.");
                }
                Functions.PlayScannerAudio("REPORT_RESPONSE_COPY_02");  //REPORT RESPONSE
                GameFiber.Wait(2000);
                Game.DisplayHelp("Perform a ~r~Traffic Stop~w~ on the Suspect.");
                BaitVehicleBlip.Delete();
                OfficerBlip.Delete();
                //GameFiber.Wait(2000);
                if (Peeps == 0) MainSuspect = Suspect;
                else if (Peeps == 1) MainSuspect = Rando1;
                else if (Peeps == 2) MainSuspect = Rando2;
                SuspectBlip = MainSuspect.AttachBlip();
                SuspectBlip.IsFriendly = false;
                SuspectBlip.Scale = 0.75f;
                SuspectBlip.Name = "Suspect";
                if (MainSuspect.IsAlive && !Functions.IsPedArrested(MainSuspect))
                {
                    Game.LogTrivial("YOBBINCALLOUTS: Suspect is Driving Away");
                    try
                    {
                        //BaitVehicle.IsDriveable = true;
                        //MainSuspect.CurrentVehicle.IsDriveable = true;
                        MainSuspect.Tasks.CruiseWithVehicle(15, VehicleDrivingFlags.FollowTraffic);
                    }
                    catch (Exception)
                    {
                        Game.DisplayNotification("There was an ~r~Error~w~ in Getting the Driver to Move. ~w~Please Check Your ~g~Log File.~w~Sorry for the Inconvenience!");
                        Game.LogTrivial("YOBBINCALLOUTS: Crash Making Bait Car Drivable/Task Invoker. Sorry for the Inconvenience.");
                        End();
                    }
                }
                Game.LogTrivial("YOBBINCALLOUTS: Suspect Started Driving Away, Waiting for a Pullover.");
                while (!Functions.IsPlayerPerformingPullover() && MainSuspect.IsAlive) GameFiber.Wait(0);
                if (MainScenario == 0)   //pursuit
                {
                    GameFiber.Wait(2000);
                    Game.LogTrivial("YOBBINCALLOUTS: Suspect Pursuit Started");
                    Functions.ForceEndCurrentPullover();
                    MainPursuit = LSPD_First_Response.Mod.API.Functions.CreatePursuit();
                    LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("CRIME_SUSPECT_ON_THE_RUN_01");
                    if (Main.CalloutInterface)
                    {
                        CalloutInterfaceHandler.SendMessage(this, "Suspect is Fleeing in the Bait Car");
                    }
                    LSPD_First_Response.Mod.API.Functions.SetPursuitIsActiveForPlayer(MainPursuit, true);
                    LSPD_First_Response.Mod.API.Functions.AddPedToPursuit(MainPursuit, MainSuspect);
                    //while (Functions.IsPursuitStillRunning(MainPursuit)) { GameFiber.Wait(0); }
                    GameFiber.Wait(1500);
                    GameFiber.Wait(3500);
                    Game.DisplayHelp("Press ~y~" + Config.MainInteractionKey + " ~w~to Activate the ~b~Kill Switch ~w~and ~o~Stop~w~ the ~r~Suspect's Vehicle.");
                    while (LSPD_First_Response.Mod.API.Functions.IsPursuitStillRunning(MainPursuit) && !Game.IsKeyDown(Config.MainInteractionKey))
                    {
                        GameFiber.Yield();
                        if (Game.IsKeyDown(Config.MainInteractionKey))
                        {
                            //this whole moble phone thing is completely fucked up lmao
                            //NativeFunction.Natives.CREATE_MOBILE_PHONE(4);
                            //NativeFunction.Natives.SET_MOBILE_PHONE_SCALE(100f);
                            //add audio
                            //if (MainSuspect.IsInAnyVehicle(false)) MainSuspect.CurrentVehicle.IsDriveable = false;
                            //BaitVehicle.IsDriveable = false;
                            MainSuspect.CurrentVehicle.FuelLevel = 0;   //this is the only way that actually works
                                                                        //BaitVehicle.FuelLevel = 0;
                            GameFiber.Wait(500);
                            //NativeFunction.Natives.DESTROY_MOBILE_PHONE();
                            Game.DisplayNotification("Dispatch, We Have ~b~Disabled~w~ the ~r~Bait Vehicle!");
                            Game.LogTrivial("YOBBINCALLOUTS: Player Has Disabled Suspects Vehicle.");
                            break;
                        }
                    }
                    CallHandler.SuspectWait(Suspect);
                }
                //Game.DisplayHelp("Press End to ~b~Finish ~w~the Callout.");
                if (Officer.Exists()) Officer.Delete(); //probably dont need this anymore
                SuspectBlip.Alpha = 0;
            }
        }
        private void SpawnSuspects()
        {
            Game.LogTrivial("YOBBINCALLOUTS: Started Suspect "+PeepsSpawned+" Spawn");
            var Peds = new string[8] { "A_M_Y_SouCent_01", "A_M_Y_StWhi_01", "A_M_Y_StBla_01", "A_M_Y_Downtown_01", "A_M_Y_BevHills_01", "G_M_Y_MexGang_01", "G_M_Y_MexGoon_01", "G_M_Y_StrPunk_01" };
            System.Random r2 = new System.Random();
            try
            {
                Ped tempsuspect;
                if (PeepsSpawned == 1)
                {
                    NativeFunction.Natives.xA0F8A7517A273C05<Vector3>(World.GetNextPositionOnStreet(player.Position.Around(45)), 360, out Vector3 suspectspawn);
                    tempsuspect = new Ped(Peds[r2.Next(0, Peds.Length)], suspectspawn, 69);
                }
                else if (PeepsSpawned == 2)
                {
                    tempsuspect = new Ped(Peds[r2.Next(0, Peds.Length)], Rando1.Position.Around(25), 69);
                }
                else
                {
                    tempsuspect = new Ped(Peds[r2.Next(0, Peds.Length)], Rando2.Position.Around(15), 69);
                }
                tempsuspect.IsPersistent = true;
                tempsuspect.BlockPermanentEvents = true;
                tempsuspect.IsVisible = false;
                tempsuspect.Tasks.Wander();
                if (PeepsSpawned == 1) Rando1 = tempsuspect;
                else if (PeepsSpawned == 2) Rando2 = tempsuspect;
                else Rando3 = tempsuspect;
            }
            catch (Rage.Exceptions.InvalidHandleableException) //this is a fairly common error that I can't seem to find a solution for.
            {
                Game.LogTrivial("YOBBINCALLOUTS: Yobbincallouts Crash Prevented. InvalidHandleableException.");
                Game.DisplayNotification("~b~YobbinCallouts~r~ Crash~g~ Prevented.~w~ I Apologize for the ~y~Inconvenience.");
                End();
            }
            Game.LogTrivial("YOBBINCALLOUTS: Finished Suspect "+PeepsSpawned+" Spawn");
            PeepsSpawned++;
        }
        private void SuspectRespawn() //test this
        {
            Game.LogTrivial("YOBBINCALLOUTS: Suspect does not exist for some reason. Attempting to use random ped.");
            var Peds = player.GetNearbyPeds(15);

            for (int i = 10; i < Peds.Length; i++) //test all this
            {
                GameFiber.Yield();
                if (Peds[i].Exists() && !Peds[i].IsPlayer && !Peds[i].IsInAnyVehicle(false))
                {
                    Suspect = Peds[i];
                    Suspect.IsPersistent = true;
                    break;
                }
            }
        }
    }
}
