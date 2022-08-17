//This callout is a WIP**

using System.Drawing;
using Rage;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage.Native;
using System.Collections.Generic;
using System;

namespace YobbinCallouts.Callouts
{
    [CalloutInfo("Citizen Arrest", CalloutProbability.High)]
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

        private LHandle SuspectPursuit;

        //Add more dialogue for each scenario(s)
        private readonly List<string> AssaultOpening = new List<string>()
        {
         "~p~Citizen:~w~ Hey Officer, I Just Performed a Citizen's Arrest on This Guy Right Here.",
         "~g~You:~w~ Alright, Can You Tell Me What Happened?",
         "~p~Citizen:~w~ Yeah, I was Just Walking Down the Street When I Noticed These Two Getting into an Argument.",
         "~p~Citizen:~w~ Just as I was Walking Over There, He Started Punching Them. I Seperated the Two and Arrested This Guy.",
         "~g~You:~w~ Alright, Let Me Go Speak With the Victim.",
        };
        private readonly List<string> TheftOpening = new List<string>()
        {
         "~p~Citizen:~w~ Hello Officer, I Just Performed a Citizen's Arrest on This Guy Right Here.",
         "~g~You:~w~ Can You Tell Me What Happened?",
         "~p~Citizen:~w~ I was Just Walking Along When I Saw This Guy Running Down the Street.",
         "~p~Citizen:~w~ The Victim Was Yelling That He Stole Thir Wallet so I Stopped and Arrested Him.",
         "~g~You:~w~ Alright, Let Me Go Speak With the Victim.",
        };
        private readonly List<string> AssaultInvestigation = new List<string>()
        {
         "~g~You:~w~ Hello, Can You Tell Me What Happened Here?",
         "~b~Victim:~w~ This Crazy Guy Bumped into me While We Were Waling Down the Street.",
         "~b~Victim:~w~ I Told Him to Watch it, and He Immediately Took Offence and Started Shoving Me.",
         "~b~Victim:~w~ I Tried to Walk Away to Diffuse the Situation, and then He Started Punching Me.",
         "~b~Victim:~w~ Then This Person Stepped in and Tackled the Guy to the Ground and Tied Him Up Before You Guys Got Here.",
         "~g~You:~w~ Do You Need Medical Attention?",
         "~b~Victim:~w~ I'm Fine, Just Glad This Guy Got Subdued.",
         "~g~You:~w~ Alright, I'll Process Him Then. You're Free to Go if You Don't Need Anything Else.",
        };
        private readonly List<string> TheftInvestigation = new List<string>()
        {
         "~p~Citizen:~w~ Hello Officer, I Just Performed a Citizen's Arrest on This Guy Right Here.",
         "~g~You:~w~ Can You Tell Me What Happened?",
         "~p~Citizen:~w~ I was Just Walking Along When I Saw This Guy Running Down the Street.",
         "~p~Citizen:~w~ The Victim Was Yelling That He Stole Thir Wallet so I Stopped and Arrested Him.",
         "~g~You:~w~ Alright, Let Me Go Speak With the Victim.",
        };

        public override bool OnBeforeCalloutDisplayed()
        {
            Game.LogTrivial("==========YOBBINCALLOUTS: Citizen Arrest Callout Start==========");
            System.Random r = new System.Random();
            int Scenario = r.Next(0, 3); //change later
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
                SuspectModels = new string[8] { "A_M_Y_SouCent_01", "A_M_Y_StWhi_01", "A_M_Y_StBla_01", "A_M_Y_Downtown_01", "A_M_Y_BevHills_01", "G_M_Y_MexGang_01", "G_M_Y_MexGoon_01", "G_M_Y_StrPunk_01" };
                System.Random r2 = new System.Random();
                int SuspectModel = r2.Next(0, SuspectModels.Length);
                Suspect = new Ped(SuspectModels[SuspectModel], nodePosition, heading);
                Suspect.IsPersistent = true;
                Suspect.BlockPermanentEvents = true;
                Functions.SetPedAsArrested(Suspect, true, false);
                Suspect.Tasks.StandStill(-1);
                Game.LogTrivial("YOBBINCALLOUTS: Suspect Spawned");

                Citizen = new Ped(Suspect.GetOffsetPositionFront(2));
                Citizen.IsPersistent = true;
                Citizen.BlockPermanentEvents = true;
                Citizen.Heading = Suspect.Heading - 180f;

                var victimspawnpoint = World.GetNextPositionOnStreet(Suspect.Position.Around(10f));
                NativeFunction.Natives.xA0F8A7517A273C05<bool>(victimspawnpoint, 0, out Vector3 outPosition);

                Victim = new Ped(outPosition);
                Victim.IsPersistent = true;
                Victim.BlockPermanentEvents = true;
                Victim.Tasks.Cower(-1);

                AreaBlip = new Blip(Suspect.Position, 25);
                AreaBlip.Color = Color.Yellow;
                AreaBlip.Alpha = 0.67f;
                AreaBlip.IsRouteEnabled = true;
                AreaBlip.Name = "Callout Location";
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
                    Game.DisplayHelp("Speak With the ~p~Arresting Citizen.");
                    CitizenBlip = Citizen.AttachBlip();
                    CitizenBlip.Scale = 0.7f;
                    CitizenBlip.Color = Color.Purple;
                    NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(Citizen, player, -1);

                    SuspectBlip = Suspect.AttachBlip();
                    SuspectBlip.Scale = 0.7f;
                    SuspectBlip.IsFriendly = false;

                    if (MainScenario == 0) Assault();
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
            if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }
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

        }
        private void Assault()
        {
            CallHandler.IdleAction(Citizen, true);
            while (player.DistanceTo(Citizen) >= 5) GameFiber.Wait(0);
            if (Config.DisplayHelp) Game.DisplayHelp("Press ~y~" + Config.MainInteractionKey + " ~w~to Speak with the ~p~Arresting Citizen.");

            CallHandler.Dialogue(AssaultOpening, Citizen);
            Citizen.Tasks.Clear();   //dismiss or nah?
            CallHandler.IdleAction(Citizen, true);

            VictimBlip = Victim.AttachBlip();
            VictimBlip.Scale = 0.69f;
            VictimBlip.IsFriendly = true;

            Game.DisplayHelp("Speak With the ~b~Victim.");
            NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(Victim, player, -1);

            while (player.DistanceTo(Victim) >= 5) GameFiber.Wait(0);
            if (Config.DisplayHelp) Game.DisplayHelp("Press ~y~" + Config.MainInteractionKey + " ~w~to Speak with the ~b~Victim.");
            CallHandler.Dialogue(AssaultInvestigation, Victim);

            Victim.Dismiss(); if(VictimBlip.Exists()) VictimBlip.Delete();
            Citizen.Dismiss(); if (CitizenBlip.Exists()) CitizenBlip.Delete();

            GameFiber.Wait(2000);
            Game.DisplayHelp("Deal With the ~r~Suspect. ~w~Press ~y~"+Config.CalloutEndKey+" ~w~When Finished.");
            Functions.SetPedAsArrested(Suspect, true, true);
            while (!Game.IsKeyDown(Config.CalloutEndKey)) GameFiber.Wait(0);
            if (Suspect.Exists()) if (Suspect.IsAlive) Game.DisplayNotification("Dispatch, We Have ~b~Arrested~w~ the Suspect.");
        }
    }
}