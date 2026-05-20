import { Canvas, useFrame } from '@react-three/fiber';
import { useMemo, useRef } from 'react';
import {
  AdditiveBlending,
  BufferAttribute,
  BufferGeometry,
  Color,
  type Points,
  type ShaderMaterial,
} from 'three';
import { mulberry32 } from '../pulse/rng';
import { sampleGradient, type RGB } from '../pulse/palettes';

// Particle-swirl-into-core: many points spiral inward toward the origin,
// re-spawn at the outer ring when they get close. Visual metaphor: "many
// messages → one summary".
//
// Driven by a custom shader so each particle's color follows the conversation
// palette (passed in via paletteStops) and its alpha falls off as it nears the
// core (the "absorbed" feel). No per-frame attribute updates - radius/angle
// math runs on the GPU using a per-particle phase + the global time uniform.

const PARTICLE_COUNT = 360;
const RING_OUTER = 0.85;
const RING_INNER = 0.04;
// Time for one full inward sweep (a single particle's lifetime, in seconds).
// Particles are randomly offset along this so the inflow looks continuous.
const SWEEP_SECONDS = 2.4;
// Tangential rotation rate (radians/sec). Combined with the inward sweep this
// produces the spiral. Positive = counter-clockwise as viewed from +Z.
const SWIRL_RATE = 1.3;

interface SwirlProps {
  paletteStops?: readonly RGB[];
}

const VERT = /* glsl */ `
  uniform float uTime;
  uniform float uSweepSeconds;
  uniform float uSwirlRate;
  uniform float uRingOuter;
  uniform float uRingInner;
  attribute float aPhase;       // 0..1 lifetime offset for this particle
  attribute float aAngle0;      // initial angular position (radians)
  attribute float aSizePx;      // pixel size for this particle
  attribute float aPaletteT;    // 0..1 lookup into the color stops
  varying float vAlpha;
  varying float vPaletteT;

  void main() {
    // life: 0 at outer ring, 1 at core. Wraps so particles continuously respawn.
    float life = fract((uTime / uSweepSeconds) + aPhase);
    float r = mix(uRingOuter, uRingInner, life);
    float a = aAngle0 + life * 6.2831853 * uSwirlRate;
    vec3 p = vec3(cos(a) * r, sin(a) * r, 0.0);

    // Soft fade at both ends: spawn-in over the first 10%, absorb-out over
    // the last 25%. Middle of life is at full brightness.
    float fadeIn  = smoothstep(0.0, 0.10, life);
    float fadeOut = 1.0 - smoothstep(0.75, 1.0, life);
    vAlpha = fadeIn * fadeOut;
    vPaletteT = aPaletteT;

    vec4 mvPos = modelViewMatrix * vec4(p, 1.0);
    gl_Position = projectionMatrix * mvPos;
    gl_PointSize = aSizePx;
  }
`;

const FRAG = /* glsl */ `
  precision mediump float;
  uniform vec3 uStop0;
  uniform vec3 uStop1;
  uniform vec3 uStop2;
  varying float vAlpha;
  varying float vPaletteT;

  void main() {
    // Round soft point sprite (gl_PointCoord is 0..1 across the quad).
    vec2 d = gl_PointCoord - vec2(0.5);
    float dist = length(d);
    float soft = smoothstep(0.5, 0.0, dist);

    // 3-stop gradient lookup. The stops correspond to the head, mid, and
    // tail of the conversation palette so the swirl color-matches the avatar.
    vec3 col = mix(uStop0, uStop1, smoothstep(0.0, 0.5, vPaletteT));
    col = mix(col, uStop2, smoothstep(0.5, 1.0, vPaletteT));

    gl_FragColor = vec4(col, soft * vAlpha * 0.85);
  }
`;

function Swirl({ paletteStops }: SwirlProps) {
  const pointsRef = useRef<Points | null>(null);
  const matRef = useRef<ShaderMaterial | null>(null);

  // Per-particle attributes - generated once, then static. Animation is
  // driven by uTime in the vertex shader (no JS-side per-frame work).
  const geometry = useMemo(() => {
    const g = new BufferGeometry();
    // Three positions per vertex but the shader replaces them; still need a
    // non-empty position attribute so three.js sets vertex count.
    const positions = new Float32Array(PARTICLE_COUNT * 3);
    const phases = new Float32Array(PARTICLE_COUNT);
    const angles = new Float32Array(PARTICLE_COUNT);
    const sizes = new Float32Array(PARTICLE_COUNT);
    const paletteTs = new Float32Array(PARTICLE_COUNT);
    // Deterministic seed so the swirl looks the same across renders of the
    // same overlay (no "shuffle on every paint" flicker).
    const rng = mulberry32(0x5113ade);
    for (let i = 0; i < PARTICLE_COUNT; i++) {
      phases[i] = rng();
      angles[i] = rng() * Math.PI * 2;
      // Mix of fine particles + a few brighter "embers" so the swirl has
      // some visual texture instead of looking like a uniform dust cloud.
      const r = rng();
      sizes[i] = r < 0.92 ? 2.0 + rng() * 1.5 : 4.5 + rng() * 2.5;
      paletteTs[i] = rng();
    }
    g.setAttribute('position', new BufferAttribute(positions, 3));
    g.setAttribute('aPhase', new BufferAttribute(phases, 1));
    g.setAttribute('aAngle0', new BufferAttribute(angles, 1));
    g.setAttribute('aSizePx', new BufferAttribute(sizes, 1));
    g.setAttribute('aPaletteT', new BufferAttribute(paletteTs, 1));
    return g;
  }, []);

  // Sample 3 stops out of the palette (head / middle / tail) for the shader's
  // 3-stop gradient. Falls back to the accent pink + a warm gold + a deep
  // violet when no palette is provided so the overlay still looks intentional
  // on the Default project.
  const stops = useMemo(() => {
    const fallback: [RGB, RGB, RGB] = [
      [214, 59, 143],
      [255, 196, 102],
      [76, 36, 140],
    ];
    if (!paletteStops || paletteStops.length < 2) return fallback;
    const head = sampleGradient(paletteStops, 0.0);
    const mid = sampleGradient(paletteStops, 0.5);
    const tail = sampleGradient(paletteStops, 1.0);
    return [head, mid, tail] as [RGB, RGB, RGB];
  }, [paletteStops]);

  const uniforms = useMemo(() => ({
    uTime: { value: 0 },
    uSweepSeconds: { value: SWEEP_SECONDS },
    uSwirlRate: { value: SWIRL_RATE },
    uRingOuter: { value: RING_OUTER },
    uRingInner: { value: RING_INNER },
    uStop0: { value: new Color(stops[0][0] / 255, stops[0][1] / 255, stops[0][2] / 255) },
    uStop1: { value: new Color(stops[1][0] / 255, stops[1][1] / 255, stops[1][2] / 255) },
    uStop2: { value: new Color(stops[2][0] / 255, stops[2][1] / 255, stops[2][2] / 255) },
  }), [stops]);

  useFrame(({ clock }) => {
    const m = matRef.current;
    if (m) m.uniforms.uTime.value = clock.elapsedTime;
  });

  return (
    <points ref={pointsRef} geometry={geometry}>
      <shaderMaterial
        ref={matRef}
        uniforms={uniforms}
        vertexShader={VERT}
        fragmentShader={FRAG}
        transparent
        depthWrite={false}
        blending={AdditiveBlending}
      />
    </points>
  );
}

// Bright central core that brightens slightly when a particle "lands". Cheap
// proxy: pulse with the swirl's sweep frequency so it feels alive.
function Core({ paletteStops }: SwirlProps) {
  const matRef = useRef<ShaderMaterial | null>(null);
  const color = useMemo(() => {
    if (paletteStops && paletteStops.length > 0) {
      const c = sampleGradient(paletteStops, 0.5);
      return new Color(c[0] / 255, c[1] / 255, c[2] / 255);
    }
    return new Color(1.0, 0.85, 0.55);
  }, [paletteStops]);

  const uniforms = useMemo(() => ({
    uTime: { value: 0 },
    uColor: { value: color },
  }), [color]);

  useFrame(({ clock }) => {
    if (matRef.current) matRef.current.uniforms.uTime.value = clock.elapsedTime;
  });

  return (
    <mesh>
      <circleGeometry args={[0.18, 64]} />
      <shaderMaterial
        ref={matRef}
        uniforms={uniforms}
        transparent
        depthWrite={false}
        blending={AdditiveBlending}
        vertexShader={/* glsl */ `
          varying vec2 vUv;
          void main() {
            vUv = uv;
            gl_Position = projectionMatrix * modelViewMatrix * vec4(position, 1.0);
          }
        `}
        fragmentShader={/* glsl */ `
          precision mediump float;
          uniform float uTime;
          uniform vec3 uColor;
          varying vec2 vUv;
          void main() {
            vec2 d = vUv - vec2(0.5);
            float dist = length(d) * 2.0;
            // Radial falloff + gentle breathing at 0.4Hz to suggest absorption.
            float breath = 0.85 + 0.15 * sin(uTime * 2.4);
            float a = pow(1.0 - clamp(dist, 0.0, 1.0), 2.4) * breath;
            gl_FragColor = vec4(uColor, a * 0.9);
          }
        `}
      />
    </mesh>
  );
}

interface CompactingOverlayProps {
  /** Conversation's palette stops for color matching the swirl to the avatar. */
  paletteStops?: readonly RGB[] | null;
  /** How many messages are being folded - displayed in the caption. */
  messageCount?: number;
}

/**
 * Full-area overlay shown while the agent runs a rolling-summary compact.
 * Renders semi-transparent over the messages list (NOT the composer), so the
 * user sees the swirl + "Compacting…" caption while the summary LLM call
 * burns its 5–30s before the real turn begins.
 *
 * Mounted by Chat.tsx when a `compactStart` event arrives; unmounted on
 * `compactDone` (or `error` / `done`, as a safety net).
 */
export function CompactingOverlay({ paletteStops, messageCount }: CompactingOverlayProps) {
  const stops = paletteStops ?? undefined;
  const label = messageCount && messageCount > 0
    ? `Compacting ${messageCount} earlier ${messageCount === 1 ? 'message' : 'messages'}…`
    : 'Compacting earlier messages…';
  return (
    <div className="compacting-overlay" role="status" aria-live="polite" aria-label={label}>
      <div className="compacting-canvas">
        <Canvas
          orthographic
          camera={{ zoom: 220, position: [0, 0, 5], near: 0.1, far: 100 }}
          gl={{ antialias: true, alpha: true }}
        >
          <Core paletteStops={stops} />
          <Swirl paletteStops={stops} />
        </Canvas>
      </div>
      <div className="compacting-caption">{label}</div>
      <div className="compacting-sub">Gabriel is folding old turns into a rolling summary so the conversation stays inside the context window.</div>
    </div>
  );
}
