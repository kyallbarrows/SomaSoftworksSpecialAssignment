using Articy.Unity;
using Articy.Unity.Interfaces;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AltEnding
{
	public class BranchButton : MonoBehaviour
	{
		[SerializeField]
		private Selectable mySelectable;
		// The unity ui button text, so we can assign in code different labels.
		[SerializeField]
		private TMP_Text buttonText;
		[SerializeField, Min(5)]
		private int maxLength;
		// The branch identifier, so we can tell the processor which way it should continue to traverse our flow when the user clicked this button
		[SerializeField] private Branch branch;

#if UNITY_EDITOR
		private void OnValidate()
		{
			if (branch != null) UpdateButtonText(branch.Target);
		}
#endif

		public void Start()
        {
			// You would usually do this in the inspector in the button itself, but it's so important for the correct functionality
			// we placed it here to show you what happened when the button is pressed by the user.
			GetComponentInChildren<Button>().onClick.AddListener(OnBranchSelected);
		}

        /// Called when the button is created to represent a single branch out of possible many. This is important to give the ui button the branch that is used to follow along if the user pressed the button in the ui
        public void AssignBranch(Branch aBranch)
		{
			// We find the text component in our children, this should be the label of the button, unless you changed the button somewhat, then you need to take care of selecting the proper text.
			if (buttonText == null) buttonText = GetComponentInChildren<TMP_Text>();

			// Store for later use
			branch = aBranch;

			// A nice debug aid, if we show all branches (valid or invalid) we can identify branches that shouldn't be allowed because of our scripts
			buttonText.color = aBranch.IsValid ? Color.black : Color.red;

			var target = aBranch.Target;
			UpdateButtonText(target);

            string choiceTooltip = ArticyStoryHelper.Instance.GetChoiceTooltip(target);
            Debug.Log($"{buttonText.text} Tooltip: {choiceTooltip}");
		}

		private void UpdateButtonText(IFlowObject target)
		{
			if (buttonText == null) return;
			buttonText.text = "";

			// Now we figure out which text our button should have, and we just try to cast our target into different types, 
			// creating some sort of priority naming: MenuText -> DisplayName -> TechnicalName -> ClassName/Null
			if (target is IObjectWithMenuText obj)
				buttonText.text = obj.MenuText;

			// If the button text is empty, whether because the flow object does not have menu text or because the menu text was left blank, set the button text to the next flow object's text
			if (string.IsNullOrWhiteSpace(buttonText.text) && target is IObjectWithLocalizableText textObj)
                buttonText.text = textObj.Text;

			// If the button text is still empty, set the button text to "Next"
			if (string.IsNullOrWhiteSpace(buttonText.text))
                buttonText.text = "Next";

			// Trim the button text length to avoid extreme lengths.
			if (buttonText.text.Length > maxLength)
				buttonText.text = buttonText.text.Remove(Mathf.Max(4, maxLength - 3)) + "...";
		}

		// The method used when the button is clicked
		public void OnBranchSelected()
		{
			// By giving the processor the branch assigned to the button on creation, the processor knows where to continue the flow
			ArticyFlowController.Instance.PlayBranch(branch);
		}

		public void OnBranchHover()
		{
			//somehow get the tooltip text
			//
		}

		public void UISelect()
		{
			if (mySelectable != null) mySelectable.Select();
		}
	}
}
