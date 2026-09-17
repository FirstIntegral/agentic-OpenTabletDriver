using System;
using System.IO;
using System.Text;
using OpenTabletDriver.Plugin;

namespace OpenTabletDriver.UX.Gtk
{
    static class GtkAppTheme
    {
        private const string ResourceName = "OpenTabletDriver.UX.Gtk.Assets.app.css";

        // Keep in sync with Assets/app.css. Used when the embedded resource cannot be read.
        private const string FallbackCss =
            """
            /* GTK3 application CSS for OpenTabletDriver.UX.Gtk.
             * Metrics only (density, radii, padding). No hex colors — follow the active theme.
             */

            button {
              min-height: 32px;
              min-width: 32px;
              padding: 6px 12px;
              border-radius: 6px;
            }

            button.image-button,
            button.flat {
              min-width: 32px;
              padding: 6px;
            }

            button.text-button {
              padding: 6px 12px;
            }

            entry {
              min-height: 32px;
              padding: 4px 8px;
              border-radius: 6px;
            }

            spinbutton {
              min-height: 32px;
              border-radius: 6px;
            }

            spinbutton entry {
              min-height: 32px;
              padding: 4px 8px;
              border-radius: 6px;
            }

            spinbutton button {
              min-height: 24px;
              min-width: 24px;
              padding: 0;
              border-radius: 4px;
            }

            combobox {
              min-height: 32px;
            }

            combobox button {
              min-height: 32px;
              padding: 4px 8px;
              border-radius: 6px;
            }

            /* Leftover TabControl / DocumentControl (About, greeter) */
            notebook > header {
              padding: 4px 6px 0;
            }

            notebook > header > tabs > tab {
              min-height: 28px;
              padding: 6px 14px;
              border-radius: 6px 6px 0 0;
              margin: 0 2px;
            }

            notebook > header > tabs > tab label {
              padding: 0 2px;
            }

            treeview {
              padding: 2px;
            }

            treeview.view {
              padding: 2px 0;
            }

            treeview.view header button {
              min-height: 28px;
              padding: 4px 8px;
              border-radius: 0;
            }

            treeview cell {
              padding: 4px 6px;
            }

            list {
              padding: 4px;
            }

            list row {
              min-height: 32px;
              padding: 4px 8px;
              border-radius: 6px;
              margin: 1px 0;
            }

            menubar {
              padding: 2px 6px;
              min-height: 28px;
            }

            menubar > menuitem {
              padding: 4px 10px;
              border-radius: 4px;
              margin: 1px;
            }

            menu {
              padding: 4px;
              border-radius: 6px;
            }

            menu menuitem {
              min-height: 24px;
              padding: 6px 12px;
              border-radius: 4px;
            }

            paned > separator {
              min-width: 6px;
              min-height: 6px;
              margin: 0;
            }

            scrollbar {
              border: none;
            }

            scrollbar slider {
              min-width: 6px;
              min-height: 6px;
              border-radius: 6px;
              margin: 2px;
            }

            scrollbar.vertical slider {
              min-width: 6px;
            }

            scrollbar.horizontal slider {
              min-height: 6px;
            }

            scrollbar.overlay-indicator slider {
              min-width: 4px;
              min-height: 4px;
              margin: 0;
            }

            scrollbar.overlay-indicator:hover slider,
            scrollbar.overlay-indicator.hovering slider,
            scrollbar.overlay-indicator.dragging slider {
              min-width: 8px;
              min-height: 8px;
              margin: 2px;
            }

            headerbar,
            .titlebar {
              min-height: 40px;
              padding: 4px 8px;
            }

            headerbar button.titlebutton {
              min-height: 24px;
              min-width: 24px;
              padding: 0;
              border-radius: 12px;
            }

            frame {
              border-radius: 6px;
            }

            frame > border {
              border-radius: 6px;
            }

            frame > label {
              padding: 0 4px;
            }

            dialog .dialog-action-box,
            messagedialog .dialog-action-box {
              padding: 8px;
            }

            tooltip {
              border-radius: 6px;
              padding: 6px 10px;
            }

            scale {
              padding: 8px 4px;
            }

            checkbutton,
            radiobutton {
              padding: 4px;
            }
            """;

        public static void Apply()
        {
            try
            {
                var screen = global::Gdk.Screen.Default;
                if (screen == null)
                    return;

                var css = ReadEmbeddedCss();
                var provider = new global::Gtk.CssProvider();
                if (css == null || !TryLoad(provider, css))
                {
                    provider = new global::Gtk.CssProvider();
                    if (!TryLoad(provider, FallbackCss))
                        return;
                }

                global::Gtk.StyleContext.AddProviderForScreen(
                    screen,
                    provider,
                    global::Gtk.StyleProviderPriority.Application);
            }
            catch (Exception ex)
            {
                TryLog($"Application CSS not applied: {ex.Message}");
            }
        }

        private static string? ReadEmbeddedCss()
        {
            try
            {
                var assembly = typeof(GtkAppTheme).Assembly;
                using var stream = assembly.GetManifestResourceStream(ResourceName);
                if (stream == null)
                    return null;

                using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
                return reader.ReadToEnd();
            }
            catch
            {
                return null;
            }
        }

        private static bool TryLoad(global::Gtk.CssProvider provider, string css)
        {
            try
            {
                return provider.LoadFromData(css);
            }
            catch
            {
                return false;
            }
        }

        private static void TryLog(string text)
        {
            try
            {
                Log.Write("UX.Gtk", text, LogLevel.Warning);
            }
            catch
            {
                // Never crash the GUI over theming or logging.
            }
        }
    }
}
