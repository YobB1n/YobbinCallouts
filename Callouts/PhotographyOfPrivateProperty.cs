//BRUH THIS CALLOUT SUCKS

using System.Collections.Generic;
using Rage;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage.Native;
using System;

namespace YobbinCallouts.Callouts
{
    [CalloutInfo("Photography of Private Property", CalloutProbability.High)]

    public class PhotographyOfPrivateProperty : Callout
    {
        private Vector3 MainSpawnPoint;
        private Vector3 SuspectSpawnPoint;

        private Blip House;
        private Blip VictimBlip;
        private Blip SuspectBlip;

        private Ped Victim;
        private Ped Suspect;
        private Ped player = Game.LocalPlayer.Character;

        private int MainScenario; private int SuspectAction;

        private bool CalloutRunning = false;

        private string Zone;

        private Rage.Object Camera;

        //All the dialogue for the callout. Haven't found a better way to store it yet, so this will have to do.
        private readonly List<string> ResidentOpeningDialogue1 = new List<string>()
        {
         "~b~Caller:~w~ Hey officer, thanks for showing up for a change.",
         "~b~Caller:~w~ That person out there has been standing outside my house for over an hour.",
         "~b~Caller:~w~ At first, I didn't think anything of it, however when he didn't leave I decided to see what he was up to.",
         "~b~Caller:~w~ Turns out he's been taking photos of my house all this time for some reason.",
         "~b~Caller:~w~ The photos are of my property, and I don't want him to take anymore.",
         "~g~You:~w~ Alright, I'll go talk to them. Stay here please.",
        };
        private readonly List<string> ResidentOpeningDialogue2 = new List<string>()
        {
         "~b~Caller:~w~ Hey officer, hope you're doing well.",
         "~g~You:~w~ I'm doing alright, thank you. What seems to be the issue?",
         "~b~Caller:~w~ That person over there keeps taking photos of my house.",
         "~b~Caller:~w~ I don't know why, but when I asked him to leave, he refused.",
         "~g~You:~w~ What would you like me to do?",
         "~b~Caller:~w~ Just get him to leave, I don't feel comfortable with him taking pictures of my home.",
         "~g~You:~w~ Alright, I'll see what's up. Stay right here please.",
        };
        private readonly List<string> ResidentOpeningDialogue3 = new List<string>()
        {
         "~b~Caller:~w~ Thanks for getting here so quickly, officer.",
         "~g~You:~w~ Not a problem. Why did you call us?",
         "~b~Caller:~w~ That person over there keeps taking pictures of my house.",
         "~b~Caller:~w~ I respectfully asked him to leave, but he never did.",
         "~b~Caller:~w~ I don't feel comfortable with him doing that, officer.",
         "~g~You:~w~ Okay, I will talk with them now.",
         "~b~Caller:~w~ Thank you, officer.",
        };
        private readonly List<string> ResidentOpeningDialogue4 = new List<string>()
        {
         "~b~Caller:~w~ Thanks for getting here so quickly, officer.",
         "~g~You:~w~ Not a problem. Why did you call us?",
         "~b~Caller:~w~ That person over there keeps taking pictures of my house.",
         "~b~Caller:~w~ I respectfully asked him to leave, but he never did.",
         "~b~Caller:~w~ I don't feel comfortable with him doing that, officer.",
         "~g~You:~w~ Okay, I will talk with them now.",
         "~b~Caller:~w~ Thank you, officer.",
        };
        private readonly List<string> ResidentOpeningDialogue5 = new List<string>()
        {
         "~b~Caller:~w~ Thanks for getting here so quickly, officer.",
         "~g~You:~w~ Not a problem. Why did you call us?",
         "~b~Caller:~w~ That person over there keeps taking pictures of my house.",
         "~b~Caller:~w~ I respectfully asked him to leave, but he never did.",
         "~b~Caller:~w~ They've also trespassed on my property several times for some reason.",
         "~g~You:~w~ Alright, I'll speak to them. Stay right here please.",
        };
        private readonly List<string> ResidentOpeningDialogue6 = new List<string>()
        {
         "~b~Caller:~w~ Hey officer, hope you're doing well.",
         "~g~You:~w~ I'm doing alright, thank you. What seems to be the issue?",
         "~b~Caller:~w~ That person over there keeps taking photos of my house.",
         "~b~Caller:~w~ I don't know why, but when I asked him to leave, he refused.",
         "~b~Caller:~w~ They've also ventured on my property several times. I was fine with the photos, until they started trespassing.",
         "~g~You:~w~ What would you like me to do?",
         "~b~Caller:~w~ Just get him to leave, I don't feel comfortable with him taking pictures of my home.",
         "~g~You:~w~ Alright, I'll see what's up. Stay right here please.",
        };
        //scenario 1
        private readonly List<string> ResidentOpeningDialogue7 = new List<string>()
        {
         "~b~Caller:~w~ Hey officer, thanks for showing up for a change.",
         "~b~Caller:~w~ That person out there keeps taking photos of me without my permission.",
         "~b~Caller:~w~ I keep telling them to leave, but they keep following me.",
         "~b~Caller:~w~ If you could just tell them to fuck off, that'd be great.",
         "~g~You:~w~ Alright, I'll go talk to them. Stay here please.",
        };
        private readonly List<string> SuspectOpeningDialogue1 = new List<string>()
        {
         "~g~You:~w~ Hey, sorry to bother you.",
         "~g~You:~w~ May I ask what you are doing here?",
         "~r~Suspect:~w~ Just out taking some pictures, officer.",
         "~g~You:~w~ May I ask what you are taking pictures of?",
         "~g~You:~w~ The resident of that house said you were taking photos of their property.",
         "~r~Suspect:~w~ Is that right? I've just been walking around the neighbourhood taking photos of the trees.",
         "~r~Suspect:~w~ It's not a problem though, I can go somewhere else if you would like.",
         "~g~You:~w~ Yeah, that would be great, if you don't mind. Just to make them a little more comfortable.",
         "~r~Suspect:~w~ Of course, officer. Not an issue.",
         "~g~You:~w~ Thanks so much for your cooperation!",
        };
        private readonly List<string> SuspectOpeningDialogue2 = new List<string>()
        {
         "~g~You:~w~ Hey, sorry to bother you.",
         "~g~You:~w~ May I ask what you are doing here?",
         "~r~Suspect:~w~ Just out taking some pictures, officer. I got a new camera the other day.",
         "~g~You:~w~ Can I ask you what you are taking photos of?",
         "~r~Suspect:~w~ Yeah, just trying to get some pics of the sky, I want to see how well the photos turn out.",
         "~g~You:~w~ I see. You're not taking any pictures of this house over here?",
         "~r~Suspect:~w~ No officer, why?",
         "~g~You:~w~ The resident was concerned you may have been taking photos of their property.",
         "~r~Suspect:~w~ Oh, it must have been a misunderstanding. I've never taken any photos of that.",
         "~r~Suspect:~w~ It's no problem however, I can go somewhere else if you want me to.",
         "~g~You:~w~ Would you mind? That would make them feel more comfortable I'd say.",
         "~r~Suspect:~w~ Yeah no problem! I can take these photos anywhere, after all.",
         "~g~You:~w~ I really appreciate the cooperation. Have a great day!",
        };
        private readonly List<string> SuspectOpeningDialogue3 = new List<string>()
        {
         "~g~You:~w~ Sorry to bother you, I was wondering if I could ask what you are doing here?",
         "~r~Suspect:~w~ I'm just out taking some pictures of the houses around here.",
         "~r~Suspect:~w~ I'm a scout for a movie, I'm looking for suitable houses to potentially use for the filming. This is Los Santos, after all.",
         "~g~You:~w~ Oh, cool! Were you, by any chance, taking a photo of that house over there?",
         "~r~Suspect:~w~ Yes I was, it was one of the more suitable houses that met the criteria for the film.",
         "~r~Suspect:~w~ Why do you ask?",
         "~g~You:~w~ Yhe resident was a bit concerned, is all. Sounds like they didn't want their house photographed.",
         "~r~Suspect:~w~ Oh, sorry about that! I never meant to disturb anyone! Would you like me to go somewhere else?",
         "~g~You:~w~ That would be great, if you don't mind.",
         "~r~Suspect:~w~ Yeah, no problem. I guess we won't be using their house for the shoot anytime soon!",
         "~g~You:~w~ Haha, I guess not. Thanks so much for your understanding!",
        };
        private readonly List<string> SuspectOpeningDialogue4 = new List<string>()
        {
            "~g~You:~w~ Hey, sorry to bother you. I was wondering what you're doing around here?",
            "~r~Suspect:~w~ That's none of your business, officer. I'm not breaking any laws.",
            "~g~You:~w~ I never said you were, I was just asking what you're up to.",
            "~r~Suspect:~w~ I don't have to answer that. What's with all these questions?",
            "~g~You:~w~ One of the residents around here expressed concern about someone taking pictures of their property.",
            "~r~Suspect:~w~ That's not my problem officer. Please stop bothering me.",
            "~g~You:~w~ I'm simply asking you why you're taking photos here. It's not a complicated question.",
            "~r~Suspect:~w~ It isn't illegal to take pictures of somebody's house officer. I'm not obligated to talk to you anymore.",
        };
        private readonly List<string> SuspectOpeningDialogue5 = new List<string>()
        {
            "~g~You:~w~ Hey, sorry to bother you.",
            "~g~You:~w~ I just want to ask what you're doing here.",
            "~r~Suspect:~w~ Why do you ask, officer? Am I doing something wrong?",
            "~g~You:~w~ No, I'm just wondering what you're up to. Some residents want to know what you're doing as well.",
            "~r~Suspect:~w~ I'm just minding my own business. Please stop harassing me if I haven't done anything wrong officer.",
            "~g~You:~w~ I'm not harassing you, I just want to know what you're doing here. Neighbours have said you've been around here awhile.",
            "~r~Suspect:~w~ I'm not going to talk to you anymore officer. Are you gonna arrest me now or what?",
        };
        //SCENARIO 1
        private readonly List<string> SuspectOpeningDialogue6 = new List<string>()
        {
            "~g~You:~w~ Hey, sorry to bother you.",
            "~g~You:~w~ I just want to ask what you're doing here.",
            "~r~Suspect:~w~ Why do you ask, officer? Am I doing something wrong?",
            "~g~You:~w~ Bo, I'm just wondering what you're up to. Apparently someone asked you to stop taking pictures of them.",
            "~r~Suspect:~w~ I'm just minding my own business. Please stop harassing me if I haven't done anything wrong officer.",
            "~g~You:~w~ I'm not harassing you, I just want to know why you're taking photos of them. They've said you refused to leave.",
            "~r~Suspect:~w~ I'm not going to talk to you anymore officer. Are you gonna arrest me now or what?",
        };
        private readonly List<string> SuspectOpeningDialogue7 = new List<string>()
        {
            "~g~You:~w~ hey, sorry to bother you.",
            "~g~You:~w~ I just want to ask what you're doing here.",
            "~r~Suspect:~w~ why do you ask, officer? Am I doing something wrong?",
            "~g~You:~w~ no, I'm just wondering what you're up to. Apparently someone asked you to stop taking pictures of them.",
            "~r~Suspect:~w~ I think that must have been a misunderstanding! I would never take photos of someone without consent!",
            "~g~You:~w~ alright, no problem. All I ask is that you head in another direction from that person over there.",
            "~r~Suspect:~w~ no problem officer. Sorry for the misunderstanding!",
        };
        private readonly List<string> ResidentEndingDialogue1 = new List<string>()
        {
         "~g~You:~w~ Alright, so I talked to the person and they agreed to move somewhere else.",
         "~g~You:~w~ They were very apologetic, I'm sure they meant no harm.",
         "~b~Caller:~w~ Thanks, officer. I just didn't want any issues, y'know?",
         "~g~You:~w~ Yeah, of course. Is there anything else I can do for you?",
         "~b~Caller:~w~ No, that's it. Thanks for your help!",
        };
        private readonly List<string> ResidentEndingDialogue2 = new List<string>()
        {
         "~g~You:~w~ Alright, so I spoke with the photographer, and they agreed to go somewhere else.",
         "~g~You:~w~ They didn't want any trouble, and were very understanding.",
         "~b~Caller:~w~ Sounds good officer. I just don't like the idea of people potentially taking pictures of my house, I guess.",
         "~g~You:~w~ Sure, I understand. Is there anything else I can help you with today?",
         "~b~Caller:~w~ No, that's about it. Thanks for the help!",
        };
        private readonly List<string> ResidentEndingDialogue3 = new List<string>()
        {
         "~g~You:~w~ So I spoke with the person and they were happy to move somewhere else.",
         "~b~Caller:~w~ Great. Thanks so much for that officer.",
         "~g~You:~w~ No problem, they were more than willing to do so, I know they didn't mean any harm.",
         "~b~Caller:~w~ Awesome. Thanks for the help today, officer!",
        };
        private readonly List<string> ResidentEndingDialogue4 = new List<string>()
        {
         "~b~Caller:~w~ Well, what's the deal?",
         "~g~You:~w~ I spoke with the person, unfortunately they refused to move.",
         "~g~You:~w~ I understand your concern, however there really isn't much I can do here.",
         "~g~You:~w~ They're legally allowed to take pictures of your property, as long as they aren't trespassing.",
         "~b~Caller:~w~ I see. Well, I'm sure it won't be a big deal. Thanks for the help anyways, officer.",
         "~g~You:~w~ No problem. Have a good day.",
        };
        private readonly List<string> ResidentEndingDialogue5 = new List<string>()
        {
         "~g~You:~w~ So I spoke with the person, however they refused to leave unfortunately.",
         "~b~Caller:~w~ So there's nothing you can do about them taking pictures of my property without consent?",
         "~g~You:~w~ Unfortunately no. They are legally allowed to take photos as long as they aren't trespassing.",
         "~b~Caller:~w~ Really? That seems unreasonable. They've been around here for hours and refuses to leave! What am I supposed to do?",
         "~g~You:~w~ If they trespass on your property, you can call us and we can have them trespassed.",
         "~g~You:~w~ However, as it stands there isn't anything else I can do, unfortunately.",
         "~b~Caller:~w~ Okay, thanks for the explanation. If they do anything else I'll call you guys.",
         "~g~You:~w~ Sounds good, have a nice day.",
        };
        private readonly List<string> ResidentEndingDialogue6 = new List<string>()
        {
         "~b~Caller:~w~ What happened, officer?",
         "~g~You:~w~ I spoke to the person, however they did not want to talk to me.",
         "~g~You:~w~ As far as I can tell, they haven't done anything wrong, and I can't arrest somebody just for taking pictures of your property.",
         "~b~Caller:~w~ Really, officer? They have been out here for hours, and refused to leave. It is making me very uncomfortable!",
         "~g~You:~w~ Unfortunately, they are allowed to do that. They never trespassed on your property, right?",
         "~b~Caller:~w~ Well no, at least not yet.",
         "~g~You:~w~ Okay, well if they ever do, you can let us know and we can have them trespassed. Otherwise I can't do anything else at this point.",
         "~b~Caller:~w~ I see. Thanks for trying anyways.",
         "~g~You:~w~ Not a problem. Take care.",
        };
        private readonly List<string> ResidentEndingDialogue7 = new List<string>()
        {
         "~g~You:~w~ The suspect refused to speak with me, so I've decided to arrest them for the time being.",
         "~b~Caller:~w~ Okay, thanks a lot officer. I really didn't like them doing that.",
         "~g~You:~w~ I understand that. Is there anything else I can do for you?",
         "~b~Caller:~w~ No, that's pretty much it. Thanks for the help.",
        };

        public override bool OnBeforeCalloutDisplayed()
        {
            Game.LogTrivial("==========YOBBINCALLOUTS: Photography of Private Property Callout Start==========");
            System.Random r = new System.Random();
            int Scenario = r.Next(0, 0);
            Scenario = MainScenario;

            //HOUSE CHOOSER FOR 1ST SCENARIO
            Zone = Functions.GetZoneAtPosition(Game.LocalPlayer.Character.Position).GameName;
            Game.LogTrivial("YOBBINCALLOUTS: Zone is " + Zone);
            CallHandler.locationChooser(CallHandler.HouseList);
            if (CallHandler.locationReturned) { MainSpawnPoint = CallHandler.SpawnPoint; }
            else
            {
                MainScenario = 1;
                MainSpawnPoint = World.GetNextPositionOnStreet(player.Position.Around(550f));
            }

            ShowCalloutAreaBlipBeforeAccepting(MainSpawnPoint, 75f);    //Callout Blip Circle with radius 50m
            AddMinimumDistanceCheck(25f, MainSpawnPoint);   //Player must be 25m or further away
            Functions.PlayScannerAudio("CITIZENS_REPORT CRIME_DISTURBING_THE_PEACE_01");
            CalloutMessage = "Photography of Private Property";
            CalloutPosition = MainSpawnPoint;
            if (MainScenario == 0) CalloutAdvisory = "Caller Has Expressed Concern With Someone ~r~Taking Pictures~w~ of their ~y~Property.";
            else CalloutAdvisory = "Caller Has Expressed Concern With Someone ~r~Taking Pictures~w~ of Them.";
            return base.OnBeforeCalloutDisplayed();
        }
        public override bool OnCalloutAccepted()
        {
            Game.LogTrivial("YOBBINCALLOUTS: Photography of Private Property Callout Accepted by User.");
            if (Main.CalloutInterface)
            {
                CalloutInterfaceHandler.SendCalloutDetails(this, "CODE 2", "");
            }
            else
            {
                Game.DisplayNotification("Respond ~b~Code 2.");
            }
            if (MainScenario == 0)
            {
                Victim = new Ped(MainSpawnPoint, 360);
                Victim.IsPersistent = true;
                Victim.BlockPermanentEvents = true;
                Game.LogTrivial("YOBBINCALLOUTS: Victim Spawned.");

                SuspectSpawnPoint = World.GetNextPositionOnStreet(MainSpawnPoint);
                NativeFunction.Natives.xA0F8A7517A273C05<bool>(SuspectSpawnPoint, 0, out Vector3 outPosition);
                Suspect = new Ped(outPosition, 360);
                Suspect.IsPersistent = true;
                Suspect.BlockPermanentEvents = true;
            }
            else
            {
                SuspectSpawnPoint = World.GetNextPositionOnStreet(MainSpawnPoint);
                NativeFunction.Natives.xA0F8A7517A273C05<bool>(SuspectSpawnPoint, 0, out Vector3 outPosition);
                Suspect = new Ped(outPosition, 360);
                Suspect.IsPersistent = true;
                Suspect.BlockPermanentEvents = true;
                Suspect.Position = Suspect.GetOffsetPositionRight(2);
                //ADD SUSPECT PHOTOGRAPHY ANIMATION

                Victim = new Ped(Suspect.GetOffsetPositionFront(-10), 360);
                Victim.IsPersistent = true;
                Victim.BlockPermanentEvents = true;
                Game.LogTrivial("YOBBINCALLOUTS: Victim Spawned.");
            }

            System.Random r = new System.Random();
            int SuspectCamera = r.Next(0, 2);
            if (SuspectCamera == 0) Camera = new Rage.Object("prop_npc_phone", Vector3.Zero);
            else Camera = new Rage.Object("prop_pap_camera_01", Vector3.Zero);
            Camera.AttachTo(Suspect, Suspect.GetBoneIndex(PedBoneId.LeftHand), new Vector3(0.1490f, 0.0560f, -0.0100f), new Rotator(-17f, -142f, -151f));
            Game.LogTrivial("YOBBINCALLOUTS: Spawned Suspect, Gave Them Camera");

            House = new Blip(MainSpawnPoint, 20f);
            House.Alpha = 0.67f;
            House.IsRouteEnabled = true;
            House.Color = System.Drawing.Color.Yellow;
            House.Name = "Callout Location";

            System.Random ryuy = new System.Random();
            SuspectAction = ryuy.Next(0, 3);
            Game.LogTrivial("YOBBINCALLOUTS: SuspectAction Value is " + SuspectAction);

            if (CalloutRunning == false) Callout();
            return base.OnCalloutAccepted();
        }
        public override void OnCalloutNotAccepted()
        {
            Game.LogTrivial("YOBBINCALLOUTS: Photography of Private Property Callout Not Accepted by User.");
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
                    while (CalloutRunning == true)
                    {
                        while (Game.LocalPlayer.Character.DistanceTo(MainSpawnPoint) >= 35f && !Game.IsKeyDown(Config.CalloutEndKey)) { GameFiber.Wait(0); }
                        if (Game.IsKeyDown(Config.CalloutEndKey))
                        {
                            EndCalloutHandler.CalloutForcedEnd = true; break;
                        }
                        TalkToVictim();
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
                        Game.DisplayNotification("There was an ~r~Error~w~ Caught with ~b~YobbinCallouts. ~w~Please Check Your ~g~Log File.~w~ Sorry for the Inconvenience!");
                        Game.DisplayNotification("Error: ~r~" + error);
                        Game.LogTrivial("If You Believe this is a Bug, Please Report it on my Discord Server. Thanks!");
                        Game.LogTrivial("==========YOBBINCALLOUTS: ERROR CAUGHT==========");
                        End();
                    }
                    else
                    {
                        Game.LogTrivial("==========YOBBINCALLOUTS: ERROR CAUGHT - CALLOUT NO LONGER RUNNING==========");
                        string error = e.ToString();
                        Game.LogTrivial("ERROR: " + error);
                        Game.LogTrivial("No Need to Report This Error if it Did not Result in an LSPDFR Crash.");
                        Game.LogTrivial("==========YOBBINCALLOUTS: ERROR CAUGHT - CALLOUT NO LONGER RUNNING==========");
                    }
                }
            }
            );
        }
        public override void End()
        {
            base.End();
            if (CalloutRunning)
            {
                Functions.PlayScannerAudio("ATTENTION_ALL_UNITS WE_ARE_CODE_4");
                Game.DisplayNotification("~g~Code 4~w~, return to patrol.");
            }

            CalloutRunning = false;
            if (Victim.Exists()) { Victim.Tasks.ClearImmediately(); }
            if (Victim.Exists()) { Victim.Dismiss(); }
            if (Suspect.Exists()) { Suspect.Dismiss(); }
            if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }
            if (VictimBlip.Exists()) { VictimBlip.Delete(); }
            if (House.Exists()) { House.Delete(); }
            if (Camera.Exists()) { Camera.Delete(); }
            Game.LogTrivial("YOBBINCALLOUTS: Photography of Private Property Callout Finished Cleaning Up.");
        }
        public override void Process()
        {
            base.Process();
        }
        private void TalkToVictim()
        {
            if (CalloutRunning)
            {
                if (MainScenario == 0)
                {
                    if (House.Exists()) { House.Delete(); }
                    VictimBlip = new Blip(Victim.Position);
                    VictimBlip.Scale = 0.65f;
                    VictimBlip.Color = System.Drawing.Color.Blue;
                    VictimBlip.Name = "Resident";
                    Game.DisplayHelp("Talk to the ~b~Resident.");
                    NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(Victim, player, -1);
                    while (Game.LocalPlayer.Character.DistanceTo(Victim) >= 5f) { GameFiber.Wait(0); }

                    if (Config.DisplayHelp) Game.DisplayHelp("Press ~y~" + Config.MainInteractionKey + " ~w~to Speak With the ~b~Caller.");
                    System.Random r2 = new System.Random();
                    int ResidentOpening = r2.Next(0, 3);

                    switch (ResidentOpening)
                    {
                        case 0:
                            CallHandler.Dialogue(ResidentOpeningDialogue1, Victim);
                            break;
                        case 1:
                            CallHandler.Dialogue(ResidentOpeningDialogue2, Victim);
                            break;
                        case 2:
                            CallHandler.Dialogue(ResidentOpeningDialogue3, Victim);
                            break;
                    }
                    if (Victim.Exists()) { Victim.Tasks.ClearImmediately(); }
                    if (VictimBlip.Exists()) { VictimBlip.Delete(); }
                    CallHandler.IdleAction(Victim, false);
                    GameFiber.Wait(2000);

                    Game.DisplayHelp("Talk to the ~r~Suspect.");
                    SuspectBlip = Suspect.AttachBlip();
                    SuspectBlip.Scale = 0.65f;
                    SuspectBlip.Color = System.Drawing.Color.Red;
                    SuspectBlip.IsFriendly = false;
                    SuspectBlip.Name = "Suspect";

                    while (Game.LocalPlayer.Character.DistanceTo(Suspect) >= 6.9f) { GameFiber.Wait(0); }
                    Suspect.Tasks.AchieveHeading(Game.LocalPlayer.Character.Heading - 180).WaitForCompletion(750);
                    if (Config.DisplayHelp) Game.DisplayHelp("Press ~y~" + Config.MainInteractionKey + "~w~ to Speak with the ~r~Suspect.");

                    System.Random sam = new System.Random();
                    int SuspectOpening = sam.Next(0, 3);
                    if (SuspectAction <= 1)    //Suspect is Cooperative
                    {
                        switch (SuspectOpening)
                        {
                            case 0:
                                CallHandler.Dialogue(SuspectOpeningDialogue1, Suspect);
                                break;
                            case 1:
                                CallHandler.Dialogue(SuspectOpeningDialogue2, Suspect);

                                break;
                            case 2:
                                CallHandler.Dialogue(SuspectOpeningDialogue3, Suspect);

                                break;
                        }
                        GameFiber.Wait(1000);
                        if (Suspect.Exists()) { Suspect.Tasks.ClearImmediately(); Suspect.Dismiss(); }
                        if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }
                        GameFiber.Wait(2000);
                        Game.DisplayHelp("Inform the ~b~Resident.");
                    }
                    else
                    {
                        switch (SuspectOpening) //Suspect is Not Cooperative
                        {
                            case 0:
                                CallHandler.Dialogue(SuspectOpeningDialogue4, Suspect);
                                break;
                            case 1:
                                CallHandler.Dialogue(SuspectOpeningDialogue5, Suspect);
                                break;
                            case 2:
                                CallHandler.Dialogue(SuspectOpeningDialogue5, Suspect);
                                break;
                        }
                        GameFiber.Wait(2000);
                        if (Suspect.Exists()) { Suspect.Tasks.ClearImmediately(); }
                        Game.DisplayHelp("Deal With the ~r~Suspect~w~ as You See Fit. When Finished, Speak With the ~b~Resident.");
                        VictimBlip = Victim.AttachBlip();
                        VictimBlip.Scale = 0.65f;
                        VictimBlip.Color = System.Drawing.Color.Blue;
                        VictimBlip.Name = "Caller";
                        while (Game.LocalPlayer.Character.DistanceTo(Victim) >= 6f) { GameFiber.Wait(0); }
                        if (Config.DisplayHelp) Game.DisplayHelp("Press ~y~" + Config.MainInteractionKey + "~w~ to Finish with the ~b~Caller.");

                    }

                    if (Suspect.Exists() && !Functions.IsPedArrested(Suspect)) //suspect not arrested
                    {
                        System.Random rondom = new System.Random(); //PAGE 'O RONDOM
                        int VictimEnding = rondom.Next(0, 3);
                        if (SuspectAction > 1)
                        {
                            switch (VictimEnding) //suspect not cooperative
                            {
                                case 0:
                                    CallHandler.Dialogue(ResidentEndingDialogue4, Victim);
                                    break;
                                case 1:
                                    CallHandler.Dialogue(ResidentEndingDialogue5, Victim);
                                    break;
                                case 2:
                                    CallHandler.Dialogue(ResidentEndingDialogue6, Victim);
                                    break;
                            }
                        }
                        else
                        {
                            switch (VictimEnding) //suspect cooperative
                            {
                                case 0:
                                    CallHandler.Dialogue(ResidentEndingDialogue1, Victim);
                                    break;
                                case 1:
                                    CallHandler.Dialogue(ResidentEndingDialogue2, Victim);
                                    break;
                                case 2:
                                    CallHandler.Dialogue(ResidentEndingDialogue3, Victim);
                                    break;
                            }
                        }
                        Victim.Tasks.ClearImmediately();
                        GameFiber.Wait(1500);
                        Victim.Dismiss();
                    }
                    else
                    {
                        CallHandler.Dialogue(ResidentEndingDialogue7, Victim);

                        Victim.Tasks.ClearImmediately();
                        GameFiber.Wait(1500);
                        Victim.Dismiss();
                    }
                    if (Suspect.Exists() && Suspect.IsCuffed) { Game.DisplayNotification("Dispatch, We Have ~b~Arrested~w~ One Suspect for Taking Pictures of Private Property."); }
                    else { Game.DisplayNotification("Dispatch, Dispute is ~b~Resolved~w~. The Suspect ~b~Never Trespassed~w~ On Their Property."); }
                }
                else  //=====SCENARIO 1======
                {
                    while (Game.LocalPlayer.Character.DistanceTo(Victim) >= 20f && !Game.IsKeyDown(Config.CalloutEndKey)) GameFiber.Wait(0);
                    if (Game.IsKeyDown(Config.CalloutEndKey)) End();
                    if (House.Exists()) House.Delete();

                    VictimBlip = new Blip(Victim.Position);
                    VictimBlip.Scale = 0.65f;
                    VictimBlip.Color = System.Drawing.Color.Blue;
                    Game.DisplayHelp("Talk to the ~b~Caller.");
                    NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(Victim, player, -1);
                    while (Game.LocalPlayer.Character.DistanceTo(Victim) >= 5f) { GameFiber.Wait(0); }

                    if (Config.DisplayHelp == true)
                    {
                        Game.DisplayHelp("Press ~y~" + Config.MainInteractionKey + "~w~ to Speak with the ~b~Caller.");
                    }
                    CallHandler.Dialogue(ResidentOpeningDialogue7, Victim);


                    if (Victim.Exists()) { Victim.Tasks.ClearImmediately(); }
                    if (VictimBlip.Exists()) { VictimBlip.Delete(); }
                    GameFiber.Wait(2000);

                    Game.DisplayHelp("Talk to the ~r~Suspect.");
                    SuspectBlip = Suspect.AttachBlip();
                    SuspectBlip.Scale = 0.65f;
                    SuspectBlip.Color = System.Drawing.Color.Red;
                    SuspectBlip.IsFriendly = false;

                    while (Game.LocalPlayer.Character.DistanceTo(Suspect) >= 6f) { GameFiber.Wait(0); }
                    if (Config.DisplayHelp == true)
                    {
                        Game.DisplayHelp("Press ~y~" + Config.MainInteractionKey + "~w~ to Speak with the ~r~Suspect.");
                    }
                    if (SuspectAction <= 1)    //Suspect is Cooperative
                    {
                        CallHandler.Dialogue(SuspectOpeningDialogue7, Suspect);

                        GameFiber.Wait(2000);
                        if (Suspect.Exists()) { Suspect.Tasks.ClearImmediately(); Suspect.Dismiss(); }
                        if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }
                        GameFiber.Wait(2000);

                        Game.DisplayHelp("Talk to the ~b~Caller~w~ to Finish the Callout.");
                        VictimBlip = new Blip(Victim.Position);
                        VictimBlip.Scale = 0.65f;
                        VictimBlip.Color = System.Drawing.Color.Blue;
                        NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(Victim, player, -1);
                        while (Game.LocalPlayer.Character.DistanceTo(Victim) >= 5f) { GameFiber.Wait(0); }

                        if (Config.DisplayHelp == true)
                        {
                            Game.DisplayHelp("Press ~y~" + Config.MainInteractionKey + "~w~ to Finish with the ~b~Caller.");
                        }
                        CallHandler.Dialogue(ResidentEndingDialogue1, Victim);

                        Victim.Tasks.ClearImmediately();
                        GameFiber.Wait(1500);
                        if (VictimBlip.Exists()) { VictimBlip.Delete(); }
                        Victim.Dismiss();
                        if (Suspect.Exists() && Functions.IsPedArrested(Suspect)) { Game.DisplayNotification("Dispatch, We Have ~b~Arrested~w~ One Suspect for Taking Pictures Without Consent."); }
                        else { Game.DisplayNotification("Dispatch, Dispute is ~b~Resolved~w~. The Suspect ~b~Agreed to Leave."); }
                    }
                    else
                    {
                        CallHandler.Dialogue(SuspectOpeningDialogue6, Suspect);

                        GameFiber.Wait(2000);
                        if (Suspect.Exists()) { Suspect.Tasks.ClearImmediately(); }
                        Game.DisplayHelp("Deal With the ~r~Suspect~w~ as You See Fit. When Finished, Speak With the ~b~Caller.");
                        VictimBlip = new Blip(Victim.Position);
                        VictimBlip.Scale = 0.65f;
                        VictimBlip.Color = System.Drawing.Color.Blue;

                        while (Game.LocalPlayer.Character.DistanceTo(Victim) >= 5f) { GameFiber.Wait(0); }
                        if (Config.DisplayHelp == true)
                        {
                            Game.DisplayHelp("Press~y~ " + Config.MainInteractionKey + "~w~ to Finish with the ~b~Caller.");
                        }
                        if (Suspect.Exists() && !Functions.IsPedArrested(Suspect))
                        {
                            CallHandler.Dialogue(ResidentEndingDialogue4, Victim);
                            Victim.Tasks.ClearImmediately();
                            GameFiber.Wait(1500);
                            Victim.Dismiss();
                        }
                        else
                        {
                            CallHandler.Dialogue(ResidentEndingDialogue7, Victim);
                            Victim.Tasks.ClearImmediately();
                            GameFiber.Wait(1500);
                            Victim.Dismiss();
                        }
                    }
                }
            }
        }
    }
}
