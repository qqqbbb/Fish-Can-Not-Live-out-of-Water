using SMLHelper.V2.Json;
using SMLHelper.V2.Options.Attributes;


namespace Fish_Out_Of_Water
{
    [Menu("Fish Can Not Live Out Of Water")]
    public class Config : ConfigFile
    {
        [Slider("Time fish lives out of water", 1, 1000, DefaultValue = 180, Step = 1, Format = "{0:F0}", Tooltip = "This is ingame, not real time seconds. 1 ingame second is 1 real time second at time scale 1")]
        public int outOfWaterLiveTime = 180;
        //[Toggle("Logging")]
        //public bool logging = false;

    }
}