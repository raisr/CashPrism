# The user interface

How CashPrism is laid out and themed, and why. The binding rules — the UI is
German, no translatable literal in markup, the component library belongs to
`CashPrism.Web` — are in [`Agents.md`](../Agents.md); this document explains
the shape they produced.

## The shell

One layout, three regions:

| Region | Holds |
|---|---|
| Drawer, left | The wordmark with its version, and the destinations |
| App bar, top | A section label, the drawer toggle, the light/dark switch |
| Content | Whatever the page renders — with no padding of its own |

Two of those earn a sentence:

- **The wordmark is two-tone**: `Cash` in the text colour, `Prism` in the
  accent. It is the one place the product name appears, so it is also the one
  place a mark is needed.
- **The app bar says where you are, not what you are looking at.** It carries
  the section in small, spaced, muted capitals; the page's own `h1` carries the
  heading. They read as two different things because they are set as two
  different things — a bar repeating the heading at heading size would look
  like a mistake.

The drawer is not clipped by the app bar: it runs the full height and carries
the brand, and the app bar sits beside it. Below the `Md` breakpoint the drawer
folds away and reopens as an overlay over the content, so the same layout serves
a phone and a large screen rather than two layouts serving one each.

### Why this shape

Three directions were drawn up before a component was written — a dense ledger
behind a permanent drawer, an overview of the last import behind a top bar, and
a list-plus-detail workbench. The shape above is the first with the second's top
bar: the application exists to make years of bookings analysable, so the
navigation is labelled and permanent where there is room for it, while a top bar
keeps the current page named at every width.

A list-plus-detail page — accounts beside the content, a booking beside the list
— is where this is expected to go once there is data to put in it. That is why
the layout contributes no padding: `MudMainContent` only keeps content clear of
the fixed app bar, and a page that wants two columns simply does not use the
ordinary frame. Adding one later changes a page, not the layout.

### The page frame

`Layout/PageFrame.razor` is the ordinary frame: the padding, the `h1` and the
line under it. It is a component rather than a convention so that the two-column
page can opt out of it without arguing with a layout.

The heading has to be a real `h1`: `Routes.razor` moves focus there after every
navigation, which is what makes the keyboard and a screen reader land on the new
page instead of at the top of the drawer.

## The theme

Both palettes live in `Theme/CashPrismTheme.cs` and are held as one static
instance — the theme is a constant of the product, and only the light/dark
switch changes at runtime.

- **Separation comes from lines, not from tint or shadow.** The ground is barely
  off-white, the surfaces are white, and one pixel of `#e3e5ea` sits between
  them. That is what lets the dark palette be a straight inversion rather than a
  second design: a shadow in dark mode costs contrast and buys nothing.
- **The accent is `#3c4ba6`, and it appears as a surface exactly once** — behind
  the active destination in the drawer. That tint is mixed from the palette in
  the stylesheet (`--cp-accent-soft`, `color-mix` at 12 % over the surface)
  rather than pinned per theme, so it follows light and dark without a second
  definition.
- **Money gets a colour, but the colour never carries the meaning.** The minus
  in front of the amount does that; green and red only reinforce it. Both
  palettes therefore pick a green and a red that stay distinguishable in
  greyscale.
- **The font is a system stack.** MudBlazor's own default asks the browser for
  Roboto from `fonts.googleapis.com` and the package ships no font file — a
  request CashPrism must not make, because everything stays local and the
  machine may have no connection at all. Setting `Typography.Default` is enough:
  it is the only entry that carries a family of its own.
- **Figures get a monospace stack of their own**, `--cp-font-mono` in the
  stylesheet, for anything that has to line up in a column — amounts, dates, a
  fingerprint. It lives in CSS rather than in the theme because `MudTheme` has
  no slot for a second family and nothing in C# reads it. Today only the version
  beside the wordmark uses it; the booking list is what it is there for.

The switch in the app bar follows the operating system's setting until it is
used; from then on the choice stands for the rest of the session. It is not
remembered across a reload — that needs browser storage, and the baseline does
not reach for it.

Nothing in the interface loads from an external host. The component library's
CSS and JavaScript are served by the application itself out of the package's
static web assets, and the only `url(` in that stylesheet is an inline
`data:` image.

## MudBlazor's own strings

The component library ships its user-visible text — dialog buttons, the table
pager, the data-grid menus — in English.
`Localisation/ResourceMudLocalizer.cs` feeds it from `Strings.resx`, the same
file the application's own text comes from.

Two details decide whether this works:

- **The keys are MudBlazor's, written with an underscore**:
  `MudDataGridPager_RowsPerPage`, `MudDataGrid_Filter`. They are the names in
  the library's own resource set, not a scheme CashPrism chose.
- **A key the resource file does not carry must report itself as not found.**
  MudBlazor reads `LocalizedString.ResourceNotFound` and only then falls back to
  its English default. A localiser that answered for every key — with the key
  itself, as a naive implementation does — would render raw identifiers such as
  `MudDataGrid_Sort` in the UI.

`IStringLocalizer` already reports a miss that way, so the lookup is passed
straight through. The consequence is that `Strings.resx` may stay as short as
the set of components actually in use: leaving a key out is a decision to keep
MudBlazor's English, not a bug.
