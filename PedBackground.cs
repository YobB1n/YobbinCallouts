using Rage;
using LSPD_First_Response.Mod.API;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Collections;
using LSPD_First_Response.Engine.Scripting.Entities;

namespace YobbinCallouts
{
    class PedBackground
    {
        //PED RELATED
        public string fullName { get; }
        public string TimesStopped { get; }

        public bool Wanted { get; }
        public ArrayList MedicalProblems { get; set; }
        public string Gender { get; }
        Random monke = new Random();
        //GENERAL
        private ArrayList commonMedicalProblems = new ArrayList()
        {
            "Multiple Lacerations all over upper body and face",
            "Gunshot wounds in the thigh, arm, neck",
            "Pneumothorax",
            "Shattered Femur",
            "Grade 3 Concussion",
            "3rd Degree Burns",
            "Broken Nose",
            "Broken Orbital",
            "Stab wounds in the stomach",
            "Bruise marks on wrists and forearms"
        };
        private ArrayList commonMentalHealthProblems = new ArrayList()
        {
            "Depression",
            "Generalised anxiety disorder",
            "Panic Disorder",
            "Obsessive-Compulsive Disorder",
            "Post-Traumatic Stress Disorder",
            "Dissociative Identity Disorder",
            "Paranoid Personality Disorder",
            "Schizophrenia",
            "Social Anxiety Disorder",
            "Nosocomephobia(Fear of hospitals)"


        }; 
        private Persona pedPersona;

        /// <summary>
        /// constructor..you know how those work catYes
        /// </summary>
        /// <param name="ped"></param>
        public PedBackground(Ped ped)
        {
            if (ped.Exists())
            {
                pedPersona = Functions.GetPersonaForPed(ped);
                fullName = pedPersona.FullName;
                TimesStopped = pedPersona.TimesStopped.ToString();
                Wanted = pedPersona.Wanted;
                Gender = pedPersona.Gender.ToString();
                MedicalProblems = new ArrayList();

            }
            
        }
        /// <summary>
        /// sets medical problems for escaped suspect using the commonMedicalProblems arraylist
        /// </summary>
        public void setMedicalProblemsForEscapedSuspect()
        {
            MedicalProblems.Clear();
            ArrayList CMP = commonMedicalProblems;
            for (int i = 0; i < monke.Next(1, 3); i++)
            {
                int num = monke.Next(0, CMP.Count);
                MedicalProblems.Add(CMP[num]);
                CMP.RemoveRange(num,1);
            }
        }
        public void setMedicalProblemsForMentallyIllSuspect()
        {
            MedicalProblems.Clear();
            ArrayList CMHP = commonMentalHealthProblems;
            for (int i = 0; i < monke.Next(1, 3); i++)
            {
                int num = monke.Next(0, CMHP.Count);
                MedicalProblems.Add(CMHP[num]);
                CMHP.RemoveRange(num, 1);
            }
        }
        /// <summary>
        /// Easy way to print out medical problems in toString() method
        /// </summary>
        /// <param name="list"></param>
        /// <returns>Elements of arrays seperated by commas</returns>
        public string arrayToString(ArrayList list)
        {
            string str = "";
            for(int i = 0; i < list.Count;i++)
            {
                str += list[i] + ", ";
            }
            string newStr = str.Substring(0, str.Length - 1);   
            return newStr; 
        }

        override
        public string ToString()    
        {
            return "~w~ Wanted Status: ~y~" + Wanted + "\n" + "~w~ Times Stopped: ~o~ " + TimesStopped + "\n" + "~w~ Medical History: ~r~" + arrayToString(MedicalProblems);
            //\n  ~w~Symptoms: ~r~ {4}"; if symptoms are needed
        }


    }
}
