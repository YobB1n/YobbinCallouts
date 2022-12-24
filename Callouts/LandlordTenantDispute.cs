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
         "~b~Landlord:~w~ No, Officer. I have a key, but they dead-bolted the door from inside.",
         "~b~Landlord:~w~ I'm done wasting time with them! Please, get them off my property, Officer!",
        };
        private readonly List<string> AcceptSpeech = new List<string>()
        {
         "~g~You:~w~ Alright, I'll see what I can do for you.",
         "~g~You:~w~ Sure, let's see what I can do here.",
         "~g~You:~w~ I understand, let me see if I can reason with them.",
        };
        private readonly List<string> Deny1 = new List<string>()
        {
         "~g~You:~w~ It doesn't seem like there's much I can do for you here, I'm afraid.",
         "~b~Landlord:~w~ What?! Why not? They're no longer welcome on my property. They're trespassing now!",
         "~g~You:~w~ You're going to have to take this up with the housing board of San Andreas. If they side with you and the tenant still doesn't leave, then you can call us.",
         "~b~Landlord:~w~ Unbelievable. I knew it was a waste of time calling you guys.",
        };
        private readonly List<string> Deny2 = new List<string>()
        {
         "~g~You:~w~ It doesn't seem like I have the authority to interfere in this situation, I'm afraid.",
         "~b~Landlord:~w~ What makes you say that? They're trespassing on my property now, Officer! I don't want them living on my property anymore.",
         "~g~You:~w~ You're going to have to take this up with the housing board of San Andreas. I don't have the authority to do anything more.",
         "~b~Landlord:~w~ Unbelievable. It's always a waste of time calling you guys.",
        };
        private readonly List<string> Deny3 = new List<string>()
        {
         "~g~You:~w~ I'm afraid I don't have the authority to help you in this kind of a situation.",
         "~b~Landlord:~w~ How so? They're trespassing on my property now, Officer! I don't want them living on my property anymore.",
         "~g~You:~w~ You're going to have to call up the housing board of San Andreas. I don't have the authority to do anything more.",
         "~b~Landlord:~w~ *Sigh* Okay Officer. Do you have their number?",
         "~g~You:~w~ Yeah, you can reach them at 323-555-6969.",
         "~b~Landlord:~w~ Alright, hopefully this turns out okay. Thanks for your help.",
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
         "~r~Tenant:~w~ Okay fine, I'll come out! But you better not arrest me or unlawfully search my property, Officer! I will not be mistreated!",
         "~g~You:~w~ Of Course, just come out here and talk!",
        };
        private readonly List<string> SuspectLeaves1 = new List<string>()
        {
         "~g~You:~w~ Thank you. Are you aware that the 30 day period for moving out of the property has expired?",
         "~r~Tenant:~w~ Yes I am Officer, I wanted to be out sooner, I haven't been able to move all my stuff out yet.",
         "~r~Tenant:~w~ I should be able to get out tomorrow if that's okay with you, most of it is gone already.",
         "~g~You:~w~ Okay, because it seems like your landlord is pretty upset about the whole thing.",
         "~r~Tenant:~w~ Yes I understand and I'm really sorry, if you could just give me one more day and I'll be out!",
         "~g~You:~w~ Okay, let me see what I can do.",
        };
        private readonly List<string> SuspectLeaves2 = new List<string>()
        {
         "~g~You:~w~ Thanks for your cooperation. Do you know why I'm here?",
         "~r~Tenant:~w~ Yes Officer, I know the 30 day period to leave is up today.",
         "~r~Tenant:~w~ It's been really hard for me to find a new place, I just found one a week ago but I need a couple more days to pack my things and leave.",
         "~g~You:~w~ Well that's good to hear. Legally, the landlord has the right to have you removed, but I'll let them know you're almost ready and I'll see what I can do.",
         "~r~Tenant:~w~ Okay, thank you so much!",
        };
        private readonly List<string> LandlordLeaves1 = new List<string>()
        {
         "~g~You:~w~ Alright, I talked to the resident and they have a new place and are almost ready to leave.",
         "~g~You:~w~ They just need a couple more days to get the rest of their stuff moved out. Is that okay for you?",
         "~b~Landlord:~w~ Can I call you guys back again if they're still hear after then?",
         "~g~You:~w~ Absolutely, you're well within your rights to have them removed now, but if you can spare a couple more days, it'll be easier for everyone.",
         "~b~Landlord:~w~ *Sigh* Alright Officer, that sounds fair enough. Hopefully this is all over soon.",
         "~g~You:~w~ Awesome, I appreciate the cooperation, glad we were able to find a middle ground.",
         "~b~Landlord:~w~ Alright, hopefully this turns out okay. Thanks for your help.",
        };
        private readonly List<string> LandlordLeaves2 = new List<string>()
        {
         "~g~You:~w~ Alright, I talked to the resident, turns out they have a new place and are almost ready to leave.",
         "~g~You:~w~ They just require a couple more days to get the rest of their stuff moved out. Is that okay for you?",
         "~b~Landlord:~w~ Can I get them arrested if they're still hear after then?",
         "~g~You:~w~ Absolutely, you're well within your rights to have them removed now, but if you can spare a couple more days, it'll be easier for everyone.",
         "~b~Landlord:~w~ *Sigh* Okay Officer, fair enough. Hopefully this is all over soon.",
         "~g~You:~w~ Great, I thanks for the cooperation, glad we were able to find a middle ground.",
         "~b~Landlord:~w~ No worries, thanks for your help.",
        };
        private readonly List<string> LandlordArrests1 = new List<string>()
        {
         "~g~You:~w~ Alright, I talked to the resident and they have a new place and are almost ready to leave.",
         "~g~You:~w~ They just need a couple more days to get the rest of their stuff moved out. Is that okay for you?",
         "~b~Landlord:~w~ ABSOLUTELY NOT! They've already outstayed their welcome. I'm not gonna just give them more time for free!",
         "~g~You:~w~ Okay, please calm down -.",
         "~b~Landlord:~w~ I WANT THEM REMOVED! ARRESTED! NOW!",
        };
        private readonly List<string> LandlordArrests2 = new List<string>()
        {
         "~g~You:~w~ Alright, I talked to the resident and they have a new place and are almost ready to leave.",
         "~g~You:~w~ They just need a couple more days to get the rest of their stuff moved out. Does that work for you?",
         "~b~Landlord:~w~ ABSOLUTELY NOT! They've wasted enough of my time, I'm done playing games!",
         "~g~You:~w~ Okay, please calm down -.",
         "~b~Landlord:~w~ I WANT THEM REMOVED! ARRESTED! NOW! DO YOUR JOB AND GET THEM OUT!",
        };

        public override bool OnBeforeCalloutDisplayed()
        {
            Game.LogTrivial("==========YOBBINCALLOUTS: Landlord-Tenant Dispute Callout Start==========");
            System.Random r = new System.Random();
            int Scenario = r.Next(0, 0); //only 1 scenario atm
            MainScenario = Scenario;
            Game.LogTrivial("YOBBINCALLOUTS: Scenario value is: " + MainScenario);

            CallHandler.locationChooser(CallHandler.HouseList, 696); //nice x2 lol
            if (CallHandler.locationReturned) MainSpawnPoint = CallHandler.SpawnPoint;
            else { Game.LogTrivial("YOBBINCALLOUTS: No house found. Aborting..."); return false; }

            ShowCalloutAreaBlipBeforeAccepting(MainSpawnPoint, 25f);
            AddMinimumDistanceCheck(60f, MainSpawnPoint);

            Functions.PlayScannerAudio("ATTENTION_ALL_UNITS_01 WE_HAVE_01 CITIZENS_REPORT_01 YC_CIVIL_DISTURBANCE");
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

                    if(Config.DisplayHelp) Game.DisplayHelp("Go to the scene and speak with the ~b~Landlord.");
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
                        CallHandler.IdleAction(Landlord, false);
                        while (!Game.IsKeyDown(Config.Key1) && !Game.IsKeyDown(Config.Key2)) GameFiber.Wait(0); //might do an animation?
                        if (Game.IsKeyDown(Config.Key1)) //accept
                        {
                            System.Random dialogue1 = new System.Random();
                            Game.DisplaySubtitle(AcceptSpeech[dialogue1.Next(0, AcceptSpeech.Count)], 2500);
                            GameFiber.Wait(3000);

                            Game.DisplayHelp("Knock on the ~o~Door.");
                            HouseBlip = new Blip(MainSpawnPoint);
                            HouseBlip.Color = Color.Orange;
                            HouseBlip.Alpha = 0.69f;

                            if (Landlord.DistanceTo(MainSpawnPoint) > 8f) Landlord.Tasks.FollowNavigationMeshToPosition(MainSpawnPoint, player.Heading, 2.69f, 2f); //test this
                            while (player.DistanceTo(MainSpawnPoint) >= 3f) GameFiber.Wait(0);
                            Game.DisplayHelp("Press ~y~" + Config.MainInteractionKey + "~w~ to ~b~Ring~w~ the Doorbell.");
                            while (!Game.IsKeyDown(Config.MainInteractionKey)) GameFiber.Wait(0);
                            CallHandler.Doorbell();
                            if (HouseBlip.Exists()) HouseBlip.Delete();
                            GameFiber.Wait(2500);

                            System.Random dialogue2 = new System.Random();
                            int ReasonDialogue = dialogue2.Next(0, 0); //change later
                            if (Config.DisplayHelp) Game.DisplayHelp("Press ~y~" + Config.MainInteractionKey + "~w~ to reason with the ~r~Tenant.");
                            if (MainScenario == 0) //cooperates (opens the door)
                            {
                                if (ReasonDialogue == 0) CallHandler.Dialogue(SuspectCoop1);
                                GameFiber.Wait(2000);
                                Suspect = new Ped(MainSpawnPoint, player.Heading - 180);
                                SuspectBlip = CallHandler.AssignBlip(Suspect, Color.Red, .69f, "Tenant");

                                System.Random r2 = new System.Random();
                                int action = r2.Next(0, 4); //what happens to the tenant
                                Game.LogTrivial("YOBBINCALLOUTS: Tenant action value is "+action);
                                if (action <= 1) //more dialogue
                                {
                                    GameFiber.Wait(1500);
                                    if(CallHandler.FiftyFifty()) CallHandler.Dialogue(SuspectLeaves1, Suspect);
                                    else CallHandler.Dialogue(SuspectLeaves2, Suspect);
                                    GameFiber.Wait(1500);
                                    CallHandler.IdleAction(Suspect, false);

                                    Game.DisplayHelp("Talk to the ~b~Landlord.");
                                    while (player.DistanceTo(Landlord) > 5) GameFiber.Wait(0);
                                    Landlord.Tasks.AchieveHeading(player.Heading - 180).WaitForCompletion(500);
                                    Game.DisplayHelp("Press ~y~" + Config.MainInteractionKey + "~w~ to Inform the ~b~Landlord.");
                                    while (!Game.IsKeyDown(Config.MainInteractionKey)) GameFiber.Wait(0);

                                    if(action == 0)
                                    {
                                        Game.LogTrivial("YOBBINCALLOUTS: Landlord leaves.");
                                        if (CallHandler.FiftyFifty()) CallHandler.Dialogue(LandlordLeaves1, Landlord);
                                        else CallHandler.Dialogue(LandlordLeaves2, Landlord);
                                        GameFiber.Wait(1500);
                                        if (Suspect.Exists()) Suspect.Tasks.AchieveHeading(Suspect.Heading - 180).WaitForCompletion(500);
                                        if (Suspect.Exists()) Suspect.Delete();
                                        if (Landlord.Exists()) Landlord.Dismiss();
                                    }
                                    else
                                    {
                                        Game.LogTrivial("YOBBINCALLOUTS: Landlord wants tenant arrested.");
                                        if (CallHandler.FiftyFifty()) CallHandler.Dialogue(LandlordArrests1, Landlord);
                                        else CallHandler.Dialogue(LandlordArrests2, Landlord);
                                        GameFiber.Wait(1500);
                                        CallHandler.IdleAction(Landlord, false);
                                        Game.DisplayHelp("Deal with the ~r~Tenant~w~ as you see fit. Press ~y~" + Config.CalloutEndKey + " ~w~when ~b~Done.~w~");
                                        while (!Game.IsKeyDown(Config.CalloutEndKey)) GameFiber.Wait(0);
                                    }
                                }
                                else //attack
                                {
                                    Game.LogTrivial("YOBBINCALLOUTS: Landlord attacks.");
                                    System.Random r3 = new System.Random();
                                    GameFiber.Wait(r3.Next(500, 3000));
                                    System.Random dialogue3 = new System.Random();
                                    Game.DisplaySubtitle(AttackSpeech[dialogue3.Next(0, AttackSpeech.Count)], 2000);
                                    GameFiber.Wait(r3.Next(500, 2000));
                                    Suspect.BlockPermanentEvents = false; //test this (wasn't fighting before)
                                    Landlord.Tasks.FightAgainst(Suspect, -1);
                                    if (action == 2)
                                    {
                                        Suspect.BlockPermanentEvents = false; //flee
                                        Suspect.Tasks.ReactAndFlee(Landlord); //test this
                                        Game.LogTrivial("YOBBINCALLOUTS: Tenant flees/fights back.");
                                    }
                                    else
                                    {
                                        Suspect.Tasks.FightAgainst(Landlord, -1); //Tenant fights back
                                        Game.LogTrivial("YOBBINCALLOUTS: Tenant fights back.");
                                    }
                                    LandlordBlip.Color = Color.Orange;

                                    if(action == 2) CallHandler.SuspectWait(Landlord);  //Suspect doesn't fight back
                                    else //Suspect does fight back
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
                                    GameFiber.Wait(2500);
                                    Game.LogTrivial("YOBBINCALLOUTS: Tenant and Landlord either arrested/killed.");
                                    //I might make the help message depend on if the landlord and/or tenants are arrested, but also might be too lazy to do that
                                    Game.DisplayNotification("Deal with the ~o~Landlord~w~ and ~r~Tenant~w~ as you see fit. Press ~b~"+Config.CalloutEndKey+" ~w~when done."); //this usually gets blocked so made it a notification
                                    while (!Game.IsKeyDown(Config.CalloutEndKey)) GameFiber.Wait(0);
                                }
                            }
                            else //does not cooperate
                            {
                                //add new dialogue
                            }
                        }
                        else //decline
                        {
                            int EndingDialogue = dialogue.Next(0, 3);
                            if (EndingDialogue == 0) CallHandler.Dialogue(Deny1, Landlord);
                            else if (EndingDialogue == 1) CallHandler.Dialogue(Deny2, Landlord);
                            else CallHandler.Dialogue(Deny3, Landlord);
                            Landlord.Dismiss();
                            if (LandlordBlip.Exists()) LandlordBlip.Delete();
                        }
                        break;
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
            if (LandlordBlip.Exists()) LandlordBlip.Delete();
            if (HouseBlip.Exists()) HouseBlip.Delete();
            if (Landlord.Exists()) Landlord.Dismiss();
            if (Suspect.Exists()) Suspect.Dismiss();
            //if (Landlord.Exists()) Landlord.Dismiss();
            Game.LogTrivial("YOBBINCALLOUTS: Landlord-Tenant Dispute Callout Finished Cleaning Up.");
        }
        public override void Process()
        {
            base.Process();
        }
    }
}
