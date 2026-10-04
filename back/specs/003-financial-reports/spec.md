# Feature Specification: Financial Reports

**Feature Branch**: `003-financial-reports`

**Created**: 2026-10-03

**Status**: Draft

**Input**: User description: "Financial reports for a reports dashboard, one report per need, each
answering for a calendar month (`YYYY-MM`, the current month when omitted, an invalid month
rejected): (1) a monthly summary — total income, total expense, balance, savings rate, a month-end
projection of expense and balance, and the previous month's totals with the percentage change of
each; (2) expenses by category with each category's share, largest first, the tail grouped into
'Outras' when there are more than 8 categories; (3) income vs. expense for the last N months
(default 12, between 1 and 24) as a continuous series with zeros for months without movement;
(4) the month's cumulative daily expense next to the previous month's, the current month only up to
today; (5) every transaction of the month, newest first, with its category name, type and amount.
Money in decimals, dates in the America/Sao_Paulo time zone, totals computed by the database, and
indexes on date and category where missing."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - See how the month is going at a glance (Priority: P1)

As someone tracking personal finances, I want to see a month's income, expense, balance and savings
rate, how the month is likely to close, and how it compares with the month before, so I can tell in a
few seconds whether I am on track.

**Why this priority**: It is the headline of the dashboard and the one report that answers "how am I
doing?" on its own. Every other report breaks down what this one summarizes.

**Independent Test**: Record income and expenses across two consecutive months, ask for the second
month's summary, and confirm every total, the savings rate, the projection and the comparison with
the first month — no other report needed.

**Acceptance Scenarios**:

1. **Given** a month with 5,000.00 of income and 3,000.00 of expenses, **When** its summary is
   requested, **Then** it shows total income 5,000.00, total expense 3,000.00, balance 2,000.00 and a
   savings rate of 40%.
2. **Given** a month with expenses but no income, **When** its summary is requested, **Then** the
   savings rate is reported as not applicable rather than as 0% or an error.
3. **Given** the previous month had 4,000.00 of income and the selected month has 5,000.00, **When**
   the summary is requested, **Then** it shows the previous month's income and a 25% increase.
4. **Given** the previous month had no expenses, **When** the summary is requested, **Then** the
   change in expenses is reported as not applicable rather than as an error or an infinite value.
5. **Given** today is the 10th of a 30-day month and 900.00 was spent from the 1st to today,
   **When** the current month's summary is requested, **Then** the projected expense is 2,700.00
   and the projected balance is the month's income minus 2,700.00.
6. **Given** a month that has already ended, **When** its summary is requested, **Then** the
   projected expense and balance equal the actual expense and balance.
7. **Given** a month that has not started yet, **When** its summary is requested, **Then** the
   projected expense and balance are zero.
8. **Given** no month is chosen, **When** the summary is requested, **Then** it covers the current
   month as it is in São Paulo.

---

### User Story 2 - See where the money went (Priority: P2)

As someone reviewing a month, I want to see how much I spent in each category and what share of the
month's spending each one represents, largest first, so I know where to cut.

**Why this priority**: The first breakdown anyone asks for after the summary. It only needs the
month's expenses and the categories that already exist.

**Independent Test**: Record expenses in several categories in one month and confirm each category's
total, its share of the month's spending, the order, and the grouping into "Outras" once there are
more than eight categories.

**Acceptance Scenarios**:

1. **Given** a month with 600.00 spent on Mercado, 300.00 on Lazer and 100.00 on Transporte,
   **When** the expenses by category are requested, **Then** they come largest first with shares of
   60%, 30% and 10% and a total of 1,000.00, each with its category's name and color.
2. **Given** a month with expenses in ten categories, **When** the expenses by category are
   requested, **Then** the seven largest are listed on their own and the other three are added up in
   a final "Outras" entry.
3. **Given** a month with expenses in exactly eight categories, **When** the expenses by category are
   requested, **Then** all eight are listed and there is no "Outras" entry.
4. **Given** a month with income but no expenses, **When** the expenses by category are requested,
   **Then** the list is empty and the total is zero.

---

### User Story 3 - Follow income against expense over the months (Priority: P3)

As someone planning ahead, I want to see income and expense side by side for the last months, so I
can spot trends and the months that went wrong.

**Why this priority**: A trend view only pays off after a few months of history, so it comes after
the month-centered reports.

**Independent Test**: Record movements in some, but not all, of the last twelve months and confirm
the series has exactly twelve months in chronological order, with zeros where nothing happened.

**Acceptance Scenarios**:

1. **Given** movements only in January and March, **When** the three months ending in March are
   requested, **Then** January, February and March come in that order, with February at zero.
2. **Given** no number of months is chosen, **When** the series is requested, **Then** it covers the
   twelve months ending in the selected month.
3. **Given** the series ends in February, **When** four months are requested, **Then** it starts in
   November of the previous year.
4. **Given** a number of months below 1 or above 24, **When** the series is requested, **Then** the
   request is rejected with an explanation.

---

### User Story 4 - Compare this month's spending pace with last month's (Priority: P4)

As someone watching my spending during the month, I want to see how much I had spent by each day of
the month next to how much I had spent by the same day last month, so I know early whether I am
spending faster than usual.

**Why this priority**: A refinement of the summary's projection — useful during the month, but the
summary already says where the month is heading.

**Independent Test**: Record expenses on a few days of two consecutive months and confirm both
day-by-day running totals, where they stop, and the number of days in each month.

**Acceptance Scenarios**:

1. **Given** a past month with expenses on the 3rd and the 10th, **When** its cumulative spending is
   requested, **Then** there is one value for every day of the month, unchanged between expenses,
   and the last value is the month's total expense.
2. **Given** today is the 15th, **When** the current month's cumulative spending is requested,
   **Then** its series stops at the 15th while the previous month's series covers all its days.
3. **Given** a selected January, **When** its cumulative spending is requested, **Then** it is
   compared with December of the previous year, and each month reports how many days it has.

---

### User Story 5 - List everything that happened in the month (Priority: P5)

As someone reviewing a month, I want to see every transaction of that month, newest first, with its
category, whether it was money in or out, and how much, so I can check the details behind the
charts.

**Why this priority**: The existing transaction list can already be narrowed to a month; this report
only bundles the category name and type the dashboard needs.

**Independent Test**: Record transactions across two months and confirm only the selected month's
transactions come back — all of them, newest first, with category name, type and amount.

**Acceptance Scenarios**:

1. **Given** 30 transactions in a month, **When** the month's transactions are requested, **Then**
   all 30 come back, without pagination or a cap.
2. **Given** an expense of 50.00 in Mercado, **When** the month's transactions are requested,
   **Then** it shows the category name "Mercado", the type "money out" and the amount 50.00.
3. **Given** two transactions on the same day, **When** the month's transactions are requested,
   **Then** the one recorded last comes first.

---

### Edge Cases

- A month is written as a four-digit year and a two-digit month (`2026-09`); anything else — `2026-9`,
  `2026-13`, `09-2026`, `2026-09-01`, text — is rejected with an explanation, for every report.
- With no month chosen, every report covers the current month as it is in São Paulo: at 22:00 of
  September 30 in São Paulo it is still September, even though it is already October 1 in UTC.
- Transaction dates are calendar days, not moments in time: a transaction dated September 1 always
  belongs to September, whatever the time zone.
- A month without any transaction returns zero totals, empty lists and zero series — never an error.
- Savings rate without income, and a percentage change from a previous value of zero, are reported as
  not applicable (no value), never as zero, infinity or an error.
- A percentage change from a negative previous balance is measured against the size of that balance,
  so a balance going from −100.00 to 50.00 is a 150% increase.
- In the current month, the projection only counts expenses dated up to and including today; an
  expense already recorded for a later day of the month is not "spent by today". Today counts as an
  elapsed day, so on the 1st the projection is the day's expense times the days in the month.
- The previous month of January is December of the previous year, for the summary and the cumulative
  comparison; a series of months crosses year boundaries the same way.
- Categories with the same spending are listed in alphabetical order. "Outras" always comes last, as
  the tail of the list, even when its total is larger than some of the categories listed on their own.
- Shares and percentage changes are rounded to two decimals, so shares may not add up to exactly 100%.
- In the cumulative comparison, a month that has not started yet has no days in its series; a past
  month has all of them.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Every report MUST accept an optional month written `YYYY-MM` and MUST cover the current
  month in the America/Sao_Paulo time zone when none is given.
- **FR-002**: Every report MUST reject a month that is not a valid `YYYY-MM` month, explaining what is
  wrong.
- **FR-003**: The system MUST treat transaction dates as calendar days: a transaction belongs to the
  month and day of its date, never shifted by a time zone.
- **FR-004**: The monthly summary MUST report the month, its total income (money in), total expense
  (money out, as a positive amount) and balance (income minus expense).
- **FR-005**: The monthly summary MUST report the savings rate as the balance divided by the total
  income, as a percentage, and MUST report it as not applicable when the month has no income.
- **FR-006**: The monthly summary MUST project the month-end expense and balance: for the current
  month, the expense from the 1st through today divided by the days elapsed (today included) times the
  days in the month, and the month's income minus that expense; for a past month, the actual values;
  for a future month, zero.
- **FR-007**: The monthly summary MUST report the previous month's total income, total expense and
  balance, and the percentage change from each of them to the selected month, reported as not
  applicable when the previous value is zero.
- **FR-008**: The expenses-by-category report MUST list, for the month, each category with expenses —
  its identifier, name, color, amount spent and share of the month's total expense — largest amount
  first, with the month's total expense.
- **FR-009**: When more than eight categories have expenses in the month, the expenses-by-category
  report MUST list the seven largest on their own and add the others up into a final "Outras" entry
  with no identifier or color.
- **FR-010**: The income-vs-expense report MUST return, in chronological order, one entry per month for
  the chosen number of months ending in the selected month — its income, expense and balance —
  including months without movement, at zero.
- **FR-011**: The income-vs-expense report MUST cover 12 months by default and MUST reject a number of
  months below 1 or above 24, explaining what is wrong.
- **FR-012**: The cumulative-expense report MUST return, for the selected month and for the previous
  month, the running total of expense for each day from the 1st to the month's last day, and the
  number of days in each of the two months.
- **FR-013**: In the cumulative-expense report, a series MUST stop at today for the current month and
  MUST have no days for a month that has not started.
- **FR-014**: The monthly-transactions report MUST return every transaction of the month, without
  pagination or limit, newest date first (the most recently recorded first on the same date), each
  with its identifier, date, description, category name, type (money in or money out) and amount (as
  a positive value).
- **FR-015**: Money amounts MUST be exact decimal values; projected amounts and percentages MUST be
  rounded to two decimals.
- **FR-016**: Each report MUST work only from the months it covers, adding the amounts up where they
  are stored, rather than reading the whole transaction history on every request.
- **FR-017**: Existing transaction and category behavior and their contracts MUST NOT change.

### Key Entities *(include if feature involves data)*

- **Report month**: The calendar month a report covers, written `YYYY-MM`; defaults to the current
  month in São Paulo.
- **Monthly summary**: A month's total income, total expense, balance, savings rate, projected expense
  and balance, and the previous month's totals with the percentage change of each.
- **Category expense**: One category's spending in a month — name, color, amount and share — or the
  "Outras" entry grouping the smaller ones.
- **Monthly income and expense**: One month's income, expense and balance within a series of months.
- **Cumulative expense**: The running total of a month's expense on one day of that month.
- **Monthly transaction**: A transaction as the reports show it — date, description, category name,
  type and positive amount.
- **Transaction** and **Category** *(existing, owned by [[002-transaction-management]] and
  [[001-category-management]])*: The reports only read them; a transaction's type comes from the sign
  of its value.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A user can see a month's income, expense, balance, savings rate, projection and
  comparison with the previous month in a single request.
- **SC-002**: Each report answers in under one second for a history of 10,000 transactions.
- **SC-003**: 100% of months without movement return zeros or empty lists rather than errors, and
  100% of invalid months or month counts are rejected with an explanation.
- **SC-004**: A year of income and expense comes back as exactly 12 consecutive months, with no gaps,
  whatever months had movement.
- **SC-005**: The sum of the amounts in the expenses-by-category report always equals its total
  expense, and the month's total expense in the summary.

## Assumptions

- Money in and money out follow the existing model: the sign of the transaction value, with no stored
  type. Expense amounts are reported as positive values, so the balance is income minus expense.
- Percentages are on a 0–100 scale (25.5 means 25.5%), rounded to two decimals; this covers the
  savings rate (balance ÷ income × 100), category shares and percentage changes.
- The savings rate is not applicable — reported without a value, not as 0% — when the month has no
  income, because there is nothing to save from; it can be negative when expense exceeds income.
- A percentage change is measured against the size of the previous value, so its sign always says
  whether the value went up or down.
- Only expense is projected; income is taken as already known for the month, since it usually arrives
  once rather than daily.
- The expenses-by-category report shows at most eight entries: when there are more than eight
  categories, seven plus "Outras".
- All values are in a single, implicit currency (BRL), as in the rest of the application.
- Single-tenant usage, as in the rest of the application; who may see the reports is out of scope.
