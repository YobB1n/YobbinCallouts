//using System.Drawing;
//using Rage;
//using LSPD_First_Response.Mod.API;
//using LSPD_First_Response.Mod.Callouts;
//using Rage.Native;
//using System.Collections.Generic;
//using System;

//namespace YobbinCallouts.Callouts
//{
//    [CalloutInfo("[YC] Person With a Weapon", CalloutProbability.High)]

//    class PersonWithWeapon : Callout
//    {
//        private int MainScenario; //If there are multiple callout scenarios, which one, starting from zero.
//        private bool CalloutRunning; //If the callout is currently running or not.

//        private Vector3 MainSpawnPoint;
//        private Ped player = Game.LocalPlayer.Character;
//        private Ped Suspect;
//        private Citizen Hostage;

//        private Blip SuspectBlip;
//        private Blip HostageBlip;
//        private Blip AreaBlip;
//        private LHandle MainPursuit;

//        public static String WeaponName;

//        //DIALOGUE VvvV
//        private readonly List<string> ReasonSuccess1 = new List<string>()
//        {
//         "~g~You:~w~ Hey, Please Drop the " + WeaponName + "! Let's talk this through!",
//         "~r~Suspect:~w~ W-Who are you? Where am I? What am I holding?!",
//         "~g~You:~w~ We can go through all that soon! First, you have to drop what's in your hand for me, okay?",
//         "~r~Suspect:~w~ W-Who are you? Where am I? What am I holding?!",
//         "~g~You:~w~ Just drop what you're holding and we can answer all those questions, please!",
//         "~r~Suspect:~w~ A-Alright, I will!",
//        };
//        private readonly List<string> ReasonSuccess2 = new List<string>()
//        {
//         "~g~You:~w~ Hey, Please Drop the " + WeaponName + "! Let's talk this through!",
//         "~r~Suspect:~w~ No, how about you drop yours!",
//         "~g~You:~w~ Look, nobody wants to hurt you! Just let it go and we'll figure it out!",
//         "~r~Suspect:~w~ I don't know! I like this " + WeaponName + "!",
//         "~g~You:~w~ Just drop it, and nobody gets hurt!",
//         "~r~Suspect:~w~ A-Alright, Fine, I will!",
//        };

//        //pre-callout logic
//        public override bool OnBeforeCalloutDisplayed()
//        {
//            Game.LogTrivial("==========YOBBINCALLOUTS: Person With a Weapon Callout Start=========="); //All callout starts are logged like this
//            System.Random r = new System.Random();
//            int Scenario = r.Next(0, 4); //0 - surrender, 1 - hostage, 2 - attack, 3 - flee
//            MainScenario = Scenario;
//            Game.LogTrivial("YOBBINCALLOUTS: Scenario value is: " + MainScenario);

//            MainSpawnPoint = World.GetNextPositionOnStreet(player.Position.Around(600)); //get the Main Spawn point.
//            ShowCalloutAreaBlipBeforeAccepting(MainSpawnPoint, 25f);
//            AddMinimumDistanceCheck(60f, MainSpawnPoint);
//            Functions.PlayScannerAudio("ATTENTION_ALL_UNITS_01 WE_HAVE_01"); //change

//            CalloutMessage = "Person With a Weapon";
//            CalloutPosition = MainSpawnPoint;
//            CalloutAdvisory = "Witness reports a person walking randomly with a weapon.";
//            return base.OnBeforeCalloutDisplayed();
//        }
//        public override bool OnCalloutAccepted()
//        {
//            try
//            {
//                Game.LogTrivial("YOBBINCALLOUTS: Person With a Weapon Callout Accepted by User");
//                if (Main.CalloutInterface)
//                {
//                    CalloutInterfaceHandler.SendCalloutDetails(this, "CODE 03", ""); //For Opus' Callout Interface Plugin
//                }
//                else
//                {
//                    Game.DisplayNotification("Respond ~r~Code 03"); //If they don't have the plugin installed
//                }

//                Suspect = new Ped(MainSpawnPoint);
//                Suspect.IsPersistent = true;
//                Suspect.BlockPermanentEvents = true;
//                NativeFunction.Natives.SET_​PED_​IS_​DRUNK(Suspect, true);

//                string[] weapons = new string[]
//                {
//                    "WEAPON_CROWBAR", "" //add more melee weapons - baseball bat, knife, hatchet, golf club, bottle, etc
//                };
//                WeaponName = weapons[CallHandler.RNG(weapons.Length)];
//                Suspect.Inventory.GiveNewWeapon(WeaponName, -1, true);
//                Game.LogTrivial("YOBBINCALLOUTS: Suspect Weapon is " + WeaponName);
//                Suspect.Tasks.Wander();

//                AreaBlip = new Blip(MainSpawnPoint, 15);
//                AreaBlip.IsRouteEnabled = true;
//                AreaBlip.Color = Color.Orange;
//                AreaBlip.Alpha = 0.69f;
//            }
//            catch (Exception e) //standard error message for callout initializing
//            {
//                Game.LogTrivial("==========YOBBINCALLOUTS: ERROR CAUGHT ON CALLOUT INTIALIZATION==========");
//                Game.LogTrivial("IN: " + this);
//                string error = e.ToString();
//                Game.LogTrivial("ERROR: " + error);
//                Game.DisplayNotification("There was an ~r~Error~w~ Caught with ~b~YobbinCallouts. ~w~Please Check Your ~g~Log File.~w~ Sorry for the Inconvenience!");
//                Game.DisplayNotification("Error: ~r~" + error);
//                Game.LogTrivial("If You Believe this is a Bug, Please Report it on my Discord Server. Thanks!");
//                Game.LogTrivial("==========YOBBINCALLOUTS: ERROR CAUGHT ON CALLOUT INTIALIZATION==========");
//            }
//            if (!CalloutRunning) { Callout(); } //Call the Callout method itself
//            return base.OnCalloutAccepted();
//        }
//        public override void OnCalloutNotAccepted()
//        {
//            Game.LogTrivial("YOBBINCALLOUTS: Person With a Weapon Callout Not Accepted by User.");
//            //Functions.PlayScannerAudio("OTHER_UNIT_TAKING_CALL_02");
//            base.OnCalloutNotAccepted();
//        }
//        //For Callout logic
//        private void Callout()
//        {
//            CalloutRunning = true; //set the callout to be running
//            GameFiber.StartNew(delegate //start a new GameFiber
//            {
//                try
//                {
//                    while (CalloutRunning) //similar to lspfr's Process() method
//                    {
//                        while (player.DistanceTo(MainSpawnPoint) >= 20f && !Game.IsKeyDown(Config.CalloutEndKey)) { GameFiber.Wait(0); }
//                        if (Game.IsKeyDown(Config.CalloutEndKey)) { EndCalloutHandler.CalloutForcedEnd = true; break; }
//                        if (Main.CalloutInterface) CalloutInterfaceHandler.SendMessage(this, "Unit on Scene.");

//                        if (AreaBlip.Exists()) AreaBlip.Delete();
//                        SuspectBlip = CallHandler.AssignBlip(Suspect, Color.Red);

//                        CallHandler.RNG(0, 2569, 9069);
//                        if (Suspect.Exists() && Suspect.IsAlive)
//                        {
//                            if (MainScenario == 0) Reason();
//                            else if (MainScenario == 1) HostageScenario();
//                            else if (MainScenario == 2) Attack();
//                            else //Pursuit scenario
//                            {
//                                CallHandler.CreatePursuit(MainPursuit, true, true, true, Suspect);
//                                CallHandler.SuspectWait(Suspect);
//                            }
//                        }

//                        break; //break out of the callout loop when done
//                    }
//                    GameFiber.Wait(2000);
//                    Game.LogTrivial("YOBBINCALLOUTS: Callout Finished, Ending...");
//                    EndCalloutHandler.EndCallout(); //call the End Callout Handler (you don't need to worry about this)
//                    End();
//                }
//                catch (System.Threading.ThreadAbortException)
//                {
//                    Game.LogTrivial("==========YOBBINCALLOUTS: ERROR CAUGHT==========");
//                    Game.LogTrivial("==========Thread abort bullshit==========");
//                    Game.LogTrivial("==========YOBBINCALLOUTS: ERROR CAUGHT==========");
//                }
//                catch (Exception e) //similar error catching to before
//                {
//                    if (CalloutRunning) //if the callout is currently running
//                    {
//                        Game.LogTrivial("==========YOBBINCALLOUTS: ERROR CAUGHT==========");
//                        Game.LogTrivial("IN: " + this);
//                        string error = e.ToString();
//                        Game.LogTrivial("ERROR: " + error);
//                        Game.DisplayNotification("There was an ~r~Error~w~ Caught with ~b~YobbinCallouts. ~w~Please Check Your ~g~Log File.~w~ Sorry for the Inconvenience!");
//                        Game.DisplayNotification("Error: ~r~" + error);
//                        Game.LogTrivial("If You Believe this is a Bug, Please Report it on my Discord Server. Thanks!");
//                        Game.LogTrivial("==========YOBBINCALLOUTS: ERROR CAUGHT==========");
//                    }
//                    else //sometimes, an error will be thrown if the callout is no longer running, especially if it was forced finished early
//                    {
//                        Game.LogTrivial("==========YOBBINCALLOUTS: ERROR CAUGHT - CALLOUT NO LONGER RUNNING==========");
//                        string error = e.ToString();
//                        Game.LogTrivial("ERROR: " + error);
//                        Game.LogTrivial("No Need to Report This Error if it Did not Result in an LSPDFR Crash.");
//                        Game.LogTrivial("==========YOBBINCALLOUTS: ERROR CAUGHT - CALLOUT NO LONGER RUNNING==========");
//                    }
//                    End();
//                }
//            });
//        }
//        public override void End() //cleanup
//        {
//            base.End();
//            if (CalloutRunning) //play and display a Code 4 message iff the callout is still running
//            {
//                Game.DisplayNotification("~g~Code 4~w~, return to patrol.");
//                Functions.PlayScannerAudio("ATTENTION_ALL_UNITS WE_ARE_CODE_4");
//            }
//            CalloutRunning = false; //once this is done, set the callout to no longer running
//            if (SuspectBlip.Exists()) { SuspectBlip.Delete(); } //delete anything else
//            if (HostageBlip.Exists()) HostageBlip.Delete();
//            if (AreaBlip.Exists()) AreaBlip.Delete();
//            Game.LogTrivial("YOBBINCALLOUTS: Person With a Weapon Callout Finished Cleaning Up."); //log it
//        }
//        public override void Process() //you don't need this but if you want to use it instead of my method, go ahead
//        {
//            base.Process();
//        }
//        private void Reason()
//        {
//            Game.LogTrivial("YOBBINCALLOUTS: Reasoning with suspect.");
//            if (CalloutRunning)
//            {
//                Game.DisplayHelp("Press ~y~" + Config.MainInteractionKey + "~w~ to Reason with the ~r~Suspect.");
//                Suspect.Tasks.AimWeaponAt(player, -1);


//                if (Suspect.IsAlive)
//                {
//                    int suspectaction = CallHandler.RNG(4);
//                    if (suspectaction <= 1)
//                    {
//                        Game.LogTrivial("YOBBINCALLOUTS: Suspect reason success.");
//                        if (suspectaction == 0) CallHandler.Dialogue(ReasonSuccess1);
//                        else CallHandler.Dialogue(ReasonSuccess2);
//                        GameFiber.Wait(CallHandler.RNG(0, 2000, 5000));
//                        Suspect.Tasks.PutHandsUp(-1, player);
//                        if (Suspect.IsAlive) CallHandler.SuspectWait(Suspect);
//                    }
//                    else
//                    {
//                        Game.LogTrivial("YOBBINCALLOUTS: Suspect reason fail.");
//                        if (suspectaction == 2) CallHandler.Dialogue(ReasonFail1);
//                        else CallHandler.Dialogue(ReasonFail2);

//                        GameFiber.Wait(CallHandler.RNG(0, 2000, 5000));
//                        int suspectaction2 = CallHandler.RNG(3);
//                        if (suspectaction2 == 0) Hostage();
//                        else if (suspectaction2 == 1) Attack();
//                        else CallHandler.CreatePursuit(MainPursuit, true, true, true, Suspect);
//                    }
//                }
//            }
//        }
//        private void HostageScenario(Ped Hostage)
//        {
//            Game.LogTrivial("YOBBINCALLOUTS: Suspect attempting to take Hostage.");
//            if (CalloutRunning)
//            {
//                if (Suspect.IsAlive)
//                {
//                    var Peds = Suspect.GetNearbyPeds(10);

//                    for (int i = 0; i < Peds.Length; i++)
//                    {
//                        GameFiber.Yield();
//                        if (Peds[i].Exists() && !Peds[i].IsPlayer && !Peds[i].IsInAnyVehicle(false))
//                        {
//                            Hostage = Peds[i];
//                            break;
//                        }
//                    }

//                    if (Hostage.Exists() && Hostage.DistanceTo(Suspect) <= 20) //test distance
//                    {
//                        Game.LogTrivial("YOBBINCALLOUTS: HOSTAGE SCENARIO STARTED");
//                        Game.LogTrivial("YOBBINCALLOUTS: Hostage Location = " + Hostage.Position);
//                        Hostage.IsPersistent = true; Hostage.BlockPermanentEvents = true;
//                        //to-do: set hostage facial override (mood native)
//                        // Suspect.Tasks.GoStraightToPosition(Hostage.Position, 3f, Hostage.Heading, 1f, 5000).WaitForCompletion();
//                        Suspect.Tasks.FollowNavigationMeshToPosition(Hostage.Position, Hostage.Heading, 5.5f, 1f).WaitForCompletion();
//                        Hostage.Position = Suspect.GetOffsetPosition(new Vector3(0f, 0.14445f, 0f));
//                        System.Random rhcp = new System.Random();
//                        int WeaponModel = rhcp.Next(1, 4);
//                        Game.LogTrivial("YOBBINCALLOUTS: Suspect Weapon Model is " + WeaponModel);
//                        if (WeaponModel == 1) Suspect.Inventory.GiveNewWeapon("WEAPON_PISTOL", -1, true);
//                        else if (WeaponModel == 2) Suspect.Inventory.GiveNewWeapon("WEAPON_APPISTOL", -1, true);
//                        else if (WeaponModel == 3) Suspect.Inventory.GiveNewWeapon("WEAPON_MICROSMG", -1, true);
//                        Game.DisplaySubtitle("~r~Patient:~w~ Don't come any closer, or they'll die!!");
//                        //GameFiber.Wait(1000);

//                        Suspect.Tasks.PlayAnimation("misssagrab_inoffice", "hostage_loop", 1f, AnimationFlags.None).WaitForCompletion(500);
//                        Suspect.Tasks.PlayAnimation("misssagrab_inoffice", "hostage_loop_mrk", 1f, AnimationFlags.Loop | AnimationFlags.SecondaryTask);
//                        HostageBlip = Hostage.AttachBlip();
//                        HostageBlip.IsFriendly = true;
//                        HostageBlip.Scale = 0.69f;
//                        if (Hostage.IsFemale) Hostage.Tasks.PlayAnimation("anim@move_hostages@female", "female_idle", 1f, AnimationFlags.Loop);
//                        else Hostage.Tasks.PlayAnimation("anim@move_hostages@male", "male_idle", 1f, AnimationFlags.Loop | AnimationFlags.SecondaryTask);

//                        //This switch doesn't actually do anything, it's just a statement to break out of if the Suspect is killed prematurely in the hostage situation
//                        var lewis = 0;
//                        switch (lewis)
//                        {
//                            case 0:
//                                GameFiber.Wait(1000);
//                                Game.DisplayHelp("Press ~y~" + Config.MainInteractionKey + "~w~ to Reason with the ~o~Patient.");
//                                HostageHold();
//                                if (Suspect.IsAlive) Game.DisplaySubtitle("~g~You:~w~ " + Suspect.Forename + "! You don't have to do this! Let's talk this through!"); //was Hostage1 Dialogue
//                                else break;
//                                HostageHold();
//                                if (Suspect.IsAlive) Game.DisplaySubtitle(DialogueAdvance(Hostage2));
//                                else break;
//                                HostageHold();
//                                if (Suspect.IsAlive) Game.DisplaySubtitle(DialogueAdvance(Hostage3));
//                                else break;
//                                System.Random morsha = new System.Random();
//                                int action = morsha.Next(0, 2);
//                                Game.LogTrivial("YOBBINCALLOUTS: Suspect Action is " + WeaponModel);
//                                if (action == 0) //release
//                                {
//                                    HostageHold();
//                                    if (Suspect.IsAlive) Game.DisplaySubtitle(DialogueAdvance(Release1));
//                                    else break;
//                                    HostageHold();
//                                    if (Suspect.IsAlive) Game.DisplaySubtitle(DialogueAdvance(Release2));
//                                    else break;
//                                    System.Random zach = new System.Random();
//                                    int WaitTime = zach.Next(2000, 5000); //in ms
//                                    GameFiber.Wait(WaitTime);
//                                    Suspect.Tasks.PutHandsUp(-1, player);
//                                    if (Suspect.IsDead) break;
//                                    Game.DisplaySubtitle("~r~Patient:~w~ Okay Officer, If you say so! Just don't let them hurt me!!");
//                                    GameFiber.Wait(500);
//                                    Hostage.Tasks.ReactAndFlee(Suspect);
//                                    GameFiber.Wait(2000);
//                                    Game.DisplayHelp("Take the ~o~Patient~w~ into Custody.");
//                                    while (!Functions.IsPedArrested(Suspect)) GameFiber.Wait(0);
//                                    if (HostageBlip.Exists()) HostageBlip.Delete();
//                                    break;
//                                }
//                                else //kill
//                                {
//                                    HostageHold();
//                                    if (Suspect.IsAlive) Game.DisplaySubtitle(DialogueAdvance(Kill1));
//                                    else break;
//                                    //while (!Game.IsKeyDown(Config.MainInteractionKey) && Suspect.IsAlive) GameFiber.Wait(0);
//                                    HostageHold();
//                                    if (Suspect.IsAlive) Game.DisplaySubtitle(DialogueAdvance(Kill2));
//                                    else break;
//                                    HostageHold();
//                                    if (Suspect.IsAlive) Game.DisplaySubtitle(DialogueAdvance(Kill3));
//                                    else break;
//                                    System.Random zach = new System.Random();
//                                    int WaitTime = zach.Next(1500, 5000); //in ms
//                                    GameFiber.Wait(WaitTime);
//                                    if (Suspect.IsDead) break;
//                                    Suspect.Tasks.FireWeaponAt(Hostage, -1, FiringPattern.SingleShot).WaitForCompletion();
//                                    while (Suspect.Exists() && !Functions.IsPedArrested(Suspect) && Suspect.IsAlive) GameFiber.Wait(0);
//                                    if (HostageBlip.Exists()) HostageBlip.Delete();
//                                    if (Hostage.Exists() && Hostage.IsAlive) Hostage.Tasks.ReactAndFlee(Suspect);
//                                    break;
//                                }
//                        }
//                        //test vvv
//                        if (HostageBlip.Exists()) HostageBlip.Delete();
//                        if (Hostage.Exists() && Hostage.IsAlive) Hostage.Tasks.ReactAndFlee(player); //might remove
//                        Suspect.Tasks.PutHandsUp(5000, player);
//                        if (Functions.IsPedArrested(Suspect) || Suspect.IsAlive)
//                        {
//                            Game.DisplayNotification("Dispatch, we have taken the Patient into ~r~Custody.");
//                            GameFiber.Wait(1500);
//                            Functions.PlayScannerAudio("REPORT_RESPONSE_COPY_02");
//                            GameFiber.Wait(1500);
//                            DriveBack();
//                        }
//                        else Game.DisplayNotification("Dispatch, Suspect has been ~r~Killed.");
//                        GameFiber.Wait(2000);
//                        Functions.PlayScannerAudio("REPORT_RESPONSE_COPY_02");
//                        GameFiber.Wait(1500);
//                    }
//                    else
//                    {
//                        Game.LogTrivial("YOBBINCALLOUTS: PURSUIT SCENARIO STARTED");
//                        CallHandler.CreatePursuit(MainPursuit, true, true, true, Suspect); //test this
//                    }
//                }
//                else
//                {
//                    Game.DisplayNotification("Dispatch, Suspect has been ~r~Killed.");
//                }
//            }
//            else CallHandler.CreatePursuit(MainPursuit, true, true, true, Suspect); //test this
//        }

//        private void Attack()
//        {
//            Game.LogTrivial("YOBBINCALLOUTS: Suspect Attacks.");
//            if (CalloutRunning)
//            {

//            }
//        }
//    }
//}
