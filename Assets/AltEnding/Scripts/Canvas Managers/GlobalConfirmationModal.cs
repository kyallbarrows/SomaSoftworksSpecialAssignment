using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AltEnding.GUI
{
    public class GlobalConfirmationModal : PersistentSingleton<GlobalConfirmationModal>
    {
        [SerializeField]
        protected GenericConfirmationModal myModal;

        public bool ShowConfirmationPrompt(string message, System.Action<bool> callback)
        {
            if (myModal == null)
                return false;

            return myModal.ShowConfirmationPrompt(message, callback);
        }

        public bool ShowConfirmationPrompt(string message, string header, System.Action<bool> callback)
        {
            if (myModal == null)
                return false;

            return myModal.ShowConfirmationPrompt(message, header, callback);
        }

        public bool ShowConfirmationPrompt(string message, string header, string confirm, string cancel, System.Action<bool> callback)
        {
            if (myModal == null)
                return false;

            return myModal.ShowConfirmationPrompt(message, header, confirm, cancel, callback);
        }
    }
}
