/**
 * Security: Color validator to prevent CSS injection attacks
 * Validates that color strings are safe CSS color values
 */

// Valid CSS named colors (subset of most common ones)
const VALID_COLOR_NAMES = new Set([
  'transparent', 'currentcolor', 'inherit',
  'black', 'white', 'red', 'green', 'blue', 'yellow', 'orange', 'purple',
  'pink', 'brown', 'gray', 'grey', 'cyan', 'magenta', 'lime', 'olive',
  'navy', 'teal', 'aqua', 'fuchsia', 'maroon', 'silver',
  // Extended colors
  'aliceblue', 'antiquewhite', 'aquamarine', 'azure', 'beige', 'bisque',
  'blanchedalmond', 'blueviolet', 'burlywood', 'cadetblue', 'chartreuse',
  'chocolate', 'coral', 'cornflowerblue', 'cornsilk', 'crimson', 'darkblue',
  'darkcyan', 'darkgoldenrod', 'darkgray', 'darkgreen', 'darkgrey', 'darkkhaki',
  'darkmagenta', 'darkolivegreen', 'darkorange', 'darkorchid', 'darkred',
  'darksalmon', 'darkseagreen', 'darkslateblue', 'darkslategray', 'darkslategrey',
  'darkturquoise', 'darkviolet', 'deeppink', 'deepskyblue', 'dimgray', 'dimgrey',
  'dodgerblue', 'firebrick', 'floralwhite', 'forestgreen', 'gainsboro',
  'ghostwhite', 'gold', 'goldenrod', 'greenyellow', 'honeydew', 'hotpink',
  'indianred', 'indigo', 'ivory', 'khaki', 'lavender', 'lavenderblush',
  'lawngreen', 'lemonchiffon', 'lightblue', 'lightcoral', 'lightcyan',
  'lightgoldenrodyellow', 'lightgray', 'lightgreen', 'lightgrey', 'lightpink',
  'lightsalmon', 'lightseagreen', 'lightskyblue', 'lightslategray', 'lightslategrey',
  'lightsteelblue', 'lightyellow', 'limegreen', 'linen', 'mediumaquamarine',
  'mediumblue', 'mediumorchid', 'mediumpurple', 'mediumseagreen', 'mediumslateblue',
  'mediumspringgreen', 'mediumturquoise', 'mediumvioletred', 'midnightblue',
  'mintcream', 'mistyrose', 'moccasin', 'navajowhite', 'oldlace', 'olivedrab',
  'orangered', 'orchid', 'palegoldenrod', 'palegreen', 'paleturquoise',
  'palevioletred', 'papayawhip', 'peachpuff', 'peru', 'plum', 'powderblue',
  'rosybrown', 'royalblue', 'saddlebrown', 'salmon', 'sandybrown', 'seagreen',
  'seashell', 'sienna', 'skyblue', 'slateblue', 'slategray', 'slategrey',
  'snow', 'springgreen', 'steelblue', 'tan', 'thistle', 'tomato', 'turquoise',
  'violet', 'wheat', 'whitesmoke', 'yellowgreen'
]);

// Regex patterns for valid color formats
const HEX_COLOR_PATTERN = /^#([0-9a-fA-F]{3}|[0-9a-fA-F]{4}|[0-9a-fA-F]{6}|[0-9a-fA-F]{8})$/;
const RGB_PATTERN = /^rgb\(\s*(\d{1,3})\s*,\s*(\d{1,3})\s*,\s*(\d{1,3})\s*\)$/;
const RGBA_PATTERN = /^rgba\(\s*(\d{1,3})\s*,\s*(\d{1,3})\s*,\s*(\d{1,3})\s*,\s*(0|1|0?\.\d+)\s*\)$/;
const HSL_PATTERN = /^hsl\(\s*(\d{1,3})\s*,\s*(\d{1,3})%\s*,\s*(\d{1,3})%\s*\)$/;
const HSLA_PATTERN = /^hsla\(\s*(\d{1,3})\s*,\s*(\d{1,3})%\s*,\s*(\d{1,3})%\s*,\s*(0|1|0?\.\d+)\s*\)$/;

/**
 * Validates if a string is a safe CSS color value
 * Returns true if the color is valid, false otherwise
 */
export function isValidColor(color: string | undefined | null): boolean {
  if (!color || typeof color !== 'string') {
    return false;
  }

  const trimmed = color.trim().toLowerCase();

  // Check named colors
  if (VALID_COLOR_NAMES.has(trimmed)) {
    return true;
  }

  // Check hex colors
  if (HEX_COLOR_PATTERN.test(color.trim())) {
    return true;
  }

  // Check rgb()
  const rgbMatch = color.trim().match(RGB_PATTERN);
  if (rgbMatch) {
    const [, r, g, b] = rgbMatch;
    return parseInt(r) <= 255 && parseInt(g) <= 255 && parseInt(b) <= 255;
  }

  // Check rgba()
  const rgbaMatch = color.trim().match(RGBA_PATTERN);
  if (rgbaMatch) {
    const [, r, g, b] = rgbaMatch;
    return parseInt(r) <= 255 && parseInt(g) <= 255 && parseInt(b) <= 255;
  }

  // Check hsl()
  const hslMatch = color.trim().match(HSL_PATTERN);
  if (hslMatch) {
    const [, h, s, l] = hslMatch;
    return parseInt(h) <= 360 && parseInt(s) <= 100 && parseInt(l) <= 100;
  }

  // Check hsla()
  const hslaMatch = color.trim().match(HSLA_PATTERN);
  if (hslaMatch) {
    const [, h, s, l] = hslaMatch;
    return parseInt(h) <= 360 && parseInt(s) <= 100 && parseInt(l) <= 100;
  }

  return false;
}

/**
 * Sanitizes a color value - returns the color if valid, or a fallback/undefined if not
 */
export function sanitizeColor(color: string | undefined | null, fallback?: string): string | undefined {
  if (isValidColor(color)) {
    return color!.trim();
  }
  return fallback;
}

/**
 * Escapes a color value for safe use in inline scripts
 * Only returns the value if it's a valid color, otherwise returns empty string
 */
export function escapeColorForScript(color: string | undefined | null): string {
  if (!isValidColor(color)) {
    return '';
  }
  // Additional safety: escape any remaining special characters
  return color!.trim().replace(/['"\\<>]/g, '');
}
