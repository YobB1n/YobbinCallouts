//THIS CALLOUT IS READY FOR TESTING
using System.Drawing;
using Rage;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage.Native;
using System;
using System.Collections.Generic;

namespace YobbinCallouts.Callouts
{
    [CalloutInfo("[YC] Stolen Mail", CalloutProbability.Medium)]
    public class StolenMail : Callout
    {
        private Vector3 MainSpawnPoint;
        private Vector3 OldLastVehiclePos;
        private DateTime OldDateTime;

        private Citizen Suspect;
        private Ped HouseOwner;
        Ped player = Game.LocalPlayer.Character;
        private Blip HouseOwnerBlip;
        private Blip SuspectBlip;
        private Blip SearchArea;
        private Vector3 DroppedMail;
        private Blip DroppedMailBlip;
        private int MainScenario;
        private Rage.Object Mail;
        private bool CalloutRunning;
        private Vehicle SuspectVehicle;
        List<string> HouseOwnerDialogue = new List<string>()
        {
            "~b~Home Owner: ~w~Hello Officer. Yes, I was the one who called about my mail being stolen. Thank you for coming.",
            "~g~You: ~w~How long has this been happening for?",
            "~b~Home Owner: ~w~This has been happening for a week now.",
            "~g~You: ~w~Why didn't you call us before?",
            "~b~Home Owner: ~w~I thought the mail company took a break.",
            "~g~You: ~w~Uh, ok? Next time if you suspect supicious activity you have to let us know earlier!",
            "~b~Home Owner: ~w~Sorry about that. I do have a CCTV video of the suspect. I think they were just here!"
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
            "~r~Suspect: ~w~Watchu want",
            "~g~You: ~w~Hey, just want to talk to you. Whats in your hand there?",
            "~r~Suspect: ~w~Watchu think it is, mail. punk ass",
            "~g~You: ~w~Where did you get that mail from?",
            "~r~Suspect: ~w~Places.....",
            "~g~You: ~w~I am going to go straight to the point if you straight with me, aight. We got a call about someone stealing mail. You are the only one in this vicinity that is walking around with mail. Did you steal it",
            "~r~Suspect: ~w~fine man, you got me...good job sherlock",
            "~g~You: ~w~Do you got any weapons on you?",
            "~r~Suspect: ~w~nah man, who the fuck you think I am. Don't piss me off 'fore I beat yo ass up."
        };
        private System.Windows.Forms.Keys EndKey = Config.CalloutEndKey;
        private System.Windows.Forms.Keys InteractionKey = Config.MainInteractionKey;
        Random monke = new Random();

        public override bool OnBeforeCalloutDisplayed()
        {
            Game.LogTrivial("==========YOBBINCALLOUTS: Stolen Mail Callout Start==========");
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
                HouseOwner = new Ped(MainSpawnPoint); //was .Around(2)
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
            if(num <= 10)
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
                        Game.DisplaySubtitle("~g~You:~w~ Hello. Did you call about your mail being stolen?");
                        HouseOwnerBlip.IsRouteEnabled = false;
                        HouseOwner.Tasks.AchieveHeading(player.Heading - 180f).WaitForCompletion(500);
                        if (Config.DisplayHelp) Game.DisplayHelp("Press ~y~" + Config.MainInteractionKey + "~w~ to speak with the ~b~Caller.");
                        Vector3 SuspectSpawn = World.GetNextPositionOnStreet(player.Position.Around(550f));
                        SuspectVehicle = CallHandler.SpawnVehicle(SuspectSpawn, 69); //specify heading if reqd
                        SuspectVehicle.IsDeformationEnabled = true;
                        SuspectVehicle.IsPersistent = true;

                        Game.LogTrivial("YOBBINCALLOUTS: Finished Spawning Suspect.");
                        CallHandler.Dialogue(HouseOwnerDialogue, HouseOwner);
                        GameFiber.Wait(3000);
                        //door camera
                        DoorCamera();
                        //while (!Game.IsKeyDown(InteractionKey)) { GameFiber.Wait(0); }
                        Game.DisplaySubtitle("~b~Caller: ~w~I also saw the thief's vehicle on my other security camera. It's a ~r~"+ SuspectVehicle.Model.Name + "~w~.");
                        GameFiber.Wait(3000);
                        Game.DisplaySubtitle("~b~You:~w~ Ok. I Will try my best to find them. If you said they were just here, they could be in the area looking for other unattended packages.");
                        GameFiber.Wait(2000);
                        HouseOwner.Tasks.FollowNavigationMeshToPosition(MainSpawnPoint, 0f, 2f, 2f); //test this
                        CallHandler.IdleAction(HouseOwner, false);
                        SearchArea = new Blip(Suspect.Position.Around(15), 50);
                        FindSuspect();
                    }
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
        private void DoorCamera()
        {
            //from Albo Assorted Callouts
            Game.LogTrivial("YOBBINCALLOUTS: Start door camera...");
            CCTVShowing = true;
            Game.LocalPlayer.HasControl = false;
            Game.FadeScreenOut(1500, true);
            NativeFunction.Natives.SET_TIMECYCLE_MODIFIER("CAMERA_BW");
            if (Game.LocalPlayer.Character.LastVehicle.Exists())

            {
                OldLastVehiclePos = Game.LocalPlayer.Character.LastVehicle.Position;
                Game.LocalPlayer.Character.LastVehicle.IsVisible = false;
                Game.LocalPlayer.Character.LastVehicle.SetPositionZ(Game.LocalPlayer.Character.LastVehicle.Position.Z + 8f);
                Game.LocalPlayer.Character.LastVehicle.IsPositionFrozen = true;
            }
            bool DateTimeChanged = false;
            try
            {
                OldDateTime = World.DateTime;
                World.DateTime = DateTime.Now;
                //World.IsTimeOfDayFrozen = true;
                DateTimeChanged = true;
            }
            catch (Exception e) { }


            Game.LocalPlayer.Character.IsVisible = false;
            HouseOwner.IsVisible = false;
            Vector3 suspectOldPosition = SuspectVehicle.Position;
            Rotator suspectOldRotator = SuspectVehicle.Rotation;
            Vector3 PlayerOldPos = Game.LocalPlayer.Character.Position;
            Vector3 HouseOwnerOldPos = HouseOwner.Position;
            Game.LocalPlayer.Character.SetPositionZ(Game.LocalPlayer.Character.Position.Z + 8f);
            Game.LocalPlayer.Character.IsPositionFrozen = true;
            HouseOwner.SetPositionZ(HouseOwner.Position.Z + 8f);
            HouseOwner.IsPositionFrozen = true;

            //originally camera position based off of suspect vehicle, will see if I can change
            //SuspectVehicle.Position = SuspectVehicleSpawnPoint;
            Camera cam = new Camera(true);
            //cam.Position = SuspectVehicle.GetOffsetPosition(Vector3.RelativeFront * 4.4f);
            cam.SetPositionZ(cam.Position.Z + 3.6f);
            //Vector3 directionFromHouseOwnerToCar = (SuspectVehicle.Position - cam.Position);
            Vector3 directionFromHouseOwnerToCar = (MainSpawnPoint - cam.Position);
            directionFromHouseOwnerToCar.Normalize();
            cam.Rotation = directionFromHouseOwnerToCar.ToRotator();
            //testing
            SuspectVehicle.Position = directionFromHouseOwnerToCar;
            //Suspect.Tasks.PerformDrivingManeuver(VehicleManeuver.Wait);
            Suspect = new Citizen(MainSpawnPoint);
            Suspect.IsPersistent = true;
            Suspect.BlockPermanentEvents = true;
            Suspect.ClearBlood();
            //Suspect.Tasks.FollowNavigationMeshToPosition(HouseOwnerOldPos, 69, 3f, 5f, 0).WaitForCompletion();
            //SuspectVehicle.IsPositionFrozen = false;

            GameFiber.Sleep(2000);

            Game.FadeScreenIn(1500, true);
            CCTVCamNumber = CallHandler.RNG(6);
            Game.FrameRender += DrawCCTVText;
            GameFiber.Wait(1500);
            Game.DisplaySubtitle("~b~Caller~s~: There they are!", 6600);
            GameFiber.Wait(2000);

            Mail = new Rage.Object("prop_cs_envolope_01", Vector3.Zero);
            Mail.IsPersistent = true;
            Mail.AttachTo(Suspect, Suspect.GetBoneIndex(PedBoneId.LeftHand), new Vector3(0.1490f, 0.0560f, -0.0100f), new Rotator(-17f, -142f, -151f));
            Suspect.Tasks.FollowNavigationMeshToPosition(PlayerOldPos, 69, 3f, 5f, 0);
            GameFiber.Wait(6500);
            Game.FadeScreenOut(1500, true);
            CCTVShowing = false;

            //re spawn everything in
            Game.LocalPlayer.Character.IsVisible = true;
            if (Game.LocalPlayer.Character.LastVehicle.Exists())
            {
                Game.LocalPlayer.Character.LastVehicle.Position = OldLastVehiclePos;
                Game.LocalPlayer.Character.LastVehicle.IsPositionFrozen = false;
                Game.LocalPlayer.Character.LastVehicle.IsVisible = true;
            }
            //World.IsTimeOfDayFrozen = false;
            if (DateTimeChanged) { World.DateTime = OldDateTime; }
            HouseOwner.IsVisible = true;
            Game.LocalPlayer.Character.IsPositionFrozen = false;
            HouseOwner.IsPositionFrozen = false;
            Game.LocalPlayer.Character.Position = PlayerOldPos;
            HouseOwner.Position = HouseOwnerOldPos;

            Suspect.WarpIntoVehicle(SuspectVehicle, -1);
            SuspectVehicle.Position = suspectOldPosition;
            SuspectVehicle.Rotation = suspectOldRotator;
            SuspectVehicle.IsPositionFrozen = false;
            Game.LocalPlayer.HasControl = true;
            cam.Delete();
            Suspect.Tasks.CruiseWithVehicle(SuspectVehicle, 17f, VehicleDrivingFlags.DriveAroundVehicles | VehicleDrivingFlags.DriveAroundObjects | VehicleDrivingFlags.DriveAroundPeds);
            Rage.Native.NativeFunction.Natives.SET_DRIVE_TASK_DRIVING_STYLE(Suspect, 786603);
            GameFiber.Sleep(2000);
            NativeFunction.CallByName<uint>("CLEAR_TIMECYCLE_MODIFIER");
            Game.FadeScreenIn(1500, true);

        }
        private bool CCTVShowing = false;
        private int CCTVCamNumber = 3;
        private void DrawCCTVText(System.Object sender, Rage.GraphicsEventArgs e)
        {
            if (CCTVShowing)
            {
                Rectangle drawRect = new Rectangle(0, 0, 200, 130);
                e.Graphics.DrawRectangle(drawRect, Color.FromArgb(100, Color.Black));

                e.Graphics.DrawText("CCTV #" + CCTVCamNumber.ToString("00"), "Aharoni Bold", 35.0f, new PointF(1, 6), Color.White);
                e.Graphics.DrawText(DateTime.Now.Day.ToString("00") + "/" + DateTime.Now.Month.ToString("00") + "/" + DateTime.Now.Year.ToString(), "Aharoni Bold", 35.0f, new PointF(1, 46), Color.White, drawRect);
                e.Graphics.DrawText(DateTime.Now.Hour.ToString("00") + ":" + DateTime.Now.Minute.ToString("00") + ":" + DateTime.Now.Second.ToString("00"), "Aharoni Bold", 35.0f, new PointF(1, 86), Color.White, drawRect);
            }
            else
            {
                Game.FrameRender -= DrawCCTVText;
            }
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
                        Game.DisplaySubtitle("~g~You:~w~ Hello! Did you call about your mail being stolen?");
                        HouseOwnerBlip.IsRouteEnabled = false;
                        HouseOwner.Tasks.AchieveHeading(player.Heading - 180f).WaitForCompletion(500);
                        if (Config.DisplayHelp) Game.DisplayHelp("Press ~y~" + Config.MainInteractionKey + "~w~ to speak with the ~b~House Owner.");
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
                //Mail.AttachTo(Suspect, Suspect.GetBoneIndex(PedBoneId.LeftHand), new Vector3(0.1490f, 0.0560f, -0.0100f), new Rotator(-17f, -142f, -151f));
                while (!player.IsInAnyVehicle(false) && Vector3.Distance(player.Position, Suspect.Position) >= 10f) { GameFiber.Wait(0); }
                if (SearchArea.Exists()) { SearchArea.Delete(); }

                Game.DisplayHelp("Start ~o~Searching~w~ for the ~r~Suspect.");
                CallHandler.VehicleInfo(SuspectVehicle, Suspect);
                SearchArea = new Blip(Suspect.Position, 150);
                SearchArea.Color = Color.Orange;
                SearchArea.Alpha = 0.4f;
                SearchArea.IsRouteEnabled = true;
                GameFiber.Wait(1500);

                while (player.DistanceTo(Suspect) >= 150) GameFiber.Wait(0);
                SuspectVehicle.IsDriveable = true;
                SuspectVehicle.IsVisible = true;
                Suspect.Tasks.CruiseWithVehicle(SuspectVehicle, 15, VehicleDrivingFlags.DriveAroundVehicles);
                Functions.PlayScannerAudio("ATTENTION_ALL_UNITS_01");   //change
                while (player.DistanceTo(Suspect) >= 85f) GameFiber.Wait(0);
                GameFiber.Wait(1000);
                if (Main.CalloutInterface)
                {
                    CalloutInterfaceHandler.SendMessage(this, "Caller spotted the suspect driving recklessly, updating map.");
                }
                else
                {
                    Game.DisplayNotification("~b~Update:~w~ A Caller Has ~y~Spotted~w~ the ~r~Suspect~w~ Driving Recklessly. ~g~Updating Map.");    //fix this, too hard to see suspect. maybe remind them what the car looks like.
                }
                GameFiber.Wait(1000);
                if (SearchArea.Exists()) { SearchArea.Delete(); }
                SearchArea = new Blip(Suspect.Position, 75);
                SearchArea.Color = Color.Orange;
                SearchArea.Alpha = 0.4f;
                SearchArea.IsRouteEnabled = true;

                while (player.DistanceTo(Suspect) >= 25f) GameFiber.Wait(0);
                if (SearchArea.Exists()) SearchArea.Delete();
                SuspectBlip = CallHandler.AssignBlip(Suspect, Color.Red, .69f);
                Game.DisplayHelp("Perform a Traffic Stop on the ~r~Suspect.");
                while (!LSPD_First_Response.Mod.API.Functions.IsPlayerPerformingPullover()) GameFiber.Wait(0);
                if (Suspect.Exists() && Suspect.IsAlive)
                {
                    if (CallHandler.FiftyFifty()) { Cooperates(); }
                    else
                    {
                        if (CallHandler.FiftyFifty()) { Runs(); } else { Shoots(); }
                    }
                }
                else
                {
                    WrapUp();
                }
            }
        }

        private void Cooperates()
        {
            if (Config.DisplayHelp) Game.DisplayNotification("Press ~y~" + Config.MainInteractionKey + " ~w~to speak with the ~r~Suspect.");
            CallHandler.Dialogue(SuspectDialogue, Suspect);
            DetachAndSetBlip();  
            if (Config.DisplayHelp) { Game.DisplayNotification("Arrest the ~r~suspect"); }
            while(Suspect.Exists() && !Suspect.IsCuffed) { GameFiber.Wait(0); }
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
            Suspect.Tasks.GoToWhileAiming(World.GetNextPositionOnStreet(Suspect.Position.Around(550)), player.Position, 5f, 5f, true, FiringPattern.FullAutomatic).WaitForCompletion(1500);
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
                Game.LogTrivial("YOBBINCALLOUTS: Starting Wrap Up Method");
                if (Config.DisplayHelp) { Game.DisplayHelp("Retrieve the ~b~mail"); }
                while(Vector3.Distance(player.Position, DroppedMail) >= 5f) { GameFiber.Wait(0); }
                if (Config.DisplayHelp) { Game.DisplayHelp("Press ~y~" + InteractionKey + " ~w~to retrieve the ~b~mail"); }
                while (!Game.IsKeyDown(InteractionKey)) { GameFiber.Wait(0); }
                player.Tasks.PlayAnimation("amb@medic@standing@kneel@idle_a", "idle_b", 1f, AnimationFlags.Loop);
                GameFiber.Wait(1000);
                Mail.AttachTo(player, player.GetBoneIndex(PedBoneId.LeftHand), new Vector3(0.1490f, 0.0560f, -0.0100f), new Rotator(-17f, -142f, -151f));
                GameFiber.Wait(1000);
                player.Tasks.Clear();
                GameFiber.Wait(1000);
                if (Config.DisplayHelp) { Game.DisplayHelp("Return the mail to the ~b~home owner."); }
                HouseOwnerBlip.IsRouteEnabled = true;
                Mail.Detach();
                if (Mail.Exists()) Mail.IsVisible = false;
                while (Vector3.Distance(player.Position, HouseOwner.Position) >= 7.5f) { GameFiber.Wait(0); }
                HouseOwner.Tasks.AchieveHeading(player.Heading - 180f).WaitForCompletion(500);
                Game.DisplaySubtitle("~g~You: ~w~I have retrieved your mail!");
                if(Config.DisplayHelp) { Game.DisplayHelp("Press + " + InteractionKey + " to return mail."); }
                while(!Game.IsKeyDown(InteractionKey)) { GameFiber.Wait(0); }
                Mail.AttachTo(player, player.GetBoneIndex(PedBoneId.LeftHand), new Vector3(0.1490f, 0.0560f, -0.0100f), new Rotator(-17f, -142f, -151f));
                Mail.IsVisible = true;
                player.Tasks.PlayAnimation("mp_common", "givetake1_b", 1f, AnimationFlags.Loop);
                GameFiber.Wait(1000);
                Mail.AttachTo(HouseOwner, HouseOwner.GetBoneIndex(PedBoneId.LeftHand), new Vector3(0.1490f, 0.0560f, -0.0100f), new Rotator(-17f, -142f, -151f));
                GameFiber.Wait(1000);
                player.Tasks.Clear();
                GameFiber.Wait(1000);
                Game.DisplaySubtitle("~b~Home Owner: ~w~Thank you so much officer.");
                HouseOwner.PlayAmbientSpeech("generic_thanks");
                GameFiber.Wait(2000);
                Game.DisplaySubtitle("~g~You: ~w~No worries!");
                GameFiber.Wait(2000);

                //test this:
                GameFiber.StartNew(delegate
                {
                    HouseOwner.Tasks.FollowNavigationMeshToPosition(MainSpawnPoint, 69, 1.25f, -1).WaitForCompletion();
                    GameFiber.Wait(500);
                    //if (Driver.Exists()) Driver.Delete();
                });

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
            NativeFunction.CallByName<uint>("CLEAR_TIMECYCLE_MODIFIER");
            Game.LocalPlayer.HasControl = true;
            Game.LogTrivial("YOBBINCALLOUTS: Stolen Mail Callout Finished Cleaning Up.");
        }

        public override void Process()
        {
            base.Process();
        }

    }
}
