using UnityEngine;

// One reusable class for both rain and snow: set activeState in the Inspector.
// Wind speed from the API pushes the particles sideways.
public class PrecipitationEffect : MonoBehaviour, IWeatherEffect
{
    [SerializeField] ParticleSystem particles;
    [SerializeField] WeatherState activeState = WeatherState.Rain;
    [SerializeField] float emissionRate = 800f;
    [SerializeField] float windInfluence = 0.5f;
    [SerializeField] float maxWind = 15f;
    [SerializeField] Color dayColor = new Color(0.8f, 0.85f, 0.95f, 0.6f);
    [SerializeField] Color nightColor = new Color(0.45f, 0.5f, 0.65f, 0.5f);

    public void Apply(WeatherReport report, bool instant)
    {
        if (report.State != activeState)
        {
            // StopEmitting lets drops already in the air finish falling, so it fades out naturally.
            particles.Stop(true, instant
                ? ParticleSystemStopBehavior.StopEmittingAndClear
                : ParticleSystemStopBehavior.StopEmitting);
            return;
        }

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = emissionRate;

        ParticleSystem.MainModule main = particles.main;
        main.startColor = report.IsDay != false ? dayColor : nightColor;

        float drift = Mathf.Min(report.WindSpeed ?? 0f, maxWind) * windInfluence;
        ParticleSystem.VelocityOverLifetimeModule velocity = particles.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.World;
        velocity.x = drift;
        velocity.y = 0f;
        velocity.z = 0f;

        if (!Application.isPlaying)
            particles.Simulate(4f, true, true);   // editor preview/screenshots
        else if (!particles.isPlaying)
            particles.Play();
    }
}
