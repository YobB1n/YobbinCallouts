using System.Collections.Generic;
using System.Windows.Forms;
using Rage;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage.Native;
using System.Drawing;
using System;

//This Callout was inspired by a LivePD Call.
namespace YobbinCallouts.Callouts
{
    [CalloutInfo("Stolen Cellphone", CalloutProbability.Medium)]

    public class StolenCellPhone : Callout
    {
        private Vector3 MainSpawnPoint;

        private Blip House;
        private Blip SuspectBlip;
        private Blip VictimBlip;
        private Blip CellPhoneAreaBlip;

        private Ped Victim;
        private Ped Suspect;

        private Rage.Object Phone;

        private LHandle SuspectPursuit;

        private int MainScenario;

        private string Zone;

        private Model VictimModel;

        Ped player = Game.LocalPlayer.Character;

        private LHandle MainPursuit;

        private bool CalloutRunning = false;
        private bool IsHouse = true;

        //All the dialogue for the callout. Haven't found a better way to store it yet, so this will have to do.
        private readonly List<string> OpeningDialogue1 = new List<string>()
        {
         "~b~Caller:~w~ How are you doing, officer?",
         "~g~You:~w~ Bot too bad, thanks. What's the issue here?",
         "~b~Caller:~w~ Somebody stole my cell phone a few hours ago. I didn't see who took it, unfortunately.",
         "~b~Caller:~w~ However, they just turned it on, and it can be tracked using the phone's tracking app.",
         "~b~Caller:~w~ Would you be able to locate who took my phone? I really need it.",
        };
        private readonly List<string> Accept = new List<string>()
        {
         "~g~You:~w~ Yeah of course, we can see if we'll be able to locate your property.",
         "~b~Caller:~w~ Great, I have some important information on that phone that I really need back. Let me get you the information.",
         "~b~Caller:~w~ You can use it on your phone's app to locate where the phone is.",
         "~g~You:~w~ Alright, hopefully I'll be able to find it. I'll let you know if I do.",
        };
        private readonly List<string> Decline = new List<string>()
        {
         "~b~Caller:~w~ How are you doing, officer?",
         "~g~You:~w~ Not too bad, thanks. What's the issue here?",
         "~b~Caller:~w~ Somebody stole my cell phone a few hours ago. I didn't see who took it, unfortunately.",
         "~b~Caller:~w~ However, they just turned it on, and it can be tracked using the phone's tracking app.",
         "~b~Caller:~w~ Would you be able to locate who took my phone? I'd like to press charges.",
        };
        private readonly List<string> SuspectCoop = new List<string>()
        {
            "~r~Suspect:~w~ Oh, hello officer, what's wrong?",
            "~g~You:~w~ I'm conducting an investigation into a stolen phone. The phone appears to be on your person according to the tracking app I have.",
            "~g~You:~w~ If you're honest with me, it'll make this go a lot easier. Do you have a stolen phone in your posession?",
            "~r~Suspect:~w~ Alright officer, I ain't gonna lie to you, yeah I have the phone. Stole it a couple hours ago.",
            "~r~Suspect:~w~ Things are tough, though I could get away with it. Guess I'm new at this, because I forgot to turn it off.",
            "~g~You:~w~ I appreciate you being honest with me. Could you hand me the phone please?",
        };
        private readonly List<string> SuspectCoopArrest = new List<string>()
        {
            "~g~You:~w~ I appreciate you being cooperative, however unfortunately I'm going to have to book you for this.",
            "~r~Suspect:~w~ I understand officer. That was a poor decision on my part.",
            "~g~You:~w~ Hands behind your back, please.",
        };
        private readonly List<string> SuspectCoopLetGo = new List<string>()
        {
            "~r~Suspect:~w~ Are you going to arrest me now, officer?",
            "~g~You:~w~ I normally would, but you've been very cooperative with my investigation. This seems like your first time doing something like this as well.",
            "~g~You:~w~ I'm going to let you off the hook, but please learn from this experience.",
            "~r~Suspect:~w~ I absolutely will officer. Thanks so much for your understanding, I really appreciate it.",
            "~g~You:~w~ Alright, take care now.",
        };
        private readonly List<string> SuspectDenies = new List<string>()
        {
            "~r~Suspect:~w~ Oh, hello officer, what's wrong?",
            "~g~You:~w~ I'm conducting an investigation into a stolen phone. The phone appears to be on your person according to the tracking app I have.",
            "~g~You:~w~ If you're honest with me, it'll make this go a lot easier. Do you have a stolen phone in your posession?",
            "~r~Suspect:~w~ I didn't do anything officer. Please stop harassing me.",
            "~g~You:~w~ I have the victim's phone tracking right here. Their phone is clearly showing up as on your person.",
            "~g~You:~w~ Now, please don't make this harder than it has to be. Could I have the phone, please?",
        };
        private readonly List<string> VictimEnding1 = new List<string>()
        {
         "~b~Caller:~w~ Did you find it Officer?",
         "~g~You:~w~ I did. Here you go!",
         "~b~Caller:~w~ Awesome! Thanks so much, I really needed it!",
        };
        public override bool OnBeforeCalloutDisplayed()
        {
            Game.LogTrivial("==========YOBBINCALLOUTS: Stolen Cellphone Callout Start==========");
            System.Random r = new System.Random();
            int Scenario = r.Next(0, 5);    //change later
            MainScenario = Scenario;
            Game.LogTrivial("YOBBINCALLOUTS: Scenario is Value is " + MainScenario);
            Zone = Functions.GetZoneAtPosition(Game.LocalPlayer.Character.Position).RealAreaName;
            Game.LogTrivial("YOBBINCALLOUTS: Zone is " + Zone);
            CallHandler.locationChooser(CallHandler.HouseList);
            if (CallHandler.locationReturned) { MainSpawnPoint = CallHandler.SpawnPoint; }
            else { Game.LogTrivial("YOBBINCALLOUTS: Could not find suitable house for callout location. Aborting Callout."); return false; }
            ShowCalloutAreaBlipBeforeAccepting(MainSpawnPoint, 75f);    //Callout Blip Circle with radius 50m
            AddMinimumDistanceCheck(50f, MainSpawnPoint);   //Player must be 50m or further away
            Functions.PlayScannerAudio("CITIZENS_REPORT YC_STOLEN_PROPERTY");  //change this
            CalloutMessage = "Stolen Cellphone";
            CalloutPosition = MainSpawnPoint;
            CalloutAdvisory = "Caller Has Reported a ~r~Cellphone Stolen.";
            return base.OnBeforeCalloutDisplayed();
        }
        public override bool OnCalloutAccepted()
        {
            Game.LogTrivial("YOBBINCALLOUTS: Stolen Cellphone Callout Accepted by User.");
            if (Main.CalloutInterface)
            {
                CalloutInterfaceHandler.SendCalloutDetails(this, "CODE 2", "");
            }
            else
            {
                Game.DisplayNotification("Respond ~b~Code 2~w~.");
            }
            House = new Blip(MainSpawnPoint, 35);
            House.IsRouteEnabled = true;
            House.Color = System.Drawing.Color.Yellow;
            House.Alpha = 0.67f;
            House.Name = "Caller";

            Victim = new Ped(MainSpawnPoint, 69);
            Victim.IsPersistent = true;
            Victim.BlockPermanentEvents = true;
            Victim.IsInvincible = true;
            VictimModel = Victim.Model;

            if (Config.DisplayHelp)
            {
                if (CallHandler.locationReturned) Game.DisplayHelp("Go to the ~y~Property~w~ Shown on The Map to Investigate.");
                else Game.DisplayHelp("Go to the ~y~Caller~w~ Shown on The Map to Investigate.");
            }
            if (CalloutRunning == false) Callout();
            return base.OnCalloutAccepted();
        }
        public override void OnCalloutNotAccepted()
        {
            Game.LogTrivial("YOBBINCALLOUTS: Stolen Cellphone Callout Not Accepted by User.");
            base.OnCalloutNotAccepted();
        }
        private void Callout()
        {
            CalloutRunning = true;
            try
            {
                GameFiber.StartNew(delegate
                {
                    while (CalloutRunning == true)
                    {
                        while (player.DistanceTo(Victim) >= 35 && !Game.IsKeyDown(Config.CalloutEndKey)) GameFiber.Wait(0);
                        if (Game.IsKeyDown(Config.CalloutEndKey)) { EndCalloutHandler.CalloutForcedEnd = true; break; }
                        House.Delete();
                        VictimBlip = Victim.AttachBlip();
                        VictimBlip.IsFriendly = true;
                        VictimBlip.Scale = 0.75f;
                        Game.DisplayHelp("Talk to the ~b~Caller.");
                        NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(Victim, player, -1);
                        while (player.DistanceTo(Victim) >= 5) GameFiber.Wait(0);
                        if (Config.DisplayHelp) Game.DisplayHelp("Press ~y~" + Config.MainInteractionKey + " ~w~to talk to the ~b~Caller.");
                        CallHandler.Dialogue(OpeningDialogue1, Victim);
                        GameFiber.Wait(1500);
                        Victim.Tasks.ClearImmediately();
                        Game.DisplayHelp("~y~" + Config.Key1 + ":~b~ Of Course, I'll Try to Find Your Phone. ~y~" + Config.Key2 + ": ~b~That's a Waste of Time, I'll Just Take Your Statement.");
                        while (!Game.IsKeyDown(Config.Key1) && !Game.IsKeyDown(Config.Key2)) GameFiber.Wait(0); //might do an animation?
                        if (Game.IsKeyDown(Config.Key1))
                        {
                            if (Config.DisplayHelp) Game.DisplayHelp("Press ~y~" + Config.MainInteractionKey + " ~w~to Continue Speaking With the ~b~Caller.");
                            CallHandler.Dialogue(Accept, Victim);
                            GameFiber.Wait(1500);
                            Victim.Tasks.ClearImmediately();
                            if (VictimBlip.Exists()) VictimBlip.Delete();
                            CallHandler.IdleAction(Victim, false);
                            Game.DisplayHelp("Return to your ~b~Vehicle~w~ to Start ~o~Tracking ~w~the~y~ Phone.");
                            TrackPhone();
                        }
                        else
                        {
                            End();
                        }
                    }
                }
                );
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
        public override void End()
        {
            base.End();
            if (CalloutRunning)
            {
                Game.DisplayNotification("~g~Code 4~w~, return to patrol.");
                Functions.PlayScannerAudio("ATTENTION_ALL_UNITS WE_ARE_CODE_4");
            }
            CalloutRunning = false;
            if (Suspect.Exists()) { Suspect.Dismiss(); }
            if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }
            if (House.Exists()) { House.Delete(); }
            if (Phone.Exists()) Phone.Delete();
            if (Victim.Exists()) Victim.Dismiss();
            if (VictimBlip.Exists()) VictimBlip.Delete();
            if (CellPhoneAreaBlip.Exists()) CellPhoneAreaBlip.Delete();
            Game.LogTrivial("YOBBINCALLOUTS: Stolen Cellphone Callout Finished Cleaning Up.");
        }
        public override void Process()
        {
            base.Process();
        }
        private void TrackPhone()
        {
            if (CalloutRunning)
            {
                while (!player.IsInAnyVehicle(false)) GameFiber.Wait(0);
                Vector3 SuspectSpawn = World.GetNextPositionOnStreet(player.Position.Around(550));
                Suspect = new Ped(SuspectSpawn, 69);
                Suspect.IsPersistent = true;
                Suspect.BlockPermanentEvents = true;
                Phone = new Rage.Object("prop_npc_phone", Vector3.Zero);
                Phone.AttachTo(Suspect, Suspect.GetBoneIndex(PedBoneId.LeftHand), new Vector3(0.1490f, 0.0560f, -0.0100f), new Rotator(-17f, -142f, -151f));
                Phone.IsPersistent = true;
                //Phone.IsVisible = false;
                Suspect.Tasks.Wander();

                CellPhoneAreaBlip = new Blip(Suspect.Position.Around(15), 100);
                CellPhoneAreaBlip.Color = Color.Yellow;
                CellPhoneAreaBlip.Alpha = 0.67f;
                CellPhoneAreaBlip.IsRouteEnabled = true;

                if(Suspect.Exists()) while (player.DistanceTo(Suspect) >= 50) GameFiber.Wait(0);
                Game.DisplayNotification("You are ~g~Close~w~ to the ~y~Phone!~w~ Enabling ~b~Fine Location~w~ Information.");
                if (CellPhoneAreaBlip.Exists()) CellPhoneAreaBlip.Delete();
                CellPhoneAreaBlip = Suspect.AttachBlip();
                CellPhoneAreaBlip.IsFriendly = false;
                CellPhoneAreaBlip.Scale = 0.75f;
                while (player.DistanceTo(Suspect) >= 5) GameFiber.Wait(0);
                Game.DisplaySubtitle("~g~You:~w~ Hey, Could I Speak With You for a Sec?", 3000);
                GameFiber.Wait(3000);
                Suspect.Tasks.AchieveHeading(player.Heading - 180).WaitForCompletion(500);
                if (Config.DisplayHelp) Game.DisplayHelp("Press ~y~" + Config.MainInteractionKey + " ~w~to talk to the ~r~Suspect.");
                if (MainScenario == 0) Cooperates();
                else if (MainScenario == 1 || MainScenario == 2) Denies();
                else if (MainScenario == 3) Runs();
            }
        }
        private void Cooperates()
        {
            if (CalloutRunning)
            {
                CallHandler.Dialogue(SuspectCoop, Suspect);
                Decide();
            }
        }
        private void Denies()
        {
            if (CalloutRunning)
            {
                CallHandler.Dialogue(SuspectDenies, Suspect);
                GameFiber.Wait(1500);
                if (MainScenario == 2) Decide();
                else Runs();
            }
        }
        private void Runs()
        {
            if (CalloutRunning)
            {
                CallHandler.CreatePursuit(MainPursuit, true, true, true, Suspect);
                if (SuspectBlip.Exists()) SuspectBlip.Delete();
                WrapUp();
            }
        }
        private void Decide()
        {
            if (CalloutRunning)
            {
                GameFiber.Wait(1500);
                Suspect.Tasks.ClearImmediately();
                Phone.IsVisible = true;
                Suspect.Tasks.AchieveHeading(player.Heading - 180).WaitForCompletion(500);
                //some animation
                GameFiber.Wait(1000);
                Suspect.Tasks.PlayAnimation("mp_common", "givetake1_b", 1f, AnimationFlags.Loop);
                Phone.Detach();
                Phone.AttachTo(player, player.GetBoneIndex(PedBoneId.RightHand), new Vector3(0.1490f, 0.0560f, -0.0100f), new Rotator(-17f, -142f, -151f)); //might have to change to other hand anim?
                GameFiber.Wait(1000);
                Suspect.Tasks.ClearImmediately();
                GameFiber.Wait(1500);
                Phone.Delete();
                GameFiber.Wait(2000);
                Game.DisplayHelp("~y~" + Config.Key1 + ":~b~ Arrest the Suspect.~y~ " + Config.Key2 + ":~b~ Let the Suspect Off the Hook.");
                while (!Game.IsKeyDown(Config.Key1) && !Game.IsKeyDown(Config.Key2)) GameFiber.Wait(0); //might do an animation?
                
                if (Game.IsKeyDown(Config.Key1)) //arrest
                {
                    if (Config.DisplayHelp) Game.DisplayHelp("Press ~y~" + Config.MainInteractionKey + " ~w~to Continue Talking to the ~r~Suspect.");
                    GameFiber.Yield();
                    CallHandler.Dialogue(SuspectCoopArrest, Suspect);
                    GameFiber.Wait(1500);
                    Suspect.Tasks.ClearImmediately();
                    Game.DisplayHelp("Arrest the ~r~Suspect.");
                    if (MainScenario == 4) { GameFiber.Wait(500); Runs(); }
                    else
                    {
                        CallHandler.SuspectWait(Suspect);
                        if (SuspectBlip.Exists()) SuspectBlip.Delete();
                        WrapUp();
                    }
                }
                else //let go
                {
                    if (Config.DisplayHelp) Game.DisplayHelp("Press ~y~" + Config.MainInteractionKey + " ~w~to Continue Talking to the ~r~Suspect.");
                    if(Suspect.Exists()) CallHandler.Dialogue(SuspectCoopLetGo, Suspect);
                    GameFiber.Wait(1500);
                    if (Suspect.Exists())
                    {
                        Suspect.Tasks.ClearImmediately();
                        if (Suspect.IsAlive) Suspect.Dismiss();
                    }
                    //Test this blip deletion
                    if(SuspectBlip.Exists()) SuspectBlip.Alpha = 0;
                    if (SuspectBlip.Exists()) SuspectBlip.Delete(); //botched
                    WrapUp();
                }
            }
        }
        private void WrapUp()
        {
            if (CalloutRunning)
            {
                if (Phone.Exists()) Phone.Delete();
                if (SuspectBlip.Exists()) SuspectBlip.Delete();
                GameFiber.Wait(1500);
                Game.DisplayHelp("Take the Phone Back to the ~b~Caller.");
                if (!Victim.Exists()) Victim = new Ped(VictimModel, MainSpawnPoint, 69);
                VictimBlip = Victim.AttachBlip();
                VictimBlip.IsFriendly = true;
                VictimBlip.IsRouteEnabled = true;
                VictimBlip.Scale = 0.75f;
                if(CalloutRunning) while (player.DistanceTo(Victim) >= 5) GameFiber.Wait(0); //test
                if (Config.DisplayHelp) Game.DisplayHelp("Press ~y~" + Config.MainInteractionKey + " ~w~to Return the Phone ~b~Caller.");
                CallHandler.Dialogue(VictimEnding1, Victim);
                Phone = new Rage.Object("prop_npc_phone", Vector3.Zero);
                Phone.IsPersistent = true;
                GameFiber.Wait(1000);
                player.Tasks.PlayAnimation("mp_common", "givetake1_b", 1f, AnimationFlags.Loop);
                Phone.Detach();
                Phone.AttachTo(player, player.GetBoneIndex(PedBoneId.RightHand), new Vector3(0.1490f, 0.0560f, -0.0100f), new Rotator(-17f, -142f, -151f)); //might have to change to other hand anim?
                GameFiber.Wait(1000);
                player.Tasks.ClearImmediately();
                GameFiber.Wait(1500);
                Phone.Delete();
                GameFiber.Wait(2000);
                Victim.Dismiss();

                Game.LogTrivial("YOBBINCALLOUTS: Callout Finished, Ending...");
                EndCalloutHandler.EndCallout();
                End();
            }
        }
    }
}