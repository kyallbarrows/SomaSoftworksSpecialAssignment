using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AltEnding.Analytics;

public class DataOptInRelay : MonoBehaviour
{
    public void CallForOptInModal()
    {
        if (!AnalyticsManager.instance_Loaded)
        {
            AnalyticsManager.WhenLoaded(CallForOptInModal);
            return;
        }
        AnalyticsManager.instance.ShowFirstOptInModal();
    }
}
