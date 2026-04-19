using Android.Content;
using Android.OS;
using Android.Views.InputMethods;
using Android.Webkit;
using AndroidHUD;
using AngleSharp;
using Microsoft.Maui.Handlers;
using MPowerKit.ProgressRing;
using System.Text.RegularExpressions;
using UraniumUI.Material.Controls;
using static Soundloader;
using Console = System.Console;
using Image = Microsoft.Maui.Controls.Image;
using Log = Android.Util.Log;


namespace SoundLoaderMaui
{
    public partial class MainPage : ContentPage
    {
        private static string Tag = nameof(MainPage);

        public Microsoft.Maui.Controls.WebView pwv;

        private uint ANIM_LENGTH = 333;

        public IServiceDownload Services;
        public static int successfulRuns = 0;
        public static bool MFailedShowInter = false;
        string mTitle = "";
        public string MTitle
        {
            get { return mTitle; }
            set
            {
                if (value == mTitle)
                {
                    return;
                }

                mTitle = value;
                OnPropertyChanged("MTitle");
            }
        }

        string mArtist = "";
        public string MArtist
        {
            get { return mArtist; }
            set
            {
                if (value == mArtist)
                {
                    return;
                }

                mArtist = value;
                OnPropertyChanged("MArtist");
            }
        }
        string mThumbnailUrl = "";
        public string MThumbnailUrl
        {
            get { return mThumbnailUrl; }
            set
            {
                if (value == mThumbnailUrl)
                {
                    return;
                }

                mThumbnailUrl = value;
                OnPropertyChanged("MThumbnailUrl");
            }
        }

        string mMessageProgress = "";
        public string MMessageProgress
        {
            get { return mMessageProgress; }
            set
            {
                Console.WriteLine($"Setting mMessageProgress={value}");
                if (value == mMessageProgress)
                {
                    return;
                }

                mMessageProgress = value;
                OnPropertyChanged("MMessageProgress");
            }
        }

        string mMessageToast = "";
        public string MMessageToast
        {
            get { return mMessageToast; }
            set
            {
                Console.WriteLine($"Setting mMessageToast={value}");
                if (value == mMessageToast)
                {
                    return;
                }

                mMessageToast = value;
                OnPropertyChanged("MMessageToast");
            }
        }

        string mFragmentTitle = "";
        public string MFragmentTitle
        {
            get { return mFragmentTitle; }
            set
            {
                if (value == mFragmentTitle)
                {
                    return;
                }

                mFragmentTitle = value;
                OnPropertyChanged("MFragmentTitle");
            }
        }

        string mFragmentSubtitle = "";
        public string MFragmentSubtitle
        {
            get { return mFragmentSubtitle; }
            set
            {
                if (value == mFragmentSubtitle)
                {
                    return;
                }

                mFragmentSubtitle = value;
                OnPropertyChanged("MFragmentSubtitle");
            }
        }

        string mFragmentBody = "";
        public string MFragmentBody
        {
            get { return mFragmentBody; }
            set
            {
                if (value == mFragmentBody)
                {
                    return;
                }

                mFragmentBody = value;
                OnPropertyChanged("MFragmentBody");
            }
        }

        string mFragmentPositive = "";
        public string MFragmentPositive
        {
            get { return mFragmentPositive; }
            set
            {
                if (value == mFragmentPositive)
                {
                    return;
                }

                mFragmentPositive = value;
                OnPropertyChanged("MFragmentPositive");
            }
        }

        string mFragmentDismiss = "";
        public string MFragmentDismiss
        {
            get { return mFragmentDismiss; }
            set
            {
                if (value == mFragmentDismiss)
                {
                    return;
                }

                mFragmentDismiss = value;
                OnPropertyChanged("MFragmentDismiss");
            }
        }

        // MAIN PAGE
        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;
        }
        public MainPage(IServiceDownload s)
        {
            InitializeComponent();
            BindingContext = this;
            Services = s;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // prepare destination file dirs
            Soundloader.PrepareFileDirs();

            MainActivity.ActivityCurrent.CheckForIntent();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            if (null != pwv)
            {
                ((IWebViewHandler)pwv.Handler).PlatformView.SetWebViewClient(null);
                ((IWebViewHandler)pwv.Handler).PlatformView.Destroy();
                pwv = null;
            }
        }

        protected override bool OnBackButtonPressed()
        {
            // Use Dispatcher to run async code on the main thread for UI interactions
            Dispatcher.Dispatch(async () =>
            {
                HidePopup();
            });

            // Return true to prevent the default back button action immediately
            return true;
        }

        // ON CLICKED
        private void OnAboutClicked(object sender, EventArgs e)
        {
            var aboutUrl = "https://mobileapps.green/";
            Intent intent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(aboutUrl));
            MainActivity.ActivityCurrent.StartActivity(intent);
        }

        private void OnPrivacyPolicyClicked(object sender, EventArgs e)
        {
            var privacyUrl = "https://mobileapps.green/privacy-policy";
            Intent intent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(privacyUrl));
            MainActivity.ActivityCurrent.StartActivity(intent);
        }

        private void OnHelpClicked(object sender, EventArgs e)
        {
            Console.WriteLine($"{Tag} OnHelpClicked");
            ShowPopup("Help");
        }

        private void OnCloseClicked(object sender, EventArgs e)
        {
            HidePopup();
        }

        private void OnPasteClicked(object sender, EventArgs e)
        {
            Console.WriteLine("OnPasteClicked");

            Task.Run(async () =>
            {
                ResetVars();
                await ClearTextfield();
                // give ontextchanged handler time to call showEmptyUI
                await Task.Delay(250);

                string clip = Clipboard.GetTextAsync().Result;
                Console.WriteLine("clipboard text: " + clip);

                TextField mTextField = (TextField)FindByName("main_textfield");
                mTextField.Text = clip;
            });
        }

        private void OnDownloadClicked(object sender, EventArgs e)
        {
            Console.WriteLine("OnDownloadClicked");

            // register broadcast receiver
            if ((int)Build.VERSION.SdkInt >= 33)
            {
                MainActivity.ActivityCurrent.RegisterReceiver(MainActivity.MFinishReceiver, new IntentFilter("69"), Android.Content.ReceiverFlags.Exported);
            }
            else
            {
                MainActivity.ActivityCurrent.RegisterReceiver(MainActivity.MFinishReceiver, new IntentFilter("69"));
            }

            ShowDownloadingUI();

            // start download
            Services.Start();
        }

        // INPUT
        private readonly string INPUT_REGEX = @"https?:\/\/(?:on\.soundcloud\.com\/[\w\-\.]+|(?:www\.)?soundcloud\.com\/[\w\-\.]+\/(?:sets\/)?[\w\-\.]+)(?:\?.*)?$";
        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Console.WriteLine($"{Tag} OnTextChanged");

            string oldText = e.OldTextValue;
            string newText = e.NewTextValue;
            string input = ((TextField)sender).Text;

            if (input != null)
            {
                int lengthDiff;
                if (oldText == null)
                {
                    lengthDiff = newText.Length;
                } else
                {
                    lengthDiff = newText.Length - oldText.Length;
                }
                
                if (input.Length == 0)
                {
                    Console.WriteLine("text field text cleared!");
                    ShowEmptyUI();
                }
                else if (lengthDiff > 1 || lengthDiff == 0)
                {
                    Console.WriteLine("text field text pasted");
                    if (input != null && input != "")
                    {
                        HandleInput(input);
                    }
                }
                else if (lengthDiff == 1)
                {
                    // character typed
                }
                else
                {
                    // character deleted
                }
            }
            else
            {
                Console.WriteLine("input is null!");
            }
        }

        private void OnTextCompleted(object sender, EventArgs e)
        {
            Console.WriteLine("OnTextCompleted");
            string input = ((TextField)FindByName("main_textfield")).Text.ToString();
            HandleInput(input);
        }

        // LOAD / DOWNLOAD
        public void HandleInput(string input)
        {
            // check internet connection
            NetworkAccess accessType = Connectivity.Current.NetworkAccess;
            if (accessType != NetworkAccess.Internet)
            {
                MMessageToast = "Please connect to the internet.";
                Console.WriteLine($"{Tag} {MMessageToast}");
                AndHUD.Shared.ShowError(MainActivity.ActivityCurrent, MMessageToast, MaskType.Black, TimeSpan.FromSeconds(2));
                return;
            }

            // validate input
            var match = Regex.Match(input, INPUT_REGEX, RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                Console.WriteLine($"{Tag} input invalid");
                // TODO show toast
            } else
            {
                Log.Info(Tag, "valid input: " + match.Groups[0].ToString());
                
                // update ui
                ShowPreparingUI();

                // trim input
                string url = match.Groups[0].ToString();
                if (url.IndexOf("https://") > 0)
                {
                    url = url[url.IndexOf("https://")..];
                }

                Dispatcher.Dispatch(async () => await LoadHtml(url));
            }
            
        }

        public async Task LoadHtml(string url)
        {
            Console.WriteLine($"{Tag} LoadHtml url={url}");
            MainPage mp = (MainPage)Shell.Current.CurrentPage;

            var config = Configuration.Default.WithDefaultLoader();
            var address = url;
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync(address);

            // get page html
            string title = document.Title;
            string html = document.Body.ToHtml();
            string head = document.Head.ToHtml();
            Console.WriteLine($"{Tag} document.Title={title}");
            Console.WriteLine($"{Tag} LoadHtml html.Length={html.Length}");

            // print html (note: uncomment when we need to look under the hood)
            /*
            try
            {
                IEnumerable<string> htmlChunks = Split(html, 3500);
                Console.WriteLine($"{Tag} head={head}");
                foreach (string v in htmlChunks)
                {
                    Console.WriteLine($"{Tag} {v}");
                }
            }
            catch (Exception) { }
            */

            // check if playlist
            if (head.Contains("soundcloud://playlist"))
            {
                Console.WriteLine("playlist detected");
            }

            // extract player url
            ExtractPlayerUrl(head);

            // extract <h1>
            string h1 = "";
            if (html.Contains("<h1"))
            {
                h1 = html[html.IndexOf("<h1")..];
                if (h1.Contains("<meta"))
                {
                    h1 = h1[..h1.IndexOf("<meta")];
                }
                else
                {
                    Console.WriteLine($"{Tag} <meta missing!");
                    return;
                }
            }
            else
            {
                Console.WriteLine($"{Tag} <h1 missing!");
                return;
            }

            // get title
            string t = "";
            if (h1.Contains("<a"))
            {
                // extract
                t = h1[h1.IndexOf("<a")..];
                t = t[(t.IndexOf('>')+1)..];
                if (t.Contains("</a"))
                {
                    t = t[..t.IndexOf("</a")];
                }
                // format
                if (t.Contains("&amp;"))
                {
                    t = t.Replace("&amp;", "&");
                }
                foreach (var c in CharsToRemove)
                {
                    t = t.Replace(c, string.Empty);
                }
            }
            MTitle = t;
            Console.WriteLine($"{Tag} MTitle={MTitle}");

            // extract artist
            string a = "";
            if (h1.Contains("<a"))
            {
                // extract
                a = h1[h1.LastIndexOf("<a")..];
                a = a[(a.IndexOf('>') + 1)..];
                if (a.Contains("</a"))
                {
                    a = a[..a.LastIndexOf("</a")];
                }
                if (a.Contains("&amp;"))
                {
                    a = a.Replace("&amp;", "&");
                }
                foreach (var c in CharsToRemove)
                {
                    a = a.Replace(c, string.Empty);
                }
            }
            MArtist = a;
            Console.WriteLine($"{Tag} MArtist={MArtist}");

            // extract thumbnail url
            MThumbnailUrl = ExtractThumbnailUrl(html);

            // extract stream url
            ExtractStreamUrl(html);
            document.Close();

            // load player url
            MainThread.BeginInvokeOnMainThread(() => {
                pwv = (Microsoft.Maui.Controls.WebView)FindByName("preview_webview");
                ((IWebViewHandler)pwv.Handler).PlatformView
                    .SetWebViewClient(new MWebViewClient());
                ((IWebViewHandler)pwv.Handler).PlatformView.Post(() =>
                {
                    ((IWebViewHandler)pwv.Handler).PlatformView
                    .LoadUrl(MPlayerUrl);
                });
            });
        }

        // DIALOG FRAGMENT
        public async void ShowPopup(string title)
        {
            Console.WriteLine($"{Tag}: ShowPopup({title})");

            // fill views
            MFragmentTitle = title;
            if (title == "Upgrade")
            {
                MFragmentSubtitle = "SoundLoader Gold";
                MFragmentBody = "✅  Playlist Downloader\n✅  Album Downloader\n✅  Fastest Speed\n✅  No More Ads!\n";
                MFragmentPositive = "Get It!";
                MFragmentDismiss = "Nah";
                ((Button)FindByName("yearly_button")).IsVisible = true;
            }
            else if (title == "Help")
            {
                MFragmentSubtitle = "How to Use SoundLoader:";
                MFragmentBody = "➊  Copy the URL\n  ⓘ  \"Share\" >> \"Copy link\"\n➋  Paste in search bar (Tap ⚡)\n➌  Tap (⬇)\n    ⓘ  File saved [in Documents folder]";
            }
            else if (title == "Rate")
            {
                MFragmentSubtitle = "";
                MFragmentBody = "Should I maintain SoundLoader?\nLet me know!\n<3";
                MFragmentPositive = "Rate";
                MFragmentDismiss = "Nah";
            }
            else if (title == "SpotiFlyer")
            {
                MFragmentSubtitle = "Downloader for Spotify";
                MFragmentBody = "Could I interest you in our Spotify downloader, too?\n\n✦Ad by Green Mobile✦";
                MFragmentPositive = "Get It";
                MFragmentDismiss = "Nah";
            }
            else if (title == "InSave")
            {
                MFragmentSubtitle = "Downloader for Instagram";
                MFragmentBody = "You might like this too\n\n✦Ad by Green Mobile✦";
                MFragmentPositive = "Get It";
                MFragmentDismiss = "Nah";
            }
            else if (title == "SaveFrom")
            {
                MFragmentSubtitle = "Downloader for Videos";
                MFragmentBody = "You might like this too\n\n✦Ad by Green Mobile✦";
                MFragmentPositive = "Get It";
                MFragmentDismiss = "Nah";
            }
            else if (title == "VscoLoader")
            {
                MFragmentSubtitle = "Downloader for VSCO";
                MFragmentBody = "You might like this too\n\n✦Ad by Green Mobile✦";
                MFragmentPositive = "Get It";
                MFragmentDismiss = "Nah";
            }
            else if (title == "Musi")
            {
                MFragmentSubtitle = "Music Player";
                MFragmentBody = "Your music library.\nFree & Ad-Free.\nForever.\n\n🎧 Ad by Green Mobile 🎧";
                MFragmentPositive = "Get It";
                MFragmentDismiss = "Nah";
            }

            // fade in
            AbsoluteLayout fragment = (AbsoluteLayout)FindByName("fragment_layout");
            fragment.Opacity = 0.0;
            fragment.IsVisible = true;
            await fragment.FadeTo(1.0, ANIM_LENGTH);
        }

        public async void HidePopup()
        {
            Console.WriteLine($"{Tag} HidePopup");

            // fade out fragment
            AbsoluteLayout fragment = (AbsoluteLayout)FindByName("fragment_layout");
            fragment.Opacity = 1.0;
            await fragment.FadeTo(0.0, ANIM_LENGTH);
            fragment.IsVisible = false;

            // ensure hidden yearly button for next open
            ((Button)FindByName("yearly_button")).IsVisible = false;

            // clear text views
            MFragmentTitle = "";
            MFragmentSubtitle = "";
            MFragmentBody = "";
            MFragmentPositive = "";
            MFragmentDismiss = "";
        }
        

        // USER INTERFACE
        public void HideKeyboard()
        {
            // hide keyboard
            var inputMethodManager = MainActivity.ActivityCurrent.GetSystemService(Context.InputMethodService) as InputMethodManager;
            if (inputMethodManager != null && MainActivity.ActivityCurrent is Android.App.Activity)
            {
                var activity = MainActivity.ActivityCurrent as Android.App.Activity;
                var token = activity.CurrentFocus?.WindowToken;
                inputMethodManager.HideSoftInputFromWindow(token, HideSoftInputFlags.None);

                activity.Window.DecorView.ClearFocus();
            }
        }

        public async Task ClearTextfield()
        {
            TextField mTextField = (TextField)FindByName("main_textfield");
            if (mTextField != null)
            {
                mTextField.Text = "";
            }
        }

        public async Task ShowEmptyUI()
        {
            Console.WriteLine($"{Tag}: ShowEmptyUI");

            ResetVars();
            HidePopup();

            // hide buttons
            ButtonView finishBtn = (ButtonView)FindByName("finish_btn");
            ButtonView dlBtn = (ButtonView)FindByName("dl_btn");
            dlBtn.Opacity = 0.0;
            finishBtn.Opacity = 0.0;
            finishBtn.IsVisible = false;
            ((Image)FindByName("preview_img")).Opacity = 0.0;
            ((ButtonView)FindByName("dl_btn")).Opacity = 0.0;
            ((ProgressRing)FindByName("progress_ring")).Opacity = 0.0;
            ((Label)FindByName("progress_label")).Opacity = 0.0;
            ((Frame)FindByName("downloader_frame")).Opacity = 0.0;
        }

        public async Task ShowPreparingUI()
        {
            Console.WriteLine($"{Tag}: ShowPreparingUI");

            HideKeyboard();

            // change progress message
            MMessageProgress = "Loading…";

            // show indeterminate progress ring
            ProgressRing pr = (ProgressRing)FindByName("progress_ring");
            pr.IsIndeterminate = true;
            pr.FadeTo(1.0, ANIM_LENGTH);
            await ((Label)FindByName("progress_label")).FadeTo(1.0, ANIM_LENGTH);
        }

        public async Task ShowPreviewUI()
        {
            Console.WriteLine($"{Tag}: ShowPreviewUI");

            // hide finish button
            ButtonView finishBtn = (ButtonView)FindByName("finish_btn");
            finishBtn.Opacity = 0.0;
            finishBtn.IsVisible = false;

            // show downloader
            ((Frame)FindByName("downloader_frame")).Opacity = 1.0;
            ButtonView dlBtn = (ButtonView)FindByName("dl_btn");
            dlBtn.IsEnabled = true;
            dlBtn.Opacity = 1.0;
            dlBtn.IsVisible = true;

            // hide progress ring
            MMessageProgress = "";
            ((ProgressRing)FindByName("progress_ring")).Opacity = 0.0;
            ((Label)FindByName("progress_label")).Opacity = 0.0;

            // increase thumbnail opacity
            ((Image)FindByName("preview_img")).Opacity = 1.0;
        }

        public async Task ShowDownloadingUI()
        {
            Console.WriteLine($"{Tag}: ShowDownloadingUI");

            // change progress message
            MMessageProgress = "Starting download…\nThis may take a moment.";

            ProgressRing pr = (ProgressRing)FindByName("progress_ring");
            ProgressRing prd = (ProgressRing)FindByName("progress_ring_dlr");
            ButtonView dlBtn = (ButtonView)FindByName("dl_btn");

            pr.IsIndeterminate = true;

            // hide preview UI
            pr.FadeTo(1.0, ANIM_LENGTH);
            pr.FadeTo(1.0, ANIM_LENGTH);
            ((Label)FindByName("progress_label")).FadeTo(1.0, ANIM_LENGTH);
            await dlBtn.FadeTo(0.0, ANIM_LENGTH);

            // show downloading UI
            ((Image)FindByName("preview_img")).FadeTo(0.45, ANIM_LENGTH);
            dlBtn.IsVisible = false;
            prd.IsVisible = true;
            prd.FadeTo(1.0, ANIM_LENGTH);
        }

        public async Task ShowFinishUI()
        {
            Console.WriteLine($"{Tag}: ShowFinishUI");

            // count successful runs
            int runs = 1;
            if (Preferences.Default.ContainsKey("SUCCESSFUL_RUNS"))
            {
                runs += Preferences.Default.Get("SUCCESSFUL_RUNS", 0);
            }
            successfulRuns = runs;
            Console.WriteLine($"{Tag} SUCCESSFUL_RUNS={runs}");
            Preferences.Default.Set("SUCCESSFUL_RUNS", runs);

            // show success message
            MMessageToast = $"Saved! In {AbsPathDocs}";

            ProgressRing prd = (ProgressRing)FindByName("progress_ring_dlr");
            ButtonView finishBtn = (ButtonView)FindByName("finish_btn");

            // hide downloading UI
            ((Image)FindByName("preview_img")).FadeTo(0.85, ANIM_LENGTH);
            prd.FadeTo(0.0, ANIM_LENGTH);
            ((ProgressRing)FindByName("progress_ring")).FadeTo(0.0, ANIM_LENGTH);
            await ((Label)FindByName("progress_label")).FadeTo(0.0, ANIM_LENGTH);

            // show finish ui
            prd.IsVisible = false;
            finishBtn.Opacity = 0.0;
            finishBtn.IsVisible = true;
            await finishBtn.FadeTo(1.0, ANIM_LENGTH);
        }

        public class MWebViewClient : WebViewClient
        {
            private static readonly string Tag = nameof(MWebViewClient);

            public override WebResourceResponse? ShouldInterceptRequest(global::Android.Webkit.WebView? view, IWebResourceRequest? request)
            {
                string url = request.Url.ToString();
                Console.WriteLine($"{Tag} ShouldInterceptRequest url={url}");
                MainPage mp = (MainPage)Shell.Current.CurrentPage;
                if (url.Contains("com/playlists/"))
                {
                    // extract client id
                    string client_id = url.Substring(url.IndexOf("client_id=") + 10);
                    if (client_id.Contains("&"))
                    {
                        client_id = client_id.Substring(0, client_id.IndexOf("&"));
                    }
                    MClientId = client_id;

                    // remove self from webview
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        ((IWebViewHandler)mp.pwv.Handler).PlatformView.SetWebViewClient(null);
                    });

                    // load json response
                    LoadJson(url);
                }
                else if (url.Contains("client_id=")
                    && MFullStreamUrl == ""
                    && MFullstreamUrls.Count == 0)
                {
                    // extract client id
                    string client_id = url.Substring(url.IndexOf("client_id=") + 10);
                    if (client_id.Contains("&"))
                    {
                        client_id = client_id.Substring(0, client_id.IndexOf("&"));
                    }
                    MClientId = client_id;

                    // build full stream url
                    string fullStreamUrl = MStreamUrl + "?client_id=" + client_id;
                    MFullStreamUrl = fullStreamUrl;

                    // TODO MFullstreamUrls.Add(fullStreamUrl);

                    Console.WriteLine($"{Tag} fullStreamUrl={fullStreamUrl}");

                    // load json response
                    LoadJson(fullStreamUrl);
                }
                else if (url.Contains("widget-9"))
                {
                    // get client_id from widget-9 ?
                }

                return base.ShouldInterceptRequest(view, request);
            }
        }
    }
}

