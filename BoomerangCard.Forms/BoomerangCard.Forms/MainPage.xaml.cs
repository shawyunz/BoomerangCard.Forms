using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace BoomerangCard.Forms
{
    public partial class MainPage : ContentPage
    {
        private bool isflying = false;
        private bool isTravelUp;
        private View topview;
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
            await FlyBoomerang(AniSpins, AniHeight);

            isflying = false;
        }

        private async Task FlyBoomerang(int spins, double height)
        {
            isflying = true;
            var viewList = GetCardList();

            var view1 = viewList[0];
            var view2 = viewList[1];

            view1.RotateTo(spins * 360, 800);
            view1.ScaleTo(0.95, 800);

            view2.ScaleTo(1, 200);
            view2.TranslateTo(0, 10, 200);

            await view1.TranslateTo(0, -height, 400, Easing.CubicOut);
            gridbox.LowerChild(view1);
            view1.Margin = new Thickness(0, 0, 0, 20);
            await view1.TranslateTo(0, 0, 400, Easing.CubicIn);

            view1.Rotation = 0;
            view1.Scale = 0.95;
            view2.Scale = 1;

            //await Task.Delay(500);
            //isflying = false;
        }

        private List<View> GetCardList()
        {
            var cardList = new List<View>();
            if (card1.Scale == 1)
            {
                cardList.Add(card1);
                cardList.Add(card2);
            }
            else
            {
                cardList.Add(card2);
                cardList.Add(card1);
            }

            return cardList;
        }

        private double GetFlyCapacity(double velocity)
        {
            // v < 0.5 means least spin, which is 1
            // v > 1.5 means 100%
            return (velocity < 0.5 ? 0 : (velocity > 1.5 ? 0.9 : ((velocity - 0.5) * 0.9))) + 0.1;
        }

        private async void PanGestureRecognizer_PanUpdated(object sender, PanUpdatedEventArgs e)
        {
            if (isflying)
            {
                return;
            }

            if (topview == null)
            {
                topview = GetCardList()[0];
            }

            var offset = e.TotalY;
            LabelStatus.Text = "Status: " + e.StatusType.ToString();
            LabelY.Text = "Movement: " + Math.Round(e.TotalY, 1);

            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    if (topview.Rotation != -10)
                    {
                        topview.Rotation = -10;
                    }
                    break;

                case GestureStatus.Running:
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

                    topview.TranslationX += e.TotalX;
                    topview.TranslationY += offset;
                    break;

                case GestureStatus.Completed:
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
                    break;
            }
        }

        private void Spinslider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            ((Slider)sender).Value = (int)e.NewValue;
        }
    }
}