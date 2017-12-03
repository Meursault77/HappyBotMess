using Barely.Util;
using LD40.Actors;
using LD40.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace LD40.Actors
{
    public class Actor
    {

        public Skills selectedSkill = Skills.Movement;
        public String name;
        public int maxHP = 7;
        public int HP;

        public int maxAmmo = 4;
        public int currAmmo;

        public int actionsPerTurn = 1;
        public int actionsLeft;

        public int movesPerTurn = 1;
        public int movesLeft;

        public Faction faction = Faction.User;

        public Point tilePosition;
        public Point realPosition;
        public int X { get { return realPosition.X; } set { realPosition.X = value; } }
        public int Y { get { return realPosition.Y; } set { realPosition.Y = value; } }
        public int yOffset = 0;


        private Sprite sprite;

        private Point offset = new Point(0, 4);

        public HashSet<Point> reachableTiles;

        public HashSet<Point> skillReachable;

        private Action<Actor> DeathCallback;
        protected Func<Point, int, bool, HashSet<Point>> CalcReach;

        public Actor(Sprite sprite, Point pos, Action<Actor> DeathCallback, Func<Point, int, bool, HashSet<Point>> CalcReach)
        {
            HP = maxHP;
            this.CalcReach = CalcReach;
            this.DeathCallback = DeathCallback;
            currAmmo = maxAmmo;
            movesLeft = movesPerTurn;
            actionsLeft = actionsPerTurn;

            this.sprite = sprite.CopyInstance();
            tilePosition = pos;
            realPosition = pos * Map.tileSize;
        }

        public void Render(SpriteBatch spriteBatch)
        {
            sprite.Render(spriteBatch, new Rectangle(realPosition - offset - new Point(0, yOffset), Map.tileSize));          
        }


        #region Game Logic Stuff
        

        public virtual int GetSkillRange()
        {
            if (actionsLeft == 0 || currAmmo == 0)
                return 0;
            else
                return maxAmmo - currAmmo + 1;
                
        }        

        public virtual int GetMovementRange()
        {            
            if (movesLeft == 0)
                return 0;
            if (currAmmo == 0)
                return 0;            
            else
                return maxAmmo - currAmmo + 1;
        }

        public virtual int GetCurrentDamage()
        {
            if (currAmmo == 0)
                return 0;
            else
                return maxAmmo - currAmmo + 1;
        }

        public virtual int GetCurrentHealing()
        {
            if (currAmmo == 0)
                return 0;
            else
                return maxAmmo - currAmmo + 1;
        }

        public virtual int CurrentHealth()
        {
            return HP;
        }

        public void MoveFinished()
        {
            Debug.Assert(movesLeft > 0);
            movesLeft--;
            currAmmo--;
            SelectSkill(Skills.None);
        }

        public void UsedSkill()
        {
            Debug.Assert(actionsLeft > 0);
            actionsLeft--;
            if (actionsLeft == 0)
                movesLeft = 0;
            SelectSkill(Skills.None);
        }

        public bool IsReachable(Point pos)
        {
            return reachableTiles != null && reachableTiles.Contains(pos);
        }

        public void EndTurn()
        {
            movesLeft = movesPerTurn;
            actionsLeft = actionsPerTurn;
        }

        /// <summary>
        /// Sets the units skill
        /// </summary>
        /// <param name="s">if unit could select the skill</param>
        /// <returns></returns>
        public bool SelectSkill(Skills s)
        {
            selectedSkill = Skills.None;
            if (s == Skills.Movement && movesLeft == 0 || actionsLeft == 0 || s == Skills.Attacking && currAmmo == 0)
            {
                Sounds.Play("couldNotSelectSkill");
                return false;            
            }

            if(s == Skills.Reloading)
            {
                Sounds.Play("reloading");
                currAmmo = maxAmmo;
                UsedSkill();                
                return true;
            }

            selectedSkill = s;
            if (s == Skills.Attacking || s == Skills.Healing)
                skillReachable = CalcReach(tilePosition, GetSkillRange(), true);
            else if(s == Skills.Movement)
            {
                reachableTiles = CalcReach(tilePosition, GetMovementRange(), false);
            }            
            return true;
        }

        public void TakeDamage(int damage)
        {
            
            HP -= damage;
            Map.damagePopup.Show(this, damage);
            if (damage > 0)
                Effects.ShowEffect(faction == Faction.Enemy ? "shotImpact" : "laserImpact", realPosition);

            if (HP <= 0)
                Die();
            else if (damage > 0)                            
                Sounds.Play("takingDamage");                                   
            else if (HP > maxHP)
                HP = maxHP;
        }

        public virtual void Die()
        {
            Sounds.Play("death");
            DeathCallback(this);
        }

        public HashSet<Point> GetReachableTiles()
        {
            if (selectedSkill == Skills.Movement)
                return reachableTiles;
            else
                return null;
        }

        public HashSet<Point> GetSkillReachableTiles()
        {
            if (selectedSkill == Skills.Attacking || selectedSkill == Skills.Healing)
                return skillReachable;
            else
                return null;
        }

        public SkillTarget GetCurrentSkillTarget()
        {
            return Skill.targets[selectedSkill];
        }

        #endregion

        public String GetInfoString()
        {
            return  $"Moves Left: {movesLeft}/{movesPerTurn}\n" +
                    $"Actions Left: {actionsLeft}/{actionsPerTurn}\n" +
                    $"Ammo: {currAmmo}/{maxAmmo}\n" +
                    $"Movement Range: {GetMovementRange()}\n" +
                    $"Damage: {GetCurrentDamage()}";
        }

    }


    public enum Faction
    {
        User,
        Enemy
    }

}
