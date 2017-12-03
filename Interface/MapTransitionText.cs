using Barely.Util;
using BarelyUI;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD40.Interface
{
    public class MapTransitionText : VerticalLayout
    {
        Text textField;
        Button buttonContinue;
        Button buttonMenu;

        Dictionary<string, SoundEffect> sounds;
        Action OnContinue;
        Action OnMenu;
        Action OnQuitDesktop;

        SoundEffectInstance playingSound;

        public MapTransitionText(Action OnContinue, Action OnMenu, Action OnQuitDesktop, Dictionary<string, SoundEffect> sounds)
        {
            this.sounds = sounds;
            this.OnContinue = OnContinue;
            this.OnMenu = OnMenu;
            GetStandardSprite(true);
            SetAnchor(AnchorX.Middle, AnchorY.Middle);
            childAllignX = AnchorX.Middle;
            overwriteChildLayout = false;

            SetFixedSize(500, 500);
            Padding = new Microsoft.Xna.Framework.Point(10, 10);
            Margin = 10;
            textField = new Text(Texts.Get("level0Intro"));
            textField.wrapText = true;
            textField.SetFixedSize(400, 300);


            buttonMenu = new Button("backMenu");
            buttonMenu.SetFixedWidth(200);
            buttonMenu.OnMouseClick = OnMenu + StopSound;

            Button buttonDesktop = new Button("quit");
            buttonDesktop.SetFixedWidth(200);
            buttonDesktop.OnMouseClick = OnQuitDesktop + StopSound;

            buttonContinue = new Button("continue");
            buttonContinue.SetFixedWidth(200);
            buttonContinue.OnMouseClick = OnContinue + StopSound;

            AddChild(textField, buttonContinue, buttonMenu, buttonDesktop);
        }

        private void StopSound()
        {
            if (playingSound != null && playingSound.State == SoundState.Playing)
                playingSound.Stop();
        }

        public void ShowText(string text, Action overwriteContinue = null)
        {
            textField.SetText(text);

            if (overwriteContinue != null)
                buttonContinue.OnMouseClick = overwriteContinue + StopSound;
            else
                buttonContinue.OnMouseClick = OnContinue + StopSound;

            if (sounds != null && sounds.ContainsKey(text))
            {
                playingSound = sounds[text].CreateInstance();
                playingSound.Play();                
            }
        }

    }
}
