using System.Collections.Generic;
using System.Windows.Forms;
using Rage;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage.Native;
using System.Drawing;
using System;

namespace YobbinCallouts.Callouts
{
    [CalloutInfo("Sovereign Citizen", CalloutProbability.High)] //Change later
    public class SovereignCitizen : Callout
    {
        private Vector3 MainSpawnPoint;

        private Blip Area;
        private Blip SuspectBlip;
        private Blip OfficerBlip;

        private Vehicle SuspectVehicle;
        private Vehicle OfficerVehicle;

        private Ped Suspect;
        private Ped Officer;
        private Ped player = Game.LocalPlayer.Character;
        private Rage.Object Ticket;
        private LHandle MainPursuit;

        private float VehicleHeading;
        private int MainScenario;

        private string Zone;

        private bool CalloutRunning = false;

        //DIALOGUE
        private readonly List<string> OpeningDialogue1 = new List<string>()
        {
         "~g~You:~w~ What's up dude, what seems to be the issue?",
         "~b~Officer:~w~ A fucking sovereign citizen. I swear I can't deal with these idiots, they piss me off so much!",
         "~b~Officer:~w~ Pulled him over for a speeding violation. He keeps talking nonsense and refuses to identify himself.",
         "~b~Officer:~w~ Maybe you can deal with his ass, I can't anymore or I'm gonna lose it.",
         "~g~You:~w~ I'll try my best, wish me luck.",
         "~b~Officer:~w~ Thanks, you're gonna need it.",
        };
        private readonly List<string> OpeningDialogue2 = new List<string>()
        {
         "~g~You:~w~ Hey, what's going on?",
         "~b~Officer:~w~ A damn sovereign citizen. Always dreaded the day I'd have to deal with an idiot like this.",
         "~b~Officer:~w~ I stopped him for failing to signal a turn. He keeps talking nonsense and refusing to cooperate with the stop.",
         "~b~Officer:~w~ I can't deal with this guy anymore, so maybe you can take a whirl at it.",
         "~g~You:~w~ I'll do my best, although I don't know how well it'll go.",
         "~b~Officer:~w~ Can't be worse than me, that's for sure. I'll back you up.",
        };
        private readonly List<string> CooperatesOpening1 = new List<string>()
        {
         "~g~You:~w~ Thank you. I'm here because you haven't been cooperative with my partner's investigation so far.",
         "~r~Suspect:~w~ I will not cooperate with an unlawful detainment officer. You should know that, after all you are the expert in the law here.",
         "~g~You:~w~ And what's so unlawful about any of this that we are doing here?",
         "~r~Suspect:~w~ I haven't committed any crime, you officers have no business interfering with my right to travel if I haven't done anything wrong.",
        };
        private readonly List<string> CooperatesOpening2 = new List<string>()
        {
         "~g~You:~w~ Thank you. I'm here because you haven't been cooperating with my partner.",
         "~r~Suspect:~w~ I refuse to cooperate with an unlawful detainment like this. You should know that, after all you are the expert in the law here.",
         "~g~You:~w~ And what's so unlawful about any of this that we are doing here?",
         "~r~Suspect:~w~ I haven't committed any crime, you officers have no business interfering with my right to travel if I haven't broken any law.",
        };
        private readonly List<string> CooperatesReason1 = new List<string>()
        {
         "~g~You:~w~ You committed a traffic violation, which gives us probable cause to pull you over. We haven't violated any of your rights.",
         "~r~Suspect:~w~ I know my rights! You officers have no power to harass me and ask for my id!",
         "~g~You:~w~ We actually do, you're operating a vehicle on a public roadway, you must surrender your license and proof of insurance to law enforcement.",
         "~r~Suspect:~w~ I'm not driving, I'm travelling you stupid cop! Stop violating my rights and wasting my time!",
         "~g~You:~w~ Listen, you're going to either cooperate, or things are going to get much worse. Your decision. Either identify yourself or go to jail.",
         "~r~Suspect:~w~ I want your name and badge number officer. And get your supervisor out here too while you're at it.",
        };
        private readonly List<string> CooperatesReason2 = new List<string>()
        {
         "~g~You:~w~ Okay, I'm going to do my best to explain this to you. You committed a traffic violation, which gives us probable cause to pull you over. Nobody has violated any rights here.",
         "~r~Suspect:~w~ Iou detained me without cause, violating my right to travel officer. Of course you violated my rights, and I don't have to give you my id either!",
         "~g~You:~w~ You actually do, you're operating a vehicle on a public roadway, you must surrender your license and proof of insurance to law enforcement.",
         "~r~Suspect:~w~ I'm not driving, I'm travelling you stupid cop! Stop violating my rights and wasting my time!",
         "~g~You:~w~ Listen, you're going to either cooperate, or things are go to jail. Your decision.",
         "~r~Suspect:~w~ I want your name and badge number officer. And get your supervisor out here too while you're at it. I want to file a formal complaining with your shitty department.",
        };
        private readonly List<string> Scold1 = new List<string>()
        {
         "~g~You:~w~ Okay listen. I'm not going to waste any more time with you, understand? You are required by law to identify yourself on a traffic stop, this is your final chance.",
         "~r~Suspect:~w~ I know my rights! You officers have no power to harass me and ask for my id!",
         "~g~You:~w~ You act like you know every single law in the book, but unfortunately there is no cure for stupidity no matter how many laws you memorize.",
         "~g~You:~w~ I'm done with you, you've wasted enough of my time and that officer's time. Stay right here you piece of shit.",
         "~r~Suspect:~w~ I want your name and badge number officer, that is no way to talk to a taxpayer! I pay your salary!",
        };
        private readonly List<string> Scold2 = new List<string>()
        {
         "~g~You:~w~ Okay listen. I'm not wasting any more time arguing with you, understand? You are required by law to identify yourself on a traffic stop, this is your final chance.",
         "~r~Suspect:~w~ I know my rights! You officers have no power to harass me and ask for my id! You are a disgrace!",
         "~g~You:~w~ We'll see who's a disgrace once I arrest your ignorant ass for obstruction.",
         "~g~You:~w~ Stay in your car while I sort this out.",
         "~r~Suspect:~w~ I want your name and badge number officer, that is no way to talk to a citizen! I pay your salary!",
        };
        private readonly List<string> Refuses1 = new List<string>()
        {
         "~g~You:~w~ Okay listen. I'm not going to waste any more time with you, understand? You are required by law to identify yourself on a traffic stop, this is your final chance.",
         "~r~Suspect:~w~ I know my rights! You officers have no power to harass me and ask for my id, let alone tell me what to do!",
         "~g~You:~w~ This is your final warning to comply with my instructions!",
         "~r~Suspect:~w~ I want your name and badge number officer, that is no way to treat to a citizen in a free country! I pay your salary!",
        };
        private readonly List<string> Refuses2 = new List<string>()
        {
         "~g~You:~w~ listen. I'm not wasting any more time arguing with you, understand? This is your final warning to cooperate with my instructions.",
         "~r~Suspect:~w~ I told you already, I will not cooperate with unlawful commands!",
         "~g~You:~w~ You're only making this worse for yourself.",
         "~r~Suspect:~w~ I want your name and badge number officer, I'll make things worse for you!",
        };

        public override bool OnBeforeCalloutDisplayed()
        {
            Game.LogTrivial("==========YOBBINCALLOUTS: Sovereign Citizen Callout Start==========");
            Zone = Functions.GetZoneAtPosition(Game.LocalPlayer.Character.Position).GameName;
            Game.LogTrivial("YOBBINCALLOUTS: Zone is " + Zone);

            Vector3 Spawn = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(550f));
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

            ShowCalloutAreaBlipBeforeAccepting(MainSpawnPoint, 25);    //Callout Blip Circle with radius 100m
            AddMinimumDistanceCheck(50f, MainSpawnPoint);   //Player must be 150m or further away
            Functions.PlayScannerAudio("WE_HAVE_01 CRIME_RESIST_ARREST_04"); //change later, resisting arrest
            CalloutMessage = "Sovereign Citizen";
            CalloutPosition = MainSpawnPoint;
            CalloutAdvisory = "A Sovereign Citizen is ~r~Refusing to Cooperate~w~ with Other Officers.";
            return base.OnBeforeCalloutDisplayed();
        }
        public override bool OnCalloutAccepted()
        {
            Game.LogTrivial("YOBBINCALLOUTS: Sovereign Citizen Callout Accepted by User");
            if (Main.CalloutInterface)
            {
                CalloutInterfaceHandler.SendCalloutDetails(this, "CODE 2", "");
            }
            else
            {
                Game.DisplayNotification("Respond ~b~Code 2~w~.");
            }

            SuspectVehicle = CallHandler.SpawnVehicle(MainSpawnPoint, VehicleHeading);
            Game.LogTrivial("YOBBINCALLOUTS: Suspect Vehicle Spawned");
            SuspectVehicle.IsPersistent = true;
            SuspectVehicle.IsEngineOn = true;
            SuspectVehicle.IsDriveable = false;

            Suspect = SuspectVehicle.CreateRandomDriver();
            Suspect.IsPersistent = true;
            Suspect.BlockPermanentEvents = true;
            Suspect.Tasks.CruiseWithVehicle(0f);

            OfficerVehicle = new Vehicle(Config.PoliceVehicle, SuspectVehicle.GetOffsetPositionFront(-7), SuspectVehicle.Heading);
            Game.LogTrivial("YOBBINCALLOUTS: Officer Vehicle Spawned");
            OfficerVehicle.IsPersistent = true;
            Officer = OfficerVehicle.CreateRandomDriver();
            Officer.Tasks.LeaveVehicle(OfficerVehicle, LeaveVehicleFlags.None);
            Officer.Tasks.FollowNavigationMeshToPosition(SuspectVehicle.GetBonePosition("wheel_lr"), SuspectVehicle.Heading + 90, 2, 1, 5000);
            Game.LogTrivial("YOBBINCALLOUTS: Officer Spawned");
            Officer.IsPersistent = true;
            Officer.BlockPermanentEvents = true;

            Area = new Blip(MainSpawnPoint, 25);
            Area.Alpha = 0.67f;
            Area.Color = Color.Yellow;
            Area.IsRouteEnabled = true;

            System.Random yuy = new System.Random();
            int ScenarioChooser = yuy.Next(0, 3);
            MainScenario = ScenarioChooser;

            if (CalloutRunning == false) { Callout(); }
            return base.OnCalloutAccepted();
        }
        public override void OnCalloutNotAccepted()
        {
            Game.LogTrivial("YOBBINCALLOUTS: Sovereign Citizen Callout Not Accepted by User.");
            Functions.PlayScannerAudio("OTHER_UNIT_TAKING_CALL_01");  //this gets annoying after a while
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
                        while (player.DistanceTo(MainSpawnPoint) >= 25 && !Game.IsKeyDown(Keys.End)) GameFiber.Wait(0);
                        if (Game.IsKeyDown(Config.CalloutEndKey)) { EndCalloutHandler.CalloutForcedEnd = true; break; }
                        if (MainScenario <= 1)
                        {
                            TalkToOfficer();
                            break;
                        }
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
        public override void End()
        {
            base.End();
            if (CalloutRunning)
            {
                Game.DisplayNotification("~g~Code 4~w~, return to patrol.");
                Functions.PlayScannerAudio("ATTENTION_ALL_UNITS WE_ARE_CODE_4");
            }
            CalloutRunning = false;
            if (Area.Exists()) { Area.Delete(); }
            if (OfficerBlip.Exists()) OfficerBlip.Delete();
            if (Suspect.Exists()) { Suspect.Dismiss(); }
            if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }
            if (Area.Exists()) { Area.Delete(); }
            if (Officer.Exists()) Officer.Dismiss();
            if (Ticket.Exists()) Ticket.Delete();
            Game.LogTrivial("YOBBINCALLOUTS: Sovereign Citizen Callout Finished Cleaning Up.");
        }
        public override void Process()
        {
            base.Process();
        }
        private void TalkToOfficer()
        {
            if (CalloutRunning)
            {
                if(Area.Exists()) Area.Delete();
                OfficerBlip = Officer.AttachBlip();
                OfficerBlip.Scale = 0.75f;
                OfficerBlip.IsFriendly = true;
                OfficerBlip.Name = "Officer";
                Game.DisplayHelp("Speak with the ~b~Officer.");
                NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(Officer, player, -1);
                while (player.DistanceTo(Officer) >= 5) GameFiber.Wait(0);
                if (Config.DisplayHelp)
                {
                    Game.DisplayHelp("Press ~y~" + Config.MainInteractionKey + " ~w~to Talk to the ~b~Officer.");
                }
                System.Random r = new System.Random();
                int OpeningDialogue = r.Next(0, 2);
                switch (OpeningDialogue)
                {
                    case 0:
                        CallHandler.Dialogue(OpeningDialogue1, Officer);
                        break;
                    case 1:
                        CallHandler.Dialogue(OpeningDialogue2, Officer);
                        break;
                }
                GameFiber.Wait(2000);
                Officer.Tasks.ClearImmediately();
                OfficerBlip.Delete();
                SuspectBlip = Suspect.AttachBlip();
                SuspectBlip.IsFriendly = false;
                SuspectBlip.Scale = 0.75f;
                Game.DisplayHelp("Talk to the ~r~Sovereign Citizen.");
                Question();
            }
        }
        private void Question()
        {
            if (CalloutRunning)
            {
                while (player.DistanceTo(Suspect) >= 3) GameFiber.Wait(0);
                Officer.Tasks.AchieveHeading(SuspectVehicle.Heading).WaitForCompletion(500);
                CallHandler.IdleAction(Officer, true);
                Game.DisplaySubtitle("~g~You:~w~ Driver, Roll Your Window Down Please.", 3000);
                GameFiber.Wait(3500);
                if (MainScenario == 0)   //rolls window down
                {
                    try { NativeFunction.Natives.ROLL_DOWN_WINDOW(SuspectVehicle, 0); }
                    catch { Game.LogTrivial("YOBBINCALLOUTS: Error Rolling Down Driver Window."); }
                    GameFiber.Wait(500);
                    if (Config.DisplayHelp) Game.DisplayHelp("Press ~y~" + Config.MainInteractionKey + " ~w~to Talk to the ~r~Suspect.");
                    System.Random r = new System.Random();
                    int OpeningDialogue = r.Next(0, 2);
                    switch (OpeningDialogue)
                    {
                        case 0:
                            CallHandler.Dialogue(CooperatesOpening1);
                            break;
                        case 1:
                            CallHandler.Dialogue(CooperatesOpening2);
                            break;
                    }
                    GameFiber.Wait(2000);
                    Game.DisplayHelp("~y~" + Config.Key1 + ":~b~ Reason with the Suspect.~y~ " + Config.Key2 + ":~b~ Scold the Suspect.");
                    while (!Game.IsKeyDown(Config.Key1) && !Game.IsKeyDown(Config.Key2)) GameFiber.Wait(0); //might do an animation?
                    if (Config.DisplayHelp) Game.DisplayHelp("Press ~y~" + Config.MainInteractionKey + " ~w~to Continue Talking to the ~r~Suspect.");
                    if (Game.IsKeyDown(Config.Key1))    //reason
                    {
                        System.Random r2 = new System.Random();
                        int CooperateDialogue = r2.Next(0, 2);
                        switch (CooperateDialogue)
                        {
                            case 0:
                                CallHandler.Dialogue(CooperatesReason1);
                                break;
                            case 1:
                                CallHandler.Dialogue(CooperatesReason2);
                                break;
                        }
                    }
                    else   //scold
                    {
                        System.Random r2 = new System.Random();
                        int ScoldDialogue = r2.Next(0, 2);
                        switch (ScoldDialogue)
                        {
                            case 0:
                                CallHandler.Dialogue(Scold1);
                                break;
                            case 1:
                                CallHandler.Dialogue(Scold2);
                                break;
                        }
                    }
                    GameFiber.Wait(2000);
                    //Suspect.Tasks.ClearImmediately();
                    Game.DisplayHelp("~y~" + Config.Key1 + ":~b~ Arrest the Suspect.~y~ " + Config.Key2 + ":~b~ Ticket the Suspect.");
                    while (!Game.IsKeyDown(Config.Key1) && !Game.IsKeyDown(Config.Key2)) GameFiber.Wait(0); //might do an animation?
                    if (Game.IsKeyDown(Config.Key1)) Detain();
                    else Ticketed();
                }
                else if(MainScenario == 1) //refuses to roll window down
                {
                    Game.DisplaySubtitle("~r~Suspect:~w~ I Will Not Follow Unlawful Commands Officer!", 3000);
                    GameFiber.Wait(3500);
                    if (Config.DisplayHelp) Game.DisplayHelp("Press ~y~" + Config.MainInteractionKey + " ~w~to Reason With the ~r~Suspect.");
                    System.Random r2 = new System.Random();
                    int ReasonDialogue = r2.Next(0, 2);
                    switch (ReasonDialogue)
                    {
                        case 0:
                            CallHandler.Dialogue(Refuses1);
                            break;
                        case 1:
                            CallHandler.Dialogue(Refuses2);
                            break;
                    }
                    GameFiber.Wait(2000);
                    //Suspect.Tasks.ClearImmediately();
                    Game.DisplayHelp("~y~" + Config.Key1 + ":~b~ Arrest the Suspect.~y~ " + Config.Key2 + ":~b~ Ticket the Suspect.");
                    while (!Game.IsKeyDown(Config.Key1) && !Game.IsKeyDown(Config.Key2)) GameFiber.Wait(0); //might do an animation?
                    if (Game.IsKeyDown(Config.Key1)) Detain();
                    else Ticketed();
                }
                else //Pursuit (test this later)
                {
                    Game.LogTrivial("YOBBINCALLOUTS: Pursuit scenario activated.");
                    Game.DisplaySubtitle("~r~Suspect:~w~ I Will Not Follow Unlawful Commands Officer!", 3000);
                    GameFiber.Wait(CallHandler.RNG(0, 3000, 8000));

                    if(Suspect.Exists() && !Functions.IsPedArrested(Suspect))
                    {
                        SuspectVehicle.IsDriveable = true;
                        CallHandler.CreatePursuit(MainPursuit, true, false, Suspect);
                        CallHandler.SuspectWait(Suspect);
                    }
                }
            }
        }
        private void Ticketed()
        {
            Game.DisplayHelp("Go back to Your ~g~Vehicle~w~ to Write the Suspect a ~r~Ticket.");
            if (player.LastVehicle.Exists())
            {
                Area = player.LastVehicle.AttachBlip();
                Area.Color = Color.Green;
                while (!player.IsInAnyVehicle(false)) GameFiber.Wait(0);
            }
            else
            {
                Game.DisplayHelp("Press ~y~" + Config.MainInteractionKey + " ~w~to Write the Suspect a ~r~Ticket.");
                while (!Game.IsKeyDown(Config.MainInteractionKey)) GameFiber.Wait(0);
            }
            GameFiber.Wait(2500);
            Ticket = new Rage.Object("prop_cd_paper_pile1", Vector3.Zero);
            Ticket.IsPersistent = true;
            Ticket.AttachTo(player, player.GetBoneIndex(PedBoneId.RightHand), new Vector3(0.1490f, 0.0560f, -0.0100f), new Rotator(-17f, -142f, -151f)); //might have to change to other hand anim?
            Game.DisplayHelp("Go to the ~r~Suspect~w~ to Hand Them the Ticket.");
            if (Area.Exists()) Area.Delete();
            while (player.DistanceTo(Suspect) >= 4f) GameFiber.Wait(0);
            Game.DisplayHelp("Press ~y~" + Config.MainInteractionKey + " ~w~to Give the ~r~Suspect~w~ the Ticket.");
            while (!Game.IsKeyDown(Config.MainInteractionKey)) GameFiber.Wait(0);

            player.Tasks.PlayAnimation("mp_common", "givetake1_b", 1f, AnimationFlags.Loop);
            GameFiber.Wait(1000);
            Ticket.Delete();
            GameFiber.Wait(1000);
            Game.DisplaySubtitle("~g~You:~w~ Here is your Ticket Sir. You Will be Expected to Appear in Court on the Date Specified.");
            player.Tasks.Clear();
            GameFiber.Wait(3500);
            Suspect.Dismiss();
        }
        private void Detain()
        {
            Game.DisplayHelp("Arrest the ~r~Suspect.");
            while (Suspect.Exists())
            {
                GameFiber.Yield();
                if (Suspect.IsDead || Functions.IsPedArrested(Suspect) || !Suspect.Exists()) break;
            }
            if (Suspect.Exists())
            {
                if (Functions.IsPedArrested(Suspect)) { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect is Under ~g~Arrest~w~ for Obstruction."); }
                else { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect is ~r~Dead."); }
            }
            else
            {
                GameFiber.Wait(5000); Game.DisplayNotification("Dispatch, We are ~g~Code 4~w~.");
            }
            GameFiber.Wait(2000);
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("REPORT_RESPONSE_COPY_02");
            GameFiber.Wait(2000);
        }
    }
}