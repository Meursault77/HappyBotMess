using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BarelyUI;
using Microsoft.Xna.Framework;
using Barely.Util;
using LD40.Actors;

namespace LD40.Interface
{
    class UnitPanel : VerticalLayout
    {

        Func<Actor> GetActor;

        KeyValueText movesLeft;
        KeyValueText actionsLeft;
        KeyValueText ammo;
        KeyValueText damage;
        KeyValueText health;
        KeyValueText moveRange;
        KeyValueText skillRange;
        Text buttonTooltip;
        Button selectedButton = null;
        Button[] buttons;

        Dictionary<string, Sprite> uiSprites;

        static Color unusableColor  = Color.DarkRed;
        static Color warningColor   = Color.Yellow;
        static Color normalColor    = Color.White;

        public UnitPanel(Func<Actor> GetActor, Dictionary<string, Sprite> uiSprites)
        {
            this.uiSprites = uiSprites;
            Padding = new Point(10, 4);
            this.SetAnchor(AnchorX.Left, AnchorY.Bottom);
            this.SetLayoutSize(LayoutSize.FixedSize, LayoutSize.WrapContent).SetFixedWidth(250);
            getStandardSprite = true;
            this.GetActor = GetActor;
            movesLeft = new KeyValueText("movesLeft", "1/1");
            actionsLeft = new KeyValueText("actionsLeft", "1/1");
            ammo = new KeyValueText("ammo", "3/4");
            damage = new KeyValueText("damage", "1");
            health = new KeyValueText("health", "1");
            moveRange = new KeyValueText("moveRange", "1");
            skillRange = new KeyValueText("skillRange", "1");

            buttonTooltip = new Text("MOvement");
            buttonTooltip.SetFontSize(FontSize.Small);
            buttonTooltip.SetColor(Color.LightSlateGray);

            HorizontalLayout buttonLayout = new HorizontalLayout();
            buttons = new Button[(int)Skills.Num];
            buttonLayout.Padding = new Point(6, 6);
            buttonLayout.SetMargin(10).SetLayoutSize(LayoutSize.MatchParent, LayoutSize.FixedSize);
            buttonLayout.SetFixedHeight(44);
            for (int i = 1; i < (int)Skills.Num; i++)
            {
                Skills s = (Skills)i;
                string sprite = s.ToString();
                string otherSprite = s.ToString() + "Selected";
                Button b = new Button(uiSprites[sprite]);
                b.OnMouseEnter = () => buttonTooltip.SetText(s.ToString());
                b.OnMouseExit = () => buttonTooltip.SetText("");
                b.OnMouseClick = () => SelectSkill(s);
                buttons[i] = b;    
                buttonLayout.AddChild(b);
            }
            AddChild(actionsLeft, movesLeft, health, ammo, damage, moveRange, skillRange, buttonTooltip, buttonLayout);
        }

        public void SelectSkill(Skills s)
        {
            Actor a = GetActor();
            Skills old = a.selectedSkill;
            if(selectedButton != null && old != Skills.None)
            {
                selectedButton.sprite = uiSprites[old.ToString()];
            }
            if (a.SelectSkill(s))
            {
                selectedButton = buttons[(int)s];
                selectedButton.sprite = uiSprites[s.ToString() + "Selected"];
            }
        }

        public override void Update(float deltaTime)
        {
            Actor a = GetActor();

            if (a == null)
            {
                buttonTooltip.SetText("");
                Close();
            }
            else
            {
                if (!isOpen)
                    Open();

                // Update the ui thingies
                movesLeft.SetValue($"{a.movesLeft}/{a.movesPerTurn}");
                movesLeft.SetValueColor(a.movesLeft == 0 ? unusableColor : normalColor);

                actionsLeft.SetValue($"{a.actionsLeft}/{a.actionsPerTurn}");
                actionsLeft.SetValueColor(a.actionsLeft == 0 ? unusableColor : normalColor);

                ammo.SetValue($"{a.currAmmo}/{a.maxAmmo}");
                ammo.SetValueColor(a.currAmmo == 0 ? unusableColor : (a.currAmmo < a.maxAmmo - 1 ? warningColor : normalColor));

                damage.SetValue($"{a.GetCurrentDamage()}");                

                health.SetValue($"{a.CurrentHealth()}/{a.maxHP}");
                health.SetValueColor(a.CurrentHealth() < a.maxHP - 2 ? warningColor : normalColor);

                moveRange.SetValue($"{a.GetMovementRange()}");
                skillRange.SetValue($"{a.GetSkillRange()}");

                Skills s = a.selectedSkill;                

                if(buttons[(int)s] != selectedButton)
                {
                    if(selectedButton != null)
                    {
                        for(int i = 1; i < (int)Skills.Num; i++)
                        {
                            if(buttons[i] == selectedButton)
                            {
                                Skills old = (Skills)i;
                                selectedButton.sprite = uiSprites[old.ToString()];
                                break;
                            }
                        }
                    }
                    if(s != Skills.None)
                    {
                        buttons[(int)s].sprite = uiSprites[s.ToString() + "Selected"];
                        selectedButton = buttons[(int)s];
                    }
                }
            }


            base.Update(deltaTime);
        }

    }
}
