using System;
using System.Collections.Generic;
using System.Text;
using Prism.

namespace TrackerServices
{
    public interface xIDialogService
    {
        void Show(string name, IDialogParameters parameters, Action<IDialogResult> callback);
        void ShowDialog(string name, IDialogParameters parameters, Action<IDialogResult> callback);

        //public static implicit operator string(IDialogService v);
    }
}