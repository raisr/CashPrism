// Marks the document with the theme it is rendered in, for the design tokens'
// [data-theme="dark"] selector. Light is the absence of the attribute.
export function applyTheme(isDarkMode) {
  if (isDarkMode) {
    document.documentElement.setAttribute("data-theme", "dark");
  } else {
    document.documentElement.removeAttribute("data-theme");
  }
}
