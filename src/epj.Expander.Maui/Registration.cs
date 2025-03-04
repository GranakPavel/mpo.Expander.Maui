using Microsoft.Maui.Handlers;

namespace epj.Expander.Maui;
public static class Registration
{
    public static MauiAppBuilder UseExpander(this MauiAppBuilder builder)
    {
        builder.ConfigureMauiHandlers(h =>
        {
            h.AddHandler<Expander, ContentViewHandler>();
            
        });

        builder.ConfigureFonts(fonts =>
        {
            fonts.AddFont("materialdesignicons.ttf", "MaterialDesignIconsFont");
        });

        return builder;
    }
}
