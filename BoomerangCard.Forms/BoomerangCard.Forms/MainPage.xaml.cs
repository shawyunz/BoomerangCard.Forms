using BoomerangCard.Forms.Effect;
using System;
using System.Collections.Generic;
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
            LabelStatus.Text = "Status: Flying";
            LabelY.Text = "Movement: ";

            await FlyBoomerang(AniSpins, AniHeight);

            LabelStatus.Text = "Status:";
            isflying = false;
        }

        private async Task FlyBoomerang(int spins, double height)
        {
            isflying = true;
            var viewList = GetCardList();

            var view1 = viewList[0];
            var view2 = viewList[1];

            view1.RotateTo(spins * (isRightSide ? -360 : 360), 800);
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