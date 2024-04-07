using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using System;
using CalloutInterfaceAPI;
using Rage.Native;
using UltimateBackup;

namespace YobbinCallouts.Callouts
{
    [CalloutInterface("Active Shooter", CalloutProbability.Medium, "Active Shooter", "Code 99")] //test to see if this works without calloutinterface installed!!
    [CalloutInfo("Active Shooter", CalloutProbability.High)]
    public class ActiveShooter : Callout
    {
        private Vector3 MainSpawnPoint;

        private Ped Suspect;
        private Ped Suspect2;
        private Ped Suspect3;
        private Ped Suspect4;
        public static Ped tempsuspect;
        private Vehicle SuspectVehicle;
        private Blip SuspectBlip;
        private Blip SuspectBlip2;
        private Blip SuspectBlip3;
        private Blip SuspectBlip4;
        private Blip AreaBlip;
        private Ped player = Game.LocalPlayer.Character;
        private LHandle MainPursuit;

        private int MainScenario;
        private bool CalloutRunning;

        //TERRORIST INFO
        public static string OrganizationName;
        public static string[] Organizations = new string[] { "Vagos", "Bills Mafia", "New Order", };

        public static Vector3 AttackLocation;

        //last element [4] is the type of facility each organization will target
        public static string[] OrganizationMessage;
        public static string[] ArmedAlliance = new string[] { "Weapons aquisition", "grow our weapons stockpile", "arm our members", "access more weapons", "police station" };
        public static string[] FreedomFighters = new string[] { "Eliminate the government", "disarm the police state", "dismantle the police", "overthrow the government", "police station" };
        public static string[] NewOrder = new string[] { "establish new rule", "dismantle the system", "send a message", "damage the system", "hospital" };

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
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("ATTENTION_ALL_SWAT_UNITS_01 WE_HAVE_01 CRIME_ASSAULT_WITH_A_DEADLY_WEAPON_01 UNITS_RESPOND_CODE_99_01");
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
                    //CalloutInterfaceHandler.SendCalloutDetails(this, "CODE 99", "");
                }
                else
                {
                    Game.DisplayNotification("Respond ~r~Code 99, ~w~Emergency.~w~");
                }

                if (MainScenario == 0) //vehicle
                {
                    SpawnVehicle(MainSpawnPoint);
                }
                else //just one dude
                {
                    Suspect = new Ped(MainSpawnPoint)
                    {
                        Health = 150,
                        Armor = 200,
                        BlockPermanentEvents = true,
                        IsPersistent = true
                    };

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
                        int WaitTime = rondom.Next(200, 300);    //Use Random WaitTime generator
                        Game.LogTrivial("YOBBINCALLOUTS: Suspect will fire when player is " + WaitTime + " metres away.");
                        while (player.DistanceTo(Suspect) >= WaitTime && !Game.IsKeyDown(Config.CalloutEndKey)) { GameFiber.Wait(0); }
                        if (Game.IsKeyDown(Config.CalloutEndKey)) { EndCalloutHandler.CalloutForcedEnd = true; break; }
                        if (Main.CalloutInterface) CalloutInterfaceHandler.SendMessage(this, "ALL UNITS: Warning - Use Extreme Caution - Suspect is Firing at Random");

                        if (MainScenario == 0)
                        {
                            Suspect.Tasks.CruiseWithVehicle(12.5f, VehicleDrivingFlags.AllowWrongWay | VehicleDrivingFlags.DriveAroundVehicles);
                            Suspect2.Tasks.FightAgainstClosestHatedTarget(WaitTime, -1);

                            Game.LogTrivial("YOBBINCALLOUTS: Suspect Pursuit Started");
                            LSPD_First_Response.Mod.API.Functions.ForceEndCurrentPullover();
                            MainPursuit = LSPD_First_Response.Mod.API.Functions.CreatePursuit();
                            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("CRIME_SUSPECT_ON_THE_RUN_01");
                            Game.DisplayNotification("Suspect is ~r~Evading!");
                            LSPD_First_Response.Mod.API.Functions.RequestBackup(player.Position, LSPD_First_Response.EBackupResponseType.Code3, LSPD_First_Response.EBackupUnitType.LocalUnit);
                            LSPD_First_Response.Mod.API.Functions.SetPursuitIsActiveForPlayer(MainPursuit, true);
                            LSPD_First_Response.Mod.API.Functions.AddPedToPursuit(MainPursuit, Suspect);
                            LSPD_First_Response.Mod.API.Functions.AddPedToPursuit(MainPursuit, Suspect2);
                            while (LSPD_First_Response.Mod.API.Functions.IsPursuitStillRunning(MainPursuit)) GameFiber.Wait(0);
                            while (Suspect.Exists() || Suspect2.Exists())
                            {
                                GameFiber.Yield();
                                if (!Suspect.Exists() || Suspect.IsDead || LSPD_First_Response.Mod.API.Functions.IsPedArrested(Suspect))
                                {
                                    if (!Suspect2.Exists() || Suspect2.IsDead || LSPD_First_Response.Mod.API.Functions.IsPedArrested(Suspect2)) break;
                                }
                            }
                            if (Main.CalloutInterface) CalloutInterfaceHandler.SendMessage(this, "Code 4, Suspects Neutralized.");
                            else Game.DisplayNotification("~g~Code 4, ~w~Suspects Neutralized.");
                            break;
                        }
                        else
                        {
                            Suspect.Tasks.FightAgainstClosestHatedTarget(WaitTime, -1);
                            Suspect.IsInvincible = false;
                            SuspectBlip.IsRouteEnabled = false;
                            SuspectBlip.Flash(500, -1);
                            //Suspect.Health = 300;
                            //LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("CRIME_OFFICER_IN_NEED_OF_ASSISTANCE_02 CRIME_SHOTS_FIRED_AT_AN_OFFICER_01");
                            //Game.DisplayNotification("Dispatch, We Need Backup ASAP, Heavy ~r~Gunfire!");
                            //GameFiber.Wait(2500);
                            //Functions.RequestBackup(MainSpawnPoint, LSPD_First_Response.EBackupResponseType.Code3, LSPD_First_Response.EBackupUnitType.LocalUnit);
                            //Functions.RequestBackup(MainSpawnPoint, LSPD_First_Response.EBackupResponseType.Code3, LSPD_First_Response.EBackupUnitType.SwatTeam);
                            CallHandler.SuspectWait(Suspect); //should be good, double-check this

                            //In case player too far away from an attack location...
                            CallHandler.locationChooser(CallHandler.PoliceStationList, 1600, 125);
                            if (CallHandler.locationReturned)
                            {
                                CallHandler.locationChooser(CallHandler.HospitalList, 1600, 125);
                                //ADD OPTION TO SKIP THIS?
                                if (CallHandler.locationReturned) Terrorist();
                                else
                                {
                                    Game.LogTrivial("YOBBINCALLOUTS: NO HOSPITAL ATTACK LOCATION FOUND, ENDING CALLOUT.");
                                    if (Main.CalloutInterface) CalloutInterfaceHandler.SendMessage(this, "Code 4, Suspect Neutralized.");
                                    else Game.DisplayNotification("~g~Code 4, ~w~Suspect Neutralized.");
                                }
                            }
                            else
                            {
                                Game.LogTrivial("YOBBINCALLOUTS: NO POLICE STATION ATTACK LOCATION FOUND, ENDING CALLOUT.");
                                if (Main.CalloutInterface) CalloutInterfaceHandler.SendMessage(this, "Code 4, Suspect Neutralized.");
                                else Game.DisplayNotification("~g~Code 4, ~w~Suspect Neutralized.");
                            }
                            if (SuspectBlip.Exists()) SuspectBlip.StopFlashing();
                            break;
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
        private void Terrorist()
        {
            Game.LogTrivial("YOBBINCALLOUTS: TERRORIST SCENARIO CONTINUING...");
            GameFiber.Wait(2000);
            Game.DisplayHelp("Search the ~r~Suspect.");
            if (Suspect.Exists()) //test this with STP too!
            {
                while (player.DistanceTo(Suspect) >= 2.5f) GameFiber.Wait(0);
            }
            GameFiber.Wait(1500);
            Game.DisplaySubtitle("~g~You: ~w~Hm, what's this piece of paper here?", 2000);
            GameFiber.Wait(2500);

            int orgchoice = CallHandler.RNG(3);
            OrganizationName = Organizations[orgchoice];
            if (orgchoice == 0) OrganizationMessage = ArmedAlliance;
            else if (orgchoice == 1) OrganizationMessage = FreedomFighters;
            else OrganizationMessage = NewOrder;

            if (orgchoice <= 1)
            {
                CallHandler.locationChooser(CallHandler.PoliceStationList, 1600, 125);
                AttackLocation = CallHandler.SpawnPoint;
            }
            else
            {
                CallHandler.locationChooser(CallHandler.HospitalList, 1600, 125);
                AttackLocation = CallHandler.SpawnPoint;
            }
            //spawn stuff at attack location
            SpawnVehicle(AttackLocation);

            Game.LogTrivial("YOBBINCALLOUTS: ORGANIZATION IS " + orgchoice + ", " + OrganizationName);
            // element [4] in OrganizationMessage is the name of the target.
            Game.DisplayNotification("Thank you for your sacrifice to the ~o~Cause~w~, to " + OrganizationMessage[CallHandler.RNG(3)] + ". You are an important part of ~r~" + OrganizationName + "~w~." +
                " While the cops are busy over there, we'll hit the " + OrganizationMessage[4] + " in ~o~" + LSPD_First_Response.Mod.API.Functions.GetZoneAtPosition(AttackLocation).RealAreaName + "~w~!");
            GameFiber.Wait(6500);
            Game.DisplaySubtitle("~g~You:~w~ Shit. Dispatch, we've got another attack planned by the ~r~" + OrganizationName + "~w~!! It's at the ~b~" + OrganizationMessage[4] + "~w~ in ~o~"
                + LSPD_First_Response.Mod.API.Functions.GetZoneAtPosition(AttackLocation).RealAreaName + "~w~!");
            GameFiber.Wait(4000);
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("REPORT_RESPONSE_COPY_02");
            GameFiber.Wait(2500);
            AreaBlip = new Blip(AttackLocation, 125f);
            AreaBlip.Color = System.Drawing.Color.Red; AreaBlip.Alpha = 0.69f; AreaBlip.IsRouteEnabled = true; AreaBlip.Name = "Planned Attack Location";
            Game.DisplayHelp("Get to the ~r~Attack Location~w~ as soon as possible!");
            if (SuspectBlip.Exists()) SuspectBlip.StopFlashing();
            if (SuspectBlip.Exists()) SuspectBlip.Delete();

            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("ATTENTION_ALL_SWAT_UNITS_01 WE_HAVE_01 CRIME_TERRORIST_ACTIVITY_01 CRIME_ASSAULT_WITH_A_DEADLY_WEAPON_01 UNITS_RESPOND_CODE_99_01"); //update
            if (Main.CalloutInterface) CalloutInterfaceHandler.SendMessage(this, "ALL UNITS: POTENTIAL TERRORIST ATTACK REPORTED AT THE " + OrganizationMessage[4] + " in or near " + LSPD_First_Response.Mod.API.Functions.GetZoneAtPosition(AttackLocation).RealAreaName);
            if (Main.CalloutInterface) CalloutInterfaceHandler.SendMessage(this, "ALL UNITS RESPOND CODE 99 - USE EXTREME CAUTION. SWAT UNITS HAVE BEEN DISPATCHED.");

            LSPD_First_Response.Mod.API.Functions.RequestBackup(World.GetNextPositionOnStreet(AttackLocation.Around(15)), LSPD_First_Response.EBackupResponseType.Code3, LSPD_First_Response.EBackupUnitType.SwatTeam);
            LSPD_First_Response.Mod.API.Functions.RequestBackup(World.GetNextPositionOnStreet(AttackLocation.Around(15)), LSPD_First_Response.EBackupResponseType.Code3, LSPD_First_Response.EBackupUnitType.LocalUnit);

            while (player.DistanceTo(AttackLocation) >= 125f) GameFiber.Wait(0);
            Game.DisplayHelp("Search the ~r~Area~w~ for any ~o~Suspicious Activity.");

            if (Main.CalloutInterface) CalloutInterfaceHandler.SendMessage(this, "Intel obtained - attackers are likely driving a utility van. Potentially a " + SuspectVehicle.Model.Name);
            else Game.DisplayNotification("~g~Dispatch:~w~ Intel obtained - attackers are likely driving a ~o~utility van~w~. Potentially a ~r~" + SuspectVehicle.Model.Name);
            //ONE SCENARIO FOR NOW, WILL ADD MORE LATER   

            //edge case tests for this scenario - kill certain combinations of enemies earlier...
            
            //Game.LogTrivial("YOBBINCALLOUTS: WAITING " + WaitTime / 1000 + " SECONDS OR UNTIL PLAYER FINDS SUSPECTS");
            //suspect2 is driver
            //while (player.DistanceTo(SuspectVehicle) >= 20f && Suspect2.Exists() && Suspect2.IsAlive && !player.IsShooting) //test this
            //{
            //    if (player.DistanceTo(SuspectVehicle) <= 20f || !Suspect2.Exists() || !Suspect2.IsAlive || player.IsShooting) break;
            //    GameFiber.Wait(WaitTime / 2);
            //    Game.DisplayNotification("~g~Dispatch:~w~ Intel obtained - attackers are likely driving a ~o~utility van~w~. Potentially a ~r~" + SuspectVehicle.Model.Name);
            //    GameFiber.Wait(WaitTime / 2);
            //    Game.LogTrivial("YOBBINCALLOUTS: STARTED ATTACK...");
            //    break;
            //}
            //rng for attack   
            if (CallHandler.FiftyFifty()) //test this scenario...
            {
                Game.LogTrivial("SUSPECT DRIVING TO LOCATION 5 SECONDS");
                Suspect2.Tasks.DriveToPosition(AttackLocation, 20f, VehicleDrivingFlags.DriveAroundVehicles | VehicleDrivingFlags.DriveAroundPeds |VehicleDrivingFlags.Emergency | VehicleDrivingFlags.AllowWrongWay).WaitForCompletion(5000);
            }
            else
            {
                Game.LogTrivial("SUSPECT WAITING");
                while (player.DistanceTo(SuspectVehicle) >= 25f && Suspect2.Exists() && Suspect2.IsAlive && !player.IsShooting && !player.IsAiming) GameFiber.Wait(0);
            }

            SuspectBlip = CallHandler.AssignBlip(SuspectVehicle, System.Drawing.Color.Red, 1, "Attack Vehicle", false);
            //add this for more peeps later
            Suspect2.Tasks.LeaveVehicle(SuspectVehicle, LeaveVehicleFlags.None).WaitForCompletion();
            SuspectBlip2 = CallHandler.AssignBlip(Suspect2, System.Drawing.Color.Red, 0.5f);
            Suspect2.Tasks.FightAgainstClosestHatedTarget(50, -1);
            //Suspect2.IsInvincible = false;
            Suspect3.Tasks.LeaveVehicle(SuspectVehicle, LeaveVehicleFlags.None).WaitForCompletion();
            SuspectBlip3 = CallHandler.AssignBlip(Suspect3, System.Drawing.Color.Red, 0.5f);
            Suspect3.Tasks.FightAgainstClosestHatedTarget(50, -1);
            Suspect4.Tasks.LeaveVehicle(SuspectVehicle, LeaveVehicleFlags.None).WaitForCompletion();
            SuspectBlip4 = CallHandler.AssignBlip(Suspect4, System.Drawing.Color.Red, 0.5f);
            Suspect4.Tasks.FightAgainstClosestHatedTarget(50, -1);
            if (AreaBlip.Exists()) AreaBlip.Delete();
            if (Main.UB)
            {
                UltimateBackup.API.Functions.callCode3SwatBackup(false, false);
            }
            //Suspect.Health = 300;
            //LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("CRIME_OFFICER_IN_NEED_OF_ASSISTANCE_02 CRIME_SHOTS_FIRED_AT_AN_OFFICER_01");
            //Game.DisplayNotification("Dispatch, We Need Backup ASAP, Heavy ~r~Gunfire!");

            //test this
            Game.LogTrivial("YOBBINCALLOUTS: Waiting for battle end.");
            while (Suspect2.Exists() || Suspect3.Exists() || Suspect4.Exists())
            {
                GameFiber.Yield();
                if (!Suspect2.Exists() || Suspect2.IsDead || LSPD_First_Response.Mod.API.Functions.IsPedArrested(Suspect2))
                {
                    if (!Suspect3.Exists() || Suspect3.IsDead || LSPD_First_Response.Mod.API.Functions.IsPedArrested(Suspect3))
                    {
                        if (!Suspect4.Exists() || Suspect4.IsDead || LSPD_First_Response.Mod.API.Functions.IsPedArrested(Suspect4))
                        {
                            break;
                        }
                    }
                }
            }
            SuspectVehicle.EngineHealth = 0f;
            GameFiber.Wait(2500);
            Game.DisplayNotification("Dispatch, multiple suspects have been killed in the ~r~Attack.");
            GameFiber.Wait(2500);
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("REPORT_RESPONSE_COPY_02");
            
            GameFiber.Wait(CallHandler.RNG(0, 7000, 12000));
            if (CallHandler.FiftyFifty() && SuspectVehicle.Exists()) //bomb
            {
                Game.LogTrivial("YOBBINCALLOUTS: BOMB AFTER SHOOTING");
                SuspectVehicle.IsOnFire = true;
                int timetoexplode = CallHandler.RNG(0, 6500, 10000);
                SuspectVehicle.AlarmTimeLeft = TimeSpan.FromMilliseconds(timetoexplode);
                GameFiber.Wait(1500);
                Game.DisplaySubtitle("~g~You:~w~ Why did the alarm start sounding?! ~r~Everyone get away from the van!!", 3000);
                GameFiber.Wait(timetoexplode);
                SuspectVehicle.Explode();
                if (Config.CallFD)
                {
                    try { LSPD_First_Response.Mod.API.Functions.RequestBackup(SuspectVehicle.Position, LSPD_First_Response.EBackupResponseType.Code3, LSPD_First_Response.EBackupUnitType.Firetruck); }
                    catch (System.NullReferenceException) { Game.LogTrivial("YOBBINCALLOUTS: Error Spawning LSPDFR Fire Truck."); }
                    Game.LogTrivial("YOBBINCALLOUTS: Fire Department Has Been Called");
                    Game.DisplayNotification("~r~Fire Department~w~ is En Route!");
                }
                GameFiber.Wait(6500);
                Game.DisplayHelp("Press End to ~b~Finish~w~ the Callout when done at the ~o~Scene.");
                while (!Game.IsKeyDown(System.Windows.Forms.Keys.End)) GameFiber.Wait(0);
            }
        }
        private void SpawnVehicle(Vector3 SpawnPoint)
        {
            Game.LogTrivial("YOBBINCALLOUTS: Started spawning suspect vehicle");
            string[] Vehicles = new string[] { "SPEEDO4", "BURRITO", "BURRITO3", "BURRITO4", "YOUGA2" }; //make sure all these vehicles work with the peds, maybe add more types?
            System.Random r = new System.Random();
            int VictimVeh = r.Next(0, Vehicles.Length);
            Game.LogTrivial("YOBBINCALLOUTS: Suspect vehicle is " + VictimVeh);

            //spawn the suspects vehicle
            Vector3 randompoint = World.GetNextPositionOnStreet(SpawnPoint.Around(15f)); //test this
            NativeFunction.Natives.GetClosestVehicleNodeWithHeading(randompoint, out Vector3 nodePosition, out float heading, 1, 3.0f, 0);
            bool success = NativeFunction.Natives.xA0F8A7517A273C05<bool>(randompoint, heading, out Vector3 outPosition);
            if (success)
            {
                SuspectVehicle = new Vehicle(Vehicles[VictimVeh], outPosition, heading);
            }
            else
            {
                SuspectVehicle = new Vehicle(Vehicles[VictimVeh], World.GetNextPositionOnStreet(SpawnPoint));
            }
            SuspectVehicle.IsPersistent = true;

            //spawn a decoy similar suspects vehicle
            //Vector3 randompoint2 = SuspectVehicle.Position.Around(25f);
            //NativeFunction.Natives.GetClosestVehicleNodeWithHeading(randompoint, out Vector3 nodePosition2, out float heading2, 1, 3.0f, 0);
            //bool success2 = NativeFunction.Natives.xA0F8A7517A273C05<bool>(randompoint, heading2, out Vector3 outPosition2);
            //if (success2)
            //{
            //    Vehicle SuspectVehicle2 = new Vehicle(Vehicles[VictimVeh], outPosition2, heading2);
            //}
            //else
            //{
            //    Vehicle SuspectVehicle2 = new Vehicle(Vehicles[VictimVeh], World.GetNextPositionOnStreet(randompoint2));
            //}
            //Game.LogTrivial("YOBBINCALLOUTS: SPAWNED DECOY VEHICLE.");

            //Suspect2 is always driver.
            Suspect2 = SuspectVehicle.CreateRandomDriver();
            Suspect3 = new Ped(SuspectVehicle.Position);
            Suspect3.WarpIntoVehicle(SuspectVehicle, -2);
            Suspect4 = new Ped(SuspectVehicle.Position);
            Suspect4.WarpIntoVehicle(SuspectVehicle, -2);

            for (int i = 0; i < 3; i++)
            {
                Game.LogTrivial("YOBBINCALLOUTS: Started spawning Suspect" + i);

                if (i == 0)
                {
                    if (Suspect2.Exists()) tempsuspect = Suspect2;
                    else break;
                }
                else if (i == 1)
                {
                    if (Suspect3.Exists()) tempsuspect = Suspect3;
                    else break;
                }
                else
                {
                    if (i == 2)
                    {
                        if (Suspect3.Exists()) tempsuspect = Suspect4;
                        else break;
                    }
                }
                tempsuspect.IsPersistent = true;
                tempsuspect.BlockPermanentEvents = true;
                tempsuspect.RelationshipGroup = RelationshipGroup.Gang1;
                tempsuspect.RelationshipGroup.SetRelationshipWith(Game.LocalPlayer.Character.RelationshipGroup, Relationship.Hate);
                tempsuspect.RelationshipGroup.SetRelationshipWith(RelationshipGroup.Cop, Relationship.Hate);
                tempsuspect.Health = 150;
                tempsuspect.Armor = 200;

                System.Random r3 = new System.Random();  //Instantiate Random Weapon  generator
                int WeaponModel = r3.Next(0, 5);    //Use Random Weapon generator
                Game.LogTrivial("YOBBINCALLOUTS: Suspect" + i + " Weapon Model is " + WeaponModel);
                if (WeaponModel == 0) tempsuspect.Inventory.GiveNewWeapon("WEAPON_ASSAULTRIFLE", -1, true);
                else if (WeaponModel == 1) tempsuspect.Inventory.GiveNewWeapon("WEAPON_SMG", -1, true);
                else if (WeaponModel == 2) tempsuspect.Inventory.GiveNewWeapon("WEAPON_APPISTOL", -1, true);
                else if (WeaponModel == 3) tempsuspect.Inventory.GiveNewWeapon("WEAPON_MICROSMG", -1, true);
                else if (WeaponModel == 4) tempsuspect.Inventory.GiveNewWeapon("WEAPON_COMPACTRIFLE", -1, true);
                //tempsuspect.Inventory.EquippedWeapon.Ammo = tempsuspect.Inventory.EquippedWeapon.MaximumAmmo;
            }

            //idk if I need this loop for all peds

            //Ped[] Randos = World.GetAllPeds();
            //for (int i = 0; i < 25; i++)
            //{
            //    GameFiber.Yield();
            //    if (Randos[i].Exists())
            //    {
            //        if (Randos[i] != player && Randos[i] != Suspect2) Suspect2.RelationshipGroup.SetRelationshipWith(Randos[i].RelationshipGroup, Relationship.Hate);
            //    }
            //}
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
            if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }
            if (SuspectBlip2.Exists()) { SuspectBlip2.Delete(); }
            if (SuspectBlip3.Exists()) { SuspectBlip3.Delete(); }
            if (SuspectBlip4.Exists()) { SuspectBlip4.Delete(); }
            if (AreaBlip.Exists()) AreaBlip.Delete();
            Game.LogTrivial("YOBBINCALLOUTS: Active Shooter Callout Finished Cleaning Up.");
        }
        public override void Process()
        {
            base.Process();
        }
    }
}
