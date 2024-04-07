
using System.Drawing;
using Rage;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage.Native;
using System.Collections.Generic;
using System;

namespace YobbinCallouts.Callouts
{
    [CalloutInfo("Human Trafficking", CalloutProbability.Medium)]
    public class HumanTrafficking : Callout
    {
        private Vector3 MainSpawnPoint;
        private Vector3 SuspectSpawnPoint;

        private Ped Suspect;
        private Ped Victim;
        private Ped Witness;
        private Vehicle SuspectVehicle;
        private LHandle MainPursuit;

        private Blip SuspectBlip;
        private Blip SuspectArea;
        private Blip VictimBlip;
        private Blip AreaBlip;
        private Blip WitnessBlip;

        private int SeatIndex;
        private int MainScenario;
        private string Zone;
        private string[] SuspectModels;
        private string[] VictimModels;
        private bool CalloutRunning;

        Ped player = Game.LocalPlayer.Character;

        //All the dialogue for the callout. Haven't found a better way to store it yet, so this will have to do.
        private readonly List<string> WitnessOpening1 = new List<string>()
        {
         "~g~You:~w~ Hello, are you the caller?",
         "~p~Store owner:~w~ Yes I am, officer. I noticed that woman over there pull up with a man in a white van.",
         "~p~Store owner:~w~ She nervously entered the store to use the restroom.",
         "~p~Store owner:~w~ She clearly was in need of assistance, as she had bruises and scrapes all over her.",
         "~g~You:~w~ I see. Is this the first time something like this has happened here?",
         "~p~Store owner:~w~ Unfortunately no, this store is a favorite for traffickers to stop and pick up supplies and gas.",
         "~p~Store owner:~w~ I'm always on the lookout for people that might need help.",
         "~g~You:~w~ Where's the suspect now?",
         "~p~Store owner:~w~ I confronted him, but he ran out of the store and drove off. I managed to get a plate, however.",
         "~g~You:~w~ Thanks so much for your help. I'll speak with the victim now.",
        };
        private readonly List<string> WitnessOpening2 = new List<string>()
        {
         "~g~You:~w~ Hello, are you the caller?",
         "~p~Store owner:~w~ Yes I am, officer. I noticed that woman over there pull up with a person in a white van.",
         "~p~Store owner:~w~ She nervously entered the store to use the restroom.",
         "~p~Store owner:~w~ She clearly was in need of assistance, as she had bruises and scrapes all over her.",
         "~g~You:~w~ I see. Is this the first time something like this has happened here?",
         "~p~Store owner:~w~ Unfortunately no, this store is a favorite for traffickers to stop and grab supplies and gas.",
         "~p~Store owner:~w~ I'm always on the lookout for people that might need help.",
         "~g~You:~w~ Where's the suspect now?",
         "~p~Store owner:~w~ I confronted him, but he ran out of the store and drove off. I managed to get a vehicle description, however.",
         "~g~You:~w~ Thanks so much for your help. I'll speak with the victim now.",
        };
        private readonly List<string> VictimInfo1 = new List<string>()
        {
         "~g~You:~w~ Hey there, do you need medical attention?",
         "~b~Victim:~w~ I-I'm alright, officer. Thanks though.",
         "~g~You:~w~ Alright. Do you mind if I ask you a few questions?",
         "~b~Victim:~w~ Yeah, go ahead. I'd love to put an end to this operation.",
         "~g~You:~w~ So is there more than just one person involved in this?",
         "~b~Victim:~w~ Uh, yeah. There's a group of guys that look for women in vulnerable positions.",
         "~b~Victim:~w~ They then trick them into working for them, before holding them against their will.",
         "~g~You:~w~ I already got the vehicle information from the store owner. These are some very good leads.",
         "~b~Victim:~w~ That's awesome. The faster we can put an end to this, the better.",
         "~g~You:~w~ Absolutely. Thanks for your help, is there anything else I can help you with?",
         "~b~Victim:~w~ I think I'm ok. Thanks for the help officer.",
        };
        private readonly List<string> VictimInfo2 = new List<string>()
        {
         "~g~You:~w~ Hey there, do you require any medical attention?",
         "~b~Victim:~w~ I-I'm alright, officer. Thanks though.",
         "~g~You:~w~ Alright, sounds good. Do you mind if I ask you a few questions?",
         "~b~Victim:~w~ Yeah, go ahead. I'd love to put an end to this operation.",
         "~g~You:~w~ So is there more than just one person involved in this?",
         "~b~Victim:~w~ Uh, yeah. There's a group of guys that look for women in vulnerable positions.",
         "~b~Victim:~w~ They then trick them into working for them, before holding them against their will.",
         "~g~You:~w~ I already got the vehicle information from the store owner. These are some very good leads.",
         "~b~Victim:~w~ That's awesome. The faster we can put an end to this, the better.",
         "~g~You:~w~ Absolutely. Thanks for your help, is there anything else I can do for you?",
         "~b~Victim:~w~ If it's not too much to ask, could I get a lift to my sister's place? I could probably stay there for a bit.",
        };
        private readonly List<string> SuspectOpening1 = new List<string>()
        {
         "~g~You:~w~ Sir, we've had reports that you've been following people around.",
         "~r~Suspect:~w~ What? I'm just taking a leisurely stroll, officer.",
         "~g~You:~w~ Ma'am, was this the man you called us about?",
         "~b~*Victim nods*",
         "~r~Suspect:~w~ What! You called the cops on me? You stupid bitch!",
         "~b~You:~w~ Sir, calm down!",
        };
        private readonly List<string> VictimEnding1 = new List<string>()
        {
         "~g~You:~w~ Hi Ma'am, Can I Speak With You? You Don't Need to Worry, Everything is Okay Now.",
         "~g~You:~w~ Do you need medical attention?",
         "~b~Victim:~w~ I-I'm alright, officer. Thanks though.",
         "~g~You:~w~ Alright. Do you mind if I ask you a few questions?",
         "~b~Victim:~w~ Uh, Sure. I don't have anything to hide Officer.",
         "~g~You:~w~ All right. Can you tell me what happened?",
         "~b~Victim:~w~ Uh, yeah. There's a group of guys that look for women in vulnerable positions.",
         "~b~Victim:~w~ They then trick them into working for them, before holding them against their will.",
         "~g~You:~w~ I see. Do you know who might be in charge of this sort of operation?",
         "~b~Victim:~w~ No I don't. They never let me see anyone except the man who just tried to kidnap me.",
         "~b~Victim:~w~ I'm just glad I got out alive. I've heard terrible things about what they've done to people.",
         "~g~You:~w~ Absolutely, this could have ended much worse. Thanks for your help, is there anything else I can help you with?",
         "~b~Victim:~w~ I think I'm ok. Thanks for the help officer.",
        };
        private readonly List<string> VictimEnding2 = new List<string>()
        {
         "~g~You:~w~ Hi Ma'am, Can I Speak With You? You Don't Need to Worry, Everything is Okay Now.",
         "~g~You:~w~ Do you need medical attention?",
         "~b~Victim:~w~ I-I'm alright, officer. Thanks though.",
         "~g~You:~w~ Alright. Do you mind if I ask you a few questions?",
         "~b~Victim:~w~ Uh, Sure. I don't have anything to hide Officer.",
         "~g~You:~w~ All right. Can you tell me what happened?",
         "~b~Victim:~w~ Uh, yeah. There's a group of guys that look for women in vulnerable positions.",
         "~b~Victim:~w~ They then trick them into working for them, before holding them against their will.",
         "~g~You:~w~ I see. Do you know who might be in charge of this sort of operation?",
         "~b~Victim:~w~ No I don't. They never let me see anyone except the man who just tried to kidnap me.",
         "~b~Victim:~w~ I'm just glad I got out alive. I've heard terrible things about what they've done to people.",
         "~g~You:~w~ Absolutely, this could have ended much worse. Thanks for your help, is there anything else I can help you with?",
         "~b~Victim:~w~ If it's not too much to ask, could I get a lift to my sister's place? I could probably stay there for a bit.",
        };
        public override bool OnBeforeCalloutDisplayed()
        {
            Game.LogTrivial("==========YOBBINCALLOUTS: Human Trafficking Callout Start==========");
            System.Random r = new System.Random();
            int Scenario = r.Next(0, 4);    //change
            MainScenario = Scenario;
            Game.LogTrivial("YOBBINCALLOUTS: Scenario is " + MainScenario + "");

            Zone = Functions.GetZoneAtPosition(Game.LocalPlayer.Character.Position).RealAreaName;
            Game.LogTrivial("YOBBINCALLOUTS: Zone is " + Zone);
            CallHandler.locationChooser(CallHandler.StoreList);
            if (CallHandler.locationReturned) { MainSpawnPoint = CallHandler.SpawnPoint; } 
            else { Game.LogTrivial("YOBBINCALLOUTS: Not near store. Aborting callout."); return false; }

            ShowCalloutAreaBlipBeforeAccepting(MainSpawnPoint, 75f);    //Callout Blip Circle with radius 50m
            AddMinimumDistanceCheck(25f, MainSpawnPoint);   //Player must be 25m or further away
            Functions.PlayScannerAudio("CITIZENS_REPORT CRIME_CIVILIAN_NEEDING_ASSISTANCE_01");
            CalloutMessage = "Human Trafficking";
            CalloutPosition = MainSpawnPoint;
            if (MainScenario <= 1) CalloutAdvisory = "A Store Owner Has Spotted a Suspected Victim of ~r~Trafficking.";
            else
            {
                if (Main.CalloutInterface)
                {
                    CalloutAdvisory = "A Store Owner Has Spotted a Suspected Victim of ~r~Trafficking.";
                    CalloutInterfaceHandler.SendMessage(this, "Suspect Reportedly Still on Scene.");
                }
                else
                {
                    CalloutAdvisory = "A Store Owner Has Spotted a Suspected Victim of ~r~Trafficking.";
                    Game.DisplayNotification("~r~Suspect ~w~Reportedly Still on Scene.");
                }
            }
            return base.OnBeforeCalloutDisplayed();
        }
        public override bool OnCalloutAccepted()
        {
            Game.LogTrivial("YOBBINCALLOUTS: Human Trafficking Callout Accepted by User.");
            if (Main.CalloutInterface)
            {
                CalloutInterfaceHandler.SendCalloutDetails(this, "CODE 2", "");
            }
            else
            {
                Game.DisplayNotification("Respond ~b~Code 2.");
            }

            if (MainScenario <= 1) //suspect not on scene
            {
                //witness spawning for the first scenario
                Witness = new Ped("mp_m_shopkeep_01", MainSpawnPoint, 69);  //might have to specify witness model
                Witness.IsPersistent = true;
                Witness.BlockPermanentEvents = true;

                //spawning of victim
                VictimModels = new string[8] { "a_f_y_bevhills_01", "a_f_y_bevhills_02", "a_f_y_bevhills_03", "a_f_y_bevhills_04", "a_f_y_business_01", "a_f_y_business_02", "a_f_y_vinewood_01", "a_f_y_vinewood_04" };
                System.Random lewis = new System.Random();
                int VictimModel = lewis.Next(0, VictimModels.Length);
                NativeFunction.Natives.xA0F8A7517A273C05<bool>(World.GetNextPositionOnStreet(Witness.Position), 0, out Vector3 outPosition);
                Victim = new Ped(VictimModels[VictimModel], outPosition, 69);
                Victim.IsPersistent = true;
                Victim.BlockPermanentEvents = true;
                NativeFunction.Natives.APPLY_PED_DAMAGE_PACK(Victim, "HOSPITAL_2", 0.0f, 0.0f); //damage, float mult?
                NativeFunction.Natives.APPLY_PED_DAMAGE_PACK(Victim, "HOSPITAL_1", 0.0f, 0.0f); //damage, float mult?
                Victim.Tasks.Cower(-1);

                AreaBlip = new Blip(Witness.Position, 25);
            }
            else
            {
                VictimModels = new string[8] { "a_f_y_bevhills_01", "a_f_y_bevhills_02", "a_f_y_bevhills_03", "a_f_y_bevhills_04", "a_f_y_business_01", "a_f_y_business_02", "a_f_y_vinewood_01", "a_f_y_vinewood_04" };
                System.Random lewis = new System.Random();
                int VictimModel = lewis.Next(0, VictimModels.Length);
                NativeFunction.Natives.xA0F8A7517A273C05<bool>(World.GetNextPositionOnStreet(MainSpawnPoint), 0, out Vector3 outPosition);
                Victim = new Ped(VictimModels[VictimModel], outPosition, 69);
                Victim.IsPersistent = true;
                Victim.BlockPermanentEvents = true;
                NativeFunction.Natives.APPLY_PED_DAMAGE_PACK(Victim, "HOSPITAL_2", 0.0f, 0.0f); //damage, float mult?
                NativeFunction.Natives.APPLY_PED_DAMAGE_PACK(Victim, "HOSPITAL_1", 0.0f, 0.0f); //damage, float mult?
                Victim.Tasks.Cower(-1);

                SuspectModels = new string[8] { "A_M_Y_SouCent_01", "A_M_Y_StWhi_01", "A_M_Y_StBla_01", "A_M_Y_Downtown_01", "A_M_Y_BevHills_01", "G_M_Y_MexGang_01", "G_M_Y_MexGoon_01", "G_M_Y_StrPunk_01" };
                System.Random r2 = new System.Random();
                int SuspectModel = r2.Next(0, SuspectModels.Length);
                Suspect = new Ped(SuspectModels[SuspectModel], Victim.GetOffsetPositionFront(3), Victim.Heading - 180);
                Suspect.IsPersistent = true;
                Suspect.BlockPermanentEvents = true;
                Suspect.Heading = Victim.Heading - 180;
                CallHandler.IdleAction(Suspect, false);

                NativeFunction.Natives.xA0F8A7517A273C05<bool>(World.GetNextPositionOnStreet(Victim.Position), 0, out Vector3 SuspectVehicleSpawnPoint);
                try
                {
                    NativeFunction.Natives.GetClosestVehicleNodeWithHeading(SuspectVehicleSpawnPoint, out Vector3 nodePosition, out float heading, 1, 3.0f, 0);
                    SuspectVehicle = new Vehicle("SPEEDO", SuspectVehicleSpawnPoint, heading);
                    SuspectVehicle.PrimaryColor = Color.White;
                    SuspectVehicle.IsPersistent = true;
                }
                catch (Exception)
                {
                    Game.LogTrivial("YOBBINCALLOUTS: Could Not Find Spawnpoint. Aborting Callout.");
                    return false;
                }
                if (MainScenario == 3) //kidnap
                {
                    Victim.WarpIntoVehicle(SuspectVehicle, 0); //passenger seat
                }

                AreaBlip = new Blip(Victim.Position, 25);
            }
            AreaBlip.Color = Color.Yellow;
            AreaBlip.Alpha = 0.67f;
            AreaBlip.IsRouteEnabled = true;
            AreaBlip.Name = "Callout Location";

            if (CalloutRunning == false) Callout();
            return base.OnCalloutAccepted();
        }
        public override void OnCalloutNotAccepted()
        {
            Game.LogTrivial("YOBBINCALLOUTS: Human Trafficking Callout Not Accepted by User.");
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
                        if (MainScenario <= 1) while (player.DistanceTo(Witness) >= 25 && !Game.IsKeyDown(Config.CalloutEndKey)) GameFiber.Wait(0);
                        else while (player.DistanceTo(Victim) >= 35 && !Game.IsKeyDown(Config.CalloutEndKey)) GameFiber.Wait(0);
                        if (Game.IsKeyDown(Config.CalloutEndKey)) { EndCalloutHandler.CalloutForcedEnd = true; break; }
                        Game.LogTrivial("YOBBINCALLOUTS: Player Arrived on Scene.");
                        AreaBlip.Delete();
                        if (MainScenario <= 1) Game.DisplayHelp("Speak with the ~g~Store Manager.");
                        else Game.DisplayHelp("Investigate the Situation.");
                        Confront();
                        break;
                    }
                    Game.LogTrivial("YOBBINCALLOUTS: Callout Finished, Ending...");
                    EndCalloutHandler.EndCallout();
                    End();
                }
                catch (System.Threading.ThreadAbortException)
                {
                    Game.LogTrivial("YOBBINCALLOUTS: THREADABORTEXCEPTION CAUGHT. Usually not a big deal, caused by another plugin/crash somewhere else.");
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
            if (Victim.Exists()) { Victim.Tasks.ClearImmediately(); Victim.ClearLastVehicle(); }
            if (Victim.Exists()) { Victim.Dismiss(); }
            //if (Suspect.Exists()) { Suspect.Dismiss(); }
            if (SuspectArea.Exists()) SuspectArea.Delete();
            if (VictimBlip.Exists()) { VictimBlip.Delete(); }
            if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }
            if (AreaBlip.Exists()) { AreaBlip.Delete(); }
            if (Witness.Exists()) Witness.Dismiss();
            if (WitnessBlip.Exists()) WitnessBlip.Delete();

            Game.LogTrivial("YOBBINCALLOUTS: Human Trafficking Callout Finished Cleaning Up.");
        }
        public override void Process()
        {
            base.Process();
        }
        private void Confront()
        {
            VictimBlip = Victim.AttachBlip();
            VictimBlip.Scale = 0.8f;
            VictimBlip.IsFriendly = true;
            VictimBlip.Name = "Victim";

            if (MainScenario <= 1) //suspect no longer on scene
            {
                WitnessBlip = Witness.AttachBlip();
                WitnessBlip.Scale = 0.8f;
                WitnessBlip.Color = Color.Purple;
                WitnessBlip.Name = "Store Owner";

                Game.DisplayHelp("Speak with the ~p~Store Owner.");
                NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(Witness, player, -1);
                while (player.DistanceTo(Witness) >= 5) GameFiber.Wait(0);
                Game.DisplayHelp("Press ~y~" + Config.MainInteractionKey + " ~w~to Speak with the ~p~Store Owner.");

                System.Random bingchilling = new System.Random();
                int Dialogue = bingchilling.Next(0, 2);
                if (Dialogue == 0) CallHandler.Dialogue(WitnessOpening1, Witness);
                else CallHandler.Dialogue(WitnessOpening2, Witness);

                GameFiber.Wait(1500);
                Witness.Tasks.ClearImmediately();
                WitnessBlip.Delete();

                Game.DisplayHelp("Speak with the ~b~Victim.");
                while (player.DistanceTo(Victim) >= 7f) GameFiber.Wait(0);
                Game.DisplaySubtitle("~g~You:~w~ Hi Ma'am, Can I Speak With You? You Don't Need to Worry, Everything is Okay Now.", 3500);
                GameFiber.Wait(2500);
                Victim.Tasks.AchieveHeading(player.Heading - 180).WaitForCompletion(1000);
                if (Config.DisplayHelp) Game.DisplayHelp("Press ~y~" + Config.MainInteractionKey + " ~w~to Speak with the ~b~Victim.");
                if (MainScenario == 0)
                {
                    CallHandler.Dialogue(VictimInfo1, Victim);
                    //Victim.Tasks.ClearImmediately();

                    Game.LogTrivial("YOBBINCALLOUTS: Dismiss victim");
                    //test this
                    GameFiber.Wait(1000);
                    if (VictimBlip.Exists()) VictimBlip.Delete();
                    Victim.Tasks.ClearImmediately();
                    if (Victim.Exists()) Victim.Dismiss();
                    Search();
                }
                else
                {
                    CallHandler.Dialogue(VictimInfo2, Victim);
                    DrivePeep();
                }
            }
            else //suspect on scene
            {
                SuspectBlip = Suspect.AttachBlip();
                SuspectBlip.Scale = 0.8f;
                SuspectBlip.IsFriendly = false;
                SuspectBlip.Name = "Suspect";

                Game.DisplaySubtitle("~r~Suspect:~w~ What?! You called the cops on me you stupid bitch?!");
                Suspect.RelationshipGroup.SetRelationshipWith(RelationshipGroup.Player, Relationship.Hate);

                if (MainScenario == 2) //maybe add another scenario where victim is kidnapped (cower in car?)
                {
                    Suspect.BlockPermanentEvents = false;
                    //Victim.BlockPermanentEvents = false;
                    Victim.Tasks.Cower(-1);
                    Suspect.Tasks.FightAgainst(Victim, -1);
                    GameFiber.Wait(1000);
                    Victim.Tasks.ReactAndFlee(Suspect); //test this
                    Game.LogTrivial("YOBBINCALLOUTS: Suspect started assaulting victim.");

                    CallHandler.SuspectWait(Suspect);
                }
                else //kidnapping
                {
                    Suspect.Tasks.EnterVehicle(SuspectVehicle, -1, -1, 4.20f).WaitForCompletion();
                    Game.LogTrivial("YOBBINCALLOUTS: Suspect entered vehicle.");
                    if (Suspect.Exists() && Suspect.IsAlive)
                    {
                        Game.LogTrivial("YOBBINCALLOUTS: Suspect Pursuit Started");
                        Game.DisplayNotification("Suspect is ~r~Evading~w~ with a ~o~Kidnapped~w~ Female!");
                        if (Main.CalloutInterface) CalloutInterfaceHandler.SendMessage(this, "Suspect is ~r~Evading~w~ with a ~o~Kidnapped~w~ Female");
                        CallHandler.CreatePursuit(MainPursuit, true, true, true, Suspect);                       
                    }
                }

                if (Victim.Exists() && Victim.IsAlive)
                {
                    Game.LogTrivial("YOBBINCALLOUTS: Started talking to victim on callout end.");
                    //wrap-up
                    if (SuspectBlip.Exists()) SuspectBlip.Delete();
                    Victim.Tasks.LeaveVehicle(SuspectVehicle, LeaveVehicleFlags.LeaveDoorOpen).WaitForCompletion();
                    Victim.Tasks.Cower(-1);
                    Game.DisplayHelp("When Ready, Locate and Speak with the ~b~Victim.");
                    while (player.DistanceTo(Victim) >= 4) GameFiber.Wait(0);
                    Victim.Tasks.AchieveHeading(player.Heading - 180).WaitForCompletion(500);

                    //dialogue
                    if (Config.DisplayHelp) Game.DisplayHelp("Press ~y~" + Config.MainInteractionKey + " ~w~to Speak with the ~b~Victim.");
                    System.Random bingchilling = new System.Random();
                    int Dialogue = bingchilling.Next(0, 2);
                    if (Dialogue == 0)
                    {
                        CallHandler.Dialogue(VictimEnding1, Victim);
                        Game.LogTrivial("YOBBINCALLOUTS: Dismiss victim");
                        //test this
                        GameFiber.Wait(1000);
                        if (VictimBlip.Exists()) VictimBlip.Delete();
                        Victim.Tasks.ClearImmediately();
                        if (Victim.Exists()) Victim.ClearLastVehicle();
                        //Dismiss should be taken care of in End()
                    }
                    else
                    {
                        CallHandler.Dialogue(VictimEnding2, Victim);
                        DrivePeep();
                    }
                }
            }
        }

        private void DrivePeep()
        {
            Victim.Tasks.AchieveHeading(Game.LocalPlayer.Character.Heading - 180).WaitForCompletion(500);
            CallHandler.IdleAction(Victim, false);
            Game.DisplayHelp("Enter Your Vehicle Give the Victim a ~g~Ride~w~. To ~r~Decline~w~ the Ride, Press ~y~" + Config.CalloutEndKey + ".");
            CallHandler.IdleAction(Victim, false);
            while (!Game.LocalPlayer.Character.IsInAnyPoliceVehicle && !Game.IsKeyDown(Config.CalloutEndKey)) { GameFiber.Wait(0); }
            if (Game.IsKeyDown(Config.CalloutEndKey))
            {
                if (MainScenario <= 1) Game.DisplaySubtitle("~g~You:~w~ Sorry, I'll Need to Locate the Suspect First.", 3500);
                else Game.DisplaySubtitle("~g~You:~w~ Sorry, I need to take care of something else.", 3500);
                GameFiber.Wait(3000);
                if (Victim.Exists()) Victim.Dismiss();
                if (VictimBlip.Exists()) VictimBlip.Delete();
                if (MainScenario <= 1) Search();
            }
            else
            {
                GameFiber.Wait(1000);
                Game.LogTrivial("YOBBINCALLOUTS: Player Has Accepted the Ride.");
                Game.DisplaySubtitle("~g~You:~w~ I Can Give You a Ride, Hop In!", 3000);
                GameFiber.Wait(2000);
                Game.DisplayHelp("~y~" + Config.Key1 + ": ~b~Ask the Victim to Enter the Passenger Seat. ~y~" + Config.Key2 + ":~b~ Ask the Victim to Enter the Rear Seat.");
                while (!Game.IsKeyDown(Config.Key1) && !Game.IsKeyDown(Config.Key2)) GameFiber.Wait(0); //might do an animation?
                if (Game.IsKeyDown(Config.Key1))
                {
                    SeatIndex = (int)Game.LocalPlayer.Character.CurrentVehicle.GetFreePassengerSeatIndex();
                    Victim.Tasks.EnterVehicle(Game.LocalPlayer.Character.CurrentVehicle, SeatIndex, EnterVehicleFlags.None).WaitForCompletion();
                }
                else
                {
                    SeatIndex = (int)Game.LocalPlayer.Character.CurrentVehicle.GetFreeSeatIndex(1, 2);
                    Victim.Tasks.EnterVehicle(Game.LocalPlayer.Character.CurrentVehicle, SeatIndex, EnterVehicleFlags.None).WaitForCompletion();
                }
                if (VictimBlip.Exists()) { VictimBlip.Delete(); }
                CallHandler.locationChooser(CallHandler.HouseList);
                Vector3 DriverDestination = CallHandler.SpawnPoint;  //catYes
                VictimBlip = new Blip(DriverDestination);
                VictimBlip.Color = System.Drawing.Color.Green;
                VictimBlip.IsRouteEnabled = true;
                VictimBlip.Name = "Destination";
                GameFiber.Wait(1000);
                Game.DisplayHelp("Drive to the ~g~House~w~ Marked on the Map.");
                while (player.DistanceTo(DriverDestination) >= 35f && !Game.IsKeyDown(Config.CalloutEndKey)) { GameFiber.Wait(0); }
                if (Game.IsKeyDown(Config.CalloutEndKey)) { End(); }
                Game.DisplayHelp("Stop Your Vehicle to Let the ~b~Victim ~w~Out.");
                while (player.CurrentVehicle.Speed > 0)
                {
                    GameFiber.Wait(0);
                }
                Victim.PlayAmbientSpeech("generic_thanks");    //fix this later
                Victim.Tasks.LeaveVehicle(Game.LocalPlayer.Character.CurrentVehicle, LeaveVehicleFlags.None).WaitForCompletion();
                GameFiber.Wait(1000);
                if (VictimBlip.Exists()) { VictimBlip.Delete(); }

                GameFiber.StartNew(delegate
                {
                    Victim.Tasks.FollowNavigationMeshToPosition(DriverDestination, 69, 1.25f, -1).WaitForCompletion();
                    //if (Victim.Exists()) Victim.Delete();
                }
                );

                if (MainScenario <= 1) Search();
            }
        }
        private void Search()
        {
            if (CalloutRunning)
            {
                SuspectSpawnPoint = World.GetNextPositionOnStreet(player.Position.Around(500f));

                SuspectVehicle = new Vehicle("SPEEDO", SuspectSpawnPoint);
                SuspectVehicle.PrimaryColor = Color.White;
                SuspectVehicle.IsDeformationEnabled = true;
                SuspectVehicle.IsPersistent = true;

                SuspectModels = new string[8] { "A_M_Y_SouCent_01", "A_M_Y_StWhi_01", "A_M_Y_StBla_01", "A_M_Y_Downtown_01", "A_M_Y_BevHills_01", "G_M_Y_MexGang_01", "G_M_Y_MexGoon_01", "G_M_Y_StrPunk_01" };
                System.Random r2 = new System.Random();
                int SuspectModel = r2.Next(0, SuspectModels.Length);

                Suspect = new Ped(SuspectModels[SuspectModel], SuspectVehicle.Position, 69);   //nice
                Suspect.WarpIntoVehicle(SuspectVehicle, -1);
                Suspect.IsPersistent = true;
                Suspect.BlockPermanentEvents = true;
                Suspect.Tasks.CruiseWithVehicle(15f);

                if (Victim.Exists()) //test this, victim dismiss after decline ride
                {
                    Victim.Tasks.Clear();
                    Victim.Dismiss();
                }

                Game.DisplayHelp("Start ~o~Searching~w~ for the ~r~Suspect.");
                if (Main.CalloutInterface) CalloutInterfaceHandler.SendMessage(this, "Suspect Vehicle is a White " + SuspectVehicle.Model.Name + " Utility Van.");
                else Game.DisplayNotification("Suspect Vehicle is a White " + SuspectVehicle.Model.Name + " Utility Van.");

                SuspectArea = new Blip(Suspect.Position.Around(15), 250);
                SuspectArea.Color = Color.Orange;
                SuspectArea.Alpha = 0.5f;
                SuspectArea.IsRouteEnabled = true;
                GameFiber.Wait(1500);

                while (player.DistanceTo(Suspect) >= 125) GameFiber.Wait(0);
                Suspect.Tasks.CruiseWithVehicle(SuspectVehicle, 15, VehicleDrivingFlags.DriveAroundVehicles);
                Functions.PlayScannerAudio("ATTENTION_ALL_UNITS_01");   //change
                GameFiber.Wait(1000);
                Game.DisplayNotification("~b~Update:~w~ A Caller Has ~y~Spotted~w~ the ~r~Suspect~w~ Driving Recklessly. ~g~Updating Map.");    //fix this, too hard to see suspect. maybe remind them what the car looks like.
                GameFiber.Wait(1000);
                if (SuspectArea.Exists()) SuspectArea.Delete();
                SuspectBlip = Suspect.AttachBlip();
                SuspectBlip.IsFriendly = false;
                SuspectBlip.Scale = 0.75f;
                GameFiber.Wait(1500);
                Game.DisplaySubtitle("Dispatch, We Have Located the ~r~Suspect!");
                GameFiber.Wait(1500);
                Game.DisplayHelp("Perform a ~o~Traffic Stop~w~ on the ~r~Suspect.");
                while (!Functions.IsPlayerPerformingPullover() && Suspect.IsAlive) GameFiber.Wait(0);
                if (MainScenario >= 0) SuspectViolent(); // change
                //else SuspectCooperates();
            }
        }
        private void SuspectViolent()
        {
            if (CalloutRunning)
            {
                System.Random r = new System.Random();
                int SuspectAction = r.Next(0, 2);
                if (SuspectAction == 0)  //pursuit
                {
                    CallHandler.CreatePursuit(MainPursuit, true, true, true, Suspect);
                }
                else // suspect attacks
                {
                    while (SuspectVehicle.Speed > 0) GameFiber.Wait(0);
                    Game.DisplayHelp("Speak With the ~r~Suspect.");

                    System.Random monke = new System.Random();  //Instantiate Random Weapon  generator
                    int WeaponModel = monke.Next(0, 5);    //Use Random Weapon generator
                    Game.LogTrivial("YOBBINCALLOUTS: Weapon Model is " + WeaponModel);

                    if (WeaponModel == 0) Suspect.Inventory.GiveNewWeapon("WEAPON_ASSAULTRIFLE", -1, true);
                    else if (WeaponModel == 1) Suspect.Inventory.GiveNewWeapon("WEAPON_SMG", -1, true);
                    else if (WeaponModel == 2) Suspect.Inventory.GiveNewWeapon("WEAPON_APPISTOL", -1, true);
                    else if (WeaponModel == 3) Suspect.Inventory.GiveNewWeapon("WEAPON_SAWEDOFFSHOTGUN", -1, true);
                    else if (WeaponModel == 4) Suspect.Inventory.GiveNewWeapon("WEAPON_COMPACTRIFLE", -1, true);

                    System.Random rondom = new System.Random();  //Instantiate Random WaitTime generator
                    int WaitTime = rondom.Next(5, 15);    //Use Random WaitTime generator
                    Game.LogTrivial("YOBBINCALLOUTS: Suspect will fire when player is " + WaitTime + " metres away.");
                    while (player.DistanceTo(Suspect) >= WaitTime && !Game.IsKeyDown(Config.CalloutEndKey)) { GameFiber.Wait(0); }
                    while (player.IsInAnyVehicle(false) || player.DistanceTo(Suspect) >= WaitTime) GameFiber.Wait(0);

                    Suspect.Tasks.ParkVehicle(SuspectVehicle, SuspectVehicle.Position, SuspectVehicle.Heading).WaitForCompletion(2500);
                    Suspect.Tasks.LeaveVehicle(SuspectVehicle, LeaveVehicleFlags.LeaveDoorOpen).WaitForCompletion();
                    Suspect.Tasks.AchieveHeading(Game.LocalPlayer.Character.LastVehicle.Heading - 180).WaitForCompletion(1500);
                    Suspect.Tasks.AimWeaponAt(Game.LocalPlayer.Character.Position, 1500).WaitForCompletion();   //test this
                    Suspect.Tasks.FightAgainst(Game.LocalPlayer.Character, -1);
                    if (LSPD_First_Response.Mod.API.Functions.IsPlayerPerformingPullover()) { LSPD_First_Response.Mod.API.Functions.ForceEndCurrentPullover(); }

                    LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("CRIME_ASSAULT_PEACE_OFFICER_01");
                    LSPD_First_Response.Mod.API.Functions.RequestBackup(Suspect.Position, LSPD_First_Response.EBackupResponseType.Code3, LSPD_First_Response.EBackupUnitType.LocalUnit);

                    CallHandler.SuspectWait(Suspect);
                }
            }
        }
    }
}