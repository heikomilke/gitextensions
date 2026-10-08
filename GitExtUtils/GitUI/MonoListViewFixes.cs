using System;
using System.Reflection;
using System.Windows.Forms;

namespace GitExtUtils.GitUI
{
    /// <summary>
    /// Workarounds for Mono's ListView implementation (Linux/Mono build).
    /// </summary>
    public static class MonoListViewFixes
    {
        /// <summary>
        /// Mono's ListView starts a rubber-band ("box") selection whenever a mouse press does not hit
        /// an item exactly, which with owner-drawn rows happens on ordinary clicks. This cancels the box
        /// selection right after Mono's own mouse handler has started it, so a click only selects.
        /// Does nothing on Windows or when Mono's internals are not found.
        /// </summary>
        public static void DisableBoxSelect(ListView listView)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT || listView is null)
            {
                return;
            }

            try
            {
                FieldInfo itemControlField = typeof(ListView).GetField("item_control", BindingFlags.Instance | BindingFlags.NonPublic);
                if (itemControlField?.GetValue(listView) is not Control itemControl)
                {
                    return;
                }

                FieldInfo modeField = itemControl.GetType().GetField("box_select_mode", BindingFlags.Instance | BindingFlags.NonPublic);
                if (modeField is null)
                {
                    return;
                }

                object none = Enum.ToObject(modeField.FieldType, 0);
                void Cancel(object sender, MouseEventArgs e) => modeField.SetValue(itemControl, none);

                // Mono attaches its own handlers in the ListView constructor, so ours run after them.
                itemControl.MouseDown += Cancel;
                itemControl.MouseMove += Cancel;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[GitExtensions] could not disable Mono ListView box selection: {ex.Message}");
            }
        }
    }
}
