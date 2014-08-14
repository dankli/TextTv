using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using TextTv.Shared.Infrastructure;
using TextTv.Shared.Infrastructure.Contracts;
using TextTv.IOS.AppContext;

namespace TextTv.IPhoneApp
{
	public partial class HybridViewController : UIViewController
	{
		readonly ApiCaller apiCaller;
		readonly PageNumberHandler pageNumberHandler;
		SyncPages syncPages;
		readonly ModeHandler modeHandler;
		readonly AppResources appResources;
		readonly INotifierTaskHandler notifierTaskHandler;
		readonly IHtmlParserFactory htmlParserFactory;

		static bool UserInterfaceIdiomIsPhone {
			get { return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone; }
		}

		public HybridViewController (IntPtr handle) : base (handle)
		{
			this.apiCaller = new ApiCaller ();
			this.pageNumberHandler = new PageNumberHandler (100);
			this.appResources = new AppResources (NSLocale.CurrentLocale.LocaleIdentifier);
		}

		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();

			// Release any cached data, images, etc that aren't in use.
		}

		private void NavigateTo(){
			Viewer.Initialize(this.modeHandler.CurrentTextTvMode, this.htmlParserFactory, this.pageNumberHandler)
				.GetRawHtml()
				.FlowActions(ifNoContent: () => {
					pageNumberHandler.Continue();
					NavigateTo();
				})
				.ParseForView(markup => this.OnUiThread(() => this.webView.LoadHtmlString(markup, null)));
		}

		object OnUiThread (Action action)
		{
			this.InvokeOnMainThread (() => {
				action();
			});
		}

		#region View lifecycle

		public override async void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			this.Initialize ();

			this.modeHandler.Initialize ();
			this.syncPages = await new SyncPages (this.apiCaller, new SyncPagesIO () ).Initialize();

			if (await this.syncPages.Any ()) {
				// Register a background task to monitor pages.
			}


			// Load the rendered HTML into the view with a base URL 
			// that points to the root of the bundled Resources folder
			//webView.LoadHtmlString (page, NSBundle.MainBundle.BundleUrl);

			// Perform any additional setup after loading the view, typically from a nib.
		}

		void Initialize ()
		{

			this.SetToolbarItems (new UIBarButtonItem[] {
				new UIBarButtonItem(UIBarButtonSystemItem.FixedSpace) { Width = 50},
				new UIBarButtonItem (UIBarButtonSystemItem.Rewind, (s, e) => {
					Console.WriteLine ("Back clicked");
				}),
				new UIBarButtonItem(UIBarButtonSystemItem.FixedSpace) { Width = 25},
				new UIBarButtonItem (UIBarButtonSystemItem.Refresh, (s, e) => {
					Console.WriteLine ("Refresh clicked");
				}),
				new UIBarButtonItem(UIBarButtonSystemItem.FixedSpace) { Width = 25},
				new UIBarButtonItem (UIBarButtonSystemItem.Compose, (s, e) => {
					Console.WriteLine ("Refresh clicked");

				}),
				new UIBarButtonItem(UIBarButtonSystemItem.FixedSpace) { Width = 25},
				new UIBarButtonItem (UIBarButtonSystemItem.FastForward, (s, e) => {
					Console.WriteLine ("Refresh clicked");
				})
			}, false);

			this.NavigationController.ToolbarHidden = false;
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
			return true;
			// If the URL is not our own custom scheme, just let the webView load the URL as usual

		}
	}
}

