import { Component } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { Admin } from '../backend/admin';
import { ErrorDialog } from '../error/error.dialog';
import { ICharity } from '../interfaces/interfaces';

@Component({
  selector: 'ff-database',
  templateUrl: './database.component.html'
})
export class DatabaseComponent {
  constructor(private admin: Admin, private dialog: MatDialog) {

  }
  public async recreate() {
    try {
      this.admin.recreateDatabase();
      alert("Success!");
    } catch (ex) {
      this.dialog.open(ErrorDialog, {
        data: {
          errors: ex.error
        }
      });
    }
  }
  public async update() {
    try {
      this.admin.updateDatabase();
      alert("Success!");
    } catch (ex) {
      this.dialog.open(ErrorDialog, {
        data: {
          errors: ex.error
        }
      });
    }
  }
}
