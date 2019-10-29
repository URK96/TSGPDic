using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace TSGPDic
{
    internal class Music
    {
        public string Name { get; private set; }
        public string CodeName { get; private set; }
        public string Artist { get; private set; }
        public string Type { get; private set; }
        public int[] Level { get; private set; }
        public string BPM { get; private set; }
        public string AddVersion { get; private set; }
        public string AddVersion_Full { get; private set; }
        public string UpdateDate { get; private set; }
        public string EarnRoute { get; private set; }
        public string[] Missions { get; private set; }
        public int MissionCompleteReward { get; private set; }

        internal Music(DataRow dr)
        {
            Name = (string)dr["Name"];
            CodeName = (string)dr["CodeName"];
            Artist = (string)dr["Artist"];
            Type = (string)dr["Type"];

            Level = new int[4];
            string[] level_temp = ((string)dr["Level"]).Split(',');
            for (int i = 0; i < Level.Length; ++i)
                Level[i] = int.Parse(level_temp[i]);

            BPM = (string)dr["BPM"];
            AddVersion = (string)dr["AddVersion"];
            SetAddVersion();

            UpdateDate = (string)dr["UpdateDate"];
            EarnRoute = (string)dr["EarnRoute"];
            Missions = ((string)dr["Mission"]).Split(';');
            MissionCompleteReward = (int)dr["MissionCompleteReward"];
        }

        private void SetAddVersion()
        {
            switch (AddVersion)
            {
                case "DMO":
                    AddVersion_Full = "DJMAX Online";
                    break;
                case "DMP1":
                    AddVersion_Full = "DJMAX Portable 1";
                    break;
                case "DMP2":
                    AddVersion_Full = "DJMAX Portable 2";
                    break;
                case "DMP3":
                    AddVersion_Full = "DJMAX Portable 3";
                    break;
                case "DMPBS":
                    AddVersion_Full = "DJMAX Portable Black Square";
                    break;
                case "DMPCE":
                    AddVersion_Full = "DJMAX Portable Clazziqui Edition";
                    break;
                case "DMR":
                    AddVersion_Full = "DJMAX Respect";
                    break;
                case "DMRAY":
                    AddVersion_Full = "DJMAX RAY";
                    break;
                case "DMT":
                    AddVersion_Full = "DJMAX Trilogy";
                    break;
                case "DMT1":
                    AddVersion_Full = "DJMAX TECHNIKA 1";
                    break;
                case "DMT2":
                    AddVersion_Full = "DJMAX TECHNIKA 2";
                    break;
                case "DMT3":
                    AddVersion_Full = "DJMAX TECHNIKA 3";
                    break;
                case "DMTT":
                    AddVersion_Full = "DJMAX TECHNIKA TUNE";
                    break;
                case "DMTQ":
                    AddVersion_Full = "DJMAX TECHNIKA Q";
                    break;
                case "BMS":
                    AddVersion_Full = "BMS";
                    break;
                case "HS12":
                    AddVersion_Full = "HARDCORE SYNDROME 12";
                    break;
                case "MuseMaker":
                    AddVersion_Full = "뮤즈메이커";
                    break;
                case "TSGP":
                    AddVersion_Full = "TAPSONIC TOP";
                    break;
                case "TSO":
                    AddVersion_Full = "TAPSONIC Original";
                    break;
                case "TWC":
                    AddVersion_Full = "TAPSONIC World Champion";
                    break;
                case "요구르팅":
                    AddVersion_Full = "요구르팅";
                    break;
                default:
                    AddVersion_Full = "";
                    break;
            }
        }
    }

    internal class Star
    {
        public string Name { get; private set; }
        public string CodeName { get; private set; }
        public int DicNumber { get; private set; }
        public string Attribute { get; private set; }
        public string Type { get; private set; }
        public int BornGrade { get; private set; }
        public int Status { get; private set; }
        public string SkillCode { get; private set; }
        public string SkillName { get; private set; }
        public string SkillExplain { get; private set; }
        public string[] SkillEffect { get; private set; }
        public string[] SkillMag { get; private set; }
        public bool HasLeaderSkill { get; private set; }
        public string[] LeaderSkillMag { get; private set; }
        public string LeaderSkillTypeA { get; private set; }
        public string[] LeaderSkillMagA { get; private set; }
        public int[] RelativeStar { get; private set; }
        public string Age { get; private set; }
        public string BloodType { get; private set; }
        public string StarExplain { get; private set; }
        public string[] Costumes { get; private set; }

        internal Star(DataRow dr, bool ReadStarExplain = false)
        {
            Name = (string)dr["Name"];
            CodeName = (string)dr["CodeName"];
            DicNumber = (int)dr["DicNumber"];
            Attribute = (string)dr["Attribute"];
            Type = (string)dr["Type"];
            BornGrade = (int)dr["BornGrade"];
            Status = 0;
            SkillName = (string)dr["Skill"];
            SkillExplain = (string)dr["SkillExplain"];
            SkillEffect = ((string)dr["SkillEffect"]).Split(',');
            SkillMag = ((string)dr["SkillMag"]).Split(',');
            HasLeaderSkill = (bool)dr["HasLeaderSkill"];

            if (HasLeaderSkill == true)
            {
                LeaderSkillMag = ((string)dr["LeaderSkillMag"]).Split(',');
                LeaderSkillTypeA = (string)dr["LeaderSkillTypeA"];
                LeaderSkillMagA = ((string)dr["LeaderSkillMagA"]).Split(',');
            }

            try
            {
                string[] r_stars = ((string)dr["RelativeStar"]).Split(';');
                for (int i = 0; i < r_stars.Length; ++i)
                    RelativeStar[i] = int.Parse(r_stars[i]);
            }
            catch (Exception)
            {
                RelativeStar = null;
            }

            Age = (string)dr["Age"];
            BloodType = (string)dr["BloodType"];
            if (ReadStarExplain == true) StarExplain = (string)dr["StarExplain"];
            else StarExplain = "";
            
            try
            {
                Costumes = ((string)dr["Costume"]).Split(';');
            }
            catch (Exception)
            {
                Costumes = null;
            }
        }
    }
}