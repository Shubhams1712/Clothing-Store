# PROJECT_RULES.md

> These rules apply to the entire project and every future phase. This
> document is the permanent engineering guide for the codebase.

------------------------------------------------------------------------

# 1. General Principles

-   Build for production quality.
-   Prefer maintainability over clever code.
-   Keep the architecture modular.
-   Every feature should be easy to extend.
-   Never duplicate business logic.
-   Every file should have a single responsibility.

------------------------------------------------------------------------

# 2. Technology Stack

Frontend

-   Next.js 15 (App Router)
-   TypeScript
-   Tailwind CSS v4
-   shadcn/ui
-   Framer Motion
-   TanStack React Query
-   Axios

Backend

-   ASP.NET Core 8 Web API
-   Entity Framework Core
-   PostgreSQL

------------------------------------------------------------------------

# 3. Folder Organization

Create files only where they naturally belong.

Do not create random utility folders or duplicate components.

Group code by feature whenever practical.

------------------------------------------------------------------------

# 4. Naming Conventions

Components: - PascalCase

Interfaces: - PascalCase

Types: - PascalCase

Functions: - camelCase

Variables: - camelCase

Constants: - UPPER_SNAKE_CASE

Database Tables: - PascalCase or singular entity naming consistently

API Routes: - kebab-case

------------------------------------------------------------------------

# 5. Component Rules

Components should:

-   Have one responsibility.
-   Be reusable.
-   Accept typed props.
-   Avoid unnecessary state.
-   Avoid duplicated UI.

Extract reusable logic into hooks when appropriate.

------------------------------------------------------------------------

# 6. Styling Rules

Use:

-   Tailwind utilities
-   Design tokens
-   Consistent spacing
-   Consistent border radius
-   Consistent typography

Do not hardcode random spacing or colors repeatedly.

------------------------------------------------------------------------

# 7. API Rules

Every API should:

-   Validate input.
-   Return consistent response models.
-   Return appropriate HTTP status codes.
-   Handle exceptions gracefully.

------------------------------------------------------------------------

# 8. Database Rules

-   Use Entity Framework migrations.
-   Never bypass the ORM for normal CRUD.
-   Maintain referential integrity.
-   Keep relationships explicit.
-   Add indexes where beneficial.

------------------------------------------------------------------------

# 9. Error Handling

Frontend

-   Show meaningful messages.
-   Never expose stack traces.

Backend

-   Centralized exception handling.
-   Structured logging.
-   Consistent error responses.

------------------------------------------------------------------------

# 10. Security

-   Validate every request.
-   Never trust client input.
-   Hash passwords securely.
-   Verify payment server-side.
-   Store secrets only in environment variables.
-   Protect privileged routes with authorization.

------------------------------------------------------------------------

# 11. Performance

-   Optimize images.
-   Lazy load non-critical content.
-   Paginate large datasets.
-   Minimize unnecessary re-renders.
-   Write efficient database queries.

------------------------------------------------------------------------

# 12. Accessibility

Support:

-   Keyboard navigation
-   Semantic HTML
-   Screen reader labels
-   Visible focus states
-   Adequate color contrast

------------------------------------------------------------------------

# 13. Testing Before Completing a Phase

Before marking a phase complete:

-   Project builds successfully.
-   No TypeScript errors.
-   No backend compilation errors.
-   No runtime errors.
-   Acceptance criteria satisfied.
-   Manual checklist completed.

------------------------------------------------------------------------

# 14. Git Workflow

Complete one phase at a time.

Recommended commit pattern:

-   Phase 01 - Foundation
-   Phase 02 - Authentication
-   Phase 03 - Admin Panel
-   ...

Keep commits focused and descriptive.

------------------------------------------------------------------------

# 15. Documentation

When a major architectural decision is made:

-   Update the relevant phase document.
-   Do not change previous completed phases unless necessary.
-   Keep documentation aligned with implementation.

------------------------------------------------------------------------

# 16. Working Method

For every phase:

1.  Read PROJECT_RULES.md.
2.  Read 00-project-vision.md.
3.  Read the current phase document.
4.  Implement only that phase.
5.  Verify all acceptance criteria.
6.  Fix build issues before moving forward.
7.  Summarize completed work.

Never begin the next phase until the current one is complete.
