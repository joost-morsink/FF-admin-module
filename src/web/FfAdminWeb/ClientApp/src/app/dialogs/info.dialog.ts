import { Component, Inject, ChangeDetectionStrategy } from '@angular/core';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';

@Component({
  selector: 'ff-info-dialog',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './info.dialog.html'
})
export class InfoDialog {
  constructor(@Inject(MAT_DIALOG_DATA) public data: { title?: string, message: string }) {
  }
} 