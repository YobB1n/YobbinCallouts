//ASSAULT ON BUS Callout by YobB1n, YobbinCallouts
//*LAST UPDATED: November 16, 2020
//*OBJECTIVE:
//*REMARKS: This code is far from perfect, no bugs/crashes in my testing but it is certainally inefficient and there probably are unused variables and things that could be better. I'm new to this, I apologize.
//*Some Commented out code is for a future implementation of this code where I add an event where the player has the option to drive the victim back to their house.
//*Comments, Remarks, or Questions, as well as Patches please use the LSPDFR Plugin Page for this Callout.
//*Thank You!

using System;
using System.Collections.Generic;
using LSPD_First_Response.Engine.Scripting;
using System.Windows.Forms;
using Rage;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage.Native;
using System.Drawing;

namespace YobbinCallouts.Callouts
{

    [CalloutInfo("Assault On Bus", CalloutProbability.Medium)]
    public class AssaultOnBus : Callout
    {
        //SPAWN POINTS
        //CITY
        private Vector3 Alta = new Vector3(258.9062f, -377.681f, 44.58482f);
        private Vector3 Burton = new Vector3(-523.7678f, -267.3726f, 35.29238f);
        private Vector3 ChamH = new Vector3(-107.0473f, -1685.895f, 29.20226f);
        private Vector3 CypressFlats = new Vector3(-878.0088f, -1766.411f, 29.84155f);
        private Vector3 Davis1 = new Vector3(-138.7037f, -1982.754f, 22.83529f);
        private Vector3 Davis2 = new Vector3(279.4628f, -1459.935f, 29.1191f);
        private Vector3 DelPerro = new Vector3(-1521.592f, -463.6807f, 35.28944f);
        private Vector3 DelPerro2 = new Vector3(-1409.31f, -570.114f, 30.30899f);
        private Vector3 DelPerro3 = new Vector3(-1478.658f, -632.9943f, 30.46013f);
        private Vector3 Elysian = new Vector3(-794.0625f, -3047.102f, 6.766771f);
        private Vector3 LaMesa1 = new Vector3(-806.9794f, -1354.459f, 26.29575f);
        private Vector3 LaMesa2 = new Vector3(788.3455f, -1368.106f, 26.49827f);
        private Vector3 LaMesa3 = new Vector3(770.7678f, -939.4482f, 25.593837f);
        private Vector3 LaMesa4 = new Vector3(785.4872f, -778.6043f, 26.33783f);
        private Vector3 KTown1 = new Vector3(-694.0866f, -667.7482f, 30.76516f);
        private Vector3 KTown2 = new Vector3(-506.7512f, -667.7038f, 33.02613f);
        private Vector3 KTown3 = new Vector3(-560.5147f, -845.7031f, 27.34039f);
        private Vector3 Airport1 = new Vector3(-1040.662f, -2725.775f, 20.0923f);
        private Vector3 Airport2 = new Vector3(-1045.258f, -2716.292f, 13.67731f);
        private Vector3 Stad = new Vector3(-146.6807f, 2041.945f, 22.94198f);
        private Vector3 MirrorPark = new Vector3(1189.356f, -416.5154f, 67.46564f);
        private Vector3 PBluff = new Vector3(-2102.175f, -295.1127f, 13.03409f);
        private Vector3 Pillbox1 = new Vector3(118.4144f, -785.7147f, 31.28741f);
        private Vector3 Pillbox2 = new Vector3(-172.1286f, -817.8408f, 31.10602f);
        private Vector3 Pillbox3 = new Vector3(-249.7246f, -883.182f, 30.56357f);
        private Vector3 PillBox4 = new Vector3(-217.3994f, -1010.452f, 29.17915f);
        private Vector3 Rockford1 = new Vector3(-692.3359f, -6.224844f, 38.15116f);
        private Vector3 Rockford2 = new Vector3(-930.4749f, -126.0807f, 37.57829f);
        private Vector3 Rockford3 = new Vector3(-1166.419f, -401.0388f, 35.45403f);
        private Vector3 Rockford4 = new Vector3(-681.0934f, -375.9459f, 34.21792f);
        private Vector3 Rockford5 = new Vector3(-258.2194f, -324.5125f, 29.88147f);
        //San Andreas North of Cypress Flats?
        private Vector3 Strawberry1 = new Vector3(257.9587f, -1187.642f, 29.45594f);
        private Vector3 Strawberry2 = new Vector3(70.83752f, -1474.274f, 29.20082f);
        private Vector3 Strawberry3 = new Vector3(49.93737f, -1537.299f, 29.19085f);
        private Vector3 Textile = new Vector3(307.6413f, -764.8298f, 29.2324f);
        private Vector3 VespucciBeach = new Vector3(-1168.946f, -1470.082f, 4.297196f);
        private Vector3 VinewoodRace = new Vector3(959.1135f, 173.7969f, 80.82661f);
        private Vector3 WestVinewood = new Vector3(-501.973f, 20.67921f, 44.73386f);
        private Vector3 Richman = new Vector3(-1611.583f, 173.999f, 59.76557f);
        private Vector3 Hawick = new Vector3(324.6387f, -88.70399f, 68.74538f);
        private Vector3 DowntownVine = new Vector3(211.5656f, 250.5646f, 105.4593f);
        private Vector3 EastVine = new Vector3(1122.201f, -252.3284f, 68.98948f);
        private Vector3 Rancho = new Vector3(434.8558f, -2024.733f, 23.24066f);
        private Vector3 Davis3 = new Vector3(128.355f, -1715.873f, 29.06286f);
        private Vector3 MissionRow = new Vector3(351.9958f, -1064.253f, 29.39734f);
        private Vector3 Textile2 = new Vector3(461.3195f, -611.6256f, 28.48598f);
        private Vector3 Textile3 = new Vector3(307.9035f, -763.6847f, 29.22663f);
        private Vector3 VespucciCanals = new Vector3(-1212.478f, -1216.602f, 7.583918f);
        private Vector3 RockfordHills = new Vector3(-862.2045f, -135.3484f, 37.80842f);
        //COUNTRY
        private Vector3 BanhamCanyon = new Vector3(-3031.738f, 593.2711f, 8.547567f);
        private Vector3 Chumash = new Vector3(-3234.587f, 1005.579f, 13.0714f);
        private Vector3 LagoZancudo = new Vector3(-2563.994f, 2320.287f, 33.89155f);
        private Vector3 TatviamMountains = new Vector3(2565.242f, 392.0405f, 109.2949f);
        private Vector3 MountChiliad = new Vector3(1733.514f, 6399.68f, 35.66004f);
        private Vector3 PaletoBay1 = new Vector3(145.0622f, 6574.145f, 32.73963f);
        private Vector3 PaletoBay2 = new Vector3(-215.3233f, 6173.316f, 32.05423f);
        private Vector3 ZancudoRiver = new Vector3(-1145.751f, 2663.053f, 18.91434f);
        private Vector3 Harmony = new Vector3(533.1015f, 2671.716f, 43.15641f);
        private Vector3 SandyShores = new Vector3(1955.849f, 3738.852f, 33.02981f);
        private Vector3 Grapessed = new Vector3(1680.88f, 4921.424f, 42.90202f);
        private Vector3 GrandSenoraDesert = new Vector3(2700.959f, 3285.273f, 56.14145f);
        //SCENARIO 1
        private Vector3 SpawnPoint;
        private Vector3 VictimSpawnpoint;
        private String Zone;
        private bool City = false;
        private bool CalloutRunning = false;

        private Vehicle Bus;

        private Ped player = Game.LocalPlayer.Character;
        private Ped Driver;
        private Ped Victim;
        private Ped Suspect;
        private Ped Witness;
        private Ped Witness2;

        private Blip VictimBlip;
        private Blip SuspectBlip;
        private Blip DriverBlip;
        private Blip SuspectArea;

        private int MainScenario;
        private int SuspectAction;
        private float VehicleHeading;
        private String[] Suspects;

        private LHandle MainPursuit;

        //Below are all the strings for all possible dialogues in the callout. For each scenario there are multiple dialogue options, so this takes up a lot of space.
        private readonly List<string> OpeningDialogue1 = new List<string>()
        {
         "~b~Victim:~w~ Hi officer, over here.",
         "~g~You:~w~ Tell me what happened.",
         "~b~Victim:~w~ Well, I was just minding my own business when some random dude started yelling at me.",
         "~b~Victim:~w~ He got in my face and started swearing at me, and when I tried to leave, he punched me multiple times.",
         "~g~You:~w~ Do you need medical attention?",
         "~b~Victim:~w~ I think I'm Fine, but I'd like to catch him. He just ran off thinking he could get away with it.",
         "~g~You:~w~ Well, where did he go? What did he look like?",
         "~b~Victim:~w~ I can't remember exactly, but I'd definitely recognize Him if I saw Him. Could I come with you to help search for Him in the area?",
        };
        private readonly List<string> OpeningDialogue2 = new List<string>()
        {
         "~b~Victim:~w~ Hi officer, over here!",
         "~g~You:~w~ What happened?.",
         "~b~Victim:~w~ Well, I was just minding my own business when some random dude started yelling at me.",
         "~b~Victim:~w~ He got in my face and started swearing at me, and when I tried to leave, he punched me multiple times.",
         "~g~You:~w~ Do you need medical attention?",
         "~b~Victim:~w~ I think I'm Fine, but I'd like to catch him. He just ran off thinking he could get away with it.",
         "~g~You:~w~ Well, where did he go? What did he look like?",
         "~b~Victim:~w~ I can't remember exactly, but I'd definitely recognize Him if I saw Him. Could I come with you to help search for Him in the area?",
        };
        private readonly List<string> OpeningDialogue3 = new List<string>()
        {
         "~b~Victim:~w~ Hi officer, over here.",
         "~g~You:~w~ Tell me what happened.",
         "~b~Victim:~w~ Well, I was just minding my own business when some random dude started yelling at me.",
         "~b~Victim:~w~ He got in my face and started swearing at me, and when I tried to leave, he punched me multiple times.",
         "~g~You:~w~ Do you need medical attention?",
         "~b~Victim:~w~ I think I'm Fine, but I'd like to catch him. He just ran off thinking he could get away with it.",
         "~g~You:~w~ Well, where did he go? What did he look like?",
         "~b~Victim:~w~ I can't remember exactly, but I'd definitely recognize Him if I saw Him. Could I come with you to help search for Him in the area?",
        };

        public override bool OnBeforeCalloutDisplayed()
        {
            Game.LogTrivial("==========YOBBINCALLOUTS: Assault on Bus Callout Start==========");
            Zone = Functions.GetZoneAtPosition(Game.LocalPlayer.Character.Position).GameName;
            Game.LogTrivial("YOBBINCALLOUTS: Zone is " + Zone);
            if (Zone == "Davis" || Zone == "Stad" || Zone == "STRAW" || Zone == "Banning" || Zone == "RANCHO" || Zone == "ChamH" || Zone == "PBOX" || Zone == "LegSqu" || Zone == "SKID" || Zone == "TEXTI")
            {
                City = true;
                System.Random r3 = new System.Random(); //LSPD
                int BusStop = r3.Next(1, 17);
                if (BusStop == 1) { SpawnPoint = Davis1; Game.LogTrivial("YOBBINCALLOUTS: Found Spawnpoint."); }
                if (BusStop == 2) { SpawnPoint = Davis2; Game.LogTrivial("YOBBINCALLOUTS: Found Spawnpoint."); }
                if (BusStop == 3) { SpawnPoint = Davis3; Game.LogTrivial("YOBBINCALLOUTS: Found Spawnpoint."); }
                if (BusStop == 4) { SpawnPoint = Stad; Game.LogTrivial("YOBBINCALLOUTS: Found Spawnpoint."); }
                if (BusStop == 5) { SpawnPoint = Strawberry1; Game.LogTrivial("YOBBINCALLOUTS: Found Spawnpoint."); }
                if (BusStop == 6) { SpawnPoint = Strawberry2; Game.LogTrivial("YOBBINCALLOUTS: Found Spawnpoint."); }
                if (BusStop == 7) { SpawnPoint = Strawberry3; Game.LogTrivial("YOBBINCALLOUTS: Found Spawnpoint."); }
                if (BusStop == 8) { SpawnPoint = Rancho; Game.LogTrivial("YOBBINCALLOUTS: Found Spawnpoint."); }
                if (BusStop == 9) { SpawnPoint = ChamH; Game.LogTrivial("YOBBINCALLOUTS: Found Spawnpoint."); }
                if (BusStop == 10) { SpawnPoint = Pillbox1; Game.LogTrivial("YOBBINCALLOUTS: Found Spawnpoint."); }
                if (BusStop == 11) { SpawnPoint = Pillbox2; Game.LogTrivial("YOBBINCALLOUTS: Found Spawnpoint."); }
                if (BusStop == 12) { SpawnPoint = Pillbox3; Game.LogTrivial("YOBBINCALLOUTS: Found Spawnpoint."); }
                if (BusStop == 13) { SpawnPoint = MissionRow; Game.LogTrivial("YOBBINCALLOUTS: Found Spawnpoint."); }
                if (BusStop == 14) { SpawnPoint = Textile; Game.LogTrivial("YOBBINCALLOUTS: Found Spawnpoint."); }
                if (BusStop == 15) { SpawnPoint = Textile2; Game.LogTrivial("YOBBINCALLOUTS: Found Spawnpoint."); }
                if (BusStop == 16) { SpawnPoint = Textile3; Game.LogTrivial("YOBBINCALLOUTS: Found Spawnpoint."); }
            }
            else if (Zone == "Cypre" || Zone == "Murri" || Zone == "EBuro" || Zone == "LMesa" || Zone == "Mirr" || Zone == "East_V" || Zone == "Hors")
            {
                City = true;
                System.Random r3 = new System.Random(); //La Mesa PD
                int BusStop = r3.Next(1, 9);
                if (BusStop == 1) { SpawnPoint = CypressFlats; Game.LogTrivial("YOBBINCALLOUTS: Found Spawnpoint."); }
                if (BusStop == 2) { SpawnPoint = LaMesa1; Game.LogTrivial("YOBBINCALLOUTS: Found Spawnpoint."); }
                if (BusStop == 3) { SpawnPoint = LaMesa2; Game.LogTrivial("YOBBINCALLOUTS: Found Spawnpoint."); }
                if (BusStop == 4) { SpawnPoint = LaMesa3; Game.LogTrivial("YOBBINCALLOUTS: Found Spawnpoint."); }
                if (BusStop == 5) { SpawnPoint = LaMesa4; Game.LogTrivial("YOBBINCALLOUTS: Found Spawnpoint."); }
                if (BusStop == 6) { SpawnPoint = MirrorPark; Game.LogTrivial("YOBBINCALLOUTS: Found Spawnpoint."); }
                if (BusStop == 7) { SpawnPoint = EastVine; Game.LogTrivial("YOBBINCALLOUTS: Found Spawnpoint."); }
                if (BusStop == 8) { SpawnPoint = VinewoodRace; Game.LogTrivial("YOBBINCALLOUTS: Found Spawnpoint."); }
            }
            else if (Zone == "Vesp" || Zone == "VCana" || Zone == "Beach" || Zone == "DelSol" || Zone == "Koreat")
            {
                City = true;
                System.Random r3 = new System.Random(); //Vesupicci Beach PD
                int BusStop = r3.Next(1, 9);
                if (BusStop == 1) { SpawnPoint = VespucciCanals; Game.LogTrivial("YOBBINCALLOUTS: Found Spawnpoint."); }
                if (BusStop == 2) { SpawnPoint = VespucciBeach; Game.LogTrivial("YOBBINCALLOUTS: Found Spawnpoint."); }
                if (BusStop == 6) { SpawnPoint = KTown1; Game.LogTrivial("YOBBINCALLOUTS: Found Spawnpoint."); }
                if (BusStop == 7) { SpawnPoint = KTown2; Game.LogTrivial("YOBBINCALLOUTS: Found Spawnpoint."); }
                if (BusStop == 8) { SpawnPoint = KTown3; Game.LogTrivial("YOBBINCALLOUTS: Found Spawnpoint."); }
            }
            else if (Zone == "DeLBe" || Zone == "DelPe" || Zone == "Morn" || Zone == "PBluff" || Zone == "Movie")
            {
                City = true;
                System.Random r3 = new System.Random(); //Del Perro PD
                int BusStop = r3.Next(1, 5);
                if (BusStop == 1) { SpawnPoint = PBluff; Game.LogTrivial("YOBBINCALLOUTS: Found Spawnpoint."); }
                if (BusStop == 2) { SpawnPoint = DelPerro; Game.LogTrivial("YOBBINCALLOUTS: Found Spawnpoint."); }
                if (BusStop == 3) { SpawnPoint = DelPerro2; Game.LogTrivial("YOBBINCALLOUTS: Found Spawnpoint."); }
                if (BusStop == 4) { SpawnPoint = DelPerro3; Game.LogTrivial("YOBBINCALLOUTS: Found Spawnpoint."); }
            }
            else if (Zone == "Rockf" || Zone == "Burton" || Zone == "Richm" || Zone == "Golf")
            {
                City = true;
                System.Random r3 = new System.Random(); //Rockford Hills PD
                int BusStop = r3.Next(1, 9);
                if (BusStop == 1) { SpawnPoint = Rockford1; Game.LogTrivial("YOBBINCALLOUTS: Found Spawnpoint."); }
                if (BusStop == 2) { SpawnPoint = Rockford2; Game.LogTrivial("YOBBINCALLOUTS: Found Spawnpoint."); }
                if (BusStop == 3) { SpawnPoint = Rockford3; Game.LogTrivial("YOBBINCALLOUTS: Found Spawnpoint."); }
                if (BusStop == 4) { SpawnPoint = Rockford4; Game.LogTrivial("YOBBINCALLOUTS: Found Spawnpoint."); }
                if (BusStop == 5) { SpawnPoint = Rockford5; Game.LogTrivial("YOBBINCALLOUTS: Found Spawnpoint."); }
                if (BusStop == 6) { SpawnPoint = Burton; Game.LogTrivial("YOBBINCALLOUTS: Found Spawnpoint."); }
                if (BusStop == 7) { SpawnPoint = Richman; Game.LogTrivial("YOBBINCALLOUTS: Found Spawnpoint."); }
                if (BusStop == 8) { SpawnPoint = RockfordHills; Game.LogTrivial("YOBBINCALLOUTS: Found Spawnpoint."); }
            }
            else if (Zone == "CHIL" || Zone == "Vine" || Zone == "DTVine" || Zone == "WVine" || Zone == "Alta" || Zone == "Hawick")
            {
                City = true;
                System.Random r3 = new System.Random(); //Vinewood PD
                int BusStop = r3.Next(1, 8);
                if (BusStop == 2) { SpawnPoint = DowntownVine; Game.LogTrivial("YOBBINCALLOUTS: Found Spawnpoint."); }
                if (BusStop == 3) { SpawnPoint = Rockford3; Game.LogTrivial("YOBBINCALLOUTS: Found Spawnpoint."); }
                if (BusStop == 4) { SpawnPoint = Rockford4; Game.LogTrivial("YOBBINCALLOUTS: Found Spawnpoint."); }
                if (BusStop == 5) { SpawnPoint = Rockford5; Game.LogTrivial("YOBBINCALLOUTS: Found Spawnpoint."); }
                if (BusStop == 6) { SpawnPoint = WestVinewood; Game.LogTrivial("YOBBINCALLOUTS: Found Spawnpoint."); }
                if (BusStop == 7) { SpawnPoint = Alta; Game.LogTrivial("YOBBINCALLOUTS: Found Spawnpoint."); }
                if (BusStop == 8) { SpawnPoint = Hawick; Game.LogTrivial("YOBBINCALLOUTS: Found Spawnpoint."); }
            }
            else if (Zone == "Sandy" || Zone == "Desrt")
            {
                System.Random r3 = new System.Random(); //Sandy Shores PD
                int BusStop = r3.Next(1, 3);
                if (BusStop == 1) { SpawnPoint = SandyShores; Game.LogTrivial("YOBBINCALLOUTS: Found Spawnpoint."); }
                if (BusStop == 2) { SpawnPoint = GrandSenoraDesert; Game.LogTrivial("YOBBINCALLOUTS: Found Spawnpoint."); }
            }
            else if (Zone == "GrapeS")
            {
                SpawnPoint = Grapessed; Game.LogTrivial("YOBBINCALLOUTS: Found Spawnpoint.");
            }
            else if (Zone == "ProcoB" || Zone == "PalFor" || Zone == "Paleto" || Zone == "MTChil")
            {
                System.Random r3 = new System.Random(); //Paleto Bay PD
                int BusStop = r3.Next(1, 4);
                if (BusStop == 1) { SpawnPoint = PaletoBay1; Game.LogTrivial("YOBBINCALLOUTS: Found Spawnpoint."); }
                if (BusStop == 2) { SpawnPoint = PaletoBay2; Game.LogTrivial("YOBBINCALLOUTS: Found Spawnpoint."); }
                if (BusStop == 3) { SpawnPoint = MountChiliad; Game.LogTrivial("YOBBINCALLOUTS: Found Spawnpoint."); }
            }
            else if (Zone == "Tatamo") { SpawnPoint = TatviamMountains; Game.LogTrivial("YOBBINCALLOUTS: Found Spawnpoint."); }
            else if (Zone == "BhamCa" || Zone == "CHU")
            {
                System.Random r3 = new System.Random();
                int BusStop = r3.Next(1, 3);
                if (BusStop == 1) { SpawnPoint = Chumash; Game.LogTrivial("YOBBINCALLOUTS: Found Spawnpoint."); }
                if (BusStop == 2) { SpawnPoint = BanhamCanyon; Game.LogTrivial("YOBBINCALLOUTS: Found Spawnpoint."); }
            }
            else if (Zone == "Lago" || Zone == "NCHU" || Zone == "ArmyB" || Zone == "Zancudo")
            {
                System.Random r3 = new System.Random();
                int BusStop = r3.Next(1, 4);
                if (BusStop == 1) { SpawnPoint = Chumash; Game.LogTrivial("YOBBINCALLOUTS: Found Spawnpoint."); }
                if (BusStop == 2) { SpawnPoint = BanhamCanyon; Game.LogTrivial("YOBBINCALLOUTS: Found Spawnpoint."); }
                if (BusStop == 3) { SpawnPoint = ZancudoRiver; Game.LogTrivial("YOBBINCALLOUTS: Found Spawnpoint."); }
            }
            else if (Zone == "Harmo") { SpawnPoint = Harmony; Game.LogTrivial("YOBBINCALLOUTS: Found Spawnpoint."); }
            else
            {
                SpawnPoint = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(550f));
                try
                {
                    Game.LogTrivial("YOBBINCALLOUTS: Player Not in Any Set Spawnpoint Zone. Choosing Random Spawn.");
                    Vector3 desiredRoadSidePosition = SpawnPoint;
                    NativeFunction.Natives.xA0F8A7517A273C05<bool>(desiredRoadSidePosition, 360, out Vector3 outPosition);
                    SpawnPoint = outPosition;
                }
                catch (Exception)
                {
                    Game.DisplayNotification("YOBBINCALLOUTS: Could Not Find the Road Side. Ending Callout.");
                    Game.LogTrivial("YOBBINCALLOUTS: Could Not Find the Road Side. Ending Callout.");
                    return false;
                }
            }

            ShowCalloutAreaBlipBeforeAccepting(SpawnPoint, 75f);    //Callout Blip Circle with radius 30m
            AddMinimumDistanceCheck(30f, SpawnPoint);   //Player must be 30m or further away

            if (City) { Functions.PlayScannerAudio("CITIZENS_REPORT YC_ASSAULT_ON_BUS"); }
            else { Functions.PlayScannerAudio("CITIZENS_REPORT YC_ASSAULT_ON_COACH_BUS"); }
            CalloutMessage = "Assault on Bus";
            CalloutPosition = SpawnPoint;
            CalloutAdvisory = "Suspect is Reported to have ~r~Violently Assaulted~w~ a Person on the Bus.";

            return base.OnBeforeCalloutDisplayed();

        }
        public override bool OnCalloutAccepted()
        {
            Game.LogTrivial("YOBBINCALLOUTS: Assault On Bus Callout Accepted by User");
            if (Main.CalloutInterface)
            {
                CalloutInterfaceHandler.SendCalloutDetails(this, "CODE 3", "");
            }
            else
            {
                Game.DisplayNotification("Respond ~r~Code 3~w~");
            }

            if (City)
            {
                Bus = new Vehicle("BUS", SpawnPoint);
            }
            else
            {
                Bus = new Vehicle("coach", SpawnPoint);
            }
            try
            {
                NativeFunction.Natives.GetClosestVehicleNodeWithHeading(SpawnPoint, out Vector3 nodePosition, out float heading, 1, 3.0f, 0);
                bool success = NativeFunction.Natives.xA0F8A7517A273C05<bool>(SpawnPoint, heading, out Vector3 outPosition);
                if (success)
                {
                    SpawnPoint = outPosition;
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

            Bus.Heading = VehicleHeading;
            Game.LogTrivial("YOBBINCALLOUTS: Bus Spawned");
            Bus.IsPersistent = true;
            Bus.IsEngineOn = true;
            Bus.IsDriveable = false;
            Driver = Bus.CreateRandomDriver();
            Driver.IsPersistent = true;
            Driver.BlockPermanentEvents = true;
            Driver.Tasks.CruiseWithVehicle(0f);
            Driver.IsInvincible = true;
            Game.LogTrivial("YOBBINCALLOUTS: Bus Driver Spawned");

            System.Random r = new System.Random();
            int Scenario = r.Next(0, 1);    //CHANGE THIS LATER
            switch (Scenario)
            {
                case 0:
                    MainScenario = 0;
                    Game.LogTrivial("YOBBINCALLOUTS: Assault on Bus Scenario 0 - Unprovoked Assault Chosen");
                    VictimSpawnpoint = Driver.GetOffsetPositionRight(3);
                    Victim = new Ped(VictimSpawnpoint);
                    Victim.IsPersistent = true;
                    Victim.BlockPermanentEvents = true;
                    Victim.IsInvincible = true;
                    VictimBlip = Victim.AttachBlip();
                    VictimBlip.IsFriendly = true;
                    VictimBlip.IsRouteEnabled = true;
                    VictimBlip.Scale = 0.65f;
                    VictimBlip.Name = "Victim";
                    break;
                case 1:
                    MainScenario = 1;
                    Game.LogTrivial("YOBBINCALLOUTS: Assault on Bus Scenario 1 - Fare Evasion Chosen");
                    DriverBlip = Driver.AttachBlip();
                    DriverBlip.IsFriendly = true;
                    DriverBlip.Scale = 0.65f;
                    DriverBlip.IsRouteEnabled = true;
                    DriverBlip.Name = "Driver";

                    Witness = new Ped(Driver.GetOffsetPositionFront(2));
                    Witness.IsPersistent = true;
                    Witness.BlockPermanentEvents = true;

                    Witness2 = new Ped(Driver.GetOffsetPositionFront(5));
                    Witness2.IsPersistent = true;
                    Witness2.BlockPermanentEvents = true;
                    break;
            }

            System.Random rYUY = new System.Random();
            SuspectAction = rYUY.Next(0, 3);  //Change this later
            Game.LogTrivial("YOBBINCALLOUTS: Suspect Action Value is " + SuspectAction);

            if (!CalloutRunning) Callout(); CalloutRunning = true;
            return base.OnCalloutAccepted();
        }
        public override void OnCalloutNotAccepted()
        {
            Game.LogTrivial("YOBBINCALLOUTS: Assault on Bus Callout Not Accepted by User");
            base.OnCalloutNotAccepted();    //Clean Up
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
                        while (player.DistanceTo(Driver) >= 15f && !Game.IsKeyDown(Config.CalloutEndKey)) GameFiber.Wait(0);
                        if (Game.IsKeyDown(Config.CalloutEndKey)) { EndCalloutHandler.CalloutForcedEnd = true; break; }
                        if (Main.CalloutInterface)  CalloutInterfaceHandler.SendMessage(this, "Unit Arrived on Scene. Talking to Witness");
                        if (MainScenario == 0) AssaultOpening();
                        if (Witness.Exists() && Witness.IsInAnyVehicle(false))
                        {
                            Witness.Tasks.LeaveVehicle(Witness.CurrentVehicle, LeaveVehicleFlags.None).WaitForCompletion();
                            Witness.ClearLastVehicle();
                            Witness.Tasks.Wander();
                        }
                        break;
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
        private void AssaultOpening()
        {
            if (CalloutRunning)
            {
                Game.DisplayHelp("Speak with the ~b~Victim.");

                NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(Victim, player, -1);
                while (player.DistanceTo(Victim) >= 5) GameFiber.Wait(0);
                if (Config.DisplayHelp) Game.DisplayHelp("Press ~y~" + Config.MainInteractionKey + " ~w~to Speak with the ~b~Victim.");
                while (!Game.IsKeyDown(Config.MainInteractionKey)) GameFiber.Wait(0);

                System.Random r = new System.Random();
                int OpeningDialogue = r.Next(0, 3);
                if (OpeningDialogue == 0)
                {
                    CallHandler.Dialogue(OpeningDialogue1, Victim);
                }
                else if (OpeningDialogue == 1)
                {
                    CallHandler.Dialogue(OpeningDialogue2, Victim);
                }
                else
                {
                    CallHandler.Dialogue(OpeningDialogue3, Victim);
                }
                Victim.Tasks.ClearImmediately();

                Game.LogTrivial("YOBBINCALLOUTS: Started Suspect 1 Spawn");
                NativeFunction.Natives.xA0F8A7517A273C05<Vector3>(World.GetNextPositionOnStreet(player.Position.Around(69)), 360, out Vector3 Suspect1Spawn);
                Suspects = new string[8] { "A_M_Y_SouCent_01", "A_M_Y_StWhi_01", "A_M_Y_StBla_01", "A_M_Y_Downtown_01", "A_M_Y_BevHills_01", "G_M_Y_MexGang_01", "G_M_Y_MexGoon_01", "G_M_Y_StrPunk_01" };
                System.Random r2 = new System.Random();
                int SuspectModel = r2.Next(0, Suspects.Length);
                Suspect = new Ped(Suspects[SuspectModel], Suspect1Spawn, 69);   //nice
                try
                {
                    Suspect.IsPersistent = true;
                    Suspect.BlockPermanentEvents = true;
                    Suspect.IsVisible = false;
                }
                catch (Rage.Exceptions.InvalidHandleableException)
                {
                    Game.LogTrivial("YOBBINCALLOUTS: Yobbincallouts Crash Prevented. InvalidHandableException.");
                    Game.DisplayNotification("~b~YobbinCallouts~r~ Crash~g~ Prevented.~w~ I Apologize for the ~y~Inconvenience.");
                    End();
                }
                Game.LogTrivial("YOBBINCALLOUTS: Finished Suspect 1 Spawn");

                Game.DisplayHelp("~y~" + Config.Key1 + ":~b~ Let the Victim Help You Search for the Suspect.~y~ " + Config.Key2 + ":~b~ Search for the Suspect Yourself.");
                CallHandler.IdleAction(Victim, false);
                while (!Game.IsKeyDown(Config.Key1) && !Game.IsKeyDown(Config.Key2)) GameFiber.Wait(0); //might do an animation?
                Victim.Tasks.ClearImmediately();
                if (Game.IsKeyDown(Config.Key1)) Follow();
                else Search();
            }
        }
        private void Follow()
        {
            if (CalloutRunning)
            {
                Game.DisplaySubtitle("~g~You:~w~ Sure, It Would Help a lot if You Were There to Help Me. Hop In!", 3000);
                GameFiber.Wait(3500);
                Game.DisplayHelp("Get in your ~g~Vehicle.");
                if (Main.CalloutInterface) CalloutInterfaceHandler.SendMessage(this, "Victim was assaulted on the bus. Victim is assisting in finding the Suspect.");

                while (!Game.LocalPlayer.Character.IsInAnyPoliceVehicle) { GameFiber.Wait(0); }
                Game.DisplayHelp("~y~" + Config.Key1 + ": ~b~Tell the Driver to Enter the Passenger Seat. ~y~" + Config.Key2 + ":~b~ Tell the Driver to Enter the Rear Seat.");
                while (!Game.IsKeyDown(Config.Key1) && !Game.IsKeyDown(Config.Key2)) GameFiber.Wait(0); //might do an animation?
                int SeatIndex;
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
                Suspect.IsVisible = false;

                Game.DisplayHelp("Start ~o~Searching~w~ for the ~r~Suspect.");
                SuspectArea = new Blip(Suspect.Position.Around(15), 250);
                SuspectArea.Color = Color.Orange;
                SuspectArea.Alpha = 0.5f;
                GameFiber.Wait(1500);
                if (Driver.Exists()) Driver.Dismiss();

                System.Random coco = new System.Random();
                int WaitTime = coco.Next(20000, 40000); //20 - 40 seconds
                Game.LogTrivial("YOBBINCALLOUTS: Waiting " + WaitTime + " Seconds.");
                GameFiber.Wait(WaitTime);

                Suspect.Position = World.GetNextPositionOnStreet(Victim.Position.Around(30));
                Suspect.IsVisible = true;
                Suspect.Tasks.Wander();
                Game.DisplaySubtitle("~b~Victim:~w~ Officer I See the ~r~Suspect~w~! He's Right Over There!", 2500);
                GameFiber.Wait(2000);
                SuspectBlip = Suspect.AttachBlip();
                SuspectBlip.IsFriendly = false;
                SuspectBlip.Scale = 0.75f;
                SuspectArea.Delete();
                Game.DisplayHelp("Arrest the ~r~Suspect.");
                while (player.DistanceTo(Suspect) >= 5) GameFiber.Wait(0);
                Game.DisplaySubtitle("~g~You:~w~ Hey Sir, I Need to Speak With You!", 2500);
                GameFiber.Wait(1000);
                SuspectEnding();
            }
        }
        private void Search()
        {
            if (CalloutRunning)
            {
                Game.DisplaySubtitle("~g~You:~w~ No Sorry, I Can't Let You Come With Me. I'll Search for the Suspect Based on Your Information.", 3500);
                GameFiber.Wait(3500);
                Victim.Dismiss();
                if (VictimBlip.Exists()) VictimBlip.Delete();

                Game.DisplayHelp("Start ~o~Searching~w~ for the ~r~Suspect.");
                if (Main.CalloutInterface) CalloutInterfaceHandler.SendMessage(this, "Victim was assaulted on the bus. Searching for the Suspect.");
                SuspectArea = new Blip(Suspect.Position.Around(15), 250);
                SuspectArea.Color = Color.Orange;
                SuspectArea.Alpha = 0.5f;
                GameFiber.Wait(1500);
                Driver.Dismiss();

                System.Random coco = new System.Random();
                int WaitTime = coco.Next(35000, 69000); //35-69 seconds
                Game.LogTrivial("YOBBINCALLOUTS: Waiting " + WaitTime + " Seconds.");
                GameFiber.Wait(WaitTime);

                Suspect.Position = World.GetNextPositionOnStreet(player.Position.Around(100));
                Suspect.IsVisible = true;
                Suspect.Tasks.Wander();
                Game.DisplaySubtitle("~g~You:~w~ Hm, That Looks Like the Suspect Right There!", 2500);
                GameFiber.Wait(2000);
                SuspectBlip = Suspect.AttachBlip();
                SuspectBlip.IsFriendly = false;
                SuspectBlip.Scale = 0.75f;
                SuspectArea.Delete();
                Game.DisplayHelp("Arrest the ~r~Suspect.");
                while (player.DistanceTo(Suspect) >= 5) GameFiber.Wait(0);
                Game.DisplaySubtitle("~g~You:~w~ Hey Sir, I Need to Speak With You!", 2500);
                GameFiber.Wait(1000);
                SuspectEnding();
            }
        }
        private void SuspectEnding()
        {
            if (CalloutRunning)
            {
                if (SuspectAction == 0)
                {
                    Game.DisplayHelp("Arrest the ~r~Suspect.");
                    Suspect.Tasks.Clear();
                    while (Suspect.Exists())
                    {
                        GameFiber.Yield();
                        if (Suspect.IsDead || Functions.IsPedArrested(Suspect) || !Suspect.Exists()) break;
                    }
                    if (Suspect.Exists())
                    {
                        if (Functions.IsPedArrested(Suspect) || Suspect.IsAlive) { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect is Under ~g~Arrest~w~ for Assault."); }
                        else { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect Was ~r~Killed~w~."); }
                    }
                    else { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect Was ~r~Killed~w~."); }

                    GameFiber.Wait(2000);
                    LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("REPORT_RESPONSE_COPY_02");
                    GameFiber.Wait(2000);
                }
                else if (SuspectAction == 1) Fight();
                else Flee();
            }
        }
        private void Fight()
        {
            if (CalloutRunning)
            {
                Suspect.Tasks.ClearImmediately();
                Suspect.Inventory.Weapons.Clear();
                //Suspect.Inventory.GiveNewWeapon("WEAPON_KNIFE", -1, true);
                GameFiber.Wait(500);
                Suspect.Tasks.FightAgainst(Game.LocalPlayer.Character, -1);
                while (Suspect.Exists() && !Functions.IsPedArrested(Suspect) && Suspect.IsAlive)
                {
                    GameFiber.Yield();
                }
                if (Suspect.Exists())
                {
                    if (Functions.IsPedArrested(Suspect) || Suspect.IsAlive) { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect is Under ~g~Arrest~w~ Attempting to ~r~Assault an Officer."); }
                    else { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect Was ~r~Killed~w~ for ~r~Assaulting an Officer."); }
                }
                else { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect Was ~r~Killed~w~ Attempting to ~r~Assault an Officer."); }
                GameFiber.Wait(2000);
                Functions.PlayScannerAudio("REPORT_RESPONSE_COPY_02");
                GameFiber.Wait(2000);
            }
        }
        private void Flee()
        {
            if (CalloutRunning)
            {
                GameFiber.Wait(500);
                Suspect.Tasks.ClearImmediately();
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("CRIME_SUSPECT_ON_THE_RUN_01");
                MainPursuit = LSPD_First_Response.Mod.API.Functions.CreatePursuit();
                LSPD_First_Response.Mod.API.Functions.RequestBackup(Suspect.Position, LSPD_First_Response.EBackupResponseType.Code3, LSPD_First_Response.EBackupUnitType.LocalUnit);
                LSPD_First_Response.Mod.API.Functions.SetPursuitIsActiveForPlayer(MainPursuit, true);
                LSPD_First_Response.Mod.API.Functions.AddPedToPursuit(MainPursuit, Suspect);
                while (LSPD_First_Response.Mod.API.Functions.IsPursuitStillRunning(MainPursuit)) { GameFiber.Wait(0); }
                if (Suspect.Exists())
                {
                    if (Functions.IsPedArrested(Suspect)) { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect is Under ~g~Arrest~w~ Following the Pursuit."); }
                    else { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect Was ~r~Killed~w~ Following the Pursuit."); }
                }
                else { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect Was ~r~Killed~w~ Following the Pursuit."); }
                GameFiber.Wait(2000);
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("REPORT_RESPONSE_COPY_02");
                GameFiber.Wait(2000);
            }
        }
        public override void End()
        {
            base.End();
            if (CalloutRunning)
            {
                Game.DisplayNotification("Dispatch, Situation is ~b~Under Control.~w~ We Have Also Taken ~b~Witness Statements.");
                Game.DisplayNotification("~g~Code 4~w~, return to patrol.");
                Functions.PlayScannerAudio("ATTENTION_ALL_UNITS WE_ARE_CODE_4");
            }

            CalloutRunning = false;
            if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }
            if (VictimBlip.Exists()) { VictimBlip.Delete(); }
            if (DriverBlip.Exists()) { DriverBlip.Delete(); }
            if (Driver.Exists()) { Driver.Tasks.ClearImmediately(); }
            if (Driver.Exists()) { Driver.Dismiss(); }
            if (Suspect.Exists()) { Suspect.Dismiss(); }

            if (Witness2.Exists()) { Witness2.Dismiss(); }
            if (Victim.Exists()) { Victim.Tasks.ClearImmediately(); }
            if (Victim.Exists()) { Victim.Dismiss(); }
            if (Bus.Exists()) { Bus.IsPersistent = false; }
            if (SuspectArea.Exists()) SuspectArea.Delete();

            Game.LogTrivial("YOBBINCALLOUTS: Assault on Bus Callout Cleaned Up");
        }
    }
}
