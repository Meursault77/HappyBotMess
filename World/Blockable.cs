using Barely.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD40.World
{
    public class Blockable
    {
        public Sprite sprite;

        public bool interactable;

        public Blockable(Sprite sprite, bool interactable)
        {
            this.sprite = sprite;
            this.interactable = interactable;
        }

    }
}
