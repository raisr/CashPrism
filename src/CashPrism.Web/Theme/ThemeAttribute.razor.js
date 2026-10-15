// Marks the document with the theme it is rendered in, for the design tokens'
// color-scheme. Light is written as well: without the attribute the tokens
// follow the operating system rather than MudBlazor.
export function applyTheme(isDarkMode) {
  document.documentElement.setAttribute("data-theme", isDarkMode ? "dark" : "light");
}
