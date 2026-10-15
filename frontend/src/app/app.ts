import { Component, inject } from '@angular/core';
import { Router, RouterOutlet, NavigationEnd } from '@angular/router';

import { toSignal } from '@angular/core/rxjs-interop';
import { filter, map } from 'rxjs';

import { Navbar } from './shared/navbar/navbar';
import { AuthService } from './core/auth/auth.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Navbar],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {
  private readonly router = inject(Router);
  private readonly auth = inject(AuthService);

  readonly showNavbar = toSignal(
    this.router.events.pipe(
      filter(
        (event): event is NavigationEnd =>
          event instanceof NavigationEnd
      ),
      map(event =>
        this.auth.isAuthenticated &&
        event.urlAfterRedirects.split('?')[0] !== '/login'
      )
    ),
    {
      initialValue:
        this.auth.isAuthenticated &&
        this.router.url.split('?')[0] !== '/login'
    }
  );
}
