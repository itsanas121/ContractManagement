import { Component } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [MatButtonModule],
  template: `
    <main class="home">
      <h1>Contract Management System</h1>

      <p>
        The shared Angular application is ready.
        Feature screens will be added during Sprint 2.
      </p>

      <a
        mat-stroked-button
        href="https://material.angular.dev/components/categories"
        target="_blank"
        rel="noopener noreferrer"
      >
        Angular Material components
        <span class="visually-hidden"> — opens in a new tab</span>
      </a>
    </main>
  `,
  styles: `
    .home {
      max-width: 60rem;
      margin-inline: auto;
      padding: 2rem 1rem;
    }

    .visually-hidden {
      position: absolute;
      width: 1px;
      height: 1px;
      padding: 0;
      margin: -1px;
      overflow: hidden;
      clip: rect(0, 0, 0, 0);
      white-space: nowrap;
      border: 0;
    }
  `,
})
export class Home {}