using System.Collections.Generic;
using System.Windows.Forms;
using Rage;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage.Native;
using System;

namespace YobbinCallouts.Callouts
{
    [CalloutInfo("Traffic Break", CalloutProbability.Medium)] //Change later
    public class TrafficBreak : Callout
    {
        private Vector3 MainSpawnPoint;
        private Vector3 Accident;

        private Vector3 Beach = new Vector3(-2011.691f, -446.1866f, 11.36965f);
        private Vector3 BeachAccident = new Vector3(-1735.433f, -702.8215f, 10.01621f);
        private Vector3 LittleSoul = new Vector3(-409.8150f, -771.6809f, 37.1339f);
        private Vector3 LittleSoulAccident = new Vector3(-418.9631f, -1311.724f, 37.00277f);
        private Vector3 Strawberry = new Vector3(215.5652f, -1235.438f, 38.14722f);
        private Vector3 StrawberryAccident = new Vector3(633.7123f, -1216.477f, 42.32148f);
        private Vector3 Vinewood = new Vector3(16.10048f, -486.4405f, 33.82341f);
        private Vector3 VinewoodAccident = new Vector3(-551.613f, -487.6694f, 24.99862f);
        private Vector3 Desert = new Vector3(2601.184f, 3058.523f, 45.74524f);
        private Vector3 DesertAccident = new Vector3(2913.273f, 3675.457f, 52.55267f);
        private Vector3 Chiliad = new Vector3(1513.909f, 6425.665f, 22.97034f);
        private Vector3 ChiliadAccident = new Vector3(2057.506f, 6063.602f, 48.86989f);
        private Vector3 Tatamo = new Vector3(2465.136f, -151.0628f, 88.83215f);
        private Vector3 TatamoAccident = new Vector3(2631.071f, 355.748f, 96.81438f);
        private Vector3 Palomino = new Vector3(2092.122f, -595.4233f, 95.54442f);
        private Vector3 PalominoAccident = new Vector3(1629.288f, -933.3981f, 63.38768f);
        private Vector3 Docks = new Vector3(722.7678f, -2576.619f, 18.62388f);
        private Vector3 DocksAccident = new Vector3(715.9458f, -2798.863f, 6.28761f);


        private Blip Area;
        private Blip AccidentBlip;

        private Vehicle AccidentVehicle;
        private Vehicle Ambulance;
        private Vehicle TowTruck;

        private Ped Paramedic;
        private Ped Mechanic;

        private Rage.Object Pylon;

        private int MainScenario;

        private string Zone;

        private bool StartedTrafficBreak = false;
        private bool CalloutRunning = false;

        private readonly List<string> Instructions = new List<string>()
        {
         "Activate your ~y~emergency lights~w~ to slow traffic down. Aim for a ~g~slow speed~w~ to keep ~b~traffic moving~w~, but only just.",
         "Once you reach the yellow ~y~waypoint,~w~ cancel your emergency lights and let traffic ~g~speed up~w~ again.",
        };
        public override bool OnBeforeCalloutDisplayed()
        {
            Zone = Functions.GetZoneAtPosition(Game.LocalPlayer.Character.Position).GameName;
            Game.LogTrivial("YOBBINCALLOUTS: Zone is " + Zone);
            if (Zone == "Davis" || Zone == "AirP" || Zone == "Stad" || Zone == "STRAW" || Zone == "Banning" || Zone == "RANCHO" || Zone == "ChamH" || Zone == "PBOX" || Zone == "LegSqu" || Zone == "SKID" || Zone == "TEXTI")
            {
                MainSpawnPoint = Strawberry;
                Accident = StrawberryAccident;
            }
            else if (Zone == "Cypre" || Zone == "Murri" || Zone == "EBuro" || Zone == "LMesa" || Zone == "Mirr" || Zone == "East_V")
            {
                MainSpawnPoint = Strawberry;
                Accident = StrawberryAccident;  //might need to change these later
            }
            else if (Zone == "Vesp" || Zone == "VCana" || Zone == "Beach" || Zone == "DelSol" || Zone == "Koreat")
            {
                MainSpawnPoint = LittleSoul;
                Accident = LittleSoulAccident;
            }
            else if (Zone == "DeLBe" || Zone == "DelPe" || Zone == "Morn" || Zone == "PBluff" || Zone == "Movie")
            {
                MainSpawnPoint = Beach;
                Accident = BeachAccident;
            }
            else if (Zone == "Rockf" || Zone == "Burton" || Zone == "Richm" || Zone == "Golf")
            {
                MainSpawnPoint = Vinewood;
                Accident = VinewoodAccident;
            }
            else if (Zone == "CHIL" || Zone == "Vine" || Zone == "DTVine" || Zone == "WVine" || Zone == "Alta" || Zone == "Hawick")
            {
                MainSpawnPoint = Vinewood;
                Accident = VinewoodAccident;
            }
            else if (Zone == "Sandy" || Zone == "GrapeS" || Zone == "Desrt")
            {
                MainSpawnPoint = Desert;
                Accident = DesertAccident;
            }
            else if (Zone == "ProcoB" || Zone == "PalFor" || Zone == "Paleto" || Zone == "MTChil")
            {
                MainSpawnPoint = Chiliad;
                Accident = ChiliadAccident;
            }
            else if (Zone == "Tatamo") { MainSpawnPoint = Tatamo; Accident = TatamoAccident; }
            else if (Zone == "PalHigh") { MainSpawnPoint = Palomino; Accident = PalominoAccident; }
            else if (Zone == "Termina" || Zone == "Elysian") { MainSpawnPoint = Docks; Accident = DocksAccident; }
            else { Game.LogTrivial("YOBBINCALLOUTS: Player is not near any freeway. Choosing Another Random Location."); MainSpawnPoint = Desert; Accident = DesertAccident; }
            ShowCalloutAreaBlipBeforeAccepting(MainSpawnPoint, 100);    //Callout Blip Circle with radius 100m
            AddMinimumDistanceCheck(150f, MainSpawnPoint);   //Player must be 150m or further away
            Functions.PlayScannerAudio("WE_HAVE_01 CRIME_MOTOR_VEHICLE_ACCIDENT_02"); //Default
            CalloutMessage = "Traffic Break";
            CalloutPosition = MainSpawnPoint;
            CalloutAdvisory = "Perform a ~y~Traffic Break~w~ Following an Accident on the Freeway.";
            return base.OnBeforeCalloutDisplayed();
        }
        public override bool OnCalloutAccepted()
        {
            Game.LogTrivial("YOBBINCALLOUTS: Traffic Break Callout Accepted by User");
            if (Main.CalloutInterface)
            {
                CalloutInterfaceHandler.SendCalloutDetails(this, "CODE 2", "");
            }
            else
            {
                Game.DisplayNotification("Respond ~b~Code 2.~w~");
            }
            Area = new Blip(MainSpawnPoint, 50);
            Area.Color = System.Drawing.Color.Yellow;
            Area.Alpha = 0.67f;
            Area.IsRouteEnabled = true;
            Area.Name = "Traffic Break Start";

            System.Random r = new System.Random();
            int Scenario = r.Next(0, 0);
            switch (Scenario)   //Scenario Chooser, I don't think I'm gonna add another one for a while
            {
                case 0:
                    MainScenario = 0;
                    Game.LogTrivial("YOBBINCALLOUTS: Traffic Break Scenario 0 Chosen");

                    NativeFunction.Natives.GetClosestVehicleNodeWithHeading(Accident, out Vector3 nodePosition, out float heading, 1, 3.0f, 0); //for heading
                    CallHandler.SpawnVehicle(Accident, heading);
                    AccidentVehicle.IsPersistent = true;
                    AccidentVehicle.EngineHealth = 0;
                    AccidentVehicle.IsDeformationEnabled = true;

                    AccidentVehicle.Deform(AccidentVehicle.GetPositionOffset(AccidentVehicle.GetBonePosition("door_dside_f")), 100f, 700f);
                    AccidentVehicle.IsDriveable = false;
                    Game.LogTrivial("YOBBINCALLOUTS: Accident Vehicle Vehicle Spawned");

                    Ambulance = new Vehicle("AMBULANCE", AccidentVehicle.GetOffsetPositionFront(10f), AccidentVehicle.Heading);
                    Ambulance.IsPersistent = true;
                    Ambulance.IndicatorLightsStatus = VehicleIndicatorLightsStatus.Both;

                    TowTruck = new Vehicle("TOWTRUCK", AccidentVehicle.GetOffsetPositionFront(-10f), AccidentVehicle.Heading);
                    TowTruck.IsPersistent = true;
                    TowTruck.IndicatorLightsStatus = VehicleIndicatorLightsStatus.Both;

                    Paramedic = new Ped("s_m_m_paramedic_01", Ambulance.GetOffsetPositionFront(-7), Ambulance.Heading - 180);
                    Paramedic.BlockPermanentEvents = true;
                    Paramedic.IsPersistent = true;
                    Paramedic.Tasks.PlayAnimation("amb@medic@standing@timeofdeath@base", "base", -0.5f, AnimationFlags.Loop);

                    Mechanic = new Ped("s_m_m_trucker_01", AccidentVehicle.GetOffsetPositionRight(-1.5f), AccidentVehicle.Heading - 90);
                    Mechanic.BlockPermanentEvents = true;
                    Mechanic.IsPersistent = true;
                    Mechanic.Tasks.PlayAnimation("mini@repair", "fixing_a_ped", -1, AnimationFlags.Loop);

                    break;
            }
            if (CalloutRunning == false) { Callout(); }
            return base.OnCalloutAccepted();
        }
        public override void OnCalloutNotAccepted()
        {
            Game.LogTrivial("YOBBINCALLOUTS: Traffic Break Callout Not Accepted by User.");
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
                        if (MainScenario == 0)
                        {
                            while (Game.LocalPlayer.Character.DistanceTo(MainSpawnPoint) >= 50f && !Game.IsKeyDown(Config.CalloutEndKey))
                            {
                                GameFiber.Wait(0);
                            }
                            if (Game.IsKeyDown(Config.CalloutEndKey)) { EndCalloutHandler.CalloutForcedEnd = true; break; }

                            Game.LogTrivial("YOBBINCALLOUTS: Player is On Scene.");
                            Game.DisplaySubtitle("Press " + Config.MainInteractionKey + " to ~g~Start~w~ the Traffic Break.", 2000);
                            if (Config.DisplayHelp == true)
                            {
                                CallHandler.Dialogue(Instructions);
                            }
                            else
                            {
                                while (!Game.IsKeyDown(Config.MainInteractionKey)) { GameFiber.Wait(0); }
                            }
                            GameFiber.Wait(2500);
                            Game.DisplayNotification("Dispatch, We are Starting the ~y~Traffic Break.");
                            AccidentBlip = new Blip(Accident, 25);
                            AccidentBlip.Color = System.Drawing.Color.Yellow;
                            AccidentBlip.IsRouteEnabled = true;
                            AccidentBlip.Name = "Accident";
                            AccidentBlip.Alpha = 0.69f;
                            GameFiber.Wait(1000);
                            if (Area.Exists()) Area.Delete();
                            GameFiber.Wait(500);
                        }

                        while (Game.LocalPlayer.Character.DistanceTo(Accident) >= 25f)
                        {
                            GameFiber.Wait(0);
                        }
                        GameFiber.Wait(1500);
                        Game.DisplayNotification("Dispatch, Traffic Break ~b~Over. ~w~Traffic Moving Back to ~b~Normal.");
                        GameFiber.Wait(3000);
                        Functions.PlayScannerAudio("REPORT_RESPONSE_COPY_02");
                        if (AccidentBlip.Exists()) AccidentBlip.Delete();
                        GameFiber.Wait(1000);
                        break;
                    }
                    GameFiber.Wait(2500);
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
            if (Area.Exists()) { Area.Delete(); }
            if (AccidentBlip.Exists()) { AccidentBlip.Delete(); }
            if (TowTruck.Exists()) { TowTruck.IsPersistent = false; }
            if (AccidentVehicle.Exists()) { AccidentVehicle.IsPersistent = false; ; }
            if (Ambulance.Exists()) { Ambulance.IsPersistent = false; ; }
            if (Mechanic.Exists()) { Mechanic.Delete(); }
            if (Paramedic.Exists()) { Paramedic.Delete(); }
            if (Pylon.Exists()) Pylon.IsPersistent = false;

            Game.LogTrivial("YOBBINCALLOUTS: Traffic Break Callout Finished Cleaning Up.");
        }
        public override void Process()
        {
            base.Process();
        }
    }
}