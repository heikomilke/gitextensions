using System;

namespace GitUI.UserControls
{
    public class NativeTreeView : System.Windows.Forms.TreeView
    {
        protected override void CreateHandle()
        {
            base.CreateHandle();
            // Linux/Mono build: no uxtheme.SetWindowTheme
        }
    }
}
