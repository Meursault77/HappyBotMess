using BarelyUI;
using LD40.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD40.Interface
{
    public class EnemyTurnSign : VerticalLayout
    {

        Func<Map> GetMap;

        public EnemyTurnSign(Func<Map> GetMap)
        {
            this.GetMap = GetMap;
            Text text = new Text("enemyTurn");
            text.SetLayoutSizeForBoth(LayoutSize.WrapContent);
            GetStandardSprite(true);
            SetLayoutSizeForBoth(LayoutSize.WrapContent);
            Padding = new Microsoft.Xna.Framework.Point(10, 10);
            SetAnchor(AnchorX.Middle, AnchorY.Top);
            AddChild(text);
        }

        public override void Update(float deltaTime)
        {
            Map m = GetMap();

            if (isOpen)
            {
                if (m == null || m.state != Map.MapState.EnemyTurns)
                    Close();
            }
            else
            {
                if (m != null && m.state == Map.MapState.EnemyTurns)
                    Open();
            }


            base.Update(deltaTime);
        }

    }
}
