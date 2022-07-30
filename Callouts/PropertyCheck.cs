using System.Collections.Generic;
using System.Windows.Forms;
using Rage;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage.Native;
using System.Linq;
using System;

namespace YobbinCallouts.Callouts
{
    [CalloutInfo("Property Checkup", CalloutProbability.High)]

    public class PropertyCheck : Callout
    {
        private Vector3 MainSpawnPoint;

        private Blip House;
        private Blip SuspectBlip;

        Ped Animal;
        private Ped Suspect;

        private int MainScenario;
        bool Backup = false;

        private string Zone;
        private string AnimalType;

        Player player = Game.LocalPlayer;

        private LHandle MainPursuit;

        private bool CalloutRunning = false;

        //All the dialogue for the callout. Haven't found a better way to store it yet, so this will have to do.
        private readonly List<string> SuspectCooperative1 = new List<string>()
        {
         "~r~Suspect:~w~ Okay officer, just please don't hurt me!",
         "~g~You:~w~ Gust stay where you are, keep your hands there. What are you doing here?",
         "~r~Suspect:~w~ Uh, nothing, why?",
         "~g~You:~w~ Is this your house? Why are you walking around with a crowbar?",
         "~r~Suspect:~w~ I don't have to answer any questions. Just please don't kill me!",
         "~g~You:~w~ You are under arrest, sir. Please stay where you are!",
        };
        private readonly List<string> SuspectCooperative2 = new List<string>()
        {
         "~r~Suspect:~w~ Alright officer, just don't hurt me!",
         "~g~You:~w~ Just stay where you are, keep your hands there. What are you doing here?",
         "~r~Suspect:~w~ That's none of your business, just don't hurt me please!",
         "~g~You:~w~ Is this your house? Why are you walking around here with a crowbar?",
         "~r~Suspect:~w~ I don't need to answer any questions.",
         "~g~You:~w~ Alright, you are under arrest, sir. Please stay where you are!",
        };
        private int SuspectOpeningCount;
        public override bool OnBeforeCalloutDisplayed()
        {
            Game.LogTrivial("==========YOBBINCALLOUTS: Property Checkup Callout Start==========");
            System.Random r = new System.Random();
            int Scenario = r.Next(0, 8);    //change this
            MainScenario = Scenario;
            Game.LogTrivial("YOBBINCALLOUTS: Scenario is Value is " + MainScenario);

            //HOUSE CHOOSER FOR 1ST SCENARIO
            Zone = Functions.GetZoneAtPosition(Game.LocalPlayer.Character.Position).GameName;
            Game.LogTrivial("YOBBINCALLOUTS: Zone is " + Zone);
            MainSpawnPoint = CallHandler.nearestLocationChooser(CallHandler.getHouseList, maxdistance: 600, mindistance: 100);
            if (!CallHandler.isHouse)
            {
                Game.LogTrivial("YOBBINCALLOUTS: Player is not near any house. Aborting Callout.");
                return false;
            }

            ShowCalloutAreaBlipBeforeAccepting(MainSpawnPoint, 75f);    //Callout Blip Circle with radius 50m
            AddMinimumDistanceCheck(50f, MainSpawnPoint);   //Player must be 50m or further away
            Functions.PlayScannerAudio("CITIZENS_REPORT YC_POSSIBLE_TRESPASSING");
            CalloutMessage = "Property Checkup";
            CalloutPosition = MainSpawnPoint;
            CalloutAdvisory = "A Checkup on a Resident's ~b~Property~w~ is Requested.";
            return base.OnBeforeCalloutDisplayed();
        }
        public override bool OnCalloutAccepted()
        {
            Game.LogTrivial("YOBBINCALLOUTS: Property Checkup Callout Accepted by User.");
            if (Main.CalloutInterface)
            {
                CalloutInterfaceHandler.SendCalloutDetails(this, "CODE 2", "");
            }
            else
            {
                Game.DisplayNotification("Respond ~b~Code 2.");
            }
            House = new Blip(MainSpawnPoint, 25);
            House.IsRouteEnabled = true;
            House.Color = System.Drawing.Color.Yellow;
            House.Alpha = 0.67f;
            House.Name = "Property";

            if (MainScenario == 0 || MainScenario == 1)   //Nothing, Away on Vacation
            {
                System.Random rondom = new System.Random();
                int Message = rondom.Next(0, 2);
                if (Message == 0)
                {
                    if (Main.CalloutInterface) CalloutInterfaceHandler.SendMessage(this, "Resident away on vacation requested a checkup on their property.");
                    else Game.DisplayNotification("~b~Resident ~r~Away~w~ on Vacation and Requested a Checkup on their ~y~Property.");
                }
                else
                {
                    if (Main.CalloutInterface) CalloutInterfaceHandler.SendMessage(this, "Resident reported suspicious movement in their Neighbour's property.");
                    else Game.DisplayNotification("~b~Resident ~w~Reported ~r~Suspicious Movement~w~ in their ~y~Neighbour's Property.");
                }
            }
            else if (MainScenario == 2 || MainScenario == 3)     //Animal
            {
                if (Main.CalloutInterface) CalloutInterfaceHandler.SendMessage(this, "Resident reported suspicious movement in their Neighbour's property.");
                else Game.DisplayNotification("~b~Resident ~w~Reported ~r~Suspicious Movement~w~ in their ~y~Neighbour's Property.");
                System.Random rondom = new System.Random();
                int AnimalChooser = rondom.Next(0, 4);
                if (AnimalChooser == 0) { Animal = new Ped("a_c_coyote", MainSpawnPoint, 0f); AnimalType = "Wild Coyote"; }
                if (AnimalChooser == 1) { Animal = new Ped("a_c_rottweiler", MainSpawnPoint, 0f); AnimalType = "Rotweiller"; }
                if (AnimalChooser == 2) { Animal = new Ped("a_c_boar", MainSpawnPoint, 0f); AnimalType = "Wild Boar"; }
                if (AnimalChooser == 3) { Animal = new Ped("a_c_retriever", MainSpawnPoint, 0f); AnimalType = "Dog"; }
                if (!Animal.Exists()) { Game.LogTrivial("YOBBINCALLOUTS: Animal does not Exist. Ending Callout."); return false; }
                Animal.IsPersistent = true;
                Animal.BlockPermanentEvents = true;
            }
            else  //person
            {
                if (Main.CalloutInterface) CalloutInterfaceHandler.SendMessage(this, "Resident reported suspicious movement in their Neighbour's property.");
                else Game.DisplayNotification("~b~Resident ~w~Reported ~r~Suspicious Movement~w~ in their ~y~Neighbour's Property.");
                string[] Suspects = new string[8] { "A_M_Y_SouCent_01", "A_M_Y_StWhi_01", "A_M_Y_StBla_01", "A_M_Y_Downtown_01", "A_M_Y_BevHills_01", "G_M_Y_MexGang_01", "G_M_Y_MexGoon_01", "G_M_Y_StrPunk_01" };
                System.Random r2 = new System.Random();
                int SuspectModel = r2.Next(0, Suspects.Length);
                Suspect = new Ped(Suspects[SuspectModel], MainSpawnPoint, 69);
                Game.LogTrivial("YOBBINCALLOUTS: Suspect Spawned.");
                Suspect.IsPersistent = true;
                Suspect.BlockPermanentEvents = true;
                Suspect.Inventory.GiveNewWeapon("WEAPON_CROWBAR", -1, true);
            }

            if (Config.DisplayHelp == true) { Game.DisplayHelp("Go to the ~y~Property~w~ Shown on The Map to Investigate."); }
            if (CalloutRunning == false) Callout();
            return base.OnCalloutAccepted();
        }
        public override void OnCalloutNotAccepted()
        {
            Game.LogTrivial("YOBBINCALLOUTS: Property Checkup Callout Not Accepted by User.");
            base.OnCalloutNotAccepted();
        }
        public override void Process()
        {
            base.Process();
        }
        private void Callout()
        {
            CalloutRunning = true;
            GameFiber.StartNew(delegate
            {
                try
                {
                    while (CalloutRunning == true)
                    {
                        while (player.Character.DistanceTo(MainSpawnPoint) >= 35 && !Game.IsKeyDown(Config.CalloutEndKey)) { GameFiber.Wait(0); }
                        if (Game.IsKeyDown(Config.CalloutEndKey)) { break; }
                        Game.DisplayHelp("Search the ~y~Property~w~ for ~r~Suspicious Activity.");
                        if (MainScenario <= 1)   //nothing
                        {
                            GameFiber.Wait(20000);
                            Game.DisplayHelp("If You ~g~Do Not See Anything~w~, Press End to ~b~Finish the Callout.");
                            while (!Game.IsKeyDown(Config.CalloutEndKey)) { GameFiber.Wait(0); }
                            Game.DisplayNotification("Dispatch, we are Code 4. We Have ~b~Secured~w~ the Property.");

                            break;
                        }
                        else if (MainScenario == 2 || MainScenario == 3)  //animal nothing
                        {
                            while (player.Character.DistanceTo(Animal) >= 6) { GameFiber.Wait(0); }
                            Game.DisplaySubtitle("Oh, Looks Like it Might Have Just Been an Animal.", 3000);
                            Animal.Dismiss();
                            GameFiber.Wait(5000);
                            Game.DisplayHelp("When You are Done ~y~Investigating, ~w~Press End to ~b~Finish the Callout.");
                            while (!Game.IsKeyDown(Config.CalloutEndKey)) { GameFiber.Wait(0); }
                            Game.DisplayNotification("Dispatch, The Property is ~g~Clear.~w~ Turned out to be a ~b~" + AnimalType + " ~w~on the Property.");
                            GameFiber.Wait(2500);
                            Functions.PlayScannerAudio("REPORT_RESPONSE_COPY_04");
                            GameFiber.Wait(2500);
                            Game.DisplayNotification("Dispatch, we are Code 4. We Have ~b~Secured~w~ the Property.");

                            break;
                        }
                        else if (MainScenario == 4)  //Suspect runs away
                        {
                            while (player.Character.DistanceTo(Suspect) >= 6) { GameFiber.Wait(0); }
                            SuspectBlip = Suspect.AttachBlip();
                            SuspectBlip.IsFriendly = false;
                            SuspectBlip.Scale = 0.75f;
                            SuspectBlip.Name = "Suspect";
                            House.Delete();
                            Game.DisplaySubtitle("Hey Sir! Stay Where You Are!", 2000);
                            GameFiber.Wait(1500);
                            Suspect.Tasks.AchieveHeading(player.Character.Heading - 180).WaitForCompletion(500);
                            if (Suspect.IsAlive)
                            {
                                MainPursuit = Functions.CreatePursuit();
                                Functions.SetPursuitIsActiveForPlayer(MainPursuit, true);
                                Functions.AddPedToPursuit(MainPursuit, Suspect);
                                Functions.PlayScannerAudio("CRIME_SUSPECT_ON_THE_RUN_01");
                                try
                                {
                                    Functions.RequestBackup(Suspect.Position, LSPD_First_Response.EBackupResponseType.Code3, LSPD_First_Response.EBackupUnitType.LocalUnit);
                                    Game.LogTrivial("YOBBINCALLOUTS: Backup Dispatched");
                                }
                                catch (Exception e)
                                {
                                    Game.LogTrivial("YOBBINCALLOUTS: Error spawning Code 3 Backup. Most Likely User Error. ERROR - " + e);
                                }
                                while (LSPD_First_Response.Mod.API.Functions.IsPursuitStillRunning(MainPursuit)) { GameFiber.Wait(0); }
                                if (Suspect.Exists())
                                {
                                    if (Functions.IsPedArrested(Suspect)) { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect is Under ~g~Arrest~w~ Following the Pursuit."); }
                                    else { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect Was ~r~Killed~w~ Following the Pursuit."); }
                                }
                                else { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect Was ~r~Killed~w~ Following the Pursuit."); }
                                GameFiber.Wait(2000);
                                Functions.PlayScannerAudio("REPORT_RESPONSE_COPY_02");
                                GameFiber.Wait(2000);
                            }
                            Game.DisplayHelp("When You are Done ~y~Investigating, ~w~Press End to ~b~Finish the Callout.");
                            while (!Game.IsKeyDown(Config.CalloutEndKey)) { GameFiber.Wait(0); }
                            Game.DisplayNotification("Dispatch, we are Code 4. We Have ~b~Secured~w~ the Property.");

                            break;
                        }
                        else if (MainScenario == 5)  //suspect attacks
                        {
                            while (player.Character.DistanceTo(Suspect) >= 7) { GameFiber.Wait(0); }
                            SuspectBlip = Suspect.AttachBlip();
                            SuspectBlip.IsFriendly = false;
                            SuspectBlip.Scale = 0.75f;
                            SuspectBlip.Name = "Suspect";
                            House.Delete();
                            Game.DisplaySubtitle("~g~You:~w~ Hey Sir! Drop Your Weapon and Put Your Hands Up!", 2500);
                            GameFiber.Wait(1500);
                            Suspect.Tasks.AimWeaponAt(Game.LocalPlayer.Character, 1000);
                            Game.LogTrivial("YOBBINCALLOUTS: Suspect Threatened Officer With Weapon.");
                            Functions.PlayScannerAudio("CRIME_ASSAULT_PEACE_OFFICER_03");
                            try
                            {
                                Functions.RequestBackup(Suspect.Position, LSPD_First_Response.EBackupResponseType.Code3, LSPD_First_Response.EBackupUnitType.LocalUnit);
                                Game.LogTrivial("YOBBINCALLOUTS: Backup Dispatched");
                            }
                            catch (Exception e)
                            {
                                Game.LogTrivial("YOBBINCALLOUTS: Error spawning Code 3 Backup. Most Likely User Error. ERROR - " + e);
                            }

                            Suspect.Tasks.FightAgainst(Game.LocalPlayer.Character);
                            while (Suspect.Exists() && !Functions.IsPedArrested(Suspect) && Suspect.IsAlive)
                            {
                                GameFiber.Yield();
                            }
                            if (Suspect.Exists())
                            {
                                if (Functions.IsPedArrested(Suspect) || Suspect.IsAlive) { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect is Under ~g~Arrest~w~ Attempting to ~r~Assault an Officer."); }
                                else { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect Was ~r~Killed~w~ for ~r~Assaulting an Officer."); }
                            }
                            else { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect Was ~r~Killed~w~ Attempting to ~r~Assault an Officer."); }
                            GameFiber.Wait(2000);
                            Functions.PlayScannerAudio("REPORT_RESPONSE_COPY_02");
                            GameFiber.Wait(4500);
                            Game.DisplayHelp("When You are Done ~y~Investigating, ~w~Press End to ~b~Finish the Callout.");
                            while (!Game.IsKeyDown(Config.CalloutEndKey)) { GameFiber.Wait(0); }
                            Game.DisplayNotification("Dispatch, we are Code 4. We Have ~b~Secured~w~ the Property.");

                            break;
                        }
                        else if (MainScenario >= 6)  //suspect cooperative
                        {
                            Suspect.Tasks.PlayAnimation("missheist_agency3aig_13", "wait_loops_player0", -1, AnimationFlags.Loop);
                            while (player.Character.DistanceTo(Suspect) >= 7) { GameFiber.Wait(0); }
                            SuspectBlip = Suspect.AttachBlip();
                            SuspectBlip.IsFriendly = false;
                            SuspectBlip.Scale = 0.75f;
                            SuspectBlip.Name = "Suspect";
                            House.Delete();
                            Game.DisplaySubtitle("~g~You:~w~ Hey Sir! Drop Your Weapon and Put Your Hands Up!", 2000);
                            GameFiber.Wait(1500);
                            if (Suspect.IsAlive)
                            {
                                Suspect.Tasks.PutHandsUp(-1, Game.LocalPlayer.Character);
                                GameFiber.Wait(500);
                                if (Config.DisplayHelp == true)
                                {
                                    Game.DisplayHelp("Press " + Config.MainInteractionKey + " to Advance the Conversation.");
                                }
                                System.Random r = new System.Random();
                                int OpeningDialogue = r.Next(0, 2);
                                switch (OpeningDialogue)
                                {
                                    case 0:
                                        CallHandler.Dialogue(SuspectCooperative1);
                                        break;
                                    case 1:
                                        CallHandler.Dialogue(SuspectCooperative2);
                                        break;
                                }
                                GameFiber.Wait(1000);
                                Game.DisplayHelp("Arrest the ~r~Suspect.");

                                if (MainScenario == 6) //cooperative fr
                                {
                                    Suspect.Tasks.ClearImmediately();
                                    while (Suspect.Exists() && !Functions.IsPedArrested(Suspect) && Suspect.IsAlive)
                                    {
                                        GameFiber.Yield();
                                    }
                                    if (Suspect.Exists())
                                    {
                                        if (Functions.IsPedArrested(Suspect) || Suspect.IsAlive) { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect is Under ~g~Arrest."); }
                                        else { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect Was ~r~Killed~w~."); }
                                    }
                                    else { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect Was ~r~Killed~w~."); }
                                    GameFiber.Wait(2000);
                                    Functions.PlayScannerAudio("REPORT_RESPONSE_COPY_02");
                                    GameFiber.Wait(6000);
                                    Game.DisplayHelp("When You are Done ~y~Investigating, ~w~Press End to ~b~Finish the Callout.");
                                    while (!Game.IsKeyDown(Config.CalloutEndKey)) { GameFiber.Wait(0); }
                                }
                                else //coperative but sike not
                                {
                                    while (player.Character.DistanceTo(Suspect) >= 3) { GameFiber.Wait(0); }
                                    if (Suspect.IsDead && !Suspect.IsCuffed)
                                    {
                                        GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, Suspect is ~r~Dead.");
                                        GameFiber.Wait(2000);
                                        Functions.PlayScannerAudio("REPORT_RESPONSE_COPY_02");
                                        GameFiber.Wait(4000);
                                        Game.DisplayHelp("When You are Done ~y~Investigating, ~w~Press End to ~b~Finish the Callout.");
                                        while (!Game.IsKeyDown(Config.CalloutEndKey)) { GameFiber.Wait(0); }
                                    }
                                    else if (Functions.IsPedArrested(Suspect))
                                    {
                                        if (Suspect.Exists())
                                        {
                                            if (Functions.IsPedArrested(Suspect) || Suspect.IsAlive) { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect is Under ~g~Arrest."); }
                                            else { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect Was ~r~Killed~w~."); }
                                        }
                                        else { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect Was ~r~Killed~w~."); }
                                    }
                                    else
                                    {
                                        Suspect.Tasks.FightAgainst(Game.LocalPlayer.Character, -1);
                                        try
                                        {
                                            Functions.RequestBackup(Suspect.Position, LSPD_First_Response.EBackupResponseType.Code3, LSPD_First_Response.EBackupUnitType.LocalUnit);
                                            Game.LogTrivial("YOBBINCALLOUTS: Backup Dispatched");
                                        }
                                        catch (Exception e)
                                        {
                                            Game.LogTrivial("YOBBINCALLOUTS: Error spawning Code 3 Backup. Most Likely User Error. ERROR - " + e);
                                        }
                                        while (Suspect.Exists() && !Functions.IsPedArrested(Suspect) && Suspect.IsAlive)
                                        {
                                            GameFiber.Yield();
                                        }
                                        if (Suspect.Exists())
                                        {
                                            if (Functions.IsPedArrested(Suspect) || Suspect.IsAlive) { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect is Under ~g~Arrest~w~ Attempting to ~r~Assault an Officer."); }
                                            else { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect Was ~r~Killed~w~ for ~r~Assaulting an Officer."); }
                                        }
                                        else { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect Was ~r~Killed~w~ Attempting to ~r~Assault an Officer."); }
                                        GameFiber.Wait(2000);
                                        Functions.PlayScannerAudio("REPORT_RESPONSE_COPY_02");
                                        GameFiber.Wait(4000);
                                        Game.DisplayHelp("When You are Done ~y~Investigating, ~w~Press End to ~b~Finish the Callout.");
                                        while (!Game.IsKeyDown(Config.CalloutEndKey)) { GameFiber.Wait(0); }
                                        Game.DisplayNotification("Dispatch, we are Code 4. We Have ~b~Secured~w~ the Property.");
                                    }
                                }
                            }
                            else
                            {
                                GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, Suspect is ~r~Dead.");
                                GameFiber.Wait(2000);
                                Functions.PlayScannerAudio("REPORT_RESPONSE_COPY_02");
                                GameFiber.Wait(4000);
                                Game.DisplayHelp("When You are Done ~y~Investigating, ~w~Press End to ~b~Finish the Callout.");
                                while (!Game.IsKeyDown(Config.CalloutEndKey)) { GameFiber.Wait(0); }
                            }
                            Game.DisplayNotification("Dispatch, we are Code 4. We Have ~b~Secured~w~ the Property.");
                            break;
                        }
                    }
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
                        End();
                    }
                    else
                    {
                        Game.LogTrivial("==========YOBBINCALLOUTS: ERROR CAUGHT - CALLOUT NO LONGER RUNNING==========");
                        string error = e.ToString();
                        Game.LogTrivial("ERROR: " + error);
                        Game.LogTrivial("No Need to Report This Error if it Did not Result in an LSPDFR Crash.");
                        Game.LogTrivial("==========YOBBINCALLOUTS: ERROR CAUGHT - CALLOUT NO LONGER RUNNING==========");
                    }
                }
                EndCalloutHandler.EndCallout();
                End();
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
            //if (Animal.Exists() && Animal.IsAlive) { Animal.Dismiss(); }
            if (Suspect.Exists()) { Suspect.Dismiss(); }
            if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }
            if (House.Exists()) { House.Delete(); }
            Game.LogTrivial("YOBBINCALLOUTS: Property Checkup Callout Finished Cleaning Up.");
        }
    }
}