// Writes the preference cookie from the browser: a circuit has no HTTP
// response to set it on. Not HttpOnly for that reason, and it carries a
// layout choice only.
export function writeCookie(name, value, maxAgeSeconds) {
  document.cookie = `${name}=${value}; Path=/; Max-Age=${maxAgeSeconds}; SameSite=Lax`;
}
