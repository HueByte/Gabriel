import { Canvas, useFrame } from '@react-three/fiber';
import { useMemo, useRef } from 'react';
import { PlaneGeometry, type Mesh, type MeshBasicMaterial } from 'three';
import { pickPalette, sampleGradient, type Palette } from '../pulse/palettes';
import { pickPattern, type Pattern } from '../pulse/patterns';
import { mulberry32 } from '../pulse/rng';

const BARS = 16;
// Sample the avatar's pattern at the middle horizontal row — one column per bar.
const SAMPLE_Y = 8;
const LOOP_SPEED = 0.22; // slightly faster than the big avatar (0.15) so it reads as "active"

// 3D layout — bars sit across [-0.5, 0.5] x and grow up from y = -MAX_H/2.
const MAX_H = 0.7;
const BAR_SPACING = 1 / BARS;
const BAR_WIDTH = BAR_SPACING * 0.55;
const MIN_INTENSITY = 0.12; // floor so even quiet bars are visible

interface PulseState {
  palette: Palette;
  pattern: Pattern;
  params: unknown;
}

function buildState(seed: number): PulseState {
  // Mirror Avatar.tsx's RNG order (palette, then pattern, then params) so a
  // shared seed yields the same palette+pattern as the big avatar.
  const rng = mulberry32(seed);
  const palette = pickPalette(rng);
  const picked = pickPattern(rng);
  const params = picked.def.init(BARS, rng);
  return { palette, pattern: picked.def, params };
}

function Bars({ seed }: { seed: number }) {
  const state = useMemo(() => buildState(seed), [seed]);
  const meshRefs = useRef<(Mesh | null)[]>([]);

  // Bottom-anchored geometry — translating the plane up by MAX_H/2 puts its
  // origin at the bottom edge, so scaling Y grows upward from the baseline.
  const geom = useMemo(() => {
    const g = new PlaneGeometry(BAR_WIDTH, MAX_H);
    g.translate(0, MAX_H / 2, 0);
    return g;
  }, []);

  useFrame(({ clock }) => {
    const time = clock.elapsedTime;
    const t = (time * LOOP_SPEED) % 1;
    for (let i = 0; i < BARS; i++) {
      const m = meshRefs.current[i];
      if (!m) continue;
      // Phase offset each bar slightly so the waveform travels visibly L→R.
      const v = state.pattern.sample(t, i, SAMPLE_Y, state.params, time);
      const intensity = MIN_INTENSITY + v * (1 - MIN_INTENSITY);
      m.scale.y = intensity;
      const [r, g, b] = sampleGradient(state.palette.stops, v);
      const mat = m.material as MeshBasicMaterial;
      mat.color.setRGB(r / 255, g / 255, b / 255);
    }
  });

  const indices = useMemo(() => Array.from({ length: BARS }, (_, i) => i), []);

  return (
    <>
      {indices.map(i => (
        <mesh
          key={i}
          ref={el => { meshRefs.current[i] = el; }}
          position={[(i + 0.5) * BAR_SPACING - 0.5, -MAX_H / 2, 0]}
          geometry={geom}
        >
          <meshBasicMaterial toneMapped={false} />
        </mesh>
      ))}
    </>
  );
}

/**
 * Mini three.js indicator shown between "user submitted" and "first token". Renders
 * a 16-bar mini equalizer driven by the same pattern + palette as the conversation's
 * avatar — same identity, different visual language.
 */
export function ThinkingPulse({ seed }: { seed: number }) {
  return (
    <Canvas
      style={{ width: 96, height: 18, flex: '0 0 auto' }}
      orthographic
      camera={{ zoom: 96, position: [0, 0, 5], near: 0.1, far: 100 }}
      gl={{ antialias: false, alpha: true }}
    >
      <Bars seed={seed} />
    </Canvas>
  );
}
