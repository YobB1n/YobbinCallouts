using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Rage;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage.Native;
using System;
using YobbinCallouts.Utilities;

namespace YobbinCallouts.Callouts
{
    [CalloutInfo("Arson", CalloutProbability.High)]

    public class Arson : Callout
    {
        private Vector3 MainSpawnPoint;

        private Blip House;
        private Blip SuspectBlip;
        private Blip VictimBlip;
        private Blip SuspectArea;

        private Ped Suspect;
        private Ped Victim;

        private int MainScenario;
        private float SuspectHeading;
        private bool CalloutRunning = false;

        private string Zone;

        private float VehicleHeading;
        private string[] Vehicles;
        private string[] Peds;

        Player player = Game.LocalPlayer;

        private LHandle MainPursuit;

        private Vehicle VictimVehicle;

        //All the dialogue for the callout. Haven't found a better way to store it yet, so this will have to do.
        private readonly List<string> OpeningDialogue1 = new List<string>()
        {
         "~b~Victim:~w~ Oh thank god you're here Officer!",
         "~b~Victim:~w~ Some guy just ran up to my car and set it on fire while I was sitting in it!",
         "~g~You:~w~ Are you alright? Do you need Medical Attention?",
         "~b~Victim:~w~ I think I'm fine, But you've got to catch him! He's crazy and could be doing the same thing again!",
         "~g~You:~w~ Alright, do you know where he ran off too?",
         "~b~Victim:~w~ He ran further up the street somewhere that way!",
        };
        private readonly List<string> OpeningDialogue2 = new List<string>()
        {
         "~b~Victim:~w~ Oh thank god you're here Officer!",
         "~b~Victim:~w~ Some dude ran up to my car and set it on fire while I was waiting at the traffic light!",
         "~g~You:~w~ Are you ok? Do you need Medical Attention?",
         "~b~Victim:~w~ I'll be fine, But you've got to catch him! He's insane and might do the same thing to someone else!",
         "~g~You:~w~ Alright, do you know where he ran off too?",
         "~b~Victim:~w~ He went further up the street somewhere in that direction!",
        };
        public override bool OnBeforeCalloutDisplayed()
        {
            Game.LogTrivial("==========YOBBINCALLOUTS: Arson Callout Start==========");
            System.Random r = new System.Random();
            int Scenario = r.Next(1, 5);    //change this
            MainScenario = Scenario;
            Game.LogTrivial("YOBBINCALLOUTS: Scenario is Value is " + MainScenario);

            Zone = LSPD_First_Response.Mod.API.Functions.GetZoneAtPosition(Game.LocalPlayer.Character.Position).GameName;
            Game.LogTrivial("YOBBINCALLOUTS: Zone is " + Zone);

            if (MainScenario > 0)
            {
                Vector3 Spawn = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(650f));
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

            ShowCalloutAreaBlipBeforeAccepting(MainSpawnPoint, 75f);    //Callout Blip Circle with radius 50m
            AddMinimumDistanceCheck(50f, MainSpawnPoint);   //Player must be 50m or further away
            Functions.PlayScannerAudio("CITIZENS_REPORT YC_ARSON");  //change this
            CalloutMessage = "Arson";
            CalloutPosition = MainSpawnPoint;
            CalloutAdvisory = "Reports of a ~r~Suspect ~w~Setting ~o~Fire~w~ to Parked Vehicles.";
            return base.OnBeforeCalloutDisplayed();
        }
        public override bool OnCalloutAccepted()
        {
            Game.LogTrivial("YOBBINCALLOUTS: Arson Callout Accepted by User.");

            if (Main.CalloutInterface)
            {
                CalloutInterfaceHandler.SendCalloutDetails(this, "CODE 3", "");
            }
            else
            {
                Game.DisplayNotification("Respond ~r~Code 3~w~");
            }

            if (MainScenario > 0)   //vehicle arson
            {
                //NativeFunction.Natives.GetClosestVehicleNodeWithHeading(MainSpawnPoint, out Vector3 nodePosition, out float heading, 1, 3.0f, 0);
                //VehicleHeading = heading;

                VictimVehicle = CallHandler.SpawnVehicle(MainSpawnPoint, VehicleHeading);
                VictimVehicle.IsPersistent = true;
                VictimVehicle.IsDriveable = false;
                VictimVehicle.EngineHealth = 0;
                MainSpawnPoint = VictimVehicle.GetOffsetPositionRight(1.5f);
                SuspectHeading = VictimVehicle.Heading + 90;
                House = VictimVehicle.AttachBlip();
                House.IsFriendly = false;
                if(House.Exists()) House.IsRouteEnabled = true;
            }
            if (MainScenario < 3)   //suspect on scene
            {
                Peds = new string[8] { "A_M_Y_SouCent_01", "A_M_Y_StWhi_01", "A_M_Y_StBla_01", "A_M_Y_Downtown_01", "A_M_Y_BevHills_01", "G_M_Y_MexGang_01", "G_M_Y_MexGoon_01", "G_M_Y_StrPunk_01" };
                System.Random r2 = new System.Random();  //Instantiate Random vehicle generator
                int SuspectModel = r2.Next(0, Peds.Length);    //Use Random vehicle generator

                Suspect = new Ped(Peds[SuspectModel], MainSpawnPoint, SuspectHeading);
                Game.LogTrivial("YOBBINCALLOUTS: Suspect Spawned.");
                Suspect.IsPersistent = true;
                Suspect.BlockPermanentEvents = true;
                Suspect.IsFireProof = true;

                System.Random r3 = new System.Random();
                int Weapon = r3.Next(0, 3);
                if (Weapon == 0) Suspect.Inventory.GiveNewWeapon("weapon_molotov", -1, true);
                else Suspect.Inventory.GiveNewWeapon("weapon_petrolcan", -1, true);

                if (Main.CalloutInterface)
                {
                    CalloutInterfaceHandler.SendMessage(this, "Victim reports the Suspect is still on the scene.");
                    CalloutInterfaceHandler.SendMessage(this, "Suspect is threatening victim with a further arson attack.");
                }
            }
            else  //suspect not on scene
            {
                Victim = new Ped(VictimVehicle.GetOffsetPositionRight(6));
                Victim.IsInvincible = true;
                Victim.IsPersistent = true;
                Victim.BlockPermanentEvents = true;

                if (Main.CalloutInterface)
                {
                    CalloutInterfaceHandler.SendMessage(this, "Victim reports Suspect fled the scene following the arson attack.");
                    CalloutInterfaceHandler.SendMessage(this, "Multiple reports that the Victim's vehicle is still on fire.");
                }
            }
            if (!CalloutRunning) CalloutRunning = true; Callout();
            return base.OnCalloutAccepted();
        }
        public override void OnCalloutNotAccepted()
        {
            Game.LogTrivial("YOBBINCALLOUTS: Attempted Arson Callout Not Accepted by User.");
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
                        Game.DisplayHelp("Locate the ~r~Suspect.");
                        while (player.Character.DistanceTo(MainSpawnPoint) >= 35 && !Game.IsKeyDown(Config.CalloutEndKey)) { GameFiber.Wait(0); }
                        if (Game.IsKeyDown(Config.CalloutEndKey)) { EndCalloutHandler.CalloutForcedEnd = true; break; }
                        if (MainScenario <= 2)
                        {
                            SuspectOnScene();
                        }
                        else if (MainScenario >= 3)
                        {
                            VictimOnFire();
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
        private void SuspectOnScene()
        {
            if (CalloutRunning)
            {
                SuspectBlip = Suspect.AttachBlip();
                SuspectBlip.IsFriendly = false;
                SuspectBlip.Scale = 0.75f;
                SuspectBlip.Name = "Suspect";
                if(House.Exists()) House.IsRouteEnabled = false;
                while (player.Character.DistanceTo(Suspect) >= 20f)
                {
                    Suspect.Tasks.PlayAnimation("weapon@w_sp_jerrycan", "fire", -1, AnimationFlags.None);
                    GameFiber.Wait(2000);
                }
                Suspect.Tasks.PlayAnimation("weapon@w_sp_jerrycan", "fire_outro", -1, AnimationFlags.Loop);
                GameFiber.Wait(1000);
                if (Suspect.Exists() && !Functions.IsPedArrested(Suspect))
                {
                    VictimVehicle.IsOnFire = true;
                    NativeFunction.Natives.StartScriptFire(VictimVehicle.Position, 5, true);
                }
                VictimVehicle.IsOnFire = true;
                Game.DisplaySubtitle("Dispatch, ~r~Suspect~w~ Just Lit a Vehicle on ~o~Fire!", 2500);
                GameFiber.Wait(1000);
                if (Config.CallFD)
                {
                    try { Functions.RequestBackup(VictimVehicle.Position, LSPD_First_Response.EBackupResponseType.Code3, LSPD_First_Response.EBackupUnitType.Firetruck); }
                    catch (System.NullReferenceException) { Game.LogTrivial("YOBBINCALLOUTS: Error Spawning LSPDFR Fire Truck."); }
                    Game.DisplayNotification("~r~Fire Department~w~ is En Route!");
                    Game.LogTrivial("YOBBINCALLOUTS: Fire Department Has Been Called");
                }
                if (MainScenario == 1) SuspectFlees();
                else SuspectAttacks();
            }
        }
        private void SuspectFlees()
        {
            if (CalloutRunning)
            {
                if (Suspect.IsAlive && Suspect.Exists() && !Functions.IsPedArrested(Suspect))
                {
                    Suspect.Tasks.ClearImmediately();
                    LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("CRIME_SUSPECT_ON_THE_RUN_01");
                    if (Main.CalloutInterface)
                    {
                        CalloutInterfaceHandler.SendMessage(this, "Suspect is Fleeing, Requesting Backup.");
                    }
                    MainPursuit = LSPD_First_Response.Mod.API.Functions.CreatePursuit();
                    LSPD_First_Response.Mod.API.Functions.RequestBackup(Suspect.Position, LSPD_First_Response.EBackupResponseType.Code3, LSPD_First_Response.EBackupUnitType.LocalUnit);
                    LSPD_First_Response.Mod.API.Functions.SetPursuitIsActiveForPlayer(MainPursuit, true);
                    LSPD_First_Response.Mod.API.Functions.AddPedToPursuit(MainPursuit, Suspect);
                    while (LSPD_First_Response.Mod.API.Functions.IsPursuitStillRunning(MainPursuit)) { GameFiber.Wait(0); }

                    if (Suspect.Exists())
                    {
                        if (Functions.IsPedArrested(Suspect)) { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect is Under ~g~Arrest~w~ Following the Pursuit."); }
                        else { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect Was ~r~Killed~w~ Following the Pursuit."); }
                    }
                    else { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect Was ~r~Killed~w~ Following the Pursuit."); }
                    GameFiber.Wait(2000);
                    LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("REPORT_RESPONSE_COPY_02");
                    GameFiber.Wait(2000);
                }
            }
        }
        private void SuspectAttacks()
        {
            if (CalloutRunning)
            {
                if (Suspect.IsAlive && Suspect.Exists() && !Functions.IsPedArrested(Suspect))
                {
                    Suspect.Tasks.ClearImmediately();
                    Suspect.Inventory.Weapons.Clear();
                    Suspect.Inventory.GiveNewWeapon("WEAPON_KNIFE", -1, true);
                    //Suspect.Tasks.FightAgainst(Game.LocalPlayer.Character, -1);
                    //Functions.PlayScannerAudio("CRIME_ASSAULT_PEACE_OFFICER_01");
                    GameFiber.Wait(500);
                    Suspect.Tasks.FightAgainst(Game.LocalPlayer.Character, -1);
                    while (Suspect.Exists() && !Functions.IsPedArrested(Suspect) && Suspect.IsAlive)
                    {
                        GameFiber.Yield();
                    }
                    if (Suspect.Exists())
                    {
                        if (Functions.IsPedArrested(Suspect)) { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect is Under ~g~Arrest~w~ for Attempting to ~r~Assault~w~ an Officer."); }
                        else { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect Was ~r~Killed~w~ Attempting to ~r~Assault~w~ an Officer."); }
                    }
                    else { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect Was ~r~Killed~w~ Attempting to ~r~Assault~w~ an Officer."); }
                    GameFiber.Wait(2000);
                    Functions.PlayScannerAudio("REPORT_RESPONSE_COPY_02");
                    GameFiber.Wait(2000);
                    //Game.DisplayHelp("Press End to ~b~Finish~w~ the Callout.");
                    //while (!Game.IsKeyDown(System.Windows.Forms.Keys.End)) GameFiber.Wait(0);
                }
            }
        }
        private void VictimOnFire()
        {
            if (CalloutRunning)
            {
               if(House.Exists()) House.IsRouteEnabled = false;
                VictimBlip = Victim.AttachBlip();
                VictimBlip.IsFriendly = true;
                VictimBlip.Scale = 0.75f;

                Victim.Tasks.AchieveHeading(VictimVehicle.Heading + 90).WaitForCompletion(500);
                Victim.Tasks.Cower(-1);
                VictimVehicle.IsOnFire = true;
                Vector3 FireBone = new Vector3();
                FireBone = VictimVehicle.GetBonePosition("bodyshell");
                NativeFunction.Natives.StartScriptFire(FireBone, 5, true);  //gas fire
                NativeFunction.Natives.StartScriptFire(FireBone, 5, false); //not
                VictimVehicle.EngineHealth = 0;
                VictimVehicle.IsOnFire = true;
                if (player.Character.IsInAnyVehicle(false)) Game.DisplayHelp("Exit Your ~b~Vehicle~w~ and Investigate the ~r~Situation.");
                while (player.Character.IsInAnyVehicle(false)) GameFiber.Wait(0);
                Game.DisplaySubtitle("~g~You: ~w~Dispatch, We Have a Vehicle ~o~Fire!", 2500);
                if (Main.CalloutInterface)
                {
                    CalloutInterfaceHandler.SendMessage(this, "Vehicle is on Fire Requesting Fire Department.");
                }
                GameFiber.Wait(2000);
                if (Config.CallFD)
                {
                    try { Functions.RequestBackup(VictimVehicle.Position, LSPD_First_Response.EBackupResponseType.Code3, LSPD_First_Response.EBackupUnitType.Firetruck); }
                    catch (System.NullReferenceException) { Game.LogTrivial("YOBBINCALLOUTS: Error Spawning LSPDFR Fire Truck."); }
                    Game.LogTrivial("YOBBINCALLOUTS: Fire Department Has Been Called");
                    Game.DisplayNotification("~r~Fire Department~w~ is En Route!");
                }
                //Might add a way to fight the fire with a fire extinguisher, I have to see if STP API will let me disable the realistic weapon system tho
                GameFiber.Wait(1000);
                Game.DisplayHelp("Either Talk to the ~b~Victim~w~, or Press ~y~" + Config.MainInteractionKey + " ~w~to Grab a ~o~Fire Extinguisher~w~ from the Back of Your ~b~Vehicle.");
                //add way to talk to victim after
                while (player.Character.IsInAnyVehicle(false)) GameFiber.Wait(0);
                Vector3 BootBone = new Vector3();
                if (player.Character.LastVehicle.Exists())
                {
                    if (player.Character.LastVehicle.HasBone("boot")) BootBone = player.Character.LastVehicle.GetBonePosition("boot");
                    else BootBone = player.Character.GetOffsetPositionFront(-4f); //test?
                }
                Game.LogTrivial("YOBBINCALLOUTS: Trunk Location is:" + BootBone + "");
                while (!Game.IsKeyDown(Config.MainInteractionKey) && player.Character.DistanceTo(Victim) >= 2f) GameFiber.Wait(0);
                if (Game.IsKeyDown(Config.MainInteractionKey) && player.Character.DistanceTo(Victim) >= 2f)
                {
                    Game.LogTrivial("YOBBINCALLOUTS: Player Has Chosen to Fight Fire.");
                    VictimVehicle.IsExplosionProof = false;
                    player.Character.Tasks.FollowNavigationMeshToPosition(BootBone, 2, player.Character.LastVehicle.Heading, 0.05f, -1).WaitForCompletion();
                    player.Character.Heading = player.Character.LastVehicle.Heading;
                    player.Character.Tasks.PlayAnimation("rcmepsilonism8", "bag_handler_grab_walk_left", 1, AnimationFlags.None);
                    GameFiber.Wait(1000);
                    player.Character.LastVehicle.OpenDoor(VehicleExtensions.Doors.Trunk);

                    player.Character.Inventory.GiveNewWeapon("WEAPON_FIREEXTINGUISHER", -1, true);
                    player.Character.IsFireProof = true;
                    player.Character.IsExplosionProof = true;
                    player.Character.CanRagdoll = false;
                    GameFiber.Wait(500);
                    player.Character.Tasks.ClearImmediately();
                    GameFiber.Wait(5000);
                    Game.DisplayHelp("Once the ~o~Fire~w~ is ~g~Extinguished~w~, Talk to the ~b~Victim.");
                }
                TalkToVictim();
            }
        }
        private void TalkToVictim()
        {
            if (CalloutRunning)
            {
                while (player.Character.DistanceTo(Victim) >= 3) GameFiber.Wait(0);
                Victim.Tasks.ClearImmediately();
                Victim.Tasks.StandStill(500).WaitForCompletion(500);
                Victim.Tasks.AchieveHeading(player.Character.Heading - 180).WaitForCompletion(500);

                if (Config.DisplayHelp) Game.DisplayHelp("Press ~y~" + Config.MainInteractionKey + " ~w~to talk to the ~b~Victim.");
                House.Delete();

                System.Random r = new System.Random();
                int OpeningDialogue = r.Next(0, 2);
                switch (OpeningDialogue)
                {
                    case 0:
                        CallHandler.Dialogue(OpeningDialogue1, Victim);
                        break;
                    case 1:
                        CallHandler.Dialogue(OpeningDialogue2, Victim);
                        break;
                }

                Vector3 outPosition = World.GetNextPositionOnStreet(Victim.Position.Around(300));
                //NativeFunction.Natives.xA0F8A7517A273C05<bool>(SuspectSpawnPoint, 360, out Vector3 outPosition);
                try
                {
                    Peds = new string[8] { "A_M_Y_SouCent_01", "A_M_Y_StWhi_01", "A_M_Y_StBla_01", "A_M_Y_Downtown_01", "A_M_Y_BevHills_01", "G_M_Y_MexGang_01", "G_M_Y_MexGoon_01", "G_M_Y_StrPunk_01" };
                    System.Random r2 = new System.Random();  //Instantiate Random vehicle generator
                    int SuspectModel = r2.Next(0, Peds.Length);    //Use Random vehicle generator
                    Suspect = new Ped(Peds[SuspectModel], outPosition, 69);
                    Game.LogTrivial("YOBBINCALLOUTS: Suspect Spawned.");
                    Suspect.IsPersistent = true;
                    Suspect.BlockPermanentEvents = true;
                    Suspect.IsFireProof = true;

                    System.Random r3 = new System.Random();
                    int Weapon = r3.Next(0, 2);
                    if (Weapon == 0) Suspect.Inventory.GiveNewWeapon("weapon_molotov", -1, true);
                    else Suspect.Inventory.GiveNewWeapon("weapon_petrolcan", -1, true);
                    Game.LogTrivial("YOBBINCALLOUTS: Suspect Weapon Given.");
                    Victim.Tasks.ClearImmediately();
                    Game.LogTrivial("YOBBINCALLOUTS: Victim Turning to Face Suspect Area.");
                    NativeFunction.Natives.TASK_TURN_PED_TO_FACE_COORD(Victim, outPosition, 1500); //test
                }
                catch (Exception)
                {
                    Game.LogTrivial("YOBBINCALLOUTS: Yobbincallouts Crash Prevented. InvalidHandleableException.");
                    End();
                }
                GameFiber.Wait(2000);
                Victim.Tasks.ClearImmediately();
                Victim.Tasks.PlayAnimation("gestures@f@standing@casual", "gesture_point", 1, AnimationFlags.Loop).WaitForCompletion(1500);
                GameFiber.Wait(1500);
                Victim.Tasks.ClearImmediately();
                Game.DisplaySubtitle("Alright, I'll Start the Search. Thanks!", 3000);
                GameFiber.Wait(3500);
                Victim.Dismiss();
                VictimBlip.Delete();
                Search();
            }
        }
        private void Search()
        {
            if (CalloutRunning)
            {
                if (player.Character.LastVehicle.Exists())
                    player.Character.LastVehicle.CloseDoor(VehicleExtensions.Doors.Trunk);

                player.Character.IsFireProof = false;
                player.Character.IsExplosionProof = false;
                player.Character.CanRagdoll = true;

                SuspectArea = new Blip(Suspect.Position.Around(15), 150);
                SuspectArea.Color = Color.Orange;
                SuspectArea.Alpha = 0.5f;
                if(SuspectArea.Exists()) SuspectArea.IsRouteEnabled = true;
                Suspect.Tasks.Wander();
                GameFiber.Wait(1500);
                Game.DisplayHelp("Start ~o~Searching~w~ for the ~r~Suspect.");
                if (Main.CalloutInterface)
                {
                    CalloutInterfaceHandler.SendMessage(this, "Search Started for the Arson Suspect.");
                }

                Victim.ClearLastVehicle(); Victim.Dismiss();

                while (player.Character.DistanceTo(Suspect) >= 60) GameFiber.Wait(0);
                Functions.PlayScannerAudio("ATTENTION_ALL_UNITS_01");   //change
                GameFiber.Wait(1000);
                Game.DisplayNotification("~b~Update:~w~ A Caller Has ~y~Spotted~w~ the ~r~Suspect. ~g~Updating Map.");
                GameFiber.Wait(1000);
                SuspectArea.Delete();
                SuspectArea = new Blip(Suspect.Position.Around(10), 50);
                SuspectArea.Color = Color.Orange;
                SuspectArea.Alpha = 0.5f;

                while (player.Character.DistanceTo(Suspect) >= 20) GameFiber.Wait(0);
                SuspectArea.Delete();
                SuspectBlip = Suspect.AttachBlip();
                SuspectBlip.IsFriendly = false;
                SuspectBlip.Scale = 0.75f;
                GameFiber.Wait(1500);
                Game.DisplaySubtitle("Dispatch, We Have Located the ~r~Suspect!");
                GameFiber.Wait(1500);

                if (MainScenario == 3) SuspectFlees();
                else SuspectAttacks();
            }
        }
        public override void End()
        {
            base.End();
            CalloutRunning = false;
            Game.DisplayNotification("~g~Code 4~w~, return to patrol.");
            //if (Animal.Exists() && Animal.IsAlive) { Animal.Dismiss(); }
            if (VictimBlip.Exists()) VictimBlip.Delete();
            if (Suspect.Exists()) { Suspect.Dismiss(); }
            if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }
            if (House.Exists()) { House.Delete(); }
            Functions.PlayScannerAudio("ATTENTION_ALL_UNITS WE_ARE_CODE_4");
            Game.LogTrivial("YOBBINCALLOUTS: Arson Callout Finished Cleaning Up.");
        }
        public override void Process()
        {
            base.Process();
        }
    }
}
