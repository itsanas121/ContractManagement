import { Component } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { Navbar } from '../../shared/navbar/navbar';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [MatButtonModule,
            Navbar],
  styleUrl: './home.scss',
  templateUrl: './home.html',
})
export class Home {}
