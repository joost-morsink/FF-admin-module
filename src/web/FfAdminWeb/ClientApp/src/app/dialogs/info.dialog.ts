import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';

@Component({
  selector: 'ff-info-dialog',
  templateUrl: './info.dialog.html'
})
export class InfoDialog {
  constructor(@Inject(MAT_DIALOG_DATA) public data: { title?: string, message: string }) {
  }
} 