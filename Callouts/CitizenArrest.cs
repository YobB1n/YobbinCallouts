//This callout is a WIP**
//TO-DO - double-check all scenarios and peaceful suspect arrest task (revert if doesn't work)

using System.Drawing;
using Rage;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage.Native;
using System.Collections.Generic;
using System;

namespace YobbinCallouts.Callouts
{
    [CalloutInfo("[YC] Citizen Arrest", CalloutProbability.High)]
    public class CitizenArrest : Callout
    {
        private Vector3 MainSpawnPoint;

        private Ped Suspect;
        private Ped Citizen;
        private Ped Victim;
        Ped player = Game.LocalPlayer.Character;

        private Blip SuspectBlip;
        private Blip CitizenBlip;
        private Blip VictimBlip;
        private Blip AreaBlip;

        private int MainScenario;
        private string Zone;
        private string Crime;
        private string[] SuspectModels; private string[] CitizenModels; private string[] VictimModels;
        private bool CalloutRunning;
        private int theftspeech = 2;

        private LHandle SuspectPursuit;

        //DIALOGUE VVvvVV DIALOGUE
        private readonly List<string> AssaultOpening1 = new List<string>()
        {
         "~p~Citizen:~w~ Hey Officer, I just performed a Citizen's Arrest on this guy right here.",
         "~g~You:~w~ Alright, Can You Tell Me What Happened?",
         "~p~Citizen:~w~ Yeah, I was Just Walking Down the Street When I Noticed These Two Getting into an Argument.",
         "~p~Citizen:~w~ Just as I was Walking Over There, He Started Punching Them. I Seperated the Two and Arrested This Guy.",
         "~g~You:~w~ Alright, let me finish up here and you should be good to go.",
        };
        private readonly List<string> AssaultOpening2 = new List<string>()
        {
         "~p~Citizen:~w~ Hello Officer, I just performed a Citizen's Arrest on this person right here!",
         "~g~You:~w~ I see that. What happened?",
         "~p~Citizen:~w~ I was going for a walk when I noticed these two arguing about something.",
         "~p~Citizen:~w~ I wanted to break it up, but before I could the person I arrested started punching them.",
         "~g~You:~w~ Gotcha. Anything else I should know?",
         "~p~Citizen:~w~ I used to be a security guard, so it was a pretty straightforward detainment.",
         "~g~You:~w~ Alright, thanks for the info.",
        };
        private readonly List<string> TheftOpening1 = new List<string>() //wallet
        {
         "~p~Citizen:~w~ Hello Officer, I just performed a Citizen's Arrest on this guy right here.",
         "~g~You:~w~ Can you tell me what happened?",
         "~p~Citizen:~w~ I was just walking along when I saw this guy running down the street.",
         "~p~Citizen:~w~ The victim was yelling that he stole their wallet so I stopped and arrested them.",
         "~g~You:~w~ Alright, let me go speak with the victim.",
        };
        private readonly List<string> TheftOpening2 = new List<string>() //bag
        {
         "~p~Citizen:~w~ Hey Officer, I just did a Citizen's Arrest on this guy right here.",
         "~g~You:~w~ I see that. What happened?",
         "~p~Citizen:~w~ This guy came running around the corner with a person chasing them saying he stole their bag.",
         "~p~Citizen:~w~ I caught up to the perp and took them down, tying his hands around his back.",
         "~g~You:~w~ Gotcha, let me quickly talk to the victim and I'll get you on your way. Thanks for the help.",
        };
        private readonly List<string> GunPointOpening1 = new List<string>() //has CCW
        {
         "~p~Citizen:~w~ Thanks for the help Officer, that was definitely one sticky situation.",
         "~g~You:~w~ I'll say. What happened?",
         "~p~Citizen:~w~ This dude was on the street while I was walking by open carrying that firearm!",
         "~p~Citizen:~w~ I wasn't gonna do anything until he started pointing it at people and vehicles.",
         "~g~You:~w~ I see. Do you have a CCW?",
         "~p~Citizen:~w~ I do Never thought I'd actually have to use it, glad I carry!",
         "~g~You:~w~ Alright. Let me check in with the victim, and then you'll be free to go. Great work here.",
        };
        private readonly List<string> GunPointOpening2 = new List<string>() //iscop
        {
         "~p~Citizen:~w~ Thanks for the help Officer, that was really scary!",
         "~g~You:~w~ I'll say. What happened?",
         "~p~Citizen:~w~ This dude got out of his car and started walking down the street open-carrying that gun!",
         "~p~Citizen:~w~ I kept an eye on them and held them at gunpoint when I saw them pointing the gun at cars and pedestrians.",
         "~g~You:~w~ I see. Do you have a CCW?",
         "~p~Citizen:~w~I do, and I'm also an off-duty Officer which is why I carry cuffs.",
         "~g~You:~w~ Excellent. Let me check in with the victim, and then you'll be free to go. Great work here.",
        };
        private readonly List<string> AssaultInvestigation1 = new List<string>()
        {
         "~g~You:~w~ Hello, can you tell me what happened here?",
         "~b~Victim:~w~ This crazy dude bumped into me while I was walking down the street!",
         "~b~Victim:~w~ I told them to watch it, and he immediately took offence and started shoving me.",
         "~b~Victim:~w~ I tried to walk away to diffuse the situation, and then he started punching me.",
         "~b~Victim:~w~ Then this person stepped in and tackled the guy to the ground and tied them up before you guys hot here.",
         "~g~You:~w~ Do you need medical attention?",
         "~b~Victim:~w~ I'm fine, just glad this guy got subdued.",
         "~g~You:~w~ Alright, I'll process them then. You're free to go if you don't need anything else.",
        };
        private readonly List<string> AssaultInvestigation2 = new List<string>()
        {
         "~g~You:~w~ Hey, are you alright? Do you need an ambulance?",
         "~b~Victim:~w~ Hi Officer, I'm fine thanks, glad I didn't get badly injured!",
         "~g~You:~w~ That makes two of us. What happened?",
         "~b~Victim:~w~ This guy was walking down the street, agitated and possibly drunk or high.",
         "~b~Victim:~w~ He kept muttering random words, and all of a sudden started punching me unprovoked.",
         "~g~You:~w~ So there was no reason for the attack?",
         "~b~Victim:~w~ No Officer, I had no idea what was heppning. Thankfully this person was able to help.",
         "~g~You:~w~ Alright, I'll process them in then. You're free to go if you don't need anything else.",
        };
        private readonly List<string> TheftInvestigation1 = new List<string>() //wallet
        {
         "~b~Victim:~w~ Hey Officer, I hope you're gonna arrest that guy over there!",
         "~g~You:~w~ It's looking like it. What happened?",
         "~b~Victim:~w~ This guy was walking down the street, all of a sudden he reached into my pocket, grabbed my wallet and ran off!.",
         "~b~Victim:~w~ Thankfully this kind person saw it happen and chased them, taking them down and getting my wallet back.",
         "~g~You:~w~ I see. Would you like an ambulance? Are you hurt?",
         "~b~Victim:~w~ I'm okay Officer, thanks.",
         "~g~You:~w~ Alright, I'll process them in then. You're free to go if you don't need anything else.",
        };
        private readonly List<string> TheftInvestigation2 = new List<string>() //bag
        {
         "~g~You:~w~ Hey, are you okay? What happened?",
         "~b~Victim:~w~ I was just walking minding my own business, when this guy comes up to me and steals my bag!!",
         "~b~Victim:~w~ I tired to wrestle it away from them but he was too strong!",
         "~b~Victim:~w~ Then this person came and tackled them, putting his hands behind his back and getting my bag back.",
         "~g~You:~w~ Gotcha. Do you need anything else? An ambulance?",
         "~b~Victim:~w~ I'm okay Officer, thanks.",
         "~g~You:~w~ Alright, I'll take them to jail then. You're free to go! Glad you're alright.",
        };

        public override bool OnBeforeCalloutDisplayed()
        {
            Game.LogTrivial("==========YOBBINCALLOUTS: Citizen Arrest Callout Start==========");
            int Scenario = CallHandler.RNG(0, 2); //scenario [2] not done
            MainScenario = Scenario;
            Game.LogTrivial("YOBBINCALLOUTS: Scenario is " + MainScenario + "");
            if (MainScenario == 0) Crime = "Assault.";  //PUT PERIODS AT THE END OF THESE CRIMES.
            else if (MainScenario == 1) Crime = "Theft.";
            else Crime = "Discharging a Firearm.";

            Zone = Functions.GetZoneAtPosition(Game.LocalPlayer.Character.Position).RealAreaName;
            Game.LogTrivial("YOBBINCALLOUTS: Zone is " + Zone);
            MainSpawnPoint = World.GetNextPositionOnStreet(player.Position.Around(569f));
            ShowCalloutAreaBlipBeforeAccepting(MainSpawnPoint, 75f);    //Callout Blip Circle with radius 50m
            AddMinimumDistanceCheck(25f, MainSpawnPoint);   //Player must be 25m or further away
            Functions.PlayScannerAudio("CITIZENS_REPORT CRIME_DISTURBING_THE_PEACE_01");    //Change
            CalloutMessage = "Citizen's Arrest";
            CalloutPosition = MainSpawnPoint;
            CalloutAdvisory = "Caller is a Citizen Who Has Arrested a Suspect for " + Crime;
            return base.OnBeforeCalloutDisplayed();
        }
        public override bool OnCalloutAccepted()
        {
            try
            {
                Game.LogTrivial("YOBBINCALLOUTS: Citizen Arrest Callout Accepted by User.");
                if (Main.CalloutInterface)
                {
                    CalloutInterfaceHandler.SendCalloutDetails(this, "CODE 03", "");
                }
                else
                {
                    Game.DisplayNotification("Respond ~r~Code 3.");
                }

                //Spawning all this shit
                NativeFunction.Natives.GetClosestVehicleNodeWithHeading(MainSpawnPoint, out Vector3 nodePosition, out float heading, 1, 3.0f, 0);
                NativeFunction.Natives.xA0F8A7517A273C05(nodePosition, 0, out Vector3 SuspectSpawn);
                SuspectModels = new string[8] { "A_M_Y_SouCent_01", "A_M_Y_StWhi_01", "A_M_Y_StBla_01", "A_M_Y_Downtown_01", "A_M_Y_BevHills_01", "G_M_Y_MexGang_01", "G_M_Y_MexGoon_01", "G_M_Y_StrPunk_01" };
                int SuspectModel = CallHandler.RNG(0, SuspectModels.Length);

                Suspect = new Ped(CallHandler.SpawnOnSreetSide(SuspectSpawn), heading); //test new spawning -> good.
                Suspect.IsPersistent = true;
                Suspect.BlockPermanentEvents = true;
                //Functions.SetPedAsArrested(Suspect, true, false);
                //Suspect.Tasks.StandStill(-1);
                Game.LogTrivial("YOBBINCALLOUTS: Suspect Spawned");

                Citizen = new Citizen(CallHandler.SpawnOnSreetSide(Suspect.GetOffsetPositionFront(2)));
                Citizen.IsPersistent = true;
                Citizen.BlockPermanentEvents = true;
                Citizen.Heading = Suspect.Heading - 180f;

                var victimspawnpoint = World.GetNextPositionOnStreet(Suspect.Position.Around(10f));
                NativeFunction.Natives.xA0F8A7517A273C05<bool>(victimspawnpoint, 0, out Vector3 outPosition);

                Victim = new Ped(CallHandler.SpawnOnSreetSide(outPosition).Around2D(2f));
                Victim.IsPersistent = true;
                Victim.BlockPermanentEvents = true;
                Victim.Tasks.Cower(-1);

                AreaBlip = new Blip(Suspect.Position, 25);
                if (MainScenario <= 1) AreaBlip.Color = Color.Yellow;
                else AreaBlip.Color = Color.Red; //red for gunpoint scenario
                AreaBlip.Alpha = 0.67f;
                AreaBlip.IsRouteEnabled = true;
                AreaBlip.Name = "Callout Location";

                //Relationship Groups (double check)
                Suspect.RelationshipGroup.SetRelationshipWith(Citizen.RelationshipGroup, Relationship.Hate);
                Suspect.RelationshipGroup.SetRelationshipWith(player.RelationshipGroup, Relationship.Hate);
                Citizen.RelationshipGroup.SetRelationshipWith(player.RelationshipGroup, Relationship.Companion);

                if (MainScenario == 2) //gunpoint scenario
                {
                    String[] weapons = {"WEAPON_PISTOL", "WEAPON_SMG", "WEAPON_APPISTOL", "WEAPON_MICROSMG", "WEAPON_HEAVYPISTOL" };
                    string SuspectWeaponModel = weapons[CallHandler.RNG(0, weapons.Length)];    //Use Random Weapon generator
                    Game.LogTrivial("YOBBINCALLOUTS: Suspect Weapon Model is " + SuspectWeaponModel.Substring(7));
                    string CtizenWeaponModel = weapons[CallHandler.RNG(0, weapons.Length)];    //Use Random Weapon generator
                    Game.LogTrivial("YOBBINCALLOUTS: Citizen Weapon Model is " + CtizenWeaponModel.Substring(7));

                    //I hate how the arguments for these two functions are reversed lmao
                    Suspect.Tasks.PutHandsUp(-1, Citizen);
                    Citizen.Tasks.AimWeaponAt(Suspect, -1);
                }

                //Shouldn't need this anymore but keeping it just in case.
                if (!Suspect.Exists())
                {
                    Game.LogTrivial("YOBBINCALLOUTS: Suspect no longer valid, aborting...");
                    return false;
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
            if (!CalloutRunning) Callout();
            return base.OnCalloutAccepted();
        }
        public override void OnCalloutNotAccepted()
        {
            Game.LogTrivial("YOBBINCALLOUTS: Citizen Arrest Callout Not Accepted by User.");
            //Functions.PlayScannerAudio("OTHER_UNIT_TAKING_CALL_01");  //this gets annoying after a while
            base.OnCalloutNotAccepted();
        }
        private void Callout()
        {
            CalloutRunning = true;
            GameFiber.StartNew(delegate
            {
                while (CalloutRunning)
                {
                    while (player.DistanceTo(Citizen) >= 25 && !Game.IsKeyDown(Config.CalloutEndKey)) GameFiber.Wait(0);
                    if (Game.IsKeyDown(Config.CalloutEndKey)) { EndCalloutHandler.CalloutForcedEnd = true; break; }
                    Game.LogTrivial("YOBBINCALLOUTS: Player Arrived on Scene.");
                    AreaBlip.Delete();
                    if (MainScenario <= 1) Game.DisplayHelp("Speak With the ~p~Arresting Citizen.");
                    else Game.DisplaySubtitle("~p~Citizen:~w~ Officer! Arrest him please! He was pointing a gun at people!");
                    CitizenBlip = Citizen.AttachBlip();
                    CitizenBlip.Scale = 0.7f;
                    CitizenBlip.Color = Color.Purple;
                    if(MainScenario <= 1) NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(Citizen, player, -1);
                    SuspectBlip = CallHandler.AssignBlip(Suspect, Color.Red, 0.7f);

                    if (MainScenario == 0 || MainScenario == 1) Peaceful();
                    else GunPoint(); //MainScenario 2
                    break;
                }
                Game.LogTrivial("YOBBINCALLOUTS: Callout Finished, Ending...");
                EndCalloutHandler.EndCallout();
                End();
            }
            );
        }
        public override void End()
        {
            base.End();
            CalloutRunning = false;
            Game.DisplayNotification("~g~Code 4~w~, return to patrol.");
            if (Victim.Exists()) { Victim.Dismiss(); }
            //if (Suspect.Exists()) { Suspect.Dismiss(); }
            if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }
            if (VictimBlip.Exists()) { VictimBlip.Delete(); }
            if (CitizenBlip.Exists()) { CitizenBlip.Delete(); }
            if (Citizen.Exists()) Citizen.Delete(); 
            if (AreaBlip.Exists()) { AreaBlip.Delete(); }
            Functions.PlayScannerAudio("ATTENTION_ALL_UNITS WE_ARE_CODE_4");
            Game.LogTrivial("YOBBINCALLOUTS: Citizen Arrest Callout Finished Cleaning Up.");
        }
        public override void Process()
        {
            base.Process();
        }
        private void GunPoint()
        {
            //test if this works...
            Suspect.Tasks.PutHandsUp(-1, Citizen);
            Citizen.Tasks.AimWeaponAt(Suspect, -1);

            if (Victim.Exists()) Victim.Tasks.ReactAndFlee(Suspect);
            GameFiber.Wait(2000);
            if (Config.DisplayHelp) Game.DisplayHelp("Arrest the ~r~Suspect.");

            if(Suspect.Exists() && Suspect.IsAlive)
            {
                if (CallHandler.FiftyFifty()) //arrest
                {
                    Game.LogTrivial("YOBBINCALLOUTS: ARREST");
                    CallHandler.SuspectWait(Suspect);
                }
                else //late shoot
                {
                    Game.LogTrivial("YOBBINCALLOUTS: LATE SHOOT");
                    GameFiber.Wait(CallHandler.RNG(0, 2500, 6900));
                    if (Suspect.Exists() && Suspect.IsAlive) Suspect.Tasks.FightAgainstClosestHatedTarget(-1);
                    CallHandler.SuspectWait(Suspect);
                }
            }
            else Game.LogTrivial("YOBBINCALLOUTS: SUSPECT KILLED EARLY");
            //
            GameFiber.Wait(4000);
            Game.DisplayHelp("Secure the Scene, then Speak with the ~p~Citizen.");
            while (player.DistanceTo(Citizen) >= 2.5f) GameFiber.Wait(0);
            if (Config.DisplayHelp) Game.DisplayHelp("Press ~y~" + Config.MainInteractionKey + " ~w~to Speak with the ~p~Arresting Citizen.");
            if (CallHandler.FiftyFifty())
            {
                //try to figure out how to set CCW
                Game.LogTrivial("YOBBINCALLOUTS: CCW");
                CallHandler.Dialogue(GunPointOpening1, Citizen);
            }
            else
            {
                Functions.SetPedAsCop(Citizen);
                Game.LogTrivial("YOBBINCALLOUTS: COP");
                CallHandler.Dialogue(GunPointOpening2, Citizen);
            }
            GameFiber.Wait(2000);
            if (Citizen.Exists()) Citizen.Dismiss();
            Game.DisplayHelp("Deal With the ~r~Suspect. ~w~Press ~y~" + Config.CalloutEndKey + " ~w~When Finished.");
            while (!Game.IsKeyDown(Config.CalloutEndKey)) GameFiber.Wait(0);
        }
        private void Peaceful()
        {
            //Functions.SetPedAsArrested(Suspect, true, false); //I know this works but suspect follows player around annoyingly
            Functions.SetPedCuffedTask(Suspect, true); //test this workaround for above
            Suspect.Tasks.StandStill(-1); //test this also
            CallHandler.IdleAction(Citizen, true);
            while (Game.LocalPlayer.Character.DistanceTo(Citizen) >= 5) GameFiber.Wait(0);
            if (Config.DisplayHelp) Game.DisplayHelp("Press ~y~" + Config.MainInteractionKey + " ~w~to Speak with the ~p~Arresting Citizen.");

            if (MainScenario == 0)
            {
                if(CallHandler.FiftyFifty()) CallHandler.Dialogue(AssaultOpening1, Citizen);
                else CallHandler.Dialogue(AssaultOpening2, Citizen);
            }
            else
            {
                if (CallHandler.FiftyFifty()) { theftspeech = 1; CallHandler.Dialogue(TheftOpening1, Citizen); }
                else CallHandler.Dialogue(TheftOpening2, Citizen);;
            }
            Citizen.Tasks.Clear();   //dismiss or nah?
            CallHandler.IdleAction(Citizen, true);

            VictimBlip = Victim.AttachBlip();
            VictimBlip.Scale = 0.69f;
            VictimBlip.IsFriendly = true;

            Game.DisplayHelp("Speak With the ~b~Victim.");
            NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(Victim, player, -1);

            while (player.DistanceTo(Victim) >= 5) GameFiber.Wait(0);
            if (Config.DisplayHelp) Game.DisplayHelp("Press ~y~" + Config.MainInteractionKey + " ~w~to Speak with the ~b~Victim.");
            if (MainScenario == 0) //assault
            {
                if (CallHandler.FiftyFifty()) CallHandler.Dialogue(AssaultInvestigation1, Victim);
                else CallHandler.Dialogue(AssaultInvestigation2, Victim);
            }
            else //theft
            {
                if (theftspeech == 1) CallHandler.Dialogue(TheftInvestigation1, Victim);
                else CallHandler.Dialogue(TheftInvestigation2, Victim);
            }

            Victim.Tasks.ClearImmediately();
            GameFiber.Wait(2000); //wait before dismissing victim and arresting citizen
            Victim.Dismiss(); if(VictimBlip.Exists()) VictimBlip.Delete();
            Citizen.Dismiss(); if (CitizenBlip.Exists()) CitizenBlip.Delete();

            GameFiber.Wait(2000);
            Game.DisplayHelp("Deal With the ~r~Suspect. ~w~Press ~y~"+Config.CalloutEndKey+" ~w~When Finished.");
            if(!Main.STP) Functions.SetPedAsArrested(Suspect, true, true);
            while (!Game.IsKeyDown(Config.CalloutEndKey)) GameFiber.Wait(0);
            if (Suspect.Exists()) if (Suspect.IsAlive) Game.DisplayNotification("Dispatch, We Have ~b~Arrested~w~ the Suspect.");
            if (SuspectBlip.Exists()) SuspectBlip.Delete();
        }
    }
}
