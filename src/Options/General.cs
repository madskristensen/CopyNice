using System.ComponentModel;
using System.Runtime.InteropServices;

namespace CopyNice
{
    public class GeneralOptions : BaseOptionModel<GeneralOptions>
    {
        [DefaultValue(true)]
        public bool Enabled { get; set; } = true;
    }
}
