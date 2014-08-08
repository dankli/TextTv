using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json;
using TextTv.PhoneApp.Infrastructure;
using TextTv.Shared.Infrastructure;
using TextTv.Shared.Model;
using TextTv.Windows.AppContext;
using AppResources = TextTv.PhoneApp.Infrastructure.AppResources;

namespace TextTv.PhoneApp
{
    public sealed partial class MainPage : Page
    {
        private readonly ApiCaller apiCaller;
        private readonly PageNumberHandler pageNumberHandler;
        private SyncPages syncPages;
        private readonly ModeHandler modeHandler;
        private readonly AppResources appResources;
        private readonly NotifierTaskHandler notifier;
        private readonly HtmlParserFactory htmlParserFactory;

        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += OnLoaded;
            this.apiCaller = new ApiCaller();
            this.pageNumberHandler = new PageNumberHandler(100);
            this.appResources = new AppResources();
            this.modeHandler = new ModeHandler(new AppResources(), new LocalSettingsProvider(),  s => this.ThemeAppBarButton.Label = s);
            this.notifier = new NotifierTaskHandler(this.SetSyncBarAppText);
            this.htmlParserFactory = new HtmlParserFactory();

            this.WebViewControl.ScriptNotify += WebViewControl_ScriptNotify;
            
            this.CreatePopup();
        }

        private async void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.modeHandler.Initialize();
            this.syncPages = await new SyncPages(this.apiCaller, new SyncPagesIO()).Initialize();
            
#if DEBUG
            if (Debugger.IsAttached)
            {
                await this.syncPages.Clear();    
            }
#endif
            if (await this.syncPages.Any())
            {
                bool valid = await notifier.RegisterTask();

                if (valid == false)
                {
                    SyncAppBarButton.Visibility = Visibility.Collapsed;
                }   
            }

            this.SetSyncBarAppText();    
        }

        private void CreatePopup()
        {
            this.popBorder.Width = Window.Current.Bounds.Width;
            this.popSyncBorder.Width = Window.Current.Bounds.Width;
            this.popBorderNoConnection.Width = Window.Current.Bounds.Width;

            List<MonitorTimespan> timespans = new List<MonitorTimespan>
            {
                new MonitorTimespan{ Text = this.appResources.Get("NoLimit"), Timespan = DateTime.MaxValue},
#if DEBUG
                new MonitorTimespan{ Text = this.appResources.Get("30seconds"), Timespan = DateTime.Now.AddSeconds(30)},
#endif
                new MonitorTimespan{ Text = this.appResources.Get("30minutes"), Timespan = DateTime.Now.AddMinutes(30)},
                new MonitorTimespan{ Text = this.appResources.Get("1hour"), Timespan = DateTime.Now.AddHours(1)},
                new MonitorTimespan{ Text = this.appResources.Get("2hours"), Timespan = DateTime.Now.AddHours(2)},
                new MonitorTimespan{ Text = this.appResources.Get("3hours"), Timespan = DateTime.Now.AddHours(3)},
                new MonitorTimespan{ Text = this.appResources.Get("4hours"), Timespan = DateTime.Now.AddHours(4)},
                new MonitorTimespan{ Text = this.appResources.Get("18hours"), Timespan = DateTime.Now.AddHours(18)},
                new MonitorTimespan{ Text = this.appResources.Get("1day"), Timespan = DateTime.Now.AddDays(1)},
            };

            this.CbmTimeSpanComboBox.ItemsSource = timespans;
        }

        private void WebViewControl_ScriptNotify(object sender, NotifyEventArgs e)
        {
            string[] values = e.Value.Split(new[] { ":" }, StringSplitOptions.None);
            if (values.Length == 1)
            {
                pageNumberHandler.SetCurrentPage(int.Parse(values[0]));
                NavigateTo();
                return;
            }

            if (values[0] == "swipe")
            {
                if (values[1] == "left")
                {
                    pageNumberHandler.Next();
                    NavigateTo();
                }
                else
                {
                    pageNumberHandler.Back();
                    NavigateTo();
                }
            }

        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null && !string.IsNullOrWhiteSpace(e.Parameter.ToString()))
            {
                dynamic param = JsonConvert.DeserializeAnonymousType(e.Parameter.ToString(), new { type = string.Empty, page = string.Empty});
                if (param.type == "toast")
                {
                    int page = int.Parse(param.page);
                    pageNumberHandler.SetCurrentPage(page);
                }
            }

            NavigateTo();
        }

        private void NavigateTo()
        {
            if (this.syncPages != null)
            {
                SetSyncBarAppText();    
            }

            Viewer.Initialize(this.modeHandler.CurrentTextTvMode, this.htmlParserFactory, this.pageNumberHandler)
                .GetRawHtml()
                .FlowActions(
                    ifException: () => this.OnUiThread(() =>
                    {
                        this.popNoConnection.IsOpen = true;
                    }),
                    ifNoContent: () =>  this.OnUiThread(() =>
                    {
                        this.bar.Visibility = Visibility.Visible;
                        this.WebViewControl.Visibility = Visibility.Collapsed;
                        pageNumberHandler.Continue();
                        NavigateTo();
                    }),
                    ifValidContent: () => this.OnUiThread(() =>
                    {
                        if (this.bar.Visibility == Visibility.Visible)
                        {
                            this.bar.Visibility = Visibility.Collapsed;
                        }

                        this.WebViewControl.Visibility = Visibility.Visible;
                    }))
                .ParseForView((markup) => this.OnUiThread(() => WebViewControl.NavigateToString(markup)));
        }

        private async void OnUiThread(Action action, CoreDispatcherPriority priority = CoreDispatcherPriority.Normal)
        {
            await this.Dispatcher.RunAsync(priority, () => action());
        }

        private async void SetSyncBarAppText()
        {
            if (await this.syncPages.ExistsInSync(this.pageNumberHandler.CurrentPage).ConfigureAwait(false))
            {
                this.OnUiThread(() =>
                {
                    this.SyncAppBarButton.Label = this.appResources.Get("RemoveMonitoring");    
                });
            }
            else
            {
                this.OnUiThread(() =>
                {
                    this.SyncAppBarButton.Label = this.appResources.Get("MonitorPage");    
                });
            }
        }

        private void Browser_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            if (!args.IsSuccess)
            {
                Debug.WriteLine("Navigation to this page failed, check your internet connection.");
            }
        }

        private void BackAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            pageNumberHandler.Back();
            NavigateTo();
        }

        private void ForwardAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            pageNumberHandler.Next();
            NavigateTo();
        }

        private void RefreshAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo();
        }

        private void SetCurrentAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            this.popSetNumber.IsOpen = true;
            this.tbSetCurrent.Focus(FocusState.Programmatic);
        }

        private void TbSetCurrent_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.tbSetCurrent.Text.Length == 3)
            {
                pageNumberHandler.SetCurrentPage(int.Parse(this.tbSetCurrent.Text));
                this.popSetNumber.IsOpen = false;
                this.tbSetCurrent.Text = string.Empty;
                NavigateTo();
            }
        }

        private async void SyncAppBarButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (await this.syncPages.ExistsInSync(this.pageNumberHandler.CurrentPage) == false)
            {
                this.CbmTimeSpanComboBox.SelectedIndex = -1;
                this.popMonitorPage.IsOpen = true;
            }
            else
            {
                await this.syncPages.Remove(this.pageNumberHandler.CurrentPage, this.SetSyncBarAppText);
                await NotiferRegisterOrNot();
            }
        }

        private async void CbmTimeSpanComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Any())
            {
                this.popMonitorPage.IsOpen = false;
                await this.syncPages.Add(this.pageNumberHandler.CurrentPage, e.AddedItems[0] as MonitorTimespan, this.SetSyncBarAppText).ConfigureAwait(false);
                await NotiferRegisterOrNot();
            }
        }

        private async Task NotiferRegisterOrNot()
        {
            if (await this.syncPages.Any())
            {
                await this.notifier.RegisterTask();
            }
            else
            {
                this.notifier.UnRegisterTask();
            }
        }

        private void ThemeAppBarButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.modeHandler.Toggle();

            this.NavigateTo();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            this.popNoConnection.IsOpen = false;
        }


        private void TbSetCurrent_OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (this.popSetNumber.IsOpen == true)
            {
                this.popSetNumber.IsOpen = false;
            }
        }

        private async void ScreenShotButton_OnClick(object sender, RoutedEventArgs e)
        {
            //FileSavePicker picker = new FileSavePicker();
            //picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            //picker.SuggestedFileName = "capture.bmp";
            //picker.FileTypeChoices.Add("Bitmap File", new List<string>() { ".bmp" });
            //StorageFile file = await picker.PickSaveFileAsync();
            StorageFile file = await KnownFolders.PicturesLibrary.CreateFileAsync("capture.bmp");

            CaptureElementToFile(this.WebViewControl, file);
        }

        private async void CaptureElementToFile(UIElement element, StorageFile file)
        {
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap();
            await renderTargetBitmap.RenderAsync(element);
            IBuffer pixelBuffer = await renderTargetBitmap.GetPixelsAsync();

            DisplayInformation dispInfo = DisplayInformation.GetForCurrentView();

            using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.BmpEncoderId, stream);

                encoder.SetPixelData(BitmapPixelFormat.Rgba8, BitmapAlphaMode.Straight,
                    (uint)renderTargetBitmap.PixelWidth,
                    (uint)renderTargetBitmap.PixelHeight,
                    dispInfo.LogicalDpi, dispInfo.LogicalDpi,
                    pixelBuffer.ToArray());

                await encoder.FlushAsync();
            }
        }
    }
}


