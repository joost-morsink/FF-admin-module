import { Component } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { Admin } from '../backend/admin';
import { ErrorDialog } from '../dialogs/error.dialog';
import { InfoDialog } from '../dialogs/info.dialog';
import { ICharity, IValidationMessage } from '../interfaces/interfaces';

@Component({
  selector: 'ff-database',
  templateUrl: './database.component.html'
})
export class DatabaseComponent {
  constructor(private admin: Admin, private dialog: MatDialog) {

  }
  private showSuccess() {
    this.dialog.open(InfoDialog, {
      data: {
        message: "Success!"
      }
    });
  }
  private showErrors(errors: IValidationMessage[]) {
    this.dialog.open(ErrorDialog, {
      data: {
        errors: errors
      }
    });
  }

  public async recreate() {
    try {
      await this.admin.recreateDatabase();
      this.showSuccess();
    } catch (ex) {
      this.showErrors(ex.error);
    }
  }
  public async update() {
    try {
      await this.admin.updateDatabase();
      this.showSuccess();
    } catch (ex) {
      this.showErrors(ex.error);
    }
  }
}
