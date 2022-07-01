using BoomerangCard.Forms.Effect;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace BoomerangCard.Forms
{
    public partial class MainPage : ContentPage
    {
        private bool isBeingDragged;
        private bool isflying = false;
        private bool isRightSide;
        private bool isTravelUp;
        private Point pressPoint;
        private View topview;
        private long touchId;
        private double travelDistance;
        private DateTime travelStart;

        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        public double AniHeight { get; set; } = 240;

        public int AniSpins { get; set; } = 1;

        private async void Button_Clicked(object sender, EventArgs e)
        {
            if (isflying)
            {
                return;
            }

            LabelStatus.Text = "Status: Flying";
            LabelY.Text = "Movement: ";

            await FlyBoomerang(AniSpins, AniHeight);

            LabelStatus.Text = "Status:";
            isflying = false;
        }

        private async Task FlyBoomerang(int spins, double height)
        {
            isflying = true;

            var cardList = gridbox.Children.Cast<CardView>().ToList();
            var topCardIndex = cardList.Count - 1;

            //fly starts
            var viewFly = cardList[topCardIndex];
            viewFly.RotateTo(spins * (isRightSide ? -360 : 360), 800);
            viewFly.ScaleTo(1 - 0.05 * topCardIndex, 800);

            for (int i = 0; i < topCardIndex; i++)
            {
                cardList[i].ScaleTo(cardList[i].Scale + 0.05, 200);
                cardList[i].TranslateTo(0, 10, 200);
            }

            await viewFly.TranslateTo(0, -height, 400, Easing.CubicOut);
            gridbox.LowerChild(viewFly);
            viewFly.Margin = new Thickness(0, 0, 0, 20 * topCardIndex);
            await viewFly.TranslateTo(0, 0, 400, Easing.CubicIn);

            //refresh positions when fly ends
            for (int i = 0; i < topCardIndex; i++)
            {
                cardList[i].TranslationY = 0;
                cardList[i].Margin = new Thickness(0, 0, 0, cardList[i].Margin.Bottom - 20);
            }
        }

        private double GetFlyCapacity(double velocity)
        {
            // v < 0.5 means least spin, which is 1
            // v > 1.5 means 100%
            return (velocity < 0.5 ? 0 : (velocity > 1.5 ? 0.9 : ((velocity - 0.5) * 0.9))) + 0.1;
        }

        private async void OnTouchEffectAction(object sender, TouchActionEventArgs args)
        {
            LabelStatus.Text = "Status: " + (isflying ? "Flying" : args.Type.ToString());
            if (isflying)
            {
                return;
            }

            switch (args.Type)
            {
                case TouchActionType.Pressed:
                    if (!isBeingDragged)
                    {
                        if (topview == null)
                        {
                            topview = sender as View;
                        }

                        isBeingDragged = true;
                        touchId = args.Id;
                        pressPoint = args.Location;

                        //Console.WriteLine("=================> coordination x -- " + pressPoint.X);
                        //Console.WriteLine("=================> coordination y -- " + pressPoint.Y);

                        isRightSide = (args.Location.X >= Application.Current.MainPage.Width / 2) ? true : false;

                        var initAngle = isRightSide ? -10 : 10;
                        if (topview.Rotation != initAngle)
                        {
                            topview.Rotation = initAngle;
                        }
                    }
                    break;

                case TouchActionType.Moved:
                    if (isBeingDragged && touchId == args.Id)
                    {
                        var offset = args.Location.Y - pressPoint.Y;
                        LabelY.Text = "Movement: " + Math.Round(offset, 1);

                        if (offset > 0)
                        {
                            travelDistance = 0;
                            isTravelUp = false;
                        }
                        else
                        {
                            if (!isTravelUp)
                            {
                                travelStart = DateTime.Now;
                                isTravelUp = true;
                            }
                            travelDistance -= offset;
                        }

                        topview.TranslationX += args.Location.X - pressPoint.X;
                        topview.TranslationY += args.Location.Y - pressPoint.Y;
                    }
                    break;

                case TouchActionType.Released:
                    if (isBeingDragged && touchId == args.Id)
                    {
                        if (isTravelUp && topview.TranslationY < 0)
                        {
                            //have to throw higher than original position to fly
                            var portion = GetFlyCapacity(travelDistance / (int)DateTime.Now.Subtract(travelStart).TotalMilliseconds);
                            await FlyBoomerang((int)(10 * portion), travelDistance + 300 * portion + 100);
                        }
                        else if (topview.TranslationY > 100)
                        {
                            await FlyBoomerang(10, 450);
                        }
                        else
                        {
                            topview.TranslationX = 0;
                            topview.TranslationY = 10;
                        }

                        topview.Rotation = 0;
                        topview = null;

                        travelDistance = 0;
                        isTravelUp = false;
                        isflying = false;
                        isBeingDragged = false;
                    }
                    break;
            }
        }

        private void Spinslider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            ((Slider)sender).Value = (int)e.NewValue;
        }
    }
}