# The user interface

How CashPrism is laid out and themed, and why. The binding rules — the UI is
German, no translatable literal in markup, the component library belongs to
`CashPrism.Web` — are in [`Agents.md`](../Agents.md); this document explains
the shape they produced.

## The shell

One layout, three regions:

| Region | Holds |
|---|---|
| Drawer, left | The wordmark and its tagline, the destinations, and a privacy note with the version pinned to the bottom |
| App bar, top | A section label, the light/dark switch and an Import button; the drawer toggle where the drawer folds away |
| Content | Whatever the page renders — with no padding of its own |

Four of those earn a sentence:

- **The wordmark is two-tone**: `Cash` in the text colour, `Prism` in the
  accent. It is the one place the product name appears, so it is also the one
  place a mark is needed.
- **The destinations are grouped where a group exists.** The start page and the
  bookings stand at the top; the destinations that bring data in sit under
  *Daten*. A heading is written wherever the section changes in
  `NavigationItems`, so a section with no destination yet — the design's
  *Auswerten* — simply does not appear.
- **Privacy is said where it is always in view.** "Nur auf diesem Rechner" is
  the product's promise, so it is pinned to the drawer rather than buried in a
  page. The version sits under it, where a bug report finds it.
- **The app bar says where you are, not what you are looking at.** It carries
  the section in small, spaced, muted capitals; the page's own `h1` carries the
  heading. They read as two different things because they are set as two
  different things — a bar repeating the heading at heading size would look
  like a mistake.

The drawer is not clipped by the app bar: it runs the full height and carries
the brand, and the app bar sits beside it. Below the `Md` breakpoint the drawer
folds away and reopens as an overlay over the content, so the same layout serves
a phone and a large screen rather than two layouts serving one each. The app bar
follows the same breakpoint: the toggle appears where the drawer folds away, the
section label where it does not.

Where the drawer is permanent, a quiet button beside the wordmark collapses it
to a 72 px rail of icons: the wordmark becomes the "CP" monogram, labels move
into tooltips, section headings become hairlines and the privacy note a lock.
The choice lasts for the session; remembering it across a reload is issue #92.
On the rail the toggle is set as a control of its own — as large as the
monogram, outlined and a step darker — where the design keeps it as quiet as
beside the full wordmark: without the wordmark next to it, a faint glyph under
the monogram read as decoration rather than as the way back.
It is done without a second drawer: MudBlazor sizes the drawer, offsets the app
bar and indents the content from one variable, `--mud-drawer-width-left`, so the
layout narrows that variable and all three move together. MudBlazor's own
`Mini` drawer was tried first and dropped — it showed the drawer beside the
content on a narrow screen until the circuit was up.

The drawer and the app bar are MudBlazor's, dressed in the design system's
classes (`cp-sidebar`, `cp-nav`, `cp-topbar`). The design folds the drawer at
1040 px; MudBlazor's breakpoints are fixed, so it folds at `Md`, 960 px. That
gap was accepted rather than replacing the drawer: MudBlazor already keeps a
narrow screen from flashing the open drawer before the circuit is up, which a
breakpoint of our own would have to rebuild.

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

`Layout/PageFrame.razor` is the ordinary frame: the padding (28 px, 16 px on a
phone), a content width of at most 1440 px, the `h1`, the lead line under it and
an optional slot for controls that act on the whole page. It is a component
rather than a convention so that the two-column page can opt out of it without
arguing with a layout.

The lead is one sentence that already says something. A page with nothing to
show yet says so with `Components/EmptyState.razor` — an icon, a heading, a
sentence on what to do and the action that does it — rather than an empty
table.

The heading has to be a real `h1`: `Routes.razor` moves focus there after every
navigation, which is what makes the keyboard and a screen reader land on the new
page instead of at the top of the drawer.

## The theme

The look comes from the design system under [`design/`](../design/) and lives
in two places, because two kinds of code read it:

| Where | Read by |
|---|---|
| `Theme/CashPrismTheme.cs` | MudBlazor's components: both palettes, the type scale, the radius, the app bar height, the drawer width and the card shadow |
| `wwwroot/css/tokens.css` | The design system's own classes: every `--cp-*` custom property — colours, the prism spectrum, type, spacing, radii, shadows, motion |

The colours therefore exist twice. That is the price of a component library
whose palette is configured in C# next to a design system written as CSS:
neither can read the other's. Both are copied from `design/README.md` §1.3 and
`design/tokens/`, and a change to one is a change to both.

`CashPrismTheme` is held as one static instance — the theme is a constant of
the product, and only the light/dark switch changes at runtime.

- **Calm, bright surfaces.** A cool grey ground (`#f2f4f8`), white cards and
  bars, one pixel of `#e3e7ee` between them, and a shadow so soft it only lifts
  a card off the ground. In the dark palette — deep navy, `#111523` on
  `#0a0d16` — the border does that work alone.
- **One confident blue**, `#4a6cf7` (`#7b93ff` in dark), for primary actions,
  the active destination, focus and selection. As a surface it appears only in
  its soft tint (`--cp-primary-soft`) — behind the active destination in the
  drawer and an empty state's icon.
- **Money gets a colour, but the colour never carries the meaning.** The minus
  in front of the amount does that; green and red only reinforce it.
- **The fonts are bundled.** Manrope for everything, JetBrains Mono for strings
  read character by character. Both ship as variable fonts under
  `wwwroot/fonts` and are declared in `app.css`. MudBlazor's own default asks
  the browser for Roboto from `fonts.googleapis.com` — a request CashPrism must
  not make, because everything stays local and the machine may have no
  connection at all. Setting `Typography.Default` is enough: it is the only
  entry that carries a family of its own.
- **Figures line up without a monospace face.** Dates and amounts are set in
  Manrope with tabular figures (`.cp-figure`), so a column of them aligns digit
  under digit. The monospace face is `.cp-mono`, for the checksum, and the
  version beside the wordmark.

### Dark mode in the stylesheet

`tokens.css` puts the light values on `:root` and the dark ones on
`[data-theme="dark"]`. MudBlazor switches its palette but leaves no mark on the
document a stylesheet could select on, so `Theme/ThemeAttribute.razor` mirrors
the layout's dark-mode flag onto `<html data-theme>` through a one-function
JavaScript module beside it. The light theme is the absence of the attribute.

The attribute is written after the first interactive render. A prerendered page
therefore arrives light, and someone whose system is set to dark sees it switch
a moment later — as MudBlazor's own palette does, which learns the system
setting the same way.

The switch in the app bar follows the operating system's setting until it is
used; from then on the choice stands for the rest of the session. MudBlazor
only reports a *change* of that setting, so the layout asks for the starting
value once, after its first render — a click that comes before the answer
wins. It is not
remembered across a reload — that needs browser storage, and the baseline does
not reach for it.

Nothing in the interface loads from an external host. The component library's
CSS and JavaScript are served by the application itself out of the package's
static web assets, the fonts and the icon font out of CashPrism's own, and
`HostBootTests` fails when a page or a stylesheet references any other host.

## The booking list

The Buchungen page is the first screen with real data behind it, and the
measured export puts 6,327 bookings over six years and seven accounts in front
of it. Five decisions follow from that number.

**The database does the sorting and the paging, not the browser.** The page
asks `IBookingReader` for one page at a time — a slice, a column and a
direction — and MudBlazor's `ServerData` is what carries the question. On
Blazor Server every rendered row is also a row pushed down the circuit, so the
alternative would mean sending six thousand of them to show twenty-five. A page
flip measures about 40 ms end to end on the machine that hosts it, a sort about
25 ms.

**The order always has a second key.** Hundreds of bookings share a date, and
SQLite is free to return equally-ranked rows in whatever order suits it. Without
a tiebreaker the same query can answer differently twice, which is how a pager
shows one booking on two pages and another on none. `BookingReader` therefore
appends the fingerprint to every ordering.

**Pages, not an infinite scroll.** MudBlazor can virtualise a grid and fetch
windows as the reader scrolls. A pager was chosen instead: it needs no fixed
grid height to be kept in step across a phone and a large screen, and its
buttons and its "rows per page" are text the resource file already carries. The
sizes offered are 25, 50 and 100 — MudBlazor's "all" entry is left out, because
it asks for every booking at once and that is exactly the render the
server-side paging exists to avoid.

**The pager writes its own info line.** `MudDataGridPager` formats the three
counts it shows with a culture of its own, so a German page came out reading
`1–25 von 6,327` — an English thousands separator beside amounts set with a
German one. The page therefore formats that sentence itself, out of the same
`MudDataGridPager_InfoFormat` resource, and hands the pager a finished line
rather than a template. The resource keeps its numeric placeholders for that
reason, and `PagerInfoFormatTests` is what stops someone taking them out again.

**A column can be sorted, not filtered.** With `ServerData` a filter is a
question for the query rather than something the grid answers by itself, so a
filter menu would look like a feature and do nothing. Filtering and searching
are their own work, and deliberately not part of this screen yet.

The amount is the only column that is more than the stored value written out.
`Bookings/BookingFormat.cs` always writes a sign and two decimals and puts the
booking's own ISO currency code behind the number — the code and not a symbol,
because the symbol of the machine's culture would be a lie about a booking in
another currency. The sign is what says credit or debit; the green and the red
only reinforce it, which is the rule the theme is built on.

An empty database gets the message and a button to the upload page rather than
a table of nothing with a pager counting to zero.

## The list of past imports

The Importverlauf page shows what was read and when: the time of the run, the
file, the export date taken from the sheet name, the file checksum and the four
counts the import reported. It is the page that turns an import from a number
that was on screen once into something you can look back at.

**One order, and no sorting.** A history is read as a history, so the list is
fixed newest first. Making the columns sortable would only make it harder to
see what happened last.

**The instant is stored without its offset, so that the database can order it.**
`ImportRun.ImportedAt` is a `DateTimeOffset`, and SQLite has no type that orders
one: it writes such a value as text with the offset appended, which compares
correctly only while every row carries the same offset, so EF Core refuses to
translate an `ORDER BY` over it at all. `ImportRunConfiguration` therefore
converts the property to a UTC `DateTime` on the way into the column and back on
the way out. The model keeps the type that says what the value is, the column
gets one that can be ordered, ranged and indexed, and nothing is lost — the only
clock that writes it is UTC.

Rows written before that decision carry the old text, so
`ImportedAtAsUtcDateTime` rewrites them. It is written by hand: the column is
`TEXT` either way, so EF Core generated an empty migration. It strips the six
characters of the offset and touches only the rows that actually end in
`+00:00`; a row with any other offset is left alone and fails loudly when it is
read, rather than being quietly shifted by however many hours it was written
with. `ImportedAtAsUtcDateTimeTests` runs it against rows in the old format.

**The checksum is shortened.** A SHA-256 written as hex is 64 characters, which
no column can carry beside seven others. The first twelve are enough to tell
two runs apart and to match one against a hash from elsewhere; the whole value
is the cell's title, so nothing is actually hidden.

**A run without an export date says so.** The date comes from the sheet name,
and a name this version cannot read a date out of leaves the field empty — the
column then reads `unbekannt` rather than showing a blank cell that could just
as well be a rendering fault.

Importing the same file twice records no second run: the import recognises the
file by its hash and stops before a run exists (see
[`finanzguru-export.md`](finanzguru-export.md)). The list shows what happened,
and nothing happened.

## The upload page

The Import page is the only page that does work rather than showing it, and
four of its decisions are not visible in what it renders.

**The read limit is set, and set by CashPrism.** `IBrowserFile.OpenReadStream`
allows 512 KB unless told otherwise and throws above it — no Finanzguru export
has ever fit in that, the measured ones being 1.15 MB. The page passes 64 MB,
far above the largest export anyone is likely to have. MudBlazor's own
`MaxFileSize` would also reject an oversized file, but with a message of its
own in English, so the size is checked in the page instead and the limit is
named in German.

**The import runs off the circuit's thread.** ClosedXML has no asynchronous
API and reading a workbook is CPU-bound — about 1.4 s for the measured export.
On Blazor Server that would occupy the thread the circuit renders on, and the
page would accept no input at all while the spinner turned. The page therefore
hands the import to `Task.Run`. That is deliberately not the pattern
`AGENTS.dotnet.md` rules out: nothing here is synchronous work dressed up as
asynchronous, it is blocking work moved out of the render path.

**What is running outlives the page.** An import keeps going when the page it
was started from is left, so what is in flight is held in a service that lives
for as long as the browser stays connected, not in the component. Coming back
to the page therefore shows the import still running, or the result of one that
finished while it was away — and a second import cannot be started on top of
the first. An overlay covers the page while it runs, so the navigation is out
of reach rather than merely ineffective.

**Why a file was refused crosses the layers as a code, not a sentence.** The
reasons are found in the export reader and the import use case, which write
English, and this UI writes German. So what comes back is an `ImportError`: an
`ImportErrorCode` plus the values that fill its gaps — a column, a row, a
worksheet name. The page looks the code up as `ImportError<Code>` in
`Strings.resx` and formats the arguments into it. A test fails for every code
without a translation, so a new one cannot reach the page as its bare key.

The page lists the first 20 reasons and counts the rest: a broken column fails
every row, and thousands of lines help nobody. The log gets all of them,
untranslated, so a bug report does not depend on what the browser showed.

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

## The design system

The look the interface is moving towards is set out in the design system under
[`design/`](../design/). Open [`design/design-system.html`](../design/design-system.html)
from a local clone in a browser to see it: colours, type, spacing, components
and brand assets on one page. Its previews load React from unpkg.com, so the
page needs an internet connection.
