using System.Windows;

namespace View
{
    /// <summary>
    /// Interaction logic for DocumentPreview.xaml
    /// </summary>
    public partial class DocumentPreview : Window
    {
        public string Text { get; set; }
        public DocumentPreview(string text)
        {
            Text = text;
            InitializeComponent();
        }
    }
}
