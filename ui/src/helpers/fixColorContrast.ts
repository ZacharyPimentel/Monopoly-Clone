export const fixColorContrast = (baseColor:string, backgroundColor:string = '#ffffff'):string => {
    let rgbColor = cssColorToRgb(baseColor);
    let rgbBackgroundColor = cssColorToRgb(backgroundColor);
    let ratio = contrastRatio(rgbColor, rgbBackgroundColor);

    // Try darkening until the ratio is acceptable (hardcoded 4.5)
    let step = 0;
    while (ratio < 4.5 && step < 50) {
        rgbColor = darkenRgb(rgbColor, 5); // darken by 5 each loop
        ratio = contrastRatio(rgbColor, rgbBackgroundColor);
        step++;
    }
    return `rgb(${rgbColor.r}, ${rgbColor.g}, ${rgbColor.b})`;
}

const cssColorToRgb = (color: string) => {
  const el = document.createElement("div");
  el.style.color = color;
  document.body.appendChild(el);
  const rgb = getComputedStyle(el).color;
  document.body.removeChild(el);

  //regex to match the rgb values from an rgb string such as rgb(255,200,100)
  const match = rgb.match(/rgb[a]?\((\d+),\s*(\d+),\s*(\d+)/);
  if (!match) throw new Error("Invalid color: " + color);
  const [, r, g, b] = match.map(Number);
  return { r, g, b };
}

function contrastRatio(colorOne: { r: number; g: number; b: number }, colorTwo: { r: number; g: number; b: number }) {
  const luminanceOne = relativeLuminance(colorOne);
  const luminanceTwo = relativeLuminance(colorTwo);
  const lighter = Math.max(luminanceOne, luminanceTwo);
  const darker = Math.min(luminanceOne, luminanceTwo);
  //WCAG Contrast ratio
  return (lighter + 0.05) / (darker + 0.05);
}

function relativeLuminance({ r, g, b }: { r: number; g: number; b: number }) {
  const toLinear = (c: number) => {
    c /= 255;
    return c <= 0.03928 ? c / 12.92 : Math.pow((c + 0.055) / 1.055, 2.4);
  };
  const R = toLinear(r), G = toLinear(g), B = toLinear(b);
  return 0.2126 * R + 0.7152 * G + 0.0722 * B;
}

function darkenRgb({ r, g, b }: { r: number; g: number; b: number }, amount: number) {
  return {
    r: Math.max(0, r - amount),
    g: Math.max(0, g - amount),
    b: Math.max(0, b - amount)
  };
}