// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;

namespace HanabiCanvas.Runtime.Firework
{
    public static class FireworkUpdater
    {
        // ---- Public Methods ----
        public static void UpdateBurst(ParticleData[] particles, int count, float deltaTime,
            float burstDrag, float steerStrength, AnimationCurve steerCurve, float phaseProgress,
            float debrisGravity, float debrisSizeFade)
        {
            float steerInfluence = steerCurve.Evaluate(phaseProgress);

            for (int i = 0; i < count; i++)
            {
                if (particles[i].IsPattern)
                {
                    Vector3 toTarget = particles[i].FormationTarget - particles[i].Position;
                    Vector3 desiredVelocity = toTarget * steerStrength;
                    particles[i].Velocity = Vector3.Lerp(
                        particles[i].Velocity, desiredVelocity, steerInfluence);
                }
                else
                {
                    particles[i].Velocity *= burstDrag;
                    particles[i].Velocity += Vector3.down * debrisGravity * deltaTime;
                    particles[i].Life -= deltaTime;
                    particles[i].Size *= 1f - debrisSizeFade * deltaTime;
                    if (particles[i].Size < 0f)
                    {
                        particles[i].Size = 0f;
                    }
                }

                particles[i].Position += particles[i].Velocity * deltaTime;
            }
        }

        public static void UpdateSteer(ParticleData[] particles, int count, float deltaTime,
            float steerStrength, AnimationCurve steerCurve, float phaseProgress,
            float debrisDrag, float debrisGravity, float debrisSizeFade)
        {
            float steerInfluence = steerCurve.Evaluate(phaseProgress);

            for (int i = 0; i < count; i++)
            {
                if (particles[i].IsPattern)
                {
                    Vector3 toTarget = particles[i].FormationTarget - particles[i].Position;
                    Vector3 desiredVelocity = toTarget * steerStrength;
                    particles[i].Velocity = Vector3.Lerp(
                        particles[i].Velocity, desiredVelocity, steerInfluence);
                }
                else
                {
                    particles[i].Velocity *= debrisDrag;
                    particles[i].Velocity += Vector3.down * debrisGravity * deltaTime;
                    particles[i].Life -= deltaTime;
                    particles[i].Size *= 1f - debrisSizeFade * deltaTime;
                    if (particles[i].Size < 0f)
                    {
                        particles[i].Size = 0f;
                    }
                }

                particles[i].Position += particles[i].Velocity * deltaTime;
            }
        }

        public static void UpdateHold(ParticleData[] particles, int count, float deltaTime,
            float sparkleIntensity, float jitterScale, float time,
            float debrisGravity, float debrisSizeFade)
        {
            for (int i = 0; i < count; i++)
            {
                if (particles[i].IsPattern)
                {
                    float jitterX = Mathf.Sin(time * 17.3f + i * 3.7f) * jitterScale * sparkleIntensity;
                    float jitterY = Mathf.Cos(time * 13.1f + i * 5.3f) * jitterScale * sparkleIntensity;
                    float jitterZ = Mathf.Sin(time * 11.7f + i * 7.1f) * jitterScale * sparkleIntensity * 0.5f;

                    particles[i].Position = particles[i].FormationTarget +
                        new Vector3(jitterX, jitterY, jitterZ);
                    particles[i].Velocity = Vector3.zero;
                }
                else
                {
                    particles[i].Velocity += Vector3.down * debrisGravity * deltaTime;
                    particles[i].Position += particles[i].Velocity * deltaTime;
                    particles[i].Life -= deltaTime;
                    particles[i].Size *= 1f - debrisSizeFade * deltaTime;
                    if (particles[i].Size < 0f)
                    {
                        particles[i].Size = 0f;
                    }
                }
            }
        }

        public static void UpdateFade(ParticleData[] particles, int count, float deltaTime,
            float fadeGravity, float sizeFadeMultiplier, float phaseDuration)
        {
            float lifeDrain = phaseDuration > 0f ? deltaTime / phaseDuration : 1f;

            for (int i = 0; i < count; i++)
            {
                particles[i].Velocity += Vector3.down * fadeGravity * deltaTime;
                particles[i].Position += particles[i].Velocity * deltaTime;

                particles[i].Size *= 1f - sizeFadeMultiplier * deltaTime;
                if (particles[i].Size < 0f)
                {
                    particles[i].Size = 0f;
                }

                particles[i].Life -= lifeDrain;

                Color color = particles[i].Color;
                color.a = Mathf.Max(0f, color.a - lifeDrain);
                particles[i].Color = color;
            }
        }
    }
}
