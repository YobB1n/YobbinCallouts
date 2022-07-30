using System.Drawing;
using Rage;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage.Native;
using System;

namespace YobbinCallouts.Callouts
{
    [CalloutInfo("Bar Fight", CalloutProbability.High)]
    public class BarFight : Callout
    {
        private Vector3 MainSpawnPoint;
        private Vector3 LegionBar = new Vector3(253.8926f, -1009.604f, 29.27279f);
        private Vector3 RanchoBar = new Vector3(500.8603f, -1536.218f, 29.27567f);
        private Vector3 DowntownBar = new Vector3(224.254f, 314.3193f, 105.5649f);
        private Vector3 EastVinewoodBar = new Vector3(966.7332f, -119.8229f, 74.35316f);
        private Vector3 PaletoBar = new Vector3(-259.9449f, 6290.934f, 31.47674f);
        private Vector3 SandyBar = new Vector3(1991.103f, 3047.539f, 47.21512f);   //inside, test
        private Vector3 WestVinewoodBar = new Vector3(-561.0571f, 273.3992f, 83.10964f);
        private Vector3 StripClub = new Vector3(142.3387f, -1299.464f, 29.17999f);  //add
        private Vector3 DelPerroBar = new Vector3(-1650.104f, -1001.059f, 13.0174f);

        private Ped Suspect;
        private Ped Suspect2;
        private Ped Victim;
        private string[] Peds;

        private Blip SuspectBlip;
        private Blip Suspect2Blip;
        private Blip VictimBlip;
        private Blip AreaBlip;

        private int MainScenario;
        private string Zone;
        private bool CalloutRunning;

        private LHandle SuspectPursuit;

        Ped player = Game.LocalPlayer.Character;

        public override bool OnBeforeCalloutDisplayed()
        {
            Game.LogTrivial("==========YOBBINCALLOUTS: Bar Fight Callout Start==========");
            System.Random r = new System.Random();
            int Scenario = r.Next(0, 2);    //change
            MainScenario = Scenario;
            Game.LogTrivial("YOBBINCALLOUTS: Scenario is " + MainScenario + "");

            if (MainScenario >= 0)
            {
                Zone = Functions.GetZoneAtPosition(Game.LocalPlayer.Character.Position).GameName;
                Game.LogTrivial("YOBBINCALLOUTS: Zone is " + Zone);
                if (Zone == "Davis" || Zone == "Stad" || Zone == "STRAW" || Zone == "ChamH")
                {
                    //MainSpawnPoint = StripClub;
                    MainSpawnPoint = StripClub;
                }
                else if (Zone == "PBOX" || Zone == "LegSqu" || Zone == "SKID" || Zone == "TEXTI")
                {
                    MainSpawnPoint = LegionBar;
                }
                else if (Zone == "Termina" || Zone == "Elysian" || Zone == "Banning" || Zone == "RANCHO" || Zone == "AirP")
                {
                    MainSpawnPoint = RanchoBar;
                }
                else if (Zone == "Mirr" || Zone == "East_V")
                {
                    MainSpawnPoint = EastVinewoodBar;
                }
                else if(Zone == "Cypre" || Zone == "Murri" || Zone == "EBuro" || Zone == "LMesa")
                {
                    MainSpawnPoint = new Vector3(1213.211f, -419.58f, 67.65f); //Mirror
                }
                else if (Zone == "Vesp" || Zone == "VCana" || Zone == "Beach" || Zone == "DeLBe" || Zone == "DelPe" || Zone == "Morn" || Zone == "PBluff" || Zone == "Movie")
                {
                    MainSpawnPoint = DelPerroBar;   //change
                }
                else if (Zone == "DelSol" || Zone == "Koreat")
                {
                    MainSpawnPoint = new Vector3(-878.88f, -1157.57f, 5.257f);   //liquor koreatown
                }
                else if (Zone == "Rockf" || Zone == "Burton" || Zone == "Richm" || Zone == "Golf")
                {
                    MainSpawnPoint = WestVinewoodBar;
                }
                else if (Zone == "CHIL" || Zone == "Vine" || Zone == "DTVine" || Zone == "WVine" || Zone == "Alta" || Zone == "Hawick")
                {
                    MainSpawnPoint = DowntownBar;
                }
                else if (Zone == "Sandy" || Zone == "GrapeS" || Zone == "Desrt")
                {
                    MainSpawnPoint = SandyBar;
                }
                else if (Zone == "ProcoB" || Zone == "PalFor" || Zone == "Paleto" || Zone == "MTChil")
                {
                    MainSpawnPoint = PaletoBar;
                }
                else
                {
                    Game.LogTrivial("YOBBINCALLOUTS: Player is Not Near Any Bar. Aborting Callout.");
                    return false;
                }
            }
            ShowCalloutAreaBlipBeforeAccepting(MainSpawnPoint, 75f);    //Callout Blip Circle with radius 50m
            AddMinimumDistanceCheck(25f, MainSpawnPoint);   //Player must be 25m or further away
            Functions.PlayScannerAudio("CITIZENS_REPORT CRIME_DISTURBING_THE_PEACE_01");
            CalloutMessage = "Bar Fight";
            CalloutPosition = MainSpawnPoint;
            CalloutAdvisory = "A Fight Has Broken Out Between Two ~r~Suspects ~w~at a ~y~Bar.";
            return base.OnBeforeCalloutDisplayed();
        }
        public override bool OnCalloutAccepted()
        {
            Game.LogTrivial("YOBBINCALLOUTS: Bar Fight Callout Accepted by User.");
            if (Main.CalloutInterface)
            {
                CalloutInterfaceHandler.SendCalloutDetails(this, "CODE 3", "");
            }
            else
            {
                Game.DisplayNotification("Respond ~r~Code 3.");
            }

            Peds = new string[8] { "A_M_Y_SouCent_01", "A_M_Y_StWhi_01", "A_M_Y_StBla_01", "A_M_Y_Downtown_01", "A_M_Y_BevHills_01", "G_M_Y_MexGang_01", "G_M_Y_MexGoon_01", "G_M_Y_StrPunk_01" };
            System.Random r2 = new System.Random();
            int SuspectModel = r2.Next(1, Peds.Length); //KEEP THIS ARRAY INDEXER LIKE THIS

            NativeFunction.Natives.GetClosestVehicleNodeWithHeading(MainSpawnPoint, out Vector3 nodePosition, out float heading, 1, 3.0f, 0);
            Suspect = new Ped(Peds[SuspectModel], MainSpawnPoint, heading);
            Suspect.IsPersistent = true;
            Suspect.BlockPermanentEvents = true;

            Vector3 Suspect2SpawnPoint = Suspect.GetOffsetPositionFront(2);
            Suspect2 = new Ped(Peds[SuspectModel - 1], Suspect2SpawnPoint, heading);
            Suspect2.IsPersistent = true;
            Suspect2.BlockPermanentEvents = true;
            Suspect2.Tasks.AchieveHeading(Suspect.Heading - 180);

            AreaBlip = new Blip(Suspect.Position, 25);
            AreaBlip.Color = Color.Yellow;
            AreaBlip.Alpha = 0.67f;
            AreaBlip.IsRouteEnabled = true;
            AreaBlip.Name = "Bar";

            if (CalloutRunning == false) CalloutRunning = true; Callout();
            return base.OnCalloutAccepted();
        }
        public override void OnCalloutNotAccepted()
        {
            Game.LogTrivial("YOBBINCALLOUTS: Bar Fight Callout Not Accepted by User.");
            //Functions.PlayScannerAudio("OTHER_UNIT_TAKING_CALL_01");  //this gets annoying after a while
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
                        while (player.DistanceTo(Suspect) >= 25 && !Game.IsKeyDown(Config.CalloutEndKey)) GameFiber.Wait(0);
                        if (Game.IsKeyDown(Config.CalloutEndKey)) { EndCalloutHandler.CalloutForcedEnd = true; break; }
                        Game.LogTrivial("YOBBINCALLOUTS: Player Arrived on Scene.");
                        AreaBlip.Delete();
                        SuspectBlip = Suspect.AttachBlip();
                        SuspectBlip.IsFriendly = false;
                        SuspectBlip.Scale = 0.75f;
                        SuspectBlip.Name = "Suspect";
                        Suspect.RelationshipGroup = RelationshipGroup.Gang1;
                        Suspect.RelationshipGroup.SetRelationshipWith(RelationshipGroup.Gang2, Relationship.Hate);
                        Suspect2Blip = Suspect2.AttachBlip();
                        Suspect2Blip.IsFriendly = false;
                        Suspect2Blip.Scale = 0.75f;
                        Suspect2Blip.Name = "Suspect";
                        Suspect2.RelationshipGroup = RelationshipGroup.Gang2;
                        Suspect2.RelationshipGroup.SetRelationshipWith(RelationshipGroup.Gang1, Relationship.Hate);
                        Game.DisplayHelp("Break Up the ~r~Fight.");
                        while (player.DistanceTo(Suspect) >= 30 && player.IsInAnyVehicle(false)) GameFiber.Wait(0);

                        if (MainScenario == 0)
                        {
                            if (Main.CalloutInterface) CalloutInterfaceHandler.SendMessage(this, "Reporting Two Males Fighting Outside the Bar");
                            Suspect.Tasks.FightAgainstClosestHatedTarget(10, -1);
                            Suspect2.Tasks.FightAgainstClosestHatedTarget(10, -1);

                            while (Suspect.Exists() || Suspect2.Exists())   //all this is a workaround for StopThePed
                            {
                                GameFiber.Yield();
                                if (!Suspect.Exists() || Suspect.IsDead || Functions.IsPedArrested(Suspect))
                                {
                                    if (!Suspect2.Exists() || Suspect2.IsDead || Functions.IsPedArrested(Suspect2)) break;
                                }
                            }
                            if (Suspect.Exists())
                            {
                                if (Functions.IsPedArrested(Suspect)) { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect is Under ~g~Arrest~w~ Following the Fight."); }
                                else { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect Was ~r~Killed~w~ Following the Fight."); }
                            }
                            else { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect Was ~r~Killed~w~ Following the Fight."); }
                            if (Suspect2.Exists())
                            {
                                if (Functions.IsPedArrested(Suspect2)) { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Second Suspect is also Under ~g~Arrest~w~ Following the Fight."); }
                                else { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Second Suspect Was also ~r~Killed~w~ Following the Fight."); }
                            }
                            else { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Second Suspect Was also ~r~Killed~w~ Following Fight."); }
                            GameFiber.Wait(2000);
                            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("REPORT_RESPONSE_COPY_02");
                            GameFiber.Wait(2000);
                            break;
                        }
                        else
                        {
                            GameFiber.Wait(1000);
                            Game.DisplaySubtitle("~r~Oh Shit, It's the Cops!", 2500);
                            GameFiber.Wait(2500);
                            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("CRIME_SUSPECT_ON_THE_RUN_01");
                            SuspectPursuit = LSPD_First_Response.Mod.API.Functions.CreatePursuit();
                            LSPD_First_Response.Mod.API.Functions.RequestBackup(player.Position, LSPD_First_Response.EBackupResponseType.Code3, LSPD_First_Response.EBackupUnitType.LocalUnit);
                            LSPD_First_Response.Mod.API.Functions.SetPursuitIsActiveForPlayer(SuspectPursuit, true);
                            LSPD_First_Response.Mod.API.Functions.AddPedToPursuit(SuspectPursuit, Suspect);
                            LSPD_First_Response.Mod.API.Functions.AddPedToPursuit(SuspectPursuit, Suspect2);
                            //while (Functions.IsPursuitStillRunning(MainPursuit)) { GameFiber.Wait(0); }
                            //GameFiber.Wait(1500);
                            while (LSPD_First_Response.Mod.API.Functions.IsPursuitStillRunning(SuspectPursuit)) GameFiber.Wait(0);
                            while (Suspect.Exists() || Suspect2.Exists())
                            {
                                GameFiber.Yield();
                                if (!Suspect.Exists() || Suspect.IsDead || Functions.IsPedArrested(Suspect))
                                {
                                    if (!Suspect2.Exists() || Suspect2.IsDead || Functions.IsPedArrested(Suspect2)) break;
                                }
                            }
                            if (Suspect.Exists())
                            {
                                if (Functions.IsPedArrested(Suspect)) { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect is Under ~g~Arrest~w~ Following the Pursuit."); }
                                else { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect Was ~r~Killed~w~ Following the Pursuit."); }
                            }
                            else { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect Was ~r~Killed~w~ Following the Pursuit."); }
                            if (Suspect2.Exists())
                            {
                                if (Functions.IsPedArrested(Suspect2)) { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Second Suspect is also Under ~g~Arrest~w~ Following the Pursuit."); }
                                else { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Second Suspect Was also ~r~Killed~w~ Following the Pursuit."); }
                            }
                            else { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Second Suspect Was also ~r~Killed~w~ Following the Pursuit."); }
                            GameFiber.Wait(2000);
                            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("REPORT_RESPONSE_COPY_02");
                            GameFiber.Wait(2000);
                        }
                        //Game.DisplayHelp("Press End to ~b~Finish~w~ the Callout.");
                        //while (!Game.IsKeyDown(System.Windows.Forms.Keys.End)) GameFiber.Wait(0);
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
                Functions.PlayScannerAudio("ATTENTION_ALL_UNITS WE_ARE_CODE_4");
            }
            CalloutRunning = false;
            if (Victim.Exists()) { Victim.Tasks.ClearImmediately(); }
            if (Victim.Exists()) { Victim.Dismiss(); }
            //if (Suspect.Exists()) { Suspect.Dismiss(); }
            if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }
            //if (Suspect2.Exists()) { Suspect2.Dismiss(); }
            if (Suspect2Blip.Exists()) { Suspect2Blip.Delete(); }
            if (VictimBlip.Exists()) { VictimBlip.Delete(); }
            if (AreaBlip.Exists()) { AreaBlip.Delete(); }
            Game.LogTrivial("YOBBINCALLOUTS: Bar Fight Callout Finished Cleaning Up.");
        }
        public override void Process()
        {
            base.Process();
        }
    }
}