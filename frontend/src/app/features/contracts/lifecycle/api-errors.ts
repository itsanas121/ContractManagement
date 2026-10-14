import { HttpErrorResponse } from '@angular/common/http';

// Turns an API error into readable messages.
// Handles both error formats our API returns:
// - our ProblemDetails:   { detail: "...", errors: ["...", "..."] }
// - ASP.NET validation:   { errors: { Reason: ["..."] } }
export function apiErrorMessages(error: unknown): string[] {
  if (!(error instanceof HttpErrorResponse)) {
    return ['Something went wrong. Please try again.'];
  }

  const body = error.error;

  if (body?.errors) {
    if (Array.isArray(body.errors)) {
      return body.errors;
    }
    return Object.values(body.errors).flat() as string[];
  }

  if (body?.detail) {
    return [body.detail];
  }

  if (error.status === 403) {
    return ['You are not allowed to do this.'];
  }

  return ['Something went wrong. Please try again.'];
}