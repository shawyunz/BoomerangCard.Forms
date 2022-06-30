using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BoomerangCard.Forms
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CardView : ContentView
    {
        public static readonly BindableProperty CardColorProperty =
            BindableProperty.Create(nameof(CardColor), typeof(Color), typeof(CardView), Color.White, BindingMode.TwoWay);

        public CardView()
        {
            InitializeComponent();
            BindingContext = this;
        }

        public Color CardColor
        {
            get => (Color)GetValue(CardColorProperty);
            set => SetValue(CardColorProperty, value);
        }
    }
}