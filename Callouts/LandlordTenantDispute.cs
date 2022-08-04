using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace YobbinCallouts.Callouts
{
    [CalloutInfo("Landlord-Tenant Dispute", CalloutProbability.High)]
    class LandlordTenantDispute : Callout
    {
        private Vector3 MainSpawnPoint;

        private Ped Suspect;
        private Ped Landlord;
        private Blip LandlordBlip;
        private Blip SuspectBlip;
        private Blip HouseBlip;
        private Ped player = Game.LocalPlayer.Character;
        private LHandle MainPursuit;

        private int MainScenario;
        private bool CalloutRunning;

        private readonly List<string> LandlordOpening1 = new List<string>()
        {
         "~b~Landlord:~w~ Hey Officer, Appreciate you coming so quick.",
         "~g~You:~w~ No worries, what's going on here?",
         "~b~Landlord:~w~ So I've got a tenant living here who I advised 30 days ago that they'd have to move out.",
         "~b~Landlord:~w~ They were very upset by this, but I have a legitimate reason and have given them enough notice to get out.",
         "~b~Landlord:~w~ According to San Andreas State Law, They have 30 days notice to leave, which they have recieved.",
         "~g~You:~w~ Okay. Have you been able to enter the house?",
         "~b~Landlord:~w~ No, Officer. I have a key, but they deadbolted the door from inside.",
         "~b~Landlord:~w~ I'm done wasting time with them! Please, get them off my property, Officer!",
        };
        private readonly List<string> AcceptSpeech = new List<string>()
        {
         "~g~You:~w~ Alright, I'll see what I can do for you.",
         "~g~You:~w~ Sure, let's see what I can do here.",
         "~g~You:~w~ I understand, let me see if I can reason with them.",
        };
        private readonly List<string> AttackSpeech = new List<string>()
        {
         "~b~Landlord:~w~ You piece of shit! Squatting on MY property for months on end!",
         "~b~Landlord:~w~ You useless freeloader! You think you can get away with wasting MONTHS of my rent money?",
         "~b~Landlord:~w~ You thought you could get away with living on MY property RENT-FREE for this long?!",
         "~b~Landlord:~w~ Weren't you scared of what I would do to you after squatting in MY HOUSE for MONTHS?!",
        };
        private readonly List<string> SuspectCoop1 = new List<string>()
        {
         "~g~You:~w~ Hey, can you please open the door and come out here? It's the Police!",
         "~r~Tenant:~w~ No Officer, I will not be unlawfully evicted from my own property!!",
         "~g~You:~w~ Look, let's at least talk this through! Let's not make this any harder than it has to be for anybody here!",
         "~r~Tenant:~w~ Okay fine, I'll come out! But you better not arrest me or unlawfully search my property, Officer! I will not be mistreated",
         "~g~You:~w~ Of Course, just come out here and talk!",
        };

        public override bool OnBeforeCalloutDisplayed()
        {
            Game.LogTrivial("==========YOBBINCALLOUTS: Landlord-Tenant Dispute Callout Start==========");
            System.Random r = new System.Random();
            int Scenario = r.Next(0, 0);
            MainScenario = Scenario;
            Game.LogTrivial("YOBBINCALLOUTS: Scenario value is: " + MainScenario);

            CallHandler.locationChooser(CallHandler.HouseList, 696); //nice
            if (CallHandler.locationReturned) MainSpawnPoint = CallHandler.SpawnPoint;
            else { Game.LogTrivial("YOBBINCALLOUTS: No house found. Aborting..."); return false; }

            ShowCalloutAreaBlipBeforeAccepting(MainSpawnPoint, 25f);
            AddMinimumDistanceCheck(60f, MainSpawnPoint);

            Functions.PlayScannerAudio("ATTENTION_ALL_UNITS_01 WE_HAVE_01 CITIZENS_REPORT_01 CRIME_DISTURBING_THE_PEACE_01");
            CalloutMessage = "Landlord-Tenant Dispute";
            CalloutPosition = MainSpawnPoint;
            if (MainScenario == 0) CalloutAdvisory = "A landlord reports a tenant refusing to leave their property.";
            return base.OnBeforeCalloutDisplayed();
        }
        public override bool OnCalloutAccepted()
        {
            try
            {
                Game.LogTrivial("YOBBINCALLOUTS: Landlord-Tenant Dispute Callout Accepted by User");
                if (Main.CalloutInterface)
                {
                    CalloutInterfaceHandler.SendCalloutDetails(this, "CODE 02", "");
                }
                else
                {
                    Game.DisplayNotification("Respond ~b~Code 2.");
                }

                //instantiation logic
                if (MainScenario == 0)
                {
                    Landlord = new Ped(MainSpawnPoint.Around(2)); //was Around2D
                    Landlord.IsPersistent = true;
                    Landlord.BlockPermanentEvents = true;
                    LandlordBlip = CallHandler.AssignBlip(Landlord, Color.Blue, .69f, "Caller", true);
                    NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(Landlord, player, -1);

                    Game.DisplayHelp("Speak with the ~b~Landlord.");
                }
            }
            catch (Exception e)
            {
                Game.LogTrivial("==========YOBBINCALLOUTS: ERROR CAUGHT ON CALLOUT INTIALIZATION==========");
                Game.LogTrivial("IN: " + this);
                string error = e.ToString();
                Game.LogTrivial("ERROR: " + error);
                Game.DisplayNotification("There was an ~r~Error~w~ Caught with ~b~YobbinCallouts. ~w~Please Chck Your ~g~Log File.~w~ Sorry for the Inconvenience!");
                Game.DisplayNotification("Error: ~r~" + error);
                Game.LogTrivial("If You Believe this is a Bug, Please Report it on my Discord Server. Thanks!");
                Game.LogTrivial("==========YOBBINCALLOUTS: ERROR CAUGHT ON CALLOUT INTIALIZATION==========");
            }
            if (!CalloutRunning) { Callout(); }
            return base.OnCalloutAccepted();
        }
        public override void OnCalloutNotAccepted()
        {
            Game.LogTrivial("YOBBINCALLOUTS: Landlord-Tenant Dispute Callout Not Accepted by User.");
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
                        while (player.DistanceTo(Landlord) >= 25 && !Game.IsKeyDown(Config.CalloutEndKey)) GameFiber.Wait(0);
                        if (Game.IsKeyDown(Config.CalloutEndKey)) break;
                        CallHandler.IdleAction(Landlord, false);
                        while (player.DistanceTo(Landlord) >= 5) GameFiber.Wait(0);
                        Landlord.Tasks.AchieveHeading(player.Heading - 180f).WaitForCompletion(500);
                        if (Config.DisplayHelp) Game.DisplayHelp("Press ~y~" + Config.MainInteractionKey + "~w~ to speak with the ~b~Landlord.");

                        System.Random dialogue = new System.Random();
                        int OpeningDialogue = dialogue.Next(0, 0); //change later
                        if (OpeningDialogue == 0) CallHandler.Dialogue(LandlordOpening1, Landlord);

                        Game.DisplayHelp("~y~" + Config.Key1 + ":~b~ Okay, I'll talk to the Resident. ~y~" + Config.Key2 + ": ~b~I can't do that for you I'm afraid.");
                        while (!Game.IsKeyDown(Config.Key1) && !Game.IsKeyDown(Config.Key2)) GameFiber.Wait(0); //might do an animation?
                        if (Game.IsKeyDown(Config.Key1)) //accept
                        {
                            System.Random dialogue1 = new System.Random();
                            Game.DisplaySubtitle(AcceptSpeech[dialogue1.Next(0, AcceptSpeech.Count)], 2500);
                            GameFiber.Wait(2500);
                            
                            HouseBlip = new Blip(MainSpawnPoint);
                            HouseBlip.Color = Color.Teal;
                            HouseBlip.Scale = 3;
                            HouseBlip.Alpha = 0.69f;

                            if (Landlord.DistanceTo(MainSpawnPoint) > 8f) Landlord.Tasks.FollowNavigationMeshToPosition(MainSpawnPoint, player.Heading, 2.69f, 2f); //test this
                            while (player.DistanceTo(MainSpawnPoint) >= 3f) GameFiber.Wait(0);
                            Game.DisplayHelp("Press ~y~" + Config.MainInteractionKey + "~w~ to ~b~Ring~w~ the Doorbell.");
                            CallHandler.Doorbell();
                            if (HouseBlip.Exists()) HouseBlip.Delete();
                            GameFiber.Wait(2500);

                            System.Random dialogue2 = new System.Random();
                            int ReasonDialogue = dialogue2.Next(0, 0); //change later
                            if (Config.DisplayHelp) Game.DisplayHelp("Press ~y~" + Config.MainInteractionKey + "~w~ to reason with the ~r~Tenant.");
                            if (MainScenario == 0) //cooperates
                            {
                                if (ReasonDialogue == 0) CallHandler.Dialogue(SuspectCoop1);
                                GameFiber.Wait(2000);
                                Suspect = new Ped(MainSpawnPoint, player.Heading - 180);
                                SuspectBlip = CallHandler.AssignBlip(Suspect, Color.Red, .69f, "Tenant");

                                System.Random r2 = new System.Random();
                                int action = r2.Next(0, 3);
                                if (action == 0) //more dialogue
                                {
                                    GameFiber.Wait(1500);
                                }
                                else //attack
                                {
                                    System.Random r3 = new System.Random();
                                    GameFiber.Wait(r3.Next(500, 3000));
                                    System.Random dialogue3 = new System.Random();
                                    Game.DisplaySubtitle(AttackSpeech[dialogue3.Next(0, AttackSpeech.Count)], 2000);
                                    GameFiber.Wait(r3.Next(500, 2000));
                                    //Suspect.BlockPermanentEvents = false;
                                    Landlord.Tasks.FightAgainst(Suspect, -1);
                                    if (action == 1) Suspect.BlockPermanentEvents = false; //or flee
                                    else Suspect.Tasks.FightAgainst(Landlord, -1); //Tenant fights back
                                    LandlordBlip.Color = Color.Orange;

                                    if(action == 1) CallHandler.SuspectWait(Landlord); 
                                    else
                                    {
                                        while (Suspect.Exists() || Landlord.Exists())   //all this is a workaround for StopThePed
                                        {
                                            GameFiber.Yield();
                                            if (!Suspect.Exists() || Suspect.IsDead || Functions.IsPedArrested(Suspect))
                                            {
                                                if (!Landlord.Exists() || Landlord.IsDead || Functions.IsPedArrested(Landlord)) break;
                                            }
                                        }
                                    }
                                    GameFiber.Wait(1500);
                                    Game.DisplayHelp("Deal with the ~o~Landlord~w~ and ~r~Suspect~w~ as you see fit. Press ~b~"+Config.CalloutEndKey+" ~w~when done.");
                                    while (!Game.IsKeyDown(Config.CalloutEndKey)) GameFiber.Wait(0);
                                    break;
                                }
                            }
                            else //does not cooperate
                            {

                            }
                        }
                        else //decline
                        {

                        }

                        GameFiber.Wait(2000);
                        Game.LogTrivial("YOBBINCALLOUTS: Callout Finished, Ending...");
                        EndCalloutHandler.EndCallout();
                        End();
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
            if (LandlordBlip.Exists()) LandlordBlip.Delete();
            if (HouseBlip.Exists()) HouseBlip.Delete();
            //if (Landlord.Exists()) Landlord.Dismiss();
            Game.LogTrivial("YOBBINCALLOUTS: Landlord-Tenant Dispute Callout Finished Cleaning Up.");
        }
        public override void Process()
        {
            base.Process();
        }
    }
}
