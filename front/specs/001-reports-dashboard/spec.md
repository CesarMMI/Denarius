# Feature Specification: Reports Dashboard

**Feature Branch**: `001-reports-dashboard`

**Created**: 2026-10-03

**Status**: Draft

**Input**: User description: "A reports page (`/reports`, lazily loaded and linked from the navigation
menu) laid out as a full-height bento grid: a row of five summary cards (balance, income, expenses,
savings rate, projection) and, below, a doughnut of expenses by category, bars of income vs. expenses
over the months, a line of the month's cumulative expenses against the previous month, and a tall
block listing the month's transactions with its own scroll. A month selector at the top updates every
block. Each block loads on its own, with independent loading, empty ('Sem movimentações neste mês')
and error-with-retry states. pt-BR formatting (BRL, dd/MM dates, months spelled out), expenses in red
everywhere, changes against the previous month with ↑/↓ and a semantic color (for expenses, going up
is bad), light/dark theme, BRL tooltips. No page scroll on desktop; on smaller screens the grid
collapses into one column and the page may scroll. Charts with ng2-charts, added with `ng add`."

## Clarifications

### Session 2026-10-03

- Q: What changes before the feature is finished? → A: The doughnut shows at most five slices, the fifth
  being "Outras" when needed; the list shows the ten latest transactions with a "Ver todas" button that
  opens the transactions page filtered by the month. A balance line over the bars was tried and dropped:
  it cluttered the chart.
- Q: Which color does "Outras" take? → A: The neutral one (`outline`), as before.
- Q: Are expenses red in the summary cards? → A: No. The cards show them as negative values without
  color, since the red drew too much attention; the charts and the list keep it.

## User Scenarios & Testing _(mandatory)_

### User Story 1 - See how the current month is going (Priority: P1)

As someone tracking my finances, I want a reports page in the menu that opens on the current month and
shows its balance, income, expenses, savings rate and month-end projection, each compared with the
previous month, so I can tell at a glance whether the month is on track.

**Why this priority**: It is the entry point and the headline of the dashboard; every other block
details what these five numbers summarize.

**Independent Test**: Open the reports page from the menu and check the five cards against the month's
summary, including the comparison with the previous month and the colors of each change.

**Acceptance Scenarios**:

1. **Given** the navigation menu, **When** the user picks "Relatórios", **Then** the reports page opens
   on the current month.
2. **Given** a month with income and expenses, **When** the page shows its summary, **Then** the cards
   show the balance, income, expenses (as a negative value), savings rate and projected expense, in BRL.
3. **Given** expenses went up compared with the previous month, **When** the expense card is shown,
   **Then** it shows an up arrow, the percentage and the previous month by name, in the "bad" color;
   income or balance going up shows the "good" color instead.
4. **Given** a month without income, **When** the savings-rate card is shown, **Then** it shows that
   there is no rate instead of a number.
5. **Given** the previous month had no value to compare with, **When** a card is shown, **Then** it says
   there is no comparison instead of a percentage.

---

### User Story 2 - Look at another month (Priority: P2)

As someone reviewing my finances, I want to pick a month at the top of the page and have every block
follow it, so I can review any month the same way.

**Why this priority**: Without it the page only ever shows the current month.

**Independent Test**: Pick another month and confirm that every block reloads for it.

**Acceptance Scenarios**:

1. **Given** the page on the current month, **When** the user picks another month, **Then** the
   summary, the three charts and the transaction list all show that month.
2. **Given** one block failed to load, **When** the user retries it, **Then** only that block loads
   again and the others stay as they are.
3. **Given** the page on a month, **When** the user picks "Mês anterior" or "Próximo mês" in the
   header, **Then** every block shows that month, loaded once.

---

### User Story 3 - See where the money went (Priority: P3)

As someone trying to spend less, I want a doughnut of the month's expenses by category, in each
category's color, so I see which categories weigh the most.

**Why this priority**: The first breakdown anyone asks for after the summary.

**Independent Test**: Open a month with expenses in several categories and check the slices, their
colors, the "Outras" slice and the tooltips.

**Acceptance Scenarios**:

1. **Given** a month with expenses in several categories, **When** the doughnut is shown, **Then** each
   category is a slice in its own color, largest first, with a legend.
2. **Given** a month with more than five categories with expenses, **When** the doughnut is shown,
   **Then** the four largest keep their slices and the rest appear together as a fifth, "Outras", in a
   neutral color.
3. **Given** the pointer over a slice, **When** the tooltip opens, **Then** it shows the category, the
   amount in BRL and its share of the month's expenses.

---

### User Story 4 - Compare income and expenses over the months (Priority: P4)

As someone planning ahead, I want bars of income and expenses for the last twelve months ending in the
selected month, so I can spot the months that went wrong.

**Why this priority**: A trend view pays off once there is some history.

**Independent Test**: Open a month and check twelve consecutive months of bars, including months
without movement, with BRL tooltips.

**Acceptance Scenarios**:

1. **Given** the selected month, **When** the bars are shown, **Then** they cover the twelve months
   ending in it, oldest first, with income in green and expenses in red.
2. **Given** a month without movement in the period, **When** the bars are shown, **Then** that month
   still has its place on the axis, at zero.
3. **Given** the pointer over a month, **When** the tooltip opens, **Then** it names the month in full
   and shows its income and expenses in BRL.

---

### User Story 5 - Follow the spending pace (Priority: P5)

As someone watching my spending during the month, I want a line of the month's expenses accumulated
day by day next to the previous month's, so I see early whether I am spending faster than usual.

**Why this priority**: A refinement of the projection card, most useful during the month.

**Independent Test**: Open the current month and check that its line stops today while the previous
month's line covers all its days, with BRL tooltips.

**Acceptance Scenarios**:

1. **Given** the current month, **When** the line chart is shown, **Then** the month's line, in red,
   stops at today and the previous month's line, in a neutral color, covers the whole month.
2. **Given** the pointer over a day, **When** the tooltip opens, **Then** it shows both months' totals
   up to that day, in BRL, with the months named in full.

---

### User Story 6 - Check the details (Priority: P6)

As someone reviewing a month, I want the month's latest transactions listed newest first, and a way to
see all of them, so I can check the details behind the charts.

**Why this priority**: The transactions page already lists them; here they are a companion to the
charts.

**Independent Test**: Open a month with more than ten transactions, check that the block lists the ten
newest, and follow "Ver todas" to the transactions page filtered by that month.

**Acceptance Scenarios**:

1. **Given** a month with transactions, **When** the list is shown, **Then** the ten most recent show
   their date (dd/MM), description, category and amount, newest first, expenses in red, under a title
   that counts all of the month's transactions.
2. **Given** more transactions than fit the block, **When** the user scrolls, **Then** only the list
   scrolls, not the page.
3. **Given** the list, **When** the user picks "Ver todas", **Then** the transactions page opens with its
   filters shown and set to that month.

---

### Edge Cases

- While a block loads it shows a loading indicator; the other blocks are not affected.
- A block that fails shows what failed and a "Tentar novamente" button that retries that block only.
- A month without movement shows "Sem movimentações neste mês." in the summary and in the list; the
  doughnut and the line, which are about expenses, say there were no expenses; the bars only show an
  empty state when the whole period had no movement.
- A month with ten transactions or fewer lists them all; "Ver todas" still opens the transactions page
  for the month.
- Reloading a block that already shows data keeps it visible, dimmed, until the new data arrives.
- A future month shows zero projection and no line for the days that haven't come.
- Desktop windows show the whole page without scrolling it; narrow or short windows stack the blocks
  in one column and let the page scroll.
- In dark mode the charts use the dark theme's colors, and they repaint when the system theme changes.
- Category names come from the user; they are shown as text, never interpreted.

## Requirements _(mandatory)_

### Functional Requirements

- **FR-001**: The application MUST offer a reports page at `/reports`, linked from the navigation menu,
  loaded only when opened.
- **FR-002**: The page MUST open on the current month (America/Sao_Paulo) and offer a month selector at
  the top that updates every block, with buttons for the previous and the next month.
- **FR-003**: The page MUST show five summary cards — balance, income, expenses, savings rate and
  projection — with balance, income and expenses compared with the previous month by an arrow (↑/↓),
  the percentage, the previous month by name and a semantic color (going up is good for income and
  balance, bad for expenses).
- **FR-004**: The page MUST show a doughnut of the month's expenses by category, in the categories'
  colors, with at most five slices: past five categories, the four largest and "Outras" — the rest added
  up — in a neutral color.
- **FR-005**: The page MUST show bars of income and expenses for the twelve months ending in the
  selected month.
- **FR-006**: The page MUST show a line of the month's cumulative expenses against the previous
  month's.
- **FR-007**: The page MUST list the month's ten most recent transactions — date (dd/MM), description,
  category and amount — newest first, scrolling inside its block, with a "Ver todas" action that opens the
  transactions page filtered by the month.
- **FR-008**: Each block MUST load independently and show its own loading, empty and error states, the
  error with a button that retries that block only.
- **FR-009**: Money MUST be shown in BRL with pt-BR formatting, including chart tooltips and axes;
  months MUST be spelled out where they are named.
- **FR-010**: Expenses MUST be shown in the same red in the charts and the list; the summary cards show
  them as negative values, without color.
- **FR-011**: The page MUST fill the window's available height without page scroll on desktop, in a
  grid of cards with rounded corners, consistent gaps and titles; narrow or short windows MUST collapse
  it into a single scrolling column.
- **FR-012**: The page MUST follow the application's light and dark themes.
- **FR-013**: The transactions page MUST accept a month in its address (`?month=YYYY-MM`) and open with
  its filters shown and set to that month.

### Key Entities _(include if feature involves data)_

- **Report month**: The month every block shows; the current month by default.
- **Monthly summary**, **Category expense**, **Monthly income and expense**, **Cumulative expense**,
  **Monthly transaction** _(owned by the backend's [[003-financial-reports]])_: the five reports the
  blocks display, as the API returns them.

## Success Criteria _(mandatory)_

### Measurable Outcomes

- **SC-001**: On a desktop window of 1366×768 or larger, all five blocks are visible at once without
  scrolling the page.
- **SC-002**: Picking a month updates the five blocks with that single action.
- **SC-003**: A block that fails never hides or blocks the other four, and can be retried on its own.
- **SC-004**: 100% of the money values on the page are in BRL with pt-BR formatting, and 100% of the
  expenses in the charts and the list use the same red.
- **SC-005**: Users who never open the reports page download nothing more when the application starts.

## Assumptions

- The five reports come from the backend feature [[003-financial-reports]], requested together with this
  page and delivered first; their shapes are in `back/specs/003-financial-reports/contracts/reports-api.yaml`.
- The month selector is the application's existing month field, which shows the month as `MM/yyyy`;
  the dashboard spells the month out where it names one (comparisons, legends, tooltips).
- Income is shown in the theme's green (`tertiary`) and expenses in red; the legend, the tooltips and the
  fixed order of the bars keep them apart for color-blind users.
- The API keeps its own limits — categories folded into "Outras" past eight, every transaction of the
  month — and the page narrows them to five slices and ten rows, so the backend is unchanged.
- The bars cover twelve months, the API's default; choosing the number of months is out of scope.
- The narrow-screen collapse is the only responsive behavior; the application otherwise stays a desktop
  layout.
