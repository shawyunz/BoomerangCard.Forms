using System.Linq;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ResolutionGroupName("XamarinDocs")]
[assembly: ExportEffect(typeof(BoomerangCard.Forms.iOS.TouchEffect), "TouchEffect")]

namespace BoomerangCard.Forms.iOS
{
    public class TouchEffect : PlatformEffect
    {
        private TouchRecognizer touchRecognizer;
        private UIView view;

        protected override void OnAttached()
        {
            // Get the iOS UIView corresponding to the Element that the effect is attached to
            view = Control == null ? Container : Control;

            // Uncomment this line if the UIView does not have touch enabled by default
            //view.UserInteractionEnabled = true;

            // Get access to the TouchEffect class in the .NET Standard library
            Effect.TouchEffect effect = (Effect.TouchEffect)Element.Effects.FirstOrDefault(e => e is Effect.TouchEffect);

            if (effect != null && view != null)
            {
                // Create a TouchRecognizer for this UIView
                touchRecognizer = new TouchRecognizer(Element, view, effect);
                view.AddGestureRecognizer(touchRecognizer);
            }
        }

        protected override void OnDetached()
        {
            if (touchRecognizer != null)
            {
                // Clean up the TouchRecognizer object
                touchRecognizer.Detach();

                // Remove the TouchRecognizer from the UIView
                view.RemoveGestureRecognizer(touchRecognizer);
            }
        }
    }
}