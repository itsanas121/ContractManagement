import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { Router, RouterLink, RouterLinkActive} from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { CurrentUserService } from '../../core/auth/current-user';

@Component({
  imports: [
    RouterLink,
    RouterLinkActive,
    MatButtonModule
  ],
  selector: 'app-navbar',
  styleUrl: './navbar.scss',
  templateUrl: './navbar.html',
})
export class Navbar {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly currentUser = inject(CurrentUserService);

  readonly role = this.currentUser.user?.role ?? '';

  logout(): void {
    this.auth.logout();
    void this.router.navigateByUrl('/login')
  }
}
