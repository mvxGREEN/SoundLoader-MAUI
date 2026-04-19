using Microsoft.Extensions.Logging;
using UraniumUI;

using Microsoft.Maui.LifecycleEvents;
using SoundLoaderMaui.Platforms.Android;

namespace SoundLoaderMaui
{
    public static class MauiProgram
    {
        private static readonly string Tag = nameof(MauiProgram);
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder.UseMauiApp<App>()
                .UseUraniumUI()
                .UseUraniumUIMaterial()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFontAwesomeIconFonts();
                    fonts.AddMaterialIconFonts();
                });

            Console.WriteLine($"{Tag} building android app");
            builder
                .ConfigureLifecycleEvents(events =>
                {
                    events.AddAndroid(android =>
                    {
                        android.OnCreate((activity, bundle) =>
                        {
                            Console.WriteLine($"{Tag} OnCreate");

                        });
                        android.OnResume(activity =>
                        {
                            Console.WriteLine($"{Tag} OnResume");
                        });
                        android.OnDestroy(activity =>
                        {
                            Console.WriteLine($"{Tag} OnDestroy");

                            // unregister broadcast receiver
                            try
                            {
                                activity.UnregisterReceiver(MainActivity.MFinishReceiver);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"{Tag} MFinishReceiver already unregistered");
                            }
                        });
                    });
                });

            // dependency injection
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddTransient<IServiceDownload, DownloadService>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
