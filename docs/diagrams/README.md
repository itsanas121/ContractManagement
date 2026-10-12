# Design diagrams

These diagrams are the team's reference design for the Contract Management System.

SVG exports are the reference diagrams.
The corresponding Markdown files contain the Mermaid source for future editing.

| Diagram | Mermaid source | SVG export |
|---|---|---|
| Entity-relationship diagram | [ERD](erd.md) | [SVG](erd.svg) |
| Business process flow | [Process flow](business-process-flow.md) | [SVG](business-process-flow.svg) |
| Contract lifecycle | [Lifecycle](contract-lifecycle.md) | [SVG](contract-lifecycle.svg) |
| High-level architecture | [Architecture](high-level-architecture.md) | [SVG](high-level-architecture.svg) |

## Maintenance

1. Open the corresponding Markdown file.
2. Copy only the Mermaid source inside the text block into mermaid.ai.
   Do not copy the Markdown headings or the opening and closing code fences.
3. Make the required changes and verify the diagram.
4. Update the Mermaid source in the corresponding Markdown file.
5. Export a new SVG and replace the corresponding SVG file listed in the table above.
   Keep the existing filename so the links remain valid.
6. Include both the Markdown source update and the SVG export in the same pull request.