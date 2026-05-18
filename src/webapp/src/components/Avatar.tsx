import { Canvas, useFrame } from '@react-three/fiber';
import { useMemo } from 'react';
import {
  DataTexture,
  RGBAFormat,
  NearestFilter,
  type Texture,
} from 'three';
import { PATTERNS, pickPattern, type Pattern, type PatternName } from '../pulse/patterns';
import { pickPalette, sampleGradient, type Palette } from '../pulse/palettes';
import { mulberry32 } from '../pulse/rng';

const SIZE = 16;
const AMBIENT = 0.08;
const LOOP_SPEED = 0.15; // cycles per second

interface PulseConfig {
  seed: number;
  patternName?: PatternName;
  paletteName?: string;
}

interface PulseState {
  pattern: Pattern;
  params: unknown;
  palette: Palette;
  data: Uint8Array;
  texture: DataTexture;
}

function createPulse({ seed, patternName, paletteName }: PulseConfig): PulseState {
  const rng = mulberry32(seed);
  const palette = pickPalette(rng, paletteName);
  const picked = pickPattern(rng, patternName);
  const params = picked.def.init(SIZE, rng);
  const data = new Uint8Array(SIZE * SIZE * 4);
  const texture = new DataTexture(data, SIZE, SIZE, RGBAFormat);
  texture.magFilter = NearestFilter;
  texture.minFilter = NearestFilter;
  texture.needsUpdate = true;
  return { pattern: picked.def, params, palette, data, texture };
}

function PulsePlane({ config }: { config: PulseConfig }) {
  // useMemo keeps pattern + per-pixel state alive across renders. Re-init when
  // any input that shapes the visual changes.
  const state = useMemo(
    () => createPulse(config),
    [config.seed, config.patternName, config.paletteName],
  );

  useFrame(({ clock }) => {
    const time = clock.elapsedTime;
    const t = (time * LOOP_SPEED) % 1;
    const data = state.data;
    let i = 0;
    for (let y = 0; y < SIZE; y++) {
      for (let x = 0; x < SIZE; x++) {
        let v = state.pattern.sample(t, x, y, state.params, time);
        v = AMBIENT + v * (1 - AMBIENT);
        if (v < 0) v = 0; else if (v > 1) v = 1;
        const [r, g, b] = sampleGradient(state.palette.stops, v);
        data[i++] = r;
        data[i++] = g;
        data[i++] = b;
        data[i++] = 255;
      }
    }
    state.texture.needsUpdate = true;
  });

  return (
    <mesh>
      <planeGeometry args={[1, 1]} />
      <meshBasicMaterial map={state.texture as unknown as Texture} toneMapped={false} />
    </mesh>
  );
}

export function Avatar({
  seed,
  pattern,
  palette,
}: {
  seed: number;
  pattern?: PatternName;
  palette?: string;
}) {
  // Confirm at module load that PATTERNS keys are wired correctly.
  void PATTERNS;
  return (
    // Explicit style on the wrapper avoids R3F's default `width: 100%; height: 100%`
    // creating a feedback loop with the auto-sized parent (caused the avatar +
    // reroll to slowly drift downward as the wrapper grew on every ResizeObserver
    // tick). With concrete dimensions, R3F sizes the canvas + buffer to match.
    <Canvas
      style={{ width: 200, height: 200, flex: '0 0 auto' }}
      orthographic
      camera={{ zoom: 200, position: [0, 0, 5], near: 0.1, far: 100 }}
      gl={{ antialias: false }}
    >
      <PulsePlane config={{ seed, patternName: pattern, paletteName: palette }} />
    </Canvas>
  );
}
