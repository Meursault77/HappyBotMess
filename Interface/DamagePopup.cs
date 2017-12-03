using Barely.Util;
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
    public class DamagePopup : VerticalLayout
    {

        float timer = 0f;
        const float showTime = 2f;
        Text damageText;
        Camera camera;
        Actor showOn;
        Vector2 offset;

        public DamagePopup(Camera camera)
        {
            this.camera = camera;
            Position = new Point(10, 10);
            SetLayoutSize(LayoutSize.WrapContent, LayoutSize.WrapContent);
            damageText = new Text("miss").SetFontSize(FontSize.Large);            
            AddChild(damageText);
            Close();
        }

        public override void Update(float deltaTime)
        {

            if (isOpen && showOn != null)
            {
                timer += deltaTime;
                offset.Y -= 100*deltaTime;
                Position = camera.ToScreen(showOn.realPosition) + offset.ToPoint();
                if(timer >= showTime)
                {
                    Close();
                }
            }

            base.Update(deltaTime);
        }

        public void Show(Actor showOn, string txt)
        {
            Color color = Color.Red;
            //string text = damageValue > 0 ? $"-{damageValue}" : $"+{-1 * damageValue}";
            damageText.SetText(txt);
            damageText.SetColor(color);
            offset = new Vector2(0, 8);
            this.showOn = showOn;
            Open();
            timer = 0f;
        }

        public void Show(Actor showOn, int damageValue)
        {
            Color color = damageValue > 0 ? Color.Red : Color.ForestGreen;
            string text = damageValue > 0 ? $"-{damageValue}" : $"+{-1*damageValue}";
            damageText.SetText(text);
            damageText.SetColor(color);
            offset = new Vector2(0, 8);
            this.showOn = showOn;
            Open();
            timer = 0f;
        }

    }
}
