using BarelyUI;
using LD40.Actors;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD40.Interface
{
    public class EnemyInfoPanel : VerticalLayout
    {
        Func<Enemy> GetEnemy;
        Text name;

        KeyValueText damage;
        KeyValueText health;
        KeyValueText ammo;
        KeyValueText moveRange;
        KeyValueText skillRange;

        public EnemyInfoPanel(Func<Enemy> GetEnemy)
        {
            this.GetEnemy = GetEnemy;

            Padding = new Point(10, 4);
            this.SetAnchor(AnchorX.Right, AnchorY.Bottom);
            this.SetLayoutSize(LayoutSize.FixedSize, LayoutSize.WrapContent).SetFixedWidth(250);
            getStandardSprite = true;

            name = new Text("wuhuhuudedasda");
            name.SetColor(Color.LightSlateGray);
            name.SetFontSize(FontSize.Medium);


            damage = new KeyValueText("damage", "1");
            health = new KeyValueText("health", "1");
            ammo = new KeyValueText("ammo", "1");
            moveRange = new KeyValueText("moveRange", "1");
            skillRange = new KeyValueText("skillRange", "1");
            
            AddChild(name, health, ammo, damage, moveRange, skillRange);
            Close();
        }

        public override void Update(float deltaTime)
        {

            Enemy e = GetEnemy();
            
            if(e == null)
            {
                if (isOpen)
                    Close();
            } else
            {

                if (!isOpen)
                    Open();
                name.SetText(e.name);
                health.SetValue($"{e.HP}/{e.maxHP}");
                ammo.SetValue($"{e.currAmmo}/{e.maxAmmo}");
                damage.SetValue($"{e.GetCurrentDamage()}");
                moveRange.SetValue($"{e.GetMovementRange()}");
                skillRange.SetValue($"{e.GetSkillRange()}");
            }


            base.Update(deltaTime);
        }

    }
}
