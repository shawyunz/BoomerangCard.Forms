﻿using BoomerangCard.Forms.Effect;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace BoomerangCard.Forms
{
    public partial class MainPage : ContentPage
    {
        private bool _isBeingDragged;
        private bool _isflying = false;
        private bool _isRightSide;
        private bool _isTravelUp;
        private Point _pressPoint;
        private View _topview;
        private long _touchId;
        private double _travelDistance;
        private DateTime _travelStart;

        public double AniHeight { get; set; } = 240;
        public int AniSpins { get; set; } = 1;

        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            if (_isflying)
            {
                return;
            }

            LabelStatus.Text = "Status: Flying";
            LabelY.Text = "Movement: ";

            await FlyBoomerang(AniSpins, AniHeight);

            LabelStatus.Text = "Status:";
            _isflying = false;
        }

        private async Task FlyBoomerang(int spins, double height)
        {
            _isflying = true;

            var cardList = gridbox.Children.Cast<CardView>().ToList();
            var topCardIndex = cardList.Count - 1;

            //fly starts
            var viewFly = cardList[topCardIndex];
            viewFly.RotateTo(spins * (_isRightSide ? -360 : 360), 800);
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
            LabelStatus.Text = "Status: " + (_isflying ? "Flying" : args.Type.ToString());
            if (_isflying)
            {
                return;
            }

            switch (args.Type)
            {
                case TouchActionType.Pressed:
                    if (!_isBeingDragged)
                    {
                        if (_topview == null)
                        {
                            _topview = sender as View;
                        }

                        _isBeingDragged = true;
                        _touchId = args.Id;
                        _pressPoint = args.Location;

                        //Console.WriteLine("=================> coordination x -- " + pressPoint.X);
                        //Console.WriteLine("=================> coordination y -- " + pressPoint.Y);

                        _isRightSide = (args.Location.X >= Application.Current.MainPage.Width / 2) ? true : false;

                        var initAngle = _isRightSide ? -10 : 10;
                        if (_topview.Rotation != initAngle)
                        {
                            _topview.Rotation = initAngle;
                        }
                    }
                    break;

                case TouchActionType.Moved:
                    if (_isBeingDragged && _touchId == args.Id)
                    {
                        var offset = args.Location.Y - _pressPoint.Y;
                        LabelY.Text = "Movement: " + Math.Round(offset, 1);

                        if (offset > 0)
                        {
                            _travelDistance = 0;
                            _isTravelUp = false;
                        }
                        else
                        {
                            if (!_isTravelUp)
                            {
                                _travelStart = DateTime.Now;
                                _isTravelUp = true;
                            }
                            _travelDistance -= offset;
                        }

                        _topview.TranslationX += args.Location.X - _pressPoint.X;
                        _topview.TranslationY += args.Location.Y - _pressPoint.Y;
                    }
                    break;

                case TouchActionType.Released:
                    if (_isBeingDragged && _touchId == args.Id)
                    {
                        if (_isTravelUp && _topview.TranslationY < 0)
                        {
                            //have to throw higher than original position to fly
                            var portion = GetFlyCapacity(velocity: _travelDistance / (int)DateTime.Now.Subtract(_travelStart).TotalMilliseconds);
                            await FlyBoomerang((int)(10 * portion), _travelDistance + 300 * portion + 100);
                        }
                        else if (_topview.TranslationY > 100)
                        {
                            await FlyBoomerang(10, 450);
                        }
                        else
                        {
                            _topview.TranslationX = 0;
                            _topview.TranslationY = 10;
                        }

                        _topview.Rotation = 0;
                        _topview = null;

                        _travelDistance = 0;
                        _isTravelUp = false;
                        _isflying = false;
                        _isBeingDragged = false;
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