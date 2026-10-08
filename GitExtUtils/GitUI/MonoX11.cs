using System;
using System.Reflection;
using System.Windows.Forms;

namespace GitExtUtils.GitUI
{
    /// <summary>
    /// Access to Mono's X11 window state (Linux/Mono build). Mono unmaps the X11 window of a control
    /// that becomes zero-sized during layout and occasionally fails to map it again, leaving a control
    /// that is Visible but never painted.
    /// </summary>
    public static class MonoX11
    {
        private static readonly Type HwndType = typeof(Control).Assembly.GetType("System.Windows.Forms.Hwnd");
        private static readonly MethodInfo ObjectFromHandle = HwndType?.GetMethod("ObjectFromHandle", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(IntPtr) }, null);
        private static readonly FieldInfo MappedField = HwndType?.GetField("mapped", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        private static readonly FieldInfo ZeroSizedField = HwndType?.GetField("zero_sized", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        public static bool IsAvailable => Environment.OSVersion.Platform != PlatformID.Win32NT && ObjectFromHandle is not null && MappedField is not null;

        /// <summary>Returns "mapped", "unmapped", "zero" or "?" for diagnostics.</summary>
        public static string Describe(Control control)
        {
            if (!IsAvailable || control is null || !control.IsHandleCreated)
            {
                return "?";
            }

            try
            {
                object hwnd = ObjectFromHandle.Invoke(null, new object[] { control.Handle });
                if (hwnd is null)
                {
                    return "?";
                }

                bool zero = ZeroSizedField is not null && (bool)ZeroSizedField.GetValue(hwnd);
                bool mapped = (bool)MappedField.GetValue(hwnd);
                return zero ? "zero" : mapped ? "mapped" : "unmapped";
            }
            catch
            {
                return "?";
            }
        }

        /// <summary>
        /// Re-maps controls in the tree that are visible, non-zero-sized, but not mapped on X11.
        /// Returns the number of controls repaired.
        /// </summary>
        public static int EnsureMapped(Control root)
        {
            if (!IsAvailable || root is null)
            {
                return 0;
            }

            int repaired = 0;
            Walk(root);
            return repaired;

            void Walk(Control c)
            {
                if (c.IsHandleCreated && c.Visible && c.Width > 0 && c.Height > 0 && Describe(c) == "unmapped")
                {
                    Console.Error.WriteLine($"[GitExtensions] re-mapping unmapped X11 window of {c.Name} ({c.GetType().Name})");
                    c.Visible = false;
                    c.Visible = true;
                    repaired++;
                }

                foreach (Control child in c.Controls)
                {
                    Walk(child);
                }
            }
        }
    }
}
