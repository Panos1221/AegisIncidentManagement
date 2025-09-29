export const getSeaState = (waveHeight: number): string => {
  if (waveHeight < 0.1) return 'seaStateCalm';
  if (waveHeight < 0.5) return 'seaStateCalmRippled';
  if (waveHeight < 1.25) return 'seaStateSmooth';
  if (waveHeight < 2.5) return 'seaStateModerate';
  if (waveHeight < 4) return 'seaStateRough';
  if (waveHeight < 6) return 'seaStateVeryRough';
  if (waveHeight < 9) return 'seaStateHigh';
  if (waveHeight < 14) return 'seaStateVeryHigh';
  return 'seaStatePhenomenal';
};

export const getWindSpeedUnit = (speed: number, t: any): string => {
  return speed === 1 ? t.knot : t.knots;
};
