using System;
using Rage;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage.Native;
using System.Collections.Generic;
using System.Drawing;

namespace YobbinCallouts.Callouts
{
    [CalloutInfo("Weapon Found", CalloutProbability.High)]
    class WeaponFound : Callout
    {
        public Vector3 MainSpawnPoint;
        private Vector3 House;

        private Ped Suspect;
        private Vehicle SuspectVehicle;
        private Blip SuspectBlip;
        private Blip WitnessBlip;
        private Blip WeaponBlip;
        private Ped player = Game.LocalPlayer.Character;
        private Ped Witness;
        private LHandle MainPursuit;
        private Rage.Object Weapon;
        public string WeaponName; //not displaying properly in dialogue
        public static string OriginalZone = "not far from here.";

        private int MainScenario;
        private bool CalloutRunning;

        //DIALOGUE VvvV
        private readonly List<string> WitnessOpeningSuspectNotFound1 = new List<string>()
        {
         "~g~You:~w~ Hello, are you the caller?",
         "~b~Witness:~w~ Yes I am, officer. I was just walking down the street and noticed this ~r~Firearm.",
         "~b~Witness:~w~ It looks like it has some blood on it to. I'm obviously really concerned so called you guys.",
         "~g~You:~w~ For sure. Did you see who dropped it or when?",
         "~b~Witness:~w~ Unfortunately I didn't, officer. I was literally just walking down here and saw the weapon on the side of the road.",
         "~g~You:~w~ Alright, I'll collect this weapon and get it over to evidence. Thanks for your help!",
          "~b~Witness:~w~ No worries, stay safe officer!",
        };
        private readonly List<string> WitnessOpeningSuspectNotFound2 = new List<string>()
        {
         "~g~You:~w~ Hello, did you call us?",
         "~b~Witness:~w~ Yes I did, officer. I was just going for a stroll when I noticed this ~r~Gun.",
         "~b~Witness:~w~ It seems like there's some blood on it as well. I'm obviously really concerned so called 9-1-1.",
         "~g~You:~w~ Absolutely. Did you witness anyone drop it? Maybe someone threw it out of a vehicle?",
         "~b~Witness:~w~ Unfortunately I didn't see anything, officer. I was just walking down this street and saw the weapon on the side of the road.",
         "~g~You:~w~ Alright, I'll take this weapon to evidence. Thanks for your help!",
          "~b~Witness:~w~ No worries, take care officer!",
        };
        private readonly List<string> WitnessOpeningSuspectNotFound3 = new List<string>()
        {
         "~b~Witness:~w~ Hey officer, over here!",
         "~g~You:~w~ Are you the caller?",
         "~b~Witness:~w~ Yes I am, officer. I was just walking to the store when I noticed this ~r~Pistol.",
         "~b~Witness:~w~ It seems like there's some blood on it as well. I was obviously really scared so called 9-1-1.",
         "~g~You:~w~ Well, I'm really glad you called us. Did you see anything? Maybe someone threw it out of a vehicle?",
         "~b~Witness:~w~ Unfortunately I didn't, officer. I was just walking here and stumbled upon the weapon.",
         "~g~You:~w~ Alright, I'll take this weapon to evidence. Thanks again for your help!",
          "~b~Witness:~w~ Of course, take care officer!",
        };
        private readonly List<string> WitnessOpeningSuspectClose1 = new List<string>()
        {
         "~b~Witness:~w~ Hey officer, over here!",
         "~g~You:~w~ Are you the caller?",
         "~b~Witness:~w~ Yes I am, officer. I was just walking to the store when I saw this guy throw this gun out of his car window!",
         "~b~Witness:~w~ He Drove off down the street! He shouldn't be far!",
         "~g~You:~w~ Alright, I'll start looking for them, Thanks!",
        };
        private readonly List<string> WitnessOpeningSuspectClose2 = new List<string>()
        {
         "~b~Witness:~w~ Officer, over here!",
         "~g~You:~w~ Did you call us? Something about finding a weapon?",
         "~b~Witness:~w~ Yes I did, officer. I was just walking down the street when I saw this guy chuck this gun out of his car window!",
         "~b~Witness:~w~ He Drove off down the street! He couldn't have gotten far!",
         "~g~You:~w~ Alright, I'll start looking for them, Thanks!",
        };
        private readonly List<string> WitnessOpeningSuspectClose3 = new List<string>()
        {
         "~b~Witness:~w~ Hey Officer!",
         "~g~You:~w~ Are you the caller?",
         "~b~Witness:~w~ Yes I am, officer. I was just walking down the street to the store when I saw someone throw this gun out of his car window!",
         "~b~Witness:~w~ He took off down the street! He shouldn't be far from here!",
         "~g~You:~w~ Alright, I'll start looking for them, Thanks!",
        };
        private readonly List<string> WitnessOpeningSuspectClose4 = new List<string>()
        {
         "~b~Witness:~w~ Hey officer, over here!",
         "~g~You:~w~ Are you the caller?",
         "~b~Witness:~w~ Yes I am, officer. I was just walking to the store when this guy came running past me and dropped this gun!",
         "~b~Witness:~w~ He went off down the street! He shouldn't be far!",
         "~g~You:~w~ Alright, I'll start looking for them, Thanks!",
        };
        private readonly List<string> WitnessOpeningSuspectClose5 = new List<string>()
        {
         "~b~Witness:~w~ Over here Officer!",
         "~g~You:~w~ Are you the caller?",
         "~b~Witness:~w~ Yes I am, officer. I was going for a stroll when I saw someone run past me and drop this gun from his bag!",
         "~b~Witness:~w~ He ran off down the street! He shouldn't be far!",
         "~g~You:~w~ Alright, I'll start looking for them, Thanks!",
        };
        private List<string> SuspectInnocent1 = new List<string>() //vvv removed readonly for testing
        {
            "~o~Suspect:~w~ Hi Officer, what seems to be the issue? Is everything alright?",
            $"~g~You:~w~ Hello, I'm here because a weapon registered to you was recovered {OriginalZone}~w~.",
            "~o~Suspect:~w~ Oh shit, I was worried about that. I can explain everything, Officer!",
            "~o~Suspect:~w~ I have my CCW and sometimes keep my handgun in my car. Last week, someone broke in and stole it!",
            "~o~Suspect:~w~ I reported it stolen, but I was advised it might take a bit to show up in the system. I'm very sorry for the confusion!",
            "~g~You:~w~ Alright, thanks for the cooperation. I'll look into this, hang tight for me here.",
        };
        private List<string> SuspectInnocent2 = new List<string>()
        {
            "~o~Suspect:~w~ Hi Officer, is everything alright?",
            $"~g~You:~w~ Well not exactly, I'm here because a weapon registered to you was recovered {OriginalZone}~w~.",
            "~o~Suspect:~w~ Oh shit, I was hoping something like that wouldn't happen. I promise I can explain everything!",
            "~o~Suspect:~w~ I have my CCW and store my weapon in the glove compartment of my vehicle.",
            "~o~Suspect:~w~ A few days ago, my car was broken into and my gun was stolen.",
            "~o~Suspect:~w~ I reported it stolen, but I was told it might take a bit to show up in the system. I'm very sorry for the confusion!",
            "~g~You:~w~ Alright, thanks for the explanation. I'll look into this, hang tight for me here.",
        };
        private List<string> SuspectInnocent3 = new List<string>()
        {
            "~o~Suspect:~w~ Hi Officer, what seems to be going on here? Is everything okay?",
            $"~g~You:~w~ Well not exactly, a weapon registered to the homeowner of this house was recovered {OriginalZone}~w~.",
            "~o~Suspect:~w~ Oh no, I was hoping this wouldn't end up happening. I promise I can explain everything!",
            "~o~Suspect:~w~ I have my Concealed Permit, and usually keep my handgun in my home.",
            "~o~Suspect:~w~ A couple days ago, my house was broken into and my gun was stolen.",
            "~o~Suspect:~w~ I reported it stolen, but the Officer told me it might take a while to show up in the system. I'm so sorry for the confusion!",
            "~g~You:~w~ Alright, thanks for the explanation. I'll take a look into this, hang tight for me here.",
        };
        private List<string> SuspectGuilty1 = new List<string>()
        {
            "~o~Suspect:~w~ Hi Officer, what seems to be the issue? Is everything alright?",
            $"~g~You:~w~ Hello, I'm here because a weapon registered to you was recovered {OriginalZone}~w~.",
            "~o~Suspect:~w~ Oh shit, uh - wow! I don't know anything about that Officer, I don't know why that would flag me.",
            "~g~You:~w~ So you don't know anything about a firearm that was discovered on the street registered to you?",
            "~o~Suspect:~w~ No idea Officer, I've never owned a gun. Now am I free to go? I haven't done anything wrong.",
            "~g~You:~w~ Okay, I'll look into this, hang tight for me here.",
        };
        private List<string> SuspectGuilty2 = new List<string>()
        {
            "~o~Suspect:~w~ Oh, Officer! What are you doing here? Is everything alright?",
            $"~g~You:~w~ Hello, I'm here because a weapon registered to you was discovered over {OriginalZone}~w~.",
            "~o~Suspect:~w~ Oh, uh - wow! I don't know anything about that Officer, I've never even shot a gun let alone owned one.",
            "~g~You:~w~ So you don't know anything about a firearm that was discovered on the street that happened to be registered to you?",
            "~o~Suspect:~w~ No idea Officer, aren't you supposed to know that? Now am I free to go?",
            "~g~You:~w~ Okay, I'll look into this, hang tight for me here. You're not free to go at the moment, no.",
        };
        private List<string> SuspectFlees = new List<string>()
        {
            "~o~Suspect:~w~ Oh, Officer! What are you doing here? Is everything alright?",
            $"~g~You:~w~ Hello, I'm here because a weapon registered to you was discovered over {OriginalZone}~w~.",
        };

        public override bool OnBeforeCalloutDisplayed()
        {
            Game.LogTrivial("==========YOBBINCALLOUTS: Weapon Found Callout Start==========");
            System.Random r = new System.Random();
            int Scenario = r.Next(0, 2); //scenario 1 is messed up, suspect invisible lol
            MainScenario = Scenario;
            Game.LogTrivial("YOBBINCALLOUTS: Scenario value is: " + MainScenario);
            var zone = Functions.GetZoneAtPosition(Game.LocalPlayer.Character.Position).RealAreaName;
            Game.LogTrivial("YOBBINCALLOUTS: Zone is " + zone);

            MainSpawnPoint = World.GetNextPositionOnStreet(player.Position.Around(550));
            ShowCalloutAreaBlipBeforeAccepting(MainSpawnPoint, 25f);
            AddMinimumDistanceCheck(60f, MainSpawnPoint);
            Functions.PlayScannerAudio("CITIZENS_REPORT A_01 YC_DEADLYWEAPON");
            CalloutMessage = "Weapon Found";
            CalloutPosition = MainSpawnPoint;
            if (MainScenario >= 0) CalloutAdvisory = "A Caller Has Reportedly Discovered a ~r~Firearm~w~.";
            else CalloutAdvisory = "A Caller Has Reportedly Discovered a ~r~Melee Weapon~w~.";
            return base.OnBeforeCalloutDisplayed();
        }
        public override bool OnCalloutAccepted()
        {
            try
            {
                Game.LogTrivial("YOBBINCALLOUTS: Weapon Found Callout Accepted by User");
                if (Main.CalloutInterface)
                {
                    CalloutInterfaceHandler.SendCalloutDetails(this, "CODE 2", "");
                }
                else
                {
                    Game.DisplayNotification("Respond ~b~Code 2");
                }

                NativeFunction.Natives.xA0F8A7517A273C05<bool>(World.GetNextPositionOnStreet(MainSpawnPoint), 0, out Vector3 outPosition);
                Witness = new Ped(outPosition, 69);  //might have to specify witness model
                Witness.IsPersistent = true;
                Witness.BlockPermanentEvents = true;
                WitnessBlip = Witness.AttachBlip();
                WitnessBlip.IsFriendly = true;
                WitnessBlip.IsRouteEnabled = true;
                WitnessBlip.Name = "Witness";
                Vector3 dir = (player.Position - Witness.Position); //thanks Albo!
                Witness.Tasks.AchieveHeading(MathHelper.ConvertDirectionToHeading(dir)).WaitForCompletion(1100);
                Witness.Tasks.PlayAnimation("friends@frj@ig_1", "wave_a", 1.1f, AnimationFlags.Loop);

                if (MainScenario >= 1)
                {
                    Suspect = new Ped(Witness.Position.Around(250), 69);
                    Suspect.IsPersistent = true;
                    Suspect.BlockPermanentEvents = true;
                    Suspect.Tasks.Wander();
                    Suspect.RelationshipGroup.SetRelationshipWith(RelationshipGroup.Cop, Relationship.Hate);
                    Suspect.RelationshipGroup.SetRelationshipWith(RelationshipGroup.Player, Relationship.Hate);
                    Suspect.RelationshipGroup.SetRelationshipWith(RelationshipGroup.AmbientFriendEmpty, Relationship.Hate);
                }

                if (MainScenario >= 0) //gun - SCENARIO 0 MUST HAVE GUN
                {
                    System.Random r = new System.Random();
                    int WeaponType = r.Next(0, 4);
                    if (WeaponType == 0) { Weapon = new Rage.Object("w_pi_appistol", World.GetNextPositionOnStreet(Witness.Position)); WeaponName = "AP Pistol"; }
                    else if (WeaponType == 1) { Weapon = new Rage.Object("w_pi_combatpistol", World.GetNextPositionOnStreet(Witness.Position)); WeaponName = "Combat Pistol"; }
                    else if (WeaponType == 2) { Weapon = new Rage.Object("w_pi_heavypistol", World.GetNextPositionOnStreet(Witness.Position)); WeaponName = "Heavy Pistol"; }
                    else if (WeaponType == 3) { Weapon = new Rage.Object("w_pi_pistol", World.GetNextPositionOnStreet(Witness.Position)); WeaponName = "Pistol"; }
                }
                //else //melee
                //{
                //    System.Random r = new System.Random();
                //    int WeaponType = r.Next(0, 4);
                //    if (WeaponType == 0) { Weapon = new Rage.Object("w_me_knife", World.GetNextPositionOnStreet(Witness.Position)); WeaponName = "Knife"; }
                //    else if (WeaponType == 1) { Weapon = new Rage.Object("w_me_hatchet", World.GetNextPositionOnStreet(Witness.Position)); WeaponName = "Hatchet"; }
                //    else if (WeaponType == 2) { Weapon = new Rage.Object("w_me_dagger", World.GetNextPositionOnStreet(Witness.Position)); WeaponName = "Dagger"; }
                //    else if (WeaponType == 3) { Weapon = new Rage.Object("prop_cs_bowie_knife", World.GetNextPositionOnStreet(Witness.Position)); WeaponName = "Bowie Knife"; }
                //}
                Game.LogTrivial("YOBBINCALLOUTS: Weapon is " + WeaponName);
                Weapon.IsPersistent = true;
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
            Game.LogTrivial("YOBBINCALLOUTS: Weapon Found Callout Not Accepted by User.");
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
                        while (player.DistanceTo(Witness) >= 20 && !Game.IsKeyDown(Config.CalloutEndKey)) { GameFiber.Wait(0); }
                        if (Game.IsKeyDown(Config.CalloutEndKey)) { EndCalloutHandler.CalloutForcedEnd = true; break; }
                        if (Main.CalloutInterface) CalloutInterfaceHandler.SendMessage(this, "Unit on Scene.");
                        WitnessFirst();
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
            if (WitnessBlip.Exists()) { WitnessBlip.Delete(); }
            if (WeaponBlip.Exists()) { WeaponBlip.Delete(); }
            if (Suspect.Exists()) Suspect.Dismiss();
            if (Witness.Exists()) Witness.Dismiss();
            Game.LogTrivial("YOBBINCALLOUTS: Weapon Found Callout Finished Cleaning Up.");
        }
        public override void Process()
        {
            base.Process();
        }
        private void WitnessFirst()
        {
            if (Config.DisplayHelp) Game.DisplayHelp("Speak with the ~b~Witness.");
            WitnessBlip.IsRouteEnabled = false;
            CallHandler.IdleAction(Witness, false);
            while (player.DistanceTo(Witness) >= 6f) GameFiber.Wait(0);
            Witness.Tasks.AchieveHeading(player.Heading - 180).WaitForCompletion(500);
            if (Config.DisplayHelp) Game.DisplayHelp("Press ~y~" + Config.MainInteractionKey + " ~w~to Speak with the ~b~Witness.");
            System.Random r = new System.Random();
            int Dialogue = r.Next(0, 3);

            if (MainScenario == 0) //No Info
            {
                if (Dialogue == 0) CallHandler.Dialogue(WitnessOpeningSuspectNotFound1, Witness);
                else if (Dialogue == 1) CallHandler.Dialogue(WitnessOpeningSuspectNotFound2, Witness);
                else CallHandler.Dialogue(WitnessOpeningSuspectNotFound3, Witness);

                if (WitnessBlip.Exists()) WitnessBlip.Delete();
                Witness.Tasks.ClearImmediately();
                if (Witness.Exists()) Witness.Dismiss();
                CollectWeapon();
            }
            else //Suspect CLose
            {
                System.Random r3 = new System.Random();  //Instantiate Random Weapon  generator
                int WeaponModel = r3.Next(0, 5);    //Use Random Weapon generator
                Game.LogTrivial("YOBBINCALLOUTS: Suspect Weapon Model is " + WeaponModel);

                if (WeaponModel == 0) { Suspect.Inventory.GiveNewWeapon("WEAPON_ASSAULTRIFLE", -1, true); WeaponName = "Assault Rifle"; }
                else if (WeaponModel == 1) { Suspect.Inventory.GiveNewWeapon("WEAPON_SMG", -1, true); WeaponName = "SMG"; }
                else if (WeaponModel == 2) { Suspect.Inventory.GiveNewWeapon("WEAPON_APPISTOL", -1, true); WeaponName = "Pistol"; }
                else if (WeaponModel == 3) { Suspect.Inventory.GiveNewWeapon("WEAPON_MICROSMG", -1, true); WeaponName = "SMG"; }
                else if (WeaponModel == 4) { Suspect.Inventory.GiveNewWeapon("WEAPON_COMPACTRIFLE", -1, true); WeaponName = "Rifle"; }

                if (MainScenario == 1) //vehicle
                {
                    if (Dialogue == 0) CallHandler.Dialogue(WitnessOpeningSuspectClose1, Witness);
                    else if (Dialogue == 1) CallHandler.Dialogue(WitnessOpeningSuspectClose2, Witness);
                    else CallHandler.Dialogue(WitnessOpeningSuspectClose3, Witness);
                    GameFiber.Wait(2000);
                    Witness.Tasks.Clear();
                    if (WitnessBlip.Exists()) WitnessBlip.Delete();
                    Witness.Tasks.ClearImmediately();
                    if (Witness.Exists()) Witness.Dismiss();

                    SuspectVehicle = CallHandler.SpawnVehicle(World.GetNextPositionOnStreet(Suspect.Position), 69);
                    SuspectVehicle.IsPersistent = true;
                    Suspect.WarpIntoVehicle(SuspectVehicle, -1);
                    Suspect.Tasks.CruiseWithVehicle(15f, VehicleDrivingFlags.AllowWrongWay | VehicleDrivingFlags.DriveAroundVehicles);
                    if (Main.CalloutInterface) CalloutInterfaceHandler.SendMessage(this, "Suspect is Driving a ~r~" + SuspectVehicle.Model.Name + "~w~ with Plate ~y~" + SuspectVehicle.LicensePlate);
                    Game.DisplayNotification("Suspect is Driving a ~r~" + SuspectVehicle.Model.Name + "~w~ with Plate ~y~" + SuspectVehicle.LicensePlate);
                    SuspectBlip = Suspect.AttachBlip();
                    SuspectBlip.IsFriendly = false;
                    SuspectBlip.Scale = 0.69f;
                    while (player.DistanceTo(Suspect) >= 20) GameFiber.Wait(0);
                    SuspectDecisions();
                }
                else //on foot
                {
                    if (Dialogue == 4) CallHandler.Dialogue(WitnessOpeningSuspectClose4, Witness);
                    else CallHandler.Dialogue(WitnessOpeningSuspectClose5, Witness);
                    Witness.Tasks.Clear();
                    if (WitnessBlip.Exists()) WitnessBlip.Delete();
                    Witness.Tasks.ClearImmediately();
                    if (Witness.Exists()) Witness.Dismiss();

                    Suspect.IsVisible = true;
                    //Suspect.Tasks.Flee(player, 100f, -1);
                    Suspect.Tasks.Wander();
                    SuspectBlip = Suspect.AttachBlip();
                    SuspectBlip.IsFriendly = false;
                    SuspectBlip.Scale = 0.69f;
                    while (player.DistanceTo(Suspect) >= 15) GameFiber.Wait(0);

                    System.Random monke = new System.Random();
                    int decision = monke.Next(0, 2);
                    if (decision == 1) CallHandler.CreatePursuit(MainPursuit, true, true, true, Suspect);
                    else
                    {
                        System.Random yuy = new System.Random();
                        int WaitTime = yuy.Next(1500, 6000);
                        GameFiber.Wait(WaitTime);
                        if (Suspect.Exists() && Suspect.IsAlive)
                        {
                            Suspect.Tasks.FightAgainst(player, -1);
                        }
                        while (Suspect.Exists())
                        {
                            GameFiber.Yield();
                            if (!Suspect.Exists() || Suspect.IsDead || Functions.IsPedArrested(Suspect)) break;
                        }
                        if (Suspect.Exists())
                        {
                            if (Functions.IsPedArrested(Suspect)) { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect is Under ~g~Arrest~w~."); }
                            else { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect Was ~r~Killed~w~."); }
                        }
                        else
                        {
                            GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect is Under ~g~Arrest.");
                        }
                    }
                }
            }
        }
        private void CollectWeapon()
        {
            if (Witness.Exists()) Witness.Dismiss();
            Game.DisplayHelp("Locate the ~p~Weapon.~w~ Press ~y~" + Config.MainInteractionKey + " ~w~to Collect it.");
            WeaponBlip = Weapon.AttachBlip();
            WeaponBlip.Scale = 0.35f;
            WeaponBlip.Color = Color.Purple;
            while (player.DistanceTo2D(Weapon) >= 0.65f) GameFiber.Wait(0);
            while (Game.IsKeyDown(Config.MainInteractionKey)) GameFiber.Wait(0);
            Weapon.AttachTo(player, player.GetBoneIndex(PedBoneId.RightHand), new Vector3(0.1f, 0.03f, 0f), new Rotator(-71f, 0f, 0f)); //use menyoo to find the right vector/rotator
            if (WeaponBlip.Exists()) WeaponBlip.Delete();

            if (player.LastVehicle.Exists())
            {
                var lastvehicle = player.LastVehicle;
                Game.DisplayHelp("Return to Your ~g~Vehicle~w~ to Run the Serial Number of the ~p~Weapon.");
                var lastvehicleblip = lastvehicle.AttachBlip();
                while (!player.IsInAnyPoliceVehicle) GameFiber.Wait(0);
                if (lastvehicleblip.Exists()) lastvehicleblip.Delete();
                if (Weapon.Exists()) Weapon.Delete();
            }
            GameFiber.Wait(2000);
            System.Random r3 = new System.Random();
            var WeaponSerial = r3.Next(50000000, 99999999);
            Game.LogTrivial("YOBBINCALLOUTS: Checking Weapon Serial.");
            Game.DisplaySubtitle("Dispatch, Requesting Weapon Serial Check for a ~r~" + WeaponName + " ~w~with Serial Number ~b~" + WeaponSerial);
            if (Main.CalloutInterface) CalloutInterfaceHandler.SendMessage(this, "Dispatch, Requesting Weapon Serial Check for a ~r~" + WeaponName + " with Serial Number ~b~" + WeaponSerial);
            GameFiber.Wait(3000);

            if (WeaponSerial < 50000000) //HIT //change to 50000000 later! always hit rn
            {
                Game.LogTrivial("YOBBINCALLOUTS: Weapon Serial Check = Hit.");
                SuspectVehicle = CallHandler.SpawnVehicle(World.GetNextPositionOnStreet(player.Position.Around(420f)), 69);
                SuspectVehicle.IsPersistent = true;
                Suspect = SuspectVehicle.CreateRandomDriver();
                Suspect.IsPersistent = true; Suspect.BlockPermanentEvents = true;
                var SuspectName = Functions.GetPersonaForPed(Suspect).FullName; //test this
                var Distance = Math.Round(Suspect.DistanceTo(player));
                Game.DisplayNotification(WeaponName + " Serial ~r~" + WeaponSerial + " ~w~Registered to ~p~" + SuspectName + "~w~. ~r~Suspect~w~ was Recently ~r~Located~o~ " + Distance + " metres~w~ Away!");
                if (Main.CalloutInterface) CalloutInterfaceHandler.SendMessage(this, WeaponName + " Serial ~r~" + WeaponSerial + " ~w~Registered to ~p~" + SuspectName + "~w~. ~r~Suspect~w~ was Recently ~r~Located~o~ " + Distance + " metres~w~ Away!");
                Game.DisplayNotification("Suspect is Driving a ~r~" + SuspectVehicle.Model.Name + "~w~ with Plate ~y~" + SuspectVehicle.LicensePlate);
                if (Main.CalloutInterface) CalloutInterfaceHandler.SendMessage(this, "Suspect is Driving a ~r~" + SuspectVehicle.Model.Name + "~w~ with Plate ~y~" + SuspectVehicle.LicensePlate);
                Search();
            }
            else //NO HIT
            {
                Game.LogTrivial("YOBBINCALLOUTS: Weapon Serial Check = NO Hit.");
                CallHandler.locationChooser(CallHandler.HouseList);
                if (CallHandler.locationReturned)
                {
                    House = CallHandler.SpawnPoint;
                    Game.LogTrivial("YOBBINCALLOUTS: House Found.");
                    //NativeFunction.Natives.xA0F8A7517A273C05<bool>(World.GetNextPositionOnStreet(House), 0, out Vector3 SuspectVehicleSpawnPoint);
                    //NativeFunction.Natives.GetClosestVehicleNodeWithHeading(SuspectVehicleSpawnPoint, out Vector3 nodePosition, out float heading, 1, 3.0f, 0);
                    //SuspectVehicle = CallHandler.SpawnVehicle(SuspectVehicleSpawnPoint, (int)heading);
                    //SuspectVehicle.IsPersistent = true;
                    //Suspect = SuspectVehicle.CreateRandomDriver();

                    Suspect = new Ped(House);
                    Suspect.IsPersistent = true; Suspect.BlockPermanentEvents = true;
                    var SuspectName = Functions.GetPersonaForPed(Suspect).FullName;
                    var Distance = Math.Round(Suspect.DistanceTo(player));
                    Game.DisplayNotification(WeaponName + " Serial ~r~" + WeaponSerial + " ~w~Registered to ~p~" + SuspectName + "~w~. Owner~w~ Lives in ~b~" + Functions.GetZoneAtPosition(House).RealAreaName + "~o~ " + Distance + " metres~w~ Away!");
                    if (Main.CalloutInterface) CalloutInterfaceHandler.SendMessage(this, WeaponName + " Serial ~r~" + WeaponSerial + " ~w~Registered to ~p~" + SuspectName + "~w~. Owner~w~ Lives in ~b~" + Functions.GetZoneAtPosition(House).RealAreaName + "~o~ " + Distance + " metres~w~ Away!");
                    Suspect.Position = House;
                    Suspect.IsVisible = false;
                    GameFiber.Wait(1500);
                    Game.DisplayHelp("Drive to the ~o~House~w~ of the ~r~Gun Owner~w~ in ~y~" + Functions.GetZoneAtPosition(House).RealAreaName);
                    WitnessBlip = new Blip(House, 20);
                    WitnessBlip.IsRouteEnabled = true;
                    WitnessBlip.Color = Color.Orange;
                    WitnessBlip.Alpha = 0.69f;
                    while (player.DistanceTo(House) >= 20) GameFiber.Wait(0);
                    if (WitnessBlip.Exists()) WitnessBlip.Delete();
                    WitnessBlip = new Blip(House);
                    WitnessBlip.Color = Color.Orange;
                    WitnessBlip.Alpha = 1f;
                    while (player.DistanceTo(House) >= 3.5f) GameFiber.Wait(0);

                    Game.DisplayHelp("Press ~y~" + Config.MainInteractionKey + "~w~ to ~b~Ring~w~ the Doorbell.");
                    while (!Game.IsKeyDown(Config.MainInteractionKey)) GameFiber.Wait(0);
                    CallHandler.Doorbell();
                    GameFiber.Wait(2000);
                    Game.LocalPlayer.HasControl = false;
                    Game.FadeScreenOut(1500, true);
                    Suspect = new Ped(House, player.Heading - 180);
                    Suspect.IsPersistent = true;
                    CallHandler.IdleAction(Suspect, false);
                    if (WitnessBlip.Exists()) WitnessBlip.Delete();
                    SuspectBlip = Suspect.AttachBlip();
                    SuspectBlip.IsFriendly = false;
                    SuspectBlip.Scale = 0.69f;
                    GameFiber.Wait(1500);
                    Game.FadeScreenIn(1500, true);
                    Game.LocalPlayer.HasControl = true;
                    Game.DisplayHelp("Press ~y~" + Config.MainInteractionKey + "~w~ to speak with the ~o~Resident.");
                    Game.LogTrivial("YOBBINCALLOUTS: Started speaking with suspect.");
                    System.Random dud = new System.Random();
                    var decision = dud.Next(0, 4);
                    Game.LogTrivial("YOBBINCALLOUTS: Suspect Action is " + decision);
                    if (decision == 0) //cooperates
                    {
                        Game.LogTrivial("YOBBINCALLOUTS: suspect cooperative");
                        System.Random yud = new System.Random();
                        var dialogue = yud.Next(0, 3);
                        if (dialogue == 0) CallHandler.Dialogue(SuspectInnocent1, Suspect);
                        else if (dialogue == 1) CallHandler.Dialogue(SuspectInnocent2, Suspect);
                        else CallHandler.Dialogue(SuspectInnocent3, Suspect);
                        GameFiber.Wait(2000);
                        CallHandler.IdleAction(Suspect, false);
                        Game.DisplayHelp("Deal with the ~o~Suspect~w~ as you see fit. Press ~y~" + Config.CalloutEndKey + " ~w~when ~b~Done.~w~");
                        while (!Game.IsKeyDown(Config.CalloutEndKey)) GameFiber.Wait(0);
                    }
                    else if (decision == 1) //not cooperative
                    {
                        Game.LogTrivial("YOBBINCALLOUTS: suspect not cooperative");
                        System.Random yud = new System.Random();
                        var dialogue = yud.Next(0, 2);
                        if (dialogue == 0) CallHandler.Dialogue(SuspectGuilty1, Suspect);
                        else CallHandler.Dialogue(SuspectGuilty1, Suspect);
                        GameFiber.Wait(2000);
                        CallHandler.IdleAction(Suspect, false);
                        Game.DisplayHelp("Deal with the ~o~Suspect~w~ as you see fit. Press ~y~" + Config.CalloutEndKey + " ~w~when ~b~Done.~w~");
                        while (!Game.IsKeyDown(Config.CalloutEndKey)) GameFiber.Wait(0);
                    }
                    else if (decision == 2) //late
                    {
                        System.Random r = new System.Random();  //Instantiate Random Weapon  generator
                        int WeaponModel = r.Next(0, 5);    //Use Random Weapon generator

                        System.Random yud = new System.Random();
                        var dialogue = yud.Next(0, 2);
                        if (dialogue == 0) CallHandler.Dialogue(SuspectGuilty1, Suspect);
                        else CallHandler.Dialogue(SuspectGuilty1, Suspect);
                        CallHandler.IdleAction(Suspect, false);
                        Game.DisplayHelp("Return to your ~p~Vehicle~w~ to conduct a ~o~Ped Check.");
                        System.Random rondom = new System.Random();  //Instantiate Random WaitTime generator
                        int WaitTime = rondom.Next(3000, 8000);    //Use Random WaitTime generator
                        Game.LogTrivial("YOBBINCALLOUTS: Waiting " + WaitTime + " ms.");
                        GameFiber.Wait(WaitTime);

                        if (Suspect.Exists() && Suspect.IsAlive && !Functions.IsPedArrested(Suspect))
                        {
                            System.Random dec = new System.Random();
                            var action = dec.Next(0, 2);
                            if (action == 0) CallHandler.CreatePursuit(MainPursuit, true, true, true, Suspect);  //flee
                            else //fight
                            {
                                Game.LogTrivial("YOBBINCALLOUTS: Suspect Weapon Model is " + WeaponModel);
                                if (WeaponModel == 0) Suspect.Inventory.GiveNewWeapon("WEAPON_UNARMED", -1, true);
                                else if (WeaponModel == 1) Suspect.Inventory.GiveNewWeapon("WEAPON_PISTOL", -1, true);
                                else if (WeaponModel == 2) Suspect.Inventory.GiveNewWeapon("WEAPON_APPISTOL", -1, true);
                                else if (WeaponModel == 3) Suspect.Inventory.GiveNewWeapon("WEAPON_MICROSMG", -1, true);
                                else if (WeaponModel == 4) Suspect.Inventory.GiveNewWeapon("WEAPON_CROWBAR", -1, true);
                                Game.LogTrivial("YOBBINCALLOUTS: Suspect fight.");
                                Suspect.Tasks.FightAgainst(player, -1);
                                //LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("CRIME_ASSAULT_PEACE_OFFICER_01");
                                LSPD_First_Response.Mod.API.Functions.RequestBackup(Suspect.Position, LSPD_First_Response.EBackupResponseType.Code3, LSPD_First_Response.EBackupUnitType.LocalUnit);
                                while (Suspect.Exists() && Suspect.IsAlive) GameFiber.Wait(0);
                                if (Suspect.IsDead) { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, Suspect Was ~r~Killed~w~ Trying to Assault an Officer."); SuspectBlip.Delete(); }
                                else { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, Suspect is Under ~g~Arrest~w~ For Trying to Assault an Officer."); }
                                GameFiber.Wait(2000);
                                LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("REPORT_RESPONSE_COPY_02");
                            }
                        }
                    }
                    else //early
                    {
                        CallHandler.Dialogue(SuspectFlees, Suspect);
                        System.Random dec = new System.Random();
                        var action = dec.Next(0, 2);
                        if (action == 0) CallHandler.CreatePursuit(MainPursuit, true, true, true, Suspect);  //flee
                        else //fight
                        {
                            System.Random r = new System.Random();  //Instantiate Random Weapon  generator
                            int WeaponModel = r.Next(0, 5);    //Use Random Weapon generator
                            Game.LogTrivial("YOBBINCALLOUTS: Suspect Weapon Model is " + WeaponModel);
                            if (WeaponModel == 0) Suspect.Inventory.GiveNewWeapon("WEAPON_UNARMED", -1, true);
                            else if (WeaponModel == 1) Suspect.Inventory.GiveNewWeapon("WEAPON_PISTOL", -1, true);
                            else if (WeaponModel == 2) Suspect.Inventory.GiveNewWeapon("WEAPON_APPISTOL", -1, true);
                            else if (WeaponModel == 3) Suspect.Inventory.GiveNewWeapon("WEAPON_MICROSMG", -1, true);
                            else if (WeaponModel == 4) Suspect.Inventory.GiveNewWeapon("WEAPON_CROWBAR", -1, true);
                            Game.LogTrivial("YOBBINCALLOUTS: Suspect fight.");
                            Suspect.Tasks.FightAgainst(player, -1);
                            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("CRIME_ASSAULT_PEACE_OFFICER_01");
                            LSPD_First_Response.Mod.API.Functions.RequestBackup(Suspect.Position, LSPD_First_Response.EBackupResponseType.Code3, LSPD_First_Response.EBackupUnitType.LocalUnit);
                            while (Suspect.Exists() && Suspect.IsAlive) GameFiber.Wait(0);
                            if (Suspect.IsDead) { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, Suspect Was ~r~Killed~w~ Trying to Assault an Officer."); SuspectBlip.Delete(); }
                            else { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, Suspect is Under ~g~Arrest~w~ For Trying to Assault an Officer."); }
                            GameFiber.Wait(2000);
                            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("REPORT_RESPONSE_COPY_02");
                        }
                    }
                }
                else
                {
                    Game.LogTrivial("YOBBINCALLOUTS: House NOT Found.");
                    Game.DisplayNotification(WeaponName + " Serial ~r~" + WeaponSerial + " ~w~did ~r~Not~w~ Match Any ~b~Entries~w~ in the Database.");
                    if (Main.CalloutInterface) CalloutInterfaceHandler.SendMessage(this, WeaponName + " Serial ~r~" + WeaponSerial + " ~w~did ~r~Not~w~ Match Any ~b~Entries~w~ in the Database.");
                }
            }
        }
        private void Search()
        {
            if (CalloutRunning)
            {
                Game.DisplayHelp("Start ~o~Searching~w~ for the ~r~Suspect.");
                SuspectBlip = new Blip(Suspect.Position.Around(15), 150f);
                SuspectBlip.Color = Color.Orange;
                SuspectBlip.Alpha = 0.5f;
                SuspectBlip.IsRouteEnabled = true;

                System.Random r3 = new System.Random();  //Instantiate Random Weapon  generator
                int WeaponModel = r3.Next(0, 5);    //Use Random Weapon generator
                Game.LogTrivial("YOBBINCALLOUTS: Suspect Weapon Model is " + WeaponModel);
                if (WeaponModel == 0) Suspect.Inventory.GiveNewWeapon("WEAPON_ASSAULTRIFLE", -1, true);
                else if (WeaponModel == 1) Suspect.Inventory.GiveNewWeapon("WEAPON_SMG", -1, true);
                else if (WeaponModel == 2) Suspect.Inventory.GiveNewWeapon("WEAPON_APPISTOL", -1, true);
                else if (WeaponModel == 3) Suspect.Inventory.GiveNewWeapon("WEAPON_MICROSMG", -1, true);
                else if (WeaponModel == 4) Suspect.Inventory.GiveNewWeapon("WEAPON_COMPACTRIFLE", -1, true);

                GameFiber.Wait(1500);

                while (player.DistanceTo(Suspect) >= 100) GameFiber.Wait(0);
                SuspectVehicle.IsDriveable = true;
                SuspectVehicle.IsVisible = true;
                Suspect.Tasks.CruiseWithVehicle(SuspectVehicle, 17, VehicleDrivingFlags.DriveAroundVehicles);
                Functions.PlayScannerAudio("ATTENTION_ALL_UNITS_01");   //change
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

                if (SuspectBlip.Exists()) SuspectBlip.Delete();
                SuspectBlip = Suspect.AttachBlip();
                SuspectBlip.IsFriendly = false;
                SuspectBlip.Scale = 0.75f;
                GameFiber.Wait(1500);
                Game.DisplaySubtitle("Dispatch, We Have Located the ~r~Suspect!");
                GameFiber.Wait(1500);

                SuspectDecisions();
            }
        }
        private void SuspectDecisions()
        {
            Game.DisplayHelp("Perform a ~o~Traffic Stop~w~ on the ~r~Suspect.");
            while (!Functions.IsPlayerPerformingPullover() && Suspect.IsAlive) GameFiber.Wait(0);
            System.Random yuy = new System.Random();
            int WaitTime = yuy.Next(1500, 6000);

            System.Random r = new System.Random();
            int action = r.Next(0, 4);
            Game.LogTrivial("YOBBINCALLOUTS: SUSPECT FINAL ACTION IS..." + action);
            if (action == 0) CallHandler.CreatePursuit(MainPursuit, true, true, true, Suspect);
            else if (action == 1) //Shoot
            {
                GameFiber.Wait(WaitTime);
                Functions.ForceEndCurrentPullover();
                if (Suspect.IsAlive)
                {
                    Suspect.Tasks.ParkVehicle(SuspectVehicle, SuspectVehicle.Position, SuspectVehicle.Heading).WaitForCompletion(5000);
                    Suspect.Tasks.LeaveVehicle(SuspectVehicle, LeaveVehicleFlags.LeaveDoorOpen).WaitForCompletion();
                    Suspect.Tasks.AchieveHeading(Game.LocalPlayer.Character.LastVehicle.Heading - 180).WaitForCompletion(1500);
                    Suspect.Tasks.AimWeaponAt(Game.LocalPlayer.Character.Position, 1500).WaitForCompletion();   //test this
                    Suspect.Tasks.FightAgainst(Game.LocalPlayer.Character, -1);
                    if (LSPD_First_Response.Mod.API.Functions.IsPlayerPerformingPullover()) { LSPD_First_Response.Mod.API.Functions.ForceEndCurrentPullover(); }
                    GameFiber.Wait(2000);
                    LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("CRIME_ASSAULT_PEACE_OFFICER_01");
                    LSPD_First_Response.Mod.API.Functions.RequestBackup(Suspect.Position, LSPD_First_Response.EBackupResponseType.Code3, LSPD_First_Response.EBackupUnitType.LocalUnit);
                    while (Suspect.Exists() && Suspect.IsAlive) GameFiber.Wait(0);
                    if (Suspect.IsDead) { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, Suspect Was ~r~Killed~w~ Trying to Assault an Officer."); SuspectBlip.Delete(); }
                    else { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, Suspect is Under ~g~Arrest~w~ For Trying to Assault an Officer."); }
                    GameFiber.Wait(2000);
                    LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("REPORT_RESPONSE_COPY_02");
                }
            }
            else //Late
            {
                while (SuspectVehicle.Speed > 0) GameFiber.Wait(0);
                Game.DisplayHelp("Approach the ~r~Suspect.");
                GameFiber.Wait(WaitTime);
                if (Suspect.Exists() && Suspect.IsAlive)
                {
                    if (action == 2) //late shoot
                    {
                        Suspect.Tasks.LeaveVehicle(SuspectVehicle, LeaveVehicleFlags.LeaveDoorOpen).WaitForCompletion();
                        Suspect.Tasks.AchieveHeading(Game.LocalPlayer.Character.LastVehicle.Heading - 180).WaitForCompletion(1500);
                        Suspect.Tasks.AimWeaponAt(Game.LocalPlayer.Character.Position, 1500).WaitForCompletion();   //test this
                        Suspect.Tasks.FightAgainst(Game.LocalPlayer.Character, -1);
                        if (LSPD_First_Response.Mod.API.Functions.IsPlayerPerformingPullover()) { LSPD_First_Response.Mod.API.Functions.ForceEndCurrentPullover(); }
                        GameFiber.Wait(2000);
                        LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("CRIME_ASSAULT_PEACE_OFFICER_01");
                        LSPD_First_Response.Mod.API.Functions.RequestBackup(Suspect.Position, LSPD_First_Response.EBackupResponseType.Code3, LSPD_First_Response.EBackupUnitType.LocalUnit);
                        while (Suspect.Exists())
                        {
                            GameFiber.Yield();
                            if (!Suspect.Exists() || Suspect.IsDead || Functions.IsPedArrested(Suspect)) break;
                        }
                        if (Suspect.IsDead) { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, Suspect Was ~r~Killed~w~ Trying to Assault an Officer."); SuspectBlip.Delete(); }
                        if (Suspect.IsCuffed) { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, Suspect is Under ~g~Arrest~w~ For Trying to Assault an Officer."); }
                        GameFiber.Wait(2000);
                        LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("REPORT_RESPONSE_COPY_02");
                    }
                    else CallHandler.CreatePursuit(MainPursuit, true, true, true, Suspect);  //late pursuit (action == 3)
                }
            }
        }
    }
}