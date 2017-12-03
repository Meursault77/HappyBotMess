using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD40.Actors
{
    public static class Skill
    {

        public static Dictionary<Skills, SkillTarget> targets = new Dictionary<Skills, SkillTarget>()
        {
            { Skills.None,  SkillTarget.None },
            { Skills.Num,  SkillTarget.None },
            { Skills.Movement,  SkillTarget.Tile },
            { Skills.Attacking, SkillTarget.Enemy },
            { Skills.Healing,   (SkillTarget)((int)SkillTarget.Friend + (int)SkillTarget.Self) }
        };        


    }



    public enum Skills
    {
        None,
        Movement,
        Attacking,
        Healing,
        Reloading,
        Num
    }

    [Flags]
    public enum SkillTarget
    {
        None = 0,
        Self = 1,
        Friend = 2,
        Enemy = 4,
        Tile = 8
    }
}
