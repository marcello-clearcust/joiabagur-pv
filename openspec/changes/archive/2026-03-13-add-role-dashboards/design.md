## Context

The application has two user roles (Administrator and Operator) with fundamentally different needs from a landing page. The current dashboard is a static placeholder. This change introduces two role-specific dashboards that surface live data from existing API endpoints and one new aggregation endpoint.

The operator dashboard targets mobile/tablet use at POS locations, requiring a dedicated layout that maximizes screen real estate for actions and hides navigation chrome.

## Goals / Non-Goals

- **Goals**:
  - Deliver role-appropriate dashboards with live KPIs, charts, and actionable tables
  - Minimize backend overhead: single pre-aggregated endpoint + 24h cache for heavy aggregations
  - Operator layout optimized for PWA/mobile use with sticky action bar
  - Reuse existing API endpoints (`/api/sales`, `/api/inventory`, `/api/inventory/centralized`, `/api/returns`) wherever possible

- **Non-Goals**:
  - Real-time websocket updates (polling or manual refresh is sufficient for 2-3 concurrent users)
  - Custom reporting or date-range selectors on dashboard (covered by EP9 Reports)
  - Dashboard configuration or widget customization

## Decisions

### 1. Single aggregation endpoint vs. multiple
- **Decision**: One `GET /api/dashboard/stats?posId={optional}` endpoint returning all KPIs in a single response
- **Rationale**: Reduces frontend waterfall requests. The query aggregates Sales, Returns, and Inventory counts in one database round-trip. For 2-3 concurrent users and ~500 products, this is well within free-tier constraints.
- **Alternative rejected**: Separate `/api/dashboard/sales-kpi`, `/api/dashboard/returns-kpi`, etc. — unnecessary complexity for the scale.

### 2. Chart data sourcing
- **Decision**: Trend charts (30-day, 7-day) reuse `GET /api/sales` with date filters; frontend groups by day. Donut charts use dedicated cached sub-endpoints on the dashboard stats route.
- **Rationale**: Sales endpoint already supports date filtering and returns per-sale records. Grouping ~500 sales over 30 days client-side is trivial. Donut aggregations (payment method distribution, return category distribution) require joins that are better cached server-side.

### 3. Caching strategy
- **Decision**: `IMemoryCache` with `AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)` for donut chart aggregations only. KPI endpoint is not cached (real-time accuracy matters for today's sales).
- **Rationale**: Donut charts show monthly distributions that change slowly. 24h cache avoids repeated aggregation queries. KPIs must reflect current-day data.
- **Alternative rejected**: Redis or distributed cache — overkill for single-instance free-tier deployment.

### 4. Operator layout
- **Decision**: New `OperatorLayout` component that wraps the main content area, hides the Layout8 sidebar (CSS override or conditional rendering in Layout8), and renders a fixed bottom bar with 3 action buttons.
- **Rationale**: Operators at POS need maximum screen space and instant access to core actions (new sale, image sale, new return). A bottom bar mimics native POS app UX.
- **Alternative rejected**: Separate React Router layout — would require duplicating protected route logic. Prefer a layout wrapper inside the existing Layout8 structure with conditional sidebar visibility.

### 5. Chart library
- **Decision**: Recharts (lightweight, React-native, tree-shakeable)
- **Rationale**: Small bundle impact (~40KB gzipped for used components), React-idiomatic API, supports line, bar, and pie/donut charts needed by both dashboards. Chart.js is heavier and requires a wrapper.
- **Alternative**: Chart.js via react-chartjs-2 — viable but larger bundle and less React-native.

## Risks / Trade-offs

- **Bundle size**: Adding Recharts adds ~40KB gzipped. Mitigated by lazy-loading the dashboard page (already in place).
- **Stale donut data**: 24h cache means donut charts could be up to 24h behind. Acceptable for monthly distribution views. Admin can see live KPIs for immediate metrics.
- **Operator layout sidebar hiding**: Must not break other pages. Implementation uses conditional CSS class on Layout8 based on a prop or context flag, not a global override.

## Open Questions

- None at this time. The design is straightforward given existing infrastructure.
