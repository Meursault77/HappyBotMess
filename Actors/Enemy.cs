using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Barely.Util;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using LD40.World;

namespace LD40.Actors
{
    public enum EnemyType
    {
        EasyRaute,
        WalkingRaute,
        NumberThreeBla
    }

    public class Enemy : Actor
    {
        public static Enemy CreateEnemy(EnemyType type, Sprite[] enemySprites, Point pos, Action<Enemy> DeathCallback, Func<Point, int, bool, HashSet<Point>> CalcReach)
        {
            if (type == EnemyType.EasyRaute)
                return new Enemy(enemySprites[(int)type], pos, DeathCallback, CalcReach, 3, 2, 2, 4, 2, "Laser Rhombus");
            else if(type == EnemyType.WalkingRaute)
                return new Enemy(enemySprites[(int)type], pos, DeathCallback, CalcReach, 4, 3, 3, 3, 2, "Egg Shaped Blorgon");
            else if (type == EnemyType.NumberThreeBla)
                return new Enemy(enemySprites[(int)type], pos, DeathCallback, CalcReach, 5, 1, 4, 3, 2, "Why are human?");
            else
                return null;
        }

        public Action<Enemy> DeathCallback;

        public int damage;
        public int movementRange;
        public int skillRange;

        private Enemy(Sprite sprite, Point pos, Action<Enemy> DeathCallback, Func<Point, int, bool, HashSet<Point>> CalcReach, int maxHP, int maxAmmo, int damage, int movementRange, int skillRange, string name)
              : base(sprite, pos, null, CalcReach)
        {
            this.DeathCallback = DeathCallback;
            faction = Faction.Enemy;
            this.maxHP = maxHP;
            HP = maxHP;
            this.maxAmmo = maxAmmo;
            currAmmo = this.maxAmmo;
            this.damage = damage;
            this.movementRange = movementRange;
            this.skillRange = skillRange;
            this.name = name;
        }

        public override int GetCurrentDamage()
        {
            return damage;
        }

        public override int GetMovementRange()
        {
            return movementRange;
        }

        public override int GetSkillRange()
        {
            return skillRange;
        }

        public override void Die()
        {
            DeathCallback(this);
        }

        public Point MakeTurn(Map map, out Action doit)
        {
            Tile[,] tiles = map.tiles;
            Debug.Assert(actionsLeft > 0);
            reachableTiles = CalcReach(tilePosition, movementRange, false);
            skillReachable = CalcReach(tilePosition, skillRange + movementRange, true);
            if(currAmmo == 0)
            {
                //find tile with cover move to there
                doit = () => {
                    currAmmo = maxAmmo;
                    UsedSkill();
                    Debug.WriteLine(name + " is Reloading");
                    Map.damagePopup.Show(this, "Reloading");
                };                
                return tilePosition;
            }
            else
            {
                //Find attack or healing possibility, can not move
                List<Tuple<Actor, int>> targetPossibilities = new List<Tuple<Actor, int>>(12);
                
                foreach(Point p in skillReachable)
                {
                    Actor a = tiles[p.X, p.Y].actorOnTop;
                    if (a != null)
                    {
                        if(a.faction == Faction.Enemy)
                        {
                            if (a.HP == a.maxHP)
                                continue;
                        }
                        
                        int score = int.MaxValue;

                        if(a.faction == Faction.Enemy)
                        {
                            score = (a.maxHP - a.HP) - GetCurrentHealing()/2;
                            if (score <= 0)
                                score = 0;
                        } else
                        {
                            score = a.HP - GetCurrentDamage();
                            if (score < 0)
                                score = 0;
                        }
                        targetPossibilities.Add(new Tuple<Actor, int>(a, score));
                        
                    }
                }

                if(targetPossibilities.Count == 0)
                {
                    doit = () => { currAmmo = maxAmmo; UsedSkill(); Debug.WriteLine(name + " is Reloading and maybe random walking"); };
                    // just reload, maybe still move somewhere?
                    foreach(Point p in map.IterateNeighboursFourDir(tilePosition))
                    {
                        if (tiles[p.X, p.Y].IsWalkable() && map.random.NextDouble() <= 0.5)
                            return p;
                    }
                    return tilePosition;
                }
                else
                {
                    targetPossibilities.Sort((a, b) => a.Item2 - b.Item2);  //is this right? the smaller number shall be sorted first. if a.item2 is smaller than b, than its negative number, tehrefore smaller?
                    Actor target = targetPossibilities[0].Item1;

                    if(target.faction == Faction.User)
                    {
                        doit = () => { map.Attack(this, target); };
                    }
                    else
                    {
                        doit = () => {
                            target.TakeDamage(-GetCurrentHealing());
                            Sounds.Play("healing");
                            Effects.ShowEffect("healing", target.realPosition);                            
                            UsedSkill();
                        };
                    }

                    skillReachable = CalcReach(tilePosition, skillRange, true);
                    if(skillReachable.Contains(target.tilePosition))
                        return tilePosition;
                    else
                    {
                        Point pos = tilePosition;

                        foreach(Point p in reachableTiles)
                        {
                            if(p != tilePosition && tiles[p.X,p.Y].IsWalkable() &&(p - target.tilePosition).ToVector2().Length() <= skillRange)
                            {
                                pos = p;
                                skillReachable = CalcReach(pos, skillRange, true);
                                break;
                            }
                        }

                        if(pos == tilePosition)
                        {
                            doit = () => { UsedSkill(); Map.damagePopup.Show(this, "Bug"); };
                        }

                        return pos; //here find a tile from where can attack
                    }
                }
                
            }
        }

    }
}
