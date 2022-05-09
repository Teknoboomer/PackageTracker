namespace TrackerVM
{
    public class TrackingDialogParameters
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string ActionLabel { get; set; }
        public string CancelLabel { get; set; }
        public bool Confirmed { get; set; }
        public object ActionParams { get; set; }
    }
}
