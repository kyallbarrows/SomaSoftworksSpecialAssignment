using TMPro;
using UnityEngine;

namespace AltEnding.GUI
{
    public class GenericConfirmationModal : AnimatedCanvasManager
    {
		[Header("Confirmation Modal References")]
		[SerializeField] protected TMP_Text messageText;
		[SerializeField] protected TMP_Text headerText;
		[SerializeField] protected TMP_Text confirmText;
		[SerializeField] protected TMP_Text cancelText;
        
		[SerializeField] protected string defaultHeaderText;
		[SerializeField] protected string defaultConfirmText;
		[SerializeField] protected string defaultCancelText;

		System.Action<bool> storedCallback;

		public bool ShowConfirmationPrompt(string message, System.Action<bool> callback) => 
			ShowConfirmationPrompt(message, defaultHeaderText, defaultConfirmText, defaultCancelText, callback);

		public bool ShowConfirmationPrompt(string message, string header, System.Action<bool> callback) =>
			ShowConfirmationPrompt(message, header, defaultConfirmText, defaultCancelText, callback);

		public bool ShowConfirmationPrompt(string message, string header, string confirm, string cancel, System.Action<bool> callback)
		{
			if (currentOpenState.OpenOrOpening())
				return false;
            messageText?.SetText(message);
            headerText?.SetText(header);
            confirmText?.SetText(confirm);
            cancelText?.SetText(cancel);
			storedCallback = callback;
			TurnOn();
			return true;
		}

		public void AnswerConfirmation(bool confirmed)
		{
			storedCallback?.Invoke(confirmed);
			TurnOff();
		}
	}
}
