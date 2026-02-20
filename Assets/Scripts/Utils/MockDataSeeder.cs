// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using System.Collections.Generic;
using UnityEngine;
using HanabiCanvas.Runtime.Persistence;

namespace HanabiCanvas.Runtime.Utils
{
    /// <summary>
    /// Development-only MonoBehaviour that populates <see cref="DataManager"/>
    /// with mock artworks and challenge requests on Start. Attach to any
    /// GameObject in the scene alongside a reference to DataManager.
    /// Destroys itself after seeding so it only runs once per play session.
    /// </summary>
    public class MockDataSeeder : MonoBehaviour
    {
        // ---- Constants ----
        private const int GRID_SIZE = 32;

        // ---- Serialized Fields ----

        [Header("References")]
        [Tooltip("DataManager to populate with mock data")]
        [SerializeField] private DataManager _dataManager;

        [Header("Options")]
        [Tooltip("Number of mock artworks to create")]
        [SerializeField] private int _artworkCount = 5;

        [Tooltip("Number of mock challenge requests to create")]
        [SerializeField] private int _requestCount = 5;

        [Tooltip("Destroy this component after seeding")]
        [SerializeField] private bool _destroyAfterSeed = true;

        // ---- Palette (8 vibrant colors) ----
        private static readonly Color32 COLOR_RED = new Color32(255, 59, 48, 255);
        private static readonly Color32 COLOR_ORANGE = new Color32(255, 149, 0, 255);
        private static readonly Color32 COLOR_YELLOW = new Color32(255, 230, 32, 255);
        private static readonly Color32 COLOR_GREEN = new Color32(52, 199, 89, 255);
        private static readonly Color32 COLOR_CYAN = new Color32(50, 173, 230, 255);
        private static readonly Color32 COLOR_BLUE = new Color32(0, 122, 255, 255);
        private static readonly Color32 COLOR_PURPLE = new Color32(175, 82, 222, 255);
        private static readonly Color32 COLOR_PINK = new Color32(255, 45, 85, 255);

        // ---- Unity Methods ----

        private void Start()
        {
            if (_dataManager == null)
            {
                Debug.LogWarning("[MockDataSeeder] DataManager not assigned. Skipping.", this);
                return;
            }

            SeedArtworks();
            SeedRequests();

            Debug.Log("[MockDataSeeder] Seeded " + _artworkCount + " artworks and "
                + _requestCount + " requests.");

            if (_destroyAfterSeed)
            {
                Destroy(this);
            }
        }

        // ================================================================
        //  ARTWORK SEEDING
        // ================================================================

        private void SeedArtworks()
        {
            List<ArtworkData> artworks = new List<ArtworkData>(_artworkCount);

            if (_artworkCount > 0)
            {
                artworks.Add(CreateArtwork("Heart", BuildHeart()));
            }

            if (_artworkCount > 1)
            {
                artworks.Add(CreateArtwork("Smiley Face", BuildSmiley()));
            }

            if (_artworkCount > 2)
            {
                artworks.Add(CreateArtwork("House", BuildHouse()));
            }

            if (_artworkCount > 3)
            {
                artworks.Add(CreateArtwork("Star", BuildStar()));
            }

            if (_artworkCount > 4)
            {
                artworks.Add(CreateArtwork("Rainbow", BuildRainbow()));
            }

            // Fill remaining with random splatter
            for (int i = artworks.Count; i < _artworkCount; i++)
            {
                artworks.Add(CreateArtwork("Abstract " + (i + 1), BuildRandomSplatter(i)));
            }

            _dataManager.SetAllArtworks(artworks);
        }

        private ArtworkData CreateArtwork(string name, PixelEntry[] pixels)
        {
            long timestamp = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                - Random.Range(0, 86400 * 7);

            return new ArtworkData(
                System.Guid.NewGuid().ToString(),
                name,
                pixels,
                GRID_SIZE,
                GRID_SIZE,
                timestamp);
        }

        // ---- Heart Pattern ----

        private PixelEntry[] BuildHeart()
        {
            List<PixelEntry> pixels = new List<PixelEntry>(128);

            // Heart shape built from two filled circles + triangle
            // Left circle center (12, 22), right circle center (20, 22), radius 5
            // Bottom point at (16, 10)
            for (int y = 0; y < GRID_SIZE; y++)
            {
                for (int x = 0; x < GRID_SIZE; x++)
                {
                    bool isInHeart = false;

                    // Left circle
                    int dx1 = x - 12;
                    int dy1 = y - 22;
                    if (dx1 * dx1 + dy1 * dy1 <= 25)
                    {
                        isInHeart = true;
                    }

                    // Right circle
                    int dx2 = x - 20;
                    int dy2 = y - 22;
                    if (dx2 * dx2 + dy2 * dy2 <= 25)
                    {
                        isInHeart = true;
                    }

                    // Triangle: from x=8..24 down to point (16, 10)
                    if (y >= 10 && y <= 22)
                    {
                        float t = (float)(y - 10) / 12f;
                        int halfWidth = Mathf.RoundToInt(t * 8f);
                        if (x >= 16 - halfWidth && x <= 16 + halfWidth)
                        {
                            isInHeart = true;
                        }
                    }

                    if (isInHeart)
                    {
                        // Gradient from pink center to red edge
                        float dist = Mathf.Sqrt((x - 16f) * (x - 16f) + (y - 18f) * (y - 18f));
                        Color32 color = dist < 5f ? COLOR_PINK : COLOR_RED;
                        pixels.Add(new PixelEntry((byte)x, (byte)y, color));
                    }
                }
            }

            return pixels.ToArray();
        }

        // ---- Smiley Face Pattern ----

        private PixelEntry[] BuildSmiley()
        {
            List<PixelEntry> pixels = new List<PixelEntry>(256);
            int cx = 16;
            int cy = 16;

            for (int y = 0; y < GRID_SIZE; y++)
            {
                for (int x = 0; x < GRID_SIZE; x++)
                {
                    int dx = x - cx;
                    int dy = y - cy;
                    int distSq = dx * dx + dy * dy;

                    // Face circle (filled, radius 12)
                    if (distSq <= 144)
                    {
                        pixels.Add(new PixelEntry((byte)x, (byte)y, COLOR_YELLOW));
                    }

                    // Left eye (radius 2, center at 12, 20)
                    int exL = x - 12;
                    int eyL = y - 20;
                    if (exL * exL + eyL * eyL <= 4)
                    {
                        pixels.Add(new PixelEntry((byte)x, (byte)y, COLOR_BLUE));
                    }

                    // Right eye (radius 2, center at 20, 20)
                    int exR = x - 20;
                    int eyR = y - 20;
                    if (exR * exR + eyR * eyR <= 4)
                    {
                        pixels.Add(new PixelEntry((byte)x, (byte)y, COLOR_BLUE));
                    }

                    // Mouth arc (y=12, from x=11 to x=21, parabolic curve)
                    if (x >= 11 && x <= 21)
                    {
                        float mx = (x - 16f) / 5f;
                        int mouthY = 12 - Mathf.RoundToInt(mx * mx * 2f);
                        if (y == mouthY || y == mouthY + 1)
                        {
                            pixels.Add(new PixelEntry((byte)x, (byte)y, COLOR_RED));
                        }
                    }
                }
            }

            return pixels.ToArray();
        }

        // ---- House Pattern ----

        private PixelEntry[] BuildHouse()
        {
            List<PixelEntry> pixels = new List<PixelEntry>(256);

            // Walls: rectangle from (8,4) to (24,16)
            for (int y = 4; y <= 16; y++)
            {
                for (int x = 8; x <= 24; x++)
                {
                    pixels.Add(new PixelEntry((byte)x, (byte)y, COLOR_ORANGE));
                }
            }

            // Roof: triangle from (6,16) to (26,16) up to (16,26)
            for (int y = 16; y <= 26; y++)
            {
                float t = (float)(y - 16) / 10f;
                int halfWidth = Mathf.RoundToInt((1f - t) * 10f);
                for (int x = 16 - halfWidth; x <= 16 + halfWidth; x++)
                {
                    if (x >= 0 && x < GRID_SIZE)
                    {
                        pixels.Add(new PixelEntry((byte)x, (byte)y, COLOR_RED));
                    }
                }
            }

            // Door: rectangle from (14,4) to (18,10)
            for (int y = 4; y <= 10; y++)
            {
                for (int x = 14; x <= 18; x++)
                {
                    pixels.Add(new PixelEntry((byte)x, (byte)y, COLOR_PURPLE));
                }
            }

            // Window left: (10,11) to (13,14)
            for (int y = 11; y <= 14; y++)
            {
                for (int x = 10; x <= 13; x++)
                {
                    pixels.Add(new PixelEntry((byte)x, (byte)y, COLOR_CYAN));
                }
            }

            // Window right: (19,11) to (22,14)
            for (int y = 11; y <= 14; y++)
            {
                for (int x = 19; x <= 22; x++)
                {
                    pixels.Add(new PixelEntry((byte)x, (byte)y, COLOR_CYAN));
                }
            }

            // Ground: green strip y=2..3
            for (int y = 2; y <= 3; y++)
            {
                for (int x = 0; x < GRID_SIZE; x++)
                {
                    pixels.Add(new PixelEntry((byte)x, (byte)y, COLOR_GREEN));
                }
            }

            return pixels.ToArray();
        }

        // ---- Star Pattern ----

        private PixelEntry[] BuildStar()
        {
            List<PixelEntry> pixels = new List<PixelEntry>(128);
            int cx = 16;
            int cy = 16;

            for (int y = 0; y < GRID_SIZE; y++)
            {
                for (int x = 0; x < GRID_SIZE; x++)
                {
                    float fx = x - cx;
                    float fy = y - cy;
                    float angle = Mathf.Atan2(fy, fx);
                    float dist = Mathf.Sqrt(fx * fx + fy * fy);

                    // 5-pointed star: alternating inner/outer radius
                    float starAngle = angle + Mathf.PI * 0.5f;
                    if (starAngle < 0f)
                    {
                        starAngle += Mathf.PI * 2f;
                    }

                    float sector = starAngle / (Mathf.PI * 2f) * 10f;
                    float frac = sector - Mathf.Floor(sector);
                    float outerRadius = 13f;
                    float innerRadius = 5.5f;
                    float targetRadius;

                    if (frac < 0.5f)
                    {
                        targetRadius = Mathf.Lerp(outerRadius, innerRadius, frac * 2f);
                    }
                    else
                    {
                        targetRadius = Mathf.Lerp(innerRadius, outerRadius, (frac - 0.5f) * 2f);
                    }

                    if (dist <= targetRadius)
                    {
                        Color32 color = dist < 6f ? COLOR_YELLOW : COLOR_ORANGE;
                        pixels.Add(new PixelEntry((byte)x, (byte)y, color));
                    }
                }
            }

            return pixels.ToArray();
        }

        // ---- Rainbow Pattern ----

        private PixelEntry[] BuildRainbow()
        {
            List<PixelEntry> pixels = new List<PixelEntry>(256);
            int cx = 16;
            int cy = 8;

            Color32[] bands = new Color32[]
            {
                COLOR_RED,
                COLOR_ORANGE,
                COLOR_YELLOW,
                COLOR_GREEN,
                COLOR_CYAN,
                COLOR_BLUE,
                COLOR_PURPLE
            };

            for (int y = 8; y < GRID_SIZE; y++)
            {
                for (int x = 0; x < GRID_SIZE; x++)
                {
                    float dx = x - cx;
                    float dy = y - cy;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    // Bands from radius 8 to 22, each band is 2px wide
                    float bandStart = 8f;
                    float bandWidth = 2f;

                    for (int b = 0; b < bands.Length; b++)
                    {
                        float rInner = bandStart + b * bandWidth;
                        float rOuter = rInner + bandWidth;
                        if (dist >= rInner && dist < rOuter)
                        {
                            pixels.Add(new PixelEntry((byte)x, (byte)y, bands[b]));
                            break;
                        }
                    }
                }
            }

            return pixels.ToArray();
        }

        // ---- Random Splatter Pattern ----

        private PixelEntry[] BuildRandomSplatter(int seed)
        {
            Random.State oldState = Random.state;
            Random.InitState(seed * 12345 + 42);

            Color32[] palette = new Color32[]
            {
                COLOR_RED, COLOR_ORANGE, COLOR_YELLOW, COLOR_GREEN,
                COLOR_CYAN, COLOR_BLUE, COLOR_PURPLE, COLOR_PINK
            };

            int count = Random.Range(40, 120);
            List<PixelEntry> pixels = new List<PixelEntry>(count);

            // Place a few random blobs
            int blobCount = Random.Range(3, 7);
            for (int b = 0; b < blobCount; b++)
            {
                int bx = Random.Range(4, 28);
                int by = Random.Range(4, 28);
                int radius = Random.Range(2, 6);
                Color32 color = palette[Random.Range(0, palette.Length)];

                for (int y = by - radius; y <= by + radius; y++)
                {
                    for (int x = bx - radius; x <= bx + radius; x++)
                    {
                        if (x < 0 || x >= GRID_SIZE || y < 0 || y >= GRID_SIZE)
                        {
                            continue;
                        }

                        int dx = x - bx;
                        int dy = y - by;
                        if (dx * dx + dy * dy <= radius * radius)
                        {
                            pixels.Add(new PixelEntry((byte)x, (byte)y, color));
                        }
                    }
                }
            }

            Random.state = oldState;
            return pixels.ToArray();
        }

        // ================================================================
        //  REQUEST SEEDING
        // ================================================================

        private void SeedRequests()
        {
            List<RequestData> requests = new List<RequestData>(_requestCount);

            if (_requestCount > 0)
            {
                requests.Add(new RequestData(
                    System.Guid.NewGuid().ToString(),
                    "Draw a sunset with limited colors",
                    new ConstraintData[]
                    {
                        new ConstraintData(ConstraintType.ColorLimit, intValue: 4),
                        new ConstraintData(ConstraintType.TimeLimit, floatValue: 90f)
                    }));
            }

            if (_requestCount > 1)
            {
                requests.Add(new RequestData(
                    System.Guid.NewGuid().ToString(),
                    "Draw a symmetrical butterfly",
                    new ConstraintData[]
                    {
                        new ConstraintData(ConstraintType.SymmetryRequired, boolValue: true),
                        new ConstraintData(ConstraintType.ColorLimit, intValue: 5)
                    }));
            }

            if (_requestCount > 2)
            {
                requests.Add(new RequestData(
                    System.Guid.NewGuid().ToString(),
                    "Speed draw a flower",
                    new ConstraintData[]
                    {
                        new ConstraintData(ConstraintType.TimeLimit, floatValue: 30f)
                    }));
            }

            if (_requestCount > 3)
            {
                requests.Add(new RequestData(
                    System.Guid.NewGuid().ToString(),
                    "Minimalist tree (few pixels only)",
                    new ConstraintData[]
                    {
                        new ConstraintData(ConstraintType.PixelLimit, intValue: 60),
                        new ConstraintData(ConstraintType.ColorLimit, intValue: 3)
                    }));
            }

            if (_requestCount > 4)
            {
                requests.Add(new RequestData(
                    System.Guid.NewGuid().ToString(),
                    "Draw a cat using two colors",
                    new ConstraintData[]
                    {
                        new ConstraintData(ConstraintType.ColorLimit, intValue: 2)
                    }));
            }

            // Fill remaining with generated requests
            string[] prompts = new string[]
            {
                "Draw a spaceship",
                "Draw ocean waves",
                "Draw a campfire",
                "Draw a mountain",
                "Draw a snowflake"
            };

            for (int i = requests.Count; i < _requestCount; i++)
            {
                string prompt = prompts[(i - 5) % prompts.Length] + " (" + (i + 1) + ")";
                requests.Add(new RequestData(
                    System.Guid.NewGuid().ToString(),
                    prompt,
                    new ConstraintData[]
                    {
                        new ConstraintData(ConstraintType.TimeLimit, floatValue: 60f)
                    }));
            }

            _dataManager.SetAllRequests(requests);
        }
    }
}
