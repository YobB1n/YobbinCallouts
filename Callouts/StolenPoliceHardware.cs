using System.Drawing;
using Rage;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage.Native;
using System.Linq;
using System;

namespace YobbinCallouts.Callouts
{
    [CalloutInfo("Stolen Police Hardware", CalloutProbability.High)]
    class StolenPoliceHardware : Callout
    {
        private Vector3 MainSpawnPoint;

        private LSPD_First_Response.Engine.Scripting.EWorldZoneCounty WorldZone;    //this is for the investigation at the end of the callout

        private LHandle MainPursuit;

        private Vehicle SuspectVehicle;

        private Ped Suspect;

        private Blip SuspectBlip;

        private bool CalloutRunning = false;

        private int MainScenario;
        private string[] Vehicles;
        private string[] Peds;

        public static string Weapon = "WEAPON_CARBINERIFLE";
        private string WeaponName;
        public override bool OnBeforeCalloutDisplayed()
        {
            Game.LogTrivial("==========YOBBINCALLOUTS: Stolen Police Hardware Callout Start==========");
            MainSpawnPoint = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(450f));

            System.Random r2 = new System.Random();
            int Scenario = r2.Next(0, 4);   //change to (0, 4) later
            MainScenario = Scenario;
            Game.LogTrivial("YOBBINCALLOUTS: Scenario " + MainScenario + " Chosen.");
            ShowCalloutAreaBlipBeforeAccepting(MainSpawnPoint, 20f);
            AddMinimumDistanceCheck(20f, MainSpawnPoint);
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("WE_HAVE_01 YC_STOLEN_FIREARM");
            CalloutMessage = "Stolen Police Hardware";
            CalloutPosition = MainSpawnPoint;
            CalloutAdvisory = "~r~ANPR Hit~w~ on Vehicle Suspected of Carrying Stolen Police Weapons.";
            return base.OnBeforeCalloutDisplayed();
        }
        public override bool OnCalloutAccepted()
        {
            Game.LogTrivial("YOBBINCALLOUTS: Stolen Police Hardware Callout Accepted by User");
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("UNITS_RESPOND_CODE_03_02");
            if (Main.CalloutInterface)
            {
                CalloutInterfaceHandler.SendCalloutDetails(this, "CODE 3", "");
            }
            else
            {
                Game.DisplayNotification("Respond ~r~Code 3.~w~");
            }
            NativeFunction.Natives.GetClosestVehicleNodeWithHeading(MainSpawnPoint, out Vector3 nodePosition, out float outheading, 1, 3.0f, 0);
            SuspectVehicle = CallHandler.SpawnVehicle(MainSpawnPoint, outheading);

            Peds = new string[8] { "A_M_Y_SouCent_01", "A_M_Y_StWhi_01", "A_M_Y_StBla_01", "A_M_Y_Downtown_01", "A_M_Y_BevHills_01", "G_M_Y_MexGang_01", "G_M_Y_MexGoon_01", "G_M_Y_StrPunk_01" };
            System.Random r2 = new System.Random();
            int SuspectModel = r2.Next(0, Peds.Length);
            Suspect = new Ped(Peds[SuspectModel], MainSpawnPoint, 69);
            Suspect.WarpIntoVehicle(SuspectVehicle, -1);
            Suspect.BlockPermanentEvents = true;
            Suspect.IsPersistent = true;
            Suspect.Tasks.CruiseWithVehicle(SuspectVehicle, 15f, VehicleDrivingFlags.FollowTraffic);
            SuspectBlip = Suspect.AttachBlip();
            SuspectBlip.IsFriendly = false;
            SuspectBlip.Scale = 0.8f;   //may botch?
            SuspectBlip.IsRouteEnabled = true;
            SuspectBlip.Name = "Suspect";
            if (!CalloutRunning) Callout();
            return base.OnCalloutAccepted();
        }
        public override void OnCalloutNotAccepted()
        {
            Game.LogTrivial("YOBBINCALLOUTS: Stolen Police Hardware Callout Not Accepted by User.");
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("OTHER_UNIT_TAKING_CALL_01");
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
                        while (Game.LocalPlayer.Character.DistanceTo(Suspect) >= 20f && !Game.IsKeyDown(Config.CalloutEndKey)) GameFiber.Wait(0);
                        if (Game.IsKeyDown(Config.CalloutEndKey)) { EndCalloutHandler.CalloutForcedEnd = true; break; }
                        SuspectBlip.IsRouteEnabled = false;
                        Game.DisplayHelp("Perform a Traffic Stop on the ~r~Suspect.");
                        while (!LSPD_First_Response.Mod.API.Functions.IsPlayerPerformingPullover())
                        {
                            GameFiber.Wait(0);
                        }

                        if (MainScenario == 0) Pursuit();
                        else if (MainScenario == 1) Shootout();
                        else if (MainScenario == 2 || MainScenario == 3) Pullover();
                        break;
                        //GameFiber.Wait(2500);
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
            });
        }
        private void Pursuit()
        {
            if (CalloutRunning)
            {
                GameFiber.Wait(2000);
                Game.DisplayNotification("Suspect is ~r~Evading!");

                LSPD_First_Response.Mod.API.Functions.ForceEndCurrentPullover();
                MainPursuit = LSPD_First_Response.Mod.API.Functions.CreatePursuit();
                LSPD_First_Response.Mod.API.Functions.RequestBackup(Suspect.Position, LSPD_First_Response.EBackupResponseType.Code3, LSPD_First_Response.EBackupUnitType.LocalUnit);
                LSPD_First_Response.Mod.API.Functions.SetPursuitIsActiveForPlayer(MainPursuit, true);
                LSPD_First_Response.Mod.API.Functions.AddPedToPursuit(MainPursuit, Suspect);
                GameFiber.Wait(1500);
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("CRIME_SUSPECT_ON_THE_RUN_01");
                while (LSPD_First_Response.Mod.API.Functions.IsPursuitStillRunning(MainPursuit)) { GameFiber.Wait(0); }
                while (Suspect.Exists())
                {
                    GameFiber.Yield();
                    if (!Suspect.Exists() || Suspect.IsDead || Functions.IsPedArrested(Suspect)) break;
                }
                if (Suspect.Exists())
                {
                    if (Functions.IsPedArrested(Suspect)) { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect is Under ~g~Arrest~w~ Following the Pursuit."); }
                    else { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect Was ~r~Killed~w~ Following the Pursuit."); }
                }
                else
                {
                    GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect is Under ~g~Arrest~w~ Following the Pursuit.");
                }
                GameFiber.Wait(2000);
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("REPORT_RESPONSE_COPY_02");
                GameFiber.Wait(2000);
                if (!Main.STP) Game.DisplayHelp("Press 'Y' to Search the ~r~Vehicle~w~ for any ~r~Stolen Police Hardware.");
                else Game.DisplayHelp("Use ~b~Stop the Ped~w~ to Search the ~r~Vehicle~w~ for any ~r~Stolen Police Hardware.");
                SearchVehicle();
            }
        }
        private void Shootout()
        {
            if (CalloutRunning)
            {
                GameFiber.Wait(2500);
                Suspect.Inventory.GiveNewWeapon("WEAPON_CARBINERIFLE", -1, true);
                Suspect.Tasks.ParkVehicle(SuspectVehicle, SuspectVehicle.Position, SuspectVehicle.Heading).WaitForCompletion(5000);
                Suspect.Tasks.LeaveVehicle(SuspectVehicle, LeaveVehicleFlags.LeaveDoorOpen).WaitForCompletion();
                Suspect.Tasks.AchieveHeading(Game.LocalPlayer.Character.LastVehicle.Heading - 180).WaitForCompletion(1500);
                Suspect.Tasks.AimWeaponAt(Game.LocalPlayer.Character.Position, 1500).WaitForCompletion();   //test this
                Suspect.Tasks.FightAgainst(Game.LocalPlayer.Character, -1);
                if (LSPD_First_Response.Mod.API.Functions.IsPlayerPerformingPullover()) { LSPD_First_Response.Mod.API.Functions.ForceEndCurrentPullover(); }
                GameFiber.Wait(2000);
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("CRIME_ASSAULT_PEACE_OFFICER_01");
                LSPD_First_Response.Mod.API.Functions.RequestBackup(Suspect.Position, LSPD_First_Response.EBackupResponseType.Code3, LSPD_First_Response.EBackupUnitType.LocalUnit);
                while (!Suspect.IsCuffed && !Suspect.IsDead) { GameFiber.Wait(0); }
                if (Suspect.IsDead) { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, Suspect Was ~r~Killed~w~ Trying to Assault an Officer."); SuspectBlip.Delete(); }
                if (Suspect.IsCuffed) { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, Suspect is Under ~g~Arrest~w~ For Trying to Assault an Officer"); }
                GameFiber.Wait(2000);
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("REPORT_RESPONSE_COPY_02");
                GameFiber.Wait(2000);
                if (!Main.STP) Game.DisplayHelp("Press 'Y' to Search the ~r~Vehicle~w~ for any Other ~r~Stolen Police Hardware.");
                else Game.DisplayHelp("Use ~b~Stop the Ped~w~ to Search the ~r~Vehicle~w~ for any Other ~r~Stolen Police Hardware.");
                SearchVehicle();
            }
        }
        private void Pullover()
        {
            if (CalloutRunning)
            {
                while (Game.LocalPlayer.Character.IsInAnyVehicle(false)) { GameFiber.Wait(0); }
                if (MainScenario == 2)
                {
                    if (Config.DisplayHelp == true) { Game.DisplayHelp("Ask the ~r~Suspect~w~ to ~y~Exit~w~ the Vehicle, then ~b~Search it."); }
                    while (Suspect.IsInVehicle(SuspectVehicle, false)) GameFiber.Wait(0);
                    GameFiber.Wait(3500);   //gives suspect enough time to leave vehicle
                    if (!Main.STP) Game.DisplayHelp("Press 'Y' to Search the ~r~Vehicle~w~ for any ~r~Stolen Police Hardware.");
                    else Game.DisplayHelp("Use ~b~Stop the Ped~w~ to Search the ~r~Vehicle~w~ for any ~r~Stolen Police Hardware.");
                    SearchVehicle();
                }
                else LSPD_First_Response.Mod.API.Functions.ForceEndCurrentPullover(); Pursuit(); //scenario 3
            }
        }
        private void SearchVehicle()
        {
            if (CalloutRunning)
            {
                if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }
                SuspectBlip = SuspectVehicle.AttachBlip();
                SuspectBlip.IsFriendly = false;

                while (Game.LocalPlayer.Character.DistanceTo(SuspectVehicle) >= 2 && !Main.STP)
                {
                    if (Game.IsKeyDown(Config.MainInteractionKey))
                    {
                        Game.DisplayHelp("Please Move Closer to the Vehicle to Search.");
                    }
                    GameFiber.Yield();
                }
                if (!Main.STP)
                {
                    while (Game.LocalPlayer.Character.DistanceTo(SuspectVehicle) <= 2 && !Game.IsKeyDown(Config.MainInteractionKey)) { GameFiber.Wait(0); }
                    if (Game.IsKeyDown(Config.MainInteractionKey) && SuspectVehicle.Exists())
                    {
                        Game.DisplayNotification("~b~Searching the Vehicle.");
                        //idk if this is necessary but I liked to check if all bones are valid
                        if (SuspectVehicle.HasBone("door_dside_f") && SuspectVehicle.Doors[0].IsValid())
                            SuspectVehicle.Doors[0].Open(false);
                        if (SuspectVehicle.HasBone("door_pside_f") && SuspectVehicle.Doors[1].IsValid())
                            SuspectVehicle.Doors[1].Open(false);
                        if (SuspectVehicle.HasBone("door_dside_r") && SuspectVehicle.Doors[2].IsValid())
                            SuspectVehicle.Doors[2].Open(false);
                        if (SuspectVehicle.HasBone("door_pside_r") && SuspectVehicle.Doors[3].IsValid())
                            SuspectVehicle.Doors[3].Open(false);
                        if (SuspectVehicle.HasBone("boot") && SuspectVehicle.Doors[5].IsValid())
                            SuspectVehicle.Doors[5].Open(false);

                        GameFiber.Wait(500);
                        Game.LocalPlayer.Character.Tasks.GoStraightToPosition(SuspectVehicle.GetOffsetPositionRight(1.5f), 1f, SuspectVehicle.Heading - 90, 1f, -1).WaitForCompletion(500);
                        Game.LocalPlayer.Character.Tasks.PlayAnimation("mini@repair", "fixing_a_ped", -1, AnimationFlags.Loop).WaitForCompletion(4000);
                        Game.LocalPlayer.Character.Tasks.Clear();
                        System.Random r2 = new System.Random();
                        int EvidenceFound = r2.Next(0, 3);
                        switch (EvidenceFound)
                        {
                            case 0:
                                Game.LocalPlayer.Character.Inventory.GiveNewWeapon("WEAPON_CARBINERIFLE", 0, true);
                                Weapon = "WEAPON_CARBINERIFLE";
                                GameFiber.Wait(1000);
                                Game.DisplayNotification("Dispatch, We Have Recovered a Stolen Police ~r~Assault Rifle~w~ on Scene.");
                                break;
                            case 1:
                                Game.LocalPlayer.Character.Inventory.GiveNewWeapon("WEAPON_PUMPSHOTGUN", 0, true);
                                Weapon = "WEAPON_PUMPSHOTGUN";
                                GameFiber.Wait(1000);
                                Game.DisplayNotification("Dispatch, We Have Recovered a Stolen Police ~r~Shotgun~w~ on Scene.");
                                break;
                            case 2:
                                Game.LocalPlayer.Character.Inventory.GiveNewWeapon("WEAPON_ADVANCEDRIFLE", 0, true);
                                Weapon = "WEAPON_ADVANCEDRIFLE";
                                GameFiber.Wait(1000);
                                Game.DisplayNotification("Dispatch, We Have Recovered a Stolen Police ~r~Rifle~w~ on Scene.");
                                break;
                        }
                        GameFiber.Wait(2000);
                        if (MainScenario == 1) { Game.DisplayNotification("We Have also Recovered a Stolen ~r~Assault Rifle~w~ Used in the Shootout."); }
                        GameFiber.Wait(2000);
                        LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("REPORT_RESPONSE_COPY_02");
                        GameFiber.Wait(1500);
                        if (SuspectVehicle.HasBone("door_dside_f"))
                        {
                            if (SuspectVehicle.Doors[0].IsValid())
                            {
                                SuspectVehicle.Doors[0].Close(false);
                            }
                        }
                        if (SuspectVehicle.HasBone("door_pside_f"))
                        {
                            if (SuspectVehicle.Doors[1].IsValid())
                            {
                                SuspectVehicle.Doors[1].Close(false);
                            }
                        }
                        if (SuspectVehicle.HasBone("door_dside_r"))
                        {
                            if (SuspectVehicle.Doors[2].IsValid())
                            {
                                SuspectVehicle.Doors[2].Close(false);
                            }
                        }
                        if (SuspectVehicle.HasBone("door_pside_r"))
                        {
                            if (SuspectVehicle.Doors[3].IsValid())
                            {
                                SuspectVehicle.Doors[3].Close(false);
                            }
                        }
                        if (SuspectVehicle.HasBone("boot"))
                        {
                            if (SuspectVehicle.Doors[5].IsValid())
                            {
                                SuspectVehicle.Doors[5].Close(false);
                            }
                        }
                    }
                    GameFiber.Wait(1500);
                }
                else  //if STP is installed
                {
                    System.Random r2 = new System.Random();
                    int EvidenceFound = r2.Next(0, 3);
                    switch (EvidenceFound)
                    {
                        case 0:
                            SuspectVehicle.Metadata.searchDriver = "~r~A Stolen Police Assault Rifle~w~, a ~g~Pair of Glasses~w~, ~g~A Buffalo Bills Cap~w~, and a ~g~Coca-Cola Bottle.";
                            WeaponName = "Assault Rifle";
                            Weapon = "WEAPON_CARBINERIFLE";
                            break;
                        case 1:
                            SuspectVehicle.Metadata.searchDriver = "~r~A Stolen Police Shotgun~w~, a ~g~Coconut~w~, a ~g~Green Bay Packers Cap~w~, and a ~g~Book.";
                            WeaponName = "Shotgun";
                            Weapon = "WEAPON_PUMPSHOTGUN";
                            break;
                        case 2:
                            SuspectVehicle.Metadata.searchDriver = "~r~A Stolen Police Rifle~w~, a ~g~Pack of Cigarettes~w~, and a ~g~Candy Bar.";
                            WeaponName = "Rifle";
                            Weapon = "WEAPON_ADVANCEDRIFLE";
                            break;
                    }
                    if (SuspectVehicle.Exists())
                    {
                        if (SuspectVehicle.HasBone("boot"))
                        {
                            if (SuspectVehicle.Doors[5].IsValid())
                            {
                                while (!SuspectVehicle.Doors[5].IsOpen) { GameFiber.Wait(0); }
                            }
                            GameFiber.Wait(6000);
                        }
                        else
                        {
                            Game.LogTrivial("YOBBINCALLOUTS: Cannot Detect Suspect Vehicle Door. Ending Callout.");
                            End();
                        }
                    }
                    else { End(); }
                    Game.DisplayNotification("Dispatch, We Have Recovered a Stolen Police ~r~" + WeaponName + " ~w~on Scene.");
                    GameFiber.Wait(2000);
                    if (MainScenario == 1) { Game.DisplayNotification("We Have also Recovered a Stolen ~r~Carbine Rifle~w~ Used in the Shootout."); }
                    GameFiber.Wait(2000);
                    LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("REPORT_RESPONSE_COPY_02");
                    GameFiber.Wait(1500);
                }
                if (MainScenario == 2)
                {
                    System.Random Runaway = new System.Random();
                    int ShouldRunaway = Runaway.Next(0, 2);
                    if (ShouldRunaway == 1 && Suspect.Exists())  //suspect runs away after search
                    {
                        Game.LogTrivial("YOBBINCALLOUTS: Suspect will attempt to flee after search.");
                        if (!Suspect.IsCuffed && !Functions.IsPedArrested(Suspect))
                        {
                            if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }
                            MainPursuit = LSPD_First_Response.Mod.API.Functions.CreatePursuit();
                            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("CRIME_SUSPECT_ON_THE_RUN_01");
                            LSPD_First_Response.Mod.API.Functions.RequestBackup(Suspect.Position, LSPD_First_Response.EBackupResponseType.Code3, LSPD_First_Response.EBackupUnitType.LocalUnit);
                            LSPD_First_Response.Mod.API.Functions.SetPursuitIsActiveForPlayer(MainPursuit, true);
                            LSPD_First_Response.Mod.API.Functions.AddPedToPursuit(MainPursuit, Suspect);
                            //while (Functions.IsPursuitStillRunning(MainPursuit)) { GameFiber.Wait(0); }
                            while (LSPD_First_Response.Mod.API.Functions.IsPursuitStillRunning(MainPursuit) && Suspect.IsAlive && !Suspect.IsCuffed) { GameFiber.Wait(0); }
                            while (Suspect.IsAlive && !Suspect.IsCuffed) { GameFiber.Wait(0); }
                            if (Suspect.IsDead) { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, Pursuit is Over. Suspect is ~r~Dead."); SuspectBlip.Delete(); }
                            if (Suspect.IsCuffed) { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, Pursuit is Over. Suspect is Under ~g~Arrest."); }
                            GameFiber.Wait(2000);
                            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("REPORT_RESPONSE_COPY_02");
                            GameFiber.Wait(2000);
                        }
                        else
                        {
                            Game.DisplayHelp("Arrest the ~r~Suspect. ~w~Press ~b~End~w~ When Done.");
                            while (!Game.IsKeyDown(Config.CalloutEndKey)) { GameFiber.Wait(0); }
                            Game.DisplayNotification("Dispatch, We are Code 4. We have ~b~Recovered the Stolen Police Weapons.");
                        }
                    }
                    else
                    {
                        Game.LogTrivial("YOBBINCALLOUTS: Suspect will NOT attempt to flee after search.");
                        Game.DisplayHelp("Arrest the ~r~Suspect. ~w~Press ~b~End~w~ When Done.");
                        while (!Game.IsKeyDown(Config.CalloutEndKey)) { GameFiber.Wait(0); }
                        Game.DisplayNotification("Dispatch, We are Code 4. We have ~b~Recovered the Stolen Police Weapons.");
                    }
                }
                End();
            }
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
            if (Suspect.Exists()) { Suspect.Dismiss(); }
            if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }

            //if (Config.RunInvestigations && !InvestigationHandler.StartInvestigation)
            //{
            //    Game.LogTrivial("YOBBINCALLOUTS: Started a New Investigation.");
            //    WorldZone = Functions.GetZoneAtPosition(MainSpawnPoint).County;
            //    if (WorldZone == LSPD_First_Response.Engine.Scripting.EWorldZoneCounty.LosSantos || WorldZone == LSPD_First_Response.Engine.Scripting.EWorldZoneCounty.LosSantosCounty)
            //    {
            //        InvestigationHandler.InvestigationLocation = new Vector3(450.0654f, -993.0596f, 30f);   //Downtown Police Station
            //    }
            //    else
            //    {
            //        InvestigationHandler.InvestigationLocation = new Vector3(1848.73f, 3689.98f, 34.27f);   //Sandy Shores Sheriff Station
            //    }
            //    Game.LogTrivial("YOBBINCALLOUTS: Found Investigation Location.");
            //    InvestigationHandler.InvestigationName = "Stolen Police Hardware Investigation";
            //    Game.LogTrivial("YOBBINCALLOUTS: Stored Investigation Data.");
            //    InvestigationHandler.StartInvestigation = true;
            //    InvestigationHandler.OnBeforeInvestigationStarted();
            //    Game.LogTrivial("YOBBINCALLOUTS: Started investigation handler. Ending Callout.");
            //}
            Game.LogTrivial("YOBBINCALLOUTS: Stolen Police Hardware Callout Finished Cleaning Up.");
        }
        public override void Process()
        {
            base.Process();
        }
    }
}

