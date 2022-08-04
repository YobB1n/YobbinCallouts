using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using System;

namespace YobbinCallouts.Callouts
{
    [CalloutInfo("Active Shooter", CalloutProbability.Medium)]
    class ActiveShooter : Callout
    {
        private Vector3 MainSpawnPoint;

        private Ped Suspect;
        private Ped Suspect2;
        private Vehicle SuspectVehicle;
        private Blip SuspectBlip;
        private Ped player = Game.LocalPlayer.Character;
        private LHandle MainPursuit;

        private int MainScenario;
        private bool CalloutRunning;

        public override bool OnBeforeCalloutDisplayed()
        {
            Game.LogTrivial("==========YOBBINCALLOUTS: Active Shooter Callout Start==========");
            System.Random r = new System.Random();
            int Scenario = r.Next(1, 2); //scenario 0 (vehicle) disabled
            MainScenario = Scenario;
            Game.LogTrivial("YOBBINCALLOUTS: Scenario value is: " + MainScenario);

            MainSpawnPoint = World.GetNextPositionOnStreet(player.Position.Around(600));
            ShowCalloutAreaBlipBeforeAccepting(MainSpawnPoint, 25f);
            AddMinimumDistanceCheck(60f, MainSpawnPoint);
            Functions.PlayScannerAudio("ATTENTION_ALL_SWAT_UNITS_01 WE_HAVE_01 CRIME_ASSAULT_WITH_A_DEADLY_WEAPON_01 UNITS_RESPOND_CODE_99_01");
            CalloutMessage = "Active Shooter";
            CalloutPosition = MainSpawnPoint;
            if (MainScenario == 0) CalloutAdvisory = "Two Suspects are Reported ~r~Discharging a Firearm ~w~at Random from a Vehicle.";
            else CalloutAdvisory = "Suspect is Reported ~r~Discharging a Firearm ~w~at Random.";
            return base.OnBeforeCalloutDisplayed();
        }
        public override bool OnCalloutAccepted()
        {
            try
            {
                Game.LogTrivial("YOBBINCALLOUTS: Active Shooter Callout Accepted by User");
                if (Main.CalloutInterface)
                {
                    CalloutInterfaceHandler.SendCalloutDetails(this, "CODE 99", "");
                }
                else
                {
                    Game.DisplayNotification("Respond ~r~Code 99, ~w~Emergency.~w~");
                }

                if (MainScenario == 0) //vehicle
                {
                    string[] Vehicles = new string[16] { "zion", "oracle", "tampa", "virgo", "serrano", "asea", "asterope", "ingot", "primo2", "premier", "regina", "stratum", "washington", "baller", "huntley", "mesa" };
                    System.Random r = new System.Random();
                    int VictimVeh = r.Next(0, Vehicles.Length);

                    SuspectVehicle = new Vehicle(Vehicles[VictimVeh], MainSpawnPoint);
                    SuspectVehicle.IsPersistent = true;
                    SuspectBlip = SuspectVehicle.AttachBlip();
                    SuspectBlip.IsFriendly = false;
                    SuspectBlip.IsRouteEnabled = true;

                    Suspect = SuspectVehicle.CreateRandomDriver();
                    Suspect.IsPersistent = true;
                    Suspect.BlockPermanentEvents = true;
                    Suspect.RelationshipGroup = RelationshipGroup.Gang1;
                    Suspect.RelationshipGroup.SetRelationshipWith(Game.LocalPlayer.Character.RelationshipGroup, Relationship.Hate);
                    Suspect.RelationshipGroup.SetRelationshipWith(RelationshipGroup.Cop, Relationship.Hate);

                    System.Random r3 = new System.Random();  //Instantiate Random Weapon  generator
                    int WeaponModel = r3.Next(0, 5);    //Use Random Weapon generator
                    Game.LogTrivial("YOBBINCALLOUTS: Suspect 1 Weapon Model is " + WeaponModel);

                    if (WeaponModel == 0) Suspect.Inventory.GiveNewWeapon("WEAPON_ASSAULTRIFLE", -1, true);
                    else if (WeaponModel == 1) Suspect.Inventory.GiveNewWeapon("WEAPON_SMG", -1, true);
                    else if (WeaponModel == 2) Suspect.Inventory.GiveNewWeapon("WEAPON_APPISTOL", -1, true);
                    else if (WeaponModel == 3) Suspect.Inventory.GiveNewWeapon("WEAPON_MICROSMG", -1, true);
                    else if (WeaponModel == 4) Suspect.Inventory.GiveNewWeapon("WEAPON_COMPACTRIFLE", -1, true);

                    Suspect2 = new Ped(SuspectVehicle.Position);
                    Suspect2.WarpIntoVehicle(SuspectVehicle, -2); //not working
                    Suspect2.IsPersistent = true;
                    Suspect2.BlockPermanentEvents = true;
                    Suspect2.RelationshipGroup = RelationshipGroup.Gang1;
                    Suspect2.RelationshipGroup.SetRelationshipWith(Game.LocalPlayer.Character.RelationshipGroup, Relationship.Hate);
                    Suspect2.RelationshipGroup.SetRelationshipWith(RelationshipGroup.Cop, Relationship.Hate);

                    System.Random r2 = new System.Random();  //Instantiate Random Weapon  generator
                    int WeaponModel2 = r2.Next(0, 5);    //Use Random Weapon generator
                    Game.LogTrivial("YOBBINCALLOUTS: Suspect 2 Weapon Model is " + WeaponModel);

                    if (WeaponModel == 0) Suspect2.Inventory.GiveNewWeapon("WEAPON_PISTOL", -1, true);
                    else if (WeaponModel == 1) Suspect2.Inventory.GiveNewWeapon("WEAPON_COMBATPISTOL", -1, true);
                    else if (WeaponModel == 2) Suspect2.Inventory.GiveNewWeapon("WEAPON_APPISTOL", -1, true);
                    else if (WeaponModel == 3) Suspect2.Inventory.GiveNewWeapon("WEAPON_PISTOL50", -1, true);
                    else if (WeaponModel == 4) Suspect2.Inventory.GiveNewWeapon("WEAPON_APPISTOL", -1, true);

                    Ped[] Randos = World.GetAllPeds();
                    for (int i = 0; i < 25; i++)
                    {
                        GameFiber.Yield();
                        if (Randos[i].Exists())
                        {
                            if (Randos[i] != player && Randos[i] != Suspect) Suspect.RelationshipGroup.SetRelationshipWith(Randos[i].RelationshipGroup, Relationship.Hate);
                            if (Randos[i] != player && Randos[i] != Suspect2) Suspect2.RelationshipGroup.SetRelationshipWith(Randos[i].RelationshipGroup, Relationship.Hate);
                        }
                    }
                }
                else //just one dude
                {
                    Suspect = new Ped(MainSpawnPoint);
                    Suspect.Health = 150;
                    Suspect.Armor = 200;
                    Suspect.BlockPermanentEvents = true;
                    Suspect.IsPersistent = true;

                    System.Random r = new System.Random();  //Instantiate Random Weapon  generator
                    int WeaponModel = r.Next(0, 5);    //Use Random Weapon generator
                    Game.LogTrivial("YOBBINCALLOUTS: Weapon Model is " + WeaponModel);

                    if (WeaponModel == 0) Suspect.Inventory.GiveNewWeapon("WEAPON_ASSAULTRIFLE", -1, true);
                    else if (WeaponModel == 1) Suspect.Inventory.GiveNewWeapon("WEAPON_SMG", -1, true);
                    else if (WeaponModel == 2) Suspect.Inventory.GiveNewWeapon("WEAPON_APPISTOL", -1, true);
                    else if (WeaponModel == 3) Suspect.Inventory.GiveNewWeapon("weapon_sawnoffshotgun", -1, true);
                    else if (WeaponModel == 4) Suspect.Inventory.GiveNewWeapon("weapon_compactrifle", -1, true);

                    SuspectBlip = Suspect.AttachBlip();
                    SuspectBlip.IsRouteEnabled = true;
                    SuspectBlip.IsFriendly = false;
                    Suspect.RelationshipGroup = RelationshipGroup.Gang1;
                    Suspect.RelationshipGroup.SetRelationshipWith(Game.LocalPlayer.Character.RelationshipGroup, Relationship.Hate);
                    Suspect.RelationshipGroup.SetRelationshipWith(RelationshipGroup.Cop, Relationship.Hate);

                    Ped[] Randos = World.GetAllPeds();
                    for (int i = 0; i < 25; i++)
                    {
                        try
                        {
                            if (Randos[i].Exists())
                            {
                                if (Randos[i] != player && Randos[i] != Suspect) Suspect.RelationshipGroup.SetRelationshipWith(Randos[i].RelationshipGroup, Relationship.Hate);
                            }
                        }
                        catch (System.IndexOutOfRangeException)
                        {
                            Game.LogTrivial("YOBBINCALLOUTS: Index out of Bounds Exception caught.");
                            break;
                        }
                        GameFiber.Yield();
                    }
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
            Game.LogTrivial("YOBBINCALLOUTS: Active Shooter Callout Not Accepted by User.");
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
                        System.Random rondom = new System.Random();  //Instantiate Random WaitTime generator
                        int WaitTime = rondom.Next(100, 225);    //Use Random WaitTime generator
                        Game.LogTrivial("YOBBINCALLOUTS: Suspect will fire when player is " + WaitTime + " metres away.");
                        while (player.DistanceTo(Suspect) >= WaitTime && !Game.IsKeyDown(Config.CalloutEndKey)) { GameFiber.Wait(0); }
                        if (Game.IsKeyDown(Config.CalloutEndKey)) { EndCalloutHandler.CalloutForcedEnd = true; break; }
                        if (Main.CalloutInterface) CalloutInterfaceHandler.SendMessage(this, "ALL UNITS: Warning - Use Extreme Caution Suspect is Firing at Random");

                        if (MainScenario == 0)
                        {
                            Suspect.Tasks.CruiseWithVehicle(12.5f, VehicleDrivingFlags.AllowWrongWay | VehicleDrivingFlags.DriveAroundVehicles);
                            Suspect2.Tasks.FightAgainstClosestHatedTarget(WaitTime, -1);

                            Game.LogTrivial("YOBBINCALLOUTS: Suspect Pursuit Started");
                            Functions.ForceEndCurrentPullover();
                            MainPursuit = LSPD_First_Response.Mod.API.Functions.CreatePursuit();
                            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("CRIME_SUSPECT_ON_THE_RUN_01");
                            Game.DisplayNotification("Suspect is ~r~Evading!");
                            LSPD_First_Response.Mod.API.Functions.RequestBackup(player.Position, LSPD_First_Response.EBackupResponseType.Code3, LSPD_First_Response.EBackupUnitType.LocalUnit);
                            LSPD_First_Response.Mod.API.Functions.SetPursuitIsActiveForPlayer(MainPursuit, true);
                            LSPD_First_Response.Mod.API.Functions.AddPedToPursuit(MainPursuit, Suspect);
                            LSPD_First_Response.Mod.API.Functions.AddPedToPursuit(MainPursuit, Suspect2);
                            while (Functions.IsPursuitStillRunning(MainPursuit)) GameFiber.Wait(0);
                            while (Suspect.Exists() || Suspect2.Exists())
                            {
                                GameFiber.Yield();
                                if (!Suspect.Exists() || Suspect.IsDead || Functions.IsPedArrested(Suspect))
                                {
                                    if (!Suspect2.Exists() || Suspect2.IsDead || Functions.IsPedArrested(Suspect2)) break;
                                }
                            }
                            break;
                        }
                        else
                        {
                            Suspect.Tasks.FightAgainstClosestHatedTarget(WaitTime, -1);
                            Suspect.IsInvincible = false;
                            SuspectBlip.IsRouteEnabled = false;
                            //Suspect.Health = 300;
                            //Functions.PlayScannerAudio("CRIME_OFFICER_IN_NEED_OF_ASSISTANCE_02 CRIME_SHOTS_FIRED_AT_AN_OFFICER_01");
                            //Game.DisplayNotification("Dispatch, We Need Backup ASAP, Heavy ~r~Gunfire!");
                            //GameFiber.Wait(2500);
                            //Functions.RequestBackup(MainSpawnPoint, LSPD_First_Response.EBackupResponseType.Code3, LSPD_First_Response.EBackupUnitType.LocalUnit);
                            //Functions.RequestBackup(MainSpawnPoint, LSPD_First_Response.EBackupResponseType.Code3, LSPD_First_Response.EBackupUnitType.SwatTeam);
                            while (Suspect.IsAlive && Suspect.Exists()) GameFiber.Wait(0);
                            break;
                        }
                    }
                    if (MainScenario == 0)
                    {
                        if (Main.CalloutInterface) CalloutInterfaceHandler.SendMessage(this, "Code 4, Suspects Neutralized.");
                        else Game.DisplayNotification("~g~Code 4, ~w~Suspects Neutralized.");
                    }
                    else
                    {
                        if (Main.CalloutInterface) CalloutInterfaceHandler.SendMessage(this, "Code 4, Suspect Neutralized.");
                        else Game.DisplayNotification("~g~Code 4, ~w~Suspect Neutralized.");
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
            Game.LogTrivial("YOBBINCALLOUTS: Active Shooter Callout Finished Cleaning Up.");
        }
        public override void Process()
        {
            base.Process();
        }
    }
}
