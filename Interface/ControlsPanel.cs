using Barely.Util;
using BarelyUI;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD40.Interface
{
    public class ControlsPanel : VerticalLayout
    {

        public ControlsPanel(Dictionary<string, Sprite> sprites)
        {

            SetAnchor(AnchorX.Right, AnchorY.Top);
            getStandardSprite = true;
            SetFixedWidth(200);
            SetLayoutSize(LayoutSize.FixedSize, LayoutSize.WrapContent);
            Padding = new Point(6, 6);
            Margin = 6;
            string[] text = { "explMouseLeft", "explMouseRight", "explMouseMiddle", "explWASD", "explTab", "explEnter" };
            string[] sprite = { "mouseLeft", "mouseRight", "mouseMiddle", "WASD", "tab", "enter" };

            for (int i = 0; i < text.Length; i++)
            {
                HorizontalLayout l = new HorizontalLayout();
                l.layoutFill = LayoutFill.StretchMargin;
                Image im = new Image(sprites[sprite[i]]);
                im.SetFixedSize(32, 32);

                Text te = new Text(text[i]);
                te.SetFontSize(FontSize.Small);

                l.AddChild(im);
                l.AddChild(te);

                AddChild(l);
            }            
        }

    }
}
