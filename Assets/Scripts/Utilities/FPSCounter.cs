using TMPro;
using UnityEngine;

namespace SpecialAssignment
{
    public class FPSCounter : MonoBehaviour
    {
        public TextMeshProUGUI text;
        public float updateRate = 0.2f;
        
        private int framesRecorded = 0;
        private float time = 0;
        
        private void Update()
        {
            time += Time.deltaTime;
            framesRecorded++;
            if (time > updateRate)
            {
                var fps = framesRecorded / time;
                text.SetText($"FPS: {fps:F0}");
                time = 0;
                framesRecorded = 0;
            }
        }
    }
}
