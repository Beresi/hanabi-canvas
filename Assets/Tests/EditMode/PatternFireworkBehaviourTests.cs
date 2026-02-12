// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using HanabiCanvas.Runtime;
using HanabiCanvas.Runtime.Firework;

namespace HanabiCanvas.Tests.EditMode
{
    public class PatternFireworkBehaviourTests
    {
        // ---- Private Fields ----
        private PatternFireworkBehaviourSO _behaviour;

        // Test config values (set in Setup)
        // convergeSpeed=10, holdDuration=1.0, fadeDelay=0.2, fadeDuration=0.5
        // patternScale=0.5, particleSize=0.2, driftSpeed=0.5, gravity=2.0

        // ---- Setup / Teardown ----
        [SetUp]
        public void Setup()
        {
            _behaviour = ScriptableObject.CreateInstance<PatternFireworkBehaviourSO>();

            SerializedObject behaviourSO = new SerializedObject(_behaviour);
            behaviourSO.FindProperty("_convergeSpeed").floatValue = 10.0f;
            behaviourSO.FindProperty("_holdDuration").floatValue = 1.0f;
            behaviourSO.FindProperty("_fadeDelay").floatValue = 0.2f;
            behaviourSO.FindProperty("_fadeDuration").floatValue = 0.5f;
            behaviourSO.FindProperty("_particleSize").floatValue = 0.2f;
            behaviourSO.FindProperty("_patternScale").floatValue = 0.5f;
            behaviourSO.FindProperty("_driftSpeed").floatValue = 0.5f;
            behaviourSO.FindProperty("_gravity").floatValue = 2.0f;
            behaviourSO.FindProperty("_formationCurve").animationCurveValue =
                AnimationCurve.Linear(0f, 0f, 1f, 1f);
            behaviourSO.FindProperty("_sizeOverLife").animationCurveValue =
                AnimationCurve.Constant(0f, 1f, 1f);
            behaviourSO.FindProperty("_alphaOverLife").animationCurveValue =
                AnimationCurve.Linear(0f, 1f, 1f, 0f);
            behaviourSO.ApplyModifiedPropertiesWithoutUndo();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_behaviour);
        }

        // ---- Helper Methods ----

        /// <summary>
        /// Creates a request with a single pixel at the given grid coords on a 32x32 grid.
        /// With patternScale=0.5:
        ///   worldOffset = (pixelX - 16) * 0.5, (pixelY - 16) * 0.5
        ///   distance = |worldOffset|
        /// </summary>
        private FireworkRequest CreateSinglePixelRequest(byte pixelX, byte pixelY,
            Vector3 origin = default)
        {
            if (origin == default)
            {
                origin = new Vector3(0f, 10f, 0f);
            }

            PixelEntry[] pattern = new[]
            {
                new PixelEntry(pixelX, pixelY, new Color32(255, 0, 0, 255))
            };

            return new FireworkRequest
            {
                Position = origin,
                Pattern = pattern,
                PatternWidth = 32,
                PatternHeight = 32
            };
        }

        /// <summary>
        /// Creates a request with two pixels at different distances.
        /// Pixel A at grid (16, 16) = center = distance 0.
        /// Pixel B at grid (16, 0) = worldY offset = (0-16)*0.5 = -8 → distance 8.
        /// With convergeSpeed=10: formationTime_A=0, formationTime_B=0.8.
        /// </summary>
        private FireworkRequest CreateTwoPixelRequest(Vector3 origin = default)
        {
            if (origin == default)
            {
                origin = new Vector3(0f, 10f, 0f);
            }

            PixelEntry[] pattern = new[]
            {
                new PixelEntry(16, 16, new Color32(255, 0, 0, 255)),  // center, dist=0
                new PixelEntry(16, 0, new Color32(0, 255, 0, 255))   // far, dist=8
            };

            return new FireworkRequest
            {
                Position = origin,
                Pattern = pattern,
                PatternWidth = 32,
                PatternHeight = 32
            };
        }

        /// <summary>
        /// Computes the expected totalLife for a request with known max distance.
        /// totalLife = maxDistance/convergeSpeed + holdDuration + fadeDelay + fadeDuration
        /// </summary>
        private float ComputeExpectedTotalLife(float maxDistance)
        {
            // convergeSpeed=10, holdDuration=1.0, fadeDelay=0.2, fadeDuration=0.5
            return maxDistance / 10.0f + 1.0f + 0.2f + 0.5f;
        }

        // ---- GetParticleCount Tests ----
        [Test]
        public void GetParticleCount_ReturnsPatternLength()
        {
            FireworkRequest request = CreateSinglePixelRequest(0, 0);

            Assert.AreEqual(1, _behaviour.GetParticleCount(request));
        }

        [Test]
        public void GetParticleCount_NullPattern_ReturnsZero()
        {
            FireworkRequest request = new FireworkRequest
            {
                Position = Vector3.zero,
                Pattern = null
            };

            Assert.AreEqual(0, _behaviour.GetParticleCount(request));
        }

        // ---- InitializeParticles Tests ----
        [Test]
        public void InitializeParticles_SetsStartPositionToOrigin()
        {
            FireworkRequest request = CreateTwoPixelRequest();
            FireworkParticle[] particles = new FireworkParticle[2];

            _behaviour.InitializeParticles(particles, 2, request);

            for (int i = 0; i < 2; i++)
            {
                Assert.AreEqual(request.Position, particles[i].StartPosition,
                    $"Particle {i} start position should be request origin");
                Assert.AreEqual(request.Position, particles[i].Position,
                    $"Particle {i} initial position should be request origin");
            }
        }

        [Test]
        public void InitializeParticles_SetsTargetFromGridCoordinates()
        {
            // Single pixel at grid position (16, 16) on a 32x32 grid
            // halfWidth = 16.0, offset = (16 - 16) * 0.5 = 0
            FireworkRequest request = CreateSinglePixelRequest(16, 16);
            FireworkParticle[] particles = new FireworkParticle[1];

            _behaviour.InitializeParticles(particles, 1, request);

            // Pixel at grid center: zero offset from origin
            Assert.AreEqual(0f, particles[0].TargetPosition.x, 0.01f,
                "Pixel at grid center should have zero X offset");
            Assert.AreEqual(10f, particles[0].TargetPosition.y, 0.01f,
                "Pixel at grid center should have zero Y offset from origin");
            Assert.AreEqual(0f, particles[0].TargetPosition.z, 0.01f,
                "Target Z should be zero offset");
        }

        [Test]
        public void InitializeParticles_SetsColorFromPixelEntry()
        {
            PixelEntry[] pattern = new[]
            {
                new PixelEntry(0, 0, new Color32(128, 64, 32, 255))
            };
            FireworkRequest request = new FireworkRequest
            {
                Position = Vector3.zero,
                Pattern = pattern,
                PatternWidth = 32,
                PatternHeight = 32
            };

            FireworkParticle[] particles = new FireworkParticle[1];
            _behaviour.InitializeParticles(particles, 1, request);

            Assert.AreEqual(128f / 255f, particles[0].Color.r, 0.01f);
            Assert.AreEqual(64f / 255f, particles[0].Color.g, 0.01f);
            Assert.AreEqual(32f / 255f, particles[0].Color.b, 0.01f);
            Assert.AreEqual(1f, particles[0].Color.a, 0.01f);
        }

        [Test]
        public void InitializeParticles_SetsCorrectLifetime()
        {
            // Two-pixel request: center (dist=0) and far (dist=8)
            // maxFormationTime = 8 / 10 = 0.8
            // totalLife = 0.8 + 1.0 + 0.2 + 0.5 = 2.5
            FireworkRequest request = CreateTwoPixelRequest();
            FireworkParticle[] particles = new FireworkParticle[2];

            _behaviour.InitializeParticles(particles, 2, request);

            float expectedLife = ComputeExpectedTotalLife(8.0f); // 2.5
            Assert.AreEqual(expectedLife, particles[0].Life, 0.01f,
                "Life should equal maxFormation + hold + fadeDelay + fade");
            Assert.AreEqual(expectedLife, particles[0].MaxLife, 0.01f,
                "MaxLife should equal totalLife");
            Assert.AreEqual(expectedLife, particles[1].Life, 0.01f,
                "All particles should share the same totalLife");
        }

        [Test]
        public void InitializeParticles_SetsVelocityToNormalizedDirection()
        {
            // Pixel at grid (16, 0): worldY = (0-16)*0.5 = -8, target = (0, 10-8, 0) = (0, 2, 0)
            // Direction from origin (0,10,0) to target (0,2,0) = (0,-8,0) normalized = (0,-1,0)
            FireworkRequest request = CreateSinglePixelRequest(16, 0);
            FireworkParticle[] particles = new FireworkParticle[1];

            _behaviour.InitializeParticles(particles, 1, request);

            Assert.AreEqual(0f, particles[0].Velocity.x, 0.01f,
                "Velocity X should be 0 for vertical offset");
            Assert.AreEqual(-1f, particles[0].Velocity.y, 0.01f,
                "Velocity Y should be -1 (normalized downward direction)");
            Assert.AreEqual(0f, particles[0].Velocity.z, 0.01f,
                "Velocity Z should be 0");
        }

        [Test]
        public void InitializeParticles_ZeroDistancePixel_SetsZeroVelocity()
        {
            // Pixel at grid center (16, 16) → distance = 0 → direction = zero vector
            FireworkRequest request = CreateSinglePixelRequest(16, 16);
            FireworkParticle[] particles = new FireworkParticle[1];

            _behaviour.InitializeParticles(particles, 1, request);

            Assert.AreEqual(Vector3.zero, particles[0].Velocity,
                "Zero-distance pixel should have zero velocity (direction)");
        }

        // ---- UpdateParticles: Converge Phase Tests ----
        [Test]
        public void UpdateParticles_ConvergePhase_MovesTowardTarget()
        {
            // Pixel at (16, 0): dist=8, formationTime=0.8
            // At elapsed=0.4 (halfway), with linear curve, position should be halfway
            FireworkRequest request = CreateSinglePixelRequest(16, 0);
            FireworkParticle[] particles = new FireworkParticle[1];
            _behaviour.InitializeParticles(particles, 1, request);

            Vector3 start = particles[0].StartPosition;
            Vector3 target = particles[0].TargetPosition;

            _behaviour.UpdateParticles(particles, 1, 0.4f);

            // Linear curve: at t=0.5 (0.4/0.8), position should be halfway
            Vector3 expected = Vector3.Lerp(start, target, 0.5f);
            Assert.AreEqual(expected.x, particles[0].Position.x, 0.05f,
                "Halfway through formation, X should be at midpoint");
            Assert.AreEqual(expected.y, particles[0].Position.y, 0.05f,
                "Halfway through formation, Y should be at midpoint");
        }

        [Test]
        public void UpdateParticles_CloserPixelArrivesBeforeFarPixel()
        {
            // Pixel 0 at center (dist=0, formationTime=0)
            // Pixel 1 at (16, 0) (dist=8, formationTime=0.8)
            // At elapsed=0.4: pixel 0 should be at target (holding), pixel 1 still converging
            FireworkRequest request = CreateTwoPixelRequest();
            FireworkParticle[] particles = new FireworkParticle[2];
            _behaviour.InitializeParticles(particles, 2, request);

            Vector3 target0 = particles[0].TargetPosition;
            Vector3 target1 = particles[1].TargetPosition;

            _behaviour.UpdateParticles(particles, 2, 0.4f);

            // Pixel 0 (center): already arrived, at target
            Assert.AreEqual(target0.x, particles[0].Position.x, 0.01f,
                "Center pixel should already be at target");
            Assert.AreEqual(target0.y, particles[0].Position.y, 0.01f,
                "Center pixel should already be at target");

            // Pixel 1 (far): still in transit, NOT at target
            float distToTarget = Vector3.Distance(particles[1].Position, target1);
            Assert.Greater(distToTarget, 0.1f,
                "Far pixel should NOT have arrived yet at elapsed=0.4");
        }

        [Test]
        public void UpdateParticles_FormationComplete_ArrivesAtTarget()
        {
            // Pixel at (16, 0): dist=8, formationTime=0.8
            // At elapsed=0.81 (just past formation), should be at target (hold)
            FireworkRequest request = CreateSinglePixelRequest(16, 0);
            FireworkParticle[] particles = new FireworkParticle[1];
            _behaviour.InitializeParticles(particles, 1, request);

            Vector3 target = particles[0].TargetPosition;

            _behaviour.UpdateParticles(particles, 1, 0.81f);

            Assert.AreEqual(target.x, particles[0].Position.x, 0.01f,
                "After formation, position should be at target X");
            Assert.AreEqual(target.y, particles[0].Position.y, 0.01f,
                "After formation, position should be at target Y");
        }

        // ---- UpdateParticles: Hold Phase Tests ----
        [Test]
        public void UpdateParticles_HoldPhase_StaysAtTarget()
        {
            // Two-pixel request: maxFormationTime=0.8, holdEnd=0.8+1.0=1.8
            // At elapsed=1.4 (well into hold), particle should be at target
            FireworkRequest request = CreateTwoPixelRequest();
            FireworkParticle[] particles = new FireworkParticle[2];
            _behaviour.InitializeParticles(particles, 2, request);

            Vector3 target1 = particles[1].TargetPosition;

            _behaviour.UpdateParticles(particles, 2, 1.4f);

            Assert.AreEqual(target1.x, particles[1].Position.x, 0.01f,
                "During hold, position should be at target");
            Assert.AreEqual(target1.y, particles[1].Position.y, 0.01f,
                "During hold, position should be at target");
        }

        [Test]
        public void UpdateParticles_BeforeFadeStart_AlphaIsFull()
        {
            // maxFormationTime=0.8, holdEnd=1.8, fadeStart=2.0
            // At elapsed=1.4 (hold phase): alpha should be 1.0
            FireworkRequest request = CreateTwoPixelRequest();
            FireworkParticle[] particles = new FireworkParticle[2];
            _behaviour.InitializeParticles(particles, 2, request);

            _behaviour.UpdateParticles(particles, 2, 1.4f);

            Assert.AreEqual(1f, particles[0].Color.a, 0.01f,
                "Alpha should be 1.0 before fade starts");
            Assert.AreEqual(1f, particles[1].Color.a, 0.01f,
                "Alpha should be 1.0 before fade starts");
        }

        // ---- UpdateParticles: Drift Phase Tests ----
        [Test]
        public void UpdateParticles_DriftPhase_MovesAlongApproachDirection()
        {
            // Two-pixel request: maxFormation=0.8, holdEnd=1.8
            // At elapsed=2.0 (0.2s into drift):
            // Pixel 1 direction=(0,-1,0), driftSpeed=0.5
            // driftOffset = (0,-1,0) * 0.5 * 0.2 = (0,-0.1,0)
            // gravityOffset = down * 0.5 * 2.0 * 0.04 = (0, -0.04, 0)
            // position = target + (0, -0.14, 0)
            FireworkRequest request = CreateTwoPixelRequest();
            FireworkParticle[] particles = new FireworkParticle[2];
            _behaviour.InitializeParticles(particles, 2, request);

            Vector3 target1 = particles[1].TargetPosition;

            _behaviour.UpdateParticles(particles, 2, 2.0f);

            // Position should have moved away from target
            float distFromTarget = Vector3.Distance(particles[1].Position, target1);
            Assert.Greater(distFromTarget, 0.01f,
                "During drift, particle should have moved away from target");

            // Drift direction for pixel 1 is (0, -1, 0), so X should stay same
            Assert.AreEqual(target1.x, particles[1].Position.x, 0.01f,
                "Drift in Y direction should not affect X");
        }

        [Test]
        public void UpdateParticles_DriftPhase_AppliesGravity()
        {
            // Center pixel (dist=0, direction=zero): drift is purely gravity
            // maxFormation=0.8, holdEnd=1.8
            // At elapsed=2.3 (0.5s into drift):
            // driftOffset = zero (direction=zero)
            // gravityOffset = down * 0.5 * 2.0 * 0.25 = (0, -0.25, 0)
            FireworkRequest request = CreateTwoPixelRequest();
            FireworkParticle[] particles = new FireworkParticle[2];
            _behaviour.InitializeParticles(particles, 2, request);

            Vector3 target0 = particles[0].TargetPosition;

            _behaviour.UpdateParticles(particles, 2, 2.3f);

            // Center pixel should have fallen below target due to gravity
            Assert.Less(particles[0].Position.y, target0.y,
                "Gravity should pull particle below target during drift");

            float driftElapsed = 0.5f;
            float expectedGravityDrop = 0.5f * 2.0f * driftElapsed * driftElapsed; // 0.25
            float expectedY = target0.y - expectedGravityDrop;
            Assert.AreEqual(expectedY, particles[0].Position.y, 0.05f,
                "Gravity drop should match analytical formula");
        }

        // ---- UpdateParticles: Fade Tests ----
        [Test]
        public void UpdateParticles_FadePhase_ReducesAlpha()
        {
            // maxFormation=0.8, holdEnd=1.8, fadeStart=2.0
            // At elapsed=2.25 (0.25s into fade = halfway through 0.5s fade):
            // fadeProgress=0.5, alphaOverLife linear 1→0 → alpha=0.5
            FireworkRequest request = CreateTwoPixelRequest();
            FireworkParticle[] particles = new FireworkParticle[2];
            _behaviour.InitializeParticles(particles, 2, request);

            _behaviour.UpdateParticles(particles, 2, 2.25f);

            Assert.Less(particles[0].Color.a, 1f,
                "Alpha should be less than 1 during fade phase");
            Assert.Greater(particles[0].Color.a, 0f,
                "Alpha should be greater than 0 halfway through fade");
            Assert.AreEqual(0.5f, particles[0].Color.a, 0.05f,
                "With linear alphaOverLife, alpha should be ~0.5 at fade midpoint");
        }

        [Test]
        public void UpdateParticles_FadeConcurrentWithDrift()
        {
            // At elapsed=2.25: drift is active AND fade is active simultaneously
            FireworkRequest request = CreateTwoPixelRequest();
            FireworkParticle[] particles = new FireworkParticle[2];
            _behaviour.InitializeParticles(particles, 2, request);

            Vector3 target1 = particles[1].TargetPosition;

            _behaviour.UpdateParticles(particles, 2, 2.25f);

            // Verify BOTH drift and fade are happening
            float distFromTarget = Vector3.Distance(particles[1].Position, target1);
            Assert.Greater(distFromTarget, 0.01f,
                "Particle should have drifted from target");
            Assert.Less(particles[1].Color.a, 1f,
                "Alpha should be fading concurrently with drift");
        }

        // ---- UpdateParticles: Edge Cases ----
        [Test]
        public void UpdateParticles_DeadParticle_IsSkipped()
        {
            FireworkParticle[] particles = new FireworkParticle[1];
            particles[0] = new FireworkParticle
            {
                Position = new Vector3(5f, 5f, 5f),
                Life = 0f,
                MaxLife = 2f
            };

            _behaviour.UpdateParticles(particles, 1, 0.1f);

            Assert.AreEqual(new Vector3(5f, 5f, 5f), particles[0].Position,
                "Dead particle should not be moved");
        }

        // ---- IsComplete Tests ----
        [Test]
        public void IsComplete_AllDead_ReturnsTrue()
        {
            FireworkParticle[] particles = new FireworkParticle[3];

            Assert.IsTrue(_behaviour.IsComplete(particles, 3));
        }

        [Test]
        public void IsComplete_SomeAlive_ReturnsFalse()
        {
            FireworkParticle[] particles = new FireworkParticle[3];
            particles[1].Life = 0.5f;

            Assert.IsFalse(_behaviour.IsComplete(particles, 3));
        }

        // ---- OnValidate Tests ----
        [Test]
        public void OnValidate_ConvergeSpeedBelowMin_ClampedToMin()
        {
            PatternFireworkBehaviourSO testBehaviour =
                ScriptableObject.CreateInstance<PatternFireworkBehaviourSO>();

            SerializedObject so = new SerializedObject(testBehaviour);
            so.FindProperty("_convergeSpeed").floatValue = 0f;
            so.ApplyModifiedPropertiesWithoutUndo();

            Assert.AreEqual(0.1f, testBehaviour.ConvergeSpeed, 0.001f);

            Object.DestroyImmediate(testBehaviour);
        }

        [Test]
        public void OnValidate_FadeDelayBelowMin_ClampedToMin()
        {
            PatternFireworkBehaviourSO testBehaviour =
                ScriptableObject.CreateInstance<PatternFireworkBehaviourSO>();

            SerializedObject so = new SerializedObject(testBehaviour);
            so.FindProperty("_fadeDelay").floatValue = -1f;
            so.ApplyModifiedPropertiesWithoutUndo();

            Assert.AreEqual(0f, testBehaviour.FadeDelay, 0.001f);

            Object.DestroyImmediate(testBehaviour);
        }

        [Test]
        public void OnValidate_PatternScaleBelowMin_ClampedToMin()
        {
            PatternFireworkBehaviourSO testBehaviour =
                ScriptableObject.CreateInstance<PatternFireworkBehaviourSO>();

            SerializedObject so = new SerializedObject(testBehaviour);
            so.FindProperty("_patternScale").floatValue = -1f;
            so.ApplyModifiedPropertiesWithoutUndo();

            Assert.AreEqual(0.01f, testBehaviour.PatternScale, 0.001f);

            Object.DestroyImmediate(testBehaviour);
        }
    }
}
