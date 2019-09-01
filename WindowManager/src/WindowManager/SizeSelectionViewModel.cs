using System.Collections.Generic;

namespace WindowManager
{
    public sealed class SizeSelectionViewModel
    {
        public int NbColumns { get; set; }
        public int NbRows { get; set; }

        public IList<ScreenItem> ScreenItems { get; set; }
    }
}