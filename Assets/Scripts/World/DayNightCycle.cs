using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace World
{
    public class DayNightCycle : MonoBehaviour
    {
        [SerializeField] private Light2D globalLight;
        [SerializeField] private Gradient dayNightGradient;
        [SerializeField] private float dayNightDuration;

        private float time;
        public float CurrentTime => time;
        public bool IsNight => time < 0.2f || time > 0.8f;

        void Update()
        {
            time += Time.deltaTime / dayNightDuration;
            if (time > 1) time = 0;
            globalLight.color = dayNightGradient.Evaluate(time);
        }
    }
}
