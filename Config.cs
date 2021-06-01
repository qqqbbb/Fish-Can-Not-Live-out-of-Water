using SMLHelper.V2.Json;
using SMLHelper.V2.Options.Attributes;


namespace Fish_Out_Of_Water
{
    [Menu("Fish Can Not Live Out Of Water")]
    public class Config : ConfigFile
    {
        [Slider("Minutes fish live out of water", Min = 1, Max = 50, DefaultValue = 10, Step = 1, Format = "{0:F0}", Tooltip = "This is ingame, not real time minutes. 1 ingame minute is 1 real time minute at time scale 1")]
        public int outOfWaterLifeTime = 10;
        //[Toggle("Logging")]
        //public bool logging = false;
    }
}