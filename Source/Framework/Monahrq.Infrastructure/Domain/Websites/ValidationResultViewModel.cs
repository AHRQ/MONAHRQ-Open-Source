namespace Monahrq.Infrastructure.Domain.Websites
{
    [PropertyChanged.ImplementPropertyChanged]
    public class ValidationResultViewModel
    {
        public ValidationLevel Quality { get; set; }
        public string Message { get; set; }
        public string HelpTopic { get; set; }
        public string HelpText { get; set; }
        public WebsiteTabViewModels CompositionArea { get; set; }
    }
}
