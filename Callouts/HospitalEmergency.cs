using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace YobbinCallouts.Callouts
{
    [CalloutInfo("Hospital Emergency", CalloutProbability.High)]
    class HospitalEmergency : Callout
    {
        private Vector3 MainSpawnPoint;

        private static Ped Suspect;
        private Ped Nurse;
        private Ped Guard;
        private Ped Hostage;
        private Vehicle SuspectVehicle;
        private Blip SuspectBlip;
        private Blip NurseBlip;
        private Blip Area;
        private Blip HostageBlip;
        private Ped player = Game.LocalPlayer.Character;
        private LHandle MainPursuit;

        private int MainScenario;
        private bool CalloutRunning;

        //DIALOGUE VvvV
        private readonly List<string> NurseOpening1 = new List<string>()
        {
         "~b~Nurse:~w~ Hey Officer, Over Here!!",
         "~g~You:~w~ What's going on? Are you guys okay?",
         "~b~Nurse:~w~ Yeah we're fine, but we got a big problem here!",
         "~b~Nurse:~w~ We just had a patient escape who has a known history of shizophrenia and paranoia.",
         "~b~Nurse:~w~ They were saying some really concerning and threatening things before they escaped!",
         "~g~You:~w~ Do you know where they went?",
         "~b~Nurse:~w~ I have no clue. You got to find them as soon as possible, for their safety and everyone else's!",
         "~g~You:~w~ Is there any information on the patient, or a description?",
         "~b~Nurse:~w~ Yes, I have some medical records right here, take them!",
        };
        private readonly List<string> GuardOpening1 = new List<string>()
        {
         "~b~Security Guard: ~w~Officer, Over Here!",
         "~g~You:~w~ What's going on? Are you guys okay?",
         "~b~Security Guard: ~w~We're fine, but our situation here isn't!",
         "~g~You:~w~ What happened?",
         "~b~Security Guard: ~w~A person came in with a bunch of stab wounds and was bleading all over.",
         "~g~You:~w~ Hospital staff started treating them immediately, but it became clear we'd need to contact the police regarding their condition.",
         "~b~Security Guard: ~w~When we mentioned the Police were on their way, the suspect freaked out and became violent with the nurses and doctors.",
         "~b~Security Guard: ~w~They ran out of the ER before anyone could stop them! I'm worried they won't make it far with their injuries, or worse yet, hurt someone else!",
         "~g~You:~w~ Do you have a description or location I should start looking?!",
         "~b~Security Guard:~w~ Yes, I have some medical records right here, take them!",
        };

        //private readonly List<string> Hostage1 = new List<string>() //this might not work, test
        //{
        //    "~g~You:~w~ " + Functions.GetPersonaForPed(Suspect).Forename + "! You don't have to do this! Let's talk this through!",
        //    "~g~You:~w~ Listen to me " + Functions.GetPersonaForPed(Suspect).Forename + ", You don't have to do this! Let's work this out!",
        //    "~g~You:~w~ Hey" + Functions.GetPersonaForPed(Suspect).Forename + "! Let them go, we can work this out!",
        //};
        private readonly List<string> Hostage2 = new List<string>()
        {
            "~r~Patient:~w~ No! They have to die! They're Gonna kill me!!",
            "~r~Patient:~w~ Don't step closer Officer!! They gotta die! They're out to kill me!",
            "~r~Patient:~w~ I can't do that Officer!! This person is trying to kill me!!",
        };
        private readonly List<string> Hostage3 = new List<string>()
        {
            "~g~You:~w~ I want to help you! The people at the hospital do, too! Just let them go and we can work this through.",
            "~g~You:~w~ I want to help you! We can't do that until you let them go! We can get you safe once you do that!",
            "~g~You:~w~ We want to help you! We can make sure that doesn't happen once you let them go!",
        };
        private readonly List<string> Release1 = new List<string>()
        {
            "~r~Patient:~w~ Do you promise? They'll keep me safe from these people trying to kill me?",
            "~r~Patient:~w~ Are you sure Officer? They'll keep all these people trying to kill me away?",
            "~r~Patient:~w~ You'll help me? I need protection from all these people around me trying to kill me!!",
        };
        private readonly List<string> Release2 = new List<string>()
        {
            "~g~You:~w~ Yes I promise! Just let them go, please!",
            "~g~You:~w~ Yes I promise! Let them go and we can work this through!",
            "~g~You:~w~ I promise we'll keep you safe once you let them go!",
        };
        private readonly List<string> Kill1 = new List<string>()
        {
            "~r~Patient:~w~ No you don't! Nobody ever takes me seriously!!",
            "~r~Patient:~w~ Nobody takes me seriously! Not a single person has ever done anything for me!",
            "~r~Patient:~w~ No you don't! This person has to die before they kill me!",
        };
        private readonly List<string> Kill2 = new List<string>()
        {
            "~g~You~w~ I'll make sure you're looked after! You need to let them go for me first!",
            "~g~You~w~ Let them go and I promise you you'll be okay! Just do that for me, please!!",
            "~g~You~w~ Whatever has happened to you, I'm sorry! We'll make sure everything is alright once you let them go!",
        };
        private readonly List<string> Kill3 = new List<string>()
        {
            "~r~Patient:~w~ I can't do that! That would be bad for everybody!!",
            "~r~Patient:~w~ I can't do that Officer! I can't let these people go! They're evil!",
            "~r~Patient:~w~ I will not do that! I won't let them go! They must die!!",
        };
        public override bool OnBeforeCalloutDisplayed()
        {
            Game.LogTrivial("==========YOBBINCALLOUTS: Hospital Emergency Callout Start==========");
            System.Random r = new System.Random();
            int Scenario = r.Next(0, 0); //change later
            MainScenario = Scenario;
            Game.LogTrivial("YOBBINCALLOUTS: Scenario value is: " + MainScenario);
            CallHandler.locationChooser(CallHandler.HospitalList, maxdistance: 1000f);
            if(CallHandler.locationReturned) { MainSpawnPoint = CallHandler.SpawnPoint; }
            else { Game.LogTrivial("No Hospital location found within range. Aborting Callout."); return false; }
            ShowCalloutAreaBlipBeforeAccepting(MainSpawnPoint, 25f);
            AddMinimumDistanceCheck(60f, MainSpawnPoint);
            Functions.PlayScannerAudio("ATTENTION_ALL_UNITS_01 WE_HAVE YC_DISTURBANCE IN_A_01 YC_HOSPITAL");
            CalloutMessage = "Hospital Emergency";
            CalloutPosition = MainSpawnPoint;
            if (MainScenario == 0) CalloutAdvisory = "An unstable patient has reportedly escaped from Hospital Custody.";
            else CalloutAdvisory = "An unstable patient has reportedly escaped from Hospital Custody."; //change
            return base.OnBeforeCalloutDisplayed();
        }
        public override bool OnCalloutAccepted()
        {
            try
            {
                Game.LogTrivial("YOBBINCALLOUTS: Hospital Emergency Callout Accepted by User");
                if (Main.CalloutInterface)
                {
                    CalloutInterfaceHandler.SendCalloutDetails(this, "CODE 3", "");
                }
                else
                {
                    Game.DisplayNotification("Respond ~r~Code 3");
                }

                if (MainScenario == 0) //escaped patient
                {
                    Nurse = new Ped("s_f_y_scrubs_01", MainSpawnPoint, 69); //change heading later
                    Nurse.IsPersistent = true;
                    Nurse.BlockPermanentEvents = true;
                    NurseBlip = Nurse.AttachBlip();
                    NurseBlip.IsFriendly = true;
                    NurseBlip.IsRouteEnabled = true;
                    Vector3 dir = (player.Position - Nurse.Position); //thanks Albo!
                    Nurse.Tasks.AchieveHeading(MathHelper.ConvertDirectionToHeading(dir)).WaitForCompletion(1100);
                    Nurse.Tasks.PlayAnimation("friends@frj@ig_1", "wave_a", 1.1f, AnimationFlags.Loop);

                    var GuardSpawnPoint = World.GetNextPositionOnStreet(Nurse.Position);
                    NativeFunction.Natives.xA0F8A7517A273C05<bool>(GuardSpawnPoint, 0, out Vector3 outPosition);

                    Guard = new Ped("s_m_m_security_01", outPosition, Nurse.Heading - 180); //offset position
                    Guard.IsPersistent = true;
                    Guard.BlockPermanentEvents = true;
                    CallHandler.IdleAction(Guard, true);

                    Suspect = new Ped(World.GetNextPositionOnStreet(MainSpawnPoint.Around(169))); //may reduce
                    Suspect.IsPersistent = true;
                    Suspect.BlockPermanentEvents = true;
                    Suspect.Tasks.Wander();
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
            Game.LogTrivial("YOBBINCALLOUTS: Hospital Emergency Callout Not Accepted by User.");
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
                        if (Config.DisplayHelp) Game.DisplayHelp("Drive to the ~b~Hospital.");
                        while (player.DistanceTo(MainSpawnPoint) >= 20 && !Game.IsKeyDown(Config.CalloutEndKey)) { GameFiber.Wait(0); }
                        if (Game.IsKeyDown(Config.CalloutEndKey)) { EndCalloutHandler.CalloutForcedEnd = true; break; }

                        if (MainScenario == 0) NurseOpening();
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
            if (NurseBlip.Exists()) NurseBlip.Delete();
            //if (Nurse.Exists()) Nurse.Dismiss();
            //if (Guard.Exists()) Guard.Dismiss();
            if (Area.Exists()) Area.Delete();
            if (Hostage.Exists()) Hostage.Dismiss();
            if (HostageBlip.Exists()) HostageBlip.Delete();
            Game.LogTrivial("YOBBINCALLOUTS: Hospital Emergency Callout Finished Cleaning Up.");
        }
        public override void Process()
        {
            base.Process();
        }
        private void NurseOpening()
        {
            if (CalloutRunning)
            {
                if (Main.CalloutInterface) CalloutInterfaceHandler.SendMessage(this, "Unit on Scene.");
                NurseBlip.Scale = 0.69f;
                NurseBlip.IsRouteEnabled = false;
                if (Config.DisplayHelp) Game.DisplayHelp("Speak with the ~b~Nurse.");
                while (player.DistanceTo(Nurse) >= 6f) GameFiber.Wait(0);
                Nurse.Tasks.AchieveHeading(player.Heading - 180);
                if (Config.DisplayHelp) Game.DisplayHelp("Press ~y~" + Config.MainInteractionKey + " ~w~to Speak with the ~b~Nurse.");
                System.Random r = new System.Random();
                int Dialogue = r.Next(0, 0); //change later
                if (Dialogue == 0) CallHandler.Dialogue(NurseOpening1, Nurse);
                //give the player documents
                GameFiber.Wait(1000);
                var document = new Rage.Object("prop_cd_paper_pile1", Vector3.Zero);
                document.IsPersistent = true;
                document.AttachTo(Nurse, Nurse.GetBoneIndex(PedBoneId.RightHand), new Vector3(0.1490f, 0.0560f, -0.0100f), new Rotator(-17f, -142f, -151f)); //might have to change to other hand anim?
                Nurse.Tasks.PlayAnimation("mp_common", "givetake1_b", 1f, AnimationFlags.Loop);
                GameFiber.Wait(1000);
                document.Delete();
                GameFiber.Wait(1000);
                //make this a helper later, test ~n~ (new break)
                var personaarray = new string[5];
                personaarray[0] = "~w~Date of Birth: ~y~" + Functions.GetPersonaForPed(Suspect).Birthday.Date;
                personaarray[1] = "~n~~w~Sex: ~y~" + Functions.GetPersonaForPed(Suspect).Gender;
                personaarray[2] = "~n~~w~Times Stopped: ~o~" + Functions.GetPersonaForPed(Suspect).TimesStopped;
                personaarray[3] = "~n~~w~Medical History: ~r~Shizophrenia, Paranoia";
                personaarray[4] = "~n~~w~Medication: ~r~Antipsychotic medication"; //make this change
                var persona = string.Concat(personaarray);
                //mpinventory -> drug_trafficking
                //commonmenu -> shop_health_icon_b
                Game.DisplayNotification("commonmenu", "shop_health_icon_b", "~g~Patient Information", "~b~" + Functions.GetPersonaForPed(Suspect).FullName, persona);
                //Functions.DisplayPedId(Suspect, true); //test this
                GameFiber.Wait(1500);
                CallHandler.IdleAction(Nurse, false);
                if (NurseBlip.Exists()) NurseBlip.Delete();
                SuspectSearch();
            }
        }
        private void SuspectSearch()
        {
            if (CalloutRunning)
            {
                if (Config.DisplayHelp) Game.DisplayHelp("Start ~b~Searching~w~ for the Patient.");
                Area = new Blip(Suspect.Position.Around(5), 100f);
                Area.Alpha = 0.69f;
                Area.Color = Color.Orange;
                Area.IsRouteEnabled = true;

                Suspect.Tasks.Wander();
                System.Random chez = new System.Random();
                int WaitDistance = chez.Next(15, 25); //in metres
                while (player.DistanceTo(Suspect) >= WaitDistance) GameFiber.Wait(0);

                //set suspect movement styles
                //search/scenarios
                //possible scenarios: 
                //*just running, obvious, coop/not
                //hostage
                //pursuit

                System.Random r2 = new System.Random();
                int SuspectScenario = r2.Next(0, 0); //change later
                if (SuspectScenario == 0) //hostage
                {
                    Area.Delete();
                    SuspectBlip = Suspect.AttachBlip();
                    SuspectBlip.IsFriendly = false;
                    SuspectBlip.Scale = 0.69f;
                    if (Suspect.IsAlive)
                    {
                        var Peds = Suspect.GetNearbyPeds(10);

                        for (int i = 0; i < Peds.Length; i++)
                        {
                            GameFiber.Yield();
                            if (Peds[i].Exists() && !Peds[i].IsPlayer && !Peds[i].IsInAnyVehicle(false))
                            {
                                Hostage = Peds[i];
                                break;
                            }
                        }

                        if (Hostage.Exists() && Hostage.DistanceTo(Suspect) <= 20) //test distance
                        {
                            Game.LogTrivial("YOBBINCALLOUTS: Hostage Location = " + Hostage.Position);
                            Hostage.IsPersistent = true; Hostage.BlockPermanentEvents = true;
                            //to-do: set hostage facial override (mood native)
                            // Suspect.Tasks.GoStraightToPosition(Hostage.Position, 3f, Hostage.Heading, 1f, 5000).WaitForCompletion();
                            Suspect.Tasks.FollowNavigationMeshToPosition(Hostage.Position, Hostage.Heading, 5.5f, 1f).WaitForCompletion();
                            Hostage.Position = Suspect.GetOffsetPosition(new Vector3(0f, 0.14445f, 0f));
                            System.Random rhcp = new System.Random();
                            int WeaponModel = rhcp.Next(1, 4);
                            Game.LogTrivial("YOBBINCALLOUTS: Suspect Weapon Model is " + WeaponModel);
                            if (WeaponModel == 1) Suspect.Inventory.GiveNewWeapon("WEAPON_PISTOL", -1, true);
                            else if (WeaponModel == 2) Suspect.Inventory.GiveNewWeapon("WEAPON_APPISTOL", -1, true);
                            else if (WeaponModel == 3) Suspect.Inventory.GiveNewWeapon("WEAPON_MICROSMG", -1, true);
                            Game.DisplaySubtitle("~r~Patient:~w~ Don't come any closer, or they'll die!!");
                            //GameFiber.Wait(1000);
                            Suspect.Tasks.PlayAnimation("misssagrab_inoffice", "hostage_loop", 1f, AnimationFlags.None).WaitForCompletion(500);
                            Suspect.Tasks.PlayAnimation("misssagrab_inoffice", "hostage_loop_mrk", 1f, AnimationFlags.Loop);
                            HostageBlip = Hostage.AttachBlip();
                            HostageBlip.IsFriendly = true;
                            HostageBlip.Scale = 0.69f;
                            if (Hostage.IsFemale) Hostage.Tasks.PlayAnimation("anim@move_hostages@female", "female_idle", 1f, AnimationFlags.Loop);
                            else Hostage.Tasks.PlayAnimation("anim@move_hostages@male", "male_idle", 1f, AnimationFlags.Loop);

                            //This switch doesn't actually do anything, it's just a statement to break out of if the Suspect is killed prematurely in the hostage situation
                            var lewis = 0;
                            switch (lewis)
                            {
                                case 0:
                                    Game.DisplayHelp("Press ~y~" + Config.MainInteractionKey + "~w~ to Reason with the ~o~Patient.");
                                    HostageHold();
                                    if (Suspect.IsAlive) Game.DisplaySubtitle("~g~You:~w~ " + Functions.GetPersonaForPed(Suspect).Forename + "! You don't have to do this! Let's talk this through!");
                                    else break;
                                    HostageHold();
                                    if (Suspect.IsAlive) Game.DisplaySubtitle(DialogueAdvance(Hostage2));
                                    else break;
                                    HostageHold();
                                    if (Suspect.IsAlive) Game.DisplaySubtitle(DialogueAdvance(Hostage3));
                                    else break;
                                    System.Random morsha = new System.Random();
                                    int action = morsha.Next(0, 2);
                                    Game.LogTrivial("YOBBINCALLOUTS: Suspect Action is " + WeaponModel);
                                    if (action == 0) //release
                                    {
                                        HostageHold();
                                        if (Suspect.IsAlive) Game.DisplaySubtitle(DialogueAdvance(Release1));
                                        else break;
                                        HostageHold();
                                        if (Suspect.IsAlive) Game.DisplaySubtitle(DialogueAdvance(Release2));
                                        else break;
                                        System.Random zach = new System.Random();
                                        int WaitTime = zach.Next(2000, 5000); //in ms
                                        GameFiber.Wait(WaitTime);
                                        Suspect.Tasks.PutHandsUp(-1, player);
                                        if (Suspect.IsDead) break;
                                        Game.DisplaySubtitle("~r~Patient:~w~ Okay Officer, If you say so! Just don't let them hurt me!!");
                                        GameFiber.Wait(500);
                                        Hostage.Tasks.ReactAndFlee(Suspect);
                                        GameFiber.Wait(2000);
                                        Game.DisplayHelp("Take the ~o~Patient~w~ into Custody.");
                                        while (!Functions.IsPedArrested(Suspect)) GameFiber.Wait(0);
                                        if (HostageBlip.Exists()) HostageBlip.Delete();
                                        break;
                                    }
                                    else //kill
                                    {
                                        HostageHold();
                                        if (Suspect.IsAlive) Game.DisplaySubtitle(DialogueAdvance(Kill1));
                                        else break;
                                        //while (!Game.IsKeyDown(Config.MainInteractionKey) && Suspect.IsAlive) GameFiber.Wait(0);
                                        HostageHold();
                                        if (Suspect.IsAlive) Game.DisplaySubtitle(DialogueAdvance(Kill2));
                                        else break;
                                        HostageHold();
                                        if (Suspect.IsAlive) Game.DisplaySubtitle(DialogueAdvance(Kill3));
                                        else break;
                                        System.Random zach = new System.Random();
                                        int WaitTime = zach.Next(1500, 5000); //in ms
                                        GameFiber.Wait(WaitTime);
                                        if (Suspect.IsDead) break;
                                        Suspect.Tasks.FireWeaponAt(Hostage, -1, FiringPattern.SingleShot).WaitForCompletion();
                                        while (Suspect.Exists() && !Functions.IsPedArrested(Suspect) && Suspect.IsAlive) GameFiber.Wait(0);
                                        if (HostageBlip.Exists()) HostageBlip.Delete();
                                        if (Hostage.Exists() && Hostage.IsAlive) Hostage.Tasks.ReactAndFlee(Suspect);
                                        break;
                                    }
                            }
                            //test vvv
                            if (HostageBlip.Exists()) HostageBlip.Delete();
                            if (Hostage.Exists() && Hostage.IsAlive) Hostage.Tasks.ReactAndFlee(player); //might remove
                            if (Functions.IsPedArrested(Suspect) || Suspect.IsAlive)
                            {
                                Game.DisplayNotification("Dispatch, we have taken the Patient into ~r~Custody.");
                                GameFiber.Wait(1500);
                                LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("REPORT_RESPONSE_COPY_02");
                                GameFiber.Wait(1500);
                                DriveBack();
                            }
                            else Game.DisplayNotification("Dispatch, Suspect has been ~r~Killed.");
                            GameFiber.Wait(2000);
                            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("REPORT_RESPONSE_COPY_02");
                            GameFiber.Wait(1500);
                        }
                        else
                        {
                            Pursuit();
                        }
                    }
                    else
                    {
                        Game.DisplayNotification("Dispatch, Suspect has been ~r~Killed.");
                    }
                }
                else Pursuit();
            }
        }
        private void DriveBack()
        {
            if (CalloutRunning)
            {
                GameFiber.Wait(1500);
                Game.DisplayHelp("Take the ~r~Patient~w~ back to the ~b~Hospital.");
                NurseBlip = Nurse.AttachBlip();
                NurseBlip.IsFriendly = true;
                NurseBlip.IsRouteEnabled = true;
                GameFiber.Wait(1500);
                if (!Main.STP)
                {
                    Game.DisplayHelp("~y~" + Config.Key1 + ": ~b~Ask the Patient to Enter the Passenger Seat. ~y~" + Config.Key2 + ":~b~ Ask the Patient to Enter the Rear Seat.");
                    while (!Game.IsKeyDown(Config.Key1) && !Game.IsKeyDown(Config.Key2)) GameFiber.Wait(0); //might do an animation?
                    if (Game.IsKeyDown(Config.Key1))
                    {
                        var SeatIndex = (int)Game.LocalPlayer.Character.LastVehicle.GetFreePassengerSeatIndex();
                        Suspect.Tasks.EnterVehicle(Game.LocalPlayer.Character.LastVehicle, SeatIndex, EnterVehicleFlags.None).WaitForCompletion();
                    }
                    else
                    {
                        var SeatIndex = (int)Game.LocalPlayer.Character.LastVehicle.GetFreeSeatIndex(1, 2);
                        Suspect.Tasks.EnterVehicle(Game.LocalPlayer.Character.LastVehicle, SeatIndex, EnterVehicleFlags.None).WaitForCompletion();
                    }
                    while (player.DistanceTo(Nurse) >= 15f && !Game.IsKeyDown(Config.CalloutEndKey)) { GameFiber.Wait(0); }
                    Game.DisplayHelp("Stop Your Vehicle to Let the ~r~Patient ~w~Out.");
                    while (player.CurrentVehicle.Speed > 0) GameFiber.Wait(0);
                    Suspect.Tasks.LeaveVehicle(Game.LocalPlayer.Character.CurrentVehicle, LeaveVehicleFlags.LeaveDoorOpen).WaitForCompletion();
                    GameFiber.Wait(1000);
                }
                else
                {
                    Game.DisplayHelp("Use ~b~StopThePed~w~ to Take the Patient ~y~Back.");
                    while (player.DistanceTo(Nurse) >= 15f && !Game.IsKeyDown(Config.CalloutEndKey)) { GameFiber.Wait(0); }
                    Game.DisplayHelp("Let the ~r~Patient ~w~Out of the Car.");
                    while (Suspect.IsInAnyVehicle(false)) GameFiber.Wait(0);
                }
                Nurse.Tasks.GoStraightToPosition(Suspect.GetOffsetPositionFront(1), 3f, Suspect.Heading, 1f, 4000).WaitForCompletion(4000);
                //some frisking animation for nurse (idle, rest of callout)
                Guard.Tasks.GoStraightToPosition(Nurse.GetOffsetPositionRight(2f), 2f, Nurse.Heading, 2f, 2500).WaitForCompletion(2500);
                CallHandler.IdleAction(Nurse, false);
                CallHandler.IdleAction(Guard, true);

                Game.DisplayNotification("Dispatch, we are ~g~Code 4.~w~ We have taken the Patient back to the ~b~Hospital.");
                GameFiber.Wait(1500);
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("REPORT_RESPONSE_COPY_02");
                GameFiber.Wait(1500);
            }
        }

        private void Pursuit()
        {
            if (CalloutRunning)
            {
                Game.DisplayNotification("Suspect is ~r~Evading!");
                Suspect.Tasks.ClearImmediately();
                LSPD_First_Response.Mod.API.Functions.ForceEndCurrentPullover();
                MainPursuit = LSPD_First_Response.Mod.API.Functions.CreatePursuit();
                LSPD_First_Response.Mod.API.Functions.RequestBackup(Suspect.Position, LSPD_First_Response.EBackupResponseType.Code3, LSPD_First_Response.EBackupUnitType.LocalUnit);
                LSPD_First_Response.Mod.API.Functions.SetPursuitIsActiveForPlayer(MainPursuit, true);
                LSPD_First_Response.Mod.API.Functions.AddPedToPursuit(MainPursuit, Suspect);
                GameFiber.Wait(1500);
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("CRIME_SUSPECT_ON_THE_RUN_01");
                while (LSPD_First_Response.Mod.API.Functions.IsPursuitStillRunning(MainPursuit)) { GameFiber.Wait(0); }
                while (Suspect.Exists())
                {
                    GameFiber.Yield();
                    if (!Suspect.Exists() || Suspect.IsDead || Functions.IsPedArrested(Suspect)) break;
                }
                if (Suspect.IsAlive) //test all this (STP )
                {
                    Game.DisplayNotification("Dispatch, a Suspect is Under ~g~Arrest~w~ Following the Pursuit.");
                    DriveBack();
                }
                else
                {
                    GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect Was ~r~Killed~w~ Following the Pursuit.");
                }
                GameFiber.Wait(2000);
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("REPORT_RESPONSE_COPY_02");
            }
        }
        //this helper is the logic the suspect is assigned in between dialogue advances in the hostage situation
        private void HostageHold()
        {
            while (true)
            {
                GameFiber.Yield();
                //Suspect.Tasks.PlayAnimation(xyz) //causes the Ped to glitch when using STP surrender
                if (Suspect.Tasks.CurrentTaskStatus == TaskStatus.Interrupted) Suspect.Tasks.PlayAnimation("misssagrab_inoffice", "hostage_loop_mrk", 1f, AnimationFlags.Loop); //test this (does it override STP surrender?)
                if (Suspect.IsDead || (Functions.IsPedArrested(Suspect)) || Hostage.IsDead) break;
                if (Game.IsKeyDown(Config.MainInteractionKey)) break;
            }
        }
        //this helper returns a certain dialogue for the specific point in the callout as indicated by dialogue.
        //see the various String lists containing the dialogue for each point in the hostage situation.
        private String DialogueAdvance(List<string> dialogue)
        {
            System.Random twboop = new System.Random();
            int dialoguechosen = twboop.Next(0, dialogue.Count);
            return dialogue[dialoguechosen];
        }
    }
}