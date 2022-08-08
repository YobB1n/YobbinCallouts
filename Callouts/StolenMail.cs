//THIS CALLOUT IS NOT FINISHED (or even started lol)
using System.Drawing;
using Rage;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage.Native;
using System;
using System.Collections.Generic;

namespace YobbinCallouts.Callouts
{
    public class StolenMail : Callout
    {
        private Vector3 MainSpawnPoint;

        private Ped Suspect;
        private Ped HouseOwner;
        Ped player = Game.LocalPlayer.Character;
        private Blip HouseOwnerBlip;
        private Blip SuspectBlip;
        private Blip SearchArea;
        private Vector3 DroppedMail;
        private Blip DroppedMailBlip;
        private int MainScenario;
        private Rage.Object Mail;
        private Rage.Object Note;
        private bool CalloutRunning;
        List<string> HouseOwnerDialogue = new List<string>()
        {
            "Home Owner: Hello Officer. Yes, I was the one who called about my mail being stolen. Thank you for coming.",
            "You: How long has this been happening for?",
            "Home Owner: This has been happening for a week now.",
            "You: Why didn't you call us before?",
            "Home Owner: I thought the mail company took a break.",
            "You: That is an excuse for being lazy if I have ever heard of one. Anyways, do you have any description of the person?",
            "Home Owner: No."
        };
        List<string> HouseOwnerFalseAlarmDialogue = new List<string>()
        {
            "Home Owner: Hello Officer. Sorry for the trouble. My mail was just put on hold for a week longer than it should have by the mail company because our vacation was cut short.",
            "You: So you are getting your mail?",
            "Home Owner: Yes. It was a false alarm. Sorry about that.",
            "You: No worries. Please make sure you contact 911 only for emergencies.",
            "Home Owner: Sorry about that. Will do. Stay safe"
        };
        List<string> SuspectDialogue = new List<string>()
        {
            "Suspect: Watchu want",
            "You: Hey, just want to talk to you. Whats in your hand there?",
            "Suspect: Watchu think it is, mail. punk ass",
            "You: Where did you get that mail from?",
            "Suspect: Places.....",
            "You: I am going to go straight to the point if you straight with me, aight. We got a call about someone stealing mail. You are the only one in this vicinity that is walking around with mail. Did you steal it",
            "Suspect: fine man, you got me...good job sherlock",
            "You: Do you got any weapons on you?",
            "Suspect: nah man, who the fuck you think I am. Don't piss me off 'fore I beat yo ass up."
        };
        System.Windows.Forms.Keys EndKey = Config.CalloutEndKey;
        System.Windows.Forms.Keys InteractionKey = Config.MainInteractionKey;
        Random monke = new Random();

        public override bool OnBeforeCalloutDisplayed()
        {
            Game.LogTrivial("==========YOBBINCALLOUTS: Explosion Callout Start==========");
            MainScenario = monke.Next(0, 0);
            Game.LogTrivial("YOBBINCALLOUTS: Scenario Number is " + MainScenario + "");

            Game.LogTrivial("Getting Location");
            CallHandler.locationChooser(CallHandler.HouseList);
            if (CallHandler.locationReturned) { MainSpawnPoint = CallHandler.SpawnPoint; Game.LogTrivial("Spawnpoint vector is " + MainSpawnPoint); } else { Game.LogTrivial("No Location found. Ending Callout");  return false; }
            ShowCalloutAreaBlipBeforeAccepting(MainSpawnPoint, 25f);

            Functions.PlayScannerAudio("CITIZENS_REPORT YC_STOLEN_PROPERTY");

            CalloutMessage = "Stolen Mail";
            CalloutPosition = MainSpawnPoint;
            CalloutAdvisory = "RP states that he has not gotten mail in several days. He thinks that his mail is being stolen.";

            return base.OnBeforeCalloutDisplayed();
        }

        public override void OnCalloutNotAccepted()
        {
            Game.LogTrivial("Callout Not Accepted by User.");
            base.OnCalloutNotAccepted();
        }

        public override bool OnCalloutAccepted()
        {
            try
            {
                Game.LogTrivial("Callout accepted by user");

                if (Main.CalloutInterface)
                {
                    CalloutInterfaceHandler.SendCalloutDetails(this, "Code 2", "");
                }
                else
                {
                    Game.DisplayNotification("Respond ~b Code 2.");

                }
                HouseOwner = new Ped(MainSpawnPoint.Around(2));
                HouseOwner.IsPersistent = true;
                HouseOwner.BlockPermanentEvents = true;
                HouseOwnerBlip = CallHandler.AssignBlip(HouseOwner, Color.Blue, .69f, "Caller", true);
                NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(HouseOwner, player, -1);

                Game.DisplayHelp("Speak with RP.");
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
            int num = monke.Next(0, 101);
            if(num <= 25)
            {
                FalseAlarm();
            }
            else
            {
                Callout();
            }


            return base.OnCalloutAccepted();
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
                        while (Vector3.Distance(player.Position, HouseOwner.Position) >= 25f && !Game.IsKeyDown(EndKey)) { GameFiber.Wait(0); }
                        if (Game.IsKeyDown(EndKey)) { break; }
                        CallHandler.IdleAction(HouseOwner, false);
                        while (Vector3.Distance(player.Position, HouseOwner.Position) >= 7.5f) { GameFiber.Wait(0); }
                        Game.DisplaySubtitle("~g~You:~w~ Hello Sir. Did you call about your mail being stolen.");
                        HouseOwnerBlip.IsRouteEnabled = false;
                        HouseOwner.Tasks.AchieveHeading(player.Heading - 180f).WaitForCompletion(500);
                        if (Config.DisplayHelp) Game.DisplayHelp("Press ~y~" + Config.MainInteractionKey + "~w~ to speak with the ~b~Landlord.");
                        Vector3 SuspectSpawn = World.GetNextPositionOnStreet(player.Position.Around(550f));
                        Suspect = new Ped(SuspectSpawn, 69);
                        Suspect.IsPersistent = true;
                        Suspect.BlockPermanentEvents = true;
                        Suspect.Tasks.Wander();
                        CallHandler.Dialogue(HouseOwnerDialogue, HouseOwner);
                        PedBackground SuspectPersona = new PedBackground(Suspect);
                        Game.DisplaySubtitle("You: Really, you cannot tell me anything about the suspect? I cannot guarantee I will be able to find him, but I will try my best.");
                        if (Config.DisplayHelp) { Game.DisplayNotification("Press " + InteractionKey + " to continue dialogue"); }
                        while (!Game.IsKeyDown(InteractionKey)) { GameFiber.Wait(0); }
                        Game.DisplaySubtitle("Home Owner: Umm...the person ran off....before I could remember any of that. I think the person was " + SuspectPersona.Gender + " and was holding my mail while wandering off.");
                        while (!Game.IsKeyDown(InteractionKey)) { GameFiber.Wait(0); }
                        Game.DisplaySubtitle("You: Ok. Will try my best. Thank you.");
                        SearchArea = new Blip(Suspect.Position.Around(15), 50);
                        FindSuspect();
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

        private void FalseAlarm()
        {
            CalloutRunning = true;
            GameFiber.StartNew(delegate
            {
                try
                {
                    while (CalloutRunning)
                    {
                        while (Vector3.Distance(player.Position, HouseOwner.Position) >= 25f && !Game.IsKeyDown(EndKey)) { GameFiber.Wait(0); }
                        if (Game.IsKeyDown(EndKey)) { break; }
                        CallHandler.IdleAction(HouseOwner, false);
                        while (Vector3.Distance(player.Position, HouseOwner.Position) >= 7.5f) { GameFiber.Wait(0); }
                        Game.DisplaySubtitle("~g~You:~w~ Hello Sir. Did you call about your mail being stolen.");
                        HouseOwnerBlip.IsRouteEnabled = false;
                        HouseOwner.Tasks.AchieveHeading(player.Heading - 180f).WaitForCompletion(500);
                        if (Config.DisplayHelp) Game.DisplayHelp("Press ~y~" + Config.MainInteractionKey + "~w~ to speak with the ~b~Landlord.");
                        CallHandler.Dialogue(HouseOwnerFalseAlarmDialogue, HouseOwner);
                        GameFiber.Wait(1500); 
                        Game.DisplayNotification("Dispatch, It was a ~g~False Alarm~w~. I will be ~g~Code 4~w~.");
                        GameFiber.Wait(2000);
                        Functions.PlayScannerAudio("REPORT_RESPONSE_COPY_02");
                        GameFiber.Wait(2000);
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
            }



            );

        }


        private void DetachAndSetBlip()
        {
            if (Mail.Exists())
            {
                Mail.Detach();
                DroppedMail = Mail.Position;
                DroppedMailBlip = CallHandler.AssignBlip(Mail, Color.Blue, .4f);
            }
        }

        private void FindSuspect()
        {
            if (CalloutRunning)
            {
                Mail = new Rage.Object("prop_cs_envolope_01", Vector3.Zero);
                Mail.IsPersistent = true;
                Mail.AttachTo(Suspect, Suspect.GetBoneIndex(PedBoneId.LeftHand), new Vector3(0.1490f, 0.0560f, -0.0100f), new Rotator(-17f, -142f, -151f));
                while (!player.IsInAnyVehicle(false) && Vector3.Distance(player.Position, Suspect.Position) <= 25f) { GameFiber.Wait(0); }
                if (SearchArea.Exists()) { SearchArea.Delete(); }
                SuspectBlip = CallHandler.AssignBlip(Suspect, Color.Red, .69f);
                while (player.DistanceTo(Suspect) >= 5) GameFiber.Wait(0);
                Game.DisplaySubtitle("You: Hey, Could I Speak With You for a Sec?", 3000);
                if (CallHandler.FiftyFifty()) { Cooperates(); }
                else
                {
                    if (CallHandler.FiftyFifty()) { Runs();} else { Shoots(); }
                }
            }
        }

        private void Cooperates()
        {
            CallHandler.Dialogue(SuspectDialogue, Suspect);
            DetachAndSetBlip();  
            if (Config.DisplayHelp) { Game.DisplayNotification("Arrest the suspect"); }
            WrapUp();
        }

        private void Runs()
        {
            if (CalloutRunning)
            {
                Functions.PlayScannerAudio("CRIME_SUSPECT_ON_THE_RUN_01");
                DetachAndSetBlip();
                LHandle SuspectPursuit = Functions.CreatePursuit();
                Suspect.Inventory.Weapons.Clear();  
                Functions.SetPursuitIsActiveForPlayer(SuspectPursuit, true);
                Functions.AddPedToPursuit(SuspectPursuit, Suspect);
                while (Functions.IsPursuitStillRunning(SuspectPursuit)) GameFiber.Wait(0);
                while (Suspect.Exists())
                {
                    GameFiber.Yield();
                    if (!Suspect.Exists() || Suspect.IsDead || Functions.IsPedArrested(Suspect))
                    {
                        break;
                    }
                }
                if (Suspect.Exists())
                {
                    if (Functions.IsPedArrested(Suspect)) { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect is Under ~g~Arrest~w~ Following the Pursuit."); }
                }
                if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }
                GameFiber.Wait(2000);
                Functions.PlayScannerAudio("REPORT_RESPONSE_COPY_02");
                GameFiber.Wait(2000);
                if (SuspectBlip.Exists()) SuspectBlip.Delete();
                WrapUp();
            }
        }
        private void Shoots()
        {
            DetachAndSetBlip();
            Suspect.Inventory.GiveNewWeapon("WEAPON_APPISTOL", -1, true);
            Suspect.Tasks.GoToWhileAiming(World.GetNextPositionOnStreet(Suspect.Position.Around(550)), player.Position, 5f, Suspect.Speed * (float)2.5, true, FiringPattern.FullAutomatic).WaitForCompletion(3000);
            while(Suspect.Exists() && Suspect.IsAlive) { GameFiber.Wait(0); }
            if (!Suspect.Exists()) { Game.DisplayNotification("Dispatch, a Suspect was ~r~killed~w~ following a foot chase and a shootout."); }
            GameFiber.Wait(2000);
            Functions.PlayScannerAudio("REPORT_RESPONSE_COPY_02");
            GameFiber.Wait(2000);
            if (SuspectBlip.Exists()) SuspectBlip.Delete();
            WrapUp();
        }
        private void WrapUp()
        {
            if (CalloutRunning)
            {
                if (Config.DisplayHelp) { Game.DisplayNotification("Retrieve the mail"); }
                while(Vector3.Distance(player.Position, DroppedMail) >= 5f) { GameFiber.Wait(0); }
                if (Config.DisplayHelp) { Game.DisplayNotification("Press " + InteractionKey + " in order to retrieve the mail"); }
                while (!Game.IsKeyDown(InteractionKey)) { GameFiber.Wait(0); }
                player.Tasks.PlayAnimation("amb@medic@standing@kneel@idle_a", "idle_b", 1f, AnimationFlags.Loop);
                GameFiber.Wait(1000);
                Mail.AttachTo(player, player.GetBoneIndex(PedBoneId.LeftHand), new Vector3(0.1490f, 0.0560f, -0.0100f), new Rotator(-17f, -142f, -151f));
                GameFiber.Wait(1000);
                player.Tasks.Clear();
                GameFiber.Wait(1000);
                if (Config.DisplayHelp) { Game.DisplayNotification("Go return mail to home owner."); }
                HouseOwnerBlip.IsRouteEnabled = true;
                while (Vector3.Distance(player.Position, HouseOwner.Position) >= 7.5f) { GameFiber.Wait(0); }
                HouseOwner.Tasks.AchieveHeading(player.Heading - 180f).WaitForCompletion(500);
                Game.DisplaySubtitle("You: I have retrieved your mail sir.");
                if(Config.DisplayHelp) { Game.DisplayNotification("Press + " + InteractionKey + " to return mail."); }
                while(!Game.IsKeyDown(InteractionKey)) { GameFiber.Wait(0); }
                player.Tasks.PlayAnimation("mp_common", "givetake1_b", 1f, AnimationFlags.Loop);
                GameFiber.Wait(1000);
                Mail.AttachTo(HouseOwner, HouseOwner.GetBoneIndex(PedBoneId.LeftHand), new Vector3(0.1490f, 0.0560f, -0.0100f), new Rotator(-17f, -142f, -151f));
                GameFiber.Wait(1000);
                player.Tasks.Clear();
                GameFiber.Wait(1000);
                Game.DisplaySubtitle("Home Owner: Thank you so much officer.");
                GameFiber.Wait(2000);
                Game.DisplaySubtitle("You: No worries");
                End();

            }
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
            if (HouseOwnerBlip.Exists()) { HouseOwnerBlip.Delete(); }
            if (Suspect.Exists()) { Suspect.Dismiss(); }
            if (Mail.Exists()) { Mail.Delete(); }
            if (HouseOwner.Exists()) { HouseOwner.Dismiss(); }
            if (SearchArea.Exists()) { SearchArea.Delete(); }
            if (DroppedMailBlip.Exists()) { DroppedMailBlip.Delete(); }
            Game.LogTrivial("YOBBINCALLOUTS: Stolen Mail Callout Finished Cleaning Up.");
        }

        public override void Process()
        {
            base.Process();
        }

    }
}
