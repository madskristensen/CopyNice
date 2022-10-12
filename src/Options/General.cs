using System.ComponentModel;

namespace CopyNice
{
    public class GeneralOptions : BaseOptionModel<GeneralOptions>, IRatingConfig
    {
        [DefaultValue(true)]
        public bool Enabled { get; set; } = true;

        public int RatingRequests { get; set; }
    }
}
