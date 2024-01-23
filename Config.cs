using Nautilus.Json;
using Nautilus.Options.Attributes;
using System.Collections.Generic;
using UnityEngine;
using Nautilus.Commands;
using Nautilus.Handlers;
using Nautilus.Options;
using System.ComponentModel;


namespace Fish_Out_Of_Water
{
    [Menu("Fish Can Not Live Out Of Water")]
    public class Config : ConfigFile
    {
        [Slider("Number of minutes fish live out of water", Min = 1, Max = 50, DefaultValue = 10, Step = 1, Format = "{0:F0}", Tooltip = "Real time minutes at default time scale")]
        public int outOfWaterLifeTime = 10;

    }
}