using System;
using System.Windows.Forms;

namespace GitExtUtils.GitUI
{
    /// <summary>Dumps a control tree to stderr when GITEXT_DEBUG_LAYOUT is set (diagnostics for the Mono port).</summary>
    public static class DebugLayout
    {
        public static bool Enabled => Environment.GetEnvironmentVariable("GITEXT_DEBUG_LAYOUT") == "1";

        public static void Dump(Control root, int maxDepth = 6)
        {
            if (!Enabled || root is null)
            {
                return;
            }

            Console.Error.WriteLine($"[layout] ---- {root.Name} ({root.GetType().Name}) ----");
            Walk(root, 0, maxDepth);
        }

        private static void Walk(Control c, int depth, int maxDepth)
        {
            string extra = c switch
            {
                SplitContainer s => $" orient={s.Orientation} dist={s.SplitterDistance} p1col={s.Panel1Collapsed} p2col={s.Panel2Collapsed}",
                TabControl t => $" tabs={t.TabCount} sel={t.SelectedIndex} display={t.DisplayRectangle}",
                _ => ""
            };
            Console.Error.WriteLine($"[layout] {new string(' ', depth * 2)}{c.Name,-28} {c.GetType().Name,-24} vis={c.Visible,-5} x11={MonoX11.Describe(c),-8} hwnd={(c.IsHandleCreated ? c.Handle.ToString("x") : "-"),-8} bounds={c.Bounds} dock={c.Dock}{extra}");
            if (depth >= maxDepth)
            {
                return;
            }

            foreach (Control child in c.Controls)
            {
                Walk(child, depth + 1, maxDepth);
            }
        }
    }
}
