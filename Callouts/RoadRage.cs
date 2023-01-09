using System.Collections.Generic;
using System.Windows.Forms;
using Rage;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage.Native;
using System.Drawing;
using System;

namespace YobbinCallouts.Callouts
{
    [CalloutInfo("Road Rage", CalloutProbability.High)] //Change later
    public class RoadRage : Callout
    {
        private Vector3 MainSpawnPoint;
        private Vector3 SuspectSpawnPoint;

        private Blip Area;
        private Blip VictimBlip;
        private Blip SuspectBlip;
        private Blip SuspectArea;

        private Vehicle VictimVehicle;
        private Vehicle SuspectVehicle;

        private Ped Victim;
        private Ped Suspect;
        private Ped player = Game.LocalPlayer.Character;

        private float VehicleHeading;
        private int MainScenario;

        private string Zone;
        private string[] Vehicles; private Color[] Colours;
        private string SuspectVehicleModel;

        private bool CalloutRunning = false;

        private LHandle MainPursuit;

        private readonly List<string> OpeningDialogue1 = new List<string>()
        {
         "~g~You:~w~ Hey, what happened here?",
         "~b~Caller:~w~ Some asshole cut me off when he changed lanes!",
         "~b~Caller:~w~ I honked at him; and he got really pissed off, officer.",
         "~b~Caller:~w~ Eventually, he cut me off again, but sideswiped my door, as you can see.",
         "~g~You:~w~ I see that. Are you okay?",
         "~b~Caller:~w~ I'm fine, but my car isn't! And you can't let that asshole get away with it either!",
         "~g~You:~w~ What did his car look like? Where did he go?", //add dashcam
        };
        private readonly List<string> OpeningDialogue2 = new List<string>()
        {
         "~b~Caller:~w~ Thanks for getting here so quickly officer! I thought I was gonna die!",
         "~g~You:~w~ Of course! What happened to start all of this?",
         "~b~Caller:~w~ Some asshole cut me off when he changed lanes!",
         "~b~Caller:~w~ I honked at him; and he got really pissed off, officer.",
         "~b~Caller:~w~ They stopped in the middle of the lane and forced me to pull over, then started smashing my car!",
         "~g~You:~w~ Did they assault you? Or was it just the car?",
         "~b~Caller:~w~ I'm fine, but my car isn't! Anyways, I'm just glad I didn't get hurt.",
         "~g~You:~w~ Yeah that's the most important thing. Well, if your car still works, then you're free to go.", //add dashcam
         "~b~Caller:~w~ Alright, thanks officer! Guess I'm on my way to the repair shop!",
        };

        public override bool OnBeforeCalloutDisplayed()
        {
            Game.LogTrivial("==========YOBBINCALLOUTS: Road Rage Callout Start==========");
            Zone = Functions.GetZoneAtPosition(Game.LocalPlayer.Character.Position).GameName;
            Game.LogTrivial("YOBBINCALLOUTS: Zone is " + Zone);

            System.Random chez = new System.Random();
            int boom = chez.Next(0, 3);
            MainScenario = boom;
            Game.LogTrivial("YOBBINCALLOUTS: Scenario is " + MainScenario);

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

            ShowCalloutAreaBlipBeforeAccepting(MainSpawnPoint, 25);    //Callout Blip Circle with radius 100m
            AddMinimumDistanceCheck(50f, MainSpawnPoint);   //Player must be 150m or further away
            Functions.PlayScannerAudio("WE_HAVE_01 CRIME_MALICIOUS_VEHICLE_DAMAGE_01"); //Default
            CalloutMessage = "Road Rage";
            CalloutPosition = MainSpawnPoint;
            if (MainScenario == 0) CalloutAdvisory = "Caller Reports a ~r~Road Rage~w~ Incident. Suspect Then Fled the Scene.";
            else CalloutAdvisory = "Caller Reprots a ~r~Road Rage~w~ Incident. Suspect is Damaging their Vehicle.";
            return base.OnBeforeCalloutDisplayed();
        }
        public override bool OnCalloutAccepted()
        {
            Game.LogTrivial("YOBBINCALLOUTS: Road Rage Callout Accepted by User");
            if (Main.CalloutInterface)
            {
                CalloutInterfaceHandler.SendCalloutDetails(this, "CODE 3", "");
            }
            else
            {
                Game.DisplayNotification("Respond ~r~Code 3~w~.");
            }

            VictimVehicle = CallHandler.SpawnVehicle(MainSpawnPoint, VehicleHeading);
            Game.LogTrivial("YOBBINCALLOUTS: Victim Vehicle Spawned");
            VictimVehicle.IsPersistent = true;
            VictimVehicle.IsEngineOn = true;
            VictimVehicle.IsDriveable = false;

            Victim = VictimVehicle.CreateRandomDriver();
            Victim.IsPersistent = true;
            Victim.BlockPermanentEvents = true;
            Victim.Tasks.CruiseWithVehicle(0f);
            Victim.IsInvincible = true;

            SuspectSpawnPoint = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(650));
            //add different deformations
            VictimVehicle.IsDeformationEnabled = true;
            VictimVehicle.Deform(VictimVehicle.GetPositionOffset(VictimVehicle.GetBonePosition("door_dside_f")), 100f, 600f);
            Area = new Blip(MainSpawnPoint, 25);
            Area.Color = System.Drawing.Color.Yellow;
            Area.Alpha = 0.67f;
            Area.IsRouteEnabled = true;
            Area.Name = "Scene";

            if (MainScenario >= 1) //suspect still on scene
            {
                SuspectVehicle = CallHandler.SpawnVehicle(VictimVehicle.GetOffsetPositionFront(5f), VictimVehicle.Heading - 180);
                SuspectVehicle.IsDeformationEnabled = true;
                SuspectVehicle.IsPersistent = true;

                Suspect = SuspectVehicle.CreateRandomDriver();
                Suspect.IsPersistent = true;
                Suspect.BlockPermanentEvents = true;
                SuspectVehicle.IsDriveable = false;
                Suspect.Tasks.CruiseWithVehicle(0f);

                Game.LogTrivial("YOBBINCALLOUTS: Finished Spawning Suspect.");
                Victim.IsInvincible = true;
                Victim.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen);

                if (Main.CalloutInterface) CalloutInterfaceHandler.SendMessage(this, "Caller reports suspect is still on scene damaging their vehicle");
            }
            else //suspect no longer on scene
            {
                if (Main.CalloutInterface) CalloutInterfaceHandler.SendMessage(this, "Caller reports suspect fled the scene after damaging their vehicle");
            }

            Game.DisplayNotification("Go to the ~y~Scene~w~ to Speak with the ~b~Caller.");

            if (CalloutRunning == false) { Callout(); }
            return base.OnCalloutAccepted();
        }
        public override void OnCalloutNotAccepted()
        {
            Game.LogTrivial("YOBBINCALLOUTS: Road Rage Callout Not Accepted by User.");
            Functions.PlayScannerAudio("OTHER_UNIT_TAKING_CALL_01");  //this gets annoying after a while
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
                        if (MainScenario == 0) while (player.DistanceTo(MainSpawnPoint) >= 25 && !Game.IsKeyDown(Config.CalloutEndKey)) GameFiber.Wait(0);
                        else { while (player.DistanceTo(MainSpawnPoint) >= 50 && !Game.IsKeyDown(Config.CalloutEndKey)) Suspect.Tasks.LeaveVehicle(SuspectVehicle, LeaveVehicleFlags.LeaveDoorOpen).WaitForCompletion(); GameFiber.Wait(0); }

                        if (Game.IsKeyDown(Config.CalloutEndKey)) { EndCalloutHandler.CalloutForcedEnd = true; break; }
                        if (MainScenario == 0) //not on scene
                        {
                            CallerFirst();
                            Search();
                            Discovered();
                        }
                        else if (MainScenario >= 1) //on scene
                        {
                            SuspectFirst();
                        }
                        break;
                    }
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
            if (Area.Exists()) { Area.Delete(); }
            if (VictimBlip.Exists()) VictimBlip.Delete();
            //if (Suspect.Exists()) { Suspect.Dismiss(); }
            if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }
            if (SuspectArea.Exists()) { SuspectArea.Delete(); }
            if (Victim.Exists()) Victim.Dismiss();
            Game.LogTrivial("YOBBINCALLOUTS: Road Rage Callout Finished Cleaning Up.");
        }
        public override void Process()
        {
            base.Process();
        }
        private void SuspectFirst()
        {
            if (CalloutRunning)
            {
                Area.Delete();
                VictimBlip = Victim.AttachBlip();
                VictimBlip.Scale = 0.75f;
                VictimBlip.IsFriendly = true;
                VictimBlip.Name = "Caller";
                SuspectBlip = Suspect.AttachBlip();
                SuspectBlip.Scale = 0.75f;
                SuspectBlip.IsFriendly = false;
                SuspectBlip.Name = "Suspect";

                //test this weapon give
                System.Random sewil = new System.Random();
                int WeaponChooser = sewil.Next(0, 3);
                if (WeaponChooser == 0) Suspect.Inventory.GiveNewWeapon("WEAPON_CROWBAR", -1, true);
                if (WeaponChooser == 1) Suspect.Inventory.GiveNewWeapon("WEAPON_BAT", -1, true);
                if (WeaponChooser == 2) Suspect.Inventory.GiveNewWeapon("WEAPON_POOLCUE", -1, true);

                //Suspect.Tasks.FightAgainst(Victim, -1);
                //Suspect.Tasks.GoStraightToPosition(VictimVehicle.FrontPosition, 3f, VictimVehicle.Heading - 180, 2f, -1).WaitForCompletion();
                Suspect.Tasks.PlayAnimation("missheist_agency3aig_13", "wait_loops_player0", -1, AnimationFlags.Loop);

                Game.DisplaySubtitle("~b~Driver:~w~ Officer! That Person Right There Just Damaged my Vehicle!!", 5000);
                Victim.Tasks.Cower(-1);
                //GameFiber.Wait(1000);
                Game.DisplayHelp("Apprehend the ~r~Suspect.");
                if (MainScenario == 2)
                {
                    Suspect.Tasks.EnterVehicle(SuspectVehicle, -1, -1, 4.20f).WaitForCompletion();
                    Discovered();
                } //flee
                else
                {
                    Game.DisplayHelp("Arrest the ~r~Suspect.");
                    CallHandler.SuspectWait(Suspect);
                    SuspectBlip.Delete();            
                    CallerEnd();
                }
            }
        }
        private void CallerFirst()
        {
            if (CalloutRunning)
            {
                Area.Delete();
                VictimBlip = Victim.AttachBlip();
                VictimBlip.Scale = 0.75f;
                VictimBlip.IsFriendly = true;
                VictimBlip.Name = "Caller";
                Game.DisplayHelp("Speak with the ~b~Caller.");
                Victim.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen).WaitForCompletion();
                NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(Victim, player, -1);
                while (player.DistanceTo(Victim) >= 5) GameFiber.Wait(0);
                if (Config.DisplayHelp)
                {
                    Game.DisplayHelp("Press ~y~" + Config.MainInteractionKey + " ~w~to Speak with the ~b~Driver.");
                }
                CallHandler.Dialogue(OpeningDialogue1, Victim);

                SuspectVehicle = CallHandler.SpawnVehicle(SuspectSpawnPoint, 69);
                Colours = new Color[8] { Color.White, Color.Black, Color.Gray, Color.Silver, Color.Red, Color.Blue, Color.Teal, Color.Beige };
                System.Random r3 = new System.Random();
                int Colour = r3.Next(0, Colours.Length);
                SuspectVehicle.PrimaryColor = Colours[Colour];

                Game.LogTrivial("YOBBINCALLOUTS: Suspect Vehicle Spawned");
                SuspectVehicle.IsPersistent = true;
                SuspectVehicle.IsEngineOn = true;
                SuspectVehicle.IsDriveable = false;
                SuspectVehicle.IsVisible = false;

                SuspectVehicle.IsDeformationEnabled = true;
                if (SuspectVehicle.HasBone("door_pside_r")) SuspectVehicle.Deform(SuspectVehicle.GetPositionOffset(SuspectVehicle.GetBonePosition("door_pside_r")), 100f, 500f);

                Suspect = SuspectVehicle.CreateRandomDriver();
                Suspect.IsPersistent = true;
                Suspect.BlockPermanentEvents = true;
                Suspect.Tasks.CruiseWithVehicle(0f);
                Suspect.IsInvincible = true;

                Victim.Tasks.PlayAnimation("missfbi3_party_d", "stand_talk_loop_a_male1", -1, AnimationFlags.Loop);
                string VehicleColour = Colours[Colour].Name;
                Game.DisplaySubtitle("~b~Caller:~w~ The Vehicle was a ~b~" + VehicleColour + "-Colored ~r~" + SuspectVehicle.Model.Name + ".", 4000); //fix color
                GameFiber.Wait(4000);
                NativeFunction.Natives.TASK_TURN_PED_TO_FACE_COORD(Victim, SuspectSpawnPoint, 1500); //test
                Game.DisplaySubtitle("~b~Caller:~w~ They Went That Way!", 2500);
                GameFiber.Wait(2000);
                Victim.Tasks.ClearImmediately();
                Victim.Tasks.PlayAnimation("gestures@f@standing@casual", "gesture_point", 1, AnimationFlags.Loop).WaitForCompletion(1500);
                GameFiber.Wait(1500);
                Victim.Tasks.ClearImmediately();
                Game.DisplaySubtitle("~g~You:~w~ Alright, I'll Start the Search. Thanks!", 3000);
                GameFiber.Wait(3500);
                Victim.Dismiss();
                VictimBlip.Delete();
            }
        }
        private void Search()
        {
            if (CalloutRunning)
            {
                Game.DisplayHelp("Start ~o~Searching~w~ for the ~r~Suspect.");
                SuspectArea = new Blip(Suspect.Position.Around(15), 250);
                SuspectArea.Color = Color.Orange;
                SuspectArea.Alpha = 0.5f;
                SuspectArea.IsRouteEnabled = true;
                GameFiber.Wait(1500);

                while (player.DistanceTo(Suspect) >= 150) GameFiber.Wait(0);
                SuspectVehicle.IsDriveable = true;
                SuspectVehicle.IsVisible = true;
                Suspect.Tasks.CruiseWithVehicle(SuspectVehicle, 20, VehicleDrivingFlags.DriveAroundVehicles);
                Functions.PlayScannerAudio("ATTENTION_ALL_UNITS_01");   //change
                GameFiber.Wait(1000);
                if (Main.CalloutInterface)
                {
                    CalloutInterfaceHandler.SendMessage(this, "Caller spotted the suspect driving recklessly, updating map.");
                }
                else
                {
                    Game.DisplayNotification("~b~Update:~w~ A Caller Has ~y~Spotted~w~ the ~r~Suspect~w~ Driving Recklessly. ~g~Updating Map.");    //fix this, too hard to see suspect. maybe remind them what the car looks like.
                }
                GameFiber.Wait(1000);

                if (SuspectArea.Exists()) SuspectArea.Delete();
                SuspectBlip = Suspect.AttachBlip();
                SuspectBlip.IsFriendly = false;
                SuspectBlip.Scale = 0.75f;
                GameFiber.Wait(1500);
                Game.DisplaySubtitle("Dispatch, We Have Located the ~r~Suspect!");
                GameFiber.Wait(1500);
                Game.DisplayHelp("Perform a ~o~Traffic Stop~w~ on the ~r~Suspect.");
                while (!Functions.IsPlayerPerformingPullover() && Suspect.IsAlive) GameFiber.Wait(0);
            }
        }
        private void Discovered()
        {
            if (CalloutRunning)
            {
                System.Random r = new System.Random();
                int SuspectAction = r.Next(0, 0);   //add more later
                if (SuspectAction == 0)  //pursuit
                {
                    CallHandler.CreatePursuit(MainPursuit, true, true, true, Suspect);                   
                }
            }
        }
        private void CallerEnd()
        {
            if (CalloutRunning)
            {
                Game.DisplayHelp("Speak With the ~b~Victim ~w~Once You're Ready.");
                while (player.DistanceTo(Victim) >= 5) GameFiber.Wait(0);
                Game.DisplayHelp("Press ~y~" + Config.MainInteractionKey + " ~w~to Speak with the ~b~Driver.");
                CallHandler.Dialogue(OpeningDialogue2, Victim);
                Victim.Tasks.ClearImmediately();
                Victim.Dismiss();
            }
        }
    }
}