using System;
using System.Drawing;
using System.Reflection;

namespace GitExtUtils.GitUI
{
    /// <summary>
    /// Mono's Windows Forms on Linux imports the desktop (GTK) colour scheme into
    /// <see cref="SystemColors"/> during initialisation, but only partially: with a dark
    /// desktop theme, Control/Menu/ControlText become dark while Window stays white, and the
    /// application ends up with a mix of dark and light surfaces.
    /// Git Extensions is designed around the light Windows palette, so this restores it.
    /// </summary>
    public static class MonoSystemColors
    {
        private static readonly (KnownColor Color, uint Argb)[] LightPalette =
        {
            (KnownColor.ActiveBorder, 0xFFB4B4B4),
            (KnownColor.ActiveCaption, 0xFF99B4D1),
            (KnownColor.ActiveCaptionText, 0xFF000000),
            (KnownColor.AppWorkspace, 0xFFABABAB),
            (KnownColor.Control, 0xFFF0F0F0),
            (KnownColor.ControlDark, 0xFFA0A0A0),
            (KnownColor.ControlDarkDark, 0xFF696969),
            (KnownColor.ControlLight, 0xFFE3E3E3),
            (KnownColor.ControlLightLight, 0xFFFFFFFF),
            (KnownColor.ControlText, 0xFF000000),
            (KnownColor.Desktop, 0xFF000000),
            (KnownColor.GrayText, 0xFF6D6D6D),
            (KnownColor.Highlight, 0xFF0078D7),
            (KnownColor.HighlightText, 0xFFFFFFFF),
            (KnownColor.HotTrack, 0xFF0066CC),
            (KnownColor.InactiveBorder, 0xFFF4F7FC),
            (KnownColor.InactiveCaption, 0xFFBFCDDB),
            (KnownColor.InactiveCaptionText, 0xFF000000),
            (KnownColor.Info, 0xFFFFFFE1),
            (KnownColor.InfoText, 0xFF000000),
            (KnownColor.Menu, 0xFFF0F0F0),
            (KnownColor.MenuText, 0xFF000000),
            (KnownColor.ScrollBar, 0xFFC8C8C8),
            (KnownColor.Window, 0xFFFFFFFF),
            (KnownColor.WindowFrame, 0xFF646464),
            (KnownColor.WindowText, 0xFF000000),
            (KnownColor.ButtonFace, 0xFFF0F0F0),
            (KnownColor.ButtonHighlight, 0xFFFFFFFF),
            (KnownColor.ButtonShadow, 0xFFA0A0A0),
            (KnownColor.GradientActiveCaption, 0xFFB9D1EA),
            (KnownColor.GradientInactiveCaption, 0xFFD7E4F2),
            (KnownColor.MenuBar, 0xFFF0F0F0),
            (KnownColor.MenuHighlight, 0xFF3399FF),
        };

        /// <summary>
        /// Overwrites the system colour entries of Mono's System.Drawing colour table with the
        /// light Windows palette. Call once, right after <c>Application.EnableVisualStyles()</c>
        /// (which is what triggers Mono's import) and before any control is created.
        /// Does nothing on Windows or when the table cannot be found.
        /// </summary>
        public static void ForceLightPalette()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                return;
            }

            try
            {
                // Mono's own System.Windows.Forms.Theme writes the imported colours through exactly this path.
                Type table = Type.GetType("System.Drawing.KnownColorTable, System.Drawing");
                MethodInfo ensure = table?.GetMethod("EnsureColorTable", BindingFlags.Static | BindingFlags.NonPublic);
                FieldInfo field = table?.GetField("s_colorTable", BindingFlags.Static | BindingFlags.NonPublic);
                if (ensure is null || field is null)
                {
                    return;
                }

                ensure.Invoke(null, null);
                if (field.GetValue(null) is not int[] colors)
                {
                    return;
                }

                foreach (var (color, argb) in LightPalette)
                {
                    int index = (int)color;
                    if (index < colors.Length)
                    {
                        colors[index] = unchecked((int)argb);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[GitExtensions] could not reset Mono system colours: {ex.Message}");
            }
        }
    }
}
