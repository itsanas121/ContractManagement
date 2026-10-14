import { Component } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [MatButtonModule],
  styleUrl: './home.scss',
  templateUrl: './home.html',
})
export class Home {}
