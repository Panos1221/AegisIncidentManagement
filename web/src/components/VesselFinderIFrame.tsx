type Props = {
  lat?: number;   // default center latitude
  lon?: number;   // default center longitude
  zoom?: number;  // zoom level (3–18)
  width?: string;
  height?: number;
};

export default function VesselFinderIFrame({
  lat = 38.0,
  lon = 23.7,
  zoom = 6,
  width = "100%",
  height = 480,
}: Props) {
  // Build the iframe URL manually
  const src =
    `https://www.vesselfinder.com/aismap` +
    `?zoom=${zoom}` +
    `&lat=${lat.toFixed(2)}` +
    `&lon=${lon.toFixed(2)}` +
    `&width=${encodeURIComponent(width)}` +
    `&height=${height}` +
    `&names=true&type=0&fleet=`;

  return (
    <div style={{ width, borderRadius: 8, overflow: "hidden", border: "1px solid #273142" }}>
      <iframe
        title="VesselFinder AIS"
        src={src}
        width="100%"
        height={height}
        style={{ display: "block", border: 0 }}
        loading="lazy"
        referrerPolicy="no-referrer-when-downgrade"
      />
    </div>
  );
}
