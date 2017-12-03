using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD40
{
    public class Config
    {
        public static bool musicPlaying { get { return isMusicPlaying; } set { isMusicPlaying = value; MediaPlayer.IsMuted = !value; } }
        private static bool isMusicPlaying = true;
        public static Point Resolution;
    }
}
