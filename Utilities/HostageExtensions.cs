using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace YobbinCallouts.Utilities
{
    public class HostageExtensions
    {
        //this helper is the logic the suspect is assigned in between dialogue advances in the hostage situation
        public static void HostageHold(Ped Suspect, Ped Hostage)
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
        public static string DialogueAdvance(List<string> dialogue)
        {
            System.Random twboop = new System.Random();
            int dialoguechosen = twboop.Next(0, dialogue.Count);
            return dialogue[dialoguechosen];
        }
    }
}
