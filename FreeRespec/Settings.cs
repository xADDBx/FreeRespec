using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityModManagerNet;

namespace FreeRespec {
    public class Settings : UnityModManager.ModSettings {
        public Main.Mode chosenMode = Main.Mode.Free;
        public int val = 0;
        public override void Save(UnityModManager.ModEntry modEntry) {
            Save(this, modEntry);
        }
    }
}
