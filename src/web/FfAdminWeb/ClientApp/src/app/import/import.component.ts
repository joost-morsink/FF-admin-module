import { Component } from '@angular/core';
import { Admin } from '../backend/admin';
import { EventStore } from '../backend/eventstore';
import { IOption, IEventNewOption, IValidationMessage, IEventStatistics, IFullEvent } from '../interfaces/interfaces';
import { FormControl, FormGroup, ValidationErrors, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { MatDialog } from '@angular/material/dialog';
import { ErrorDialog } from '../error/error.dialog';

@Component({
  selector: 'ff-import-csv',
  templateUrl: './import.component.html'
})
export class ImportCsvComponent {
  constructor(private eventStore: EventStore, private dialog: MatDialog) { }
  public file: File;
  public fileName: string;

  public onFileSelected(e) {
    this.file = (<HTMLInputElement>event.target).files[0];
    this.fileName = this.file?.name;
  }
  public async executeUpload() {

    if (this.file) {
      const formData = new FormData();
      formData.append("file", this.file);

      try {
        await this.eventStore.importCsv(formData);
      } catch (ex) {
        await this.dialog.open(ErrorDialog, {
          data: { errors: ex.error }
        }).afterClosed().toPromise();
      }
    }
  }
}