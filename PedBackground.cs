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
        public DateTime Birthday { get; }
        public string TimesStopped { get; }
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
                Birthday = pedPersona.Birthday;
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
            ArrayList CMP = (ArrayList)commonMedicalProblems.Clone();
            for (int i = 0; i < monke.Next(1, 4); i++)
            {
                int num = monke.Next(0, CMP.Count);
                MedicalProblems.Add(CMP[num]);
                CMP.Remove(num);
            }
        }
        public void setMedicalProblemsForMentallyIllSuspect()
        {
            MedicalProblems.Clear();
            ArrayList CMHP = (ArrayList)commonMentalHealthProblems.Clone();
            for (int i = 0; i < monke.Next(1, 4); i++)
            {
                int num = monke.Next(0, CMHP.Count);
                MedicalProblems.Add(CMHP[num]);
                CMHP.Remove(num);
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
            foreach(string item in list)
            {
                str += item + ", ";
            }
            return str; 
        }

        public string toString()
        {
            return String.Format("~w~Date of Birth: ~y~ {0} \n ~w~Sex: ~y~ {1} \n  ~w~Times Stopped: ~o~ {2} \n  ~w~Medical History: ~r~ {3}", Birthday.ToString(), Gender, TimesStopped, arrayToString(MedicalProblems));
            //\n  ~w~Symptoms: ~r~ {4}"; if symptoms are needed
        }


    }
}
