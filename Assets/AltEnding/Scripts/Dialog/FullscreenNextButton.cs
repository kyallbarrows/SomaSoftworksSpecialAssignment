using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AltEnding.GUI;
using Articy.Unity;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace AltEnding.Dialog
{
	public class FullscreenNextButton : MonoBehaviour
	{
		// the branch identifier, so we can tell the processor which way it should continue to traverse our flow when the user clicked this button
		private Branch branch;
#if UseNA
        [NaughtyAttributes.ReadOnly]
#endif
		[SerializeField] private bool amIActive;
		[SerializeField]
		private UnityEngine.UI.Image trigger;
		[SerializeField]
		protected DialogueCanvasManager myDialogCanvasManager;
#if ENABLE_INPUT_SYSTEM
		protected RedemptionControls controls;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER && !ENABLE_INPUT_SYSTEM
		[SerializeField]
		protected KeyCode primaryKeybind;
		[SerializeField]
		protected KeyCode secondaryKeybind;
#endif
#if ENABLE_INPUT_SYSTEM
		[SerializeField] protected InputActionReference nextDialogLineAction;
#endif

		Coroutine delayedContinueCoroutine;

		private void OnValidate()
		{
			if(trigger == null)
			{
				trigger = GetComponentInChildren<UnityEngine.UI.Image>();
			}
        }

        private void OnEnable()
		{
			if(trigger == null)
			{
				this.enabled = false;
				return;
			}
			ArticyFlowController.NewChoices += ArticyFlowController_NewChoices;

#if ENABLE_INPUT_SYSTEM
            if (nextDialogLineAction != null) nextDialogLineAction.action.performed += NextDialogLine_performed;
#endif
        }

		private void OnDisable()
		{
			ArticyFlowController.NewChoices -= ArticyFlowController_NewChoices;

#if ENABLE_INPUT_SYSTEM
            if (nextDialogLineAction != null) nextDialogLineAction.action.performed -= NextDialogLine_performed;
#endif
        }

        protected void ArticyFlowController_NewChoices(IList<Branch> branches)
		{
			Debug.Log($"FullscreenNextButton:ShowChoices", this);
			if(branches != null && branches.Count == 1 && branches[0].IsValid)
			{
				branch = branches[0];
				amIActive = true;
			}
			else
			{
				branch = null;
				amIActive = false;
			}
			if(trigger.enabled != amIActive) trigger.enabled = amIActive;
		}

#if ENABLE_INPUT_SYSTEM
        public void NextDialogLine_performed(InputAction.CallbackContext obj)
        {
            Debug.Log($"Next Dialog Line Action Performed! Phase: {obj.phase}; Button Value: {obj.ReadValueAsButton().ToString()}");
			if (amIActive && delayedContinueCoroutine == null) delayedContinueCoroutine = StartCoroutine(DelayedContinue());
        }
#endif

        private void Update()
		{
#if ENABLE_LEGACY_INPUT_MANAGER && !ENABLE_INPUT_SYSTEM
			if (trigger.enabled && (Input.GetKeyDown(primaryKeybind) || Input.GetKeyDown(secondaryKeybind)))
                delayedContinueCoroutine = StartCoroutine(DelayedContinue());
#endif
		}

		public void OnBranchSelected()
		{
			if (!amIActive) return;
			// by giving the processor the branch assigned to the button on creation, the processor knows where to continue the flow
			if (delayedContinueCoroutine != null) StopCoroutine(delayedContinueCoroutine);
			if (myDialogCanvasManager != null && !myDialogCanvasManager.canContinue) return;
			if(ArticyFlowController.Instance != null) ArticyFlowController.Instance.PlayBranch(branch);
		}

		WaitForEndOfFrame wait;
		IEnumerator DelayedContinue()
		{
			yield return wait;
			delayedContinueCoroutine = null;
			OnBranchSelected();
		}
	}
}
