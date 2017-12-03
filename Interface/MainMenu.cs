using Barely.Util;
using BarelyUI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD40.Interface
{
    public class MainMenu : VerticalLayout
    {

        public MainMenu(ContentManager Content, Action OnStartGame, Action OnQuit)
        {
            Texture2D logoTex = Content.Load<Texture2D>("logo_ingame");

            SetAnchor(AnchorX.Middle, AnchorY.Middle);
            SetFixedSize(400, 500);
            overwriteChildLayout = false;
            Padding = new Point(10, 10);
            Margin = 10;

            int w = 315, h = 250;
            Rectangle[] rects = {   new Rectangle(0*w, 0, w, h),
                                    new Rectangle(1*w, 0, w, h),
                                    new Rectangle(2*w, 0, w, h),
                                    new Rectangle(3*w, 0, w, h),
                                    new Rectangle(2*w, 0, w, h),
                                    new Rectangle(1*w, 0, w, h),
                    };
            double[] length = { 167, 167, 167, 167, 167, 167};
            Animation animation = new Animation(6, length, rects);
            Sprite logoSprite = new Sprite(logoTex, rects[0], Color.White, animation);

            Image logo = new Image(logoSprite);


            Button bStart = new Button("startGame");
            bStart.SetFixedWidth(200);
            bStart.OnMouseClick = OnStartGame;

            Button bQuit = new Button("quit");
            bQuit.SetFixedWidth(200);
            bQuit.OnMouseClick = OnQuit;

            Text ld = new Text("A game for Ludum Dare 40");

            AddChild(logo, ld, bStart, bQuit);

        }

    }
}
