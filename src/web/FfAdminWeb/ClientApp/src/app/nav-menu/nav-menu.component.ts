import { Component, ChangeDetectionStrategy } from '@angular/core';

@Component({
  selector: 'app-nav-menu',
  standalone: false,
  templateUrl: './nav-menu.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrls: ['./nav-menu.component.css']
})
export class NavMenuComponent {
  isExpanded = false;

  collapse() {
    this.isExpanded = false;
  }

  toggle() {
    this.isExpanded = !this.isExpanded;
  }
}
