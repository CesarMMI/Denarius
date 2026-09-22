# Feature Specification: Transaction Management

**Feature Branch**: `002-transaction-management`

**Created**: 2026-09-22

**Status**: Draft

**Input**: User description: "Retroactive specification of the existing transaction management
capability — creating, viewing, listing, editing, and deleting financial transactions, each
transaction linked to exactly one category — derived from the current implementation
(Denarius.Domain/Denarius.Application/Denarius.Infrastructure/Denarius.WebAPI) and its automated
test suite. This mirrors the retroactive approach already used for 001-category-management: the
spec should document verified current behavior, not propose new work."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Record and maintain individual transactions (Priority: P1)

As someone tracking personal finances, I want to record a transaction with a date, an amount,
and a category — plus an optional note — and edit or remove it later, so I can keep an accurate
record of the money I've received or spent.

**Why this priority**: Nothing else in this feature is possible until a transaction can be
recorded. This is the minimum needed to make transaction tracking usable at all, and it's what
gives [[001-category-management]] categories something to attach to.

**Independent Test**: Can be fully tested by creating a transaction against an existing category,
confirming it appears with the chosen date/value/category/description, editing it, and deleting
it — without needing any other transactions to exist.

**Acceptance Scenarios**:

1. **Given** an existing category, **When** a user records a transaction with a date, a value,
   and that category, **Then** the transaction appears with those details.
2. **Given** a transaction exists, **When** a user changes its description, date, value, or
   category, **Then** the transaction reflects the new details everywhere it is shown.
3. **Given** a transaction exists, **When** a user deletes it, **Then** it no longer appears in
   the transaction list.
4. **Given** a user submits a value of zero, a missing date, a description longer than 255
   characters, or a category that does not exist, **When** they try to create or edit a
   transaction, **Then** the request is rejected with a clear explanation of what is wrong.
5. **Given** a user leaves the description blank, **When** they create or edit a transaction,
   **Then** the transaction is saved successfully with no description, rather than being
   rejected.

---

### User Story 2 - Review recorded transactions (Priority: P2)

As someone reviewing their finances, I want to see every transaction I've recorded, so I can
check my spending and income history in one place.

**Why this priority**: Recording transactions (Story 1) has little value if they can never be
reviewed afterward. This depends on Story 1 existing but adds no new lifecycle actions of its
own.

**Independent Test**: Can be fully tested by recording several transactions and confirming every
one of them appears when the transaction list is viewed, and that the list is empty when none
have been recorded.

**Acceptance Scenarios**:

1. **Given** several transactions have been recorded, **When** the transaction list is viewed,
   **Then** every recorded transaction is shown with its date, value, category, and description.
2. **Given** no transactions have been recorded, **When** the transaction list is viewed,
   **Then** an empty list is shown.

---

### Edge Cases

- A transaction value of exactly zero is rejected outright, on both create and edit; a negative
  value (money spent) or a positive value (money received) is otherwise accepted without
  restriction.
- A transaction with no date (or a default/unset date) is rejected outright, on both create and
  edit.
- A description that is blank or whitespace-only is not rejected — it is treated as "no
  description" rather than an error.
- Leading/trailing spaces in a provided description are trimmed automatically.
- A description longer than 255 characters is rejected outright, on both create and edit.
- Creating or editing a transaction against a category that does not exist is rejected with a
  clear "not found" outcome for the category, on both create and edit.
- Viewing, editing, or deleting a transaction that does not exist (or was already deleted) is
  rejected with a clear "not found" outcome.
- Deleting a transaction is never blocked by other data — nothing else currently depends on a
  transaction the way transactions depend on categories.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST allow a user to create a transaction by providing a date, a value,
  and a category, with an optional description.
- **FR-002**: The system MUST reject a transaction value of zero, explaining what is wrong.
- **FR-003**: The system MUST allow a transaction value to be positive (money received) or
  negative (money spent).
- **FR-004**: The system MUST reject a transaction with no date, explaining what is wrong.
- **FR-005**: The system MUST reject a transaction referencing a category that does not exist,
  explaining what is wrong.
- **FR-006**: The system MUST trim a provided description and treat a blank or whitespace-only
  description as no description, rather than rejecting it.
- **FR-007**: The system MUST reject a description longer than 255 characters, explaining what is
  wrong.
- **FR-008**: The system MUST allow a user to view an individual transaction's details.
- **FR-009**: The system MUST allow a user to view the full list of recorded transactions.
- **FR-010**: The system MUST allow a user to edit a transaction's description, date, value,
  and/or category at any time, subject to the same validation as creation.
- **FR-011**: The system MUST allow a user to permanently delete a transaction, with no
  restriction based on other data.
- **FR-012**: The system MUST reject any attempt to view, edit, or delete a transaction that does
  not exist, with a clear "not found" outcome.

### Key Entities *(include if feature involves data)*

- **Transaction**: A single recorded movement of money, with a date, a value (positive or
  negative), a required category reference, an optional description, plus creation and
  last-updated timestamps.
- **Category** *(existing entity, owned by [[001-category-management]])*: Each transaction
  belongs to exactly one category, which must already exist at the time the transaction is
  created or re-categorized.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A user can record a usable transaction (date, value, category) in a single step,
  with it immediately visible in their transaction list.
- **SC-002**: A user can tell whether any recorded transaction was money received or money spent
  without needing any additional lookup — 100% of transactions carry that information directly.
- **SC-003**: A user can permanently remove any transaction they no longer want in a single
  action, with no unexpected refusal.
- **SC-004**: A user can review their entire recorded transaction history in a single request,
  with no manual pagination or lookup required.
- **SC-005**: Attempting to record or re-categorize a transaction against a category that does
  not exist never silently succeeds — 100% of such attempts are rejected with an explanation.

## Assumptions

These document the capability's current, verified behavior (this is a retroactive spec), rather
than open defaults chosen for a new feature:

- The transaction list is not searchable, filterable, sortable, or paginated — every recorded
  transaction is returned together, in whatever order the system naturally provides. This is
  narrower than [[001-category-management]]'s list, which does support search/filter/sort; this
  spec does not assume that gap is closed.
- A transaction can be re-categorized to any existing category at any time via edit; there is no
  restriction analogous to Category's delete-guard, because nothing currently depends on a
  Transaction the way Transactions depend on Categories.
- There is no currency field — all values are assumed to be in a single, implicit currency,
  consistent with the rest of the application.
- Who may manage transactions (single user vs. multiple users/accounts, permissions) is out of
  scope here — this spec assumes the same single-tenant usage model as the rest of the
  application.
