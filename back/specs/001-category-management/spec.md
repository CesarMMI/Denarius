# Feature Specification: Category Management

**Feature Branch**: `001-category-management`

**Created**: 2026-09-21

**Status**: Draft

**Input**: User description: "Retroactive specification of the existing category management capability — creating, viewing, listing, editing, and deleting categories, plus per-category usage statistics (transaction count and balance) — derived from the current implementation and its automated test suite."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Build and maintain a category list (Priority: P1)

As someone tracking personal finances, I want to create categories with a name and a color, and
rename, recolor, or remove them later, so I can organize my transactions in a way that makes
sense to me and keep that organization accurate over time.

**Why this priority**: Nothing else in this feature — or in transaction tracking generally — is
possible until categories can be created. This is the minimum needed to make categories usable
at all.

**Independent Test**: Can be fully tested by creating a category, confirming it appears with the
chosen name and color, editing it, and deleting it — without needing any transaction data to
exist.

**Acceptance Scenarios**:

1. **Given** no categories exist yet, **When** a user creates a category with a name and a
   color, **Then** the category appears with that name and color.
2. **Given** a category exists, **When** a user renames it or changes its color, **Then** the
   category reflects the new name and color everywhere it is shown.
3. **Given** a category has no transactions, **When** a user deletes it, **Then** it no longer
   appears in the category list.
4. **Given** a category already has transactions recorded against it, **When** a user tries to
   delete it, **Then** the deletion is refused and the user is told why.
5. **Given** a user submits a blank name, an overly long name, or an invalid color, **When**
   they try to create or edit a category, **Then** the request is rejected with a clear
   explanation of what is wrong.

---

### User Story 2 - See how much each category is used (Priority: P2)

As someone reviewing their spending, I want each category to show how many transactions it has
and what they add up to, so I can immediately see where my money is going without manually
tallying transactions myself.

**Why this priority**: This turns a plain list of labels into a useful financial overview. It
depends on Story 1 existing but does not require any new category-management actions of its own.

**Independent Test**: Can be fully tested by recording a known set of transactions against a
category and confirming the category list reports the correct transaction count and balance for
it, and zero/zero for a category with none.

**Acceptance Scenarios**:

1. **Given** a category has three transactions worth a combined total, **When** the category
   list is viewed, **Then** that category shows a count of three and the correct combined
   balance.
2. **Given** a category has no transactions, **When** the category list is viewed, **Then** it
   shows a count of zero and a balance of zero.
3. **Given** a user asks to see usage for a specific month, **When** the category list is
   viewed, **Then** only transactions dated within that month count toward each category's
   totals.

---

### User Story 3 - Narrow down a long category list (Priority: P3)

As someone with many categories, I want to search by name, show only categories that are (or
are not) in use, and sort by name, activity, or balance, so I can quickly find or compare the
categories I care about.

**Why this priority**: Purely a refinement of browsing an already-working list (Stories 1-2).
Valuable once the category list grows, but not required for the feature to be useful on day
one.

**Independent Test**: Can be fully tested by creating several categories with varying names and
usage, then confirming search, the in-use/not-in-use filter, and each sort option each narrow or
reorder the list correctly on their own.

**Acceptance Scenarios**:

1. **Given** several categories exist, **When** a user searches by part of a name, **Then**
   only categories whose name contains that text are shown.
2. **Given** some categories have transactions and others do not, **When** a user filters to
   "in use" or "not in use", **Then** only the matching categories are shown.
3. **Given** categories with different names, transaction counts, and balances, **When** a user
   sorts by name, activity, or balance (ascending or descending), **Then** the list is ordered
   accordingly.

---

### Edge Cases

- A category name that is blank, whitespace-only, or longer than 100 characters is rejected
  outright, on both create and edit.
- Leading/trailing spaces in a name are trimmed automatically rather than treated as making it a
  different category.
- A color that is not a valid color code is rejected outright, on both create and edit.
- Viewing, editing, or deleting a category that does not exist (or was already deleted) is
  rejected with a clear "not found" outcome.
- Deleting a category that still has transactions is refused rather than silently orphaning
  those transactions.
- A search or filter combination that matches nothing returns an empty list, not an error.
- A transaction dated on the very first or very last moment of a month still counts toward that
  month's usage totals for its category.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST allow a user to create a category by providing a name and a
  color.
- **FR-002**: The system MUST reject a category name that is empty, whitespace-only, or longer
  than 100 characters, explaining what is wrong.
- **FR-003**: The system MUST trim leading and trailing whitespace from a category name rather
  than treating it as significant.
- **FR-004**: The system MUST reject a color value that is not a valid color code, explaining
  what is wrong.
- **FR-005**: The system MUST allow a user to view an individual category's details.
- **FR-006**: The system MUST allow a user to view the full list of categories.
- **FR-007**: The system MUST allow a user to rename a category and/or change its color at any
  time, subject to the same validation as creation.
- **FR-008**: The system MUST allow a user to delete a category that has no transactions
  associated with it.
- **FR-009**: The system MUST refuse to delete a category that has one or more transactions
  associated with it, and MUST explain why.
- **FR-010**: The system MUST reject any attempt to view, edit, or delete a category that does
  not exist, with a clear "not found" outcome.
- **FR-011**: For every category shown in the list, the system MUST report the number of
  transactions associated with it and the resulting balance (the sum of those transactions'
  values).
- **FR-012**: The system MUST allow a user to restrict a category's reported transaction count
  and balance to a single calendar month.
- **FR-013**: The system MUST allow a user to filter the category list to only categories with
  at least one transaction, or only those with none.
- **FR-014**: The system MUST allow a user to search the category list by partial name match.
- **FR-015**: The system MUST allow a user to sort the category list by name, transaction
  count, or balance, in ascending or descending order, defaulting to name ascending.

### Key Entities *(include if feature involves data)*

- **Category**: A user-defined label used to group transactions, with a name and a color for
  visual identification, plus creation and last-updated timestamps. A category can exist with
  zero transactions.
- **Transaction** *(existing entity, owned by its own feature)*: Each transaction belongs to
  exactly one category. A category's transaction count and balance are derived entirely from the
  transactions that reference it.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A user can create a usable category (name and color) in a single step, with it
  immediately visible in their category list.
- **SC-002**: A user can state any shown category's transaction count and balance without
  performing a single manual calculation — the figures are visible for 100% of categories in the
  list.
- **SC-003**: Attempting to delete a category that is still in use never results in lost or
  orphaned transaction history — 100% of such attempts are blocked.
- **SC-004**: In a list of any size, a user can locate a specific category by typing part of its
  name, or by filtering to only categories that are (or are not) in use.
- **SC-005**: A user can name their single most- or least-active category, for a given month or
  for all time, in one sorting action — with no need to cross-reference individual
  transactions.

## Assumptions

These document the capability's current, verified behavior (this is a retroactive spec), rather
than open defaults chosen for a new feature:

- Category names are not required to be unique; two categories may currently share the same
  name.
- A color is any valid hex color code; there is no fixed palette or named-color list beyond
  validity.
- There is no "reassign transactions, then delete" flow — a category with any transactions must
  have those transactions removed or recategorized elsewhere before it can be deleted.
- A "month" for usage scoping always means a full calendar month (its first moment through its
  last), not a rolling 30-day window.
- The category list is not paginated; all categories are returned together. This spec does not
  assume that changes as category counts grow.
- Who may manage categories (single user vs. multiple users/accounts, permissions) is out of
  scope here — this spec assumes the same single-tenant usage model as the rest of the
  application.
