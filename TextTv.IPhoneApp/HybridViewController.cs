using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using TextTv.Shared.Infrastructure;
using TextTv.Shared.Infrastructure.Contracts;

namespace TextTv.IPhoneApp
{
	public partial class HybridViewController : UIViewController
	{
		readonly ApiCaller apiCaller;
		readonly PageNumberHandler pageNumberHandler;
		SyncPages syncPages;
		readonly ModeHandler modeHandler;
		readonly IAppResources appResources;
		readonly INotifierTaskHandler notifierTaskHandler;
		readonly IHtmlParserFactory htmlParserFactory;

		static bool UserInterfaceIdiomIsPhone {
			get { return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone; }
		}

		public HybridViewController (IntPtr handle) : base (handle)
		{
			this.apiCaller = new ApiCaller ();
			this.pageNumberHandler = new PageNumberHandler (100);
			this.appResources = 
		}

		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();

			// Release any cached data, images, etc that aren't in use.
		}

		#region View lifecycle

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			// Load the rendered HTML into the view with a base URL 
			// that points to the root of the bundled Resources folder
			webView.LoadHtmlString (page, NSBundle.MainBundle.BundleUrl);

			// Perform any additional setup after loading the view, typically from a nib.
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
		}

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
		}

		public override void ViewWillDisappear (bool animated)
		{
			base.ViewWillDisappear (animated);
		}

		public override void ViewDidDisappear (bool animated)
		{
			base.ViewDidDisappear (animated);
		}

		#endregion

		bool HandleShouldStartLoad (UIWebView webView, NSUrlRequest request, UIWebViewNavigationType navigationType)
		{

			// If the URL is not our own custom scheme, just let the webView load the URL as usual

		}
	}
}

