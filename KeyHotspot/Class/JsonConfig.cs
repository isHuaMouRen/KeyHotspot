using System.Windows.Forms;

namespace KeyHotspot.Class
{
    public class JsonConfig
    {
        public class Data
        {
            public class Root
            {
                public JsonConfig.Data.KeyInfo[] data { get; set; }
            }

            public class KeyInfo
            {
                public Keys key { get; set; }
                public long press_count { get; set; }
            }
        }
    }
}
